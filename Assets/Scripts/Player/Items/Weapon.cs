using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Weapon : Item
{
    [Space]
    public WeaponAttackType attackType;
    public int baseDamage;
    public bool hasDamageFallOf = false;
    public DamageFallOf damageFallOfStats;
    public float range = float.MaxValue;
    public bool usesAmmo = true;
    public int baseAmmo;
    [HideInInspector] public int ammo;
    private bool reloading = false;
    public float reloadTime = 1.0f;
    private Coroutine reloadCoroutine;

    [Header("Attacks per second")]
    public float fireRate;
    private float nextTimeToFire = 0;

    public LayerMask attackMask;

    public string onHitEnvironmentAudioKey;
    public float onHitEnvironmentAudioRange;
    public GameObject[] onHitEnvironmentDecals;

    public static Action<int, int> OnAmmoUpdated;
    

    [System.Serializable]
    public class DamageFallOf
    {
        public float[] ranges;
        public int[] newDamageValue;
    }

    private void Start()
    {
        if (pv.IsMine)
        {
            RefillAmmo();
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();

        if (pv.IsMine)
        {
            BombAndDefuse.OnPreRoundStart += OnPreRoundStarted;
        }
    }

    public override void OnDeSelected()
    {
        base.OnDeSelected();

        if (pv.IsMine)
        {
            BombAndDefuse.OnPreRoundStart -= OnPreRoundStarted;

            if (reloading)
            {
                ProgressBar.instance.EndProgress();
                StopCoroutine(reloadCoroutine);
                reloading = false;
            }
        }
    }

    private void OnPreRoundStarted()
    {
        RefillAmmo();
    }

    public override void OnDropped()
    {
        PlayerInventory.instance.SyncWeaponStats(this, ammo);
    }

    public virtual void Update()
    {
        if (pv.IsMine)
        {
            if (BombAndDefuse.canUseItems && itemPulledOut && reloading == false && PauseMenuManager.paused == false)
            {
                if(ammo < baseAmmo && Input.GetKeyDown(KeyCode.R) && usesAmmo)
                {
                    Reload();
                    return;
                }

                var attackInput = attackType == WeaponAttackType.SemiAuto ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);
                if (attackInput && Time.time >= nextTimeToFire)
                {
                    if (usesAmmo)
                    {
                        if (ammo > 0)
                        {
                            nextTimeToFire = Time.time + (1 / fireRate);
                            Attack();
                            ammo--;
                            OnAmmoUpdated?.Invoke(ammo, baseAmmo);

                            if(ammo <= 0)
                            {
                                Reload();
                            }
                        }
                    }
                    else
                    {
                        nextTimeToFire = Time.time + (1 / fireRate);
                        Attack();
                    }
                }
            }
        }
    }

    public void RefillAmmo()
    {
        ammo = baseAmmo;
        OnAmmoUpdated?.Invoke(ammo, baseAmmo);
    }

    private void Reload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }

        reloadCoroutine = StartCoroutine(ReloadEnumerator());
    }

    private IEnumerator ReloadEnumerator()
    {
        ProgressBar.instance.StartProgressBar("Reloading...", reloadTime, 0);
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        reloading = false;
        RefillAmmo();
    }

    public virtual void Attack()
    {

    }

    [PunRPC]
    public void OnHitEnvironment(Vector3 point, Vector3 normalDirection)
    {
        if (pv.IsMine)
        {
            PlayerAudioManager.instance.PlaySpatialSoundAtPosition(onHitEnvironmentAudioKey, point, onHitEnvironmentAudioRange);
        }

        var decal = Instantiate(onHitEnvironmentDecals[UnityEngine.Random.Range(0,onHitEnvironmentDecals.Length)], point + normalDirection * 0.01f, Quaternion.LookRotation(normalDirection));
        Destroy(decal.gameObject, 10);
    }


    public int CalculateDamageDropOf(float distanceToTarget)
    {
        int damageToReturn = baseDamage;
        if (hasDamageFallOf)
        {
            for (int i = 0; i < damageFallOfStats.ranges.Length; i++)
            {
                if (distanceToTarget >= damageFallOfStats.ranges[i])
                {
                    damageToReturn = damageFallOfStats.newDamageValue[i];
                }
            }
        }

        return damageToReturn;
    }

    public enum WeaponAttackType
    {
        SemiAuto,
        Automatic
    }
}
