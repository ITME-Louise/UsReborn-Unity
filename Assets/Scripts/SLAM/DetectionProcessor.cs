using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Barracuda;

public class DetectionProcessor : MonoBehaviour
{
    [SerializeField] private float confidenceThreshold = 0.6f;
    [SerializeField] private bool debugMode = true;
    
    private string[] classNames = new string[] {
        "paper", "pack", "can", "glass", "pet", "plastic", "vinyl"
    };
    
    private int inputWidth = 640;
    private int inputHeight = 640;
    
    public List<Detection> ProcessDetectionResults(Tensor outputTensor)
    {
        int valuesPerBox = outputTensor.width;    // 12 (x, y, w, h, obj + 클래스 개수)
        int numBBoxes = outputTensor.channels;   // 25200
        
        if (debugMode)
        {
            Debug.Log($"DetectionProcessor: numBBoxes={numBBoxes}, valuesPerBox={valuesPerBox}");
        }
        
        List<Detection> detections = new List<Detection>();
        
        int expectedValuesPerBox = 5 + classNames.Length; // 5 + 클래스 개수
        
        if (valuesPerBox != expectedValuesPerBox)
        {
            Debug.LogWarning($"DetectionProcessor: valuesPerBox ({valuesPerBox}) 가 기대값({expectedValuesPerBox})과 다릅니다.");
        }
        
        for (int i = 0; i < numBBoxes; i++)
        {
            float x = outputTensor[0, 0, 0, i];
            float y = outputTensor[0, 0, 1, i];
            float w = outputTensor[0, 0, 2, i];
            float h = outputTensor[0, 0, 3, i];
            float objectness = outputTensor[0, 0, 4, i];
            
            float sigmoid_objectness = 1f / (1f + Mathf.Exp(-objectness));
            
            if (sigmoid_objectness > 0.6f)
            {
                float[] classScores = new float[classNames.Length];
                int bestClassIdx = 0;
                float bestScore = 0f;
                
                for (int c = 0; c < classNames.Length; c++)
                {
                    float rawClassScore = outputTensor[0, 0, 5 + c, i];
                    float sigmoidClassScore = 1f / (1f + Mathf.Exp(-rawClassScore));
                    classScores[c] = sigmoidClassScore;
                    
                    if (sigmoidClassScore > bestScore)
                    {
                        bestScore = sigmoidClassScore;
                        bestClassIdx = c;
                    }
                }
                
                float confidence = sigmoid_objectness * bestScore;
                
                if (confidence > confidenceThreshold && 
                    w > 10 && h > 10 &&
                    w < inputWidth * 0.8f && h < inputHeight * 0.8f &&
                    x > 0 && y > 0 && x < inputWidth && y < inputHeight)
                {
                    Detection detection = new Detection
                    {
                        BoundingBox = new Rect(
                            Mathf.Clamp01((x - w / 2) / inputWidth),
                            Mathf.Clamp01((y - h / 2) / inputHeight),
                            Mathf.Clamp01(w / inputWidth),
                            Mathf.Clamp01(h / inputHeight)
                        ),
                        Confidence = confidence,
                        ClassIndex = bestClassIdx,
                        ClassName = classNames[bestClassIdx]
                    };
                    
                    detections.Add(detection);
                    
                    if (debugMode)
                    {
                        string allScores = string.Join(", ", classNames.Select((name, idx) => $"{name}:{classScores[idx]:F3}"));
                        
                         Debug.Log($"감지됨 - {detection.ClassName}, 신뢰도: {confidence:F3}, " +
                         $"위치: ({detection.BoundingBox.x:F2}, {detection.BoundingBox.y:F2}, " +
                         $"{detection.BoundingBox.width:F2}, {detection.BoundingBox.height:F2}), " +
                         $"모든 클래스 점수: [{allScores}]");
                    }
                }
            }
        }
        
        if (debugMode) Debug.Log($"DetectionProcessor: 총 {detections.Count}개 감지 결과");
        
        return ApplyNMS(detections, 0.45f);
    }
    
    private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
    {
        // 신뢰도 기준 내림차순 정렬
        var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
        List<Detection> selectedDetections = new List<Detection>();
        
        while (sortedDetections.Count > 0)
        {
            // 가장 높은 신뢰도의 검출 선택
            Detection current = sortedDetections[0];
            selectedDetections.Add(current);
            sortedDetections.RemoveAt(0);
            
            // 나머지 검출과 IoU 계산하여 겹치는 것 제거
            sortedDetections.RemoveAll(d => IoU(current.BoundingBox, d.BoundingBox) > iouThreshold);
        }
        
        if (debugMode) Debug.Log($"DetectionProcessor: NMS 적용 후 {selectedDetections.Count}개 감지 결과 남음");
        return selectedDetections;
    }
    
    private float IoU(Rect box1, Rect box2)
    {
        // 두 박스가 겹치는 영역 계산
        float xOverlap = Mathf.Max(0, Mathf.Min(box1.xMax, box2.xMax) - Mathf.Max(box1.xMin, box2.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(box1.yMax, box2.yMax) - Mathf.Max(box1.yMin, box2.yMin));
        
        float intersection = xOverlap * yOverlap;
        float union = box1.width * box1.height + box2.width * box2.height - intersection;
        
        return union > 0 ? intersection / union : 0;
    }
}
