using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlankSnapper : MonoBehaviour
{
    public Transform snapTarget;
    public float snapThreshold = 0.5f;
    public bool isSnapped = false;

    void Update()
    {
        if (!isSnapped && Vector3.Distance(transform.position, snapTarget.position) < snapThreshold)
        {
            transform.position = snapTarget.position;
            transform.rotation = snapTarget.rotation;
            isSnapped = true;
            MiniGameManager.Instance.OnPlankSnapped();
        }
    }
}
