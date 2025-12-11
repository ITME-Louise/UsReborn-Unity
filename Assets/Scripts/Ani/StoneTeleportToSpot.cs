using UnityEngine;
using UnityEngine.InputSystem;

public class StoneTeleportToSpot : MonoBehaviour
{
    [Header("Refs (비워도 자동 세팅)")]
    public GameObject stonePrefab;     // 없으면 런타임 Sphere 자동 생성
    public Transform spawnPoint;       // 없으면 Main Camera 앞 자동 생성

    [Tooltip("기본 타겟: Enter로 이동 (이름 'stonespot' / 'StoneSpot' 자동 탐색)")]
    public Transform stoneSpot;

    [Tooltip("보조 타겟: 키보드 '1'로 이동 (이름 'stonespot2' / 'StoneSpot2' 자동 탐색)")]
    public Transform stoneSpot2;

    [Tooltip("보조 타겟: 키보드 '2'로 이동 (이름 'stonespot3' / 'StoneSpot3' 자동 탐색)")]
    public Transform stoneSpot3;

    [Header("Teleport Settings (세 스팟 공통 적용)")]
    public bool dropFromTop = false;
    public float dropHeight = 3f;
    public Vector3 worldOffset = Vector3.zero;
    public float initialDownSpeed = 0f;
    public bool snapRotationToSpot = false;

    [Header("Physics Preset (강제 적용)")]
    public float mass = 1f;
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Spawn / Safety")]
    public float forwardOffset = 0.6f;
    public Collider[] ignoreCollisionsWith;
    public float cleanupAfterSeconds = 8f; // ← 떨어진 뒤 사라지지 않게 하려면 0으로

    private Rigidbody rbInHand;
    private GameObject goInHand;

    // ---------- SETUP ----------
    private void Awake()
    {
        // spawnPoint 자동
        if (!spawnPoint)
        {
            var cam = Camera.main ? Camera.main : FindObjectOfType<Camera>();
            if (cam)
            {
                var sp = new GameObject("StoneSpawn (auto)");
                sp.transform.SetParent(cam.transform, true);
                sp.transform.SetPositionAndRotation(
                    cam.transform.position + cam.transform.forward * 0.6f,
                    cam.transform.rotation);
                spawnPoint = sp.transform;
            }
            else
            {
                Debug.LogWarning("[StoneToSpot] 카메라를 찾지 못했습니다. spawnPoint를 수동 할당하세요.");
            }
        }

        // spot 자동
        if (!stoneSpot)
        {
            var a = GameObject.Find("stonespot");
            if (!a) a = GameObject.Find("StoneSpot");
            if (a) stoneSpot = a.transform;
        }
        if (!stoneSpot2)
        {
            var b = GameObject.Find("stonespot2");
            if (!b) b = GameObject.Find("StoneSpot2");
            if (b) stoneSpot2 = b.transform;
        }
        if (!stoneSpot3)
        {
            var c = GameObject.Find("stonespot3");
            if (!c) c = GameObject.Find("StoneSpot3");
            if (c) stoneSpot3 = c.transform;
        }

        // stonePrefab 자동
        if (!stonePrefab) stonePrefab = CreateRuntimeStonePrefab();
    }

    private GameObject CreateRuntimeStonePrefab()
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.name = "StonePrefab (auto)";
        var rb = g.AddComponent<Rigidbody>();
        rb.useGravity = true;
        g.tag = "Stone";
        g.SetActive(false); // 프리팹처럼 사용
        return g;
    }

    // ---------- UPDATE ----------
    private void Update()
    {
        var kb = Keyboard.current; if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame) SpawnStone();

        // Enter → spot1, '1' → spot2, '2' → spot3
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) TeleportTo(stoneSpot);
        if (kb.digit1Key != null && kb.digit1Key.wasPressedThisFrame) TeleportTo(stoneSpot2);
        if (kb.digit2Key != null && kb.digit2Key.wasPressedThisFrame) TeleportTo(stoneSpot3);

        // 디버그(F1=Spawn, F2=spot1, F3=spot2, F4=spot3)
        if (kb.f1Key.wasPressedThisFrame) SpawnStone();
        if (kb.f2Key.wasPressedThisFrame) TeleportTo(stoneSpot);
        if (kb.f3Key.wasPressedThisFrame) TeleportTo(stoneSpot2);
        if (kb.f4Key.wasPressedThisFrame) TeleportTo(stoneSpot3);
    }

    // ---------- SPAWN ----------
    private void SpawnStone()
    {
        if (goInHand != null) return;
        if (!stonePrefab || !spawnPoint)
        {
            Debug.LogError("[StoneToSpot] spawnPoint/stonePrefab 없음");
            return;
        }

        Vector3 pos = spawnPoint.position + spawnPoint.forward * forwardOffset;
        Quaternion rot = spawnPoint.rotation;

        goInHand = Instantiate(stonePrefab, pos, rot);
        goInHand.name = "Stone (runtime)";
        goInHand.SetActive(true);

        var rb = goInHand.GetComponent<Rigidbody>();
        if (!rb) rb = goInHand.AddComponent<Rigidbody>();

        // 물리 강제 설정
        rb.mass = Mathf.Max(0.0001f, mass);
        rb.drag = Mathf.Max(0f, drag);
        rb.angularDrag = Mathf.Max(0f, angularDrag);
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None;

        // 스폰 직후 자기 몸/카메라와 충돌 무시
        var col = goInHand.GetComponent<Collider>();
        if (col && ignoreCollisionsWith != null)
            foreach (var c in ignoreCollisionsWith) if (c) Physics.IgnoreCollision(col, c, true);

        // 손에 든 상태(정지)
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rbInHand = rb;
        Debug.Log($"[StoneToSpot] Spawn @ {pos}");
    }

    // ---------- TELEPORT (공통) ----------
    private void TeleportTo(Transform targetSpot)
    {
        if (rbInHand == null || goInHand == null) SpawnStone();
        if (rbInHand == null || goInHand == null) return;

        if (!targetSpot)
        {
            Debug.LogWarning("[StoneToSpot] 타겟 스팟이 없습니다. (stonespot/2/3 또는 StoneSpot/2/3 확인)");
            return;
        }

        // 목표 위치 계산
        Vector3 targetPos = targetSpot.position + worldOffset;
        if (dropFromTop)
            targetPos = targetSpot.position + Vector3.up * Mathf.Max(0.1f, dropHeight) + worldOffset;

        // 충돌 끄고 텔레포트 → 다시 켜기
        var col = rbInHand.GetComponent<Collider>();
        if (col) col.enabled = false;

        rbInHand.isKinematic = false;
        rbInHand.position = targetPos;
        rbInHand.rotation = snapRotationToSpot ? targetSpot.rotation : Quaternion.identity;

        rbInHand.velocity = (dropFromTop && initialDownSpeed > 0f)
            ? Vector3.down * initialDownSpeed
            : Vector3.zero;
        rbInHand.angularVelocity = Vector3.zero;

        if (col) col.enabled = true;

        if (cleanupAfterSeconds > 0f) Destroy(goInHand, cleanupAfterSeconds); // 유지하려면 0으로

        // 손에서 놔주기
        goInHand = null;
        rbInHand = null;

        Debug.Log($"[StoneToSpot] Teleport → {targetSpot.name} @ {targetPos}");
    }

    // ---------- GIZMOS ----------
    private void OnDrawGizmosSelected()
    {
        void DrawFor(Transform t, Color c)
        {
            if (!t) return;
            Gizmos.color = c;
            Gizmos.DrawSphere(t.position, 0.06f);
            if (dropFromTop)
            {
                Vector3 p = t.position + Vector3.up * Mathf.Max(0.1f, dropHeight);
                Gizmos.DrawLine(t.position, p);
                Gizmos.DrawSphere(p, 0.06f);
            }
        }

        DrawFor(stoneSpot, Color.magenta);
        DrawFor(stoneSpot2, Color.cyan);
        DrawFor(stoneSpot3, Color.green);
    }
}
