using UnityEngine;

public class VRMenuButtonHandler : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[VRMenuButton] 초기화 완료");
    }

    void Update()
    {
        // Oculus (Meta Quest) - 오른쪽 컨트롤러 메뉴 버튼
        if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.RTouch))
        {
            OnMenuButtonPressed();
        }

        // 에디터 테스트용 - M 키
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("[VRMenuButton] M 키 감지 (Hub 복귀)");
            OnMenuButtonPressed();
        }
    }

    void OnMenuButtonPressed()
    {
        Debug.Log("[VRMenuButton] 메뉴 버튼 눌림!");

        // Hub에 있으면 무시
        if (GameSceneManager.Instance != null &&
            GameSceneManager.Instance.GetCurrentScene() == GameSceneManager.HUB_SCENE)
        {
            Debug.Log("[VRMenuButton] 이미 Hub에 있음");
            return;
        }

        // Hub로 복귀
        ReturnToHub();
    }

    void ReturnToHub()
    {
        Debug.Log("[VRMenuButton] Hub로 복귀 실행");

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.ReturnToHub();
        }
        else
        {
            Debug.LogError("[VRMenuButton] GameSceneManager를 찾을 수 없음!");
        }
    }
}