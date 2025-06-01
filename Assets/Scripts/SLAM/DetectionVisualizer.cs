using System.Collections.Generic;
using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [SerializeField] private Material[] highlightMaterials;
    [SerializeField] private bool debugMode = true;
    
    private string[] classNames = new string[] {
        "paper", "pack", "can", "glass", "pet", "plastic", "vinyl"
    };
    
    void Start()
    {
        InitializeHighlightMaterials();
    }
    
    private void InitializeHighlightMaterials()
    {
        Debug.Log("DetectionVisualizer: InitializeHighlightMaterials 함수 호출됨");
        try
        {
            if (highlightMaterials == null || highlightMaterials.Length < classNames.Length)
            {
                Debug.Log("DetectionVisualizer: 하이라이트 머티리얼 초기화 시작");
                highlightMaterials = new Material[classNames.Length];
                Color[] colors = new Color[] {
                    Color.red,     // paper
                    Color.blue,    // pack
                    Color.yellow,  // can
                    Color.cyan,    // glass
                    Color.green,   // pet
                    Color.magenta, // plastic
                    Color.white    // vinyl
                };
                
                Debug.Log("DetectionVisualizer: 쉐이더 찾기 시도");
                
                Shader shaderToUse = Shader.Find("Standard");
                if (shaderToUse == null) shaderToUse = Shader.Find("Mobile/Diffuse");
                if (shaderToUse == null) shaderToUse = Shader.Find("Legacy Shaders/Diffuse");
                
                if (shaderToUse == null)
                {
                    Debug.LogError("DetectionVisualizer: 사용 가능한 쉐이더를 찾을 수 없습니다!");
                    return;
                }
                
                Debug.Log("DetectionVisualizer: 사용할 쉐이더: " + shaderToUse.name);
                
                for (int i = 0; i < classNames.Length; i++)
                {
                    highlightMaterials[i] = new Material(shaderToUse);
                    highlightMaterials[i].color = new Color(colors[i].r, colors[i].g, colors[i].b, 0.8f);
                }
                
                Debug.Log("DetectionVisualizer: 하이라이트 머티리얼 초기화 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("DetectionVisualizer: 하이라이트 머티리얼 초기화 중 오류 발생: " + e.Message);
        }
    }
    
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
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"Trash_{detection.ClassName}_{Time.time:F2}";
        sphere.transform.position = worldPos;

        float size = 0.1f + detection.BoundingBox.width * 0.2f;
        sphere.transform.localScale = Vector3.one * size;

        if (detection.ClassIndex < highlightMaterials.Length && highlightMaterials[detection.ClassIndex] != null)
            sphere.GetComponent<Renderer>().material = highlightMaterials[detection.ClassIndex];
        else
            sphere.GetComponent<Renderer>().material.color = Color.red;

        var label = new GameObject($"Label_{detection.ClassName}_{Time.time:F2}");
        label.transform.position = worldPos + Vector3.up * (size + 0.1f);
        var tm = label.AddComponent<TextMesh>();
        tm.text = $"{detection.ClassName}\n{detection.Confidence:F2}";
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = Color.white;

        Vector3 direction = camera.transform.position - label.transform.position;
        if (direction != Vector3.zero)
        {
            label.transform.LookAt(camera.transform.position, Vector3.up);
        }

        Destroy(sphere, 15f);
        Destroy(label, 15f);
    }
}
