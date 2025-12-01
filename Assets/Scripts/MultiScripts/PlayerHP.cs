using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class PlayerHP : MonoBehaviourPun, IPunObservable
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

        if (hpSlider != null)
            hpSlider.maxValue = maxHp;

        if (hpCanvas == null)
            hpCanvas = GetComponentInChildren<Canvas>();

        if (photonView.IsMine)
        {
            if (hpCanvas != null)
                hpCanvas.gameObject.SetActive(false);

            GameObject mySliderObj = GameObject.Find("MyHPSlider");
            if (mySliderObj != null)
            {
                myHPSlider = mySliderObj.GetComponent<Slider>();
                myHPSlider.maxValue = maxHp;
            }
        }

        if (hpCanvas != null &&
            hpCanvas.renderMode == RenderMode.WorldSpace &&
            hpCanvas.worldCamera == null)
        {
            Camera centerEyeCam = GameObject.Find("CenterEyeAnchor")?.GetComponent<Camera>();
            if (centerEyeCam != null)
                hpCanvas.worldCamera = centerEyeCam;
        }

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
            lastPosition = transform.position;
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
        gameObject.SetActive(false);

        yield return new WaitForSeconds(delay);

        transform.position = lastPosition;
        currentHp = maxHp;
        UpdateUI();
        gameObject.SetActive(true);
    }

    // HP 동기화용
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내가 소유한 캐릭터일 때 현재 HP를 네트워크로 보냄
            stream.SendNext(currentHp);
        }
        else
        {
            // 다른 클라이언트에서 온 HP 값을 수신
            currentHp = (int)stream.ReceiveNext();
            UpdateUI();
        }
    }
}
