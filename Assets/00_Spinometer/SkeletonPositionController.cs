using DG.Tweening;
using UnityEngine;

namespace GetBack.Spinometer
{
  public class SkeletonPositionController : MonoBehaviour
  {
    [SerializeField] private Transform _skeleton;
    [SerializeField] private Transform _skeletonPositionUnfocused;
    [SerializeField] private Transform _skeletonPositionFocused;
    private Vector3 _skeletonPositionWebCamOriginalPosition;

    private float _screenAspectRatio = 0f;

    private bool _focused = false;
    public bool Focused
    {
      get => _focused;
      set
      {
        _focused = value;
        MoveSkeleton();
      }
    }

    private void MoveSkeleton()
    {
      Transform goal = _focused ? _skeletonPositionFocused : _skeletonPositionUnfocused;

      float duration = 0.2f;
      var twPos = _skeleton.DOLocalMove(goal.localPosition, duration);
      var twRot = _skeleton.DOLocalRotateQuaternion(goal.localRotation, duration);
      var twSca = _skeleton.DOScale(goal.localScale, duration);

      twPos.Play();
      twRot.Play();
      twSca.Play();
    }

    void Start()
    {
      _skeletonPositionWebCamOriginalPosition = _skeletonPositionUnfocused.localPosition;
    }

    void Update()
    {
      float r = 1.0f * Screen.width / Screen.height;
      if (Mathf.Approximately(r, _screenAspectRatio))
        return;

      _screenAspectRatio = r;
      _skeletonPositionUnfocused.localPosition = new Vector3(
        _skeletonPositionWebCamOriginalPosition.x * _screenAspectRatio / (16.0f / 9.0f),
        _skeletonPositionWebCamOriginalPosition.y,
        _skeletonPositionWebCamOriginalPosition.z);
      MoveSkeleton();
    }
  }
}
