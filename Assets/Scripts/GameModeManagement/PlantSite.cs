using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlantSite : MonoBehaviour
{
    public static int inSiteValue;
    private bool hasEnteredSite = false;

    private void OnEnable()
    {
        BombAndDefuse.OnPreRoundStart += ResetBombSiteValue;
    }

    private void OnDisable()
    {
        BombAndDefuse.OnPreRoundStart -= ResetBombSiteValue;
    }

    private void ResetBombSiteValue()
    {
        inSiteValue = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PhotonView>(out var pv))
        {
            if(pv.Owner == PhotonNetwork.LocalPlayer)
            {
                if (hasEnteredSite == false) 
                {
                    inSiteValue++;
                    hasEnteredSite = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PhotonView>(out var pv))
        {
            if (pv.Owner == PhotonNetwork.LocalPlayer)
            {
                if (hasEnteredSite)
                {
                    hasEnteredSite = false;
                    inSiteValue--;
                }
            }
        }
    }
}
