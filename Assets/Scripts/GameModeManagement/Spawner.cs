using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;

    private void Start()
    {
        PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
    }
}
