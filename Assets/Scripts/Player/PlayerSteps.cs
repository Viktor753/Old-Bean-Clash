using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSteps : MonoBehaviour
{
    public PhotonView pv;
    public PlayerController controller;
    public float timeBetweenSteps;
    private float currentTimer = 0.0f;
    private bool grounded = false;
    private bool walking = false;
    private bool isMoving = false;
    public float stepAudioRange = 15f;

    public string[] stepKeys;

    private void Update()
    {
        if (pv.IsMine)
        {
            grounded = controller.movement.isGrounded;
            walking = controller.movement.walking;
            isMoving = controller.movement.controller.velocity.magnitude > 0;

            if (grounded && isMoving && !walking)
            {
                currentTimer += Time.deltaTime;
                if (currentTimer >= timeBetweenSteps)
                {
                    currentTimer = 0.0f;



                    PlayStep(stepKeys[UnityEngine.Random.Range(0, stepKeys.Length)]);
                }
            }
        }
    }

    private void PlayStep(string audioKey)
    {
        PlayerAudioManager.instance.PlaySpatialSoundOnPlayer(audioKey, stepAudioRange);
    }
}
