using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameSetup : MonoBehaviour
{
    public PostProcessProfile postProcessingProfile;

    private void Start()
    {

        #region video settings
        if (PlayerPrefs.HasKey(VideoSettingsManager.ambientOcclusionKey))
        {
            if (postProcessingProfile.TryGetSettings<AmbientOcclusion>(out var _ambientOcclusion))
                _ambientOcclusion.active = PlayerPrefs.GetInt(VideoSettingsManager.ambientOcclusionKey) == 1;
        }

        if (PlayerPrefs.HasKey(VideoSettingsManager.bloomKey))
        {
            if (postProcessingProfile.TryGetSettings<Bloom>(out var _bloom))
                _bloom.active = PlayerPrefs.GetInt(VideoSettingsManager.bloomKey) == 1;
        }

        if (PlayerPrefs.HasKey(VideoSettingsManager.colorGradingKey))
        {
            if (postProcessingProfile.TryGetSettings<ColorGrading>(out var _colorGrading))
                _colorGrading.active = PlayerPrefs.GetInt(VideoSettingsManager.colorGradingKey) == 1;
        }

        if (PlayerPrefs.HasKey(VideoSettingsManager.autoExposureKey))
        {
            if (postProcessingProfile.TryGetSettings<AutoExposure>(out var _autoExposure))
                _autoExposure.active = PlayerPrefs.GetInt(VideoSettingsManager.autoExposureKey) == 1;
        }

        if (PlayerPrefs.HasKey(VideoSettingsManager.vignetteKey))
        {
            if (postProcessingProfile.TryGetSettings<Vignette>(out var _vignette))
                _vignette.active = PlayerPrefs.GetInt(VideoSettingsManager.vignetteKey) == 1;
        }


        if (PlayerPrefs.HasKey(VideoSettingsManager.motionBlurKey))
        {
            if (postProcessingProfile.TryGetSettings<MotionBlur>(out var _motionBlur))
                _motionBlur.active = PlayerPrefs.GetInt(VideoSettingsManager.motionBlurKey) == 1;
        }

        #endregion

        #region control settings

        if (PlayerPrefs.HasKey(ControlSettingsManager.sensitivityKey))
        {
            ControlSettingsManager.sensitivity = PlayerPrefs.GetFloat(ControlSettingsManager.sensitivityKey);
        }
        #endregion
    }
}
