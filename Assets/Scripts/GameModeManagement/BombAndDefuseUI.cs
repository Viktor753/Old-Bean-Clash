using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Photon.Pun;
using UnityEngine.UI;

public class BombAndDefuseUI : MonoBehaviour
{
    public BombAndDefuse bombAndDefuse;

    public bool displayMilliSecondsInTimer = false;
    public TextMeshProUGUI matchStateDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI roundsPlayedDisplay;
    public TextMeshProUGUI teamOneScoreDisplay;
    public TextMeshProUGUI teamTwoScoreDisplay;

    public TextMeshProUGUI teamOneAliveCountDisplay;
    public TextMeshProUGUI teamTwoAliveCountDisplay;
    public GameObject bombPlantedImg;

    public CanvasGroup preRoundGreyScreen;
    public float preRoundAlpha;

    private void OnEnable()
    {
        bombAndDefuse.OnMatchStateUpdated += OnMatchStateChanged;

        bombAndDefuse.OnRoundsPlayedUpdated += OnRoundsPlayedUpdated;
        bombAndDefuse.OnTeamOneScoreUpdated += OnTeamOneScoreUpdated;
        bombAndDefuse.OnTeamTwoScoreUpdated += OnTeamTwoScoreUpdated;
        bombAndDefuse.isBombPlanted += OnBombPlantUpdate;
        BombAndDefuse.OnPreRoundStart += OnPreRound;
        BombAndDefuse.OnRoundStarted += OnRoundStart;

        if (PhotonNetwork.IsMasterClient)
        {
            bombAndDefuse.OnTeamOneAliveCountUpdated += OnTeamOneAliveCountUpdated;
            bombAndDefuse.OnTeamTwoAliveCountUpdated += OnTeamTwoAliveCountUpdated;

            bombAndDefuse.OnTimerUpdated += OnTimerUpdated;
        }
    }

    private void OnDisable()
    {
        bombAndDefuse.OnMatchStateUpdated -= OnMatchStateChanged;

        bombAndDefuse.OnRoundsPlayedUpdated -= OnRoundsPlayedUpdated;
        bombAndDefuse.OnTeamOneScoreUpdated -= OnTeamOneScoreUpdated;
        bombAndDefuse.OnTeamTwoScoreUpdated -= OnTeamTwoScoreUpdated;
        bombAndDefuse.isBombPlanted -= OnBombPlantUpdate;
        BombAndDefuse.OnPreRoundStart -= OnPreRound;
        BombAndDefuse.OnRoundStarted -= OnRoundStart;

        if (PhotonNetwork.IsMasterClient)
        {
            bombAndDefuse.OnTeamOneAliveCountUpdated -= OnTeamOneAliveCountUpdated;
            bombAndDefuse.OnTeamTwoAliveCountUpdated -= OnTeamTwoAliveCountUpdated;

            bombAndDefuse.OnTimerUpdated -= OnTimerUpdated;
        }
    }

    private void OnPreRound()
    {
        preRoundGreyScreen.alpha = preRoundAlpha;
    }

    private void OnRoundStart()
    {
        preRoundGreyScreen.alpha = 0;
    }

    private void OnTimerUpdated(float timer)
    {
        bombAndDefuse.pv.RPC("SyncTimerWithClients", RpcTarget.All, timer);
    }

    private string GetTimerText(float timeInSeconds, bool includeMilliseconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(timeInSeconds);


        string hourText = "";
        if(t.Hours > 9)
        {
            hourText = t.Hours.ToString() + ":";
        }
        else
        {
            hourText = $"0{t.Hours}:";
        }

        string minuteText = "";
        if (t.Minutes > 9)
        {
            minuteText = t.Minutes.ToString();
        }
        else
        {
            minuteText = $"0{t.Minutes}";
        }

        string secondsText = "";
        if (t.Seconds > 9)
        {
            secondsText = t.Seconds.ToString();
        }
        else
        {
            secondsText = $"0{t.Seconds}";
        }

        string milliSecondsText = t.Milliseconds.ToString();

        if(t.Hours > 0)
        {
            if (includeMilliseconds)
            {
                return $"{hourText}{minuteText}:{secondsText}:{milliSecondsText}";
            }
            else
            {
                return $"{hourText}{minuteText}:{secondsText}";
            }
        }
        else
        {
            if (includeMilliseconds)
            {
                return $"{minuteText}:{secondsText}:{milliSecondsText}";
            }
            else
            {
                return $"{minuteText}:{secondsText}";
            }
        }
    }

    private void OnBombPlantUpdate(bool bombPlanted)
    {
        bombPlantedImg.SetActive(bombPlanted);
    }

    private void OnMatchStateChanged(MatchState state)
    {
        matchStateDisplay.text = state.ToString();
    }

    private void OnRoundsPlayedUpdated(int roundsPlayed)
    {
        roundsPlayedDisplay.text = $"{roundsPlayed}/{bombAndDefuse.maximumRounds}";
    }

    private void OnTeamOneScoreUpdated(int teamOneScore)
    {
        teamOneScoreDisplay.text = teamOneScore.ToString();
    }

    private void OnTeamTwoScoreUpdated(int teamTwoScore)
    {
        teamTwoScoreDisplay.text = teamTwoScore.ToString();
    }

    private void OnTeamOneAliveCountUpdated(int teamOneAliveCount)
    {
        bombAndDefuse.pv.RPC("SyncTeamOneAliveCountUI", RpcTarget.All, teamOneAliveCount);
    }

    private void OnTeamTwoAliveCountUpdated(int teamTwoAliveCount)
    {
        bombAndDefuse.pv.RPC("SyncTeamTwoAliveCountUI", RpcTarget.All, teamTwoAliveCount);
    }

    [PunRPC]
    private void SyncTimerWithClients(float timer)
    {
        timerDisplay.text = GetTimerText(timer, displayMilliSecondsInTimer);
    }

    [PunRPC]
    private void SyncTeamOneAliveCountUI(int teamOneAliveCount)
    {
        teamOneAliveCountDisplay.text = teamOneAliveCount.ToString();
    }

    [PunRPC]
    private void SyncTeamTwoAliveCountUI(int teamTwoAliveCount)
    {
        teamTwoAliveCountDisplay.text = teamTwoAliveCount.ToString();
    }
}
