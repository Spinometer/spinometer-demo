using System;

using UnityEngine;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class Localizer : IDisposable
  {
    private Unity.InferenceEngine.Worker _worker;

    public Localizer(Unity.InferenceEngine.Model runtimeModel)
    {
      var backendType = SystemInfo.supportsComputeShaders ? Unity.InferenceEngine.BackendType.GPUCompute : Unity.InferenceEngine.BackendType.GPUPixel;
      _worker = new Unity.InferenceEngine.Worker(runtimeModel, backendType);
    }

    public (float, TrackerNeuralNet.BoundingBox) Run(Unity.InferenceEngine.Tensor<float> inputTensor)
    {
      _worker.Schedule(inputTensor);
      var results_ = _worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;
      var results = results_.DownloadToNativeArray();
      float localizerProbability = Sigmoid(results[0]);
      var normalizedRoi = new TrackerNeuralNet.BoundingBox(results[1], results[2], results[3], results[4]);
      return ValueTuple.Create(localizerProbability, normalizedRoi);
    }

    private float Sigmoid(float a)
    {
      return 1f / (1f + Mathf.Exp(-a));
    }

    public void Dispose()
    {
      _worker?.Dispose();
    }
  }
}
