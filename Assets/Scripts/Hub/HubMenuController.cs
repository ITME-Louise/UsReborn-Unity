using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HubMenuController : MonoBehaviour
{
    public GameObject minigameSelectPanel;
    public GameObject mainMenuPanel;

    private bool isCanvasSetup = false;

    void Start()
    {
        StartCoroutine(SetupCanvasDelayed());
    }

    void Update()
    {
        if (isCanvasSetup && Input.GetMouseButtonDown(0))
        {
            CheckUIClick();
        }
    }

    void CheckUIClick()
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log("[HubMenu] UI Raycast 결과: " + results.Count + "개");

        foreach (RaycastResult result in results)
        {
            Debug.Log("[HubMenu] UI Hit: " + result.gameObject.name + " (Layer: " + LayerMask.LayerToName(result.gameObject.layer) + ")");

            Button button = result.gameObject.GetComponent<Button>();
            if (button == null)
            {
                button = result.gameObject.GetComponentInParent<Button>();
            }

            if (button != null)
            {
                string buttonName = button.gameObject.name;
                Debug.Log("[HubMenu] 버튼 클릭: " + buttonName);

                switch (buttonName)
                {
                    case "Button_Minigame":
                        OnMinigameButtonClick();
                        return;
                    case "Button_SlamScene":
                        OnSlamSceneButtonClick();
                        return;
                    case "Button_Multiplay":
                        OnMultiplayButtonClick();
                        return;
                    case "Button_Quit":
                        OnQuitButtonClick();
                        return;
                    case "Button_FoxGame":
                        OnFoxGameButtonClick();
                        return;
                    case "Button_RabbitGame":
                        OnRabbitGameButtonClick();
                        return;
                    case "Button_Back":
                        OnBackButtonClick();
                        return;
                }
            }
        }
    }

    private IEnumerator SetupCanvasDelayed()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Camera mainCam = Camera.main;

        if (mainCam == null)
        {
            GameObject centerEye = GameObject.Find("CenterEyeAnchor");
            if (centerEye != null)
            {
                mainCam = centerEye.GetComponent<Camera>();
            }
        }

        if (mainCam == null)
        {
            Debug.LogError("[HubMenu] 카메라를 찾을 수 없습니다!");
            yield break;
        }

        Debug.Log("[HubMenu] 카메라 찾음: " + mainCam.name);

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                if (canvas.name == "MainMenuCanvas")
                {
                    canvas.worldCamera = mainCam;

                    Debug.Log("[HubMenu] Canvas 설정: " + canvas.name);

                    GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (raycaster == null)
                    {
                        raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                        Debug.Log("[HubMenu] GraphicRaycaster 추가");
                    }

                    Transform camTransform = mainCam.transform;
                    canvas.transform.position = camTransform.position + camTransform.forward * 1f;
                    canvas.transform.rotation = camTransform.rotation;
                    //canvas.transform.position += canvas.transform.up * -0.9f;

                    Debug.Log("[HubMenu] Canvas 위치: " + canvas.transform.position);
                }
            }
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);

        Debug.Log("[HubMenu] 초기화 완료");
        isCanvasSetup = true;
    }

    public void OnMinigameButtonClick()
    {
        Debug.Log("[HubMenu] 미니게임 메뉴 열기");
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(true);
    }

    public void OnSlamSceneButtonClick()
    {
        Debug.Log("[HubMenu] Slam Scene으로 이동");
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
        GameSceneManager.Instance.LoadSceneAdditive(GameSceneManager.SLAM_SCENE);
    }

    public void OnMultiplayButtonClick()
    {
        Debug.Log("[HubMenu] Multi Scene으로 이동");
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
        GameSceneManager.Instance.LoadSceneAdditive(GameSceneManager.MULTI_SCENE);
    }

    public void OnQuitButtonClick()
    {
        Debug.Log("[HubMenu] 게임 종료");
        GameSceneManager.Instance.QuitGame();
    }

    public void OnFoxGameButtonClick()
    {
        Debug.Log("[HubMenu] 여우 게임 시작");
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
        GameSceneManager.Instance.LoadSceneAdditive(GameSceneManager.FOX_GAME);
        CloseMinigameMenu();
    }

    public void OnRabbitGameButtonClick()
    {
        Debug.Log("[HubMenu] 토끼 게임 시작");
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
        GameSceneManager.Instance.LoadSceneAdditive(GameSceneManager.RABBIT_GAME);
        CloseMinigameMenu();
    }

    public void OnBackButtonClick()
    {
        Debug.Log("[HubMenu] 미니게임 메뉴 닫기");
        CloseMinigameMenu();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    private void CloseMinigameMenu()
    {
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
    }

    void OnDisable()
    {
        if (minigameSelectPanel != null)
            minigameSelectPanel.SetActive(false);
    }
}
