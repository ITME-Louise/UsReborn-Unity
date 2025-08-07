using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;

public class DetectionProcessor : MonoBehaviour
{
    [SerializeField] private float confidenceThreshold = 0.7f;
    [SerializeField] private bool debugMode = true;

    private string[] classNames = {
        "paper", "pack", "can", "glass", "pet", "plastic", "vinyl"
    };
    private int inputWidth = 640;
    private int inputHeight = 640;

    public List<Detection> ProcessDetectionResults(Tensor outputTensor)
    {
        int valuesPerBox = outputTensor.width;
        int numBBoxes = outputTensor.channels;

        if (debugMode)
            Debug.Log($"DetectionProcessor: numBBoxes={numBBoxes}, valuesPerBox={valuesPerBox}");

        int expectedValuesPerBox = 5 + classNames.Length;
        if (valuesPerBox != expectedValuesPerBox)
            Debug.LogWarning($"DetectionProcessor: valuesPerBox ({valuesPerBox}) 와 기대값({expectedValuesPerBox}) 불일치");

        List<Detection> detections = new List<Detection>();

        for (int i = 0; i < numBBoxes; i++)
        {
            float x = outputTensor[0, 0, 0, i];
            float y = outputTensor[0, 0, 1, i];
            float w = outputTensor[0, 0, 2, i];
            float h = outputTensor[0, 0, 3, i];
            float objectness = outputTensor[0, 0, 4, i];

            if (objectness > 0.6f)
            {
                float bestScore = 0f;
                int bestClassIdx = 0;
                float[] classScores = new float[classNames.Length];
                for (int c = 0; c < classNames.Length; c++)
                {
                    float score = outputTensor[0, 0, 5 + c, i];
                    classScores[c] = score;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestClassIdx = c;
                    }
                }

                float confidence = objectness * bestScore;

                if (debugMode)
                    Debug.Log($"Box[{i}] confidence={confidence:F3}");

                if (confidence > confidenceThreshold &&
                    w > 10 && h > 10 &&
                    w < inputWidth * 0.8f && h < inputHeight * 0.8f &&
                    x > 0 && y > 0 && x < inputWidth && y < inputHeight)
                {
                    var detection = new Detection()
                    {
                        BoundingBox = new Rect(
                            Mathf.Clamp01((x - w / 2) / inputWidth),
                            Mathf.Clamp01((y - h / 2) / inputHeight),
                            Mathf.Clamp01(w / inputWidth),
                            Mathf.Clamp01(h / inputHeight)),

                        Confidence = confidence,
                        ClassIndex = bestClassIdx,
                        ClassName = classNames[bestClassIdx]
                    };
                    detections.Add(detection);
                }
            }
        }

        if (debugMode)
            Debug.Log($"DetectionProcessor: 총 {detections.Count}개 감지됨");

        return ApplyNMS(detections, 0.45f);
    }

    private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
    {
        var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
        List<Detection> selectedDetections = new List<Detection>();

        while (sortedDetections.Count > 0)
        {
            var current = sortedDetections[0];
            selectedDetections.Add(current);
            sortedDetections.RemoveAt(0);
            sortedDetections.RemoveAll(d => IoU(current.BoundingBox, d.BoundingBox) > iouThreshold);
        }

        if (debugMode)
            Debug.Log($"DetectionProcessor: NMS 후 {selectedDetections.Count}개 감지");

        return selectedDetections;
    }

    private float IoU(Rect a, Rect b)
    {
        float xOverlap = Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
        float intersection = xOverlap * yOverlap;
        float union = a.width * a.height + b.width * b.height - intersection;
        return union > 0 ? intersection / union : 0;
    }
}
