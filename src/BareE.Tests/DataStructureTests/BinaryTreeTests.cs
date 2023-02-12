using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Tests.DataStructureTests
{
    [TestClass]
    public class BinaryTreeTests
    {
        [TestMethod]
        public void BinaryTree_Ordered()
        {
            BareE.DataStructures.BinaryTree<int> toTest = new DataStructures.BinaryTree<int>(Comparer<int>.Default);
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                toTest.Insert(i);
                //toTest.Enqueue(i, i);
            }
            var p = 0;
            foreach(var c in toTest.Values)
            {
                //var c = toTest.Dequeue();
                Assert.IsTrue(c >= p);
                p = c;
            }
        }
        [TestMethod]
        public void BinaryKeyValueTree_RetainsOrder()
        {
            BareE.DataStructures.BinaryTree<int> toTest = new DataStructures.BinaryTree<int>(Comparer<int>.Default);
            foreach (int i in new List<int>() { 10, 3, 5, 2, 5, 5, 2, 6, 8, 9, 2, 3, 10 })
            {
                toTest.Insert(i);
            }
            toTest.Remove(5);
            toTest.Remove(3);
            toTest.Remove(10);
            foreach (int i in new List<int>() { 101, 2, 5, 3, 10 })
            {
                toTest.Insert(i);
            }
            var p = 0;
            int[] actual = new int[15] { 2, 2, 2, 2, 3, 3, 5, 5, 5, 6, 8, 9, 10, 10, 101 };
            foreach (var c in toTest.Values)
            {
                Assert.IsTrue(c == actual[p]);
                p++;
            }
            Assert.AreEqual(p, 15);

        }


    }
}
