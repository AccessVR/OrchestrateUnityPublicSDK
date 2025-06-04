using UnityEngine;
using NUnit.Framework;

namespace AccessVR.OrchestrateVR.SDK
{
    public class AssertUtils
    {
        public static void AreApproximatelyEqual(Quaternion expected, Quaternion actual, float tolerance = 1e-5f, string message = "")
        {
            // Account for double-coverage: q and -q represent the same rotation
            if (Quaternion.Dot(expected, actual) < 0f)
            {
                actual = new Quaternion(-actual.x, -actual.y, -actual.z, -actual.w);
            }

            Assert.That(actual.x, Is.EqualTo(expected.x).Within(tolerance), $"{message} (x)");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(tolerance), $"{message} (y)");
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(tolerance), $"{message} (z)");
            Assert.That(actual.w, Is.EqualTo(expected.w).Within(tolerance), $"{message} (w)");
        }

        public static void AreApproximatelyEqual(Vector3 expected, Vector3 actual, float tolerance = 1e-5f, string message = "")
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(tolerance), $"{message} (x)");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(tolerance), $"{message} (y)");
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(tolerance), $"{message} (z)");
        }

        public static void AreApproximatelyEqual(float expected, float actual, float tolerance = 1e-5f, string message = "")
        {
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), $"{message}");       ;
        }
    }
}