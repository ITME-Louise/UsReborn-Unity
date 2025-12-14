using UnityEngine;
using System.Collections; // ← 추가

public class HandCollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask trashLayerMask; // trash
    [SerializeField] private float recognitionDelay = 1.5f;

    private GameObject currentTrash = null;
    private Coroutine recognitionCoroutine = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        // 이미 쓰레기를 잡고 있으면 무시
        if (currentTrash != null) return;
        // LayerMask 체크
        int otherLayer = other.gameObject.layer;
        if ((trashLayerMask.value & (1 << otherLayer)) == 0) return;

        // 쓰레기 감지
        currentTrash = other.gameObject;
        Debug.Log($"HandCollisionDetector: 손-쓰레기 충돌 인식 - {currentTrash.name}");

        // 코루틴 시작 (딜레이 후 퀴즈)
        if (recognitionCoroutine != null)
        {
            StopCoroutine(recognitionCoroutine);
        }
        recognitionCoroutine = StartCoroutine(RecognizeTrashWithDelay());
    }

    private IEnumerator RecognizeTrashWithDelay()
    {
        Debug.Log($"HandCollisionDetector: 쓰레기 인식 중... ({recognitionDelay}초)");

        // 딜레이
        yield return new WaitForSeconds(recognitionDelay);

        // 딜레이 후에도 여전히 충돌 중인지 확인
        if (currentTrash == null)
        {
            Debug.Log("HandCollisionDetector: 인식 중 손이 떨어짐");
            yield break;
        }

        // TrashType에서 직접 className 가져오기
        TrashType trashType = currentTrash.GetComponent<TrashType>();
        if (trashType != null && !string.IsNullOrEmpty(trashType.className))
        {
            Vector3 trashPos = currentTrash.transform.position;
            if (QuizManager.Instance != null)
            {
                // 직접 퀴즈 시작
                Debug.Log($"HandCollisionDetector: 인식 완료! 퀴즈 시작 - {trashType.className}");
                QuizManager.Instance.StartQuiz(trashType.className, currentTrash);
            }
            else
            {
                Debug.LogWarning("HandCollisionDetector: QuizManager.Instance가 null입니다.");
            }
        }
        else
        {
            Debug.LogWarning($"HandCollisionDetector: {currentTrash.name}에 TrashType 컴포넌트가 없거나 className이 비어있습니다.");
        }

        recognitionCoroutine = null;

        /* 기존 코드
        // ML 인식 트리거
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.TriggerMLRecognition(currentTrash);
        }
        else
        {
            Debug.LogWarning("HandCollisionDetector: QuizManager.Instance가 null입니다.");
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        // 현재 잡고 있는 쓰레기가 손에서 벗어남
        if (other.gameObject == currentTrash)
        {
            Debug.Log($"HandCollisionDetector: 손에서 쓰레기 물체 분리됨 - {currentTrash.name}");

            // 인식 중이었다면 코루틴 중지
            if (recognitionCoroutine != null)
            {
                StopCoroutine(recognitionCoroutine);
                recognitionCoroutine = null;
            }

            currentTrash = null;
        }
    }
}