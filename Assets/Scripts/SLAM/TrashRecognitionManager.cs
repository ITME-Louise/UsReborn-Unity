using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashRecognitionManager : MonoBehaviour
{
    public static TrashRecognitionManager Instance;
    [SerializeField] private QuizManager quizManager;

    private void Awake()
    {
        Instance = this;
    }

    public void OnTrashPicked(GameObject trashObj)
    {
        var trashType = trashObj.GetComponent<TrashType>();
        if (trashType == null) return;

        string className = trashType.className;
        Vector3 spawnPos = trashObj.transform.position;
        quizManager?.StartQuiz(className, spawnPos);
    }
}
