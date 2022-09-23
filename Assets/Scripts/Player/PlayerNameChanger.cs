using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Linq;

public class PlayerNameChanger : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public string nick;
    public int charLimit = 20;
    public Room room;

    private void OnEnable()
    {
        room.action_OnRoomJoined += OnJoinedRoom;
        if (PlayerPrefs.HasKey("SavedNick"))
        {
            nick = PlayerPrefs.GetString("SavedNick");
            SetNick(nick);
        }
        else
        {
            SetNick("Player#" + Random.Range(1000, 9999));
        }
    }

    private void OnDisable()
    {
        room.action_OnRoomJoined -= OnJoinedRoom;
        SetNick(nick);
    }

    private void OnJoinedRoom()
    {
        SetNick(nick);
    }

    public void OnInputChanged()
    {
        SetNick(nameInputField.text);
    }

    private void SetNick(string submittedNickName)
    {
        string nickToSet = submittedNickName;
        if (ValidNameInput(submittedNickName))
        {
            if(submittedNickName.Length > charLimit)
            {
                nickToSet = string.Concat(submittedNickName.Reverse().Skip(submittedNickName.Length - charLimit).Reverse());
                room.PrintStatus("Name char limit is " + charLimit + "!");
            }
        }
        else
        {
            //Set default name
            nickToSet = "Player#" + Random.Range(1000, 9999);
        }

        nick = nickToSet;
        nameInputField.text = nickToSet;
        PlayerPrefs.SetString("SavedNick", nick);
        PhotonNetwork.LocalPlayer.NickName = nick;
    }

    private bool ValidNameInput(string nameToCheck)
    {
        return nameToCheck != string.Empty;
    }
}
