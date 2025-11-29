using System;
using UnityEngine;
using Unity.Barracuda;

public class MLModelManager : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;

    private Model runtimeModel;
    private IWorker worker;

    public bool IsModelLoaded => worker != null;

    public event Action OnModelLoaded;
    public event Action<string> OnModelLoadError;

    void Start()
    {
        if (modelAsset == null)
        {
            Debug.LogError("MLModelManager: NNModel 에셋이 할당되지 않았습니다.");
            OnModelLoadError?.Invoke("Model asset is null");
            return;
        }

        LoadModel();
    }

    private void LoadModel()
    {
        try
        {
            runtimeModel = ModelLoader.Load(modelAsset);

            if (runtimeModel == null)
            {
                throw new Exception("ModelLoader.Load returned null");
            }

            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);

            if (worker == null)
            {
                throw new Exception("WorkerFactory.CreateWorker returned null");
            }

            Debug.Log("MLModelManager: 모델 로드 완료");
            OnModelLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"MLModelManager: 모델 로드 실패 - {e.Message}");
            OnModelLoadError?.Invoke(e.Message);
        }
    }

    public Tensor ExecuteModel(Tensor inputTensor)
    {
        if (worker == null)
        {
            Debug.LogError("MLModelManager: 모델이 로드되지 않았습니다.");
            return null;
        }

        if (inputTensor == null)
        {
            Debug.LogError("MLModelManager: 입력 텐서가 null입니다.");
            return null;
        }

        try
        {
            worker.Execute(inputTensor);
            return worker.PeekOutput("output0");
        }
        catch (Exception e)
        {
            Debug.LogError($"MLModelManager: 모델 실행 실패 - {e.Message}");
            return null;
        }
    }

    void OnDestroy()
    {
        if (worker != null)
        {
            worker.Dispose();
            worker = null;
        }

        runtimeModel = null;
        Debug.Log("MLModelManager: 리소스 해제 완료");
    }
}
