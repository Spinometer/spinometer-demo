using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBack.Spinometer.DataExporter
{
  public class PeriodicDataExporter : MonoBehaviour
  {
    private DataExporter _dataExporter;
    private float _timer;
    [SerializeField] private float _exportInterval = 60f; // seconds
    [SerializeField] private TrackerNeuralNet _trackerNeuralNet;
    [SerializeField] private UiDataSource _uiDataSource;
    private MeshRenderer _backgroundMeshRenderer;

    void OnEnable()
    {
    }

    void OnDisable()
    {
      StopExporting();
    }

    public void StartExporting()
    {
      if (_dataExporter != null)
        return;
      _timer = 0f;
      string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      string filename = $"spinometer-data-{DateTime.Now:yyyyMMddTHHmmss}.jsonl";
      _dataExporter = new DataExporter($"{desktop}/{filename}");
      _dataExporter.Write("{\"data-version\": \"1\"}\n");
      _backgroundMeshRenderer.enabled = false;
    }

    public void StopExporting()
    {
      _dataExporter?.Dispose();
      _dataExporter = null;
      _backgroundMeshRenderer.enabled = true;
    }

    void Start()
    {
      _backgroundMeshRenderer = GameObject.Find("/BackgroundPlane").GetComponent<MeshRenderer>();
    }

    void Update()
    {
      if (Keyboard.current.tKey.isPressed && Keyboard.current.gKey.isPressed && Keyboard.current.bKey.isPressed &&
          Keyboard.current.rKey.wasPressedThisFrame) {
        if (_dataExporter == null) {
          StartExporting();
        } else {
          StopExporting();
        }
      }

      if (_dataExporter == null)
        return;

      _timer -= Time.deltaTime;
      if (_timer > 0f)
        return;

      _timer = Mathf.Max(_exportInterval * 0.25f, _timer + _exportInterval);
      CultureInfo ci = CultureInfo.InvariantCulture;
      string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss.fff", ci);
      var sa = _trackerNeuralNet.spinalAlignment;
      var sas = _trackerNeuralNet.spinalAlignmentScore;
      var saJson = JsonConvert.SerializeObject(sa);
      var sasJson = JsonConvert.SerializeObject(sas);
      string line = $"{{\"timestamp\": \"{timestamp}\", " +

                    $"\"tracker\": {{\"status\": \"{_trackerNeuralNet.TrackerStatus}\", " +
                    $"\"pitch\": {_uiDataSource.pitch}, " +
                    $"\"distance\": {_uiDataSource.distance}" +
                    $"}}, " +

                    $"\"spinalAlignment\": {saJson}, " +

                    $"\"spinalAlignmentScore\": {sasJson}" +

                    $"}}\n";
      _dataExporter.Write(line);
    }
  }
}
