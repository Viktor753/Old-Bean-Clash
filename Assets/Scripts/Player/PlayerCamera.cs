using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    public Camera cam;
    public GameObject miniMapCamera;

    public PhotonView photonView;
    public Transform playerParent;
    public float minYRotation;
    public float maxYRotation;

    private float xRotation;

    public GameObject[] objsToHide;


    private void OnEnable()
    {
        if (photonView.IsMine)
        {
            miniMapCamera.SetActive(true);
            foreach(var obj in objsToHide)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            miniMapCamera.SetActive(false);
            if(TryGetComponent<AudioListener>(out var listener))
            {
                listener.enabled = false;
            }
            cam.enabled = false;
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (PlayerController.AllowCameraInput())
            {
                float sens = ControlSettingsManager.sensitivity;

                var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
                var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minYRotation, maxYRotation);

                playerParent.Rotate(Vector3.up * mouseX);
                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
    }
}
