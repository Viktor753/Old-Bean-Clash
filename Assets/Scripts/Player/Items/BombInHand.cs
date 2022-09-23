using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombInHand : Item
{
    public GameObject bombOnGroundPrefab;
    private bool planting = false;
    private float currentTimer = 0.0f;
    public float plantTime = 3.0f;

    public float audioRange = 15f;

    private bool initPlantFlag = false;
    public string startPlantingSoundKey;
    public LayerMask bombSpawnPosMask;

    private void Start()
    {
        if (pv.IsMine)
        {
            currentTimer = plantTime;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (BombAndDefuse.instance.state == MatchState.RoundPlaying && itemPulledOut && PauseMenuManager.paused == false && PlantSite.inSiteValue > 0)
            {
                Debug.Log("Allowed to plant = true");
                planting = Input.GetMouseButton(0) || Input.GetKey(KeyCode.E);
                if (planting)
                {
                    Debug.Log("Planting...");
                    if (initPlantFlag == false)
                    {
                        Debug.Log("Started planting...");
                        StartPlant();
                    }
                    currentTimer -= Time.deltaTime;
                    if (currentTimer <= 0)
                    {
                        Debug.Log("Planted bomb!");
                        pv.RPC("Plant", RpcTarget.AllViaServer);
                        PlayerInventory.instance.itemsInInventory.Remove(this);
                        PlayerInventory.instance.SpecialSlot = null;
                        PlayerInventory.instance.AutoSelectBestItem();
                    }
                }
                else
                {
                    if (initPlantFlag)
                    {
                        Debug.Log("Stopped planting!");
                        StopPlant();
                    }
                    currentTimer = plantTime;
                }
            }
            else
            {
                Debug.Log("Allowed to plant = false");
            }
        }
    }

    public override void OnDisable()
    {
        if (pv.IsMine)
        {
            base.OnDisable();
            if (initPlantFlag)
            {
                StopPlant();
            }
        }
    }

    private void StopPlant()
    {
        ProgressBar.instance.EndProgress();
        initPlantFlag = false;
        PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>().freeze = false;
    }

    private void StartPlant()
    {
        initPlantFlag = true;
        ProgressBar.instance.StartProgressBar("Planting...", plantTime, 1);
        PlayerAudioManager.instance.PlaySpatialSoundAtPosition(startPlantingSoundKey, transform.position, audioRange);
        PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>().freeze = true;
    }

    [PunRPC]
    private void Plant()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateRoomObject(bombOnGroundPrefab.name, GetBombPosition(), Quaternion.identity);
        }
    }

    

    private Vector3 GetBombPosition()
    {
        var rayOrigin = new Vector3(transform.root.position.x, transform.position.y, transform.root.position.z);
        var rayDir = Vector3.down;

        if (Physics.Raycast(rayOrigin, rayDir, out var hit, bombSpawnPosMask))
        {
            return hit.point;
        }
        else
        {
            return rayOrigin;
        }
    }
}
