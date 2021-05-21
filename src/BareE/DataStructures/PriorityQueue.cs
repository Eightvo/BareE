using System.Collections;
using System.Collections.Generic;

namespace BareE.DataStructures
{
    public class PriorityQueue<T> : IEnumerable<T>
    {
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