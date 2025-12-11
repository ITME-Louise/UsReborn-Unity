using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [SerializeField] private bool debugMode = true;

    private RenderTexture renderTexture;
    private Texture2D cameraTexture;

    private int captureWidth = 640;
    private int captureHeight = 640;

    public Texture2D CurrentTexture => cameraTexture;

    void Start()
    {
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        if (ovrCameraRig == null)
        {
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
            if (ovrCameraRig == null)
                Debug.LogError("CameraCapture: OVRCameraRig를 찾을 수 없습니다!");
            else if (debugMode)
                Debug.Log("CameraCapture: OVRCameraRig 자동 찾기 성공");
        }

        renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        renderTexture.Create();
        cameraTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
    }

    public Camera GetCamera()
    {
        if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
        {
            if (debugMode) Debug.Log("CameraCapture: OVR 중앙 카메라 사용");
            return ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
        }

        if (Camera.main == null)
        {
            Debug.LogError("CameraCapture: 카메라를 찾을 수 없습니다!");
            return null;
        }

        if (debugMode) Debug.Log("CameraCapture: 메인 카메라 사용");
        return Camera.main;
    }

    public void CaptureImage(Camera camera)
    {
        var prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;

        RenderTexture.active = renderTexture;
        cameraTexture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        cameraTexture.Apply();
        RenderTexture.active = null;

        if (debugMode) Debug.Log("CameraCapture: 카메라 이미지 캡처 완료");
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (cameraTexture != null)
        {
            Destroy(cameraTexture);
        }
        if (debugMode) Debug.Log("CameraCapture: 리소스 해제 완료");
    }
}
