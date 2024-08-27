using GetBack.Spinometer;
using GetBack.Spinometer.SpinalAlignment;
using NUnit.Framework;
using UnityEngine;

public class AngleTest
{
  [Test]
  public void AngleTestSimplePasses()
  {
    var settings = ScriptableObject.CreateInstance<Settings>();
    var o = ScriptableObject.CreateInstance<UiDataSource>();
    var alignment = new SpinalAlignment();
    var estimator = new SpinalAlignmentEstimator(settings, o);
    o.distance = 0.243f;
    o.pitch = -29.3f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.5f;
    o.pitch = -15f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.5f;
    o.pitch = 15f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.5f;
    o.pitch = 5f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.5f;
    o.pitch = -5f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.4f;
    o.pitch = -5f;
    estimator.Estimate(o.distance, o.pitch, alignment);
    o.distance = 0.3f;
    o.pitch = -5f;
    estimator.Estimate(o.distance, o.pitch, alignment);
  }
}
