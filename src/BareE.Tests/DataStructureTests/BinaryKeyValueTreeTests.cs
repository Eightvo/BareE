using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BareE.Tests.DataStructureTests
{
    [TestClass]
    public class BinaryKeyValueTreeTests
    {
        [TestMethod]
        public void BinaryKeyValueTree_Ordered()
        {
            BareE.DataStructures.BinaryKeyValueTree<int, String> toTest = new DataStructures.BinaryKeyValueTree<int,String>(Comparer<int>.Default);
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                toTest.Insert(i, i.ToString());
            }
            var p = 0;
            foreach (var c in toTest.Keys)
            {
                Assert.IsTrue(c >= p);
                p = c;
            }
        }

        [TestMethod]
        public void BinaryKeyValueTree_SingleValuePerKey()
        {
            BareE.DataStructures.BinaryKeyValueTree<int, String> toTest = new DataStructures.BinaryKeyValueTree<int, String>(Comparer<int>.Default);
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                toTest.Insert(i, i.ToString());
            }
            Assert.IsTrue(toTest.ContainsKey(5));
            toTest.Remove(5);
            Assert.IsFalse(toTest.ContainsKey(5));
        }


        [TestMethod]
        public void BinaryKeyValueTree_RetainsOrder()
        {
            BareE.DataStructures.BinaryKeyValueTree<int, String> toTest = new DataStructures.BinaryKeyValueTree<int, String>(Comparer<int>.Default);
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                toTest.Insert(i, i.ToString());
            }
            toTest.Remove(5);
            toTest.Remove(3);
            toTest.Remove(10);
            foreach (int i in new List<int>() { 101, 2, 5, 3, 10 })
            {
                toTest.Insert(i, i.ToString());
            }
            var p = 0;
            foreach (var c in toTest.Keys)
            {
                Assert.IsTrue(c >= p);
                p = c;
            }

        }


        [TestMethod]
        public void BinaryKeyValueTree_LookupCorrect()
        {
            BareE.DataStructures.BinaryKeyValueTree<int, String> toTest = new DataStructures.BinaryKeyValueTree<int, String>(Comparer<int>.Default);
            int z = 0;
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                z++;
                toTest.Insert(i, z.ToString());
            }
            toTest.Remove(5);
            toTest.Remove(3);
            toTest.Remove(10);
            foreach (int i in new List<int>() { 101, 2, 5, 3, 10 })
            {
                toTest.Insert(i, i.ToString());
            }

            Assert.AreEqual("10", toTest[10]);
            Assert.AreEqual("101", toTest[101]);
            Assert.AreEqual("8", toTest[6]);

        }

    }
}
