using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "uiDataSourceInstance", menuName = "ScriptableObjects/UiDataSource", order = 1)]
public class UiDataSource : ScriptableObject
{
  public string webCamStateString;

  public string distanceStr;
  public string pitchStr;
  public string rel_C2_C7_vert;
  public string rel_T1_slope;
  public string rel_C7_T3_vert;
  public string rel_C7_T3_T8;
  public string rel_T3_T8_T12;
  public string rel_T8_T12_L3;
  public string rel_T12_L3_S;
  public string abs_EyePost;
  public string abs_C2;
  public string abs_C7;
  public string abs_T3;
  public string abs_T8;
  public string abs_T12;
  public string abs_L3;
  public string abs_S;

  public DisplayStyle poseGoodDisplayStyle = DisplayStyle.None;
  public DisplayStyle poseBadDisplayStyle = DisplayStyle.None;
  
  private float _distance;

  private float _pitch;

  public float distance // [m].  distance between eye-center and camera.
  {
    get => _distance;
    set
    {
      _distance = value;
      distanceStr = (value * 100f).ToString("0.0");
    }
    //set  { distanceStr = (value * 100f).ToString("0.0"); }
  }

  public float pitch // [deg].  angle of head.  - = looking down, + = looking up.
  {
    get => _pitch;
    set
    {
      _pitch = value;
      pitchStr = value.ToString("0.0");
    }
  }

  public bool IsPoseGood
  {
    get => poseGoodDisplayStyle != DisplayStyle.None;
    set {
      poseGoodDisplayStyle = value ? DisplayStyle.Flex : DisplayStyle.None;
      poseBadDisplayStyle = !value ? DisplayStyle.Flex : DisplayStyle.None;
    }
  }
}
