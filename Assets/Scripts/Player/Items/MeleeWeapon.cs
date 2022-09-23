using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MeleeWeapon : Weapon
{
    public float rangeToSphereCast;
    public float attackRadius;
    public string attackAnimTrigger = "melee_swing";
    
    public override void Attack()
    {
        pv.RPC("PlayAnim", RpcTarget.All, attackAnimTrigger);
        //Spawn a spherecast in front of knife
        var playerCameraTransform = PlayerManager.instance.localSpawnedPlayer.GetComponentInChildren<PlayerCamera>().transform;
        if(Physics.SphereCast(playerCameraTransform.position, rangeToSphereCast, playerCameraTransform.forward, out var hit, attackRadius))
        {
            if(hit.transform.TryGetComponent<Health>(out var healthComponent))
            {
                healthComponent.Attack(baseDamage, PhotonNetwork.LocalPlayer, transform.position, itemKey);
            }
            else
            {
                pv.RPC("OnHitEnvironment", RpcTarget.All, hit.point, hit.normal);
            }
        }
    }

    [PunRPC]
    private void PlayAnim(string trigger)
    {
        playerHandAnimator.SetTrigger(trigger);
    }
}
