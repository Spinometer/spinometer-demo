using System.Collections.Generic;
using GetBack.Spinometer.SpinalAlignmentCore;
using Newtonsoft.Json;

namespace GetBack.Spinometer.SpinalAlignmentAux
{
  public class SpinalAlignmentScore
  {
    public Dictionary<SpinalAlignment.RelativeAngleId, float> normalizedRelativeAngles = new();
    public Dictionary<SpinalAlignment.RelativeAngleId, float> relativeAngleScores = new();

    public SpinalAlignmentScore Clone()
    {
      var serialized = JsonConvert.SerializeObject(this);
      var cloned = JsonConvert.DeserializeObject<SpinalAlignmentScore>(serialized);
      return cloned;
    }
  }
}
