using System;
using Unity.Sentis;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class BrightnessNormalizer : IDisposable
  {
#if false
    static Ops s_Ops;
    ITensorAllocator m_Allocator;
#endif

    public BrightnessNormalizer()
    {
#if false
      m_Allocator = new TensorCachingAllocator();
//      s_Ops = WorkerFactory.CreateOps(BackendType.GPUCompute, m_Allocator);
#endif
    }

    public Tensor<float> DoIt(Tensor<float> inputTensor)
    {
      //var t0 = s_Ops.Add(inputTensor, -128f);
      //var t1 = s_Ops.Mul(inputTensor, 1f / 100f);
      return inputTensor;
    }

    public void Dispose()
    {
#if false
      s_Ops?.Dispose();
      m_Allocator?.Dispose();
#endif
    }
  }
}
