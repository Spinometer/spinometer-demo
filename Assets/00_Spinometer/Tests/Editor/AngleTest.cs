using GetBack.Spinometer.SpinalAlignmentCore;
using NUnit.Framework;

public class AngleTest
{
  [Test]
  public void AngleTestSimplePasses()
  {
    var options = new SpinalAlignmentEstimator.Options();
    var alignment = new SpinalAlignment();
    var estimator = new SpinalAlignmentEstimator(options);
    estimator.Estimate(0.243f, -29.3f, alignment);
    estimator.Estimate(0.5f, -15f, alignment);
    estimator.Estimate(0.5f, 15f, alignment);
    estimator.Estimate(0.5f, 5f, alignment);
    estimator.Estimate(0.5f, -5f, alignment);
    estimator.Estimate(0.4f, -5f, alignment);
    estimator.Estimate(0.3f, -5f, alignment);
  }
}
