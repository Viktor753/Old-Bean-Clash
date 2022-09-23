using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameAudioManager : MonoBehaviourPunCallbacks
{
    public AudioListener listener;
    public AudioSource networkSource;

    public AudioClip playerJoinedSound;
    public AudioClip playerLeftSound;

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerManager.OnLocalPlayerControllerSpawned += OnLocalPlayerSpawned;
        PlayerManager.OnLocalPlayerControllerDeSpawned += OnLocalPlayerDeSpawned;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerManager.OnLocalPlayerControllerSpawned -= OnLocalPlayerSpawned;
        PlayerManager.OnLocalPlayerControllerDeSpawned -= OnLocalPlayerDeSpawned;
    }


    private void OnLocalPlayerSpawned(PlayerController controller)
    {
        listener.enabled = false;
    }

    private void OnLocalPlayerDeSpawned()
    {
        listener.enabled = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        networkSource.PlayOneShot(playerJoinedSound);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        networkSource.PlayOneShot(playerLeftSound);
    }
}
