using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public PlayerScoreDisplay[] teamOneDisplays;
    public PlayerScoreDisplay[] teamTwoDisplays;
    public TextMeshProUGUI playersWaitingText;
    public GameObject scoreboardUI;

    public bool showScoreboard = false;

    public override void OnEnable()
    {
        base.OnEnable();
        PlayersInMatch.OnPlayerEnteredMatch += OnPlayerEnteredMatch;
        PlayersInMatch.OnPlayerLeftMatch += OnPlayerLeftMatch;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayersInMatch.OnPlayerEnteredMatch -= OnPlayerEnteredMatch;
        PlayersInMatch.OnPlayerLeftMatch -= OnPlayerLeftMatch;
    }

    private void Update()
    {
        showScoreboard = Input.GetKey(KeyCode.Tab);
        scoreboardUI.SetActive(showScoreboard);
    }

    private void OnPlayerEnteredMatch(Player player)
    {
        UpdateScoreboardUI();
    }

    private void OnPlayerLeftMatch(Player player)
    {
        UpdateScoreboardUI();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.teamIDKey) &&
            targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.killsKey) &&
            targetPlayer.CustomProperties.ContainsKey(PlayerPropertyKeys.deathsKey))
        {
            UpdateScoreboardUI();
        }
    }


    public void UpdateScoreboardUI()
    {
        scoreboardUI.SetActive(true);
        var players = PlayersInMatch.instance.players;

        int xIndex = 0;
        int yIndex = 0;
        int playersInNoTeam = 0;

        foreach(var display in teamOneDisplays)
        {
            display.actorNumber = -1;
        }
        foreach(var dispaly in teamTwoDisplays)
        {
            dispaly.actorNumber = -1;
        }

        foreach (var player in players)
        {
            if (player.CustomProperties.ContainsKey(PlayerPropertyKeys.teamIDKey) == false)
            {
                return;
            }
            var teamID = (int)player.CustomProperties[PlayerPropertyKeys.teamIDKey];
            if (teamID == 0)
            {
                teamOneDisplays[xIndex].gameObject.SetActive(true);
                teamOneDisplays[xIndex].canvasGroup.alpha = (bool)player.CustomProperties[PlayerPropertyKeys.isDeadKey] == true ? 0.4f : 1f;
                teamOneDisplays[xIndex].actorNumber = player.ActorNumber;
                teamOneDisplays[xIndex].nickName.text = player.NickName;
                teamOneDisplays[xIndex].nickName.color = player == PhotonNetwork.LocalPlayer ? Color.yellow : Color.white;
                teamOneDisplays[xIndex].killsText.text = "Kills : " + player.CustomProperties[PlayerPropertyKeys.killsKey].ToString();
                teamOneDisplays[xIndex].deathsText.text = "Deaths : " + player.CustomProperties[PlayerPropertyKeys.deathsKey].ToString();
                xIndex++;
            }
            else if(teamID == 1)
            {
                teamTwoDisplays[yIndex].gameObject.SetActive(true);
                teamTwoDisplays[yIndex].canvasGroup.alpha = (bool)player.CustomProperties[PlayerPropertyKeys.isDeadKey] == true ? 0.4f : 1f;
                teamTwoDisplays[yIndex].actorNumber = player.ActorNumber;
                teamTwoDisplays[yIndex].nickName.text = player.NickName;
                teamTwoDisplays[yIndex].nickName.color = player == PhotonNetwork.LocalPlayer ? Color.yellow : Color.white;
                teamTwoDisplays[yIndex].killsText.text = "Kills : " + player.CustomProperties[PlayerPropertyKeys.killsKey].ToString();
                teamTwoDisplays[yIndex].deathsText.text = "Deaths : " + player.CustomProperties[PlayerPropertyKeys.deathsKey].ToString();
                yIndex++;
            }
            else
            {
                playersInNoTeam++;
            }
        }

        for (int i = 0; i < teamOneDisplays.Length; i++)
        {
            if(teamOneDisplays[i].actorNumber == -1)
            {
                teamOneDisplays[i].gameObject.SetActive(false);
            }

            if(teamTwoDisplays[i].actorNumber == -1)
            {
                teamTwoDisplays[i].gameObject.SetActive(false);
            }
        }

        playersWaitingText.text = "Spectators : " + playersInNoTeam;

        scoreboardUI.SetActive(showScoreboard);
    }
}
