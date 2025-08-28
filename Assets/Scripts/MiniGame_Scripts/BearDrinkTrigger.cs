using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class BearDrinkTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    private bool bucketInside = false;   // 버킷이 트리거 안에 있는지
    private Bucket currentBucket = null; // 현재 들어온 버킷 참조

    private int receivedBucketCount = 0; // 이 곰이 받은 버킷 개수

    private void Update()
    {
        // 트리거 안에 버킷이 있고, 그 버킷이 "채워진 상태 + 손에서 놓인 상태"라면 받기 처리
        if (bucketInside && currentBucket != null)
        {
            if (currentBucket.IsFilled && !currentBucket.IsGrabbed)
            {
                receivedBucketCount++;
                Debug.Log($"{gameObject.name} 곰이 채워진 버킷을 받음! 현재 받은 개수: {receivedBucketCount}");

                Destroy(currentBucket.gameObject); // 버킷 제거

                // 이 곰이 버킷을 3개 이상 받았으면 게임 성공 처리
                if (receivedBucketCount >= 3)
                {
                    MiniGameManager_Bear.Instance.OnBearSatisfied();
                }

                // 상태 초기화
                bucketInside = false;
                currentBucket = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bucket"))
        {
            Bucket bucket = other.GetComponent<Bucket>();
            if (bucket != null)
            {
                bucketInside = true;
                currentBucket = bucket;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bucket"))
        {
            if (currentBucket != null && other.gameObject == currentBucket.gameObject)
            {
                bucketInside = false;
                currentBucket = null;
            }
        }
    }
}
