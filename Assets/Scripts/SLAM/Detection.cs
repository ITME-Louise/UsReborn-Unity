using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Detection
{
    public Rect BoundingBox;
    public float Confidence;
    public int ClassIndex;
    public string ClassName;
}
