/*
 * Copyright 2024 GET BACK Ltd.
 *
 * Licensed for Non-Commercial Personal Use Only.
 * See LICENSE.md in this directory for more info.
 */

using System;
using System.Collections.Generic;

namespace GetBack.Spinometer.SpinometerCore
{
  public class SpinalAlignmentEstimator
  {
    [Serializable]
    public class Options
    {
      public bool useNew = true;

      public int user_sex = -1; // m:1, f:0
      public int user_birthYear = -1;
      public float user_height_cm = -1f; // not in [m]
      public float user_weight_kg = -1f;

      public float s_distance_offset = 80.0f;
      public float s_distance_multiplier_forward = 45.0f;
      public float s_distance_multiplier_backward = 120.0f;
    }

    public enum ParamName
    {
      HA = 1,
      VD = 2,
      S = 4,
      A = 8,
      H = 16,
      W = 32
    }

    public enum ParamSet
    {
      ha_vd_s_a_h_w = 0,
      HA_vd_s_a_h_w = ParamName.HA,
      ha_VD_s_a_h_w = ParamName.VD,
      HA_VD_s_a_h_w = ParamName.HA + ParamName.VD,
      HA_VD_S_a_h_w = ParamName.HA + ParamName.VD + ParamName.S,
      HA_VD_s_A_h_w = ParamName.HA + ParamName.VD + ParamName.A,
      HA_VD_s_a_H_w = ParamName.HA + ParamName.VD + ParamName.H,
      HA_VD_s_a_h_W = ParamName.HA + ParamName.VD + ParamName.W,
      HA_VD_S_A_h_w = ParamName.HA + ParamName.VD + ParamName.S + ParamName.A,
      HA_VD_s_a_H_W = ParamName.HA + ParamName.VD + ParamName.H + ParamName.W,
      HA_VD_S_A_H_W = ParamName.HA + ParamName.VD + ParamName.S + ParamName.A + ParamName.H + ParamName.W
    }

    /// neck only model, excerpt from 係数表.xlsx
    public static readonly Dictionary<ParamSet, float[]> angleCoeffs_C2_C7_vert_new = new() {
      // y = a*HA + b*HA^2 + c*VD + d*VD^2 + e*S + f*A + g*A^2 + h*H + i*H^2 + j*W + k*W^2 + const
      { ParamSet.ha_vd_s_a_h_w, new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
      { ParamSet.HA_vd_s_a_h_w, new[] { 0.484934548952713f, 0.00311398881034657f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 11.1007972539194f } },
      { ParamSet.ha_VD_s_a_h_w, new[] { 0f, 0f, -0.0835841684217039f, 1.61690525281094E-05f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 72.8703112333777f } },
      { ParamSet.HA_VD_s_a_h_w, new[] { -0.0993366374703265f, 0.00731697076987945f, -0.0500906569623535f, -5.91820542011101E-07f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 54.7582343484252f } },
      { ParamSet.HA_VD_S_a_h_w, new[] { 0.0401065358165494f, 0.0055151401573933f, -0.0665119448966417f, 1.49597132419251E-05f, -5.80865681350276f, 0f, 0f, 0f, 0f, 0f, 0f, 59.4403564601839f } },
      { ParamSet.HA_VD_s_A_h_w, new[] { -0.0137052757004784f, 0.00552811379982341f, -0.0588754558441095f, 4.87780112153893E-06f, 0f, 2.25273481588246f, -0.0272294116117155f, 0f, 0f, 0f, 0f, 16.364671105758f } },
      { ParamSet.HA_VD_s_a_H_w, new[] { -0.0997302265293328f, 0.00743686014660137f, -0.0467314014297802f, -2.94186078691812E-06f, 0f, 0f, 0f, -8.76472335052163f, 0.0265363595680086f, 0f, 0f, 775.707222434024f } },
      { ParamSet.HA_VD_s_a_h_W, new[] { -0.111040898437036f, 0.00753447912041302f, -0.0458688182934818f, -4.02945229660773E-06f, 0f, 0f, 0f, 0f, 0f, -0.75603871760029f, 0.00698392875573901f, 73.3024380488081f } },
      { ParamSet.HA_VD_S_A_h_w, new[] { 0.020962117981256f, 0.00509452543615939f, -0.0635693972825615f, 9.33545020084701E-06f, -2.22230715990563f, 2.17064060371178f, -0.0267527153187668f, 0f, 0f, 0f, 0f, 20.1841733165184f } },
      { ParamSet.HA_VD_s_a_H_W, new[] { -0.0872789036925648f, 0.00733264067028671f, -0.0469408772556756f, -2.52139952527048E-06f, 0f, 0f, 0f, -7.00729225120357f, 0.0208205515706113f, -0.314559794352616f, 0.00369484377973705f, 646.934658165416f } },
      { ParamSet.HA_VD_S_A_H_W, new[] { 0.0487759752929283f, 0.00490523318012583f, -0.0631162965983865f, 9.94302642502485E-06f, -3.86530573546618f, 2.19468995020504f, -0.0280091678859624f, -11.628921468101f, 0.0350286382566647f, -0.065252036408606f, 0.000639130428569629f, 985.355689453936f } }
    };

    /// neck only model, excerpt from 係数表.xlsx
    public static readonly Dictionary<ParamSet, float[]> angleCoeffs_C7_T3_vert_new = new() {
      // y = a*HA + b*HA^2 + c*VD + d*VD^2 + e*S + f*A + g*A^2 + h*H + i*H^2 + j*W + k*W^2 + const
      { ParamSet.ha_vd_s_a_h_w, new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
      { ParamSet.HA_vd_s_a_h_w, new[] { 0.599733146839498f, 0.00129725037430285f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 14.1718257192888f } },
      { ParamSet.ha_VD_s_a_h_w, new[] { 0f, 0f, -0.154227377306082f, 5.66532854931022E-05f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 104.293200543779f } },
      { ParamSet.HA_VD_s_a_h_w, new[] { -0.0849438504348709f, 0.00420637861022857f, -0.135804882237781f, 4.6601295729247E-05f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 95.3031331708744f } },
      { ParamSet.HA_VD_S_a_h_w, new[] { -0.367226424383979f, 0.00785392456836949f, -0.102562355433591f, 1.51194606425295E-05f, 11.7587871665059f, 0f, 0f, 0f, 0f, 0f, 0f, 85.8248528786426f } },
      { ParamSet.HA_VD_s_A_h_w, new[] { -0.188760501943049f, 0.00538573713676977f, -0.127566681395238f, 3.77714747605077E-05f, 0f, 0.280303603906859f, -0.00718102304843649f, 0f, 0f, 0f, 0f, 95.1534113555574f } },
      { ParamSet.HA_VD_s_a_H_w, new[] { -0.323423884221774f, 0.0065863581315501f, -0.114723684775046f, 2.41084695815157E-05f, 0f, 0f, 0f, 15.0164872491327f, -0.0432406250368682f, 0f, 0f, -1202.7796847587f } },
      { ParamSet.HA_VD_s_a_h_W, new[] { -0.259563078817813f, 0.00582543930174034f, -0.123538478863639f, 3.20907861334532E-05f, 0f, 0f, 0f, 0f, 0f, 3.40984784525274f, -0.0243870025147188f, -16.2622615955011f } },
      { ParamSet.HA_VD_S_A_h_w, new[] { -0.380237881250453f, 0.0077805627857575f, -0.10164077495103f, 1.31506748407353E-05f, 12.2744027111275f, 0.733732112177699f, -0.00981394543473807f, 0f, 0f, 0f, 0f, 74.0572689605046f } },
      { ParamSet.HA_VD_s_a_H_W, new[] { -0.443652167133574f, 0.00831520635096077f, -0.102692305669073f, 1.25318633195632E-05f, 0f, 0f, 0f, 9.11742727547404f, -0.0273725786228756f, 2.08459965887403f, -0.0156520665953735f, -734.930587586352f } },
      { ParamSet.HA_VD_S_A_H_W, new[] { -0.434365910105706f, 0.0080768023555283f, -0.10304758429817f, 1.19934162092978E-05f, 14.5582760031222f, 1.03990846256493f, -0.0133386857301428f, 6.28618842326246f, -0.0195815160663505f, 2.20316927796598f, -0.0169208714833884f, -502.272253732986f } }
    };

    /// excerpt from 60_プレゼンテーション/最新版/技術系プレゼン/発明説明資料.pptx
    public static readonly Dictionary<SpinalAlignment.RelativeAngleId, float[]> angleCoeffs = new() {
      { SpinalAlignment.RelativeAngleId.C2_C7_vert_new, new[] { 0f, 0f, 0f, 0f, 0f } },
      { SpinalAlignment.RelativeAngleId.C7_T3_vert_new, new[] { 0f, 0f, 0f, 0f, 0f } },
      { SpinalAlignment.RelativeAngleId.C2_C7_vert, new[] { 0.411f, 0.002f, -0.00107f, -0.0000328f, 32.1f } },
      { SpinalAlignment.RelativeAngleId.T1_slope, new[] { -0.159f, -0.034f, 0.00301f, -0.0000361f, 78.6f } },
      { SpinalAlignment.RelativeAngleId.C7_T3_T8, new[] { 0.542f, 0.037f, -0.00375f, 0.0000015f, 133.8f } },
      { SpinalAlignment.RelativeAngleId.T3_T8_T12, new[] { -0.021f, -0.025f, -0.00060f, 0.0000131f, 164.2f } },
      { SpinalAlignment.RelativeAngleId.T8_T12_L3, new[] { -0.103f, 0.047f, 0.00168f, -0.0000450f, 167.3f } },
      { SpinalAlignment.RelativeAngleId.T12_L3_S, new[] { -0.024f, -0.022f, 0.000115f, 0.0000102f, 180.4f } }
    };

    public readonly ParamSet[] preferredParamSet = {
      // ordered by R
      ParamSet.HA_VD_S_A_H_W, // 0.9479
      ParamSet.HA_VD_S_A_h_w, // 0.9405
      ParamSet.HA_VD_s_A_h_w, // 0.9389
      ParamSet.HA_VD_S_a_h_w, // 0.9030
      ParamSet.HA_VD_s_a_H_W, // 0.8953
      ParamSet.HA_VD_s_a_h_W, // 0.8928
      ParamSet.HA_VD_s_a_H_w, // 0.8918
      ParamSet.HA_VD_s_a_h_w, // 0.8861
      ParamSet.HA_vd_s_a_h_w, // 0.7086
      ParamSet.ha_VD_s_a_h_w, // 0.8332
      ParamSet.ha_vd_s_a_h_w

    };

    private readonly Options _options;

    public readonly int _thisYear = DateTime.Now.Year;

    public SpinalAlignmentEstimator(Options options)
    {
      _options = options;
    }

    public ParamSet AvailableParamSet()
    {
      var actualSet = (ParamSet)((int)ParamName.HA + ParamName.VD +
                                 (_options.user_sex < 0 ? 0 : (int)ParamName.S) +
                                 (_options.user_birthYear < 0 ? 0 : (int)ParamName.A) +
                                 (_options.user_height_cm < 0f ? 0 : (int)ParamName.H) +
                                 (_options.user_weight_kg < 0f ? 0 : (int)ParamName.W));

      foreach (var setToUse in preferredParamSet)
        if ((actualSet & setToUse) == setToUse)
          return setToUse;

      return 0;
    }

    public void Estimate(float distance, float pitch, SpinalAlignment alignmentOut)
    {
      // Debug.Log("UpdateAngles():  distance = " + distance + ", pitch = " + pitch);
      UpdateRelativeAngles(distance, pitch, alignmentOut);
      UpdateAbsoluteAngles(distance, pitch, alignmentOut);
    }

    private void UpdateRelativeAngles(float distance, float pitch, SpinalAlignment alignmentOut)
    {
      var relativeAngles = alignmentOut.relativeAngles;

      // FIXME: BUG: wrong direction??? // var a1 = -pitch; // a1 [deg].  facial angle.  - = looking up, 0 = looking straight ahead, + = looking down.
      var a1 = -pitch;
      var a2 = a1 * a1;
      var d1 = distance * 1000f; // d1 [mm].  facial distance.
      var d2 = d1 * d1;
      foreach (var c in angleCoeffs) {
        // Y = A1 * X1 + A2 * X2 + A3 * X1^2 + A4 * X2^2 + A5
        // X1 = 頭部傾斜角度 (deg), X2 = 視距離 (mm)
        var angleId = c.Key;
        var coeffs = c.Value;
        var UpdatedAngle = coeffs[0] * a1 + coeffs[1] * d1 + coeffs[2] * a2 + coeffs[3] * d2 + coeffs[4];
        relativeAngles[angleId] = UpdatedAngle;
      }

      // recalculate and overwrite *_new, which uses special formula and coefficients.
      {
        float calculateRelativeNew(float[] coeffs)
        {
          /* BUG: ?? wrong direction???  this should not be correct.  there is something wrong in the process... */
          // var a1 = pitch; // hide a1 with negated value as workaround to address unnatural neck rotation issue.
          /* BUG: ?? wrong direction??? */
          var age1 = _thisYear - _options.user_birthYear;
          var age2 = age1 * age1;
          var height1 = _options.user_height_cm;
          var height2 = height1 * height1;
          var weight1 = _options.user_weight_kg;
          var weight2 = weight1 * weight1;
          var y = coeffs[0] * a1 + coeffs[1] * a2 +
                  coeffs[2] * d1 + coeffs[3] * d2 +
                  coeffs[4] * _options.user_sex +
                  coeffs[5] * age1 + coeffs[6] * age2 +
                  coeffs[7] * height1 + coeffs[8] * height2 +
                  coeffs[9] * weight1 + coeffs[10] * weight2 +
                  coeffs[11];
          return y;
        }

        var paramSet = AvailableParamSet();
        // Debug.Log($"available = {paramSet}");
        // y = a*HA + b*HA^2 + c*VD + d*VD^2 + e*S + f*A + g*A^2 + h*H + i*H^2 + j*W + k*W^2 + const
        relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert_new] = calculateRelativeNew(angleCoeffs_C2_C7_vert_new[paramSet]);
        relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_vert_new] = calculateRelativeNew(angleCoeffs_C7_T3_vert_new[paramSet]);

        // Debug.Log(relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert]);
        // Debug.Log(relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert_new]);
      }

      relativeAngles[SpinalAlignment.RelativeAngleId.EyePost_C2_C7] = 90f;
      // rel_EyePost_C2_C7 = relativeAngles[RelativeAngleId.EyePost_C2_C7].ToString("0.0");

#if false
    foreach (var id_ in Enum.GetValues(typeof(RelativeAngleId))) {
      var id = (RelativeAngleId)id_;
      Debug.Log(id + ": " + relativeAngles[id]);
    }
#endif
    }

    
    private void UpdateAbsoluteAngles(float distance, float pitch, SpinalAlignment alignmentOut)
    {
      var relativeAngles = alignmentOut.relativeAngles;
      var absoluteAngles = alignmentOut.absoluteAngles;

      if (false && _options.useNew) {
        relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_T8] = 180f;
        relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert] = 180f;
        relativeAngles[SpinalAlignment.RelativeAngleId.T1_slope] = 0f;
        relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_T8] = 180f - relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_vert_new];
        relativeAngles[SpinalAlignment.RelativeAngleId.T3_T8_T12] = 180f;
        relativeAngles[SpinalAlignment.RelativeAngleId.T8_T12_L3] = 180f;
        relativeAngles[SpinalAlignment.RelativeAngleId.T12_L3_S] = 180f;
      }

      absoluteAngles[SpinalAlignment.AbsoluteAngleId.EyePost] = pitch;
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.C2] = absoluteAngles[SpinalAlignment.AbsoluteAngleId.EyePost] +
                                                           (180f - relativeAngles[SpinalAlignment.RelativeAngleId.EyePost_C2_C7]);
      if (!_options.useNew) {
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.C2_C7] = 90f - relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert];
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.C7_T3] = 90f - (relativeAngles[SpinalAlignment.RelativeAngleId.T1_slope] - 7.158f) / 0.7338f;
      } else {
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.C2_C7] = 90f - relativeAngles[SpinalAlignment.RelativeAngleId.C2_C7_vert_new];
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.C7_T3] = 90f - relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_vert_new];
      }
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.T3_T8] =
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.C7_T3] + (180f - relativeAngles[SpinalAlignment.RelativeAngleId.C7_T3_T8]);
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.T8_T12] =
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.T3_T8] + (180f - relativeAngles[SpinalAlignment.RelativeAngleId.T3_T8_T12]);
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.T12_L3] =
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.T8_T12] + (180f - relativeAngles[SpinalAlignment.RelativeAngleId.T8_T12_L3]);
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.L3_S] =
        absoluteAngles[SpinalAlignment.AbsoluteAngleId.T12_L3] + (-180f + relativeAngles[SpinalAlignment.RelativeAngleId.T12_L3_S]);

      // FIXME: treating S specially for more natural motion.
      var mult = distance < 0.5f ? _options.s_distance_multiplier_forward : _options.s_distance_multiplier_backward;
      absoluteAngles[SpinalAlignment.AbsoluteAngleId.L3_S] = _options.s_distance_offset + mult * (distance - 0.5f);

#if false
    foreach (var id_ in Enum.GetValues(typeof(AbsoluteAngleId))) {
      var id = (AbsoluteAngleId)id_;
      Debug.Log(id + ": " + absoluteAngles[id]);
    }
#endif
    }
  }
}
