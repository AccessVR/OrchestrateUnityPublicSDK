using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public class NumberUtils
    {
        public static bool AssertNotNullOrEmpty(int? number)
        {
            if (number == null || number <= 0)
            {
                throw new Exception("Number cannot be null or less than or equal to 0");
            }
            return true;
        }
        
        public static bool AssertNotNullOrEmpty(float? number)
        {
            if (number == null || number <= 0)
            {
                throw new Exception("Number cannot be null or less than or equal to 0");
            }
            return true;
        }
        
        public static bool AssertNotNullOrEmpty(double? number)
        {
            if (number == null || number <= 0)
            {
                throw new Exception("Number cannot be null or less than or equal to 0");
            }
            return true;
        }

        public static bool AreApproximatelyEqual(float a, float b, float tolerance = 1e-5f)
        {
            return Math.Abs(a - b) <= tolerance;
        }

        public static bool AreApproximatelyEqual(double a, double b, float tolerance = 1e-5f)
        {
            return Math.Abs(a - b) <= tolerance;
        }
    }
}