using System;
using Drawing;
using GetBack.Spinometer.UI;
using Unity.Mathematics;
using UnityEngine;
using Camera = UnityEngine.Camera;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class WebCamViewRenderer : MonoBehaviour
  {
    [SerializeField] private Camera _webViewRendererCamera;
    [SerializeField] private RenderTexture _renderTexture;

    public void UpdateRenderTexture(Texture inputTexture,
                                    TrackerNeuralNet.BoundingBox? localizerLastRoi,
                                    TrackerNeuralNet.BoundingBox localizerRect,
                                    TrackerNeuralNet.BoundingBox faceRect,
                                    float2 faceCircleCenter,
                                    float faceCircleRadius)
    {
      if (!isActiveAndEnabled)
        return;

      {
        RenderTexture.active = _renderTexture;
        Graphics.Blit(inputTexture, _renderTexture);
        RenderTexture.active = null;
      }

      var _draw = DrawingManager.GetBuilder(true);
      // FIXME: dispose even if error occurs
      _draw.cameraTargets = new [] { _webViewRendererCamera };

      {
        //using (_draw.InScreenSpace(_webViewRendererCamera)) {
        using (InScreenSpace(_draw, _webViewRendererCamera)) {
          using (_draw.WithLineWidth(1.0f)) {
            if (localizerLastRoi.HasValue) {
              _draw.xy.WireRectangle(localizerRect.Center3, localizerRect.Size2, Color.magenta);
            }

            _draw.xy.WireRectangle(faceRect.Center3,
                                   faceRect.Size2,
                                   Color.green);

            _draw.xy.Circle(new Vector3(faceCircleCenter.x, faceCircleCenter.y, 0f),
                            faceCircleRadius, Color.white);
          }
        }
      }

      _draw.Dispose();
    }

    private IDisposable InScreenSpace(CommandBuilder draw, Camera camera)
    {
      // basically draw.InScreenSpace(camera) but with y flipped
      return draw.WithMatrix(camera.cameraToWorldMatrix *
                             camera.nonJitteredProjectionMatrix.inverse *
                             Matrix4x4.TRS(new Vector3(-1.0f, 1.0f, 0),
                                           Quaternion.identity,
                                           new Vector3(2.0f / camera.pixelWidth, -2.0f / camera.pixelHeight, 1)));
    }
  }
}
