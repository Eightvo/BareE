using System;
using System.Collections.Generic;
using System.Linq;

namespace BareE.DataStructures
{

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
        Dictionary<String, int> _dataIdMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        List<mapData> _data = new List<mapData>();
        Queue<int> _freeIDs = new Queue<int>();

        public AliasMap()
        {
            _data.Add(new mapData(default(T)));
        }

        public bool AliasExists(String alias)
        {
            return (!String.IsNullOrEmpty(alias)) && _dataIdMap.ContainsKey(alias);
        }


        public int GetIndex(T indxOf)
        {
            if (indxOf == null || indxOf.Equals(default(T))) return 0;
            return _data.FindIndex(x => x.data.Equals(indxOf));
        }
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
        public void UnIndex(int entId)
        {
            foreach (var a in _dataIdMap.Where(x => x.Value == entId).Select(x => x.Key).ToList())
                _dataIdMap.Remove(a);
            //_data[entId].data = null;
            _data[entId] = default(mapData);
            _freeIDs.Enqueue(entId);

        }

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
        public T this[int indx]
        {
            get
            {
                return _data[indx].data;
            }
        }

        public IEnumerable<String> Keys()
        {
            foreach (String s in _dataIdMap.Keys)
                yield return s;
            yield break;
        }

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

        public int Count
        {
            get { return _data.Count(); }
        }
    }

}
