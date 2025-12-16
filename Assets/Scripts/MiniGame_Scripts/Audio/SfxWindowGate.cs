using UnityEngine;

public class SfxWindowGate : MonoBehaviour
{
    public static SfxWindowGate Instance;

    [Tooltip("Start() 시점부터 몇 초 동안만 SFX 허용")]
    public float duration = 10f;

    private float startTime;
    private bool started;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        startTime = Time.time;
        started = true;
    }

    public bool IsAllowed()
    {
        if (!started) return false;
        return (Time.time - startTime) <= duration;
    }
}
