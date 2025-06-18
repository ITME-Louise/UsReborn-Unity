using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private PotSpawner potSpawner;
    [SerializeField] private QuizManager quizManager;
    
    private bool isQuizActive = false;
    
    public void VisualizeDetections(List<Detection> detections, Camera camera, SLAMCalibrator calibrator)
    {
        Debug.Log("=== DetectionVisualizer.VisualizeDetections 시작 ===");
        
        if (isQuizActive)
        {
            Debug.Log("DetectionVisualizer: 퀴즈가 진행 중이므로 새로운 감지를 건너뜁니다.");
            return;
        }
        
        if (camera == null)
        {
            Debug.LogError("DetectionVisualizer: 카메라가 null입니다!");
            return;
        }
        
        if (calibrator == null)
        {
            Debug.LogError("DetectionVisualizer: SLAMCalibrator가 null입니다!");
            return;
        }
        
        if (detections == null)
        {
            Debug.LogError("DetectionVisualizer: detections 리스트가 null입니다!");
            return;
        }

        Debug.Log($"DetectionVisualizer: 받은 감지 결과 개수: {detections.Count}");

        if (detections.Count == 0)
        {
            Debug.Log("DetectionVisualizer: 감지된 객체가 없습니다.");
            return;
        }

        for (int i = 0; i < detections.Count; i++)
        {
            var det = detections[i];
            Debug.Log($"DetectionVisualizer: 처리 중인 감지 결과 [{i}] - 클래스: {det.ClassName}, 신뢰도: {det.Confidence:F3}");

            Vector2 uv = new Vector2(det.BoundingBox.center.x, 1f - det.BoundingBox.center.y);

            Debug.Log($"DetectionVisualizer: 바운딩 박스 중심: ({det.BoundingBox.center.x:F2}, {det.BoundingBox.center.y:F2}) → UV: ({uv.x:F2}, {uv.y:F2})");

            // UV → 월드 좌표로 변환 (SLAM 좌표계 변환 포함)
            Vector3? worldPos = calibrator.GetWorldPosition(uv);

            if (worldPos == null)
            {
                Debug.LogWarning($"DetectionVisualizer: UV → 3D 매핑 실패 : {uv}");
                continue;
            }

            Vector3 finalWorldPos = worldPos.Value;

            Debug.Log($"DetectionVisualizer: 최종 처리 결과 - 클래스: {det.ClassName}, 신뢰도: {det.Confidence:F2}, UV: {uv}, 월드좌표: {finalWorldPos}");

            CreateVisualization(det, finalWorldPos, camera);
            break;
        }
        
        Debug.Log("=== DetectionVisualizer.VisualizeDetections 완료 ===");
    }
    
    private void CreateVisualization(Detection detection, Vector3 worldPos, Camera camera)
    {
        Debug.Log($"DetectionVisualizer: CreateVisualization 호출 - 클래스: {detection.ClassName}, 위치: {worldPos}");

        if (quizManager == null)
        {
            Debug.LogError("DetectionVisualizer: QuizManager가 null입니다. Inspector에서 할당해주세요.");
            return;
        }

        isQuizActive = true;
        quizManager.StartQuiz(detection.ClassName, worldPos);
    }

    public void OnQuizCompleted()
    {
        isQuizActive = false;
        Debug.Log("DetectionVisualizer: 퀴즈 완료, 다음 감지 준비됨");
    }
}