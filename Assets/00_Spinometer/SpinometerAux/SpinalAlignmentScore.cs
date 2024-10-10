using System.Collections.Generic;
using GetBack.Spinometer.SpinometerCore;
using Newtonsoft.Json;

namespace GetBack.Spinometer.SpinometerAux
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
