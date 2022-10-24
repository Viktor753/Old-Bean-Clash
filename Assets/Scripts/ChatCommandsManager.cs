using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Realtime;
using Photon.Pun;

public class ChatCommandsManager : MonoBehaviour
{
    public GameChat chat;
    public Command[] Commands;
    public Command[] HiddenCommands;

    private void Start()
    {
        chat.WriteMessage("Type !help for commands");
    }

    private void OnEnable()
    {
        chat.OnMessageSent += OnMessageSent;
    }

    private void OnDisable()
    {
        chat.OnMessageSent -= OnMessageSent;
    }

    public void OnMessageSent(string message, Player sender)
    {
        var command = GetCommand(message);
        if(command != null)
        {
            if (command.adminCommand)
            {
                if (sender.IsMasterClient)
                {
                    command.action?.Invoke();
                }
            }
            else
            {
                command.action?.Invoke();
            }
        }
    }

    public void PrintChatCommands()
    {
        var message = "System : \n";
        foreach(var command in Commands)
        {
            bool includeCommand = (command.adminCommand && PhotonNetwork.LocalPlayer.IsMasterClient) || command.adminCommand == false;
            if (includeCommand)
            {
                message += command.commandKey + "\n";
            }
        }
        chat.WriteMessage(message);
    }

    public void SetHighMoveSpeed()
    {
        PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>().moveSpeed = 10;
    }

    public void SetDefaultMoveSpeed()
    {
        var player = PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>();
        player.moveSpeed = player.defaultSpeed;
    }

    private Command GetCommand(string key)
    {
        foreach(var command in Commands)
        {
            if(command.commandKey == key)
            {
                return command;
            }
        }

        foreach (var command in HiddenCommands)
        {
            if (command.commandKey == key)
            {
                return command;
            }
        }
        return null;
    }

    [Serializable]
    public class Command
    {
        public string commandKey;
        public bool adminCommand = true;
        public UnityEvent action;
    }
}
