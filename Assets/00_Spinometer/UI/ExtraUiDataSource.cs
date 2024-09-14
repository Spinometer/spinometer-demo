using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GetBack.Spinometer.UI
{
  [CreateAssetMenu(fileName = "extraUiDataSourceInstance", menuName = "ScriptableObjects/ExtraUiDataSource", order = 1)]
  public class ExtraUiDataSource : ScriptableObject
  {
    // tracker
    public TrackerNeuralNet tracker;

    // face localizer
    public float localizerProbability;
    public TrackerNeuralNet.BoundingBox localizerNormalizedRoi;
    public TrackerNeuralNet.BoundingBox localizerRect;
    public TrackerNeuralNet.BoundingBox localizerUnNormalizedRoi;
    public TrackerNeuralNet.BoundingBox? localizerLastRoi;

    // pose estimator
    public TrackerNeuralNet.BoundingBox? poseEstimatorLastRoi;
    public float2 faceCenter;
    public float2 faceCenterStdDev;
    public float faceSize;
    public float2 faceSizeStdDev;
    public Quaternion faceRotation;
    public TrackerNeuralNet.BoundingBox faceBox;
    public Vector3 posePosition;
    public Quaternion poseRotation;
  }
}
