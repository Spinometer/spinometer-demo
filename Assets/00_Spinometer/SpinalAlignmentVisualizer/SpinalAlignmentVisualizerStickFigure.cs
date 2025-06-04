using Drawing;
using GetBack.Spinometer.SpinometerAux;
using GetBack.Spinometer.SpinometerCore;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.SpinalAlignmentVisualizer
{
  public class SpinalAlignmentVisualizerStickFigure : MonoBehaviour
  {
    [SerializeField] private Transform _avatar_skeleton;
    [SerializeField] private Transform _refS;
    [SerializeField] private Transform _refL3;
    [SerializeField] private Transform _refT12;
    [SerializeField] private Transform _refT8;
    [SerializeField] private Transform _refT3;
    [SerializeField] private Transform _refC7;
    [SerializeField] private Transform _refC2;

    private float _dist_S_L3 = 0f;
    private float _dist_L3_T12 = 0f;
    private float _dist_T12_T8 = 0f;
    private float _dist_T8_T3 = 0f;
    private float _dist_T3_C7 = 0f;
    private float _dist_C7_C2 = 0f;

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _alignmentValueLabelPrototype;
    [SerializeField] private float _textScale = 1.0f;
    [SerializeField] private float _scoreWarningIndicatorSensitivity = 0.5f;

    private VisualElement _alignmentValueLabelContainer;
    private Label[] _alignmentValueLabelElements = null;

    public enum AlignmentValueDisplayModeEnum
    {
      Hidden,
      ShowRelativeAngles,
      ShowAbsoluteAngles,
    }

    private AlignmentValueDisplayModeEnum _alignmentValueDisplayMode = AlignmentValueDisplayModeEnum.ShowAbsoluteAngles;
    public AlignmentValueDisplayModeEnum AlignmentValueDisplayMode
    {
      get => _alignmentValueDisplayMode;
      set
      {
        _alignmentValueDisplayMode = value;
        foreach (var el in _alignmentValueLabelElements)
          el.visible = false;
      }
    }

    public bool SmallScreenMode { get; set; }

    private void Awake()
    {
      GrabSegmentLengths();

      /*
       _dist_S_L3 = 0.066f;
       _dist_L3_T12 = 0.093f;
       _dist_T12_T8 = 0.100f;
       _dist_T8_T3 = 0.106f;
       _dist_T3_C7 = 0.086f;
       _dist_C7_C2 = 0.086f;
       _dist_C2_EyePost = 0.080f;
       _dist_EyePost_EyeAnt = 0.050f;
      */

      //

      _alignmentValueLabelContainer = _uiDocument.rootVisualElement.Q<VisualElement>("alignment-values");
      _alignmentValueLabelElements = new Label[16]; // should be enough
      for (var i = 0; i < _alignmentValueLabelElements.Length; i++) {
        var el = _alignmentValueLabelPrototype.Instantiate()[0] as Label;
        _alignmentValueLabelContainer.Add(el);
        _alignmentValueLabelElements[i] = el;
        el.visible = false;
        el.text = $"{i}";
        var pos = Camera.main.WorldToScreenPoint(new Vector3(0f, 0.1f * i, 0f));
        el.style.left = pos.x;
        el.style.top = pos.y;
      }
    }

    private void GrabSegmentLengths()
    {
      _dist_S_L3 = (_refL3.position - _refS.position).magnitude;
      _dist_L3_T12 = (_refT12.position - _refL3.position).magnitude;
      _dist_T12_T8 = (_refT8.position - _refT12.position).magnitude;
      _dist_T8_T3 = (_refT3.position - _refT8.position).magnitude;
      _dist_T3_C7 = (_refC7.position - _refT3.position).magnitude;
      _dist_C7_C2 = (_refC2.position - _refC7.position).magnitude;
    }

    public void DrawAlignment(SpinalAlignment spinalAlignment,
                              SpinalAlignmentScore spinalAlignmentScore,
                              bool verbose, bool onSide,
                              float face_dist, float face_pitch)
    {
      // FIXME:  pitch and dist should not be here
      if (!spinalAlignment.absoluteAngles.ContainsKey(SpinalAlignment.AbsoluteAngleId.L3_S))
        return;

      float scale = _avatar_skeleton.localScale.x;

      GrabSegmentLengths();

      Vector3 Dpos(float dist, float angle)
      {
        angle *= Mathf.Deg2Rad;
        return dist * new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 0f);
      }

      Vector3 DrawSegment_(Vector3 pos0, Vector3 dpos, bool draw = true)
      {
        Vector3 pos1 = pos0 + dpos;
        if (draw) {
          var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero);
          offset *= scale;
          var normal = Vector3.back;
          using (Draw.ingame.WithLineWidth(SmallScreenMode ? 3f : 1f)) {
            Draw.ingame.Line(pos1 + offset, pos0 + offset);
            Draw.ingame.Circle(pos1 + offset, normal, 0.01f);
          }
        }
        return pos1;
      }

      Vector3 DrawSegment(Vector3 pos0, float dist, SpinalAlignment.AbsoluteAngleId id, bool draw = true)
      {
        // dist *= _avatar_skeleton.localScale.x;
        float angle = spinalAlignment.absoluteAngles[id];
        Vector3 pos1 = DrawSegment_(pos0, Dpos(dist, angle), draw);
        return pos1;
      }

      void DrawAngle_(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                      float angle,
                      float score,
                      string label, Vector2 labelOffset, Color color, int n,
                      bool shortestArc = false)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        GrabSegmentLengths();

        var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero); // FIXME: scale
        offset *= scale;
        pos0 += offset;
        pos1 += offset;
        pos2 += offset;
        var normal = Vector3.back;
        pos0 = Vector3.Lerp(pos1, pos0, 0.4f);
        pos2 = Vector3.Lerp(pos1, pos2, 0.4f);
        var vec10 = (pos0 - pos1).normalized;
        var vec12 = (pos2 - pos1).normalized;
        var radius = ((pos0 - pos1).magnitude + (pos2 - pos0).magnitude) * 0.5f * 0.2f * scale;
        using (Draw.ingame.WithColor(color)) {
          if (!verbose) {
            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              el.visible = false;
            }
          } else {
            using (Draw.ingame.WithLineWidth(SmallScreenMode ? 5f : 3f)) {
              if (shortestArc)
                Draw.ingame.Arc(pos1, pos1 + vec10 * radius, pos1 + vec12 * radius);
              else
                Util.Drawing.DrawArc(pos1, pos1 + vec10 * radius, pos1 + vec12 * radius);
            }

            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              var screenPos = Camera.main.WorldToScreenPoint(pos1/* + offset*/);
              var uiPosX = screenPos.x / Screen.width * _alignmentValueLabelContainer.layout.width;
              var uiPosY = (1.0f - screenPos.y / Screen.height) * _alignmentValueLabelContainer.layout.height;
              el.visible = true;
              // el.text = $"{label}\n{angle:0.0}";
              el.text = $"{label}<size=15> : </size>{angle:0.0}";
              el.style.left = uiPosX + (labelOffset.x + 40f) * scale * _textScale * 0.25f;
              el.style.top = uiPosY + (labelOffset.y - 120f) * scale * _textScale * 0.25f;
              //el.style.color = color;
              el.style.fontSize = 16.0f * scale * _textScale;
              bool withinNormalBound = score >= _scoreWarningIndicatorSensitivity;
              bool withinNormalBound2 = score >= _scoreWarningIndicatorSensitivity * (1.0f / 0.75f);
              //el.style.backgroundColor = withinNormalBound ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0f, 0f, 0.2f);
              el.style.unityBackgroundImageTintColor = withinNormalBound ?
                withinNormalBound2 ?
                new Color(0x34 / 255.0f, 0x5a / 255.0f, 0x9e / 255.0f) :
                new Color(0x34 / 255.0f, 0x9e / 255.0f, 0x5a / 255.0f) :

                new Color(0xb2 / 255.0f, 0x31 / 255.0f, 0x40 / 255.0f);
            }
          }
        }
      }

      void DrawRelativeAngleWithoutScore(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                                         SpinalAlignment.RelativeAngleId id,
                                         string label, Vector2 labelOffset, Color color, int n)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        float angle = spinalAlignment.relativeAngles[id];
        // float score = spinalAlignmentScore.relativeAngleScores[id];
        float score = 1.0f; // do not use relativeAngleScores
        DrawAngle_(pos0, pos1, pos2,
                   angle,
                   score,
                   label, labelOffset, color, n);
      }

      void DrawRelativeAngle(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                             SpinalAlignment.RelativeAngleId id,
                             SpinalAlignment.AbsoluteAngleId idAbs,
                             string label, Vector2 labelOffset, Color color, int n)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        float angle = spinalAlignment.relativeAngles[id];
        float score = spinalAlignmentScore.absoluteAngleScores[idAbs];
        DrawAngle_(pos0, pos1, pos2,
                   angle,
                   score,
                   label, labelOffset, color, n);
      }

      void DrawAbsoluteAngleWithoutScore(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                                         SpinalAlignment.AbsoluteAngleId id,
                                         string label, Vector2 labelOffset, Color color, int n)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        float angle = 90f - spinalAlignment.absoluteAngles[id];
        float score = 1.0f; // do not use relativeAngleScores
        DrawAngle_(pos0, pos1, pos2,
                   angle,
                   score,
                   label, labelOffset, color, n, true);
      }

      void DrawAbsoluteAngle(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                             SpinalAlignment.AbsoluteAngleId id,
                             string label, Vector2 labelOffset, Color color, int n)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        float angle = 90f - spinalAlignment.absoluteAngles[id];
        float score =
          !spinalAlignmentScore.absoluteAngleScores.ContainsKey(id) ? 1f : spinalAlignmentScore.absoluteAngleScores[id];
        DrawAngle_(pos0, pos1, pos2,
                   angle,
                   score,
                   label, labelOffset, color, n, true);
      }

      var pos_s = _refS.position;
      var pos_l3 = DrawSegment(pos_s, _dist_S_L3, SpinalAlignment.AbsoluteAngleId.L3_S);
      var pos_t12 = DrawSegment(pos_l3, _dist_L3_T12, SpinalAlignment.AbsoluteAngleId.T12_L3);
      var pos_t8 = DrawSegment(pos_t12, _dist_T12_T8, SpinalAlignment.AbsoluteAngleId.T8_T12);
      var pos_t3 = DrawSegment(pos_t8, _dist_T8_T3, SpinalAlignment.AbsoluteAngleId.T3_T8);
      var pos_c7 = DrawSegment(pos_t3, _dist_T3_C7, SpinalAlignment.AbsoluteAngleId.C7_T3);
      var pos_c2 = DrawSegment(pos_c7, _dist_C7_C2, SpinalAlignment.AbsoluteAngleId.C2_C7);
      // pos0 = NextPos(pos0, _dist_EyePost_EyeAnt, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.EyePost);
      var headJointOffset = new Vector3(-0.086f, 0.102f, 0f); // FIXME: scale 
      headJointOffset *= scale * 0.25f;
      var pos_eyepost = pos_c2 + headJointOffset + Quaternion.AngleAxis(-face_pitch, Vector3.forward) * (new Vector3(-0.480f, 0.200f, 0f) * scale * 0.25f - headJointOffset); // FIXME: scale
      var vec_sight = Quaternion.AngleAxis(-face_pitch, Vector3.forward) * Vector3.left * 0.5f * scale * 0.25f; // FIXME: scale
      var off_headCenter = new Vector3(-0.185f, 0.257f, 0f);
      off_headCenter *= scale * 0.25f;
      {
        using (Draw.ingame.WithColor(Color.gray)) {
          DrawSegment_(pos_t3 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
          DrawSegment_(pos_c7 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
          if (_alignmentValueDisplayMode == AlignmentValueDisplayModeEnum.ShowAbsoluteAngles) {
            DrawSegment_(pos_t3 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
            DrawSegment_(pos_t8 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
            DrawSegment_(pos_t12 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
            DrawSegment_(pos_l3 + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
            DrawSegment_(pos_s + scale * 0.2f * Vector3.up * 0.25f, -0.2f * Vector3.up * scale * 0.25f, true);
          }
          DrawSegment_(pos_eyepost + scale * 0.5f * 0.25f * Vector3.left, -0.5f * Vector3.left * scale * 0.25f, true);
        }
        DrawSegment_(pos_eyepost, vec_sight, true);
        var normal = Vector3.back;
        var radius = 0.35f * scale * 0.25f;
        var headCenterPos = pos_c2 + headJointOffset + Quaternion.AngleAxis(-face_pitch, Vector3.forward) * (off_headCenter - headJointOffset); // FIXME: scale 
        var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero);
        offset *= scale;
        using (Draw.ingame.WithLineWidth(SmallScreenMode ? 3f : 1f)) {
          Draw.ingame.Circle(offset + headCenterPos, normal, radius, Color.white);
        }
      }

      if (_alignmentValueDisplayMode == AlignmentValueDisplayModeEnum.Hidden)
        return;

      float length = !verbose ? 0.1f : 0.4f;
      var color0 = new Color(0.5f, 1.0f, 1.0f);
      var color1 = new Color(1.0f, 0.5f, 1.0f);
      int n = 0;
      switch (_alignmentValueDisplayMode) {
      case AlignmentValueDisplayModeEnum.ShowRelativeAngles:
        DrawRelativeAngle(pos_c7 + Vector3.up * length,
                          pos_c7,
                          pos_c2,
                          SpinalAlignment.RelativeAngleId.C2_C7_vert_new,
                          SpinalAlignment.AbsoluteAngleId.C2_C7,
                          "C2_C7_vert", new Vector2(10f, -55f), color0, n++);
        DrawRelativeAngle(pos_t3 + Vector3.up * length,
                          pos_t3,
                          pos_c7,
                          SpinalAlignment.RelativeAngleId.C7_T3_vert_new,
                          SpinalAlignment.AbsoluteAngleId.C7_T3,
                          "C7_T3_vert", new Vector2(10f, -80f), color1, n++);
        // T1_slope
        DrawRelativeAngle(pos_c7,
                          pos_t3,
                          pos_t8,
                          SpinalAlignment.RelativeAngleId.C7_T3_T8,
                          SpinalAlignment.AbsoluteAngleId.C7_T3,
                          "C7_T3_T8", new Vector2(25f, 40f), color0, n++);
        DrawRelativeAngle(pos_t3,
                          pos_t8,
                          pos_t12,
                          SpinalAlignment.RelativeAngleId.T3_T8_T12,
                          SpinalAlignment.AbsoluteAngleId.T3_T8,
                          "T3_T8_T12", new Vector2(10f, 0f), color1, n++);
        DrawRelativeAngleWithoutScore(pos_t8,
                                      pos_t12,
                                      pos_l3,
                                      SpinalAlignment.RelativeAngleId.T8_T12_L3,
                                      "T8_T12_L3", new Vector2(10f, 0f), color0, n++);
        DrawRelativeAngle(pos_s,
                          pos_l3,
                          pos_t12,
                          SpinalAlignment.RelativeAngleId.T12_L3_S,
                          SpinalAlignment.AbsoluteAngleId.L3_S,
                          "T12_L3_S", new Vector2(10f, 0f), color1, n++);
        break;

      case AlignmentValueDisplayModeEnum.ShowAbsoluteAngles:
        DrawAbsoluteAngle(pos_c7 + Vector3.up * length,
                          pos_c7,
                          pos_c2,
                          SpinalAlignment.AbsoluteAngleId.C2_C7,
                          "C2_C7", new Vector2(40f, -150f), color0, n++);
        DrawAbsoluteAngle(pos_t3 + Vector3.up * length,
                          pos_t3,
                          pos_c7,
                          SpinalAlignment.AbsoluteAngleId.C7_T3,
                          "C7_T3", new Vector2(40f, -80f), color1, n++);
        DrawAbsoluteAngle(pos_t8 + Vector3.up * length,
                          pos_t8,
                          pos_t3,
                          SpinalAlignment.AbsoluteAngleId.T3_T8,
                          "T3_T8", new Vector2(20f, -80f), color1, n++);
        DrawAbsoluteAngleWithoutScore(pos_t12 + Vector3.up * length,
                                      pos_t12,
                                      pos_t8,
                                      SpinalAlignment.AbsoluteAngleId.T8_T12,
                                      "T8_T12", new Vector2(20f, -80f), color0, n++);
        DrawAbsoluteAngle(pos_l3 + Vector3.up * length,
                          pos_l3,
                          pos_t12,
                          SpinalAlignment.AbsoluteAngleId.T12_L3,
                          "T12_L3", new Vector2(30f, -80f), color1, n++);
        DrawAbsoluteAngle(pos_s + Vector3.up * length,
                          pos_s,
                          pos_l3,
                          SpinalAlignment.AbsoluteAngleId.L3_S,
                          "L3_S", new Vector2(25f, -80f), color0, n++);
        break;
      }

      {
        DrawAngle_(pos_eyepost + Vector3.left * 2.0f, pos_eyepost, pos_eyepost + vec_sight, // FIXME: scale
                   face_pitch, 1f,
                   "pitch", new Vector2(-340f, -80f), color0, n++, true);
      }

      for (; n < _alignmentValueLabelElements.Length; n++) {
        _alignmentValueLabelElements[n].visible = false;
      }
    }
  }
}
