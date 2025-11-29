using UnityEngine;

public class HandCollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask trashLayerMask; // trash

    private GameObject currentTrash = null;

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

        // ML 인식 트리거
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.TriggerMLRecognition(currentTrash);
        }
        else
        {
            Debug.LogWarning("HandCollisionDetector: QuizManager.Instance가 null입니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentTrash)
        
        // 현재 잡고 있는 쓰레기가 손에서 벗어남
        if (other.gameObject == currentTrash)
        {
            Debug.Log($"HandCollisionDetector: 손에서 쓰레기 물체 분리됨 - {currentTrash.name}");
            currentTrash = null;
        }
    }
}