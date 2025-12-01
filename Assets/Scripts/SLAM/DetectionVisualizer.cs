using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private float minimumConfidence = 0.5f;

    private bool isQuizActive = false;
    private GameObject waitingTrashObject = null;

    public void SetWaitingForRecognition(GameObject trashObj)
    {
        if (trashObj == null)
        {
            Debug.LogWarning("DetectionVisualizer: trashObj가 null입니다.");
            return;
        }

        waitingTrashObject = trashObj;
        Debug.Log($"ML 인식 대기 중: {trashObj.name}");
    }

    public void VisualizeDetections(List<Detection> detections, Camera camera, Camera vrCamera)
    {
        // 퀴즈가 이미 활성화 or 대기 중인 쓰레기가 없으면 무시
        if (isQuizActive || waitingTrashObject == null)
        {
            return;
        }

        // 감지 결과가 없으면 무시
        if (detections == null || detections.Count == 0)
        {
            if (debugMode) Debug.Log("DetectionVisualizer: 감지된 결과가 없습니다.");
            return;
        }

        if (debugMode)
        {
            foreach (var detection in detections)
            {
                Debug.Log($"DetectionVisualizer: ML 감지 - {detection.ClassName}, 신뢰도: {detection.Confidence:F2}");
            }
        }

        // 가장 높은 신뢰도의 감지 결과 찾기
        Detection bestDetection = GetBestDetection(detections);

        if (bestDetection != null && bestDetection.Confidence >= minimumConfidence)
        {
            Debug.Log($"DetectionVisualizer: 인식 완료 - {bestDetection.ClassName} (신뢰도: {bestDetection.Confidence:F2})");

            Vector3 spawnPos = waitingTrashObject.transform.position;

            if (quizManager != null)
            {
                quizManager.StartQuiz(bestDetection.ClassName, spawnPos);
                isQuizActive = true;
                waitingTrashObject = null;
            }
            else
            {
                Debug.LogWarning("DetectionVisualizer: QuizManager가 할당되지 않았습니다.");
            }
        }
        else if (debugMode)
        {
            Debug.Log($"DetectionVisualizer: 신뢰도가 낮아 퀴즈를 시작하지 않음 (최소 요구: {minimumConfidence})");
        }
    }

    private Detection GetBestDetection(List<Detection> detections)
    {
        if (detections == null || detections.Count == 0) return null;

        Detection best = detections[0];
        for (int i = 1; i < detections.Count; i++)
        {
            if (detections[i].Confidence > best.Confidence)
            {
                best = detections[i];
            }
        }

        return best;
    }

    public void OnQuizCompleted()
    {
        isQuizActive = false;
        waitingTrashObject = null;

        if (debugMode) Debug.Log("DetectionVisualizer: 퀴즈 완료 – 다음 감지 준비");
    }
}