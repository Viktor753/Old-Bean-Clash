using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerAudioManager : MonoBehaviour
{
    public PhotonView pv;
    public static PlayerAudioManager instance;
    public PlayerAudioClips[] audioClips;
    public AudioSource spatialSource;
    [SerializeField] private AudioSource spatialSourcePrefab;

    #region ClientSide
    private void Awake()
    {
        if (pv.IsMine)
        {
            instance = this;
        }    
    }

    public void PlaySpatialSoundOnPlayer(string key, float audioRange)
    {
        pv.RPC("PlayClipOnPlayer", RpcTarget.All, key, audioRange);
    }

    public void PlaySpatialSoundAtPosition(string key, Vector3 pos, float audioRange)
    {
        pv.RPC("PlayClipAtPos", RpcTarget.All, key, pos, audioRange);
    }

    private AudioClip GetClipFromKey(string key)
    {
        foreach(var audio in audioClips)
        {
            if(audio.key == key)
            {
                return audio.clip;
            }
        }

        Debug.LogError($"Could not find audioclip from key {key}");
        return null;
    }

    [Serializable]
    public class PlayerAudioClips
    {
        public AudioClip clip;
        public string key;
    }

    #endregion

    #region Networked

    [PunRPC]
    private void PlayClipAtPos(string key, Vector3 pos, float audioRange)
    {
        var clip = GetClipFromKey(key);

        if (clip != null)
        {
            var spawnedSource = Instantiate(spatialSourcePrefab, pos, Quaternion.identity);
            spawnedSource.maxDistance = audioRange;
            spawnedSource.clip = clip;
            spawnedSource.Play();
            Destroy(spawnedSource.gameObject, clip.length);
        }
    }

    [PunRPC]
    private void PlayClipOnPlayer(string key, float audioRange)
    {
        var clip = GetClipFromKey(key);

        if (clip != null)
        {
            spatialSource.maxDistance = audioRange;
            spatialSource.PlayOneShot(clip);
        }
    }

    #endregion
}
