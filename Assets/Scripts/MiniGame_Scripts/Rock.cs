using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Rock : MonoBehaviour
{
    public bool Scored { get; private set; }
    public Rigidbody RB { get; private set; }
    void Awake() => RB = GetComponent<Rigidbody>();
    public bool IsSettled(float speedThreshold) =>
        RB && RB.velocity.sqrMagnitude <= speedThreshold * speedThreshold;
    public bool TryMarkScored() { if (Scored) return false; Scored = true; return true; }
}
