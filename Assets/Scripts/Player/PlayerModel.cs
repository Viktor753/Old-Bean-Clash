using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerModel : MonoBehaviour
{
    public PhotonView pv;
    public Transform cam;

    public GameObject playerModel;
    public GameObject playerEyes;

    public Material defaultMat;
    public Material teamOneMat;
    public Material teamTwoMat;

    private void Start()
    {
        var teamID = (int)pv.Owner.CustomProperties[PlayerPropertyKeys.teamIDKey];

        playerEyes.transform.SetParent(cam);
        switch (teamID)
        {
            case -1:
                SetupMaterial(defaultMat);
                break;
            case 0:
                SetupMaterial(teamOneMat);
                break;
            case 1:
                SetupMaterial(teamTwoMat);
                break;
        }

        if (pv.IsMine)
        {
            playerModel.gameObject.SetActive(false);
            playerEyes.gameObject.SetActive(false);
        }
    }

    private void SetupMaterial(Material mat)
    {
        playerModel.GetComponent<MeshRenderer>().material = mat;
        playerEyes.GetComponent<MeshRenderer>().material = mat;
    }
}
