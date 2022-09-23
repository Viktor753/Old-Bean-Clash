using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerBuyMenu : MonoBehaviour
{
    public PhotonView pv;

    public int money = int.MaxValue;
    public GameObject canvas;
    public GameObject buyMenuItemPrefab;
    public PlayerInventory inventory;

    public BuyMenuItem[] rifles;
    public BuyMenuItem[] pistols;
    public BuyMenuItem[] smgs;
    public BuyMenuItem[] heavy;
    public BuyMenuItem[] gear;
    public BuyMenuItem[] grenades;
    [Space]
    public Transform riflesParent, pistolsParent, smgsParent, heavyParent, gearParent, grenadesParent;

    [System.Serializable]
    public class BuyMenuItem
    {
        public string itemName;
        public string itemKey;
        public Sprite icon;
        public int cost;
    }

    private void Start()
    {
        canvas.SetActive(false);

        if (pv.IsMine)
        {
            SetupBuyMenuItems();
        }
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            if (PauseMenuManager.pauseFactors.Contains(this))
            {
                PauseMenuManager.RemovePauseFactor(this);
            }
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (canvas.activeSelf == false)
                {
                    if (PauseMenuManager.paused == false)
                    {
                        PauseMenuManager.AddPauseFactor(this);
                        canvas.SetActive(true);
                    }
                }
                else
                {
                    PauseMenuManager.RemovePauseFactor(this);
                    canvas.SetActive(false);
                }
            }
        }
    }

    public void ShowMenu(Transform parent)
    {
        riflesParent.gameObject.SetActive(riflesParent == parent);
        pistolsParent.gameObject.SetActive(pistolsParent == parent);
        smgsParent.gameObject.SetActive(smgsParent == parent);
        heavyParent.gameObject.SetActive(heavyParent == parent);
        gearParent.gameObject.SetActive(gearParent == parent);
        grenadesParent.gameObject.SetActive(grenadesParent == parent);
    }

    private void SetupBuyMenuItems()
    {
        ClearOutPreviousItems();

        SetupItems(rifles, riflesParent);
        SetupItems(pistols, pistolsParent);
        SetupItems(smgs, smgsParent);
        SetupItems(heavy, heavyParent);
        SetupItems(gear, gearParent);
        SetupItems(grenades, grenadesParent);
    }

    private void SetupItems(BuyMenuItem[] itemArray, Transform parent)
    {
        foreach (var item in itemArray)
        {
            var obj = Instantiate(buyMenuItemPrefab, parent);
            obj.GetComponent<Image>().sprite = item.icon;
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.itemName;
            obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"${item.cost}";

            obj.GetComponent<Button>().onClick.AddListener(delegate { AttemptBuyItem(item.itemKey, item.cost); });
        }
    }

    public void AttemptBuyItem(string itemKey, int cost)
    {
        if(money >= cost)
        {
            inventory.GiveItem(itemKey);
            money -= cost;
        }
    }

    private void ClearOutPreviousItems()
    {
        for (int i = 0; i < riflesParent.childCount; i++)
        {
            Destroy(riflesParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < pistolsParent.childCount; i++)
        {
            Destroy(pistolsParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < smgsParent.childCount; i++)
        {
            Destroy(smgsParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < heavyParent.childCount; i++)
        {
            Destroy(heavyParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < gearParent.childCount; i++)
        {
            Destroy(gearParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < grenadesParent.childCount; i++)
        {
            Destroy(grenadesParent.GetChild(i).gameObject);
        }
    }


    
}