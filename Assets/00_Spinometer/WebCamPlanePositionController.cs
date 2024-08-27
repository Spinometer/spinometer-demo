using DG.Tweening;
using UnityEngine;

namespace GetBack.Spinometer
{
  public class WebCamPlanePositionController : MonoBehaviour
  {
    [SerializeField] private Transform _webCamPlane;
    [SerializeField] private Transform _webCamPlanePositionFocused;
    [SerializeField] private Transform _webCamPlanePositionUnfocused;

    private bool _focused = false;
    public bool Focused
    {
      get => _focused;
      set
      {
        _focused = value;
        MoveWebCamPlane();
      }
    }

    private void MoveWebCamPlane()
    {
      Transform goal = _focused ? _webCamPlanePositionFocused : _webCamPlanePositionUnfocused;

      float duration = 0.2f;
      var twPos = _webCamPlane.DOLocalMove(goal.localPosition, duration);
      var twRot = _webCamPlane.DOLocalRotateQuaternion(goal.localRotation, duration);
      var twSca = _webCamPlane.DOScale(goal.localScale, duration);

      twPos.Play();
      twRot.Play();
      twSca.Play();
    }
  }
}
