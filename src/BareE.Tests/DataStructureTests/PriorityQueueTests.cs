using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

namespace BareE.Tests.DataStructureTests
{
    [TestClass]
    public class PriorityQueueTests
    {
        [TestMethod]
        public void PriorityQueueTest_WellOrdered()
        {
            BareE.DataStructures.PriorityQueue<int> toTest = new DataStructures.PriorityQueue<int>();
            foreach(int i in new List<int>() { 10,3,5,2,5,5,2,6,8,9,2,3,10})
            {
                toTest.Enqueue(i, i);
            }
            var p = 0;
            while(!toTest.IsEmpty)
            {
                var c = toTest.Dequeue();
                Assert.IsTrue(c >= p);
                p = c;
            }
        }
    }
}
