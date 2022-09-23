using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInteract : MonoBehaviour
{
    public PhotonView pv;
    public bool enablePlayerInteraction = true;
    public float interactRange = 5;
    public LayerMask interactMask = ~0;
    public KeyCode interactKey = KeyCode.E;
    public Transform playerCameraTransform;

    private string currentInteractDisplayMessage;

    public void OnTriggerEnter(Collider other)
    {
        if(pv.IsMine)
        {
            Debug.Log($"Player OnTriggerEnter, 'other' = {other.name}");
        }
    }

    private void Update()
    {
        if(pv.IsMine == false)
        {
            return;
        }

        if (enablePlayerInteraction)
        {
            Vector3 rayOrigin = playerCameraTransform.position;
            Vector3 rayDirection = playerCameraTransform.forward;
            if (Physics.Raycast(rayOrigin, rayDirection, out var hitInfo, interactRange, interactMask))
            {
                if (hitInfo.transform.TryGetComponent<IInteractable>(out var interactable))
                {
                    if (interactable.canInteract)
                    {
                        currentInteractDisplayMessage = $"[{interactKey}]";
                        if (Input.GetKeyDown(interactKey))
                        {
                            interactable.Interact();
                        }
                    }
                }
                else
                {
                    currentInteractDisplayMessage = string.Empty;
                }
            }
            else
            {
                currentInteractDisplayMessage = string.Empty;
            }
        }
        else
        {
            currentInteractDisplayMessage = string.Empty;
        }

        PlayerManager.SendInteractTextMessage?.Invoke(currentInteractDisplayMessage);
    }
}
