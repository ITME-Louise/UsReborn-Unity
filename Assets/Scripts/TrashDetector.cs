using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;

public class TrashDetector : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [SerializeField] private float confidenceThreshold = 0.4f;
    [SerializeField] private Material[] highlightMaterials;
    [SerializeField] private bool debugMode = true;
    
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
        
        // 하이라이트 머티리얼 초기화
        InitializeHighlightMaterials();
        
        // 3초 후에 쓰레기 감지 시작, 3초 간격으로 반복
        InvokeRepeating("DetectTrash", 3.0f, 3.0f);
        Debug.Log("TrashDetector: 초기화 완료, 3초 후 쓰레기 감지 시작");
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
            Debug.Log($"BBox 값: x={x}, y={y}, w={w}, h={h}");
            float objectness = outputTensor[0, 0, 4, i];
            
            float sigmoid_objectness = 1f / (1f + Mathf.Exp(-objectness));
            
            if (sigmoid_objectness > 0.5f)
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
                
                if (confidence > confidenceThreshold && w > 5 && h > 5 && x > 0 && y > 0 && x < inputWidth && y < inputHeight)
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
        VisualizeDetections(filteredDetections, camera);
    
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
    
    private void VisualizeDetections(List<Detection> detections, Camera camera)
    {
        Debug.Log($"TrashDetector: 감지 개수: {detections.Count}");
        
        foreach (var detection in detections)
        {
            // 정규화된 좌표를 화면 좌표로 변환
            float screenX = detection.BoundingBox.center.x * camera.pixelWidth;
            float screenY = (1.0f - detection.BoundingBox.center.y) * camera.pixelHeight; // Y축 뒤집기
            
            // 화면 좌표를 월드 좌표로 변환
            Vector3 screenPoint = new Vector3(screenX, screenY, 2.0f);
            Vector3 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            
            // 구체 생성
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"Trash_{detection.ClassName}_{Time.time}";
            sphere.transform.position = worldPoint;
            
            // 크기 설정
            float size = 0.2f + (detection.BoundingBox.width * 0.3f);
            sphere.transform.localScale = new Vector3(size, size, size);
            
            // 머티리얼 적용
            if (detection.ClassIndex < highlightMaterials.Length && highlightMaterials[detection.ClassIndex] != null)
            {
                sphere.GetComponent<Renderer>().material = highlightMaterials[detection.ClassIndex];
            }
            else
            {
                // 기본 색상 설정
                sphere.GetComponent<Renderer>().material.color = Color.red;
            }
            
            Debug.Log($"TrashDetector: 구체 생성 - 이름: {sphere.name}, 위치: {worldPoint}, 크기: {size:F2}");
            
            // 라벨 추가
            GameObject textObj = new GameObject($"Label_{detection.ClassName}_{Time.time}");
            textObj.transform.position = worldPoint + Vector3.up * (size + 0.1f);
            
            // 텍스트가 카메라를 향하도록 설정
            textObj.transform.LookAt(camera.transform);
            textObj.transform.Rotate(0, 180, 0);
            
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = $"{detection.ClassName}\n{detection.Confidence:F2}";
            textMesh.fontSize = 20;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = Color.white;
            textMesh.characterSize = 0.05f;
            
            // 10초 후 제거
            Destroy(sphere, 10.0f);
            Destroy(textObj, 10.0f);
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