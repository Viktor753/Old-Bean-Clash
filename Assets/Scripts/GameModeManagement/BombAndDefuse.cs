using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Linq;

public class BombAndDefuse : MonoBehaviourPunCallbacks
{
    public static BombAndDefuse instance;
    public PhotonView pv;
    public static bool playerCanSpawnIn = true;
    public static bool canUseItems = true;
    public Transform teamOneSpawn;
    public Transform teamTwoSpawn;
    public MatchState state = MatchState.InRoom;

    public AudioSource source;
    public AudioClip roundDrawClip, teamOneWinClip, teamTwoWinClip;

    private float warmupDuration = 60*5;
    private float endWarmUpDuration = 15;
    private float preRoundDuration = 15;
    private float roundDuration = 3*60;
    private float endRoundDuration = 10;
    private float endMatchDuration = 30;

    public float bombTimer = 45;
    [Space]
    public int teamOnePoints = 0;
    public int teamTwoPoints = 0;
    public int teamSize = 5;
    [Space]
    public int roundsPlayed = 0;
    public int maximumRounds = 10;
    [Space]
    private float respawnTime;

    private float currentTimerVariable = 0.0f;
    private bool freezeTimer = true;
    


    public Action<MatchState> OnMatchStateUpdated;
    public Action<int> OnRoundsPlayedUpdated;
    public Action<int> OnTeamOneScoreUpdated;
    public Action<int> OnTeamTwoScoreUpdated;
    public Action<int> OnTeamOneAliveCountUpdated;
    public Action<int> OnTeamTwoAliveCountUpdated;
    public Action<float> OnTimerUpdated;
    public static Action OnMatchStart;
    public static Action OnPreRoundStart;
    public static Action OnRoundStarted;
    public static Action OnRoundEnded;

    public Action<bool> isBombPlanted;

    private bool bombPlanted = false;
    private int teamOneAliveCount;
    private int teamTwoAliveCount;

    private bool roundDecided = false;

    public ItemOnGround bombOnGround;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        FetchCanJoinBoolFromMaster();

        state = (MatchState)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.matchStateKey];
        maximumRounds = (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.roundsKey];
        teamOnePoints = (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.teamOneScore];
        teamTwoPoints = (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.teamTwoScore];
        roundsPlayed = (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.roundsPlayed];
        warmupDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.warmupDuration];
        endWarmUpDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.endWarmupDuration];
        preRoundDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.preRoundDuration];
        roundDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.roundDuration];
        endRoundDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.endRoundDuration];
        endMatchDuration = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.matchEndDuration];
        bombTimer = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.bombTimer];
        respawnTime = (float)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.respawnTimer];
        OnMatchStateUpdated?.Invoke(state);
        OnRoundsPlayedUpdated?.Invoke(roundsPlayed);
        OnTeamOneScoreUpdated?.Invoke(teamOnePoints);
        OnTeamTwoScoreUpdated?.Invoke(teamTwoPoints);

        if (PhotonNetwork.IsMasterClient)
        {
            StartWarmup();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayersInMatch.PlayerSpawned += OnPlayerSpawned;
        PlayersInMatch.PlayerDeSpawned += OnPlayerDeSpawned;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayersInMatch.PlayerSpawned -= OnPlayerSpawned;
        PlayersInMatch.PlayerDeSpawned -= OnPlayerDeSpawned;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            if (freezeTimer == false)
            {
                if (currentTimerVariable > 0)
                {
                    currentTimerVariable -= Time.deltaTime;
                    if (currentTimerVariable <= 0)
                    {
                        OnTimerZero();
                    }
                }

                OnTimerUpdated?.Invoke(currentTimerVariable);
            }
        }
    }

    private void ClearItemsOnGround()
    {
        foreach(var obj in FindObjectsOfType<MonoBehaviour>().OfType<DeleteOnRoundStart>())
        {
            obj.DestroyOnRoundStart();
        }
    }

    [PunRPC]
    private void FetchCanJoinBoolFromMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("SyncVariables", RpcTarget.All, playerCanSpawnIn, bombPlanted, canUseItems);
        }
    }

    [PunRPC]
    private void SyncVariables(bool canSpawn, bool bombPlanted, bool itemsCanBeUsed)
    {
        playerCanSpawnIn = canSpawn;
        this.bombPlanted = bombPlanted;
        canUseItems = itemsCanBeUsed;

        isBombPlanted?.Invoke(this.bombPlanted);
    }

    private void OnTimerZero()
    {
        switch (state)
        {
            case MatchState.Warmup:
                StartMatch();
                break;
            case MatchState.WarmupEnding:
                pv.RPC("TellPlayersMatchStarted", RpcTarget.All);
                StartPreRound();
                break;
            case MatchState.PreRound:
                StartRound();
                break;
            case MatchState.RoundPlaying:
                if (bombPlanted == false)
                {
                    EndRound();
                }
                break;
            case MatchState.RoundEnding:
                StartPreRound();
                break;
            case MatchState.MatchEnding:
                freezeTimer = true;
                break;
        }
    }

    public void StartWarmup()
    {
        canUseItems = true;
        pv.RPC("SyncVariables", RpcTarget.All, true, bombPlanted, canUseItems);
        freezeTimer = false;
        currentTimerVariable = warmupDuration;
        UpdateMatchStateProperty(MatchState.Warmup);
    }

    public void EndWarmup()
    {
        canUseItems = false;
        currentTimerVariable = endWarmUpDuration;
        UpdateMatchStateProperty(MatchState.WarmupEnding);
        pv.RPC("DeSpawnAllPlayers", RpcTarget.All);
    }

    public void ToggleGamePause()
    {
        freezeTimer = !freezeTimer;
    }

    [PunRPC]
    private void DeSpawnAllPlayers()
    {
        PlayerManager.instance.DeSpawnLocalPlayer();
    }

    public void StartMatch()
    {
        UpdateRoundsPlayed(0);
        UpdateTeamOneScore(0);
        UpdateTeamTwoScore(0);

        EndWarmup();
    }

    [PunRPC]
    private void TellPlayersMatchStarted()
    {
        OnMatchStart?.Invoke();
    }

    public void EndMatch()
    {
        canUseItems = false;
        currentTimerVariable = endMatchDuration;
        UpdateMatchStateProperty(MatchState.MatchEnding);
        if(teamOnePoints > teamTwoPoints)
        {
            //TeamOne won
        }
        else if(teamTwoPoints > teamOnePoints)
        {
            //TeamTwo won
        }
        else
        {
            //Draw
        }

        //Show end screen with stats. Button to leave room or Quit application
    }

    public void StartPreRound()
    {
        canUseItems = false;
        ClearItemsOnGround();

        roundDecided = false;
        pv.RPC("SyncVariables", RpcTarget.All, true, false, canUseItems);
        pv.RPC("SpawnPlayers", RpcTarget.All);
        DropBombInTeamTwoSpawn();

        TeleportPlayersToSpawn();
        currentTimerVariable = preRoundDuration;
        UpdateMatchStateProperty(MatchState.PreRound);
    }

    [PunRPC]
    private void SwitchSides()
    {
        int currentTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey];
        int newTeam = -1;
        if(currentTeam == 0)
        {
            newTeam = 1;
        }
        else if(currentTeam == 1)
        {
            newTeam = 0;
        }
        PlayersInMatch.instance.SetLocalTeam(newTeam);


        //Reduce death by 1, since switching sides will despawn player -> giving them 1 more death
        PlayerStats.instance.deaths--;
        PlayerStats.instance.UpdateDeaths();
    }

    private void DropBombInTeamTwoSpawn()
    {
        PhotonNetwork.InstantiateRoomObject(bombOnGround.name, teamTwoSpawn.GetChild(UnityEngine.Random.Range(0,teamTwoSpawn.childCount)).position + Vector3.up * 1.35f, Quaternion.identity);
    }

    public void StartRound()
    {
        canUseItems = true;
        pv.RPC("SyncVariables", RpcTarget.All, false, bombPlanted, canUseItems);
        currentTimerVariable = roundDuration;
        UpdateMatchStateProperty(MatchState.RoundPlaying);

        pv.RPC("TellPlayersRoundStarted", RpcTarget.All);
    }

    [PunRPC]
    private void TellPlayersRoundStarted()
    {
        OnRoundStarted?.Invoke();
    }

    public void EndRound()
    {
        if (roundDecided)
        {
            return;
        }

        roundDecided = true;
        UpdateRoundsPlayed(roundsPlayed + 1);
        
        if(roundsPlayed == maximumRounds)
        {
            currentTimerVariable = endMatchDuration;
            UpdateMatchStateProperty(MatchState.MatchEnding);
        }
        else
        {
            currentTimerVariable = endRoundDuration;
            UpdateMatchStateProperty(MatchState.RoundEnding);

            if (roundsPlayed == maximumRounds / 2)
            {
                pv.RPC("SwitchSides", RpcTarget.All);
            }
        }

        pv.RPC("TellPlayersRoundEnded", RpcTarget.All);
    }

    [PunRPC]
    private void TellPlayersRoundEnded()
    {
        OnRoundEnded?.Invoke();
    }

    [PunRPC]
    private void SpawnPlayers()
    {
        OnPreRoundStart?.Invoke();
        var localPlayerManager = PlayerManager.instance;
        if(localPlayerManager.localSpawnedPlayer == null && (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] != -1)
        {
            if (localPlayerManager.readyToJoin)
            {
                localPlayerManager.SpawnLocalPlayer();
            }
            else
            {
                var healthComponent = localPlayerManager.localSpawnedPlayer.GetComponent<Health>();
                healthComponent.SetHealth(healthComponent.baseHealth);
            }
        }
    }

    [ContextMenu("Spawn")]
    public void TeleportPlayersToSpawn()
    {
        int teamOneSpawnIndex = 0;
        int teamTwoSpawnIndex = 0;
        foreach(var player in PlayersInMatch.instance.players)
        {
            var playerTeamID = player.CustomProperties[PlayerPropertyKeys.teamIDKey];
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.Euler(0, 0, 0);
            if((int)playerTeamID == 0)
            {
                teamOneSpawnIndex++;
                spawnPos = teamOneSpawn.GetChild(teamOneSpawnIndex).position;
                spawnRot = teamOneSpawn.GetChild(teamOneSpawnIndex).rotation;
            }
            else
            {
                teamTwoSpawnIndex++;
                spawnPos = teamTwoSpawn.GetChild(teamTwoSpawnIndex).position;
                spawnRot = teamTwoSpawn.GetChild(teamTwoSpawnIndex).rotation;
            }

            pv.RPC("TeleportPlayer", RpcTarget.AllViaServer, player, spawnPos, spawnRot);
        }
    }

    [PunRPC]
    private void TeleportPlayer(Player player, Vector3 pos, Quaternion rot)
    {
        if(player == PhotonNetwork.LocalPlayer)
        {
            var playerManager = PlayerManager.instance;
            if (playerManager.localSpawnedPlayer != null)
            {
                playerManager.localSpawnedPlayer.transform.SetPositionAndRotation(pos, rot);
            }
        }
    }

    private int TeamOneAliveCount()
    {
        int teamOneAliveCount = 0;
        foreach (var _player in PlayersInMatch.instance.players)
        {
            if ((int)_player.CustomProperties[PlayerPropertyKeys.teamIDKey] == 0)
            {
                if ((bool)_player.CustomProperties[PlayerPropertyKeys.isDeadKey] == false)
                {
                    teamOneAliveCount++;
                }
            }
        }

        return teamOneAliveCount;
    }

    private int TeamTwoAliveCount()
    {
        int teamTwoAliveCount = 0;
        foreach (var _player in PlayersInMatch.instance.players)
        {
            if ((int)_player.CustomProperties[PlayerPropertyKeys.teamIDKey] == 1)
            {
                if ((bool)_player.CustomProperties[PlayerPropertyKeys.isDeadKey] == false)
                {
                    teamTwoAliveCount++;
                }
            }
        }

        return teamTwoAliveCount;
    }

    private void UpdateTeamAliveCounts()
    {
        this.teamOneAliveCount = TeamOneAliveCount();
        this.teamTwoAliveCount = TeamTwoAliveCount();
        OnTeamOneAliveCountUpdated?.Invoke(this.teamOneAliveCount);
        OnTeamTwoAliveCountUpdated?.Invoke(this.teamTwoAliveCount);
    }

    [PunRPC]
    private void PlayRoundDrawSound()
    {
        source.PlayOneShot(roundDrawClip);
    }

    [PunRPC]
    private void PlayTeamOneWinSound()
    {
        source.PlayOneShot(teamOneWinClip);
    }

    [PunRPC]
    private void PlayTeamTwoWinSound()
    {
        source.PlayOneShot(teamTwoWinClip);
    }

    private void OnPlayerSpawned(Player player)
    {
        
    }

    private void OnPlayerDeSpawned(Player player)
    {
        if(player == PhotonNetwork.LocalPlayer)
        {
            Invoke(nameof(RespawnPlayer), respawnTime);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (roundDecided == false)
            {
                if (state == MatchState.RoundPlaying || state == MatchState.PreRound)
                {
                    if (bombPlanted)
                    {
                        if (this.teamOneAliveCount <= 0)
                        {
                            Debug.Log("TeamCount changed. Bomb planted, team two wins!");
                            UpdateTeamTwoScore(teamTwoPoints + 1);
                            EndRound();
                            pv.RPC("PlayTeamTwoWinSound", RpcTarget.All);
                        }
                    }
                    else
                    {
                        if (this.teamOneAliveCount <= 0)
                        {
                            //Team two won
                            Debug.Log("TeamCount changed. Team two wins");
                            UpdateTeamTwoScore(teamTwoPoints + 1);
                            EndRound();
                            pv.RPC("PlayTeamTwoWinSound", RpcTarget.All);
                        }
                        else if (this.teamTwoAliveCount <= 0)
                        {
                            //Team one won
                            Debug.Log("TeamCount changed. Team one wins");
                            UpdateTeamOneScore(teamOnePoints + 1);
                            EndRound();
                            pv.RPC("PlayTeamOneWinSound", RpcTarget.All);
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("Plant bomb")]
    public void OnBombPlanted()
    {
        if (state == MatchState.RoundPlaying || state == MatchState.PreRound)
        {
            bombPlanted = true;
            currentTimerVariable = bombTimer;
            pv.RPC("SyncVariables", RpcTarget.All, false, bombPlanted, canUseItems);
        }
    }

    [ContextMenu("Defuse Bomb")]
    public void OnBombDefused()
    {
        if (bombPlanted && roundDecided == false)
        {
            bombPlanted = false;
            UpdateTeamOneScore(teamOnePoints + 1);
            EndRound();
            Invoke(nameof(DelayTeamOneWinSound),1.907f);
            pv.RPC("SyncVariables", RpcTarget.All, false, bombPlanted, canUseItems);
        }
    }

    private void DelayTeamOneWinSound()
    {
        pv.RPC("PlayTeamOneWinSound", RpcTarget.All);
    }

    public void OnBombExploded()
    {
        if(bombPlanted && roundDecided == false)
        {
            bombPlanted = false;
            UpdateTeamTwoScore(teamTwoPoints + 1);
            pv.RPC("PlayTeamTwoWinSound", RpcTarget.All);
            EndRound();
            pv.RPC("SyncVariables", RpcTarget.All, false, bombPlanted, canUseItems);
        }
    }


    [ContextMenu("Reduce timer to 0")]
    public void Test2()
    {
        OnTimerZero();
    }

    private void UpdateMatchStateProperty(MatchState newState)
    {
        state = newState;
        SetRoomProperty(RoomPropertyKeys.matchStateKey, newState);
    }

    private void UpdateRoundsPlayed(int newValue)
    {
        roundsPlayed = newValue;
        SetRoomProperty(RoomPropertyKeys.roundsPlayed, roundsPlayed);
    }

    private void UpdateTeamOneScore(int newScore)
    {
        teamOnePoints = newScore;
        SetRoomProperty(RoomPropertyKeys.teamOneScore, teamOnePoints);
    }

    private void UpdateTeamTwoScore(int newScore)
    {
        teamTwoPoints = newScore;
        SetRoomProperty(RoomPropertyKeys.teamTwoScore, teamTwoPoints);
    }

    private void SetRoomProperty(object key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add(key, value);

        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    private void SetPlayerProperty(object key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add(key, value);

        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }

    private void RespawnPlayer()
    {
        if (IsPlayerAllowedToSpawn())
        {
            if (PlayerManager.instance.localSpawnedPlayer == null)
            {
                PlayerManager.instance.SpawnLocalPlayer();
            }
        }
    }

    private bool IsPlayerAllowedToSpawn()
    {
        bool validState = state == MatchState.Warmup;
        return validState;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PlayerPropertyKeys.isDeadKey))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                UpdateTeamAliveCounts();
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomPropertyKeys.matchStateKey))
        {
            switch ((MatchState)propertiesThatChanged[RoomPropertyKeys.matchStateKey])
            {
                case MatchState.InRoom:
                    state = MatchState.InRoom;
                    break;
                case MatchState.Warmup:
                    state = MatchState.Warmup;
                    break;
                case MatchState.PreRound:
                    state = MatchState.PreRound;
                    break;
                case MatchState.RoundPlaying:
                    state = MatchState.RoundPlaying;
                    break;
                case MatchState.RoundEnding:
                    state = MatchState.RoundEnding;
                    break;
                case MatchState.MatchEnding:
                    state = MatchState.MatchEnding;
                    break;
            }

            OnMatchStateUpdated?.Invoke(state);
        }

        if (propertiesThatChanged.ContainsKey(RoomPropertyKeys.roundsPlayed))
        {
            OnRoundsPlayedUpdated?.Invoke((int)propertiesThatChanged[RoomPropertyKeys.roundsPlayed]);
        }

        if (propertiesThatChanged.ContainsKey(RoomPropertyKeys.teamOneScore))
        {
            OnTeamOneScoreUpdated?.Invoke((int)propertiesThatChanged[RoomPropertyKeys.teamOneScore]);
        }

        if (propertiesThatChanged.ContainsKey(RoomPropertyKeys.teamTwoScore))
        {
            OnTeamTwoScoreUpdated?.Invoke((int)propertiesThatChanged[RoomPropertyKeys.teamTwoScore]);
        }
    }
}

public enum MatchState
{
    InRoom,
    Warmup,
    WarmupEnding,
    PreRound,
    RoundPlaying,
    RoundEnding,
    MatchEnding
}


