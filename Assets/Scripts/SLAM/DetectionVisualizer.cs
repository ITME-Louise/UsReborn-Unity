using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private bool showRaycastGizmo = true;

    private bool isQuizActive = false;

    private readonly string[] classNames = { "paper", "pack", "can", "glass", "pet", "plastic", "vinyl" };

    public void VisualizeDetectionsWithRaycast(List<Detection> detections, Camera camera, Camera vrCamera, LayerMask trashLayerMask)
    {
        if (isQuizActive || vrCamera == null || detections == null || detections.Count == 0)
            return;

        Vector3 rayOrigin = vrCamera.transform.position;
        Vector3 rayDirection = vrCamera.transform.forward;

        if (showRaycastGizmo)
            Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red, 2f);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, raycastDistance))
        {
            if (debugMode) Debug.Log($"Raycast 히트: {hit.collider.name}, 위치: {hit.point}");

            string trashClass = GetTrashClassFromObject(hit.collider.gameObject);
            if (!string.IsNullOrEmpty(trashClass))
            {
                Detection matched = FindMatchingDetection(detections, trashClass);
                if (matched != null)
                {
                    if (debugMode) Debug.Log($"매칭 감지: {matched.ClassName}, 신뢰도: {matched.Confidence:F2}");
                    CreateVisualization(matched, hit.point, camera);
                }
                else if (debugMode) Debug.Log($"감지 결과에 {trashClass} 없음");
            }
            else if (debugMode) Debug.Log($"클래스 없음: {hit.collider.name}");
        }
        else if (debugMode)
            Debug.Log("Raycast 히트 없음");
    }

    private string GetTrashClassFromObject(GameObject obj)
    {
        string lowerName = obj.name.ToLower();
        foreach (var cname in classNames)
            if (lowerName.Contains(cname))
                return cname;

        var trashType = obj.GetComponent<TrashType>();
        return trashType != null ? trashType.className : null;
    }

    private Detection FindMatchingDetection(List<Detection> detections, string target)
    {
        foreach (var d in detections)
            if (d.ClassName == target)
                return d;
        return null;
    }

    private void CreateVisualization(Detection detection, Vector3 hitPoint, Camera camera)
    {
        Vector3 spawnPos = hitPoint;
        quizManager.StartQuiz(detection.ClassName, spawnPos);
    }

    public void OnQuizCompleted()
    {
        isQuizActive = false;
        if (debugMode) Debug.Log("퀴즈 완료 – 다음 감지 준비");
    }
}

[System.Serializable]
public class TrashType : MonoBehaviour
{
    public string className;
}
