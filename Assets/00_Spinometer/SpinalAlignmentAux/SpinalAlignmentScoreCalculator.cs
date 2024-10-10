using GetBack.Spinometer.SpinometerCore;
using UnityEngine;

namespace GetBack.Spinometer.SpinalAlignmentAux
{
  public class SpinalAlignmentScoreCalculator
  {
    public static void CalculateScore(SpinalAlignment alignment_in, SpinalAlignmentScore score_out)
    {
      CalculateNormalizedAngles(alignment_in, score_out);
      CalculateScore_(score_out);
    }

    private static float NormalizedAngle(float angle, float normalCenter, float normalHalfWidth)
    {
      return (angle - normalCenter) / normalHalfWidth;
    }

    private static void CalculateNormalizedAngles(SpinalAlignment alignmentIn, SpinalAlignmentScore scoreOut)
    {
      void calc(float normalCenter, float normalHalfWidth, SpinalAlignment.RelativeAngleId id)
      {
        if (!alignmentIn.relativeAngles.ContainsKey(id))
          return;
        var angle = alignmentIn.relativeAngles[id];
        scoreOut.normalizedRelativeAngles[id] = NormalizedAngle(angle, normalCenter, normalHalfWidth);
      }

      calc(31.3f, 5f,
           SpinalAlignment.RelativeAngleId.C2_C7_vert_new);
      calc(41.7f, 10f,
           SpinalAlignment.RelativeAngleId.C7_T3_vert_new);
      calc(158.4f, 8f,
           SpinalAlignment.RelativeAngleId.C7_T3_T8);
      calc(155.0f, 1f,
           SpinalAlignment.RelativeAngleId.T3_T8_T12);
      calc(178.3f, 2.5f,
           SpinalAlignment.RelativeAngleId.T8_T12_L3);
      calc(172.0f, 1.5f,
           SpinalAlignment.RelativeAngleId.T12_L3_S);
    }

    private static void CalculateScore_(SpinalAlignmentScore scoreOut)
    {
      foreach (var kv in scoreOut.normalizedRelativeAngles) {
        var score = -(Mathf.Abs(kv.Value) - 1.0f);
        scoreOut.relativeAngleScores[kv.Key] = score;
      }
    }
  }
}
