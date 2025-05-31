using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSnapper : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform snapTarget;
    public float snapThreshold = 0.3f;
    public bool isSnapped = false;
    public StoneSpawner spawner;

    void Update()
    {
        if (!isSnapped && Vector3.Distance(transform.position, snapTarget.position) < snapThreshold)
        {
            transform.position = snapTarget.position;
            transform.rotation = snapTarget.rotation;
            isSnapped = true;

            // ´ÙÀ½ ½ºÅæ ½ºÆù ¿äÃ»
            if (spawner != null)
            {
                spawner.SpawnNextStone();
            }

            // ¸ðµç ½ºÅæÀÌ ´Ù ½º³ÀµÆ´ÂÁö È®ÀÎ
        }
    }
}
