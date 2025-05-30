using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSnapper : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform snapTarget;
    public float snapThreshold = 0.3f;
    private bool isSnapped = false;

    void Update()
    {
        if (!isSnapped && Vector3.Distance(transform.position, snapTarget.position) < snapThreshold)
        {
            transform.position = snapTarget.position;
            transform.rotation = snapTarget.rotation;
            isSnapped = true;
            MiniGameManager_Penguin.Instance.OnStoneSnapped();
        }
    }
}
