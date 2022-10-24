using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerManager : MonoBehaviour
{
    public static void SetCurrentAudioListener(AudioListener newListener)
    {
        foreach(var listener in FindObjectsOfType<AudioListener>())
        {
            listener.enabled = newListener == listener;
        }
    }
}
