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
                Debug.Log($"Player Hand�� {gameObject.name} ������Ʈ�� ��ȣ�ۿ� ����");
            }
        }
    }

    public void OnGrab()
    {
        isGrabbed = true;
        Debug.Log($"{gameObject.name} ������Ʈ�� �÷��̾� �տ� �������ϴ�.");
    }

    public void OnRelease()
    {
        isGrabbed = false;
        Debug.Log($"{gameObject.name} ������Ʈ�� �÷��̾� �տ��� �������ϴ�.");
    }
}
