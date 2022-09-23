using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponOnGround : ItemOnGround
{
    public int ammo;

    public override void OnSpawned(Item itemDropped)
    {
        base.OnSpawned(itemDropped);
        pv.RPC("SyncAmmoWithServer", RpcTarget.All, itemDropped.GetComponent<Weapon>().ammo);
    }

    [PunRPC]
    public void SyncAmmoWithServer(int ammo)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("ReceiveAmmoValueFromServer", RpcTarget.All, ammo);
        }
    }

    [PunRPC]
    private void ReceiveAmmoValueFromServer(int ammo)
    {
        this.ammo = ammo;
    }

    public override void DestroyOnRoundStart()
    {
        pv.RPC("TellServerToDelete", RpcTarget.AllViaServer);
    }
}
