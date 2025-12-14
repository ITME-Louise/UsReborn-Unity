using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class RockGroundHitSfx : MonoBehaviour
{
    [SerializeField] private AudioClip hitGroundClip;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float minImpactSpeed = 0.5f;
    [SerializeField] private float cooldown = 0.08f;

    private AudioSource _audio;
    private Rigidbody _rb;
    private float _lastPlayTime = -999f;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();

        _audio.playOnAwake = false;
        _audio.loop = false;
        _audio.spatialBlend = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥만
        if (!collision.gameObject.CompareTag(groundTag)) return;

        // 연속 재생 방지
        if (Time.time - _lastPlayTime < cooldown) return;

        // 충돌 강도(속도) 기준
        float speed = _rb.velocity.magnitude;
        if (speed < minImpactSpeed) return;

        if (hitGroundClip != null)
        {
            _audio.PlayOneShot(hitGroundClip);
            _lastPlayTime = Time.time;
        }
    }
}
