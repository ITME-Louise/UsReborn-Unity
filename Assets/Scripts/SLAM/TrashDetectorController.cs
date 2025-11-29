using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class TrashDetectorController : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;

    private MLModelManager modelManager;
    private CameraCapture cameraCapture;
    private DetectionProcessor detectionProcessor;
    private DetectionVisualizer visualizer;

    private bool isProcessing = false;
    private Coroutine currentDetectionCoroutine;

    void Awake()
    {
        // 컴포넌트 참조 가져오기
        modelManager = GetComponent<MLModelManager>();
        cameraCapture = GetComponent<CameraCapture>();
        detectionProcessor = GetComponent<DetectionProcessor>();
        visualizer = GetComponent<DetectionVisualizer>();

        // 필수 컴포넌트 체크
        if (modelManager == null)
        {
            Debug.LogError("TrashDetectorController: MLModelManager 컴포넌트가 필요합니다");
        }
        if (cameraCapture == null)
        {
            Debug.LogError("TrashDetectorController: CameraCapture 컴포넌트가 필요합니다");
        }
        if (detectionProcessor == null)
        {
            Debug.LogError("TrashDetectorController: DetectionProcessor 컴포넌트가 필요합니다");
        }
        if (visualizer == null)
        {
            Debug.LogError("TrashDetectorController: DetectionVisualizer 컴포넌트가 필요합니다");
        }

        // 이벤트 구독
        if (modelManager != null)
        {
            modelManager.OnModelLoaded += OnModelLoaded;
            modelManager.OnModelLoadError += OnModelLoadError;
        }
    }

    void Start()
    {
        // 모델이 이미 로드되어 있으면 초기화
        if (modelManager != null && modelManager.IsModelLoaded)
            OnModelLoaded();
    }

    private void OnModelLoaded()
    {
        if (debugMode)
        {
            Debug.Log("TrashDetectorController: 모델 로드 완료, 손 충돌 대기 중");
        }
    }

    private void OnModelLoadError(string error)
    {
        Debug.LogError($"TrashDetectorController: 모델 로드 실패 - {error}");
    }

    // 손 충돌 시 QuizManager에서 호출
    public void TriggerDetection()
    {

        if (isProcessing)
        {
            if (debugMode) Debug.Log("TrashDetectorController: 이미 처리 중입니다.");
            return;
        }

        if (modelManager == null || !modelManager.IsModelLoaded)
        {
            Debug.LogWarning("TrashDetectorController: 모델이 로드되지 않았습니다.");
            return;
        }

        // 기존 코루틴이 있으면 중지
        if (currentDetectionCoroutine != null)
        {
            StopCoroutine(currentDetectionCoroutine);
        }

        currentDetectionCoroutine = StartCoroutine(CaptureAndProcess());
    }

    private IEnumerator CaptureAndProcess()
    {
        isProcessing = true;

        yield return new WaitForEndOfFrame();

        Camera camera = cameraCapture?.GetCamera();
        if (camera == null)
        {
            Debug.LogError("TrashDetectorController: 카메라를 가져올 수 없습니다.");
            isProcessing = false;
            currentDetectionCoroutine = null;
            yield break;
        }

        // 이미지 캡처
        cameraCapture.CaptureImage(camera);

        // 이미지 처리
        try
        {
            ProcessImage(camera);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TrashDetectorController: 이미지 처리 중 오류 - {e.Message}\n{e.StackTrace}");
        }

        isProcessing = false;
        currentDetectionCoroutine = null;
    }

    private void ProcessImage(Camera camera)
    {
        if (debugMode) Debug.Log("TrashDetectorController: 이미지 처리 시작");

        Texture2D texture = cameraCapture.CurrentTexture;
        if (texture == null)
        {
            Debug.LogError("TrashDetectorController: 캡처된 텍스처가 null임");
            return;
        }

        Tensor inputTensor = null;
        Tensor outputTensor = null;

        try
        {
            // 입력 텐서 생성
            inputTensor = new Tensor(texture, 3);

            if (debugMode)
            {
                Debug.Log($"TrashDetectorController: 입력 텐서 크기 {inputTensor.shape}");
            }

            // 모델 실행
            outputTensor = modelManager.ExecuteModel(inputTensor);
            if (outputTensor == null)
            {
                Debug.LogError("TrashDetectorController: 모델 출력이 null입니다.");
                return;
            }

            if (debugMode)
            {
                Debug.Log($"TrashDetectorController: 출력 텐서 크기 {outputTensor.shape}");
            }

            // 감지 결과 처리
            var detections = detectionProcessor.ProcessDetectionResults(outputTensor);

            visualizer.VisualizeDetections(detections, camera, camera);
        }
        finally
        {
            inputTensor?.Dispose();
            outputTensor?.Dispose();
        }
    }

    void OnDestroy()
    {
        // 코루틴 중지
        if (currentDetectionCoroutine != null)
        {
            StopCoroutine(currentDetectionCoroutine);
            currentDetectionCoroutine = null;
        }

        // 이벤트 구독 해제
        if (modelManager != null)
        {
            modelManager.OnModelLoaded -= OnModelLoaded;
            modelManager.OnModelLoadError -= OnModelLoadError;
        }
    }
}