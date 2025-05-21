using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;

public class TrashDetector : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [SerializeField] private float confidenceThreshold = 0.25f;
    [SerializeField] private Material[] highlightMaterials;
    [SerializeField] private bool debugMode = true;
    
    private string[] classNames = new string[] { 
        "Metal", "Plastic", "Special Waste", 
        "Glass", "Paper", "General Waste" 
    };
    
    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;
    private RenderTexture renderTexture;
    private bool isProcessing = false;
    
    private int inputWidth = 960;
    private int inputHeight = 960;

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
                    Color.red,     // Metal
                    Color.blue,    // Plastic
                    Color.yellow,  // Special Waste
                    Color.cyan,    // Glass
                    Color.green,   // Paper
                    Color.magenta  // General Waste
                };
                
                Debug.Log("TrashDetector: 쉐이더 찾기 시도");
                
                // "Standard" 쉐이더로 시도
                Shader standardShader = Shader.Find("Standard");
                Debug.Log("TrashDetector: Standard 쉐이더 찾기 결과: " + (standardShader != null ? "성공" : "실패"));
                
                // 다른 대체 쉐이더 시도
                Shader mobileShader = Shader.Find("Mobile/Diffuse");
                Debug.Log("TrashDetector: Mobile/Diffuse 쉐이더 찾기 결과: " + (mobileShader != null ? "성공" : "실패"));
                
                Shader legacyShader = Shader.Find("Legacy Shaders/Diffuse");
                Debug.Log("TrashDetector: Legacy Shaders/Diffuse 쉐이더 찾기 결과: " + (legacyShader != null ? "성공" : "실패"));
                
                // 사용할 쉐이더 선택
                Shader shaderToUse = mobileShader;
                if (shaderToUse == null) shaderToUse = legacyShader;
                if (shaderToUse == null) shaderToUse = standardShader;
                
                if (shaderToUse == null)
                {
                    Debug.LogError("TrashDetector: 사용 가능한 쉐이더를 찾을 수 없습니다!");
                    return;
                }
                
                Debug.Log("TrashDetector: 사용할 쉐이더: " + shaderToUse.name);
                
                for (int i = 0; i < classNames.Length; i++)
                {
                    Debug.Log("TrashDetector: 머티리얼 " + i + " 생성 중");
                    highlightMaterials[i] = new Material(shaderToUse);
                    highlightMaterials[i].color = new Color(colors[i].r, colors[i].g, colors[i].b, 0.6f);
                    Debug.Log("TrashDetector: 머티리얼 " + i + " 생성 완료");
                }
                
                Debug.Log("TrashDetector: 하이라이트 머티리얼 초기화 완료");
            }
            else
            {
                Debug.Log("TrashDetector: 하이라이트 머티리얼이 이미 초기화되어 있음");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("TrashDetector: 하이라이트 머티리얼 초기화 중 오류 발생: " + e.Message);
            Debug.LogError("TrashDetector: 스택 트레이스: " + e.StackTrace);
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
        
        // 입력 텐서 생성
        using (var inputTensor = PreprocessImageToTensor(cameraTexture))
        {
            // 모델 실행
            worker.Execute(inputTensor);
            
            // 결과 가져오기
            Tensor outputTensor = worker.PeekOutput("output0");
            
            if (debugMode) 
            {
                Debug.Log($"TrashDetector: 모델 실행 완료, 출력 텐서 크기: {outputTensor.shape}");
                Debug.Log($"TrashDetector: 텐서 차원: {outputTensor.dimensions} [{outputTensor.shape[0]}, {outputTensor.shape[1]}, {outputTensor.shape[2]}]");
            }
            
            // 결과 처리
            ProcessDetectionResults(outputTensor, camera);
            
            // 텐서 해제
            outputTensor.Dispose();
        }
    }
    
    private Tensor PreprocessImageToTensor(Texture2D texture)
    {
        // YOLOv5 모델용 텐서 생성
        float[] inputData = new float[inputWidth * inputHeight * 3];
        
        // 이미지 리사이징 및 정규화
        for (int y = 0; y < inputHeight; y++)
        {
            for (int x = 0; x < inputWidth; x++)
            {
                // 텍스처에서 위치 계산 (캡처 크기에 맞춤)
                int texX = (int)((float)x / inputWidth * captureWidth);
                int texY = (int)((float)y / inputHeight * captureHeight);
                
                Color pixel = cameraTexture.GetPixel(texX, texY);
                
                int idx = y * inputWidth + x;
                // R 채널
                inputData[idx] = pixel.r;
                // G 채널
                inputData[idx + inputWidth * inputHeight] = pixel.g;
                // B 채널
                inputData[idx + 2 * inputWidth * inputHeight] = pixel.b;
            }
        }
        
        // [1, 3, 960, 960] 형식의 텐서 생성
        return new Tensor(1, inputHeight, inputWidth, 3, inputData);
    }
    
    private void ProcessDetectionResults(Tensor outputTensor, Camera camera)
    {
        
        // 출력 텐서에서 데이터 읽기
        var output = outputTensor.ToReadOnlyArray();
        
        if (debugMode) Debug.Log($"TrashDetector: 출력 배열 크기: {output.Length}");
        
        int numBBoxes = outputTensor.shape[1];      // 56700
        int valuesPerBox = outputTensor.shape[2];   // 11
        
        List<Detection> detections = new List<Detection>();
        
        // 바운딩 박스 처리
        for (int i = 0; i < numBBoxes; i++)
        {
            int offset = i * valuesPerBox;
            
            // YOLOv5 출력 형식
            float objectness = output[offset + 4];
            
            // 신뢰도가 임계값을 넘는 경우만 처리
            if (objectness > confidenceThreshold)
            {
                float x = output[offset + 0];
                float y = output[offset + 1];
                float w = output[offset + 2];
                float h = output[offset + 3];
                
                // 클래스 점수 확인
                int bestClassIdx = 0;
                float bestScore = 0;
                
                for (int c = 0; c < classNames.Length; c++)
                {
                    float classScore = output[offset + 5 + c];
                    if (classScore > bestScore)
                    {
                        bestScore = classScore;
                        bestClassIdx = c;
                    }
                }
                
                // 최종 신뢰도
                float confidence = objectness * bestScore;
                
                if (confidence > confidenceThreshold)
                {
                    Detection detection = new Detection
                    {
                        BoundingBox = new Rect(x - w/2, y - h/2, w, h),
                        Confidence = confidence,
                        ClassIndex = bestClassIdx,
                        ClassName = classNames[bestClassIdx]
                    };
                    
                    detections.Add(detection);
                    
                    if (debugMode)
                    {
                        Debug.Log($"TrashDetector: 감지됨 - {detection.ClassName}, 신뢰도: {detection.Confidence:F2}, " +
                                  $"위치: ({detection.BoundingBox.x:F2}, {detection.BoundingBox.y:F2}, " +
                                  $"{detection.BoundingBox.width:F2}, {detection.BoundingBox.height:F2})");
                    }
                }
            }
        }
        
        List<Detection> filteredDetections = ApplyNMS(detections, 0.45f);
        
        // 결과 시각화
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
        
        return intersection / union;
    }
    
    private void VisualizeDetections(List<Detection> detections, Camera camera)
    {
        Debug.Log($"TrashDetector: 카메라 위치: {camera.transform.position}, 방향: {camera.transform.forward}, " +
        $"nearClipPlane: {camera.nearClipPlane}, farClipPlane: {camera.farClipPlane}, " +
        $"cullingMask: {camera.cullingMask}, 감지 개수: {detections.Count}");
        
        foreach (var detection in detections)
        {
            float normalizedX = (detection.BoundingBox.center.x * 2f) - 1f; 
            float normalizedY = 1f - (detection.BoundingBox.center.y * 2f);
            
            // 고정된 깊이 값 설정 (적절한 거리로 조정)
            float depth = 2.0f;
            
            // 카메라 전방에 고정된 거리로 위치시킴
            Vector3 forward = camera.transform.forward * depth;
            Vector3 right = camera.transform.right * normalizedX * depth * 0.5f;
            Vector3 up = camera.transform.up * normalizedY * depth * 0.5f;
            
            // 최종 월드 좌표 계산 (카메라 기준)
            Vector3 worldPoint = camera.transform.position + forward + right + up;
            
            // 구체 생성
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"Trash_{detection.ClassName}";
            sphere.transform.position = worldPoint;
            
            // 크기 설정
            float size = 0.1f + (detection.BoundingBox.width * 0.2f);
            sphere.transform.localScale = new Vector3(size, size, size);
            
            // 머티리얼 적용
            if (detection.ClassIndex < highlightMaterials.Length)
            {
                sphere.GetComponent<Renderer>().material = highlightMaterials[detection.ClassIndex];
            }
            
            // 레이어 설정
            sphere.layer = LayerMask.NameToLayer("UI") != -1 ? LayerMask.NameToLayer("UI") : 5;
            
            Debug.Log($"TrashDetector: 구체 생성 - 이름: {sphere.name}, 위치: {worldPoint}, 크기: {sphere.transform.localScale}, " +
            $"머티리얼 있음: {sphere.GetComponent<Renderer>().material != null}");
            
            // 라벨 추가
            GameObject textObj = new GameObject($"Label_{detection.ClassName}");
            textObj.transform.position = worldPoint + camera.transform.up * 0.15f;
            
            // 텍스트가 카메라를 향하도록 설정
            textObj.transform.LookAt(camera.transform);
            textObj.transform.Rotate(0, 180, 0);
            
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = $"{detection.ClassName}: {detection.Confidence:F2}";
            textMesh.fontSize = 24;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.LowerCenter;
            textMesh.color = Color.white;
            textMesh.characterSize = 0.3f;
            
            // 텍스트도 항상 보이도록 레이어 설정
            textObj.layer = sphere.layer;
            
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