using UnityEngine;

public class StageSwitchSfxOnHit : MonoBehaviour
{
    [Header("Detect")]
    public string rockTag = "Rock";   // 돌에 태그 Rock 달기 
    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip stageUpClip;
    public float volume = 1f;

    [Header("One shot guard")]
    public float cooldown = 0.5f;
    private float lastTime = -999f;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(rockTag)) return;
        if (Time.time - lastTime < cooldown) return; // 연속 충돌 중복 방지
        lastTime = Time.time;

        if (audioSource && stageUpClip)
            audioSource.PlayOneShot(stageUpClip, volume);
    }
}
