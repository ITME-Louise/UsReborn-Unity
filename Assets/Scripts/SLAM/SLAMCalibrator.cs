using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

public class SLAMCalibrator : MonoBehaviour
{
    [Header("SLAM Model")]
    [SerializeField] private GameObject slamModel;

    [Header("Calibration Points")]
    [SerializeField] private CalibrationPoint[] calibrationPoints = new CalibrationPoint[]
    {
        new CalibrationPoint("vinyl", new Vector2(0.36f, 0.51f), new Vector3(1.746f, -2.13f, 0.635f)),
        new CalibrationPoint("glass", new Vector2(0.12f, 0.5f), new Vector3(-0.008f, -2.13f, 2.24f)),
        new CalibrationPoint("paper", new Vector2(0.29f, 0.52f), new Vector3(2.411f, -2.13f, 2.42f)),
        new CalibrationPoint("plastic", new Vector2(0.43f, 0.42f), new Vector3(2.63f, -2.13f, 1.261f)),
        new CalibrationPoint("paper2", new Vector2(0.57f, 0.57f), new Vector3(2.5f, -2.13f, -0.62f)),
        new CalibrationPoint("can", new Vector2(0.59f, 0.46f), new Vector3(0.62f, -2.13f, 0.45f))
    };

    [Header("Transform Parameters")]
    [SerializeField] private Vector3 translation = Vector3.zero;
    [SerializeField] private Vector3 rotation = Vector3.zero;
    [SerializeField] private Vector3 scale = Vector3.one;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Mesh mesh;
    private Vector2[] uvs;
    private Vector3[] vertices;
    private Transform meshTransform;

    [System.Serializable]
    public class CalibrationPoint
    {
        public string name;
        public Vector2 uv;
        public Vector3 worldPos;

        public CalibrationPoint(string n, Vector2 u, Vector3 w)
        {
            name = n; uv = u; worldPos = w;
        }
    }

    void Start()
    {
        InitializeMesh();
        AutoCalibrate();
    }

    private void InitializeMesh()
    {
        if (slamModel == null) return;

        MeshFilter mf = slamModel.GetComponentInChildren<MeshFilter>();
        if (mf?.sharedMesh == null) return;

        mesh = mf.sharedMesh;
        uvs = mesh.uv;
        vertices = mesh.vertices;
        meshTransform = mf.transform;

        if (debugMode)
            Debug.Log($"Mesh initialized: {vertices.Length} vertices");
    }

    private Vector3 UVToWorld(Vector2 uv)
    {
        if (uvs == null || vertices == null) return Vector3.zero;

        int bestIndex = 0;
        float minDistance = Vector2.Distance(uv, uvs[0]);

        for (int i = 1; i < uvs.Length; i++)
        {
            float distance = Vector2.Distance(uv, uvs[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestIndex = i;
            }
        }

        return meshTransform.TransformPoint(vertices[bestIndex]);
    }

    private Vector3 ApplyTransform(Vector3 pos)
    {
        pos = Vector3.Scale(pos, scale);
        pos = Quaternion.Euler(rotation) * pos;
        pos += translation;
        return pos;
    }

    private void AutoCalibrate()
    {
        if (calibrationPoints.Length < 3)
        {
            Debug.LogError("최소 3개의 캘리브레이션 포인트가 필요합니다.");
            return;
        }

        List<Vector3> slamPoints = new List<Vector3>();
        List<Vector3> worldPoints = new List<Vector3>();

        foreach (var point in calibrationPoints)
        {
            Vector3 slamPos = UVToWorld(point.uv);
            slamPoints.Add(slamPos);
            worldPoints.Add(point.worldPos);
        }

        FindBestTransform(slamPoints, worldPoints);

        if (debugMode)
        {
            Debug.Log("=== 자동 캘리브레이션 완료 ===");
            Debug.Log($"Translation: {translation}");
            Debug.Log($"Rotation: {rotation}");
            Debug.Log($"Scale: {scale}");
            TestAccuracy();
        }
    }

    private void FindBestTransform(List<Vector3> fromPoints, List<Vector3> toPoints)
    {
        int n = fromPoints.Count;
        
        float[,] fromArray = new float[n, 3];
        float[,] toArray = new float[n, 3];
        
        for (int i = 0; i < n; i++)
        {
            fromArray[i, 0] = fromPoints[i].x;
            fromArray[i, 1] = fromPoints[i].y;
            fromArray[i, 2] = fromPoints[i].z;
            
            toArray[i, 0] = toPoints[i].x;
            toArray[i, 1] = toPoints[i].y;
            toArray[i, 2] = toPoints[i].z;
        }
        
        var fromMatrix = DenseMatrix.OfArray(fromArray);
        var toMatrix = DenseMatrix.OfArray(toArray);

        var fromCentroid = fromMatrix.ColumnSums() / n;
        var toCentroid = toMatrix.ColumnSums() / n;

        for (int i = 0; i < n; i++)
        {
            fromMatrix[i, 0] -= fromCentroid[0];
            fromMatrix[i, 1] -= fromCentroid[1];
            fromMatrix[i, 2] -= fromCentroid[2];
            
            toMatrix[i, 0] -= toCentroid[0];
            toMatrix[i, 1] -= toCentroid[1];
            toMatrix[i, 2] -= toCentroid[2];
        }
        
        var H = fromMatrix.TransposeThisAndMultiply(toMatrix);
        var svd = H.Svd();
        var R = svd.VT.TransposeThisAndMultiply(svd.U.Transpose());

        if (R.Determinant() < 0)
        {
            R.SetColumn(2, R.Column(2).Multiply(-1));
        }

        float scaleNumerator = 0f;
        float scaleDenominator = 0f;

        for (int i = 0; i < n; i++)
        {
            var p = DenseVector.OfArray(new float[] { fromMatrix[i, 0], fromMatrix[i, 1], fromMatrix[i, 2] });
            var q = DenseVector.OfArray(new float[] { toMatrix[i, 0], toMatrix[i, 1], toMatrix[i, 2] });
            var Rp = R.Multiply(p);
            scaleNumerator += q.DotProduct(Rp);
            scaleDenominator += p.DotProduct(p);
        }

        float s = scaleNumerator / scaleDenominator;
        var t = toCentroid - R.Multiply(fromCentroid).Multiply(s);

        translation = new Vector3(t[0], t[1], t[2]);
        scale = Vector3.one * s;
        rotation = QuaternionFromMatrix(R).eulerAngles;
    }

    private Quaternion QuaternionFromMatrix(Matrix<float> m)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(1.0f + m[0, 0] + m[1, 1] + m[2, 2]) / 2f;
        float w4 = 4.0f * q.w;
        q.x = (m[2, 1] - m[1, 2]) / w4;
        q.y = (m[0, 2] - m[2, 0]) / w4;
        q.z = (m[1, 0] - m[0, 1]) / w4;
        return q;
    }

    public Vector3 GetWorldPosition(Vector2 uv)
    {
        Vector3 slamPos = UVToWorld(uv);
        return ApplyTransform(slamPos);
    }

    [ContextMenu("Test Accuracy")]
    public void TestAccuracy()
    {
        Debug.Log("=== 캘리브레이션 정확도 테스트 ===");
        float totalError = 0f;

        foreach (var point in calibrationPoints)
        {
            Vector3 calculated = GetWorldPosition(point.uv);
            float error = Vector3.Distance(calculated, point.worldPos);
            totalError += error;

            Debug.Log($"[{point.name}] 계산: {calculated:F2}, 실제: {point.worldPos:F2}, 오차: {error:F3}m");
        }

        float avgError = totalError / calibrationPoints.Length;
        Debug.Log($"평균 오차: {avgError:F3}m");

        if (avgError < 0.1f)
            Debug.Log("✓ 매우 정확한 캘리브레이션");
        else if (avgError < 0.3f)
            Debug.Log("✓ 양호한 캘리브레이션");
        else
            Debug.LogWarning("⚠ 캘리브레이션 정확도가 낮습니다.");

        Debug.Log("================================");
    }

    [ContextMenu("Recalibrate")]
    public void Recalibrate()
    {
        AutoCalibrate();
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        translation = Vector3.zero;
        rotation = Vector3.zero;
        scale = Vector3.one;
    }
}
