using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHP : MonoBehaviourPun
{
    public Canvas hpCanvas;
    public Slider hpSlider;

    public int maxHp = 100;
    public int currentHp;

    void Start()
    {
        currentHp = maxHp;
        UpdateUI();

        //  자동 연결 (Editor에서 드래그할 필요 없이)
        if (hpCanvas == null)
            hpCanvas = GetComponentInChildren<Canvas>();

        if (photonView.IsMine && hpCanvas != null)
        {
            hpCanvas.gameObject.SetActive(false); // 내 HP바는 안 보이게
        }

        //  World Space Canvas의 Event Camera 설정
        if (hpCanvas.renderMode == RenderMode.WorldSpace && hpCanvas.worldCamera == null)
        {
            Camera centerEyeCam = GameObject.Find("CenterEyeAnchor")?.GetComponent<Camera>();
            if (centerEyeCam != null)
            {
                hpCanvas.worldCamera = centerEyeCam;
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hpSlider != null)
            hpSlider.value = (float)currentHp / maxHp;
    }
}
