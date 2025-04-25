using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField nicknameInput;
    public GameObject nicknamePanel;  // 닉네임 입력 패널 연결
    public Camera_Moving cameraController; // 카메라 제어 연결
    // Start is called before the first frame update
    void Start()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogues[currentIndex];
        nicknamePanel.SetActive(false); // 닉네임 패널은 처음엔 숨김
        cameraController.canMove = true; // 처음엔 카메라 움직일 수 있음
    }

    private string[] dialogues = new string[5]
    {
        "오, 드디어 일어났네? 첫 출근에 늦잠 잘까 봐 걱정했어.",
        "난 레온 대리! 앞으로 네 멘토가 될 사람이지.",
        "여긴 우주 환경 복구국 US-Reborn.",
        "우주에 퍼진 오염을 정화하고 생태계를 복구하는 곳이야.",
        "먼저, 사원증을 만들어 볼까?",
    };

    private string[] secondDialogues = new string[5]
    {   
    "좋아, 이제부터 넌 정식 US-Reborn의 인턴이야!",
    "하지만 아직 멀었어.",
    "일단 기초부터 배우자고!",
    "여긴 네 개인 우주선 방이야. 개인 기지라고 생각하면 돼.",
    "우선, 창 밖으로 보이는 EARTH에 가보도록 할까?"

    };

    private int currentIndex = 0;
    private bool isSecondPhase = false;  // 두 번째 대사 출력인지 여부


    public void OnDialogueClick()
    {
        currentIndex++;
        if (!isSecondPhase)
        {
            if (currentIndex < dialogues.Length)
            {
                dialogueText.text = dialogues[currentIndex];
            }
            else
            {
                dialoguePanel.SetActive(false);
                nicknamePanel.SetActive(true);
                cameraController.canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            if (currentIndex < secondDialogues.Length)
            {
                dialogueText.text = secondDialogues[currentIndex];
            }
            else
            {
                dialoguePanel.SetActive(false);
                Debug.Log("두 번째 대사까지 모두 완료!");
                // SceneManager.LoadScene("NextScene");
            }
        }
    }

    public void OnNicknameSubmit()
    {
        string nickname = nicknameInput.text;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            Debug.LogWarning("닉네임을 입력");
            return;
        }

        PlayerPrefs.SetString("PlayerNickname", nickname);
        Debug.Log("닉네임 저장됨: " + nickname);

        nicknamePanel.SetActive(false);
        dialoguePanel.SetActive(true);

        currentIndex = 0;
        isSecondPhase = true;
        dialogueText.text = secondDialogues[currentIndex];

        cameraController.canMove = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // SceneManager.LoadScene("NextScene");
}
