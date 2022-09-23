using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviourPunCallbacks
{
    public static bool paused = false;
    public GameObject pauseMenuCanvas;
    private bool leaving = false;
    public static List<object> pauseFactors = new List<object>();

    private void Awake()
    {
        pauseFactors.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu(!paused);
        }
    }

    public static void AddPauseFactor(object factor)
    {
        if(pauseFactors.Contains(factor) == false)
        {
            pauseFactors.Add(factor);
        }

        paused = pauseFactors.Count <= 0 ? false : true;

        CheckPauseFactors();
    }

    public static void RemovePauseFactor(object factor)
    {
        if (pauseFactors.Contains(factor))
        {
            pauseFactors.Remove(factor);
        }

        paused = pauseFactors.Count <= 0 ? false : true;

        CheckPauseFactors();
    } 

    private static void CheckPauseFactors()
    {
        if (pauseFactors.Count > 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadSceneAsync(SceneNames.settingsScene, LoadSceneMode.Additive);
    }

    public void ToggleMenu(bool toggle)
    {
        pauseMenuCanvas.SetActive(toggle);
        if (toggle)
        {
            AddPauseFactor(this);
        }
        else
        {
            RemovePauseFactor(this);
        }
    }

    public void LeaveRoom()
    {
        if (leaving == false)
        {
            leaving = true;
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(SceneNames.mainMenu);
    }

    public void CopyRoomName()
    {
        Room.CopyRoomName();
    }
}
