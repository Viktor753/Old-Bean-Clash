using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class PlayersInMatch : MonoBehaviour
{
    public PhotonView pv;
    public static PlayersInMatch instance;
    public List<Player> players = new List<Player>();

    public static Action<Player> OnPlayerLeftMatch;
    public static Action<Player> OnPlayerEnteredMatch;
    public static Action<Player> PlayerSpawned;
    public static Action<Player> PlayerDeSpawned;


    public GameObject teamSelectUI;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        OpenTeamSelect();
    }

    private void Update()
    {
        if (teamSelectUI.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTeamSelect();
        }
    }

    public void OpenTeamSelect()
    {
        teamSelectUI.SetActive(true);
        PauseMenuManager.AddPauseFactor(this);
    }

    public void CloseTeamSelect()
    {
        teamSelectUI.SetActive(false);
        PauseMenuManager.RemovePauseFactor(this);
    }


    public void OnPlayerSpawned(Player owner)
    {
        pv.RPC("TellClientsPlayerSpawned", RpcTarget.All, owner);
    }

    [PunRPC]
    private void TellClientsPlayerSpawned(Player owner)
    {
        PlayerSpawned?.Invoke(owner);
    }

    public void OnPlayerDeSpawned(Player owner)
    {
        pv.RPC("TellClientsPlayerDeSpawned", RpcTarget.All, owner);
    }

    [PunRPC]
    private void TellClientsPlayerDeSpawned(Player owner)
    {
        PlayerDeSpawned?.Invoke(owner);
    }

    public void AddPlayer(Player player)
    {
        if (players.Contains(player) == false)
        {
            players.Add(player);
            OnPlayerEnteredMatch?.Invoke(player);
        }
    }

    public void RemovePlayer(Player player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            OnPlayerLeftMatch?.Invoke(player);
        }
    }

    public void SetLocalTeam(int teamIDToSet)
    {
        if (PlayerStats.instance.teamID != teamIDToSet)
        {
            PlayerManager.selectedTeam = true;
            PlayerStats.instance.teamID = teamIDToSet;
            PlayerStats.instance.UpdateTeam();
        }

        teamSelectUI.SetActive(false);
    }
}
