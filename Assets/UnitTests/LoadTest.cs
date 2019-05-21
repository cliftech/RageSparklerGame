using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests
{
    public class LoadTest
    {
        

        [Test]
        public void LoadTestSimplePasses()
        {
        }

        [UnityTest]
        public IEnumerator LoadTestWithEnumeratorPasses()
        {
            SaveProfile p = SaveManager.LoadProfile(0);            

            yield return new WaitForFixedUpdate();

            Assert.Zero(p.id);            
        }
    }
}
