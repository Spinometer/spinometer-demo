using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace GetBack.Spinometer
{

  public class App : MonoBehaviour
  {
    public enum State {
      Disclaimer,
      Running
    };

    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private Settings _settings;

    private string _sceneName_debug = "Debug";
    private string _sceneName_disclaimer = "Disclaimer";
    private string _sceneName_settings = "Settings";
    private string _sceneName_easySetupCamera = "EasySetupCamera";
    private string _sceneName_easySetupAngle = "EasySetupAngle";
    private string _sceneName_easySetupDistance = "EasySetupDistance";
    private string _sceneName_spinometer = "Spinometer";

    private State _state;
    private bool _settingsInitialized = false;

    // TODO: move to dedicated class
    public bool InitialSetupAlreadyCompleted {
      get => PlayerPrefs.GetInt("opt_initial_setup_already_completed", 0) != 0;
      set { PlayerPrefs.SetInt("opt_initial_setup_already_completed", value ? 1 : 0); }
    }

    void Awake()
    {
      DOTween.Init();
    }

    void Start()
    {
      ChangeLocale("en");
      _state = State.Disclaimer;
#if UNITY_EDITOR
      {
        var scene = SceneManager.GetSceneByName(_sceneName_spinometer);
        if (scene != null && scene.isLoaded)
          _state = State.Running;
      }
#endif
      if (_state != State.Disclaimer)
        CloseDisclaimerScene();
      else
        LoadDisclaimerScene();
    }

    void Update()
    {
      if (!_uiDataSource || !_settings)
        return;
      if (!_settingsInitialized) {
        _settings.Awake();
        _settingsInitialized = true;
      }
      Application.targetFrameRate = _settings.opt_targetFrameRate;
#if !UNITY_EDITOR
    int vSyncCount = 60 / _settings.opt_targetFrameRate;
    QualitySettings.vSyncCount = (int)Mathf.Clamp(vSyncCount, 1, 4);
#endif

      if (Keyboard.current.dKey.wasPressedThisFrame)
        ToggleDebugUI();
    }

    private async void ToggleDebugUI()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_debug);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_debug, LoadSceneMode.Additive);
      } else {
        await SceneManager.UnloadSceneAsync(scene);
      }
    }

    private async void LoadDisclaimerScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_disclaimer);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_disclaimer, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/DisclaimerUIDocument").GetComponent<UIDocument>();
      uidoc.rootVisualElement.style.opacity = 0f;
      RegisterLocaleChangeButtonEvents(uidoc);
      var btnOk = uidoc.rootVisualElement.Q<Button>("btn-disclaimer-ok");
      btnOk.style.opacity = 0f;
      var tw = DOTween.To(() => uidoc.rootVisualElement.style.opacity.value,
                          x => uidoc.rootVisualElement.style.opacity = x, 1.0f, 2.0f);
      tw.onComplete += (() =>
          {
            btnOk.clicked += CloseDisclaimerScene;
            btnOk.style.opacity = 1f;
          }
        );
      tw.Play();
    }

    private async void CloseDisclaimerScene()
    {
      Debug.Log("Close");
      var uidoc = GameObject.Find("/DisclaimerUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        var btnOk = uidoc.rootVisualElement.Q<Button>();
        btnOk.clicked += CloseDisclaimerScene;
      }

      LoadSpinometerScene();

      var scene = SceneManager.GetSceneByName(_sceneName_disclaimer);
      if (scene != null && scene.isLoaded) {
        Debug.Log("Unload");
        SceneManager.UnloadSceneAsync(scene);
      }

      if (!InitialSetupAlreadyCompleted)
        LoadSettingsOrEasySetupScene();
    }

    private async void LoadSpinometerScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_spinometer);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_spinometer, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SpinometerUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("settings");
          btn.clicked += LoadSettingsOrEasySetupScene;
        }
        RegisterLocaleChangeButtonEvents(uidoc);
      }
    }

    private static void RegisterLocaleChangeButtonEvents(UIDocument uidoc)
    {
      {
        var btn = uidoc.rootVisualElement.Q<Button>("change-locale-en");
        if (btn != null) {
          btn.clicked += () => ChangeLocale("en");
        }
      }
      {
        var btn = uidoc.rootVisualElement.Q<Button>("change-locale-ja");
        if (btn != null) {
          btn.clicked += () => ChangeLocale("ja");
        }
      }
    }

    private static void ChangeLocale(string localeName)
    {
      if (localeName != "ja")
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
      else
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
    }

    private void LoadSettingsOrEasySetupScene()
    {
      if (Keyboard.current.aKey.isPressed)
        LoadSettingsScene();
      else
        LoadEasySetupCameraScene();
    }

    private async void LoadSettingsScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_settings);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_settings, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SettingsUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        var btnOk = uidoc.rootVisualElement.Q<Button>("btn-settings-close");
        btnOk.clicked += () => CloseScene(_sceneName_settings);
        RegisterLocaleChangeButtonEvents(uidoc);
      }
    }

    private void CloseScene(string sceneName)
    {
      var scene = SceneManager.GetSceneByName(sceneName);
      if (scene != null && scene.isLoaded) {
        SceneManager.UnloadSceneAsync(scene);
      }
    }

    private async void LoadEasySetupCameraScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_easySetupCamera);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupCamera, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupCameraUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-close");
          btn.clicked += () => CloseScene(_sceneName_easySetupCamera);
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-next");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupCamera);
            LoadEasySetupAngleScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }

    private async void LoadEasySetupAngleScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_easySetupAngle);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupAngle, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupAngleUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupAngle);
            LoadEasySetupCameraScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-next");
          btn.clicked += () => {
            var tracker = GameObject.Find("/Tracker").GetComponent<TrackerNeuralNet>();
            tracker?.CalibrateAngle();
            CloseScene(_sceneName_easySetupAngle);
            LoadEasySetupDistanceScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }
    
    private async void LoadEasySetupDistanceScene()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_easySetupDistance);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupDistance, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupDistanceUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupDistance);
            LoadEasySetupAngleScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-finish");
          btn.clicked += () =>
          {
            var tracker = GameObject.Find("/Tracker").GetComponent<TrackerNeuralNet>();
            tracker?.CalibrateDistance();
            CloseScene(_sceneName_easySetupDistance);
            _settings.SaveSettings();
            InitialSetupAlreadyCompleted = true;
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }
  }

}
