using System.Collections;
using System.Collections.Generic;

namespace BareE.DataStructures
{

    public class SafeQueue<T> : Queue<T>
    {
        public void SafeEnqueue(T item)
        {
            ICollection ic = (ICollection)this;
            lock (ic.SyncRoot)
            {
                this.Enqueue(item);
            }
        }
        public T SafeDequeue()
        {
            ICollection ic = (ICollection)this;
            lock (ic.SyncRoot)
            {
                return this.Dequeue();
            }
        }
    }
}
