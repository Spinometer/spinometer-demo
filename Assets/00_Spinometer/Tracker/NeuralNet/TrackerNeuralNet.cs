using System;
using Drawing;
using GetBack.Spinometer.SpinalAlignment;
using GetBack.Spinometer.SpinalAlignmentVisualizer;
using GetBack.Spinometer.TrackerNeuralNetImpl;
using GetBack.Spinometer.UI;
using Unity.Mathematics;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer
{
  public class TrackerNeuralNet : MonoBehaviour
  {
    public enum TrackerStatus
    {
      Initializing,
      InternalError,
      Normal,
      CameraOffline,
      TrackingLost,
      TrackingUnstable,
    }

    [SerializeField] private ModelAsset _localizerModel;
    [SerializeField] private ModelAsset _poseEstimatorModel;
    [SerializeField] private WebCam _webCam;
    [SerializeField] private Settings _settings;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private ExtraUiDataSource _extraUiDataSource;
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private Transform _webCamPlane;
    private SpinalAlignmentVisualizerSkeleton _visualizerSkeleton = null;
    private SpinalAlignmentVisualizerStickFigure _visualizerStickFigure = null;
    private Model _localizerRuntimeModel;
    private Model _poseEstimatorRuntimeModel;
    private Localizer _localizer;
    private PoseEstimator _poseEstimator;
    private float _localizerProbablity;
    private BoundingBox? _localizerLastRoi = null;
    private BoundingBox? _poseEstimatorLastRoi = null;
    private float _secondsToNextFrame;
    private BoundingBox _localizerRect; // for drawing
    private BoundingBox _faceRect; // for drawing
    private float2 _faceCircleCenter; // for drawing
    private float _faceCircleRadius; // for drawing
    private float _headSizeWorldM = 0.2f;
    private PoseEstimator.Face _lastPoseEstimatorFace; // used in calibration
    private TensorShape _lastInputTensorShape; // used in calibration

    private bool _webCamFocused = true;
    private bool _showSkeleton = true;
    private bool _showStickFigure = false;
    private bool _stickFigureOnSide = false;
    private bool _smallScreenMode = false;
    private TrackerStatus _trackerStatus = TrackerStatus.Initializing;
    private VisualElement _warningMessageTrackingLost = null;
    private VisualElement _warningMessageTrackingUnstable = null;
    private VisualElement _warningMessageCameraOffline = null;

    private SpinalAlignment.SpinalAlignment _spinalAlignment = new SpinalAlignment.SpinalAlignment();
    private SpinalAlignmentEstimator _spinalAlignmentEstimator = null;
    private float _dt = 1.0f;

    public bool WebCamFocused
    {
      get => _webCamFocused;
      set => _webCamFocused = value;
    }
    public bool ShowSkeleton
    {
      get => _showSkeleton;
      set {
        _showSkeleton = value;
        UpdateSkeletonVisualizerVisibility();
      }
    }

    public bool ShowStickFigure
    {
      get => _showStickFigure;
      set {
        _showStickFigure = value;
        UpdateSkeletonVisualizerVisibility();
      }
    }

    public bool StickFigureOnSide
    {
      get => _stickFigureOnSide;
      set {
        _stickFigureOnSide = value;
      }
    }

    public bool SmallScreenMode
    {
      get => _smallScreenMode;
      set {
        _smallScreenMode = value;
        _visualizerStickFigure.SmallScreenMode = _smallScreenMode;
        var el = _uiDocument.rootVisualElement.Q<VisualElement>("face-pose");
        if (!_smallScreenMode)
          el.RemoveFromClassList("small-screen");
        else
          el.AddToClassList("small-screen");
      }
    }

    private void UpdateSkeletonVisualizerVisibility()
    {
      _visualizerSkeleton.ShowSkeleton = _showSkeleton;
      _visualizerSkeleton.ShowAlignmentValues = _showSkeleton && !_showStickFigure;
      _visualizerStickFigure.ShowAlignmentValues = _showStickFigure;
    }


    void Awake()
    {
      _spinalAlignmentEstimator = new SpinalAlignmentEstimator(_settings, _uiDataSource);
      _visualizerSkeleton = GetComponent<SpinalAlignmentVisualizerSkeleton>();
      _visualizerStickFigure = GetComponent<SpinalAlignmentVisualizerStickFigure>();
    }

    void Start()
    {
      _localizerRuntimeModel = ModelLoader.Load(_localizerModel);
      _poseEstimatorRuntimeModel = ModelLoader.Load(_poseEstimatorModel);
      _localizer = new Localizer(_localizerRuntimeModel);
      _poseEstimator = new PoseEstimator(_poseEstimatorRuntimeModel);
      _secondsToNextFrame = 0f;

      {
        _warningMessageTrackingLost = _uiDocument.rootVisualElement.Q<Label>("warning-tracking-lost");
        _warningMessageTrackingUnstable = _uiDocument.rootVisualElement.Q<Label>("warning-tracking-unstable");
        _warningMessageCameraOffline = _uiDocument.rootVisualElement.Q<Label>("warning-camera-offline");
        _warningMessageTrackingLost.visible = false;
        _warningMessageTrackingUnstable.visible = false;
        _warningMessageCameraOffline.visible = false;
      }
    }

    void OnDisable()
    {
      // Tell the GPU we're finished with the memory the engine used
      _localizer.Dispose();
      _poseEstimator.Dispose();
    }

    void Update()
    {
      _secondsToNextFrame -= Time.deltaTime;
      if (_secondsToNextFrame <= 0f) {
        _secondsToNextFrame += 1.0f / _settings.opt_poseEstimationFrequency;
        UpdatePositions();
      }

      {
        var tr = _webCamPlane.transform;
        var scale = tr.localScale.y; // FIXME:
        var lw = (!_smallScreenMode ? 1.0f : 2.0f) * (_webCamFocused ? 1f : 0.8f);
        using (Draw.ingame.WithLineWidth(lw)) {
          if (_localizerLastRoi.HasValue) {
            Draw.ingame.WireRectangle(tr.position + scale * _localizerRect.Center3,
                                      Quaternion.Euler(-90f, 0f, 0f),
                                      scale * _localizerRect.Size2,
                                      Color.magenta);
          }

          Draw.ingame.WireRectangle(tr.position + scale * _faceRect.Center3,
                                    Quaternion.Euler(-90f, 0f, 0f),
                                    scale * _faceRect.Size2,
                                    Color.green);

          Draw.ingame.Circle(tr.position + scale * new Vector3(_faceCircleCenter.x, _faceCircleCenter.y, 0f),
                             Vector3.back, scale * _faceCircleRadius, Color.white);
        }
      }

      if (_showSkeleton || _showStickFigure) {
        // stick figure depends on skeleton bone positions
        _visualizerSkeleton.UpdateAvatarPose(_spinalAlignment);
      }

      if (_showSkeleton) {
        // _visualizerSkeleton.DrawAlignment(_spinalAlignment, !_showStickFigure);
      }

      if (_showStickFigure) {
        _visualizerStickFigure.DrawAlignment(_spinalAlignment, true, _stickFigureOnSide, _uiDataSource.distance, _uiDataSource.pitch);
      }
    }

    void UpdatePositions()
    {
      if (_webCam == null) {
        _uiDataSource.webCamStateString = "";
        UpdateTrackerStatus(TrackerStatus.CameraOffline);
        // Debug.LogError("No webcam connected");
        return;
      }

      _uiDataSource.webCamStateString = _webCam.StateString;
      if (_webCam.State != WebCam.StateEnum.running) {
        UpdateTrackerStatus(TrackerStatus.CameraOffline);
        return;
      }
      var inputTexture = _webCam.InputTexture;
      if (inputTexture == null) {
        // Debug.LogError("No webcam texture available");
        UpdateTrackerStatus(TrackerStatus.InternalError);
        return;
      }

      using var inputTensor = TextureConverter.ToTensor(inputTexture);
      var inputTensorShape = inputTensor.shape;
      _lastInputTensorShape = inputTensorShape;

      // If there is no past ROI from the localizer or if the match of its output
      // with the current ROI is too poor we have to run it again. This causes a
      // latency spike of maybe an additional 50%. But it only occurs when the user
      // moves his head far enough - or when the tracking ist lost ...
      //Debug.Log("***AAA***");
      if (!_localizerLastRoi.HasValue || !_poseEstimatorLastRoi.HasValue ||
          Iou(_localizerLastRoi.Value, _poseEstimatorLastRoi.Value) < 0.25f) {
        //Debug.Log("***BBB***");
        var (localizerProbability, localizerNormalizedRoi) = _localizer.Run(inputTensor);
        _localizerRect = UnNormalize(localizerNormalizedRoi,
                                     float2.zero, new float2(4f, -3f));

        var localizerUnNormalizedRoi = UnNormalize(localizerNormalizedRoi,
                                                   new float2(1f, 1f),
                                                   new float2(inputTensor.shape[3], inputTensor.shape[2]));

        //Debug.Log("***CCC*** " + localizerProbability);
        //Debug.Log("***DDD*** " + localizerUnNormalizedRoi.x1 + ", " + localizerUnNormalizedRoi.y1 + ", " + localizerUnNormalizedRoi.x2 + ", " + localizerUnNormalizedRoi.y2);

        _extraUiDataSource.localizerProbability = localizerProbability;
        _extraUiDataSource.localizerNormalizedRoi = localizerNormalizedRoi;
        _extraUiDataSource.localizerRect = _localizerRect;
        _extraUiDataSource.localizerUnNormalizedRoi = localizerUnNormalizedRoi;

        if (_poseEstimatorLastRoi.HasValue
            && Iou(localizerUnNormalizedRoi, _poseEstimatorLastRoi.Value) >= 0.25f
            && localizerProbability > 0.5f) {
          // The new ROI matches the result from tracking, so the user is
          // still there and to not disturb recurrent models, we only update...
          _localizerLastRoi = localizerUnNormalizedRoi;
        } else if (localizerProbability > 0.5f
                   && localizerUnNormalizedRoi.Height > 32
                   && localizerUnNormalizedRoi.Width > 32) {
          // Tracking probably got lost since the ROI's don't match, but the
          // localizer still finds a face, so we use the ROI from the localizer
          _localizerLastRoi = localizerUnNormalizedRoi;
          _poseEstimatorLastRoi = localizerUnNormalizedRoi;
        } else {
          // Tracking lost and no localization result. The user probably can't be seen.
          _localizerLastRoi = null;
          _poseEstimatorLastRoi = null;
        }
        _extraUiDataSource.localizerLastRoi = _localizerLastRoi;
        _extraUiDataSource.poseEstimatorLastRoi = _poseEstimatorLastRoi;
      }
      //Debug.Log("***MMM***");

      if (!_poseEstimatorLastRoi.HasValue) {
        UpdateTrackerStatus(TrackerStatus.TrackingLost);
        return;
      }
      //Debug.Log("***NNN***");

      //
      {
        var face = _poseEstimator.Run(inputTensor, _poseEstimatorLastRoi.Value);
        _extraUiDataSource.faceCenter = face.center;
        _extraUiDataSource.faceCenterStdDev = face.centerStdDev;
        _extraUiDataSource.faceSize = face.size;
        _extraUiDataSource.faceSizeStdDev = face.sizeStdDev;
        _extraUiDataSource.faceRotation = face.rotation;
        _extraUiDataSource.faceBox = face.box;

        // Debug.Log("face: " + face.center + " " + face.size);
        if (face.size < 0.001f) {
          _poseEstimatorLastRoi = null;
          _extraUiDataSource.poseEstimatorLastRoi = _poseEstimatorLastRoi;
          UpdateTrackerStatus(TrackerStatus.TrackingLost);
          return;
        }
        //Debug.Log("***OOO***");
        if (face.sizeStdDev.x >= 7f || face.sizeStdDev.y >= 7f) {
          UpdateTrackerStatus(TrackerStatus.TrackingUnstable);
        } else {
          UpdateTrackerStatus(TrackerStatus.Normal);
        }

        float settings_roi_zoom = 1.0f;
        var roi = Expand(face.box, settings_roi_zoom);
        float settings_roi_filter_alpha = 1.0f;
        _poseEstimatorLastRoi = EwaFilter(_poseEstimatorLastRoi.Value, roi, settings_roi_filter_alpha);
        _extraUiDataSource.poseEstimatorLastRoi = _poseEstimatorLastRoi;

        var normalizedBox = Normalize(face.box,
                                      new float2(1f, 1f),
                                      new float2(inputTensor.shape[3], inputTensor.shape[2]));
        _lastPoseEstimatorFace = face;
        _faceRect = UnNormalize(normalizedBox,
                                float2.zero, new float2(4f, -3f));
        _faceCircleCenter = UnNormalize(Normalize(
                                          face.center,
                                          new float2(1f, 1f),
                                          new float2(inputTensor.shape[3], inputTensor.shape[2])),
                                        float2.zero, new float2(4f, -3f));
        _faceCircleRadius = face.size / inputTensor.shape[2] * 3f;

        var pose = TransformToWorldPose(face, inputTensorShape);
//        var angles = pose.rotation.eulerAngles;
        //Debug.Log(pose.position);
        pose.position = new Vector3(pose.position.z, pose.position.y, pose.position.x);
        _extraUiDataSource.posePosition = pose.position;
        _extraUiDataSource.poseRotation = pose.rotation;
        // now _extraUiDataSource has damped posePosition and poseRotation.  we use them below to translate/rotate face proxy.
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin").transform.localRotation = Quaternion.AngleAxis(
          -90f + _settings.opt_displaySurfaceAngle + _settings.opt_additionalPitchOffset, Vector3.left);
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy").transform.localPosition = _extraUiDataSource.posePosition;
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy").transform.localRotation = _extraUiDataSource.poseRotation;

        float smoothingLambda = 6.0f;
        float dt = Time.deltaTime;
        _dt = Damp(_dt, dt, smoothingLambda, dt);
        dt = _dt;
        float angle = -pose.rotation.eulerAngles.x;
        float pitchSensitivity = 1.4f; // FIXME:  arbitrary adjustment using magic number.
        _uiDataSource.pitch = Damp(_uiDataSource.pitch,
                                   ((angle + 540f) % 360f - 180f) * pitchSensitivity +
                                   (_settings.opt_displaySurfaceAngle - 90f) +
                                   _settings.opt_additionalPitchOffset,
                                   smoothingLambda, dt);

        _uiDataSource.distance = CorrectDistance(Damp(_uiDataSource.distance, -pose.position.z,  smoothingLambda, dt));

        _spinalAlignmentEstimator.Estimate(_uiDataSource.distance, _uiDataSource.pitch, _spinalAlignment);
      }
    }

    private float CorrectDistance(float rawDistance)
    {
      float d = rawDistance;
      float pitch = _uiDataSource.pitch;
      d += (_settings.opt_distance_correction_angle_distance_factor * 0.001f) * pitch;
      d += _settings.opt_distance_correction_cos_factor * 0.001f * (Mathf.Cos(pitch * Mathf.Deg2Rad) - 1.0f);
      return d;
    }

    private void UpdateTrackerStatus(TrackerStatus st)
    {
      _trackerStatus = st;

      switch (st) {
      case TrackerStatus.Initializing:
      case TrackerStatus.Normal:
      case TrackerStatus.InternalError:
        _warningMessageTrackingLost.visible = false;
        _warningMessageTrackingUnstable.visible = false;
        _warningMessageCameraOffline.visible = false;
        break;
      case TrackerStatus.CameraOffline:
        _warningMessageTrackingLost.visible = false;
        _warningMessageTrackingUnstable.visible = false;
        _warningMessageCameraOffline.visible = true;
        break;
      case TrackerStatus.TrackingLost:
        _warningMessageTrackingLost.visible = true;
        _warningMessageTrackingUnstable.visible = false;
        _warningMessageCameraOffline.visible = false;
        break;
      case TrackerStatus.TrackingUnstable:
        _warningMessageTrackingLost.visible = false;
        _warningMessageTrackingUnstable.visible = true;
        _warningMessageCameraOffline.visible = false;
        break;
      }
    }

    public static float Damp(float a, float b, float lambda, float dt)
    {
      return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

    private BoundingBox EwaFilter(BoundingBox last, BoundingBox current, float alpha)
    {
      float2 lastCenter = last.Center2;
      float2 lastSize2 = last.Size2;
      float2 newSize = lastSize2 + alpha * (current.Size2 - lastSize2); 
      float2 newCenter = lastCenter + alpha * (current.Center2 - lastCenter);
      float2 newHalfSize = newSize * 0.5f;
      return new BoundingBox(newCenter - newHalfSize, newCenter + newHalfSize);
    }

    private Pose TransformToWorldPose(PoseEstimator.Face face, TensorShape inputTensorShape)
    {
      var faceCenter = Image2World(face.center, face.size, _headSizeWorldM, inputTensorShape);
      var rotationCorrection = ComputeRotationCorrection(faceCenter);
      var rot = rotationCorrection * Image2World(face.rotation);

      float settings_offset_fwd = 0f;//.2f;
      float settings_offset_up = 0f;
      float settings_offset_right = 0f;
      var local_offset = new float3(
        settings_offset_fwd,
        settings_offset_up,
        settings_offset_right);
      float3 offset = rot * local_offset;
      float3 pos = faceCenter + offset;
//      pos.x = 0.784f * pos.x - 0.113f; // FIXME: adjustment with magic number only applicable to specific person and camera.

      var pose = new Pose(pos,
                          face.rotation);
      return pose;
    }

    private Quaternion Image2World(Quaternion rot)
    {
      return new Quaternion(-rot.z, rot.y, -rot.x, rot.w);
    }

    private Quaternion ComputeRotationCorrection(float3 faceCenter)
    {
      return Quaternion.FromToRotation(Vector3.left, faceCenter);
    }

    private float3 Image2World(float2 faceCenter, float faceSize, float referenceHeadSize_world_m, TensorShape inputTensorShape)
    {
      // excerpt from make_intrinsics() in ftnoir_tracker_neuralnet.cpp of opentrack
      /*  a
        ______  <--- here is sensor area
        |    /
        |   /
      f |  /
        | /  2 x angle is the fov
        |/
          <--- here is the hole of the pinhole camera

        So, a / f = tan(fov / 2)
        => f = a/tan(fov/2)
        What is a?
        1 if we define f in terms of clip space where the image plane goes from -1 to 1. Because a is the half-width.
      */

      int w = 288;
      int h = 244;
      float diag_fov = _settings.opt_displayDiagonalFov * Mathf.Deg2Rad;
      float fov_w = 2f * Mathf.Atan(Mathf.Tan(diag_fov / 2f) / Mathf.Sqrt(1f + (float)h / w * h / w));
      float fov_h = 2f * Mathf.Atan(Mathf.Tan(diag_fov / 2f) / Mathf.Sqrt(1f + (float)w / h * w / h));
      float intrinsics_focal_length_w = 1f / Mathf.Tan(.5f * fov_w);
      float intrinsics_focal_length_h = 1f / Mathf.Tan(.5f * fov_h);
      //Debug.Log($"{fov_w * Mathf.Rad2Deg}*{fov_h * Mathf.Rad2Deg}");
      //Debug.Log($"{intrinsics_focal_length_w}*{intrinsics_focal_length_h}");

      // excerpt from image_to_world() in ftnoir_tracker_neuralnet.cpp of opentrack
      /*
        Compute the location the network outputs in 3d space.

           hhhhhh  <- head size (meters)
          \      | -----------------------
           \     |                         \
            \    |                          |
             \   |                          |- x (meters)
              ____ <- face.size / width     |
               \ |  |                       |
                \|  |- focal length        /
                   ------------------------
                ------------------------------------------------>> z direction
            z/x = zi / f
            zi = image position
            z = world position
            f = focal length

            We can also do deltas:
            dz / x = dzi / f
            => x = dz / dzi * f
            which means we can compute x from the head size (dzi) if we assume some reference size (dz).
        */
      var headSizeVertical = 2f * faceSize;
      var xpos = -(intrinsics_focal_length_w * inputTensorShape[3] * 0.5f) / headSizeVertical * referenceHeadSize_world_m;
      var zpos = (faceCenter.x / inputTensorShape[3] * 2f - 1f) * xpos / intrinsics_focal_length_w;
      var ypos = (faceCenter.y / inputTensorShape[2] * 2f - 1f) * xpos / intrinsics_focal_length_h;
      return new float3(xpos, ypos, zpos);
    }

    public void CalibrateAngle()
    {
      float angleDiff = _uiDataSource.pitch - (_settings.opt_additionalPitchOffset +
                                               _settings.opt_displaySurfaceAngle);
      _uiDataSource.pitch = 0f;
      _settings.opt_additionalPitchOffset = 15f;
      _settings.opt_displaySurfaceAngle = -angleDiff - _settings.opt_additionalPitchOffset;
    }

    public void CalibrateDistance()
    {
      const float targetDistance = 0.5f;
      float r = 1.1f;
      for (int i = 0; i < 16; i++) {
        var pose = TransformToWorldPose(_lastPoseEstimatorFace, _lastInputTensorShape);
        _uiDataSource.distance = CorrectDistance(-pose.position.x);
        if (targetDistance < _uiDataSource.distance)
          _settings.opt_displayDiagonalFov *= r;
        else
          _settings.opt_displayDiagonalFov /= r;
        r = 1.0f + (r - 1.0f) * 0.9f;
      }
    }

    public struct BoundingBox : IEquatable<BoundingBox>
    {
      public float x1, y1, x2, y2;

      public BoundingBox(float x1_, float y1_, float x2_, float y2_)
      {
        x1 = x1_;
        y1 = y1_;
        x2 = x2_;
        y2 = y2_;
      }

      public BoundingBox(float2 topLeft, float2 bottomRight)
      {
        x1 = topLeft.x;
        y1 = topLeft.y;
        x2 = bottomRight.x;
        y2 = bottomRight.y;
      }

      public float2 TopLeft => new(x1, y1);
      public float2 TopRight => new(x2, y1);
      public float2 BottomLeft => new(x1, y2);
      public float2 BottomRight => new(x2, y2);
      public float2 Center2 => new((x1 + x2) * 0.5f, (y1 + y2) * 0.5f);
      public Vector3 Center3 => new((x1 + x2) * 0.5f, (y1 + y2) * 0.5f, 0f);
      public float2 Size2 => new(Width, Height);
      public float3 Size3 => new(Width, Height, 1f);
      public float Width => x2 - x1;
      public float Height => y2 - y1;

      public bool Equals(BoundingBox other)
      {
        return x1.Equals(other.x1) && y1.Equals(other.y1) && x2.Equals(other.x2) && y2.Equals(other.y2);
      }

      public override bool Equals(object obj)
      {
        return obj is BoundingBox other && Equals(other);
      }

      public override int GetHashCode()
      {
        return HashCode.Combine(x1, y1, x2, y2);
      }

      public override string ToString()
      {
        return $"({x1:F2}, {y1:F2})..({x2:F2}, {y2:F2})";
      }
    };

    private float UnNormalize(float x, float preOffset, float newSize)
    {
      return 0.5f * (preOffset + x) * newSize;
    }

    private float2 UnNormalize(float2 point, float2 preOffset, float2 newSize)
    {
      return new float2(UnNormalize(point.x, preOffset.x, newSize.x),
                        UnNormalize(point.y, preOffset.y, newSize.y));
    }

    private BoundingBox UnNormalize(BoundingBox box, float2 preOffset, float2 newSize)
    {
      return new BoundingBox(
        UnNormalize(box.TopLeft, preOffset, newSize),
        UnNormalize(box.BottomRight, preOffset, newSize));
    }

    private float Normalize(float x, float postOffset, float oldSize)
    {
      return x / oldSize * 2f - postOffset;
    }

    private float2 Normalize(float2 point, float2 postOffset, float2 oldSize)
    {
      return new float2(
        Normalize(point.x, postOffset.x, oldSize.x),
        Normalize(point.y, postOffset.y, oldSize.y));
    }

    private BoundingBox Normalize(BoundingBox box, float2 postOffset, float2 oldSize)
    {
      return new BoundingBox(
        Normalize(box.TopLeft, postOffset, oldSize),
        Normalize(box.BottomRight, postOffset, oldSize));
    }

    private float Iou(BoundingBox boxA, BoundingBox boxB)
    {
      if (boxA.Equals(boxB)) {
        return 1.0f;
      } else if (((boxA.x1 <= boxB.x1 && boxA.x2 > boxB.x1) || (boxA.x1 >= boxB.x1 && boxB.x2 > boxA.x1))
                 && ((boxA.y1 <= boxB.y1 && boxA.y2 > boxB.y1) || (boxA.y1 >= boxB.y1 && boxB.y2 > boxA.y1))) {
        float intersection = (Mathf.Min(boxA.x2, boxB.x2) - Mathf.Max(boxA.x1, boxB.x1))
                             * (Mathf.Min(boxA.y2, boxB.y2) - Mathf.Max(boxA.y1, boxB.y1));
        float union = (boxA.x2 - boxA.x1) * (boxA.y2 - boxA.y1) + (boxB.x2 - boxB.x1) * (boxB.y2 - boxB.y1) - intersection;
        return intersection / union;
      } else {
        return 0.0f;
      }
    }

    private BoundingBox Expand(BoundingBox r, float factor)
    {
      float2 newHalfSize = new float2(r.Width, r.Height) * factor * 0.5f;
      float2 tl = r.Center2 - newHalfSize;
      float2 br = r.Center2 + newHalfSize;
      return new BoundingBox(tl, br);
    }
  }
}
