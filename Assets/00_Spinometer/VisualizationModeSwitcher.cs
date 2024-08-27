using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBack.Spinometer
{
  public class VisualizationModeSwitcher : MonoBehaviour
  {
    public enum FocusedObjectEnum
    {
      WebCam,
      SkeletonOnly,
      StickFigureOnly,
      SideBySide,
      Overlayed
    }

    [SerializeField] private TrackerNeuralNet _tracker;
    [SerializeField] private WebCamPlanePositionController _webCamPlanePositionController;
    [SerializeField] private SkeletonPositionController _skeletonPositionController;
    
    private FocusedObjectEnum _focusedObject;
    public FocusedObjectEnum focusedObject
    {
      get => _focusedObject;
      set {
        _focusedObject = value;
        bool webCamFocused = _focusedObject == FocusedObjectEnum.WebCam;
        bool skeletonFocused = _focusedObject is FocusedObjectEnum.SkeletonOnly or FocusedObjectEnum.StickFigureOnly or FocusedObjectEnum.SideBySide or FocusedObjectEnum.Overlayed; // includes StickFigureOnly to do positioning
        bool showSkeleton = _focusedObject is FocusedObjectEnum.SkeletonOnly or FocusedObjectEnum.SideBySide or FocusedObjectEnum.Overlayed;
        bool showStickFigure = _focusedObject is FocusedObjectEnum.StickFigureOnly or FocusedObjectEnum.SideBySide or FocusedObjectEnum.Overlayed;
        bool sideBySide = _focusedObject is FocusedObjectEnum.SideBySide;
        _tracker.WebCamFocused = webCamFocused;
        _tracker.ShowSkeleton = showSkeleton;
        _tracker.ShowStickFigure = showStickFigure;
        _tracker.StickFigureOnSide = sideBySide;
        _webCamPlanePositionController.Focused = webCamFocused;
        _skeletonPositionController.Focused = skeletonFocused;
      }
    }

    private bool _smallScreenMode = false;

    public static T NextEnumValue<T>(T src) where T : struct
    {
      if (!typeof(T).IsEnum)
        throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

      T[] Arr = (T[])Enum.GetValues(src.GetType());
      int j = Array.IndexOf<T>(Arr, src) + 1;
      //return (j == Arr.Length) ? Arr[0] : Arr[j];
      return (j == Arr.Length) ? Arr[1] : Arr[j];
    }

    private void NextMode()
    {
      focusedObject = NextEnumValue(focusedObject);
    }

  
    private void ToggleSmallScreen()
    {
      _smallScreenMode = !_smallScreenMode;
      _tracker.SmallScreenMode = _smallScreenMode;
    }

    void Start()
    {
      focusedObject = FocusedObjectEnum.Overlayed; 
    }

    void Update()
    {
      if (Keyboard.current.spaceKey.wasPressedThisFrame) {
        NextMode();
      }
      if (Keyboard.current.sKey.wasPressedThisFrame) {
        ToggleSmallScreen();
      }
    }
  }
}
