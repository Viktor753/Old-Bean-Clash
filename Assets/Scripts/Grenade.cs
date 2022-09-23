using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : Item
{
    //This is in the player hand
    public GameObject grenadeToThrowPrefab;
    public bool removeFromInventoryOnThrow = true;

    private void Update()
    {
        if (pv.IsMine)
        {
            if (PauseMenuManager.paused == false && BombAndDefuse.canUseItems && itemPulledOut)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var origin = PlayerInventory.instance.GetComponentInChildren<PlayerCamera>().transform;
                    Throw(origin.position, origin.rotation);
                    if (removeFromInventoryOnThrow)
                    {
                        PlayerInventory.instance.RemoveItem(this);
                        PlayerInventory.instance.AutoSelectBestItem();
                    }
                }
            }
        }
    }

    private void Throw(Vector3 pos, Quaternion rotation)
    {
        PhotonNetwork.Instantiate(grenadeToThrowPrefab.name, pos, rotation);
    }
}
