using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class TrashDetectorController : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private LayerMask trashLayerMask = -1;

    private MLModelManager modelManager;
    private CameraCapture cameraCapture;
    private DetectionProcessor detectionProcessor;
    private DetectionVisualizer visualizer;
    private Camera vrCamera;

    private bool isProcessing = false;

    void Awake()
    {
        Debug.Log("TrashDetector: 초기화 시작");

        modelManager = GetComponent<MLModelManager>();
        cameraCapture = GetComponent<CameraCapture>();
        detectionProcessor = GetComponent<DetectionProcessor>();
        visualizer = GetComponent<DetectionVisualizer>();

        // OVR Camera Rig에서 Center Eye Anchor 찾기
        GameObject ovrCameraRig = GameObject.Find("OVRCameraRig");
        if (ovrCameraRig != null)
        {
            Transform centerEyeAnchor = ovrCameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            if (centerEyeAnchor != null)
            {
                vrCamera = centerEyeAnchor.GetComponent<Camera>();
            }
        }

        // OVRCameraRig가 없으면 다른 방법으로 찾기
        if (vrCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.name.Contains("CenterEyeAnchor") || cam.name.Contains("Eye"))
                {
                    vrCamera = cam;
                    break;
                }
            }
        }

        if (vrCamera == null)
        {
            Debug.LogError("TrashDetectorController: OVR 카메라를 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log($"TrashDetectorController: 카메라 찾음 - {vrCamera.name}");
        }

        // trash 레이어를 LayerMask로 설정
        trashLayerMask = LayerMask.GetMask("trash");

        if (modelManager == null) Debug.LogError("TrashDetector: MLModelManager 컴포넌트가 필요합니다!");
        if (cameraCapture == null) Debug.LogError("TrashDetector: CameraCapture 컴포넌트가 필요합니다!");
        if (detectionProcessor == null) Debug.LogError("TrashDetector: DetectionProcessor 컴포넌트가 필요합니다!");
        if (visualizer == null) Debug.LogError("TrashDetector: DetectionVisualizer 컴포넌트가 필요합니다!");

        if (modelManager != null)
        {
            modelManager.OnModelLoaded += OnModelLoaded;
            modelManager.OnModelLoadError += OnModelLoadError;
            Debug.Log("TrashDetector: 이벤트 구독 완료");
        }
    }

    void Start()
    {
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

        if (!IsInvoking("DetectTrash"))
        {
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

        Camera camera = cameraCapture.GetCamera();
        if (camera == null)
        {
            isProcessing = false;
            yield break;
        }

        cameraCapture.CaptureImage(camera);

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

        Texture2D capturedTexture = cameraCapture.GetCapturedTexture();

        if (capturedTexture == null)
        {
            Debug.LogError("TrashDetector: 캡처된 텍스처가 null입니다!");
            return;
        }

        using (var inputTensor = new Tensor(capturedTexture, channels: 3))
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

            // 레이캐스트 방식으로 시각화
            visualizer.VisualizeDetectionsWithRaycast(detections, camera, vrCamera, trashLayerMask);

            // 텐서 해제
            outputTensor.Dispose();
        }
    }

    void OnDestroy()
    {
        CancelInvoke();

        if (modelManager != null)
        {
            modelManager.OnModelLoaded -= OnModelLoaded;
            modelManager.OnModelLoadError -= OnModelLoadError;
        }
    }
}