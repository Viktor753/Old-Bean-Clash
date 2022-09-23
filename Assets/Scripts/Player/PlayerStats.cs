using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerStats : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public PlayerManager manager;
    public static PlayerStats instance;

    public int kills = 0;
    public int deaths = 0;
    public int teamID = -1;
    public bool isDead = false;

    private bool propertiesSetup = false;

    private void Start()
    {
        if (pv.IsMine)
        {
            instance = this;
            kills = 0;
            deaths = 0;
            teamID = -1;
            isDead = true;
            UpdateKills();
            UpdateDeaths();
            UpdateTeam();
            UpdateIsDead();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (pv.IsMine)
        {
            BombAndDefuse.OnMatchStart += ResetPlayerStats;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (pv.IsMine)
        {
            BombAndDefuse.OnMatchStart -= ResetPlayerStats;
        }
    }

    public void ResetPlayerStats()
    {
        kills = 0;
        deaths = 0;
        UpdateKills();
        UpdateDeaths();
    }

    [ContextMenu("Update kills")]
    public void UpdateKills()
    {
        if (pv.IsMine)
        {
            SetProperty(PlayerPropertyKeys.killsKey, kills);
        }
    }

    [ContextMenu("Update deaths")]
    public void UpdateDeaths()
    {
        if (pv.IsMine)
        {
            SetProperty(PlayerPropertyKeys.deathsKey, deaths);
        }
    }

    [ContextMenu("Update Team")]
    public void UpdateTeam()
    {
        if (pv.IsMine)
        {
            SetProperty(PlayerPropertyKeys.teamIDKey, teamID);
        }
    }

    [ContextMenu("Update IsDead")]
    public void UpdateIsDead()
    {
        if (pv.IsMine)
        {
            SetProperty(PlayerPropertyKeys.isDeadKey, isDead);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (pv.IsMine)
        {
            if (targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.killsKey) &&
                   targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.deathsKey) &&
                   targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.teamIDKey) &&
                   targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.isDeadKey))
            {

                if (targetPlayer == PhotonNetwork.LocalPlayer && propertiesSetup == false)
                {
                    propertiesSetup = true;
                }
            }
        }
    }

    private void SetProperty(object key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add(key, value);

        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }
}
