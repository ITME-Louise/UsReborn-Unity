using System.Collections;
using UnityEngine;

public class FoxAnicontroller : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private string triggerName = "Clapping";
    [SerializeField] private float delayToShow = 5f;

    [Header("UI Root")]
    [SerializeField] private string talkCanvasName = "TALK";
    [SerializeField] private GameObject talkCanvas;

    [Header("Normal Sequence (Children of TALK)")]
    [SerializeField] private string talk1Name = "foxtalk";
    [SerializeField] private string talk2Name = "foxtalk2";
    [SerializeField] private float talk1Duration = 3f;
    [SerializeField] private float talk2Duration = 2f;
    [SerializeField] private GameObject talk1;
    [SerializeField] private GameObject talk2;

    [Header("Success Sequence (Children of TALK)")]
    [SerializeField] private string successTalk1Name = "foxtalk_success1";
    [SerializeField] private string successTalk2Name = "foxtalk_success2";
    [SerializeField] private float successTalk1Duration = 3f;
    [SerializeField] private float successTalk2Duration = 2f;
    [SerializeField] private GameObject successTalk1;
    [SerializeField] private GameObject successTalk2;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (talkCanvas == null)
            talkCanvas = FindInSceneByName(talkCanvasName);

        if (talkCanvas != null)
        {
            if (talk1 == null) talk1 = FindChildByName(talkCanvas.transform, talk1Name);
            if (talk2 == null) talk2 = FindChildByName(talkCanvas.transform, talk2Name);
            if (successTalk1 == null) successTalk1 = FindChildByName(talkCanvas.transform, successTalk1Name);
            if (successTalk2 == null) successTalk2 = FindChildByName(talkCanvas.transform, successTalk2Name);

            talkCanvas.SetActive(false);
            if (talk1) talk1.SetActive(false);
            if (talk2) talk2.SetActive(false);
            if (successTalk1) successTalk1.SetActive(false);
            if (successTalk2) successTalk2.SetActive(false);
        }

        StartCoroutine(Flow());
    }

    // 기존 일반 대사 시퀀스
    private IEnumerator Flow()
    {
        yield return new WaitForSeconds(delayToShow);
        if (anim != null && !string.IsNullOrEmpty(triggerName))
            anim.SetTrigger(triggerName);

        if (talkCanvas != null)
        {
            talkCanvas.SetActive(true);

            if (talk1) talk1.SetActive(true);
            if (talk2) talk2.SetActive(false);
            yield return new WaitForSeconds(talk1Duration);

            if (talk1) talk1.SetActive(false);
            if (talk2) talk2.SetActive(true);
            yield return new WaitForSeconds(talk2Duration);

            if (talk2) talk2.SetActive(false);
            talkCanvas.SetActive(false);
        }
    }

    // 성공용 대사 시퀀스 추가
    public void PlaySuccessTalkSequence()
    {
        StartCoroutine(SuccessFlow());
    }

    private IEnumerator SuccessFlow()
    {
        // 1) 성공 시 "happy" 애니메이션 트리거 발동
        if (anim != null)
            anim.SetTrigger("happy");

        // 2) TALK 시퀀스: foxtalk_success1 → foxtalk_success2
        if (talkCanvas != null)
        {
            talkCanvas.SetActive(true);

            if (successTalk1) successTalk1.SetActive(true);
            if (successTalk2) successTalk2.SetActive(false);
            yield return new WaitForSeconds(successTalk1Duration);

            if (successTalk1) successTalk1.SetActive(false);
            if (successTalk2) successTalk2.SetActive(true);
            yield return new WaitForSeconds(successTalk2Duration);

            if (successTalk2) successTalk2.SetActive(false);
            talkCanvas.SetActive(false);
        }
    }

    // 기존 외부 재실행용 (일반 대사)
    public void PlayTalkSequenceAgain()
    {
        StartCoroutine(Flow());
    }

    private GameObject FindInSceneByName(string targetName)
    {
        var roots = gameObject.scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
                if (t.name == targetName) return t.gameObject;
        }
        return null;
    }

    private GameObject FindChildByName(Transform parent, string childName)
    {
        var all = parent.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
            if (t.name == childName) return t.gameObject;
        return null;
    }
}
