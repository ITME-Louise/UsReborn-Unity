using System.Collections;
using UnityEngine;

public class FoxAnicontroller : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator anim;                     // 비워두면 자동 GetComponent
    [SerializeField] private string triggerName = "Clapping";   // delay 후 실행할 트리거
    [SerializeField] private float delayToShow = 5f;            // 나타나기까지 대기(초)

    [Header("UI Root")]
    [SerializeField] private string talkCanvasName = "TALK";
    [SerializeField] private GameObject talkCanvas;             // 비워두면 이름으로 자동 탐색(비활성 포함)

    [Header("Sequence (Children of TALK)")]
    [SerializeField] private string talk1Name = "foxtalk";
    [SerializeField] private string talk2Name = "foxtalk2";
    [SerializeField] private float talk1Duration = 3f;          // foxtalk 표시 시간
    [SerializeField] private float talk2Duration = 2f;          // foxtalk2 표시 시간
    [SerializeField] private GameObject talk1;                  // 비워두면 자동 찾기
    [SerializeField] private GameObject talk2;                  // 비워두면 자동 찾기

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        // TALK 캔버스 자동 탐색(비활성 포함)
        if (talkCanvas == null)
            talkCanvas = FindInSceneByName(talkCanvasName);

        // 자식 요소 자동 탐색(비활성 포함)
        if (talkCanvas != null)
        {
            if (talk1 == null) talk1 = FindChildByName(talkCanvas.transform, talk1Name);
            if (talk2 == null) talk2 = FindChildByName(talkCanvas.transform, talk2Name);

            // 시작 시 모두 끄기
            talkCanvas.SetActive(false);
            if (talk1 != null) talk1.SetActive(false);
            if (talk2 != null) talk2.SetActive(false);
        }

        StartCoroutine(Flow());
    }

    private IEnumerator Flow()
    {
        // 1) 대기
        yield return new WaitForSeconds(delayToShow);

        // 2) 애니메이션 트리거
        if (anim != null && !string.IsNullOrEmpty(triggerName))
            anim.SetTrigger(triggerName);

        // 3) TALK 시퀀스: foxtalk(3s) → foxtalk2(2s)
        if (talkCanvas != null)
        {
            talkCanvas.SetActive(true);

            if (talk1 != null) talk1.SetActive(true);
            if (talk2 != null) talk2.SetActive(false);
            yield return new WaitForSeconds(talk1Duration);

            if (talk1 != null) talk1.SetActive(false);
            if (talk2 != null) talk2.SetActive(true);
            yield return new WaitForSeconds(talk2Duration);

            // 4) 종료: 전부 끄기
            if (talk2 != null) talk2.SetActive(false);
            talkCanvas.SetActive(false);
        }
    }

    // 씬 전체에서 이름으로 GameObject 찾기(비활성 포함)
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

    // 특정 부모 아래에서 이름으로 자식 찾기(비활성 포함)
    private GameObject FindChildByName(Transform parent, string childName)
    {
        var all = parent.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
            if (t.name == childName) return t.gameObject;
        return null;
    }
}
