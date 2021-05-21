using System.Collections;
using System.Collections.Generic;

namespace BareE.DataStructures
{
    public class SafeQueue<T> :IEnumerable
    {
        Queue<T> _queue;
        object SyncRoot;
        public void SafeEnqueue(T item)
        {
            lock (SyncRoot)
            {
                _queue.Enqueue(item);
            }
        }

        public T SafeDequeue()
        {
            lock (SyncRoot)
            {
                return _queue.Dequeue();
            }
        }
        public void Clear()
        {
            _queue.Clear();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) _queue.GetEnumerator();
        }
    }
}