using System;
using System.Collections.Generic;

namespace BareE.DataStructures
{
    public class PriorityQueue<T>
    {
        private class PQNode
        {
            public T Value { get; set; }
            public long Weight { get; set; }
            public PQNode Left { get; set; }
            public PQNode Right { get; set; }

            public PQNode(T value, long weight)
            {
                Value = value;
                Weight = weight;
            }

            internal void Enqueue(PQNode element)
            {
                if (element.Weight < this.Weight)
                {
                    if (this.Left == null)
                        this.Left = element;
                    else
                        this.Left.Enqueue(element);
                }
                else
                {
                    if (this.Right == null)
                        this.Right = element;
                    else
                        this.Right.Enqueue(element);
                }
            }

            internal IEnumerable<T> Elements
            {
                get
                {
                    if (Left != null)
                        foreach (var e in Left.Elements)
                            yield return e;
                    yield return Value;
                    if (Right != null)
                        foreach (var e in Right.Elements)
                            yield return e;
                }
            }
        }

        private PQNode Head;

        public void Enqueue(T value, long weight)
        {
            if (Head == null)
                Head = new PQNode(value, weight);
            else
                Head.Enqueue(new PQNode(value, weight));
        }

        public T Dequeue()
        {
            if (Head == null) throw new IndexOutOfRangeException("Queue is empty");
            T ret;
            if (Head.Left == null)
            {
                ret = Head.Value;
                Head = Head.Right;
                return ret;
            }
            var cP = Head;
            var h = cP.Left;
            while (h.Left != null)
            {
                cP = h;
                h = h.Left;
            }
            ret = h.Value;
            cP.Left = h.Right;
            return ret;
        }

        public bool IsEmpty { get { return Head == null; } }

        public object Current { get; set; } = null;

        private PQNode _currentNode = null;
        private PQNode _currentNodeParent = null;

        public long PeekWeight()
        {
            if (Head == null) throw new IndexOutOfRangeException("Queue is empty");
            var h = Head;
            while (h.Left != null)
                h = h.Left;
            return h.Weight;
        }

        public T Peek()
        {
            if (Head == null) throw new IndexOutOfRangeException("Queue is empty");
            var h = Head;
            while (h.Left != null)
                h = h.Left;
            return h.Value;
        }

        public IEnumerable<T> Elements
        {
            get
            {
                if (Head == null) yield break;
                foreach (var e in Head.Elements)
                    yield return e;
            }
        }
    }
}