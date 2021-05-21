using System;
using System.Collections.Generic;

namespace BareE.DataStructures
{
    /// <summary>
    /// A buffer that maintains only the last N elements provided where N is the Ring Buffers capacity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        /// <summary>
        /// Maximum number of elements.
        /// </summary>
        public int Capacity;
        private object _lockObj = new object();

        private T[] _data;
        private uint head;

        private uint ptr;

        public RingBuffer(int capacity)
        {
            _data = new T[capacity];
            for (int i = 0; i < capacity; i++) _data[i] = default(T);
            Capacity = capacity;
        }


        //public void Push(T value)
        //{
        //    _data[(head) % _data.Length] = value;
        //    if (head < this.Capacity)
        //        head++;
        //    else
        //    {
        //        head++;
        //        ptr++;
        //    }
        //}

        public void Push(T value)
        {
            lock (_lockObj)
            {
                _data[(head) % _data.Length] = value;
                if (head < this.Capacity)
                    head++;
                else
                {
                    head++;
                    ptr++;
                }
            }
        }

        public IEnumerable<T> Enumerate(bool reversed = false)
        {
            lock (_lockObj)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (reversed)
                        yield return this[Count - (i + 1)];
                    else
                        yield return this[i];
                }
            }
        }

        public T this[int indx]
        {
            get
            {
                if (indx < 0 || indx >= Count)
                    throw new IndexOutOfRangeException();
                return _data[(ptr + indx) % _data.Length];
            }
        }

        public int Count
        {
            get
            {
                return (int)(head - ptr);
            }
        }

        internal void Clear()
        {
            head = 0;
            ptr = 0;
        }
    }
}