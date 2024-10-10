using Drawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.SpinalAlignmentVisualizer
{
  public class SpinalAlignmentVisualizerSkeleton : MonoBehaviour
  {
    [SerializeField] private Transform _avatar_skeleton;
    [SerializeField] private bool _neckOnly = true;
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _alignmentValueLabelPrototype;

    private VisualElement _alignmentValueLabelContainer;
    private Label[] _alignmentValueLabelElements = null;

    private bool _showSkeleton;
    public bool ShowSkeleton
    {
      get => _showSkeleton;
      set
      {
        _showSkeleton = value;
        _avatar_skeleton.GetComponentInChildren<SkinnedMeshRenderer>().enabled = _showSkeleton;
      }
    }

    private bool _showAlignmentValues;
    public bool ShowAlignmentValues
    {
      get => _showAlignmentValues;
      set
      {
        _showAlignmentValues = value;
        foreach (var el in _alignmentValueLabelElements)
          el.visible = false;
      }
    }

    private Transform _bone_pelvis;
    private Transform _bone_spine_01;
    private Transform _bone_spine_02;
    private Transform _bone_spine_03;
    private Transform _bone_neck_01;
    private Transform _bone_head;

    private Transform _ref_s;
    private Transform _ref_l3;
    private Transform _ref_t12;
    private Transform _ref_t8;
    private Transform _ref_t3;
    private Transform _ref_c7;
    private Transform _ref_c2;

    [SerializeField] private float _angleAdjustment_pelvis = -15f;
    [SerializeField] private float _angleAdjustment_spine_01 = 19f;
    [SerializeField] private float _angleAdjustment_spine_02 = 4f;
    [SerializeField] private float _angleAdjustment_spine_03 = -50f;
    [SerializeField] private float _angleAdjustment_neck_01 = 0f;
    [SerializeField] private float _angleAdjustment_head = 15f;

    void Awake()
    {
      _bone_pelvis = _avatar_skeleton.Find("SK_Skeleton/root/pelvis");
      _bone_spine_01 = _bone_pelvis.Find("spine_01");
      _bone_spine_02 = _bone_spine_01.Find("spine_02");
      _bone_spine_03 = _bone_spine_02.Find("spine_03");
      _bone_neck_01 = _bone_spine_03.Find("neck_01");
      _bone_head = _bone_neck_01.Find("head");

      _ref_s = _bone_pelvis.Find("visref_s");
      _ref_l3 = _bone_spine_01.Find("visref_l3");
      _ref_t12 = _bone_spine_02.Find("visref_t12");
      _ref_t8 = _bone_spine_02.Find("visref_t8");
      _ref_t3 = _bone_spine_03.Find("visref_t3");
      _ref_c7 = _bone_spine_03.Find("visref_c7");
      _ref_c2 = _bone_neck_01.Find("visref_c2");

      /*
      _angleAdjustment_pelvis = AngleAdjustmentValue(_bone_pelvis, _ref_s, _ref_l3);
      _angleAdjustment_spine_01 = AngleAdjustmentValue(_bone_spine_01, _ref_s, _ref_l3);
      // ...

      float AngleAdjustmentValue(Transform bone, Transform ref0, Transform ref1)
      {
        // not implemented
        // perfect adjustment is not possible in this way anyway
      }
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

    private void UpdateAvatarSkeletonPoseWholeBody(SpinometerCore.SpinalAlignment spinalAlignment)
    {
      _bone_pelvis.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.L3_S]);
      _bone_spine_01.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.T12_L3] - 13f);
      _bone_spine_02.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.T8_T12]/* - 15f*/);
      _bone_spine_03.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C7_T3]/* - 15f*/);
      _bone_neck_01.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C2_C7]);
      _bone_head.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C2]/* + 15f*/);
      var thigh_l = _bone_pelvis.Find("thigh_l");
      var calf_l = thigh_l.Find("calf_l");
      var thigh_r = _bone_pelvis.Find("thigh_r");
      var calf_r = thigh_r.Find("calf_r");
      thigh_l.rotation = Quaternion.Euler(0f, 0f, 185f);
      calf_l.rotation = Quaternion.Euler(0f, 0f, -90f);
      thigh_r.rotation = Quaternion.Euler(0f, 0f, 5f);
      calf_r.rotation = Quaternion.Euler(0f, 0f, 90f);

      var upperarm_l = _bone_spine_03.Find("clavicle_l/upperarm_l");
      var lowerarm_l = upperarm_l.Find("lowerarm_l");
      var upperarm_r = _bone_spine_03.Find("clavicle_r/upperarm_r");
      var lowerarm_r = upperarm_r.Find("lowerarm_r");
      upperarm_l.rotation = new Quaternion(-0.64255f, -0.68394f, -0.26167f, 0.22558f);
      upperarm_r.rotation = new Quaternion(0.68394f, -0.64255f, 0.22558f, 0.26167f);
      upperarm_l.rotation *= Quaternion.Euler(0f, -30f, 30f);
      upperarm_r.rotation *= Quaternion.Euler(0f, -30f, 30f);
      lowerarm_l.localRotation = new Quaternion(-0.05402f, 0.07918f, 0.26341f, 0.95991f);
      lowerarm_r.localRotation = new Quaternion(-0.05402f, 0.07918f, 0.26341f, 0.95991f);
      lowerarm_l.rotation *= Quaternion.Euler(0f, 0f, 30f);
      lowerarm_r.rotation *= Quaternion.Euler(0f, 0f, 30f);
    }

    private void UpdateAvatarSkeletonPoseNeckOnly(SpinometerCore.SpinalAlignment spinalAlignment)
    {
      _bone_pelvis.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.L3_S] + _angleAdjustment_pelvis);
      _bone_spine_01.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.T12_L3] + _angleAdjustment_spine_01);
      _bone_spine_02.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.T8_T12] + _angleAdjustment_spine_02);
      _bone_spine_03.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C7_T3] + _angleAdjustment_spine_03);
      _bone_neck_01.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C2_C7] + _angleAdjustment_neck_01);
      _bone_head.rotation = Quaternion.Euler(0f, 0f, -spinalAlignment.absoluteAngles[SpinometerCore.SpinalAlignment.AbsoluteAngleId.C2] + _angleAdjustment_head);

      // sit down regardless if it is neck only or not
      var thigh_l = _bone_pelvis.Find("thigh_l");
      var calf_l = thigh_l.Find("calf_l");
      var thigh_r = _bone_pelvis.Find("thigh_r");
      var calf_r = thigh_r.Find("calf_r");
      thigh_l.rotation = Quaternion.Euler(0f, 0f, 185f);
      calf_l.rotation = Quaternion.Euler(0f, 0f, -90f);
      thigh_r.rotation = Quaternion.Euler(0f, 0f, 5f);
      calf_r.rotation = Quaternion.Euler(0f, 0f, 90f);

#if false
      var upperarm_l = spine_03.Find("clavicle_l/upperarm_l");
      var lowerarm_l = upperarm_l.Find("lowerarm_l");
      var upperarm_r = spine_03.Find("clavicle_r/upperarm_r");
      var lowerarm_r = upperarm_r.Find("lowerarm_r");
      upperarm_l.rotation = new Quaternion(-0.64255f, -0.68394f, -0.26167f, 0.22558f);
      upperarm_r.rotation = new Quaternion(0.68394f, -0.64255f, 0.22558f, 0.26167f);
      upperarm_l.rotation *= Quaternion.Euler(0f, -30f, 30f);
      upperarm_r.rotation *= Quaternion.Euler(0f, -30f, 30f);
      lowerarm_l.localRotation = new Quaternion(-0.05402f, 0.07918f, 0.26341f, 0.95991f);
      lowerarm_r.localRotation = new Quaternion(-0.05402f, 0.07918f, 0.26341f, 0.95991f);
      lowerarm_l.rotation *= Quaternion.Euler(0f, -20f, 20f);
      lowerarm_r.rotation *= Quaternion.Euler(0f, -20f, 20f);
#endif
#if true
      var ik_hand_l = _avatar_skeleton.Find("SK_Skeleton/root/ik_hand_root/ik_hand_gun/ik_hand_l");
      var ik_hand_r = _avatar_skeleton.Find("SK_Skeleton/root/ik_hand_root/ik_hand_gun/ik_hand_r");
      ik_hand_l.position = calf_l.position + new Vector3(0.08f, 0.24f, -0.05f) * _avatar_skeleton.localScale.x;
      ik_hand_l.rotation = Quaternion.Euler(20f, -160f, 175f);
      ik_hand_r.position = calf_r.position + new Vector3(0.08f, 0.24f, 0.05f) * _avatar_skeleton.localScale.x;
      ik_hand_r.rotation =  Quaternion.Euler(-160f, -20f, 175f);
#endif
    }

    public void UpdateAvatarPose(SpinometerCore.SpinalAlignment spinalAlignment)
    {
      if (!_avatar_skeleton)
        return;
      if (!spinalAlignment.absoluteAngles.ContainsKey(SpinometerCore.SpinalAlignment.AbsoluteAngleId.L3_S))
        return;

      if (_neckOnly)
        UpdateAvatarSkeletonPoseNeckOnly(spinalAlignment);
      else
        UpdateAvatarSkeletonPoseWholeBody(spinalAlignment);
    }

    public void DrawAlignment(SpinometerCore.SpinalAlignment spinalAlignment, bool verbose)
    {
      if (!spinalAlignment.absoluteAngles.ContainsKey(SpinometerCore.SpinalAlignment.AbsoluteAngleId.L3_S))
        return;

      void DrawAngle(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                     float normalCenter, float normalHalfWidth,
                     SpinometerCore.SpinalAlignment.RelativeAngleId id, string label, Vector2 labelOffset, Color color, int n)
      {
        var normalMin = normalCenter - normalHalfWidth;
        var normalMax = normalCenter + normalHalfWidth;

        float angle = spinalAlignment.relativeAngles[id];
        var offset = 1.0f * Vector3.back;
        pos0 += offset;
        pos1 += offset;
        pos2 += offset;
        var normal = Vector3.back;
        pos0 = Vector3.Lerp(pos1, pos0, 0.4f);
        pos2 = Vector3.Lerp(pos1, pos2, 0.4f);
        var vec10 = (pos0 - pos1).normalized;
        var vec12 = (pos2 - pos1).normalized;
        var radius = ((pos0 - pos1).magnitude + (pos2 - pos0).magnitude) * 0.5f * 0.2f;
        using (Draw.ingame.WithColor(color)) {
          using (Draw.ingame.WithLineWidth(1.0f)) {
            Draw.ingame.Line(pos0, pos1);
            Draw.ingame.Line(pos1, pos2);
          }

          if (!verbose) {
            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              el.visible = false;
            }
          } else {
            using (Draw.ingame.WithLineWidth(3f)) {
              Util.Drawing.DrawArc(pos1, pos1 + vec10 * radius, pos1 + vec12 * radius);
            }
            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              var screenPos = Camera.main.WorldToScreenPoint(pos1 + offset);
              var uiPosX = screenPos.x / Screen.width * _alignmentValueLabelContainer.layout.width;
              var uiPosY = (1.0f - screenPos.y / Screen.height) * _alignmentValueLabelContainer.layout.height;
              el.visible = true;
              el.text = $"{label}\n{angle:0.0}";
              el.style.left = uiPosX + labelOffset.x;
              el.style.top = uiPosY + labelOffset.y - 20f;
              el.style.color = color;
              bool withinNormalBound = angle >= normalMin && angle <= normalMax;
              el.style.backgroundColor = withinNormalBound ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0f, 0f, 0.2f);
            }
          }
        }
      }

      float length = !verbose ? 0.1f : 0.4f;
      /*
           DrawAngle(head.position + Vector3.left * length,
                     head.position,
                     neck_01.position,
                     UiDataSource.RelativeAngleId.EyePost_C2_C7,
                     "EyePost_C2_C7");
                     */
      var color0 = new Color(0.5f, 1.0f, 1.0f);
      var color1 = new Color(1.0f, 0.5f, 1.0f);
      int n = 0;
      DrawAngle(_ref_c7.position + Vector3.up * length,
                _ref_c7.position,
                _ref_c2.position,
                30f, 5f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.C2_C7_vert_new, "C2_C7_vert", new Vector2(10f, -55f), color0, n++);
      DrawAngle(_ref_t3.position + Vector3.up * length,
                _ref_t3.position,
                _ref_c7.position,
                40f, 10f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.C7_T3_vert_new, "C7_T3_vert", new Vector2(10f, -40f), color1, n++);
      // T1_slope
      DrawAngle(_ref_c7.position,
                _ref_t3.position,
                _ref_t8.position,
                150f, 8f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.C7_T3_T8, "C7_T3_T8", new Vector2(25f, 10f), color0, n++);
      DrawAngle(_ref_t3.position,
                _ref_t8.position,
                _ref_t12.position,
                155f, 1f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.T3_T8_T12, "T3_T8_T12", new Vector2(20f, 0f), color1, n++);
      DrawAngle(_ref_t8.position,
                _ref_t12.position,
                _ref_l3.position,
                177.7f, 2.5f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.T8_T12_L3, "T8_T12_L3", new Vector2(20f, 0f), color0, n++);
      DrawAngle(_ref_s.position,
                _ref_l3.position,
                _ref_t12.position,
                172.5f, 1.5f,
                SpinometerCore.SpinalAlignment.RelativeAngleId.T12_L3_S, "T12_L3_S", new Vector2(30f, 0f), color1, n++);

      for (; n < _alignmentValueLabelElements.Length; n++) {
        _alignmentValueLabelElements[n].visible = false;
      }
    }
  }
}
