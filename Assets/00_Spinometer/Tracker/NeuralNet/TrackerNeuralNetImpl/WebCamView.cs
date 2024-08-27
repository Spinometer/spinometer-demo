using UnityEngine;

namespace GetBack.Spinometer.TrackerNeuralNetImpl
{
  public class WebCamView
  {
    public Texture InputTexture
    {
      get => _cameraViewPlaneRenderer.material.mainTexture;
      set => _cameraViewPlaneRenderer.material.mainTexture = value;
    }

    private readonly MeshRenderer _cameraViewPlaneRenderer;

    public WebCamView(MeshRenderer cameraViewPlaneRenderer)
    {
      if (cameraViewPlaneRenderer == null) {
        Debug.LogError("WebCamView():  cameraViewPlaneRenderer is null");
        return;
      }

      _cameraViewPlaneRenderer = cameraViewPlaneRenderer;
      // _cameraViewPlaneRenderer.material.color = Color.white;
    }
  }
}
