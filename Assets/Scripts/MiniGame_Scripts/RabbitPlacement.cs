using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform plankTop;
    public float placementThreshold = 0.3f;
    public bool isPlaced = false;

    void Update()
    {
        if (!isPlaced && Vector3.Distance(transform.position, plankTop.position) < placementThreshold)
        {
            transform.position = plankTop.position;
            isPlaced = true;
            MiniGameManager.Instance.OnRabbitPlaced();
        }
    }
}
