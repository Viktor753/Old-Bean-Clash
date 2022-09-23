using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class ThrownGrenade : MonoBehaviour, DeleteOnRoundStart
{
    public PhotonView pv;
    public float initalForwardForce;
    public Rigidbody rb;

    [Space]

    public GameObject objModel;
    public bool hideMeshOnActivation = false;
    private bool activated = false;
    public float timeBeforeActivation = 5f;
    public float timeAddedOnCollision = 0f;
    public int maximumBounces = 1;
    private int currentBounceCount = 0;
    private float currentActivationTimer = 0f;

    public float timeActivated = 10f;
    public GameObject OnActivateEffect;
    public AudioClip onActivationClip;
    public AudioClip collisionSoundEffect;
    public AudioSource source;

    public bool freezeAllOnActivation = false;

    public Action OnActivated;
    

    private void Start()
    {
        if (pv.IsMine)
        {
            currentActivationTimer = timeBeforeActivation;
            ApplyForce(initalForwardForce, transform.forward);
        }
        else
        {
            rb.isKinematic = true;
        }
    }

    public virtual void Update()
    {
        if (pv.IsMine && activated == false)
        {
            currentActivationTimer -= Time.deltaTime;
            if(currentActivationTimer <= 0)
            {
                activated = true;
                pv.RPC("Activate", RpcTarget.All);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (pv.IsMine)
        {
            if (!activated)
            {
                if (currentBounceCount < maximumBounces)
                {
                    currentBounceCount++;
                    currentActivationTimer += timeAddedOnCollision;
                }
            }
        }

        source.PlayOneShot(collisionSoundEffect);
    }


    [PunRPC]
    public void Activate()
    {
        if (pv.IsMine)
        {
            OnActivated?.Invoke();
        }

        source.PlayOneShot(onActivationClip);

        activated = true;
        objModel.SetActive(!hideMeshOnActivation);
        OnActivateEffect.SetActive(true);
        if (freezeAllOnActivation)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(DestroyOnNetwork), timeActivated);
        }
    }

    private void DestroyOnNetwork()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    private void ApplyForce(float force, Vector3 direction)
    {
        rb.AddForce(force * direction);
    }

    public void DestroyOnRoundStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
