using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSettings : MonoBehaviour
{
    public SettingsManager[] settings;

    private void OnEnable()
    {
        LoadAllSettings();
        PauseMenuManager.AddPauseFactor(this);
    }

    private void OnDisable()
    {
        PauseMenuManager.RemovePauseFactor(this);
    }

    public void SaveAndExit()
    {
        SaveAllSettings();
        SceneManager.UnloadSceneAsync(SceneNames.settingsScene);
    }

    private void SaveAllSettings()
    {
        foreach (var setting in settings)
        {
            setting.Save();
        }
    }

    private void LoadAllSettings()
    {
        foreach (var setting in settings)
        {
            setting.Load();
        }
    }
}
