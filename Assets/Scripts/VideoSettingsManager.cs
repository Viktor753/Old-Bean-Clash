using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VideoSettingsManager : SettingsManager
{
    public PostProcessProfile postProcessingProfile;

    public CustomToggle ambientOcclusion;
    public CustomToggle bloom;
    public CustomToggle colorGrading;
    public CustomToggle autoExposure;
    public CustomToggle vignette;
    public CustomToggle motionBlur;

    public static string ambientOcclusionKey = "ambientOcclusion";
    public static string bloomKey = "bloom";
    public static string colorGradingKey = "colorGrading";
    public static string autoExposureKey = "autoExposure";
    public static string vignetteKey = "vignette";
    public static string motionBlurKey = "motionBlur";

    public override void Save()
    {
        PlayerPrefs.SetInt(ambientOcclusionKey, ambientOcclusion.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<AmbientOcclusion>(out var _ambientOcclusion))
            _ambientOcclusion.active = ambientOcclusion.On;

        PlayerPrefs.SetInt(bloomKey, bloom.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<Bloom>(out var _bloom))
            _bloom.active = bloom.On;

        PlayerPrefs.SetInt(colorGradingKey, colorGrading.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<ColorGrading>(out var _colorGrading))
            _colorGrading.active = colorGrading.On;

        PlayerPrefs.SetInt(autoExposureKey, autoExposure.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<AutoExposure>(out var _autoExposure))
            _autoExposure.active = autoExposure.On;

        PlayerPrefs.SetInt(vignetteKey, vignette.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<Vignette>(out var _vignette))
            _vignette.active = vignette.On;

        PlayerPrefs.SetInt(motionBlurKey, motionBlur.On == true ? 1 : 0);
        if (postProcessingProfile.TryGetSettings<MotionBlur>(out var _motionBlur))
            _motionBlur.active = motionBlur.On;
    }

    public override void Load()
    {
        ambientOcclusion.On = PlayerPrefs.GetInt(ambientOcclusionKey) == 1;
        ambientOcclusion.UpdateUI();

        bloom.On = PlayerPrefs.GetInt(bloomKey) == 1;
        bloom.UpdateUI();

        colorGrading.On = PlayerPrefs.GetInt(colorGradingKey) == 1;
        colorGrading.UpdateUI();

        autoExposure.On = PlayerPrefs.GetInt(autoExposureKey) == 1;
        autoExposure.UpdateUI();

        vignette.On = PlayerPrefs.GetInt(vignetteKey) == 1;
        vignette.UpdateUI();

        motionBlur.On = PlayerPrefs.GetInt(motionBlurKey) == 1;
        motionBlur.UpdateUI();
    }
}
