using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinMover : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform[] steppingStones;
    public float moveSpeed = 1.5f;
    private int currentStep = 0;
    private bool isMoving = false;

    public void StartMoving()
    {
        isMoving = true;
    }

    void Update()
    {
        if (isMoving && currentStep < steppingStones.Length)
        {
            Transform target = steppingStones[currentStep];
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                currentStep++;
                if (currentStep >= steppingStones.Length)
                {
                    isMoving = false;
                    MiniGameManager_Penguin.Instance.OnRescueSuccess();
                }
            }
        }
    }
}
