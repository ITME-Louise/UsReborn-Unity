using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlankMover : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform targetPosition;
    public float moveSpeed = 1f;
    private bool shouldMove = false;

    public void StartMoving()
    {
        shouldMove = true;
    }

    void Update()
    {
        if (shouldMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition.position) < 0.1f)
            {
                shouldMove = false;
                MiniGameManager.Instance.OnRescueSuccess();
            }
        }
    }
}
