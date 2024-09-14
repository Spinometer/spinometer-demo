using System;
using Unity.Mathematics;
using Unity.Sentis;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class PoseEstimator : IDisposable
  {
    private IWorker _worker;
    private bool hasUncertainty = true;
    private bool hasEyeClosedDetection = false;
    private BrightnessNormalizer _brightnessNormalizer;

    public PoseEstimator(Model poseEstimatorRuntimeModel)
    {
      var backendType = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.GPUPixel;
      _worker = WorkerFactory.CreateWorker(backendType, poseEstimatorRuntimeModel);
      _brightnessNormalizer = new BrightnessNormalizer();
    }

    /// <summary>
    /// Runs face detection and pose estimation on the given input tensor within the specified bounding box.
    /// </summary>
    /// <param name="inputTensor">The input tensor containing the face image.</param>
    /// <param name="box">The bounding box that encloses the face in the input tensor.</param>
    /// <returns>A Face object containing the detected face information.</returns>
    public Face Run(TensorFloat inputTensor, TrackerNeuralNet.BoundingBox box)
    {
      //int patchSize = Mathf.FloorToInt(Math.Max(box.Width, box.Height) * 1.05f);
      float patchSize = Math.Max(box.Width, box.Height) * 1.05f;
      float2 patchCenter = math.clamp(box.Center2, float2.zero, new float2(inputTensor.shape[3], inputTensor.shape[2]));
      var patchBox = new TrackerNeuralNet.BoundingBox(patchCenter.x - patchSize * 0.5f, patchCenter.y - patchSize * 0.5f,
                                                      patchCenter.x + patchSize * 0.5f, patchCenter.y + patchSize * 0.5f);
#if false
      if ((int)patchBox.x1 < 0 || (int)patchBox.x2 >= inputTensor.shape[3] ||
          (int)patchBox.y1 < 0 || (int)patchBox.y2 >= inputTensor.shape[2]) {
        // TODO: do something?
        return new Face();
      }
#endif

      var tempTex0 = TextureConverter.ToTexture(inputTensor, inputTensor.shape[3], inputTensor.shape[2], 1, false);

      // crop & scale
      var tempTex1 = RenderTexture.GetTemporary(129, 129, 0, tempTex0.graphicsFormat);
      //Debug.Log("format = " + tempTex0.graphicsFormat);
      tempTex1.Create();
      {
        // dst[x] = src[x * scale - offset]
        float2 scale = patchSize / new float2(tempTex0.width, tempTex0.height);
        float2 offset = new float2(0f, 1f) + patchBox.BottomLeft / new float2(tempTex0.width, -tempTex0.height);
        Graphics.Blit(tempTex0, tempTex1, scale, offset);
      }

      //
      using var croppedResizedInputTensor = TextureConverter.ToTensor(tempTex1);

      var material = GameObject.Find("/DebugPlane")?.GetComponent<MeshRenderer>()?.material;
      if (material != null)
        material.mainTexture = tempTex1;

      //
      using var croppedResizedBrightnessNormalizedInputTensor = _brightnessNormalizer.DoIt(croppedResizedInputTensor);

      //
      _worker.Execute(croppedResizedBrightnessNormalizedInputTensor);

      //
      var face = new Face();
      {
        var results = _worker.PeekOutput("pos_size") as TensorFloat;
        results.CompleteOperationsAndDownload();
        face.center = patchCenter + 0.5f * patchSize * new float2(results[0], results[1]);
        face.size = 0.5f * patchSize * results[2];
      }
      {
        var results = _worker.PeekOutput("pos_size_scales") as TensorFloat;
        results.CompleteOperationsAndDownload();
        face.centerStdDev = 0.5f * patchSize * new float2(results[0], results[1]);
        face.sizeStdDev = 0.5f * patchSize * results[2];
      }
      {
        var results = _worker.PeekOutput("quat") as TensorFloat;
        results.CompleteOperationsAndDownload();
        face.rotation = new Quaternion(results[0], results[1], results[2], results[3]);
      }
      {
        var results = _worker.PeekOutput("rotaxis_scales_tril") as TensorFloat;
        results.CompleteOperationsAndDownload();
        face.matrix = Matrix4x4.identity;
        face.matrix.m00 = results[0];
        face.matrix.m10 = results[1];
        face.matrix.m20 = results[2];
        face.matrix.m01 = results[3];
        face.matrix.m11 = results[4];
        face.matrix.m21 = results[5];
        face.matrix.m02 = results[6];
        face.matrix.m12 = results[7];
        face.matrix.m22 = results[8];
      }
      {
        var results = _worker.PeekOutput("box") as TensorFloat;
        results.CompleteOperationsAndDownload();
        face.box = new TrackerNeuralNet.BoundingBox(patchCenter.x + 0.5f * patchSize * results[0],
                                                    patchCenter.y + 0.5f * patchSize * results[1],
                                                    patchCenter.x + 0.5f * patchSize * results[2],
                                                    patchCenter.y + 0.5f * patchSize * results[3]);
      }

      RenderTexture.ReleaseTemporary(tempTex1);

      return face;
    }

    public void Dispose()
    {
      _worker?.Dispose();
      _brightnessNormalizer?.Dispose();
    }

    public struct Face
    {
      public float2 center; // x, y coordinates in screen space
      public float2 centerStdDev;
      public float size; // head radius in screen space
      public float2 sizeStdDev;
      public Quaternion rotation; // a quaternion representing the orientation
      public Matrix4x4 matrix;
      public TrackerNeuralNet.BoundingBox box; // a new bounding box. This enables tracking without the localization component.
    }
  }
}
