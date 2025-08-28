using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;


public class Bucket : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Materials")]
    public Material defaultMaterial;
    public Material filledMaterial;

    [Header("References")]
    public MeshRenderer bucketRenderer;

    private Grabbable grabbable;
    private bool isInOasis = false;
    private bool isFilled = false;

    // Interaction SDK용: 현재 잡힌 상태인지 확인
    public bool IsGrabbed =>
        grabbable != null && grabbable.SelectingPointsCount > 0;
    public bool IsFilled => isFilled;  // 외부에서 확인 가능

    private void Awake()
    {
        if (bucketRenderer == null)
            bucketRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void Start()
    {
        grabbable = GetComponent<Grabbable>();

        if (defaultMaterial != null && bucketRenderer != null)
            bucketRenderer.material = defaultMaterial;
    }

    void Update()
    {
        // 오아시스 안 + 손에서 놓임 + 아직 안 채워짐
        if (!IsGrabbed && isInOasis && !isFilled)
        {
            FillBucket();
        }
    }

    private void FillBucket()
    {
        if (bucketRenderer != null && filledMaterial != null)
        {
            bucketRenderer.material = filledMaterial;
            isFilled = true;
            Debug.Log("버킷이 물로 채워졌습니다! (Interaction SDK)");
            MiniGameManager_Bear.Instance.OnBucketFilled();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oasis"))
            isInOasis = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Oasis"))
            isInOasis = false;
    }
}
