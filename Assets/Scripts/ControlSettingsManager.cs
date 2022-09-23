using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlSettingsManager : SettingsManager
{
    public static string sensitivityKey = "sensitivity";
    public static float sensitivity = 100f;
    public Slider sensSlider;
    public TextMeshProUGUI sensValueDisplay;

    public override void Load()
    {
        if (PlayerPrefs.HasKey(sensitivityKey))
        {
            sensitivity = PlayerPrefs.GetFloat(sensitivityKey);
        }
        sensSlider.value = GetDisplaySensitvity();
        sensValueDisplay.text = GetDisplaySensitvity().ToString("F1");
    }

    public override void Save()
    {
        PlayerPrefs.SetFloat(sensitivityKey, sensitivity);
    }

    public void OnSensitivityValueChanged()
    {
        sensValueDisplay.text = sensSlider.value.ToString("F1");
        sensitivity = sensSlider.value * 100;
    }

    public float GetDisplaySensitvity()
    {
        return sensitivity / 100f;
    }
}
