using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public static PlayerManager instance;

    public GameObject playerPrefab;
    public GameObject spectatePrefab;
    public GameObject localSpawnedPlayer = null;
    public GameObject localSpawnedSpectatePlayer = null;

    public bool readyToJoin = false;
    public static bool selectedTeam = false;

    public static Action<PlayerController> OnLocalPlayerControllerSpawned;
    public static Action OnLocalPlayerControllerDeSpawned;
    public static Action<string> SendInteractTextMessage;
    public static Action<int> OnItemSwitched;

    private void Awake()
    {
        if (pv.IsMine)
        {
            instance = this;
        }
        PlayersInMatch.instance.AddPlayer(pv.Owner);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (pv.IsMine)
        {
            instance = null;
        }
        else
        {
            PlayersInMatch.instance.RemovePlayer(pv.Owner);
        }
    }

    public void OnTeamSelected()
    {
        readyToJoin = true;


        if (PlayerStats.instance.teamID != -1)
        {
            if (BombAndDefuse.playerCanSpawnIn)
            {
                if (localSpawnedPlayer == null)
                {
                    SpawnLocalPlayer();
                }
                else
                {
                    DeSpawnLocalPlayer();
                    SpawnLocalPlayer();
                }
            }
        }
        else
        {
            DeSpawnLocalPlayer();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(PhotonNetwork.LocalPlayer == targetPlayer)
        {
            if (changedProps.ContainsKey(PlayerPropertyKeys.teamIDKey) && selectedTeam)
            {
                OnTeamSelected();
                selectedTeam = false;
            }
        }
    }

    [ContextMenu("Spawn")]
    public void SpawnLocalPlayer()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            Debug.LogWarning("Tried spawning local player without being connected to Photon!");
            return;
        }

        if(localSpawnedPlayer != null || (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] == -1)
        {
            return;
        }

        if(localSpawnedSpectatePlayer != null)
        {
            PhotonNetwork.Destroy(localSpawnedSpectatePlayer);
            localSpawnedSpectatePlayer = null;
        }

        localSpawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, GetSpawnPosition(), GetSpawnRotation());
        PlayerStats.instance.isDead = false;
        PlayerStats.instance.UpdateIsDead();

        OnLocalPlayerControllerSpawned?.Invoke(localSpawnedPlayer.GetComponent<PlayerController>());
        PlayersInMatch.instance.OnPlayerSpawned(PhotonNetwork.LocalPlayer);
    }

    [ContextMenu("DeSpawn")]
    public void DeSpawnLocalPlayer()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            Debug.LogWarning("Tried DeSpawning local player without being connected to Photon!");
            return;
        }

        if (localSpawnedSpectatePlayer == null)
        {
            var spawnForSpectator = localSpawnedPlayer != null ? localSpawnedPlayer.GetComponentInChildren<PlayerCamera>().transform : transform;
            localSpawnedSpectatePlayer = PhotonNetwork.Instantiate(spectatePrefab.name, spawnForSpectator.position, spawnForSpectator.rotation);
        }

        if (localSpawnedPlayer != null) 
        {
            OnLocalPlayerControllerDeSpawned?.Invoke();

            PhotonNetwork.Destroy(localSpawnedPlayer);
            localSpawnedPlayer = null;
            PlayerStats.instance.isDead = true;
            PlayerStats.instance.deaths++;
            PlayerStats.instance.UpdateDeaths();
            PlayerStats.instance.UpdateIsDead();
            PlayersInMatch.instance.OnPlayerDeSpawned(PhotonNetwork.LocalPlayer);
        }

        

        
    }

    private Vector3 GetSpawnPosition()
    {
        return Vector3.zero;
    }

    private Quaternion GetSpawnRotation()
    {
        return Quaternion.identity;
    }
}
