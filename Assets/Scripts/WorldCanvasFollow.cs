using UnityEngine;

public class WorldCanvasFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (보통 Player)
    public Vector3 offset = new Vector3(0, 2f, 0); // 머리 위 위치

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();

        // 현재 존재하는 카메라 중 "PlayerCamera" 태그/이름을 찾아서 연결
        Camera cam = Camera.main; // 또는 GameObject.Find("PlayerCamera").GetComponent<Camera>();
        if (canvas != null && cam != null)
        {
            canvas.worldCamera = cam;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(Camera.main.transform); // 항상 카메라 바라보게
        }
    }
}
