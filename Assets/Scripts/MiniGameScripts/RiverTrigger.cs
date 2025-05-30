using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rabbit"))
        {
            MiniGameManager.Instance.OnRabbitFellInWater();
        }
    }
}
