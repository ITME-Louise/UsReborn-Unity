using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private PotSpawner potSpawner;
    
    public void VisualizeDetections(List<Detection> detections, Camera camera, SLAMCalibrator calibrator)
    {
        if (camera == null || calibrator == null) return;

        Debug.Log($"DetectionVisualizer: 최종 감지 개수: {detections.Count}");

        foreach (var det in detections)
        {
            Vector2 uv = new Vector2(det.BoundingBox.center.x, 1f - det.BoundingBox.center.y);

            if (debugMode)
            {
                Debug.Log($"바운딩 박스 중심: ({det.BoundingBox.center.x:F2}, {det.BoundingBox.center.y:F2}) → UV: ({uv.x:F2}, {uv.y:F2})");
            }

            // UV → 월드 좌표로 변환 (SLAM 좌표계 변환 포함)
            Vector3? worldPos = calibrator.GetWorldPosition(uv);

            if (worldPos == null)
            {
                Debug.LogWarning($"UV → 3D 매핑 실패 : {uv}");
                continue;
            }

            Vector3 finalWorldPos = worldPos.Value;

            if (debugMode)
            {
                Debug.Log($"감지됨: {det.ClassName} / Confidence: {det.Confidence:F2} / UV: {uv} / FinalWorldPos: {finalWorldPos}");
            }

            CreateVisualization(det, finalWorldPos, camera);
        }
    }
    
    private void CreateVisualization(Detection detection, Vector3 worldPos, Camera camera)
    {
        if (potSpawner != null)
        {
            potSpawner.SpawnPot(worldPos);
        }
    }
}