using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;

[System.Serializable]
public class Detection
{
    public Rect BoundingBox;
    public float Confidence;
    public int ClassIndex;
    public string ClassName;
}

public class DetectionProcessor : MonoBehaviour
{
    [SerializeField] private float confidenceThreshold = 0.7f;
    [SerializeField] private bool debugMode = true;

    private string[] classNames = {
        "paper", "pack", "can", "glass", "pet", "plastic", "vinyl"
    };
    private const int INPUT_WIDTH = 640;
    private const int INPUT_HEIGHT = 640;
    private const float OBJECTNESS_THRESHOLD = 0.6f;
    private const float NMS_IOU_THRESHOLD = 0.45f;
    private const float MIN_BOX_SIZE = 10f;
    private const float MAX_BOX_SIZE_RATIO = 0.8f;

    private float[] classScoresBuffer;

    private void Awake()
    {
        classScoresBuffer = new float[classNames.Length];
    }

    public List<Detection> ProcessDetectionResults(Tensor outputTensor)
    {
        if (outputTensor == null)
        {
            Debug.LogError("DetectionProcessor: outputTensor is null");
            return new List<Detection>();
        }

        int valuesPerBox = outputTensor.width;
        int numBBoxes = outputTensor.channels;

        if (debugMode)
            Debug.Log($"DetectionProcessor: numBBoxes={numBBoxes}, valuesPerBox={valuesPerBox}");

        int expectedValuesPerBox = 5 + classNames.Length;
        if (valuesPerBox != expectedValuesPerBox)
        {
            Debug.LogWarning($"DetectionProcessor: valuesPerBox ({valuesPerBox})와 기대값({expectedValuesPerBox}) 불일치");
        }

        List<Detection> detections = new List<Detection>(numBBoxes / 10);

        for (int i = 0; i < numBBoxes; i++)
        {
            float objectness = outputTensor[0, 0, 4, i];

            if (objectness <= OBJECTNESS_THRESHOLD) continue;

            float x = outputTensor[0, 0, 0, i];
            float y = outputTensor[0, 0, 1, i];
            float w = outputTensor[0, 0, 2, i];
            float h = outputTensor[0, 0, 3, i];

            // 박스 크기 검증
            if (w < MIN_BOX_SIZE || h < MIN_BOX_SIZE ||
                w > INPUT_WIDTH * MAX_BOX_SIZE_RATIO || h > INPUT_HEIGHT * MAX_BOX_SIZE_RATIO ||
                x <= 0 || y <= 0 || x >= INPUT_WIDTH || y >= INPUT_HEIGHT)
            {
                continue;
            }

            // 클래스 스코어 계산
            float bestScore = 0f;
            int bestClassIdx = 0;

            for (int c = 0; c < classNames.Length; c++)
            {
                float score = outputTensor[0, 0, 5 + c, i];
                classScoresBuffer[c] = score;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestClassIdx = c;
                }
            }

            float confidence = objectness * bestScore;

            if (debugMode)
                Debug.Log($"Box[{i}] confidence={confidence:F3}");

            if (confidence > confidenceThreshold)
            {
                // Bounding Box 정규화
                float normalizedX = Mathf.Clamp01((x - w * 0.5f) / INPUT_WIDTH);
                float normalizedY = Mathf.Clamp01((y - h * 0.5f) / INPUT_HEIGHT);
                float normalizedW = Mathf.Clamp01(w / INPUT_WIDTH);
                float normalizedH = Mathf.Clamp01(h / INPUT_HEIGHT);

                detections.Add(new Detection
                {
                    BoundingBox = new Rect(normalizedX, normalizedY, normalizedW, normalizedH),
                    Confidence = confidence,
                    ClassIndex = bestClassIdx,
                    ClassName = classNames[bestClassIdx]
                });
            }
        }

        if (debugMode)
            Debug.Log($"DetectionProcessor: 총 {detections.Count}개 감지됨");

        return ApplyNMS(detections, NMS_IOU_THRESHOLD);
    }

    private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
    {
        if (detections.Count == 0) return detections;

        // 신뢰도 내림차순 정렬
        detections.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

        List<Detection> selectedDetections = new List<Detection>();

        while (detections.Count > 0)
        {
            Detection current = detections[0];
            selectedDetections.Add(current);
            detections.RemoveAt(0);

            // IoU가 임계값 이상인 박스 제거
            detections.RemoveAll(d => CalculateIoU(current.BoundingBox, d.BoundingBox) > iouThreshold);
        }

        if (debugMode)
            Debug.Log($"DetectionProcessor: NMS 후 {selectedDetections.Count}개 감지");

        return selectedDetections;
    }

    private float CalculateIoU(Rect a, Rect b)
    {
        float xOverlap = Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
        float intersection = xOverlap * yOverlap;

        if (intersection == 0) return 0f;

        float union = a.width * a.height + b.width * b.height - intersection;
        return union > 0 ? intersection / union : 0f;
    }
}
