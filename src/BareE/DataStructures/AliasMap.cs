using System;
using System.Collections.Generic;
using System.Linq;

namespace BareE.DataStructures
{
    /// <summary>
    /// An alias map is meant to recyle unused space while allowing access to elements through either numeric or string identifiers.
    /// An item may be added to an alias map without a name, it will only be accessible by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AliasMap<T> : IDisposable
    {
        protected struct mapData
        {
            public T data;
            public int refCount;

            public mapData(T value)
            {
                data = value;
                refCount = 1;
            }

            public int AdjustReferences(int delta)
            {
                refCount += delta;
                return refCount;
            }

            public void ClearData()
            {
                data = default(T);
                refCount = 0;
            }
        }

        private Dictionary<String, int> _dataIdMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private List<mapData> _data = new List<mapData>();
        private Queue<int> _freeIDs = new Queue<int>();

        public AliasMap()
        {
            _data.Add(new mapData(default(T)));
        }

        /// <summary>
        /// Returns true only if an element exists by alias, even if the value of that alias is null.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public bool AliasExists(String alias)
        {
            return (!String.IsNullOrEmpty(alias)) && _dataIdMap.ContainsKey(alias);
        }

        /// <summary>
        /// Returns the the lowest index of an element equivalent to the input value.
        /// The index 0 will always be the index of Null or unmodified structs.
        /// </summary>
        /// <param name="indxOf"></param>
        /// <returns></returns>
        public int GetIndex(T indxOf)
        {
            if (indxOf == null || indxOf.Equals(default(T))) return 0;
            return _data.FindIndex(x => x.data.Equals(indxOf));
        }

        /// <summary>
        /// Add an element to the alias map by Name
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="toAlias"></param>
        /// <returns>index of element</returns>
        public int Alias(String alias, T toAlias)
        {
            if (AliasExists(alias))
            {
                int ret = _dataIdMap[alias];
                var v = _data[ret];
                v.refCount += 1;
                v.data = toAlias;
                _data[ret] = v;
                return ret;
            }

            mapData mDat = new mapData(toAlias);
            int newId = _data.Count;
            bool r = false;
            if (_freeIDs.Count > 0)
            {
                newId = _freeIDs.Dequeue();
                r = true;
            }
            if (r)
                _data[newId] = mDat;
            else
                _data.Add(mDat);
            if (!String.IsNullOrEmpty(alias))
                _dataIdMap.Add(alias, newId);
            return newId;
        }

        /// <summary>
        /// Remove an element from the alias map by name.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public bool Unalias(String alias)
        {
            if (!_dataIdMap.ContainsKey(alias))
                return true;

            var v = _data[_dataIdMap[alias]];
            v.refCount -= 1;

            if (v.refCount > 0)
                return false;

            var indx = _dataIdMap[alias];

            _data[indx].ClearData();
            _freeIDs.Enqueue(indx);
            _dataIdMap.Remove(alias);
            return true;
        }

        /// <summary>
        /// Add an element to the alias map without specifiying a name.
        /// </summary>
        /// <param name="toIndex"></param>
        /// <returns>index of element</returns>
        public int Index(T toIndex)
        {
            int newId = _data.Count;
            if (_freeIDs.Count > 0)
            {
                newId = _freeIDs.Dequeue();
                _data[newId] = new mapData(toIndex);
                return newId;
            }
            _data.Add(new mapData(toIndex));
            return newId;
        }

        /// <summary>
        /// Remove an element from the alias map by index.
        /// </summary>
        /// <param name="entId"></param>
        public void UnIndex(int entId)
        {
            foreach (var a in _dataIdMap.Where(x => x.Value == entId).Select(x => x.Key).ToList())
                _dataIdMap.Remove(a);
            //_data[entId].data = null;
            _data[entId] = default(mapData);
            _freeIDs.Enqueue(entId);
        }

        /// <summary>
        /// Access a value by Alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public T this[String alias]
        {
            get
            {
                if (!_dataIdMap.ContainsKey(alias))
                    return default(T);
                int indx = _dataIdMap[alias];
                return _data[indx].data;
            }
        }

        /// <summary>
        /// Access a value by Index
        /// </summary>
        /// <param name="indx"></param>
        /// <returns></returns>
        public T this[int indx]
        {
            get
            {
                return _data[indx].data;
            }
        }

        /// <summary>
        /// Enumerates all Aliases.
        /// Un named values are not represented by a key.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<String> Keys()
        {
            foreach (String s in _dataIdMap.Keys)
                yield return s;
            yield break;
        }

        /// <summary>
        /// Enumerates all values; Regardless of being named or not.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<object> Values()
        {
            foreach (var val in _data)
                yield return val.data;
            yield break;
        }


        public void Dispose()
        {
            foreach (var v in _data)
            {
                if ((v as IDisposable) != null)
                    (v as IDisposable).Dispose();
            }
            _data.Clear();

            _dataIdMap.Clear();

            _freeIDs.Clear();
        }

        /// <summary>
        /// Returns quantity of values stored regardless of named or not.
        /// </summary>
        public int Count
        {
            get { return _data.Count(); }
        }
    }
}