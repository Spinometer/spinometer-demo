using Drawing;
using UnityEngine;

namespace GetBack.Spinometer.Util
{
  public class Drawing
  {
    /// <summary>
    /// Draws an arc in ccw direction between two points around a center point using a series of smaller arcs.
    /// </summary>
    /// <remarks>
    /// This method assumes that the center, start, and end points are all on the XY plane in the global coordinate system.
    /// </remarks>
    /// <param name="center">The center point of the arc.</param>
    /// <param name="start">The starting point of the arc.</param>
    /// <param name="end">The ending point of the arc.</param>
    public static void DrawArc(Vector3 center, Vector3 start, Vector3 end)
    {
      Vector3 v0 = start - center;
      Vector3 v1 = end - center;
      float angle = Vector3.SignedAngle(v0, v1, Vector3.forward);
      angle = (angle < 0) ? 360f + angle : angle;
      while (angle > 0) {
        float angle_ = Mathf.Min(90f, angle);
        v0 = Quaternion.AngleAxis(angle_, Vector3.forward) * v0;
        end = center + v0;
        Draw.ingame.Arc(center, start, end);
        start = end;
        angle -= angle_;
      }
    }
  }
}
