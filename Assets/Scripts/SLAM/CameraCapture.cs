using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [SerializeField] private bool debugMode = true;

    private RenderTexture renderTexture;
    private Texture2D cameraTexture;
    private Camera cachedCamera;

    private const int CAPTURE_WIDTH = 640;
    private const int CAPTURE_HEIGHT = 640;

    public Texture2D CurrentTexture => cameraTexture;

    void Start()
    {
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        // OVRCameraRig 자동 찾기
        if (ovrCameraRig == null)
        {
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
            if (ovrCameraRig == null)
            {
                Debug.LogError("CameraCapture: OVRCameraRig를 찾을 수 없습니다");
            }
        }

        // RenderTexture 초기화
        renderTexture = new RenderTexture(CAPTURE_WIDTH, CAPTURE_HEIGHT, 24);
        cameraTexture = new Texture2D(CAPTURE_WIDTH, CAPTURE_HEIGHT, TextureFormat.RGB24, false);

        // 카메라 캐싱
        cachedCamera = FindCamera();
    }

    private Camera FindCamera()
    {
        // OVR 중앙 카메라 우선
        if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
        {
            Camera ovrCamera = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
            if (ovrCamera != null)
            {
                if (debugMode) Debug.Log("CameraCapture: OVR 중앙 카메라 사용");
                return ovrCamera;
            }
        }

        if (Camera.main != null)
        {
            if (debugMode) Debug.Log("CameraCapture: 메인 카메라 사용");
            return Camera.main;
        }

        Debug.LogError("CameraCapture: 카메라를 찾을 수 없습니다.");
        return null;
    }

    public Camera GetCamera()
    {
        // 캐시된 카메라 반환, null이면 다시 찾기
        if (cachedCamera == null)
        {
            cachedCamera = FindCamera();
        }
        return cachedCamera;
    }

    public void CaptureImage(Camera camera)
    {
        if (camera == null || renderTexture == null || cameraTexture == null) return;

        // 카메라 렌더링
        RenderTexture prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;

        // 텍스처 읽기
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        cameraTexture.ReadPixels(new Rect(0, 0, CAPTURE_WIDTH, CAPTURE_HEIGHT), 0, 0, false);
        cameraTexture.Apply();
        RenderTexture.active = prevActive;

        if (debugMode) Debug.Log("CameraCapture: 카메라 이미지 캡처 완료");
    }

    void OnDestroy()
    {
        // RenderTexture 정리
        if (renderTexture != null)
        {
            if (renderTexture.IsCreated())
            {
                renderTexture.Release();
            }
            Destroy(renderTexture);
            renderTexture = null;
        }

        // Texture2D 정리
        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
            cameraTexture = null;
        }

        cachedCamera = null;

        if (debugMode) Debug.Log("CameraCapture: 리소스 해제 완료");
    }
}
