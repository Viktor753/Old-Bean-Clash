using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    public PhotonView photonView;
    public PlayerMovement movement;
    public PlayerCamera playerCam;
    [HideInInspector] public bool IsOwner;
    public AudioSource spatialAudioSource;

    private void Awake()
    {
        IsOwner = photonView.IsMine;
    }

    public static bool AllowMovementInput()
    {
        return PauseMenuManager.pauseFactors.Count == 0;
    }

    public static bool AllowCameraInput()
    {
        return PauseMenuManager.pauseFactors.Count == 0;
    }
}
