using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer
{
  [CreateAssetMenu(fileName = "settingsInstance", menuName = "ScriptableObjects/Settings", order = 1)]
  public class Settings : ScriptableObject
  {
    public bool opt_useNew = true;
    public string opt_webCamDeviceName;
    public List<string> opt_webCamDeviceNameList;
    public int opt_webCamDeviceNameIndex;

    // le:
    // additionalPitchOffset = 15
    // displayDiagonalFov = 56

    /// <summary>
    ///   Angle of the display surface from the front vector (based on the user) in degrees.
    ///   0 means it is completely flatten away and 90 indicates it is right up.
    /// </summary>
    public float opt_displaySurfaceAngle;

    /// <summary>
    /// </summary>
    public float opt_additionalPitchOffset;

    /// <summary>
    ///   The display diagonal field of view in degrees.
    /// </summary>
    public float opt_displayDiagonalFov;

    [Range(1, 30)] public int opt_poseEstimationFrequency = 4; // per sec

    [Range(1, 30)] public int opt_targetFrameRate = 15;

    public int opt_user_sex; // m:1, f:0
    public int opt_user_birthYear;
    public float opt_user_height_cm; // not in [m]
    public float opt_user_weight_kg;

//    public float opt_s_distance_offset = 50.0f;
//    public float opt_s_distance_multiplier = 66.0f;

    // backward
    //public float opt_s_distance_offset = 0.0f;
    //public float opt_s_distance_multiplier = 160.0f;

    public float opt_s_distance_offset = 80.0f;
    public float opt_s_distance_multiplier_foreward = 45.0f;
    public float opt_s_distance_multiplier_backward = 120.0f;

    public float opt_distance_correction_angle_distance_factor = 0f;
    public float opt_distance_correction_cos_factor = 64f;

    public void Awake()
    {
      opt_webCamDeviceNameList.Clear();
      opt_webCamDeviceNameIndex = -1;
      RevertSettings();
    }

    public void RegisterSettingsUIEventHandler()
    {
      var uidoc = GameObject.Find("/SettingsUIDocument").GetComponent<UIDocument>();
      var btnSettingsSave = uidoc.rootVisualElement.Q<Button>("btn-settings-save");
      var btnSettingsRevert = uidoc.rootVisualElement.Q<Button>("btn-settings-revert");
      btnSettingsSave.clicked += SaveSettings;
      btnSettingsRevert.clicked += RevertSettings;
    }

    private void RevertSettings()
    {
      Debug.Log("Settings#RevertSettings(): reverting settings...");
      LoadSettings();
      if (!string.IsNullOrEmpty(opt_webCamDeviceName)) {
        if (!opt_webCamDeviceNameList.Exists(name => name == opt_webCamDeviceName))
          opt_webCamDeviceNameList.Add(opt_webCamDeviceName);
        opt_webCamDeviceNameIndex = opt_webCamDeviceNameList.FindIndex(name => name == opt_webCamDeviceName);
      }

      Debug.Log("Settings#RevertSettings(): done.");
    }

    public void LoadSettings()
    {
      Debug.Log("Settings#LoadSettings(): loading settings...");
      opt_webCamDeviceName = PlayerPrefs.GetString("opt_webCamDeviceName", null);
      opt_displaySurfaceAngle = PlayerPrefs.GetFloat("opt_displaySurfaceAngle", 65f);
      opt_additionalPitchOffset = PlayerPrefs.GetFloat("opt_additionalPitchOffset", 15f);
      opt_displayDiagonalFov = PlayerPrefs.GetFloat("opt_displayDiagonalFov", 56f);
      opt_user_sex = PlayerPrefs.GetInt("opt_user_sex", -1);
      opt_user_birthYear = PlayerPrefs.GetInt("opt_user_birthYear", -1);
      opt_user_height_cm = PlayerPrefs.GetFloat("opt_user_height_cm", -1f);
      opt_user_weight_kg = PlayerPrefs.GetFloat("opt_user_weight_kg", -1f);
      opt_targetFrameRate = PlayerPrefs.GetInt("opt_targetFrameRate", 15);
      opt_poseEstimationFrequency = PlayerPrefs.GetInt("opt_poseEstimationFrequency", 4);

      Debug.Log("Settings#LoadSettings(): done.");
    }

    public void SaveSettings()
    {
      Debug.Log("Settings#SaveSettings(): saving settings...");
      PlayerPrefs.SetString("opt_webCamDeviceName", opt_webCamDeviceName);
      PlayerPrefs.SetFloat("opt_displaySurfaceAngle", opt_displaySurfaceAngle);
      PlayerPrefs.SetFloat("opt_additionalPitchOffset", opt_additionalPitchOffset);
      PlayerPrefs.SetFloat("opt_displayDiagonalFov", opt_displayDiagonalFov);
      PlayerPrefs.SetInt("opt_user_sex", opt_user_sex);
      PlayerPrefs.SetInt("opt_user_birthYear", opt_user_birthYear);
      PlayerPrefs.SetFloat("opt_user_height_cm", opt_user_height_cm);
      PlayerPrefs.SetFloat("opt_user_weight_kg", opt_user_weight_kg);
      PlayerPrefs.SetInt("opt_targetFrameRate", opt_targetFrameRate);
      PlayerPrefs.SetInt("opt_poseEstimationFrequency", opt_poseEstimationFrequency);
      Debug.Log("Settings#SaveSettings(): done.");
    }
  }
}
