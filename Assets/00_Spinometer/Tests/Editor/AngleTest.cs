using GetBack.Spinometer;
using GetBack.Spinometer.SpinalAlignmentCore;
using NUnit.Framework;
using UnityEngine;

public class AngleTest
{
  [Test]
  public void AngleTestSimplePasses()
  {
    var settings = ScriptableObject.CreateInstance<Settings>();
    var alignment = new SpinalAlignment();
    var estimator = new SpinalAlignmentEstimator(settings);
    estimator.Estimate(0.243f, -29.3f, alignment);
    estimator.Estimate(0.5f, -15f, alignment);
    estimator.Estimate(0.5f, 15f, alignment);
    estimator.Estimate(0.5f, 5f, alignment);
    estimator.Estimate(0.5f, -5f, alignment);
    estimator.Estimate(0.4f, -5f, alignment);
    estimator.Estimate(0.3f, -5f, alignment);
  }
}
