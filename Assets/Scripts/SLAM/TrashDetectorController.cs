using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class TrashDetectorController : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;

    private MLModelManager modelManager;
    private CameraCapture cameraCapture;
    private DetectionProcessor detectionProcessor;
    private SLAMCalibrator slamCalibrator;
    private DetectionVisualizer visualizer;
    
    private bool isProcessing = false;
    
    // Awake에서 컴포넌트 참조와 이벤트 구독
    void Awake()
    {
        Debug.Log("TrashDetector: 초기화 시작");
        
        // 필요한 컴포넌트들 가져오기
        modelManager = GetComponent<MLModelManager>();
        cameraCapture = GetComponent<CameraCapture>();
        detectionProcessor = GetComponent<DetectionProcessor>();
        slamCalibrator = GetComponent<SLAMCalibrator>();
        visualizer = GetComponent<DetectionVisualizer>();
        
        // 컴포넌트 확인
        if (modelManager == null) Debug.LogError("TrashDetector: MLModelManager 컴포넌트가 필요합니다!");
        if (cameraCapture == null) Debug.LogError("TrashDetector: CameraCapture 컴포넌트가 필요합니다!");
        if (detectionProcessor == null) Debug.LogError("TrashDetector: DetectionProcessor 컴포넌트가 필요합니다!");
        if (slamCalibrator == null) Debug.LogError("TrashDetector: SLAMCalibrator 컴포넌트가 필요합니다!");
        if (visualizer == null) Debug.LogError("TrashDetector: DetectionVisualizer 컴포넌트가 필요합니다!");
        
        // 모델 로드 완료 이벤트 구독
        if (modelManager != null)
        {
            modelManager.OnModelLoaded += OnModelLoaded;
            modelManager.OnModelLoadError += OnModelLoadError;
            Debug.Log("TrashDetector: 이벤트 구독 완료");
        }
    }
    
    // Start에서 이미 로드된 모델 확인
    void Start()
    {
        // 이미 모델이 로드되어 있다면 즉시 시작
        if (modelManager != null && modelManager.IsModelLoaded)
        {
            Debug.Log("TrashDetector: 모델이 이미 로드되어 있음, 즉시 시작");
            OnModelLoaded();
        }
        else
        {
            Debug.Log("TrashDetector: 모델 로드 대기 중...");
        }
    }
    
    private void OnModelLoaded()
    {
        Debug.Log("OnModelLoaded 이벤트 호출됨");

        // 이미 InvokeRepeating이 실행 중인지 확인
        if (!IsInvoking("DetectTrash"))
        {
            // 3초 후에 쓰레기 감지 시작, 3초 간격으로 반복
            InvokeRepeating("DetectTrash", 3.0f, 3.0f);
            Debug.Log("TrashDetector: 초기화 완료, 3초 후 쓰레기 감지 시작");
        }
    }
    
    private void OnModelLoadError(string error)
    {
        Debug.LogError($"TrashDetector: 모델 로드 실패로 인한 감지 중단 - {error}");
    }
    
    void DetectTrash()
    {
        if (isProcessing || !modelManager.IsModelLoaded) return;
        isProcessing = true;
        
        StartCoroutine(CaptureAndProcess());
    }
    
    IEnumerator CaptureAndProcess()
    {
        yield return new WaitForEndOfFrame();
        
        // 카메라에서 이미지 캡처
        Camera camera = cameraCapture.GetCamera();
        if (camera == null)
        {
            isProcessing = false;
            yield break;
        }
        
        // 카메라 이미지 캡처
        cameraCapture.CaptureImage(camera);
        
        // 이미지 전처리 및 모델 실행
        try
        {
            ProcessImage(camera);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TrashDetector: 이미지 처리 중 오류 - {e.Message}");
        }
        
        isProcessing = false;
    }
    
    private void ProcessImage(Camera camera)
    {
        if (debugMode) Debug.Log("TrashDetector: 이미지 처리 시작");
        
        // 텍스처를 직접 텐서로 변환
        using (var inputTensor = new Tensor(cameraCapture.CurrentTexture, channels: 3))
        {
            if (debugMode) 
            {
                Debug.Log($"TrashDetector: 입력 텐서 크기: {inputTensor.shape}");
                Debug.Log($"TrashDetector: 입력 텐서 차원: batch={inputTensor.batch}, height={inputTensor.height}, width={inputTensor.width}, channels={inputTensor.channels}");
            }
            
            // 모델 실행
            Tensor outputTensor = modelManager.ExecuteModel(inputTensor);
            
            if (outputTensor == null)
            {
                Debug.LogError("TrashDetector: 모델 실행 결과가 null입니다.");
                return;
            }
            
            if (debugMode) 
            {
                Debug.Log($"TrashDetector: 모델 실행 완료, 출력 텐서 크기: {outputTensor.shape}");
                Debug.Log($"TrashDetector: 출력 텐서 차원: batch={outputTensor.batch}, height={outputTensor.height}, width={outputTensor.width}, channels={outputTensor.channels}");
            }
            
            // 결과 처리
            List<Detection> detections = detectionProcessor.ProcessDetectionResults(outputTensor);
            
            // 시각화
            visualizer.VisualizeDetections(detections, camera, slamCalibrator);
            
            // 텐서 해제
            outputTensor.Dispose();
        }
    }
    
    void OnDestroy()
    {
        // InvokeRepeating 중단
        CancelInvoke();
        
        // 이벤트 구독 해제
        if (modelManager != null)
        {
            modelManager.OnModelLoaded -= OnModelLoaded;
            modelManager.OnModelLoadError -= OnModelLoadError;
        }
    }
}