using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableSample : MonoBehaviour
{

    private bool isGrabbed = false;
    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision)
    {
        if (isGrabbed)
        {
            if (collision.gameObject.CompareTag("Player Hands"))
            {
                Debug.Log($"Player Hand로 {gameObject.name} 오브젝트와 상호작용 성공");
            }
        }
    }

    public void OnGrab()
    {
        isGrabbed = true;
        Debug.Log($"{gameObject.name} 오브젝트가 플레이어 손에 잡혔습니다.");
    }

    public void OnRelease()
    {
        isGrabbed = false;
        Debug.Log($"{gameObject.name} 오브젝트가 플레이어 손에서 놓였습니다.");
    }
}
