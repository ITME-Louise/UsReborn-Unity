using UnityEngine;
using Photon.Pun;

public class MonsterMovement : MonoBehaviourPun
{
    [Header("이동 설정")]
    [SerializeField] private float moveInterval = 2f;
    [SerializeField] private float moveRange = 1.5f;

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        startPosition = transform.position;
        timer = moveInterval;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // 순간이동!
            Vector3 randomOffset = new Vector3(
                Random.Range(-moveRange, moveRange),
                0f,
                Random.Range(-moveRange, moveRange)
            );
            transform.position = startPosition + randomOffset;
            timer = moveInterval;
        }
    }
}