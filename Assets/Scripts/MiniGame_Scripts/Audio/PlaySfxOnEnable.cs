using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySfxOnEnable : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; 
    }

    private void OnEnable()
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}
