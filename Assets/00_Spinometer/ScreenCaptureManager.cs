using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBack.Spinometer
{
  public class ScreenCaptureManager : MonoBehaviour
  {
    [SerializeField] private Camera _targetCamera;
    private bool _screenCaptureEnabled = false;
    private int _frameCount = 0;


    public static void Capture(string filename, Camera targetCamera, Vector2Int captureSize)
    {
      var screenShot = new Texture2D(captureSize.x, captureSize.y, TextureFormat.ARGB32, false);
      var renderTexture = new RenderTexture(screenShot.width, screenShot.height, 32);
      targetCamera.clearFlags = CameraClearFlags.SolidColor;
      targetCamera.backgroundColor = new Color(0, 0, 0, 0);
      var prev = targetCamera.targetTexture;
      targetCamera.targetTexture = renderTexture;
      targetCamera.Render();
      targetCamera.targetTexture = prev;
      RenderTexture.active = renderTexture;
      screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
      screenShot.Apply();

      var bytes = screenShot.EncodeToPNG();
      DestroyImmediate(screenShot);
      System.IO.File.WriteAllBytes(filename, bytes);
    }

    void Update()
    {
      if (Keyboard.current.enterKey.wasPressedThisFrame) {
        _screenCaptureEnabled = !_screenCaptureEnabled;
      }
      if (_screenCaptureEnabled) {
        _frameCount++;
        const int N = 6;
        if (_frameCount % N != 0)
          return;
        int n = _frameCount / N;
        var filename = $"screenshot_{n:D5}.png";
        Debug.Log($"Capturing screenshot: {filename}");
        // ScreenCapture.CaptureScreenshot(filename);
        Capture(filename, _targetCamera, new Vector2Int(Screen.width, Screen.height));
      }
    }
  }
}
