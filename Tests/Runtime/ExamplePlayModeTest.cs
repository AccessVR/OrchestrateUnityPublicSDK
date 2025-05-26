using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

namespace AccessVR.OrchestrateVR.SDK
{
    public class ExamplePlayModeTest
    {
        [UnityTest]
        public IEnumerator PlayModeTest_Passes()
        {
            yield return null;
            Assert.IsTrue(true);
        }
    }
} 