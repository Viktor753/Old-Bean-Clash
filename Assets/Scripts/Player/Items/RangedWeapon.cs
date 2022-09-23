using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RangedWeapon : Weapon
{
    public Transform gunBarrel;
    public GameObject onFireEffectPrefab;

    public float audioRange = 40f;
    public string[] onFireAudioKeys;

    public string weaponFireAnimTrigger = "weapon_ranged_fire";

    public override void Attack()
    {
        var fireOrigin = PlayerManager.instance.localSpawnedPlayer.GetComponentInChildren<PlayerCamera>().transform;

        if(Physics.Raycast(fireOrigin.position, fireOrigin.forward, out var hitInfo, range, attackMask))
        {
            if(hitInfo.transform.TryGetComponent<Health>(out var health))
            {
                int finalDamage = CalculateDamageDropOf(Vector3.Distance(transform.position, hitInfo.transform.position));
                health.Attack(finalDamage, PhotonNetwork.LocalPlayer, transform.position, itemKey);
            }
            else
            {
                pv.RPC("OnHitEnvironment", RpcTarget.All, hitInfo.point, hitInfo.normal);
            }
        }

        

        pv.RPC("OnFireEffects", RpcTarget.All);
    }

    [PunRPC]
    private void OnFireEffects()
    {
        if (pv.IsMine)
        {
            PlayerAudioManager.instance.PlaySpatialSoundAtPosition(onFireAudioKeys[Random.Range(0,onFireAudioKeys.Length)], gunBarrel.position, audioRange);
        }
        playerHandAnimator.SetTrigger(weaponFireAnimTrigger);
        var effect = Instantiate(onFireEffectPrefab, gunBarrel.position, gunBarrel.rotation, gunBarrel);
        Destroy(effect, 5f);
    }
}
