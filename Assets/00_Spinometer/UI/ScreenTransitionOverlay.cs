using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.UI
{
  public class ScreenTransitionOverlay : MonoBehaviour
  {
    public enum TransitionStyle
    {
      Fade,
      Open,
      Close,
      Prev,
      Next,
    }

    [SerializeField] private Camera _mainCamera;
    private RenderTexture _renderTexture;
    private VisualElement _panelElement;

    void OnDisable()
    {
      if (_renderTexture != null) {
        _renderTexture.Release();
        _renderTexture = null;
        if (_panelElement != null) {
          _panelElement.style.backgroundImage = null;
          _panelElement.style.opacity = 0f;
        }
      }
    }

    private void EnsurePanelElementIsInitialized()
    {
      if (_panelElement != null)
        return;
      _panelElement = transform.GetComponent<UIDocument>().rootVisualElement.Children().First();
    }

    void EnsureRenderTextureConstructed()
    {
      EnsurePanelElementIsInitialized();
      if (_renderTexture != null && _renderTexture.width == Screen.width && _renderTexture.height == Screen.height)
        return;
      if (_renderTexture != null) {
        _renderTexture.Release();
        _renderTexture = null;
        _panelElement.style.backgroundImage = null;
      }
      _renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
      _panelElement.style.backgroundImage = Background.FromRenderTexture(_renderTexture);
    }

    public async void StartTransition(TransitionStyle transitionStyle,
                                      int delayFrames = 1,
                                      float durationSeconds = 0.3f)
    {
      // TODO accept cancellation token

      void ApplyPhase(float phase)
      {
        _panelElement.style.opacity = 1f - phase;
        switch (transitionStyle) {
        case TransitionStyle.Fade:
          _panelElement.style.translate = new StyleTranslate(new Translate(0f, 0f));
          _panelElement.style.scale = new StyleScale(new Scale(new Vector2(1f, -1f)));
          break;
        case TransitionStyle.Open:
          _panelElement.style.translate = new StyleTranslate(new Translate(0f, 0f));
          _panelElement.style.scale = new StyleScale(new Scale(new Vector2(1f + 0.1f * phase, -(1f + 0.1f * phase))));
          break;
        case TransitionStyle.Close:
          _panelElement.style.translate = new StyleTranslate(new Translate(0f, 0f));
          _panelElement.style.scale = new StyleScale(new Scale(new Vector2(1f - 0.1f * phase, -(1f - 0.1f * phase))));
          break;
        case TransitionStyle.Prev:
          // _panelElement.style.translate = new StyleTranslate(new Translate(0.01f * phase * Screen.width, 0f));
          _panelElement.style.scale = new StyleScale(new Scale(new Vector2(1f, -1f)));
          break;
        case TransitionStyle.Next:
          // _panelElement.style.translate = new StyleTranslate(new Translate(-0.01f * phase * Screen.width, 0f));
          _panelElement.style.scale = new StyleScale(new Scale(new Vector2(1f, -1f)));
          break;
        }

        
      }

      try {
        EnsureRenderTextureConstructed();
        _panelElement.style.opacity = 0f;
        await CaptureCurrentFrame();
        await UniTask.DelayFrame(delayFrames);
        float phase = 0f;
        ApplyPhase(phase);
        while (phase < 0.999f) {
          await UniTask.DelayFrame(1);
          phase = Mathf.Min(1f, phase + Time.deltaTime / durationSeconds);
          ApplyPhase(phase);
        }
        ApplyPhase(1f);
      }
      catch (Exception e) {
        //throw; // TODO handle exception
      }
    }

    public async Task CaptureCurrentFrame()
    {
      await UniTask.WaitForEndOfFrame();
      Graphics.Blit(null, _renderTexture);
    }
  }
}
