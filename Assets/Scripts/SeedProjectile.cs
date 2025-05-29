using UnityEngine;
using Photon.Pun;

public class SeedProjectile : MonoBehaviourPun
{
    void Start()
    {
        Debug.Log("SeedProjectile 생성됨 위치: " + transform.position);

        //  충돌 무시 설정 (플레이어의 모든 콜라이더와 충돌 무시)
        Collider seedCollider = GetComponent<Collider>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            // 플레이어 자신이 아닌 다른 플레이어는 무시하지 않도록 예외 처리
            if (photonView.Owner != null && player.GetComponent<PhotonView>()?.Owner != photonView.Owner)
                continue;

            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (var col in playerColliders)
            {
                Physics.IgnoreCollision(seedCollider, col);
            }
        }

        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        Debug.Log($"충돌 발생 → {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("몬스터에 맞음!");

            EnemyHP hp = collision.gameObject.GetComponent<EnemyHP>();
            if (hp != null)
            {
                hp.TakeDamage(20f);
                Debug.Log("몬스터 데미지 적용 완료");
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
