using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearDrinkTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bucket"))
        {
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null && rend.material.color == Color.blue)
            {
                MiniGameManager_Bear.Instance.OnBearSatisfied();
            }
        }
    }
}
