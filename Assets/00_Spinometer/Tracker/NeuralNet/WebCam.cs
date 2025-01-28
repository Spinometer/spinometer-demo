using System;
using System.Collections.Generic;
using System.Linq;
using GetBack.Spinometer.TrackerNeuralNetImpl;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace GetBack.Spinometer
{
  public class WebCam : MonoBehaviour
  {
    public enum StateEnum
    {
      notInitialized,
      waitingForAuthorization,
      accessRejected,
      accessGranted,
      noDeviceSelected, // access granted but no device selected
      failedToCreatedWebCamTexture, // access granted but failed to create webcam texture, probably due to bug
      running,
      webCamNotResponding,
    }

    [SerializeField] protected Material _grayscaleBlitMaterial;
    [SerializeField] private Settings _settings;

    private WebCamDevice[] _devices;
    private string _deviceName;
    private WebCamTexture _webcamRaw;
    private RenderTexture _webcamBufferColor;
    private RenderTexture _webcamBufferGrayscale;
    private StateEnum _state;
    private DateTime _lastSeen;

    public Texture InputTexture => _webcamBufferGrayscale;

    public StateEnum State
    {
      get => _state;
      private set {
        if (_state == value)
          return;
        _state = value;
        Debug.Log($"WebCam.State: {_state}");
      }
    }

    public string StateString => _stateToStringMap[_state];

    private readonly Dictionary<StateEnum, string> _stateToStringMap = new () {
      { StateEnum.notInitialized, "not initialized" },
      { StateEnum.waitingForAuthorization, "waiting for webcam access" },
      { StateEnum.accessRejected, "rejected to access webcam" },
      { StateEnum.accessGranted, "granted to access webcam" },
      { StateEnum.noDeviceSelected, "access granted but no device selected" },
      { StateEnum.failedToCreatedWebCamTexture, "failed to create webcam texture" },
      { StateEnum.running, "running" },
      { StateEnum.webCamNotResponding, "webcam not responding" },
    };
    // TODO: make camera intrinsics public

    void Awake()
    {
      State = StateEnum.notInitialized;
    }

    async void Start()
    {
      // FIXME: workaround to wait _settings populated.
      // this is needed when the app is started with Spinometer scene loaded.
      await Awaitable.NextFrameAsync();
      await Awaitable.NextFrameAsync();

      _webcamBufferColor = new RenderTexture(288, 224, 0);
      _webcamBufferColor.Create();
      _webcamBufferGrayscale = new RenderTexture(288, 224, 0, GraphicsFormat.R32_SFloat);
      _webcamBufferGrayscale.Create();

      //

      State = StateEnum.waitingForAuthorization;
      await Application.RequestUserAuthorization(UserAuthorization.WebCam);
      if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
        State = StateEnum.accessRejected;
        Debug.Log("no webcams accessible");
        return;
      }

      State = StateEnum.accessGranted;

      RescanDevices();
      var deviceName = _settings.opt_webCamDeviceName;
      deviceName = !string.IsNullOrEmpty(deviceName) ? deviceName : _devices[0].name;
      SelectDeviceByName(deviceName);
    }

    private void RescanDevices()
    {
      _devices = WebCamTexture.devices;
      var deviceNames = _devices.Select((WebCamDevice device) => device.name).ToList();
      if (!string.IsNullOrEmpty(_settings.opt_webCamDeviceName)) {
        if (!deviceNames.Exists(name => name == _settings.opt_webCamDeviceName))
          deviceNames.Add(_settings.opt_webCamDeviceName);
      }
      _settings.opt_webCamDeviceNameList = deviceNames;
      for (int cameraIndex = 0; cameraIndex < _devices.Length; ++cameraIndex) {
        Debug.Log($"devices[{cameraIndex}]: .name = " +
                  _devices[cameraIndex].name +
                  ", .isFrontFacing = " +
                  _devices[cameraIndex].isFrontFacing);
      }
    }

    private void SelectDeviceByName(string deviceName)
    {
      if (_webcamRaw != null) {
        _webcamRaw.Stop();
        _webcamRaw = null;
      }

      if (deviceName == null) {
        _deviceName = null;
        _settings.opt_webCamDeviceNameIndex = -1;
        State = StateEnum.noDeviceSelected;
        return;
      }

      _deviceName = deviceName;
      _settings.opt_webCamDeviceName = deviceName;
      _settings.opt_webCamDeviceNameIndex = _settings.opt_webCamDeviceNameList.FindIndex(name => name == deviceName);

      _webcamRaw = new WebCamTexture(deviceName, 320, 240, 30);
      if (_webcamRaw == null) {
        State = StateEnum.failedToCreatedWebCamTexture;
        return;
      }

      _webcamRaw.Play();
      State = StateEnum.running;
      _lastSeen = DateTime.Now;
    }

    void OnDestroy()
    {
      if (_webcamRaw != null) Destroy(_webcamRaw);
      if (_webcamBufferColor != null) Destroy(_webcamBufferColor);
      if (_webcamBufferGrayscale != null) Destroy(_webcamBufferGrayscale);
    }

    void Update()
    {
      if (_settings.opt_webCamDeviceNameIndex < 0)
        return;
      if (_settings.opt_webCamDeviceNameIndex >= _settings.opt_webCamDeviceNameList.Count)
        return;

      {
        var deviceName = _settings.opt_webCamDeviceNameList[_settings.opt_webCamDeviceNameIndex];
        if (_deviceName != deviceName) {
          SelectDeviceByName(deviceName);
        }
#if false
        if (_deviceName != _settings.opt_webCamDeviceName) {
          SelectDeviceByName(_settings.opt_webCamDeviceName);
        }
#endif
      }
      if (!_webcamRaw)
        return;
      var now = DateTime.Now;
      // Debug.Log(_webcamRaw.isPlaying + ", " + now + ", " + (now - _lastSeen));
      if (!_webcamRaw.didUpdateThisFrame) {
        var diff = now - _lastSeen;
        if (diff > TimeSpan.FromSeconds(4.0)) {
          // >5 doesn't work, because Unity reports fake blank frames after 5 (and sometimes less) seconds from disconnection.
          // >1 is reasonably reliable.
          Debug.Log("webcam not responded for 4 secs.  disconnecting...");
          RescanDevices();
          SelectDeviceByName(null);
          return;
        }
        if (diff > TimeSpan.FromSeconds(1.5)) {
          State = StateEnum.webCamNotResponding;
          return;
        }
        return;
      }

      State = StateEnum.running;
      _lastSeen = now;

      // TODO: do (crop/trim + scale and maybe flip + grayscale) in single pass

#if false
      // CopyTexture is not available in WebGL.
      // crop/trim
      bool trimSides = ((float)_webcamRaw.width / _webcamRaw.height > (float)_webcamBufferColor.width / _webcamBufferColor.height);
      int croppedWidth = !trimSides ? _webcamRaw.width : Mathf.RoundToInt(_webcamRaw.height * _webcamBufferColor.width / (float)_webcamBufferColor.height);
      int croppedHeight = trimSides ? _webcamRaw.height : Mathf.RoundToInt(_webcamRaw.width * _webcamBufferColor.height / (float)_webcamBufferColor.width);
      var tempTex = RenderTexture.GetTemporary(croppedWidth, croppedHeight, 0, _webcamRaw.graphicsFormat);
      // tempTex.Create();
      int srcX = (_webcamRaw.width - croppedWidth) / 2;
      int srcY = (_webcamRaw.height - croppedHeight) / 2;
      Graphics.CopyTexture(_webcamRaw, 0, 0, srcX, srcY, croppedWidth, croppedHeight,
                           tempTex, 0, 0, 0, 0);
#else
      // TODO: crop/trip
#endif

      // scale and maybe flip
      var vflip = _webcamRaw.videoVerticallyMirrored;
      var scale = new Vector2(1, vflip ? -1 : 1);
      var offset = new Vector2(0, vflip ? 1 : 0);
      Graphics.Blit(_webcamRaw, _webcamBufferColor, scale, offset);

      // grayscale
      Graphics.Blit(_webcamBufferColor, _webcamBufferGrayscale, _grayscaleBlitMaterial); 

      var material = GameObject.Find("/DebugPlane")?.GetComponent<MeshRenderer>()?.material;
      if (material != null)
        material.mainTexture = _webcamBufferGrayscale;
      //
      // RenderTexture.ReleaseTemporary(tempTex);
    }
  }
}
