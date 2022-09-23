using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;

public class GameChat : MonoBehaviour
{
    public PhotonView pv;
    public ChatMessageContainer chatMessagePrefab;
    public GameObject chatObject;
    public Transform chatLayoutPanel;
    public TMP_InputField textInput;
    public float messageLifeTime = 10.0f;

    public Action<string, Player> OnMessageSent;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (textInput.gameObject.activeSelf)
            {
                if(textInput.text.Length > 0)
                {
                    SendMessageClient();
                }
                CloseChatInput();
            }
            else
            {
                OpenChatInput();
            }
        }
    }

    private void OpenChatInput()
    {
        PauseMenuManager.AddPauseFactor(this);
        textInput.gameObject.SetActive(true);
        textInput.ActivateInputField();
    }

    private void CloseChatInput()
    {
        PauseMenuManager.RemovePauseFactor(this);
        textInput.gameObject.SetActive(false);
    }

    private void ToggleChatVisible(bool visible)
    {
        chatObject.SetActive(visible);
    }

    private void SendMessageClient()
    {
        var message = textInput.text;
        var sender = PhotonNetwork.LocalPlayer;
        OnMessageSent?.Invoke(message, sender);
        if (message[0] != '!')
        {
            pv.RPC("SendMessageServer", RpcTarget.All, sender, message);
        }
        textInput.text = string.Empty;
    }


    [PunRPC]
    public void SendMessageServer(Player sender, string message)
    {
        WriteMessage($"{sender.NickName} : {message}");
    }

    public void WriteMessage(string text)
    {
        var newMessage = Instantiate(chatMessagePrefab, chatLayoutPanel);
        newMessage.messageText.text = text;
        Destroy(newMessage.gameObject, messageLifeTime);
    }
}
