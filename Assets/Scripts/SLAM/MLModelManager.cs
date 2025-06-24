using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class MLModelManager : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;

    private Model runtimeModel;
    private IWorker worker;

    public bool IsModelLoaded => worker != null;

    public event System.Action<string> OnModelLoadError;
    public event System.Action OnModelLoaded;

    void Start()
    {
        LoadModel();
    }

    private void LoadModel()
    {
        Debug.Log("MLModelManager: 모델 로드 시작");
        
        try 
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);
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
        
        worker.Execute(inputTensor);
        return worker.PeekOutput("output0");
    }
    
    void OnDestroy()
    {
        worker?.Dispose();
        Debug.Log("MLModelManager: 리소스 해제 완료");
    }
}
