using System;
using System.Collections;
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
                if (element.Weight<this.Weight)
                {
                    if (this.Left == null)
                        this.Left = element;
                    else
                        this.Left.Enqueue(element);
                } else
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
            if (Head.Left==null)
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

        private PQNode _currentNode=null;
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


    public class PriorityQueueOld<T> : IEnumerable<T>
    {
        //public struct BinaryTreeNode<T>

        internal readonly struct PriorityNode
        {
            public readonly T Cell;
            public readonly long Cost;

            public PriorityNode(T cell, long cost)
            {
                Cell = cell;
                Cost = cost;
            }
        }

        private readonly LinkedList<PriorityNode> _list = new LinkedList<PriorityNode>();

        public void Push(T cell, long cost)
        {
            LinkedListNode<PriorityNode> node = _list.First;
            if (node == null || node.Value.Cost > cost)
            {
                _list.AddFirst(new PriorityNode(cell, cost));
            }
            else
            {
                while (node.Next != null && node.Next.Value.Cost <= cost)
                {
                    node = node.Next;
                }
                _list.AddAfter(node, new PriorityNode(cell, cost));
            }
        }

        public T Pop()
        {
            T cell = _list.First.Value.Cell;
            _list.RemoveFirst();
            return cell;
        }

        public T Peek()
        {
            return _list.First.Value.Cell;
        }

        public long PeekWeight()
        {
            return _list.First.Value.Cost;
        }

        public bool IsEmpty()
        {
            return _list.Count == 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PriorityQueueEnumerator(this._list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        private class PriorityQueueEnumerator : IEnumerator<T>
        {
            private LinkedListNode<PriorityNode> toEnumerate;
            private LinkedListNode<PriorityNode> curr;

            public PriorityQueueEnumerator(LinkedList<PriorityNode> toenum)
            {
                toEnumerate = toenum.First;
                curr = null;
            }

            public T Current { get { return curr.Value.Cell; } }

            object IEnumerator.Current { get => Current; }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (curr == null)
                    curr = toEnumerate;
                else
                    curr = curr.Next;
                return curr != null;
            }

            public void Reset()
            {
                curr = toEnumerate;
            }
        }
    }
}