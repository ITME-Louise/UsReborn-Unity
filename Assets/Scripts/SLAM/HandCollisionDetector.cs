using UnityEngine;
using System.Collections;

public class HandCollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask trashLayerMask;

    [SerializeField] private float minQuizDelay = 2.0f;
    [SerializeField] private float maxQuizDelay = 5.0f;

    [SerializeField] private float debounceDelay = 1.0f;

    private bool canTriggerQuiz = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canTriggerQuiz)
        {
            Debug.Log("HandCollisionDetector: 쿨다운 중이라 충돌 무시");
            return;
        }

        if (other == null) return;

        if ((trashLayerMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        TrashType trashType = other.GetComponent<TrashType>();
        if (trashType == null || string.IsNullOrEmpty(trashType.className))
        {
            Debug.LogWarning($"{other.name}에 TrashType 또는 className 없음");
            return;
        }

        if (QuizManager.Instance == null)
        {
            Debug.LogError("QuizManager.Instance가 null입니다!");
            return;
        }

        float randomDelay = Random.Range(minQuizDelay, maxQuizDelay);
        Debug.Log($"손-쓰레기 충돌! {randomDelay:F2}초 후 퀴즈 시작 예정");

        canTriggerQuiz = false;
        StartCoroutine(StartQuizWithDelay(randomDelay, trashType.className, other.gameObject));
    }

    private IEnumerator StartQuizWithDelay(float delay, string className, GameObject trashObject)
    {
        yield return new WaitForSeconds(delay);
        QuizManager.Instance.StartQuiz(className, trashObject);
    }

    public void OnQuizCompleted()
    {
        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(debounceDelay);
        canTriggerQuiz = true;
    }
}
