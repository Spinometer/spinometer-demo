using GetBack.Spinometer;
using UnityEngine;

public class SettingsUIInitializer : MonoBehaviour
{
  [SerializeField] private Settings _settings;

  void Awake()
  {
    _settings.RegisterSettingsUIEventHandler();
  }
}
