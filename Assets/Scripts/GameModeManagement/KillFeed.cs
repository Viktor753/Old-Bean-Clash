using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System;
using Photon.Pun;

public class KillFeed : MonoBehaviour
{
    public PhotonView pv;
    public static KillFeed instance;

    public Transform killFeedPanel;
    public KillFeedObject killObject;
    public float objLifetime = 6.0f;

    public WeaponSprites[] weaponSprites;
    public Sprite defaultKillWeaponIcon;

    private void Awake()
    {
        instance = this;
    }

    public void OnKill(Player killer, Player victim, string weaponKey)
    {
        pv.RPC("SpawnKillFeedObjOnAllClients", RpcTarget.All, killer, victim, weaponKey);
    }

    [PunRPC]
    private void SpawnKillFeedObjOnAllClients(Player killer, Player victim, string weaponKey)
    {
        var newKill = Instantiate(killObject, killFeedPanel);
        var weaponIcon = GetIcon(weaponKey);
        newKill.SetupUI(weaponIcon, killer, victim);
        Destroy(newKill.gameObject, objLifetime);
    }

    private Sprite GetIcon(string itemKey)
    {
        foreach(var sprite in weaponSprites)
        {
            if(sprite.itemKey == itemKey)
            {
                return sprite.icon;
            }
        }

        return defaultKillWeaponIcon;
    }

    [Serializable]
    public class WeaponSprites
    {
        public string itemKey;
        public Sprite icon;
    }
}
