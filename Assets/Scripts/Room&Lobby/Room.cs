using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;

public class Room : MonoBehaviourPunCallbacks
{
    public static string GameVersion = "0.1";
    public TextMeshProUGUI gameVersionText;

    public TextMeshProUGUI statusText;
    public int mapIndex = 0;
    public string[] mapCodenames;
    public int rounds = 10;
    public float warmupDuration = 10f;
    public float endWarmupDuration = 10f;
    public float preRoundDuration = 10f;
    public float roundDuration = 10f;
    public float endRoundDuration = 10f;
    public float matchEndDuration = 10f;
    public float bombTimer = 10f;
    public float respawnTimer = 10f;
    public byte maxPlayersPerRoom;

    public bool connectedToMaster = false;
    public bool connectingToRoom = false;
    public bool inRoom = false;

    public Action action_OnRoomCreated;
    public Action action_OnRoomJoined;
    public Action action_OnRoomLeft;
    public Action action_OnPropertiesUpdated;


    private void Start()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            Debug.Log("Connected to photon!");
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PlayerPrefs.HasKey("SavedNick") == false)
        {
            var rndName = "Player#" + UnityEngine.Random.Range(1000, 9999);
            PhotonNetwork.LocalPlayer.NickName = rndName;
            PlayerPrefs.SetString("SavedNick", rndName);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        action_OnRoomJoined += SetupPlayerProperties;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        action_OnRoomJoined -= SetupPlayerProperties;
    }

    public void CreateRoom(string _roomName)
    {
        if(PhotonNetwork.IsConnected == false || connectingToRoom || connectedToMaster == false)
        {
            return;
        }


        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add(RoomPropertyKeys.mapKey, mapCodenames[mapIndex]);
        table.Add(RoomPropertyKeys.matchStateKey, MatchState.InRoom);
        table.Add(RoomPropertyKeys.roundsKey, rounds);
        table.Add(RoomPropertyKeys.teamOneScore, 0);
        table.Add(RoomPropertyKeys.teamTwoScore, 0);
        table.Add(RoomPropertyKeys.roundsPlayed, 0);
        table.Add(RoomPropertyKeys.warmupDuration, warmupDuration);
        table.Add(RoomPropertyKeys.endWarmupDuration, endWarmupDuration);
        table.Add(RoomPropertyKeys.preRoundDuration, preRoundDuration);
        table.Add(RoomPropertyKeys.roundDuration, roundDuration);
        table.Add(RoomPropertyKeys.endRoundDuration, endRoundDuration);
        table.Add(RoomPropertyKeys.matchEndDuration, matchEndDuration);
        table.Add(RoomPropertyKeys.bombTimer, bombTimer);
        table.Add(RoomPropertyKeys.respawnTimer, respawnTimer);
        roomOptions.CustomRoomProperties = table;

        connectingToRoom = true;
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }
    public void JoinRoom(string _roomName)
    {
        if (PhotonNetwork.IsConnected == false || connectingToRoom || connectedToMaster == false)
        {
            return;
        }

        if (_roomName == "")
        {
            PrintStatus("Unable to join room with no name");
            return;
        }

        connectingToRoom = true;
        PhotonNetwork.JoinRoom(_roomName);
    }

    [ContextMenu("Leave")]
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            return;
        }

        PhotonNetwork.LeaveRoom();
    }

    [ContextMenu("Lock Room")]
    public void LockRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    [ContextMenu("UnLock Room")]
    public void UnLockRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }
        }
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        if (PhotonNetwork.IsConnected && inRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
            else
            {
                PrintStatus("You are not room master!");
            }
        }
    }

    public void SaveGameSettings()
    {
        UpdateMapProperty(mapIndex);
        UpdateRoundsProperty(rounds);
        UpdateWarmupDurationProperty(warmupDuration);
        UpdateEndWarmupDurationProperty(endWarmupDuration);
        UpdatePreRoundDurationProperty(preRoundDuration);
        UpdateRoundDurationProperty(roundDuration);
        UpdateEndRoundDurationProperty(endRoundDuration);
        UpdateMatchEndDurationProperty(matchEndDuration);
        UpdateBombTimerDurationProperty(bombTimer);
        UpdateRespawnTimerDurationProperty(respawnTimer);
    }

    [ContextMenu("Update Map")]
    public void UpdateMapProperty(int index)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                mapIndex = index;
                SetCustomProperty(RoomPropertyKeys.mapKey, mapCodenames[index]);
            }
        }
    }

    public void UpdateRoundsProperty(int rounds)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                this.rounds = rounds;
                SetCustomProperty(RoomPropertyKeys.roundsKey, rounds);
            }
        }
    }

    public void UpdateWarmupDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                warmupDuration = duration;
                SetCustomProperty(RoomPropertyKeys.warmupDuration, duration);
            }
        }
    }

    public void UpdateEndWarmupDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                endWarmupDuration = duration;
                SetCustomProperty(RoomPropertyKeys.endWarmupDuration, duration);
            }
        }
    }

    public void UpdatePreRoundDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                preRoundDuration = duration;
                SetCustomProperty(RoomPropertyKeys.preRoundDuration, duration);
            }
        }
    }

    public void UpdateRoundDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                roundDuration = duration;
                SetCustomProperty(RoomPropertyKeys.roundDuration, duration);
            }
        }
    }

    public void UpdateEndRoundDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                endRoundDuration = duration;
                SetCustomProperty(RoomPropertyKeys.endRoundDuration, duration);
            }
        }
    }

    public void UpdateMatchEndDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                matchEndDuration = duration;
                SetCustomProperty(RoomPropertyKeys.matchEndDuration, duration);
            }
        }
    }

    public void UpdateBombTimerDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                bombTimer = duration;
                SetCustomProperty(RoomPropertyKeys.bombTimer, duration);
            }
        }
    }

    public void UpdateRespawnTimerDurationProperty(float duration)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (inRoom && PhotonNetwork.IsMasterClient)
            {
                respawnTimer = duration;
                SetCustomProperty(RoomPropertyKeys.respawnTimer, duration);
            }
        }
    }

    private void SetCustomProperty(object key, object value)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient && inRoom)
            {
                ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
                property.Add(key, value);
                PhotonNetwork.CurrentRoom.SetCustomProperties(property);
            }
        }
    }

    public void PrintStatus(string message)
    {
        CancelInvoke(nameof(HideStatusText));
        Invoke(nameof(HideStatusText), 5f);
        statusText.text = message;
    }

    private void HideStatusText()
    {
        statusText.text = string.Empty;
    }

    public static void CopyRoomName()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
    }

    private void SetupPlayerProperties()
    {
        SetProperty(PlayerPropertyKeys.killsKey, 0);
        SetProperty(PlayerPropertyKeys.deathsKey, 0);
        SetProperty(PlayerPropertyKeys.teamIDKey, -1);
        SetProperty(PlayerPropertyKeys.isDeadKey, false);
    }

    private void SetProperty(object key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add(key, value);

        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }

    #region Pun Callbacks

    public override void OnCreatedRoom()
    {
        inRoom = true;

        Debug.Log("OnCreated");

        action_OnRoomCreated?.Invoke();
    }

    public override void OnLeftRoom()
    {
        inRoom = false;
        connectingToRoom = false;
        Debug.Log("OnLeftRoom");

        action_OnRoomLeft?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        inRoom = true;
        connectingToRoom = false;

        Debug.Log("OnJoinedRoom");
        action_OnRoomJoined?.Invoke();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnected");
        connectedToMaster = true;
        gameVersionText.text = PhotonNetwork.GameVersion;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectedToMaster = false;
        Debug.Log("OnDisconnect");
        PrintStatus("Disconnected!");
        gameVersionText.text = "OFFLINE";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        connectingToRoom = false;
        Debug.Log("OnJoinRoomFailed, returnCode : " + returnCode + ". " + message);
        PrintStatus(message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        connectingToRoom = false;
        Debug.Log("OnCreateRoomFailed, returnCode : " + returnCode + ". " + message);
        PrintStatus(message);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        action_OnPropertiesUpdated?.Invoke();
    }

    #endregion



   

    
}
