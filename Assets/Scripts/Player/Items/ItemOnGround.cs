using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemOnGround : MonoBehaviourPunCallbacks, IInteractable, DeleteOnRoundStart
{
    private Vector3 initSpawnPos;

    public PhotonView pv;
    public Item itemPrefab;

    public List<int> teamsThatCanInteract = new List<int>() { 0, 1 };
    public bool canInteract { get; set; }

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] != null)
        {
            canInteract = teamsThatCanInteract.Contains((int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey]);
        }

        
        GetComponent<Rigidbody>().isKinematic = !PhotonNetwork.IsMasterClient;
        if (pv.IsMine)
        {
            initSpawnPos = transform.position;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if(transform.position.y < -100)
            {
                transform.position = initSpawnPos;
            }
        }
    }

    public virtual void OnSpawned(Item itemDropped)
    {
        
    }

    [ContextMenu("Interact")]
    public void Interact()
    {
        PlayerInventory.instance.PickUpItem(itemPrefab.itemKey);
        pv.RPC("OnPickedUpServer", RpcTarget.All);
    }

    [PunRPC]
    public void OnPickedUpServer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public virtual void DestroyOnRoundStart()
    {
        pv.RPC("TellServerToDelete", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void TellServerToDelete()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(pv);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(PlayerPropertyKeys.teamIDKey))
        {
            canInteract = teamsThatCanInteract.Contains((int)changedProps[PlayerPropertyKeys.teamIDKey]);
        }
    }
}
