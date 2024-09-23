using System;
using Unity.Sentis;
using UnityEngine;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class Localizer : IDisposable
  {
    private Worker _worker;

    public Localizer(Model runtimeModel)
    {
      var backendType = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.GPUPixel;
      _worker = new Worker(runtimeModel, backendType);
    }

    public (float, TrackerNeuralNet.BoundingBox) Run(Tensor<float> inputTensor)
    {
      _worker.Schedule(inputTensor);
      var results_ = _worker.PeekOutput() as Tensor<float>;
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
