using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class RoomUI : MonoBehaviourPunCallbacks
{
    public Room room;

    public TextMeshProUGUI roomName;
    public TextMeshProUGUI roomProperties;
    public TMP_Dropdown mapDropdown;
    public TMP_InputField roundsInput;
    public TMP_InputField warmupDurationInput;
    public TMP_InputField endwarmupDurationInput;
    public TMP_InputField preRoundDurationInput;
    public TMP_InputField roundDurationInput;
    public TMP_InputField endRoundDurationInput;
    public TMP_InputField matchEndDurationInput;
    public TMP_InputField bombTimerInput;
    public TMP_InputField respawnTimerInput;

    public List<string> propertyKeysToDisplay = new List<string>();

    public List<TextMeshProUGUI> playerNamePlates = new List<TextMeshProUGUI>();
    public GameObject[] masterClientShowcase;

    private void Start()
    {
        roundsInput.text = room.rounds.ToString();
        warmupDurationInput.text = room.warmupDuration.ToString();
        endwarmupDurationInput.text = room.endWarmupDuration.ToString();
        preRoundDurationInput.text = room.preRoundDuration.ToString();
        roundDurationInput.text = room.roundDuration.ToString();
        endRoundDurationInput.text = room.endRoundDuration.ToString();
        matchEndDurationInput.text = room.matchEndDuration.ToString();
        bombTimerInput.text = room.bombTimer.ToString();
        respawnTimerInput.text = room.respawnTimer.ToString();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        room.action_OnRoomJoined += OnRoomJoined;
        room.action_OnRoomLeft += OnRoomLeft;

        room.action_OnPropertiesUpdated += UpdateProperties;

        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(room.mapCodenames.ToList());
    }

    public override void OnDisable()
    {
        base.OnDisable();
        room.action_OnRoomJoined -= OnRoomJoined;
        room.action_OnRoomLeft -= OnRoomLeft;

        room.action_OnPropertiesUpdated -= UpdateProperties;
    }

    private void OnRoomJoined()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerListUI();

        if (PhotonNetwork.IsMasterClient == false)
        {
            //Master client already updates UI properties when creating room, no need to do it twice in a row
            UpdateProperties();
        }
    }

    private void OnRoomLeft()
    {
        roomName.text = "None";
        roomProperties.text = "None";
    }

    private void UpdateProperties()
    {
        string message = "";

        foreach (var property in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            if (propertyKeysToDisplay.Contains((string)property.Key))
            {
                if ((message == "") == false)
                {
                    message += " - ";
                }

                message += $"{property.Key} : {property.Value}";
            }
        }
        roomProperties.text = message;
    }

    public void OnMapDropDownChanged()
    {
        room.UpdateMapProperty(mapDropdown.value);
    }

    public void OnRoundsInputChanged()
    {
        if(int.TryParse(roundsInput.text, out var result))
        {
            if(result > 100)
            {
                result = 100;
            }
            else if(result <= 0)
            {
                result = 1;
            }
            room.UpdateRoundsProperty(result);
        }
    }

    public void OnWarmupDurationInputChanged()
    {
        if (int.TryParse(warmupDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateWarmupDurationProperty(result);
        }
    }

    public void OnEndWarmupDurationInputChanged()
    {
        if (int.TryParse(endwarmupDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateEndWarmupDurationProperty(result);
        }
    }

    public void OnPreRoundDurationInputChanged()
    {
        if (int.TryParse(preRoundDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdatePreRoundDurationProperty(result);
        }
    }

    public void OnRoundDurationInputChanged()
    {
        if (int.TryParse(roundDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateRoundDurationProperty(result);
        }
    }

    public void OnEndRoundDurationInputChanged()
    {
        if (int.TryParse(endRoundDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateEndRoundDurationProperty(result);
        }
    }

    public void OnMatchEndDurationInputChanged()
    {
        if (int.TryParse(matchEndDurationInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateMatchEndDurationProperty(result);
        }
    }

    public void OnBombTimerInputChanged()
    {
        if (int.TryParse(bombTimerInput.text, out var result))
        {
            if (result <= 0)
            {
                result = 1;
            }
            room.UpdateBombTimerDurationProperty(result);
        }
    }

    public void OnRespawnTimerInputChanged()
    {
        if (int.TryParse(respawnTimerInput.text, out var result))
        {
            if(result <= 0)
            {
                result = 1;
            }
            room.UpdateRespawnTimerDurationProperty(result);
        }
    }


    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdatePlayerListUI();
    }

    private void UpdatePlayerListUI()
    {
        int index = 0;
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            playerNamePlates[index].gameObject.SetActive(true);
            playerNamePlates[index].text = player.Value.NickName;
            playerNamePlates[index].color = player.Value.IsLocal ? Color.yellow : Color.white;
            masterClientShowcase[index].SetActive(player.Value.IsMasterClient);
            index++;
        }

        for (int i = 0; i < playerNamePlates.Count; i++) 
        {
            if (i >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                playerNamePlates[i].gameObject.SetActive(false);
                masterClientShowcase[i].gameObject.SetActive(false);
            }
        }
    }
}
