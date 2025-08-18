using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private bool showRaycastGizmo = true;

    private bool isQuizActive = false;
    private GameObject waitingTrashObject = null;
    private readonly string[] classNames = { "paper", "pack", "can", "glass", "pet", "plastic", "vinyl" };

    public void SetWaitingForRecognition(GameObject trashObj)
    {
        waitingTrashObject = trashObj;
        Debug.Log($"ML 인식 대기 중: {trashObj.name}");
    }

    public void VisualizeDetectionsWithRaycast(List<Detection> detections, Camera camera, Camera vrCamera, LayerMask trashLayerMask)
    {
        if (isQuizActive || detections == null || detections.Count == 0 || waitingTrashObject == null)
            return;

        if (debugMode)
        {
            foreach (var detection in detections)
            {
                Debug.Log($"ML 감지: {detection.ClassName}, 신뢰도: {detection.Confidence:F2}");
            }
        }

        // 가장 높은 신뢰도의 감지 결과 사용
        Detection bestDetection = null;
        float maxConfidence = 0f;

        foreach (var detection in detections)
        {
            if (detection.Confidence > maxConfidence)
            {
                maxConfidence = detection.Confidence;
                bestDetection = detection;
            }
        }

        if (bestDetection != null && bestDetection.Confidence > 0.5f) // 신뢰도 임계값
        {
            Debug.Log($"인식된 쓰레기: {bestDetection.ClassName} (신뢰도: {bestDetection.Confidence:F2})");

            Vector3 spawnPos = waitingTrashObject.transform.position;
            quizManager?.StartQuiz(bestDetection.ClassName, spawnPos);

            isQuizActive = true;
            waitingTrashObject = null;
        }
        else if (debugMode)
        {
            Debug.Log("신뢰도가 낮아 퀴즈를 시작하지 않음");
        }
    }

    public void OnQuizCompleted()
    {
        isQuizActive = false;
        waitingTrashObject = null;
        if (debugMode) Debug.Log("퀴즈 완료 – 다음 감지 준비");
    }
}