using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorInHand : Item
{
    public int armorAmount;

    private void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Health.localInstance.AddArmor(armorAmount);
                PlayerInventory.instance.itemsInInventory.Remove(this);
                PlayerInventory.instance.UtilitySlot = null;
                PlayerInventory.instance.AutoSelectBestItem();
            }
        }
    }
}
