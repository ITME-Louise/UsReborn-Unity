using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class PlayerHP : MonoBehaviourPun
{
    public Canvas hpCanvas;             // 머리 위 World Space Canvas
    public Slider hpSlider;             // 머리 위 슬라이더
    private Slider myHPSlider;          // 내 화면용 UI 슬라이더 (스크린 공간)

    public int maxHp = 100;
    public int currentHp;

    private Vector3 lastPosition;       // 죽기 직전 위치 저장

    void Start()
    {
        currentHp = maxHp;

        // 머리 위 HP 슬라이더 세팅
        if (hpSlider != null)
            hpSlider.maxValue = maxHp;

        // 머리 위 HP Canvas 자동 연결
        if (hpCanvas == null)
            hpCanvas = GetComponentInChildren<Canvas>();

        if (photonView.IsMine)
        {
            // 머리 위 HP는 내 화면에선 안 보이게
            if (hpCanvas != null)
                hpCanvas.gameObject.SetActive(false);

            // 내 화면용 UI 슬라이더 자동 연결
            GameObject mySliderObj = GameObject.Find("MyHPSlider");
            if (mySliderObj != null)
            {
                myHPSlider = mySliderObj.GetComponent<Slider>();
                myHPSlider.maxValue = maxHp;
            }
        }

        // World Space Canvas에 카메라 자동 할당
        if (hpCanvas != null &&
            hpCanvas.renderMode == RenderMode.WorldSpace &&
            hpCanvas.worldCamera == null)
        {
            Camera centerEyeCam = GameObject.Find("CenterEyeAnchor")?.GetComponent<Camera>();
            if (centerEyeCam != null)
                hpCanvas.worldCamera = centerEyeCam;
        }

        // 모든 연결 끝난 뒤 UI 반영
        UpdateUI();
    }

    public void TakeDamage(int dmg)
    {
        if (!photonView.IsMine) return;

        currentHp -= dmg;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateUI();

        if (currentHp == 0)
        {
            lastPosition = transform.position; // 죽기 직전 위치 저장
            StartCoroutine(RespawnAfterDelay(3f));
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null)
            hpSlider.value = currentHp;

        if (photonView.IsMine && myHPSlider != null)
            myHPSlider.value = currentHp;
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        // 사망 시 처리 (원한다면 여기서 숨기기 가능)
        gameObject.SetActive(false);

        yield return new WaitForSeconds(delay);

        // 리스폰: 위치 복원 + 체력 회복
        transform.position = lastPosition;
        currentHp = maxHp;
        UpdateUI();
        gameObject.SetActive(true);
    }
}
