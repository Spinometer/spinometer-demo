/*
 * Copyright 2024 GET BACK Ltd.
 *
 * Licensed for Non-Commercial Personal Use Only.
 * See LICENSE.md in this directory for more info.
 */

using System.Collections.Generic;
using Newtonsoft.Json;

namespace GetBack.Spinometer.SpinometerCore
{
  public class SpinalAlignment
  {
    public enum RelativeAngleId
    {
      C2_C7_vert_new = 8,
      C7_T3_vert_new = 9,
      C2_C7_vert = 0,
      T1_slope = 1,
      C7_T3_T8 = 2,
      T3_T8_T12 = 3,
      T8_T12_L3 = 4,
      T12_L3_S = 5,
      EyePost_C2_C7 = 6,
      EyeAnt_EyePost_C2 = 7
    }

    public enum AbsoluteAngleId
    {
      /*
       * 頸椎 C  Cervical vertebrae
       * 胸椎 T  Thoracic vertebrae
       * 腰椎 L  Lumbar vertebrae
       * 仙骨 S  Sacrum
       * 尾骨 Co Coccyx
       */
      EyePost,
      C2, // Mechanim の Head (おおよそ)
      C2_C7, // Neck
      C7_T3, // UpperChest
      T3_T8, // Chest
      T8_T12,
      T12_L3, // Spine
      L3_S // Hips
    }

    /// "the alignment"
    public Dictionary<RelativeAngleId, float> relativeAngles = new();

    /// absolute angles in deg.  forward = 0, up = 90
    public Dictionary<AbsoluteAngleId, float> absoluteAngles = new();

    public SpinalAlignment Clone()
    {
      var serialized = JsonConvert.SerializeObject(this);
      var cloned = JsonConvert.DeserializeObject<SpinalAlignment>(serialized);
      return cloned;
    }
  }
}
