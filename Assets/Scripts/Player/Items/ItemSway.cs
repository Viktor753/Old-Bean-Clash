using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemSway : MonoBehaviour
{
    public PhotonView pv;
    public float swayAmount;
    public float maxSwayAmount;
    public float smoothSwayAmount;

    private Vector3 initPos;
    private void Start()
    {
        if (pv.IsMine)
        {
            initPos = transform.localPosition;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (PauseMenuManager.paused == false)
            {
                var x = -Input.GetAxis("Mouse X") * swayAmount;
                var y = -Input.GetAxis("Mouse Y") * swayAmount;

                x = Mathf.Clamp(x, -maxSwayAmount, maxSwayAmount);
                y = Mathf.Clamp(y, -maxSwayAmount, maxSwayAmount);

                var desiredPos = new Vector3(x, y, 0);
                transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos + initPos, Time.deltaTime * smoothSwayAmount);
            }
        }
    }

}
