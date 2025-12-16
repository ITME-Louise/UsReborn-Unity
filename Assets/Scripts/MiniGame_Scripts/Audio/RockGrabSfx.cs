using UnityEngine;

public class RockGrabSfx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioSource audioSource;

    [Header("SFX")]
    [SerializeField] private AudioClip grabClip;
    [Range(0f, 1f)][SerializeField] private float grabVolume = 1f;

    [Header("State")]
    [SerializeField] private float grabbedVelocityThreshold = 0.02f; // 잡히면 속도가 거의 0으로 고정되는 경우가 많음
    private bool _wasGrabbed;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f; // 3D
    }

    private void Update()
    {
        // "잡힘" 상태를 가장 안전하게 추정하는 방식:
        // - 잡히면 (특히 핀치/거리잡기) 물리가 억제되며 rb.velocity가 매우 작아지거나 kinematic으로 바뀜
        bool isGrabbedNow = (rb != null && (rb.isKinematic || rb.velocity.magnitude < grabbedVelocityThreshold));

        // 처음 잡힌 순간만 1회 재생
        if (!_wasGrabbed && isGrabbedNow)
        {
            PlayGrab();
        }

        _wasGrabbed = isGrabbedNow;
    }

    private void PlayGrab()
    {
        if (audioSource == null || grabClip == null) return;
        audioSource.PlayOneShot(grabClip, grabVolume);
    }
}
