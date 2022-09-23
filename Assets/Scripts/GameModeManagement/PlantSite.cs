using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlantSite : MonoBehaviour
{
    //Multiple instances of this script will exist, foreach bombsite we want to increase the value by 1 when we enter a site, and decrease by 1 when we leave it.
    //When we plant, we want to check if the value is more than 1. Meaning, are we in a site?
    //This value gets reset at the start of every round in case we entered a site but never left it during a round
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
                    Debug.Log("Entered site!");
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
                    Debug.Log("Exited site!");
                    hasEnteredSite = false;
                    inSiteValue--;
                }
            }
        }
    }
}
