using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;

public class TrashDetector : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [SerializeField] private float confidenceThreshold = 0.6f;
    [SerializeField] private Material[] highlightMaterials;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private GameObject slamModel;
    
    [Header("SLAM Calibration")]
    [SerializeField] private Vector3 slamPositionOffset = new Vector3(0.749f, -1.01f, 2.641f);
    [SerializeField] private Vector3 slamRotationOffset = new Vector3(-110f, 60f, 0f);
    [SerializeField] private float slamScale = 1.0f;
    [SerializeField] private bool applySlamCalibration = true;
    
    private string[] classNames = new string[] {
        "paper", "pack", "can", "glass", "pet", "plastic", "vinyl"
    };
    
    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;
    private RenderTexture renderTexture;
    private bool isProcessing = false;
    
    private int inputWidth = 640;
    private int inputHeight = 640;
    private int captureWidth = 640;
    private int captureHeight = 640;

    void Start()
    {
        Debug.Log("TrashDetector: 초기화 시작");
        
        // ONNX 모델 로드
        try {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);
            Debug.Log("TrashDetector: 모델 로드 완료");
        }
        catch (System.Exception e) {
            Debug.LogError($"TrashDetector: 모델 로드 실패 - {e.Message}");
            return;
        }
        
        // 캡처 크기를 모델 입력 크기와 동일하게 설정
        captureWidth = inputWidth;   // 640
        captureHeight = inputHeight; // 640
        
        // 카메라 텍스처 초기화
        renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        renderTexture.Create();
        cameraTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        
        // OVRCameraRig 찾기
        if (ovrCameraRig == null)
        {
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
            if (ovrCameraRig == null)
            {
                Debug.LogError("TrashDetector: OVRCameraRig를 찾을 수 없습니다!");
            }
            else
            {
                Debug.Log("TrashDetector: OVRCameraRig 자동 찾기 성공");
            }
        }
        
        // SLAM 모델 좌표계 설정
        ApplySlamCalibration();
        
        // 하이라이트 머티리얼 초기화
        InitializeHighlightMaterials();
        
        // 3초 후에 쓰레기 감지 시작, 3초 간격으로 반복
        InvokeRepeating("DetectTrash", 3.0f, 3.0f);
        Debug.Log("TrashDetector: 초기화 완료, 3초 후 쓰레기 감지 시작");
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

    private void InitializeHighlightMaterials()
    {
        Debug.Log("TrashDetector: InitializeHighlightMaterials 함수 호출됨");
        try
        {
            if (highlightMaterials == null || highlightMaterials.Length < classNames.Length)
            {
                Debug.Log("TrashDetector: 하이라이트 머티리얼 초기화 시작");
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
                
                Debug.Log("TrashDetector: 쉐이더 찾기 시도");
                
                Shader shaderToUse = Shader.Find("Standard");
                if (shaderToUse == null) shaderToUse = Shader.Find("Mobile/Diffuse");
                if (shaderToUse == null) shaderToUse = Shader.Find("Legacy Shaders/Diffuse");
                
                if (shaderToUse == null)
                {
                    Debug.LogError("TrashDetector: 사용 가능한 쉐이더를 찾을 수 없습니다!");
                    return;
                }
                
                Debug.Log("TrashDetector: 사용할 쉐이더: " + shaderToUse.name);
                
                for (int i = 0; i < classNames.Length; i++)
                {
                    highlightMaterials[i] = new Material(shaderToUse);
                    highlightMaterials[i].color = new Color(colors[i].r, colors[i].g, colors[i].b, 0.8f);
                }
                
                Debug.Log("TrashDetector: 하이라이트 머티리얼 초기화 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("TrashDetector: 하이라이트 머티리얼 초기화 중 오류 발생: " + e.Message);
        }
    }

    void OnDestroy()
    {
        worker?.Dispose();
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
        }
        Debug.Log("TrashDetector: 리소스 해제 완료");
    }
    
    void DetectTrash()
    {
        if (isProcessing) return;
        isProcessing = true;
        
        StartCoroutine(CaptureAndProcess());
    }
    
    IEnumerator CaptureAndProcess()
    {
        yield return new WaitForEndOfFrame();
        
        // 카메라에서 이미지 캡처
        Camera centerCamera = GetCamera();
        if (centerCamera == null)
        {
            isProcessing = false;
            yield break;
        }
        
        // 카메라 이미지 캡처
        CaptureImage(centerCamera);
        
        // 이미지 전처리 및 모델 실행
        try
        {
            ProcessImage(centerCamera);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TrashDetector: 이미지 처리 중 오류 - {e.Message}");
        }
        
        isProcessing = false;
    }
    
    private Camera GetCamera()
    {
        Camera centerCamera = null;
        if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
        {
            centerCamera = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
            if (debugMode) Debug.Log("TrashDetector: OVR 중앙 카메라 사용");
        }
        
        if (centerCamera == null)
        {
            centerCamera = Camera.main;
            if (centerCamera == null)
            {
                Debug.LogError("TrashDetector: 카메라를 찾을 수 없습니다!");
                return null;
            }
            if (debugMode) Debug.Log("TrashDetector: 메인 카메라 사용");
        }
        
        return centerCamera;
    }
    
    private void CaptureImage(Camera camera)
    {
        RenderTexture prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;
        
        // 렌더 텍스처에서 텍스처로 변환
        RenderTexture.active = renderTexture;
        cameraTexture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        cameraTexture.Apply();
        RenderTexture.active = null;
        
        if (debugMode) Debug.Log("TrashDetector: 카메라 이미지 캡처 완료");
    }
    
    private void ProcessImage(Camera camera)
    {
        if (debugMode) Debug.Log("TrashDetector: 이미지 처리 시작");
        
        // 텍스처를 직접 텐서로 변환 (더 안전한 방법)
        using (var inputTensor = new Tensor(cameraTexture, channels: 3))
        {
            if (debugMode) 
            {
                Debug.Log($"TrashDetector: 입력 텐서 크기: {inputTensor.shape}");
                Debug.Log($"TrashDetector: 입력 텐서 차원: batch={inputTensor.batch}, height={inputTensor.height}, width={inputTensor.width}, channels={inputTensor.channels}");
            }
            
            // 모델 실행
            worker.Execute(inputTensor);
            
            // 결과 가져오기
            Tensor outputTensor = worker.PeekOutput("output0");
            
            if (debugMode) 
            {
                Debug.Log($"TrashDetector: 모델 실행 완료, 출력 텐서 크기: {outputTensor.shape}");
                Debug.Log($"TrashDetector: 출력 텐서 차원: batch={outputTensor.batch}, height={outputTensor.height}, width={outputTensor.width}, channels={outputTensor.channels}");
            }
            
            // 결과 처리
            ProcessDetectionResults(outputTensor, camera);
            
            // 텐서 해제
            outputTensor.Dispose();
        }
    }
    
    private void ProcessDetectionResults(Tensor outputTensor, Camera camera)
    {
        int valuesPerBox = outputTensor.width;    // 12 (x, y, w, h, obj + 클래스 개수)
        int numBBoxes = outputTensor.channels;   // 25200
        
        if (debugMode)
        {
            Debug.Log($"TrashDetector: numBBoxes={numBBoxes}, valuesPerBox={valuesPerBox}");
        }
        
        List<Detection> detections = new List<Detection>();
        
        int expectedValuesPerBox = 5 + classNames.Length; // 5 + 클래스 개수
        
        if (valuesPerBox != expectedValuesPerBox)
        {
            Debug.LogWarning($"TrashDetector: valuesPerBox ({valuesPerBox}) 가 기대값({expectedValuesPerBox})과 다릅니다.");
        }
        
        // outputTensor shape: (batch=1, height=1, width=12, channels=25200)
        // valuesPerBox = width = 12
        // numBBoxes = channels = 25200
        
        for (int i = 0; i < numBBoxes; i++)
        {
            float x = outputTensor[0, 0, 0, i];
            float y = outputTensor[0, 0, 1, i];
            float w = outputTensor[0, 0, 2, i];
            float h = outputTensor[0, 0, 3, i];
            float objectness = outputTensor[0, 0, 4, i];
            
            float sigmoid_objectness = 1f / (1f + Mathf.Exp(-objectness));
            
            if (sigmoid_objectness > 0.6f)
            {
                float[] classScores = new float[classNames.Length];
                int bestClassIdx = 0;
                float bestScore = 0f;
                
                for (int c = 0; c < classNames.Length; c++)
                {
                    float rawClassScore = outputTensor[0, 0, 5 + c, i];
                    float sigmoidClassScore = 1f / (1f + Mathf.Exp(-rawClassScore));
                    classScores[c] = sigmoidClassScore;
                    
                    if (sigmoidClassScore > bestScore)
                    {
                        bestScore = sigmoidClassScore;
                        bestClassIdx = c;
                    }
                }
                
                float confidence = sigmoid_objectness * bestScore;
                
                if (confidence > confidenceThreshold && 
                    w > 10 && h > 10 &&
                    w < inputWidth * 0.8f && h < inputHeight * 0.8f &&
                    x > 0 && y > 0 && x < inputWidth && y < inputHeight)
                {
                    Detection detection = new Detection
                    {
                        BoundingBox = new Rect(
                            Mathf.Clamp01((x - w / 2) / inputWidth),
                            Mathf.Clamp01((y - h / 2) / inputHeight),
                            Mathf.Clamp01(w / inputWidth),
                            Mathf.Clamp01(h / inputHeight)
                        ),
                        Confidence = confidence,
                        ClassIndex = bestClassIdx,
                        ClassName = classNames[bestClassIdx]
                    };
                    
                    detections.Add(detection);
                    
                    if (debugMode)
                    {
                        string allScores = string.Join(", ", classNames.Select((name, idx) => $"{name}:{classScores[idx]:F3}"));
                        
                         Debug.Log($"감지됨 - {detection.ClassName}, 신뢰도: {confidence:F3}, " +
                         $"위치: ({detection.BoundingBox.x:F2}, {detection.BoundingBox.y:F2}, " +
                         $"{detection.BoundingBox.width:F2}, {detection.BoundingBox.height:F2}), " +
                         $"모든 클래스 점수: [{allScores}]");
                    }
                }
            }
        }
        
        if (debugMode) Debug.Log($"TrashDetector: 총 {detections.Count}개 감지 결과");
        
        var filteredDetections = ApplyNMS(detections, 0.45f);
        VisualizeDetections(filteredDetections);
    
    }
    
    private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
    {
        // 신뢰도 기준 내림차순 정렬
        var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
        List<Detection> selectedDetections = new List<Detection>();
        
        while (sortedDetections.Count > 0)
        {
            // 가장 높은 신뢰도의 검출 선택
            Detection current = sortedDetections[0];
            selectedDetections.Add(current);
            sortedDetections.RemoveAt(0);
            
            // 나머지 검출과 IoU 계산하여 겹치는 것 제거
            sortedDetections.RemoveAll(d => IoU(current.BoundingBox, d.BoundingBox) > iouThreshold);
        }
        
        if (debugMode) Debug.Log($"TrashDetector: NMS 적용 후 {selectedDetections.Count}개 감지 결과 남음");
        return selectedDetections;
    }
    
    private float IoU(Rect box1, Rect box2)
    {
        // 두 박스가 겹치는 영역 계산
        float xOverlap = Mathf.Max(0, Mathf.Min(box1.xMax, box2.xMax) - Mathf.Max(box1.xMin, box2.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(box1.yMax, box2.yMax) - Mathf.Max(box1.yMin, box2.yMin));
        
        float intersection = xOverlap * yOverlap;
        float union = box1.width * box1.height + box2.width * box2.height - intersection;
        
        return union > 0 ? intersection / union : 0;
    }
    
    // SLAM 좌표계 변환 함수 추가
    private Vector3 TransformSlamCoordinate(Vector3 originalPos)
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

    
    private Vector3? GetWorldPosFromUV(Vector2 uv, GameObject model, Camera camera)
    {
        MeshFilter mf = model.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning($"MeshFilter 없음 또는 sharedMesh가 할당 안됨: 모델 이름 = {model.name}");
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

    private void VisualizeDetections(List<Detection> detections)
    {
        Camera camera = GetCamera();
        if (camera == null) return;

        Debug.Log($"TrashDetector: 최종 감지 개수: {detections.Count}");

        foreach (var det in detections)
        {
            Vector2 uv = new Vector2(det.BoundingBox.center.x, 1f - det.BoundingBox.center.y);

            if (debugMode)
            {
                Debug.Log($"바운딩 박스 중심: ({det.BoundingBox.center.x:F2}, {det.BoundingBox.center.y:F2}) → UV: ({uv.x:F2}, {uv.y:F2})");
            }

            // UV → 월드 좌표로 변환 (SLAM 좌표계 변환 포함)
            Vector3? worldPos = GetWorldPosFromUV(uv, slamModel, camera);

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

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"Trash_{det.ClassName}_{Time.time:F2}";
            sphere.transform.position = finalWorldPos;

            float size = 0.1f + det.BoundingBox.width * 0.2f;
            sphere.transform.localScale = Vector3.one * size;

            if (det.ClassIndex < highlightMaterials.Length && highlightMaterials[det.ClassIndex] != null)
                sphere.GetComponent<Renderer>().material = highlightMaterials[det.ClassIndex];
            else
                sphere.GetComponent<Renderer>().material.color = Color.red;

            var label = new GameObject($"Label_{det.ClassName}_{Time.time:F2}");
            label.transform.position = finalWorldPos + Vector3.up * (size + 0.1f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = $"{det.ClassName}\n{det.Confidence:F2}";
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
}

// 감지 결과 저장용 클래스
public class Detection
{
    public Rect BoundingBox;
    public float Confidence;
    public int ClassIndex;
    public string ClassName;
}