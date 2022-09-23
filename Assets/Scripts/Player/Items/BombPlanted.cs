using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombPlanted : MonoBehaviour, DeleteOnRoundStart
{
    public PhotonView pv;
    public string itemKey;

    public AudioSource spatialSource;
    public AudioSource mainSource;
    public float maxBeepInterval;
    public float minBeepInterval;
    private float bombDuration = 0;
    private float currentBombTimer = 0;
    public float defuseTime = 10.0f;
    public float defuseReach = 3.0f;

    private float defuseTimer = 10.0f;
    private float currentIntervalTimer = 0;

    public int explosionDamage;
    public float explosionRadius;
    public AudioClip beep;
    public Light lightSource;

    public GameObject explosionPrefab;
    public AudioClip explosionSound;
    public AudioClip bombPlantedSound;
    public AudioClip bombDefusedSound;
    public AudioClip startDefusingSound;

    private bool planted = false;

    private bool mouseOver = false;
    private bool defuseInitFlag = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Planted();
        }
    }

    private void Planted()
    {
        BombAndDefuse.instance.OnBombPlanted();
        bombDuration = BombAndDefuse.instance.bombTimer;
        currentBombTimer = bombDuration;
        planted = true;
        pv.RPC("SyncBombPlanted", RpcTarget.All, planted);
    }

    private void Defused()
    {
        if (planted)
        {
            pv.RPC("SyncBombDefused", RpcTarget.All);
            BombAndDefuse.instance.OnBombDefused();
        }
    }

    private void Explode()
    {
        pv.RPC("SyncBombExplosion", RpcTarget.All);
    }

    private void Update()
    {
        if (planted)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                currentBombTimer -= Time.deltaTime;
                currentIntervalTimer += Time.deltaTime;
                if (currentIntervalTimer >= Interval(bombDuration, currentBombTimer))
                {
                    pv.RPC("SyncBeepSound", RpcTarget.All);
                    currentIntervalTimer = 0;
                    pv.RPC("SyncLightFlicker", RpcTarget.All);
                }

                if(currentBombTimer <= 0)
                {
                    Explode();
                }
            }


            var player = PlayerInventory.instance;

            if (player != null)
            {
                if (PauseMenuManager.paused == false && mouseOver && Input.GetKey(KeyCode.E) && (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] == 0 &&
                    Vector3.Distance(transform.position, player.GetComponentInChildren<PlayerCamera>().transform.position) < defuseReach)
                {

                    if (defuseInitFlag == false)
                    {
                        StartDefusing();
                    }

                    defuseTimer -= Time.deltaTime;
                    if (defuseTimer <= 0)
                    {
                        Defused();
                    }
                }
                else
                {
                    if (defuseInitFlag)
                    {
                        StopDefusing();
                    }
                    defuseTimer = defuseTime;
                }
            }
        }
    }

    private void StartDefusing()
    {
        PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>().freeze = true;
        ProgressBar.instance.StartProgressBar("Defusing", defuseTime, 1);
        defuseInitFlag = true;
        pv.RPC("PlayStartDefuseSound", RpcTarget.All);
    }

    private void StopDefusing()
    {
        PlayerManager.instance.localSpawnedPlayer.GetComponent<PlayerMovement>().freeze = false;
        ProgressBar.instance.EndProgress();
        defuseInitFlag = false;
    }

    private float Interval(float bombDuration, float secondsLeft)
    {
        float interval = (secondsLeft / bombDuration) * maxBeepInterval;
        return Mathf.Clamp(interval, minBeepInterval, maxBeepInterval);
    }

    private void FlickerLight()
    {
        lightSource.enabled = true;
        Invoke(nameof(EndLight), 0.1f);
    }

    private void EndLight()
    {
        lightSource.enabled = false;
    }

    private void OnMouseEnter()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
    }


    #region Syncing over network

    [PunRPC]
    private void SyncBombPlanted(bool planted)
    {
        this.planted = planted;
        mainSource.PlayOneShot(bombPlantedSound);
    }

    [PunRPC]
    private void SyncBeepSound()
    {
        spatialSource.PlayOneShot(beep);
    }
    
    [PunRPC]
    private void SyncLightFlicker()
    {
        Invoke(nameof(FlickerLight), 1f);
    }


    [PunRPC]
    private void SyncBombDefused()
    {
        planted = false;
        mainSource.PlayOneShot(bombDefusedSound);
        if (PhotonNetwork.IsMasterClient)
        {
            BombAndDefuse.instance.OnBombDefused();
        }
    }

    [PunRPC]
    private void SyncBombExplosion()
    {
        planted = false;
        Invoke(nameof(ExplosionEffect), 0.3f);
        spatialSource.spatialBlend = 0;
        spatialSource.PlayOneShot(explosionSound);

        if (PhotonNetwork.IsMasterClient)
        {
            BombAndDefuse.instance.OnBombExploded();

            var objs = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach(var obj in objs)
            {
                if(obj.TryGetComponent<Health>(out var health))
                {
                    health.Attack(explosionDamage, health.pv.Owner, transform.position, itemKey);
                }
            }
        }
    }

    [PunRPC]
    private void PlayStartDefuseSound()
    {
        spatialSource.PlayOneShot(startDefusingSound);
    }

    private void ExplosionEffect()
    {
        Instantiate(explosionPrefab, transform);
    }

    public void DestroyOnRoundStart()
    {
        PhotonNetwork.Destroy(pv);
    }

    #endregion
}
