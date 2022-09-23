using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using System;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;
    public PhotonView pv;
    public Transform playerHand;
    public Vector3 playerHandPosOffsetForNonOwners;
    private Item[] itemPrefabs;
    private Dictionary<string, Item> allAvailableItems = new Dictionary<string, Item>();
    public List<Item> itemsInInventory = new List<Item>();
    public string[] teamOneDefaultItemKeys;
    public string[] teamTwoDefaultItemKeys;
    public KeyCode dropItemKeyCode;

    public ItemType slotSelected = ItemType.None;

    public Item PrimaryItemSlot;
    public Item SecondaryItemSlot;
    public Item KnifeSlot;
    public Item UtilitySlot;
    public Item SpecialSlot;

    List<int> itemsWithSetParent = new List<int>();
    public Animator inventoryHandAnimator;
    public string pullOutItemAnimatorTrigger;

    public static Action<Item> OnItemSelected;
    

    

    private void Awake()
    {
        if (pv.IsMine)
        {
            instance = this;

            itemPrefabs = Resources.LoadAll<Item>("Items");
            foreach (var itemPrefab in itemPrefabs)
            {
                var spawnedItem = PhotonNetwork.Instantiate(itemPrefab.name, playerHand.position, playerHand.rotation);

                var item = spawnedItem.GetComponent<Item>();
                allAvailableItems.Add(item.itemKey, item);

                spawnedItem.gameObject.SetActive(false);
                pv.RPC("ShowHideItem", RpcTarget.Others, spawnedItem.GetComponent<PhotonView>().ViewID, false);
            }
            OnPlayerSpawned();
        }
        else
        {
            playerHand.localPosition += playerHandPosOffsetForNonOwners;
        }
        
    }

    private void Start()
    {
        pv.RPC("TellOwnerToSyncItems", RpcTarget.All);
    }

   

    [PunRPC]
    private void TellOwnerToSyncItems()
    {
        if (pv.IsMine)
        {
            foreach(var item in allAvailableItems)
            {
                pv.RPC("SetItemParent", RpcTarget.All, item.Value.pv.ViewID);
            }

            SelectItem(GetSelectedItem(slotSelected));
        }
    }

    [PunRPC]
    private void SetItemParent(int itemID)
    {
        if (itemsWithSetParent.Contains(itemID) == false)
        {
            var item = PhotonView.Find(itemID);
            if (item != null)
            {
                var wasItemActivated = item.gameObject.activeSelf;
                item.gameObject.SetActive(true);
                item.transform.SetParent(playerHand);
                item.transform.SetPositionAndRotation(playerHand.position, playerHand.rotation);
                item.gameObject.SetActive(wasItemActivated);
            }

            itemsWithSetParent.Add(itemID);
        }
    }

    private void OnEnable()
    {
        if (pv.IsMine)
        {
            PlayerManager.OnLocalPlayerControllerDeSpawned += OnPlayerDeSpawned;
            BombAndDefuse.OnPreRoundStart += OnPreRoundStarted;
            BombAndDefuse.OnRoundEnded += OnRoundEnded;
        }
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            PlayerManager.OnLocalPlayerControllerDeSpawned -= OnPlayerDeSpawned;
            BombAndDefuse.OnPreRoundStart -= OnPreRoundStarted;
            BombAndDefuse.OnRoundEnded -= OnRoundEnded;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (!PauseMenuManager.paused)
            {

                var input = Input.inputString;
                if (int.TryParse(input, out var numberInput))
                {
                    SelectItem(GetItemFromInput(numberInput));
                }

                if (Input.GetKeyDown(dropItemKeyCode) && slotSelected != ItemType.None)
                {
                    var item = GetSelectedItem(slotSelected);
                    if (item.canBeDropped)
                    {
                        DropItem(item);
                        AutoSelectBestItem();
                    }
                }
            }
        }
    }

    private void OnPlayerSpawned()
    {
        var defaultKeys = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] == 1 ? teamTwoDefaultItemKeys : teamOneDefaultItemKeys;

        foreach (var defaultItemKey in defaultKeys)
        {
            GiveItem(defaultItemKey);
        }
    }

    private void OnPlayerDeSpawned()
    {
        //Drop items that should be dropped and destroy others
        List<Item> itemsToDrop = new List<Item>();
        foreach(var item in itemsInInventory)
        {
            itemsToDrop.Add(item);
        }

        foreach(var item in itemsToDrop)
        {
            DropItem(item);
        }
        itemsToDrop.Clear();
    }

    public void OnRoundEnded()
    {
        var itemsToRemove = new List<Item>();

        foreach (var item in itemsInInventory)
        {
            if (item.destroyOnRoundEnd)
            {
                itemsToRemove.Add(item);
                HandleDropItemType(item);
            }
        }

        foreach (var item in itemsToRemove)
        {
            pv.RPC("DestroyItemOnServer", RpcTarget.All, item.pv.ViewID);
        }
    }

    public void OnPreRoundStarted()
    {
        var defaultKeys = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] == 1 ? teamTwoDefaultItemKeys : teamOneDefaultItemKeys;

        foreach(var defaultItemKey in defaultKeys)
        {
            var item = allAvailableItems[defaultItemKey];
            if (itemsInInventory.Contains(item) == false)
            {
                switch (item.type)
                {
                    case ItemType.Primary:
                        if(PrimaryItemSlot == null)
                        {
                            GiveItem(defaultItemKey);
                        }
                        break;
                    case ItemType.Secondary:
                        if(SecondaryItemSlot == null)
                        {
                            GiveItem(defaultItemKey);
                        }
                        break;
                    case ItemType.Knife:
                        if(KnifeSlot == null)
                        {
                            GiveItem(defaultItemKey);
                        }
                        break;
                    case ItemType.Utility:
                        if(UtilitySlot == null)
                        {
                            GiveItem(defaultItemKey);
                        }
                        break;
                    case ItemType.Special:
                        if(SpecialSlot == null)
                        {
                            GiveItem(defaultItemKey);
                        }
                        break;
                }
            }
        }

        foreach(var item in itemsInInventory)
        {
            if(item.TryGetComponent<Weapon>(out var weaponComponent))
            {
                weaponComponent.RefillAmmo();
            }
        }
    }

    public void GiveItem(string itemKey)
    {
        foreach(var availableItem in allAvailableItems)
        {
            if(availableItem.Value.itemKey == itemKey)
            {
                itemsInInventory.Add(availableItem.Value);
                HandleGiveItemType(availableItem.Value);
            }
        }
    }

    public void RemoveItem(Item item)
    {
        var itemType = item.type;
        if (itemsInInventory.Contains(item))
        {
            itemsInInventory.Remove(item);
        }
        switch (itemType)
        {
            case ItemType.Primary:
                if(PrimaryItemSlot == item)
                {
                    PrimaryItemSlot = null;
                }
                break;
            case ItemType.Secondary:
                if (SecondaryItemSlot == item)
                {
                    SecondaryItemSlot = null;
                }
                break;
            case ItemType.Knife:
                if (KnifeSlot == item)
                {
                    KnifeSlot = null;
                }
                break;
            case ItemType.Utility:
                if (UtilitySlot == item)
                {
                    UtilitySlot = null;
                }
                break;
            case ItemType.Special:
                if (SpecialSlot == item)
                {
                    SpecialSlot = null;
                }
                break;
        }
    }

    private void HandleGiveItemType(Item item)
    {
        switch (item.type)
        {
            case ItemType.Primary:
                if (PrimaryItemSlot != null)
                {
                    if (PrimaryItemSlot.canBeDropped)
                        DropItem(PrimaryItemSlot);
                    else
                        return;
                }

                PrimaryItemSlot = item;
                break;
            case ItemType.Secondary:
                if (SecondaryItemSlot != null)
                {
                    if (SecondaryItemSlot.canBeDropped)
                        DropItem(SecondaryItemSlot);
                    else
                        return;
                }
                SecondaryItemSlot = item;
                break;
            case ItemType.Knife:
                if (KnifeSlot != null)
                {
                    if (KnifeSlot.canBeDropped)
                        DropItem(KnifeSlot);
                    else
                        return;
                }
                KnifeSlot = item;
                break;
            case ItemType.Utility:
                if (UtilitySlot != null)
                {
                    if (UtilitySlot.canBeDropped)
                        DropItem(UtilitySlot);
                    else
                        return;
                }
                UtilitySlot = item;
                break;
            case ItemType.Special:
                if (SpecialSlot != null)
                {
                    if (SpecialSlot.canBeDropped)
                        DropItem(SpecialSlot);
                    else
                        return;
                }
                SpecialSlot = item;
                break;
        }

        if(item.canBeSelected)
        {
            SelectItem(item);
        }
    }

    private void HandleDropItemType(Item item)
    {
        switch (item.type)
        {
            case ItemType.Primary:
                PrimaryItemSlot = null;
                break;
            case ItemType.Secondary:
                SecondaryItemSlot = null;
                break;
            case ItemType.Knife:
                KnifeSlot = null;
                break;
            case ItemType.Utility:
                UtilitySlot = null;
                break;
            case ItemType.Special:
                SpecialSlot = null;
                break;
        }
    }

    private void DropItem(Item item)
    {
        bool hasItem = itemsInInventory.Contains(item);
        if (hasItem)
        {
            HandleDropItemType(item);
            if (item.canBeDropped)
            {
                item.OnDropped();
                pv.RPC("ShowHideItem", RpcTarget.All, item.pv.ViewID, false);
                pv.RPC("SpawnDroppedItemByServer", RpcTarget.All, item.pv.ViewID);
                itemsInInventory.Remove(item);
            }
            else
            {
                itemsInInventory.Remove(item);
                //pv.RPC("DestroyItemOnServer", RpcTarget.All, item.pv.ViewID);
                //PhotonNetwork.Destroy(item.pv);
            }
        }
    }

    [PunRPC]
    private void DestroyItemOnServer(int itemViewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var item = PhotonView.Find(itemViewID);
            if (item != null)
            {
                PhotonNetwork.Destroy(item);
            }
        }
    }

    public void PickUpItem(string itemKey)
    {
        if (HasItem(itemKey))
        {
            DropItem(GetItemFromKey(itemKey));
        }
        GiveItem(itemKey);
        //sync ammo
    }

    public Item GetItemFromKey(string key)
    {
        foreach(var item in itemsInInventory)
        {
            if(item.itemKey == key)
            {
                return item;
            }
        }

        return null;
    }

    private Item GetSelectedItem(ItemType selectedSlot)
    {
        foreach(var item in itemsInInventory)
        {
            if(item.type == selectedSlot)
            {
                return item;
            }
        }

        return null;
    }

    private Item GetItemFromInput(int numberInput)
    {
        if(numberInput == 1)
        {
            return PrimaryItemSlot;
        }
        else if(numberInput == 2)
        {
            return SecondaryItemSlot;
        }
        else if(numberInput == 3)
        {
            return KnifeSlot;
        }
        else if(numberInput == 4)
        {
            return UtilitySlot;
        }
        else if(numberInput == 5)
        {
            return SpecialSlot;
        }
        return null;
    }

    private void SelectItem(Item itemToSelect)
    {
        if(itemToSelect == null)
        {
            return;
        }

        foreach(var item in allAvailableItems)
        {
            var showItem = item.Value == itemToSelect;
            item.Value.gameObject.SetActive(showItem);
            pv.RPC("ShowHideItem", RpcTarget.Others, item.Value.pv.ViewID, showItem);
            if (showItem)
            {
                slotSelected = item.Value.type;
                PlayerManager.OnItemSwitched?.Invoke(itemsInInventory.IndexOf(item.Value));
                pv.RPC("PlayAnim", RpcTarget.All, pullOutItemAnimatorTrigger);
                OnItemSelected?.Invoke(item.Value);
            }
        }
    }

    [PunRPC]
    private void PlayAnim(string trigger)
    {
        inventoryHandAnimator.SetTrigger(trigger);
    }

    public void AutoSelectBestItem()
    {
        if(PrimaryItemSlot != null)
        {
            SelectItem(PrimaryItemSlot);
        }
        else if(SecondaryItemSlot != null)
        {
            SelectItem(SecondaryItemSlot);
        }
        else if(KnifeSlot != null)
        {
            SelectItem(KnifeSlot);
        }
        else if(UtilitySlot != null)
        {
            SelectItem(UtilitySlot);
        }
        else
        {
            slotSelected = ItemType.None;
        }
    }

    [PunRPC]
    private void ShowHideItem(int viewID, bool show)
    {
        var itemPhotonView = PhotonView.Find(viewID);
        if (itemPhotonView != null)
        {
            itemPhotonView.gameObject.SetActive(show);
        }
    }

    [PunRPC]
    private void SpawnDroppedItemByServer(int viewIDOfItem)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var photonViewOfItem = PhotonView.Find(viewIDOfItem);
            if (photonViewOfItem != null)
            {
                var item = photonViewOfItem.GetComponent<Item>();
                var spawnedItem = PhotonNetwork.InstantiateRoomObject(item.droppedItemVersion.gameObject.name, playerHand.position, playerHand.rotation);
                spawnedItem.GetComponent<ItemOnGround>().OnSpawned(item);
                if (spawnedItem.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(playerHand.forward * 100);
                }
            }
        }
    }

    private bool HasItem(string itemKey)
    {
        foreach(var item in itemsInInventory)
        {
            if(item.itemKey == itemKey)
            {
                return true;
            }
        }
        return false;
    }


    
    public void SyncWeaponStats(Weapon weapon, int ammo)
    {
        pv.RPC("SyncWeaponStatsWithClients", RpcTarget.Others, weapon.pv.ViewID, ammo);
    }
    [PunRPC]
    private void SyncWeaponStatsWithClients(int itemPhotonViewID, int ammo)
    {
        var obj = PhotonView.Find(itemPhotonViewID);
        if (obj != null)
        {
            var item = obj.GetComponent<Item>();
            if (item != null)
            {
                if (item.TryGetComponent<Weapon>(out var weapon))
                {
                    weapon.ammo = ammo;
                }
            }
        }
    }
}
