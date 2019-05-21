using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests
{
    public class SaveTest
    {
        

        [Test]
        public void SaveTestSimplePasses()
        {
        }

        [UnityTest]
        public IEnumerator SaveTestWithEnumeratorPasses()
        {
            SaveProfile p = new SaveProfile(0, 0, 0, 0, 0, 0, null, null, null, null, null, null, 0, false, false, false, false, false, 0, 0, 0, 0, 0, null);
            SaveManager.SaveProfile(p);

            SaveProfile pp = SaveManager.LoadProfile(0);
            yield return new WaitForFixedUpdate();

            Debug.Log(p.id + " " + pp.id);
            Assert.AreEqual(p.id, pp.id);           
        }
    }
}
