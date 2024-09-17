using Unity.Mathematics;
using UnityEngine;

namespace GetBack.Spinometer.UI
{
  [CreateAssetMenu(fileName = "extraUiDataSourceInstance", menuName = "ScriptableObjects/ExtraUiDataSource", order = 1)]
  public class ExtraUiDataSource : ScriptableObject
  {
    // common
    public float smoothingLambda = 6.0f;
    public float smoothingDt = 1.0f / 15;

    // face localizer
    public string localizerProbabilityStr;
    private float _localizerProbability;
    public string localizerNormalizedRoiStr;
    private TrackerNeuralNet.BoundingBox _localizerNormalizedRoi;
    public string localizerRectStr;
    private TrackerNeuralNet.BoundingBox _localizerRect;
    public string localizerUnNormalizedRoiStr;
    private TrackerNeuralNet.BoundingBox _localizerUnNormalizedRoi;
    public string localizerLastRoiStr;
    private TrackerNeuralNet.BoundingBox? _localizerLastRoi;

    // pose estimator
    public string poseEstimatorLastRoiStr;
    private TrackerNeuralNet.BoundingBox? _poseEstimatorLastRoi;
    public string faceCenterStr;
    private float2 _faceCenter;
    public string faceCenterStdDevStr;
    private float2 _faceCenterStdDev;
    public string faceSizeStr;
    private float _faceSize;
    public string faceSizeStdDevStr;
    private float2 _faceSizeStdDev;
    public string faceRotationStr;
    private Quaternion _faceRotation;
    public string faceBoxStr;
    private TrackerNeuralNet.BoundingBox _faceBox;

    // tracker
    public string posePositionStr;
    private Vector3 _posePosition;
    public string poseRotationStr;
    private Quaternion _poseRotation;

    //

    public float localizerProbability
    {
      get => _localizerProbability;
      set {
        _localizerProbability = Damp(_localizerProbability, value);
        localizerProbabilityStr = Fmt(_localizerProbability);
      }
    }

    public TrackerNeuralNet.BoundingBox localizerNormalizedRoi
    {
      get => _localizerNormalizedRoi;
      set {
        _localizerNormalizedRoi = Damp(_localizerNormalizedRoi, value);
        localizerNormalizedRoiStr = Fmt(_localizerNormalizedRoi);
      }
    }

    public TrackerNeuralNet.BoundingBox localizerRect
    {
      get => _localizerRect;
      set {
        _localizerRect = Damp(_localizerRect, value);
        localizerRectStr = Fmt(_localizerRect);
      }
    }

    public TrackerNeuralNet.BoundingBox localizerUnNormalizedRoi
    {
      get => _localizerUnNormalizedRoi;
      set {
        _localizerUnNormalizedRoi = Damp(_localizerUnNormalizedRoi, value);
        localizerUnNormalizedRoiStr = Fmt(_localizerUnNormalizedRoi);
      }
    }

    public TrackerNeuralNet.BoundingBox? localizerLastRoi
    {
      get => _localizerLastRoi;
      set {
        _localizerLastRoi = Damp(_localizerLastRoi, value);
        localizerLastRoiStr = Fmt(_localizerLastRoi);
      }
    }

    public TrackerNeuralNet.BoundingBox? poseEstimatorLastRoi
    {
      get => _poseEstimatorLastRoi;
      set {
        _poseEstimatorLastRoi = Damp(_poseEstimatorLastRoi, value);
        poseEstimatorLastRoiStr = Fmt(_poseEstimatorLastRoi);
      }
    }

    public float2 faceCenter
    {
      get => _faceCenter;
      set {
        _faceCenter = Damp(_faceCenter, value);
        faceCenterStr = Fmt(_faceCenter);
      }
    }

    public float2 faceCenterStdDev
    {
      get => _faceCenterStdDev;
      set {
        _faceCenterStdDev = Damp(_faceCenterStdDev, value);
        faceCenterStdDevStr = Fmt(_faceCenterStdDev);
      }
    }

    public float faceSize
    {
      get => _faceSize;
      set {
        _faceSize = Damp(_faceSize, value);
        faceSizeStr = Fmt(_faceSize);
      }
    }

    public float2 faceSizeStdDev
    {
      get => _faceSizeStdDev;
      set {
        _faceSizeStdDev = Damp(_faceSizeStdDev, value);
        faceSizeStdDevStr = Fmt(_faceSizeStdDev);
      }
    }

    public Quaternion faceRotation
    {
      get => _faceRotation;
      set {
        _faceRotation = Damp(_faceRotation, value);
        faceRotationStr = Fmt(_faceRotation);
      }
    }

    public TrackerNeuralNet.BoundingBox faceBox
    {
      get => _faceBox;
      set {
        _faceBox = Damp(_faceBox, value);
        faceBoxStr = Fmt(_faceBox);
      }
    }

    public Vector3 posePosition
    {
      get => _posePosition;
      set {
        _posePosition = Damp(_posePosition, value);
        posePositionStr = Fmt(_posePosition);
      }
    }

    public Quaternion poseRotation
    {
      get => _poseRotation;
      set {
        _poseRotation = Damp(_poseRotation, value);
        poseRotationStr = Fmt(_poseRotation);
      }
    }

    //

    public float Damp(float a, float b)
    {
      return Mathf.Lerp(a, b, 1 - Mathf.Exp(-smoothingLambda * smoothingDt));
    }

    public Vector2 Damp(Vector2 a, Vector2 b)
    {
      return Vector2.Lerp(a, b, 1 - Mathf.Exp(-smoothingLambda * smoothingDt));
    }

    public Vector3 Damp(Vector3 a, Vector3 b)
    {
      return Vector3.Lerp(a, b, 1 - Mathf.Exp(-smoothingLambda * smoothingDt));
    }

    public Quaternion Damp(Quaternion a, Quaternion b)
    {
      return Quaternion.Lerp(a, b, 1 - Mathf.Exp(-smoothingLambda * smoothingDt));
    }

    public TrackerNeuralNet.BoundingBox Damp(TrackerNeuralNet.BoundingBox a, TrackerNeuralNet.BoundingBox b)
    {
      return new TrackerNeuralNet.BoundingBox(Damp(a.x1, b.x1),
                                              Damp(a.y1, b.y1),
                                              Damp(a.x2, b.x2),
                                              Damp(a.y2, b.y2));
    }

    public TrackerNeuralNet.BoundingBox? Damp(TrackerNeuralNet.BoundingBox? a, TrackerNeuralNet.BoundingBox? b)
    {
      a ??= b;
      b ??= a;
      if (!a.HasValue)
        return null;  
  
      return new TrackerNeuralNet.BoundingBox(Damp(a.Value.x1, b.Value.x1),
                                              Damp(a.Value.y1, b.Value.y1),
                                              Damp(a.Value.x2, b.Value.x2),
                                              Damp(a.Value.y2, b.Value.y2));
    }

    //

    public string Fmt(float value)
    {
      return value.ToString("0.00");
    }

    public string Fmt(Vector2 value)
    {
      return value.ToString();
    }

    public string Fmt(Vector3 value)
    {
      return value.ToString();
    }

    public string Fmt(Quaternion value)
    {
      return $"({Fmt(value[0])}, {Fmt(value[1])}, {Fmt(value[2])}, {Fmt(value[3])})";
    }

    public string Fmt(TrackerNeuralNet.BoundingBox value)
    {
      return value.ToString();
    }

    public string Fmt(TrackerNeuralNet.BoundingBox? value)
    {
      return (!value.HasValue) ? "---" : value.Value.ToString();
    }
  }
}
