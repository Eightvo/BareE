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
        public void PriorityQueueTest_Ordered()
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

        [TestMethod]
        public void PriorityQueueTest_WellOrdered()
        {
            BareE.DataStructures.PriorityQueue<int> toTest = new DataStructures.PriorityQueue<int>();
            toTest.Enqueue(3, 5); //Enque Element to receive third.
            toTest.Enqueue(4, 5); //Enqueue Element to receive fourth.
            toTest.Enqueue(1, 1); //Enqueue Element to Recieve First
            toTest.Enqueue(2, 2); //Enqueue Element to receive Second.
            toTest.Enqueue(5, 5); //Enqueue Element to Receive Fith.
            Assert.AreEqual(toTest.Dequeue(), 1);
            Assert.AreEqual(toTest.Dequeue(), 2);
            Assert.AreEqual(toTest.Dequeue(), 3);
            Assert.AreEqual(toTest.Dequeue(), 4);
            Assert.AreEqual(toTest.Dequeue(), 5);

        }


    }
}
