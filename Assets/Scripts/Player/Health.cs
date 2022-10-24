using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;

public class Health : MonoBehaviour
{
    public PhotonView pv;
    public static Health localInstance;

    public int baseHealth = 100;
    public int maxHealth = 100;


    public int baseArmor = 0;
    public int maxArmor = 100;
    [SerializeField] private int currentHealth = 0;
    [SerializeField] private int currentArmor = 0;

    public Action<int,int> OnHealthChanged;
    public Action<int, int> OnArmorChanged;
    public GameObject OnHitEffectPrefab;
    public Vector3 onHitEffectPositionOffset;

    private void Awake()
    {
        if (pv.IsMine)
        {
            localInstance = this;
            SetHealth(baseHealth);
        }
        else
        {
            pv.RPC("FetchHealthValueFromPlayer", RpcTarget.All);
        }
    }

    private void OnEnable()
    {
        if (pv.IsMine)
        {
            BombAndDefuse.OnPreRoundStart += RefillHealth;
        }
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            BombAndDefuse.OnPreRoundStart -= RefillHealth;
        }
    }

    //Called by external clients
    public void Attack(int damage, Player damageSource, Vector3 attackOrigin, string weaponUsedKey)
    {
        pv.RPC("ReceiveAttackFromOther", RpcTarget.All, damage, damageSource, attackOrigin, weaponUsedKey);
    }

    [PunRPC]
    private void ReceiveAttackFromOther(int damage, Player damageSourceOwner, Vector3 attackOrigin, string weaponUsedKey)
    {
        if (pv.IsMine)
        {
            double reducedDamage = damage * Math.Pow(0.99, currentArmor);
            damage = (int)reducedDamage;

            //Show damage indicator on screen to determine which direction enemy is

            if(currentHealth-damage <= 0)
            {

                var localTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey];
                var damageSourceTeam = (int)damageSourceOwner.CustomProperties[PlayerPropertyKeys.teamIDKey];
                var teamKill = localTeam == damageSourceTeam;
                pv.RPC("OnKilledByPlayer", RpcTarget.All, damageSourceOwner, teamKill, weaponUsedKey);
            }
            SetHealth(currentHealth - damage);
        }

        PlayOnHitEffect();
    }

    [PunRPC]
    private void OnKilledByPlayer(Player player, bool sameTeam, string weaponUsedKey)
    {
        if(player == PhotonNetwork.LocalPlayer)
        {
            KillFeed.instance.OnKill(player, pv.Owner, weaponUsedKey);

            if (sameTeam)
            {
                PlayerStats.instance.kills--;
            }
            else
            {
                PlayerStats.instance.kills++;
            }
            PlayerStats.instance.UpdateKills();
        }
    }

    public void AddArmor(int armorAmountToAdd)
    {
        int oldArmor = currentArmor;
        currentArmor += armorAmountToAdd;
        currentArmor = Mathf.Clamp(currentArmor, 0, maxArmor);
        OnArmorChanged?.Invoke(oldArmor, currentArmor);
    }

    //Called by owner of this object
    public void SetHealth(int newValue)
    {
        if(currentHealth == newValue)
        {
            return;
        }

        OnHealthChanged?.Invoke(currentHealth, newValue);
        currentHealth = newValue;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(currentHealth <= 0)
        {
            PlayerManager.instance.DeSpawnLocalPlayer();
        }
        else
        {
            pv.RPC("SyncHealth", RpcTarget.Others, currentHealth);
        }
    }

    [PunRPC]
    private void SyncHealth(int healthSentFromClient)
    {
        currentHealth = healthSentFromClient;
    }

    //This is called from other clients (not owner of this object)
    [PunRPC]
    private void FetchHealthValueFromPlayer()
    {
        if (pv.IsMine)
        {
            pv.RPC("SyncHealth", RpcTarget.Others, currentHealth);
        }
    }

    private void PlayOnHitEffect()
    {
        var onHit = Instantiate(OnHitEffectPrefab, transform.position + onHitEffectPositionOffset, Quaternion.identity);
        Destroy(onHit, 3f);
    }

    private void RefillHealth()
    {
        SetHealth(baseHealth);
    }
}
