using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExtraGravity : MonoBehaviour
{
    [Range(0f, 5f)] public float gravityMultiplier = 1.5f; // 1=기본중력, 1.5~2 추천
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (rb == null) return;
        // 기본 중력 외에 추가 가속을 더함
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }
}
