using Cysharp.Threading.Tasks;
using DG.Tweening;
using GetBack.Spinometer.UI;
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
      Opening,
      Disclaimer,
      Running
    };

    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private Settings _settings;
    [SerializeField] private ScreenTransitionOverlay _screenTransitionOverlay;

    private string _sceneName_debug = "Debug";
    private string _sceneName_extra = "Extra";
    private string _sceneName_opening = "Opening";
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
      _state = !SceneLoaded(_sceneName_spinometer) ? State.Opening : State.Running;
      if (_state == State.Opening)
        LoadOpeningScene();
      else
        CloseOpeningScene();
    }

    private bool SceneLoaded(string sceneName)
    {
      var scene = SceneManager.GetSceneByName(sceneName);
      return scene != null && scene.isLoaded;
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

#if false
      if (Keyboard.current.dKey.wasPressedThisFrame)
        ToggleDebugUI();
      if (Keyboard.current.eKey.wasPressedThisFrame)
        ToggleExtraUI();
#endif
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

    private async void ToggleExtraUI()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_extra);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_extra, LoadSceneMode.Additive);
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy/Cube").SetActive(true);
      } else {
        await SceneManager.UnloadSceneAsync(scene);
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy/Cube").SetActive(false);
      }
    }

    private async void LoadOpeningScene()
    {
      if (!SceneLoaded(_sceneName_opening)) {
        await SceneManager.LoadSceneAsync(_sceneName_opening, LoadSceneMode.Additive);
      }

      await UniTask.WaitForSeconds(5);
      CloseOpeningScene();
    }

    private async void CloseOpeningScene()
    {
      LoadDisclaimerScene();

      var scene = SceneManager.GetSceneByName(_sceneName_opening);
      if (scene != null && scene.isLoaded) {
        Debug.Log("Unload");
        SceneManager.UnloadSceneAsync(scene);
      }

      _screenTransitionOverlay.StartTransition(durationSeconds: 2f,
                                               transitionStyle: ScreenTransitionOverlay.TransitionStyle.Fade);
    }

    private async void LoadDisclaimerScene()
    {
      if (!SceneLoaded(_sceneName_disclaimer)) {
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

      if (!InitialSetupAlreadyCompleted) {
        _screenTransitionOverlay.StartTransition(durationSeconds: 2f,
                                                 transitionStyle: ScreenTransitionOverlay.TransitionStyle.Open);
        LoadSettingsOrEasySetupScene();
      } else {
        _screenTransitionOverlay.StartTransition(durationSeconds: 2f,
                                                 transitionStyle: ScreenTransitionOverlay.TransitionStyle.Close);
      }
    }

    private async void LoadSpinometerScene()
    {
      if (!SceneLoaded(_sceneName_spinometer)) {
        await SceneManager.LoadSceneAsync(_sceneName_spinometer, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SpinometerUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("settings");
          btn.clicked += () =>
          {
            _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Open);
            LoadSettingsOrEasySetupScene();
          };
        }
        RegisterLocaleChangeButtonEvents(uidoc);
      }
      ToggleExtraUI();
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
      if (!SceneLoaded(_sceneName_settings)) {
        await SceneManager.LoadSceneAsync(_sceneName_settings, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SettingsUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        var btnOk = uidoc.rootVisualElement.Q<Button>("btn-settings-close");
        btnOk.clicked += () =>
        {
          _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Close);
          CloseScene(_sceneName_settings);
        };
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
      if (!SceneLoaded(_sceneName_easySetupCamera)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupCamera, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupCameraUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-close");
          btn.clicked += () =>
          {
            _screenTransitionOverlay.StartTransition(ScreenTransitionOverlay.TransitionStyle.Close);
            CloseScene(_sceneName_easySetupCamera);
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-next");
          btn.clicked += () => {
            _screenTransitionOverlay.StartTransition(ScreenTransitionOverlay.TransitionStyle.Next);
            CloseScene(_sceneName_easySetupCamera);
            LoadEasySetupAngleScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }

    private async void LoadEasySetupAngleScene()
    {
      if (!SceneLoaded(_sceneName_easySetupAngle)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupAngle, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupAngleUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Prev);
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
            _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Next);
            CloseScene(_sceneName_easySetupAngle);
            LoadEasySetupDistanceScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }
    
    private async void LoadEasySetupDistanceScene()
    {
      if (!SceneLoaded(_sceneName_easySetupDistance)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupDistance, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupDistanceUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Prev);
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
            _screenTransitionOverlay.StartTransition(transitionStyle: ScreenTransitionOverlay.TransitionStyle.Close);
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
