using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLAMCalibrator : MonoBehaviour
{
    [Header("SLAM Calibration")]
    [SerializeField] private Vector3 slamPositionOffset = new Vector3(0.749f, -1.01f, 2.641f);
    [SerializeField] private Vector3 slamRotationOffset = new Vector3(-110f, 60f, 0f);
    [SerializeField] private float slamScale = 1.0f;
    [SerializeField] private bool applySlamCalibration = true;
    [SerializeField] private GameObject slamModel;
    [SerializeField] private bool debugMode = true;
    
    void Start()
    {
        ApplySlamCalibration();
    }
    
    private void ApplySlamCalibration()
    {
        if (slamModel != null && applySlamCalibration)
        {
            slamModel.transform.position = slamPositionOffset;
            slamModel.transform.rotation = Quaternion.Euler(slamRotationOffset);
            slamModel.transform.localScale = Vector3.one * slamScale;
            
            Debug.Log($"SLAM Calibration 적용: Pos={slamPositionOffset}, Rot={slamRotationOffset}, Scale={slamScale}");
        }
    }

    [ContextMenu("Apply SLAM Calibration")]
    public void ApplySlamCalibrationFromInspector()
    {
        ApplySlamCalibration();
    }
    
    public Vector3 TransformSlamCoordinate(Vector3 originalPos)
    {
        if (!applySlamCalibration) return originalPos;
        
        // SLAM 좌표계 → Unity 좌표계로 변환 (Z축 반전)
        Vector3 convertedPos = new Vector3(originalPos.x, originalPos.y, -originalPos.z);
        Debug.Log($"ConvertedPos: {convertedPos}");

        // 회전 적용 (슬램에서 X축 -90도 → Unity에서 Y축 회전으로 맞춤)
        Vector3 rotatedPos = Quaternion.Euler(slamRotationOffset) * convertedPos;
        Debug.Log($"RotatedPos: {rotatedPos}");

        // 위치 오프셋 적용
        Vector3 finalPos = rotatedPos * slamScale + slamPositionOffset;
        Debug.Log($"FinalPos: {finalPos}");

        return finalPos;
    }
    
    public Vector3? GetWorldPosFromUV(Vector2 uv, Camera camera)
    {
        if (slamModel == null) return null;
        
        MeshFilter mf = slamModel.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning($"MeshFilter 없음 또는 sharedMesh가 할당 안됨: 모델 이름 = {slamModel.name}");
            return null;
        }

        Mesh mesh = mf.sharedMesh;
        Vector2[] uvArray = mesh.uv;
        Vector3[] vertices = mesh.vertices;

        if (uvArray == null || uvArray.Length == 0) return null;

        float minDist = float.MaxValue;
        int bestIdx = -1;

        // 가장 가까운 UV 좌표 찾기
        for (int i = 0; i < uvArray.Length; ++i)
        {
            float d = Vector2.Distance(uv, uvArray[i]);
            if (d < minDist) { minDist = d; bestIdx = i; }
        }

        if (bestIdx < 0) return null;

        // 로컬 좌표에서 월드 좌표로 변환
        Vector3 localPos = vertices[bestIdx];
        Vector3 worldPos = mf.transform.TransformPoint(localPos);
        
        // SLAM 좌표계 변환 적용
        Vector3 transformedPos = TransformSlamCoordinate(worldPos);

        if (debugMode)
        {
            Debug.Log($"UV: {uv}, VertexIdx: {bestIdx}, LocalPos: {localPos}, OriginalWorldPos: {worldPos}, TransformedPos: {transformedPos}");
        }

        return transformedPos;
    }

}
