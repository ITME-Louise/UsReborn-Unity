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
        modelManager = GetComponent<MLModelManager>();
        cameraCapture = GetComponent<CameraCapture>();
        detectionProcessor = GetComponent<DetectionProcessor>();
        visualizer = GetComponent<DetectionVisualizer>();

        // OVR Camera Rig에서 Center Eye Anchor 찾기
        var ovrRig = GameObject.Find("OVRCameraRig");
        if (ovrRig != null)
        {
            var centerEyeAnchor = ovrRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            if (centerEyeAnchor != null)
                vrCamera = centerEyeAnchor.GetComponent<Camera>();
        }

        if (vrCamera == null)
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam.name.Contains("CenterEyeAnchor") || cam.name.Contains("Eye"))
                {
                    vrCamera = cam;
                    break;
                }
            }
        }

        if (vrCamera == null)
            Debug.LogError("TrashDetectorController: VR 카메라를 찾을 수 없습니다.");
        else if (debugMode)
            Debug.Log($"TrashDetectorController: VR 카메라 찾음: {vrCamera.name}");

        trashLayerMask = LayerMask.GetMask("trash");

        if (modelManager == null) Debug.LogError("MLModelManager 컴포넌트가 필요합니다!");
        if (cameraCapture == null) Debug.LogError("CameraCapture 컴포넌트가 필요합니다!");
        if (detectionProcessor == null) Debug.LogError("DetectionProcessor 컴포넌트가 필요합니다!");
        if (visualizer == null) Debug.LogError("DetectionVisualizer 컴포넌트가 필요합니다!");

        if (modelManager != null)
        {
            modelManager.OnModelLoaded += OnModelLoaded;
            modelManager.OnModelLoadError += OnModelLoadError;
        }
    }

    void Start()
    {
        if (modelManager != null && modelManager.IsModelLoaded)
            OnModelLoaded();
    }

    private void OnModelLoaded()
    {
        if (!IsInvoking(nameof(DetectTrash)))
            InvokeRepeating(nameof(DetectTrash), 3f, 3f);
        if (debugMode) Debug.Log("TrashDetectorController: 감지 시작");
    }

    private void OnModelLoadError(string error)
    {
        Debug.LogError($"TrashDetectorController: 모델 로드 실패 - {error}");
    }

    private void DetectTrash()
    {
        if (isProcessing || !modelManager.IsModelLoaded) return;
        isProcessing = true;
        StartCoroutine(CaptureAndProcess());
    }

    private IEnumerator CaptureAndProcess()
    {
        yield return new WaitForEndOfFrame();

        var camera = cameraCapture.GetCamera();
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
            Debug.LogError($"TrashDetectorController: 이미지 처리 중 오류 - {e.Message}");
        }

        isProcessing = false;
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

        using (var inputTensor = new Tensor(texture, 3))
        {
            if (debugMode)
                Debug.Log($"입력 텐서 크기 {inputTensor.shape}");

            var outputTensor = modelManager.ExecuteModel(inputTensor);
            if (outputTensor == null)
            {
                Debug.LogError("TrashDetectorController: 모델 출력이 null");
                return;
            }

            if (debugMode)
                Debug.Log($"출력 텐서 크기 {outputTensor.shape}");

            var detections = detectionProcessor.ProcessDetectionResults(outputTensor);

            visualizer.VisualizeDetectionsWithRaycast(detections, camera, vrCamera, trashLayerMask);

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
