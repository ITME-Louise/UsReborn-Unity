using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonColliderCube : MonoBehaviour
{
    public GameObject visualCubePrefab;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 size = rt.rect.size;
        Vector3 worldScale = transform.lossyScale;

        // Create cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(transform, false);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = new Vector3(size.x * worldScale.x, size.y * worldScale.y, 1f);

        // Set transparent
        var renderer = cube.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.material.color = new Color(1, 0, 0, 0.2f); // 반투명 빨간색

        // Remove unnecessary components
        Destroy(cube.GetComponent<Rigidbody>());

    }
}

