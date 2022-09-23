using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Room room;
    public GameObject mainUI;
    public GameObject roomUI;

    public GameObject[] activeByDefault;
    public GameObject[] disabledByDefault;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        PauseMenuManager.AddPauseFactor(this);
        room.action_OnRoomJoined += OnRoomJoined;
        room.action_OnRoomLeft += OnRoomLeft;
    }

    private void OnDisable()
    {
        PauseMenuManager.RemovePauseFactor(this);
        room.action_OnRoomJoined -= OnRoomJoined;
        room.action_OnRoomLeft -= OnRoomLeft;
    }

    private void OnRoomJoined()
    {
        mainUI.SetActive(false);
        roomUI.SetActive(true);
    }

    private void OnRoomLeft()
    {
        mainUI.SetActive(true);
        roomUI.SetActive(false);

        ResetMainUI();
    }

    public void OnCreateRoomButton(TMP_InputField roomName)
    {
        room.CreateRoom(roomName.text);
    }

    public void OnJoinRoomButton(TMP_InputField roomName)
    {
        room.JoinRoom(roomName.text);
    }

    private void ResetMainUI()
    {
        foreach(var obj in activeByDefault)
        {
            obj.SetActive(true);
        }

        foreach(var obj in disabledByDefault)
        {
            obj.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadSceneAsync(SceneNames.settingsScene, LoadSceneMode.Additive);
    }

    public void OnQuitButton()
    {
#if !UNITY_EDITOR

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        Application.Quit();
#endif
    }
}
