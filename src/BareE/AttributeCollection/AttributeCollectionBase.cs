using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public abstract class AttributeCollectionBase : IAttributeCollection, INotifyPropertyChanged, ICustomTypeDescriptor
    {
        static int _LAST_ENTITY_ID = 0;
        int _entID;

        [JsonIgnore]
        public int EntityID { get { return _entID; } }

        [JsonIgnore]
        public abstract DataAccessContainer DataAccess { get; set; }

        [JsonIgnore]
        protected bool _dirty;

        [JsonIgnore]
        public bool Dirty
        {
            get
            {
                if (_dirty) return true;
                foreach (AttributeValue eav in _data.Values)
                    if (eav._isPersisted && eav.Dirty)
                        return true;
                    else if (eav.Value as AttributeCollectionBase != null)
                    {
                        if ((eav.Value as AttributeCollectionBase).Dirty)
                            return true;
                    }
                return false;
            }
            set
            {
                _dirty = value;
                foreach (AttributeValue eav in _data.Values)
                    if (eav._isPersisted)
                        eav.Dirty = value;
            }
        }
        protected bool _persisted;
        protected bool _deleted;

        protected internal List<String> KeyAttributes = new List<string>();
        [JsonProperty("Attributes")]
        protected internal Dictionary<String, AttributeValue> _data = new Dictionary<string, AttributeValue>(StringComparer.CurrentCultureIgnoreCase);
        protected void AddAttribute(AttributeValue eav)
        {
            if (eav.IsKey)
                KeyAttributes.Add(eav.AttributeName);
            _data.Add(eav.AttributeName, eav);
        }


        [Browsable(false)]
        [JsonIgnore]
        public List<AttributeValue> Attributes
        {
            get
            {
                return _data.Values.ToList<AttributeValue>();
            }

        }
        protected virtual void InitializeData()
        {
            _data = new Dictionary<string, AttributeValue>(StringComparer.CurrentCultureIgnoreCase);
            _entID = ++_LAST_ENTITY_ID;
            KeyAttributes = new List<string>();
        }
        public virtual bool IsKey(string attributeName) { return _data[attributeName].IsKey; }
        public virtual bool IsPersistedAttribute(string attributeName) { return _data[attributeName]._isPersisted; }
        public bool IsDirty(String key)
        {
            if (_data.ContainsKey(key))
                return _data[key].Dirty;
            return false;
        }

        public void SetDirty(String key, Boolean dirtyState)
        {
            if (_data.ContainsKey(key))
                _data[key].Dirty = dirtyState;
        }
        public void SetDirty(String key, object value, bool dirtyState=true)
        {
           
        }

        public virtual bool IsReadOnly(String attributeName) { return _data[attributeName].IsReadOnly; }

        public void UseData(AttributeCollectionBase src)
        {
            this._data = src._data;
        }
        private void SetIndexedValue(object obj, String indx, object value)
        {
            if (obj.GetType().IsArray)
            {
                Array array = obj as Array;
                int arrayRank = obj.GetType().GetArrayRank();
                string[] indicesStr = indx.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int[] intIndices = new int[arrayRank];

                if (indicesStr.Length < arrayRank)
                {
                    Console.WriteLine("Returning null b/c invalid indexes");
                    return;
                }


                for (int ii = 0; ii < arrayRank; ii++)
                {
                    intIndices[ii] = int.Parse(indicesStr[ii]);
                }
                if (indicesStr.Length > arrayRank)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int z = 0; z < arrayRank; z++)
                        sb.AppendFormat("{0},", indicesStr[z]);
                    SetIndexedValue(array.GetValue(intIndices), sb.ToString(), value);
                } else
                    array.SetValue(value, intIndices);
                return;
            }
            var subOjb = obj.GetType().GetProperty("Item");
            if (subOjb == null) return;
            //obj.GetType().p

            var indxParms = subOjb.GetIndexParameters();
            String[] indices = indx.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (indices.Length < indxParms.Length)
            {
                Console.WriteLine("Returning null b/c invalid indexes");
                return;
            }

            object[] index = new object[indxParms.Length];
            int j = 0;
            foreach (var iParm in indxParms)
            {
                index[j] = Convert.ChangeType(indices[j], indxParms[j].ParameterType);
            }

            if (indices.Length > indxParms.Length)
            {
                StringBuilder sb = new StringBuilder();
                for (int z = indxParms.Length; z < indices.Length; z++)
                    sb.AppendFormat("{0},", indices[z]);
                SetIndexedValue(subOjb.GetValue(obj, index), sb.ToString(), value);
                return;
            } else
            {
                //var t = ;
                subOjb.SetValue(obj, value, index);

            }

        }

        private void SetIndexedValue(String key, String indx, object value)
        {
            var p = TypeDescriptor.GetProperties(this)[key];
            var obj = p.GetValue(this);
            if (obj == null) return;
            SetIndexedValue(obj, indx, value);

        }

        private object GetIndexedValue(Object obj, String indx)
        {
            if (obj.GetType().IsArray)
            {
                Array array = obj as Array;
                int arrayRank = obj.GetType().GetArrayRank();
                string[] indicesStr = indx.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (indicesStr.Length < arrayRank)
                {
                    Console.WriteLine("Returning null b/c invalid indexes");
                    return null;
                }


                int[] intIndices = new int[arrayRank];
                for (int ii = 0; ii < arrayRank; ii++)
                {
                    intIndices[ii] = int.Parse(indicesStr[ii]);
                }
                if (indicesStr.Length == arrayRank)
                    return array.GetValue(intIndices);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int z = arrayRank; z < indicesStr.Length; z++)
                        sb.AppendFormat("{0},", indicesStr[z]);

                    return GetIndexedValue(array.GetValue(intIndices), sb.ToString());
                }
            }
            var subOjb = obj.GetType().GetProperty("Item");
            if (subOjb == null) return null;
            //obj.GetType().p

            var indxParms = subOjb.GetIndexParameters();
            String[] indices = indx.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            while (indices.Length > 0)
            {
                if (indices.Length < indxParms.Length)
                {
                    Console.WriteLine("Too few Indices");
                    return null;
                }

                object[] index = new object[indxParms.Length];
                int j = 0;
                foreach (var iParm in indxParms)
                {
                    index[j] = Convert.ChangeType(indices[j], indxParms[j].ParameterType);
                }

                if (indices.Length > indxParms.Length)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int z = indxParms.Length; z < indices.Length; z++)
                        sb.AppendFormat("{0},", indices[z]);
                    return GetIndexedValue(subOjb.GetValue(obj, index), sb.ToString());
                } else
                {
                    return subOjb.GetValue(obj, index);
                }

            }
            return null;
        }

        private object GetIndexedValue(String key, String indx)
        {
            var p = TypeDescriptor.GetProperties(this)[key];
            if (p == null) return null;
            var obj = p.GetValue(this);
            return GetIndexedValue(obj, indx);
        }

        /// <summary>
        /// Recursivly searches entities to find a particular attribute which may be either an entry in the entity data
        /// or a property of the entitiy.
        /// </summary>
        /// <param name="key">Path to attribute to find. [ent1.][ent2.][...][ent3.]Attribute</param>
        /// <returns>get/sets value of attribute</returns>
        public object this[string key]
        {
            get
            {
                int bS = key.IndexOf('[');
                int bE = key.IndexOf(']');
                String indx = String.Empty;
                int i = key.IndexOf(".");
                if (i < 0)
                {
                    //No "." found, so it is looking for an attribute of this entity
                    bS = key.IndexOf('[');
                    bE = key.IndexOf(']');
                    indx = String.Empty;
                    if (bS > 0 && bE > 0)
                    {
                        if (bE == key.Length - 1)
                        {

                            indx = key.Substring(bS + 1, (bE - bS) - 1);
                            key = key.Substring(0, bS);
                            return GetIndexedValue(key, indx);
                        }
                    }

                    if (!_data.ContainsKey(key))
                    {
                        //Was not found in _data so look for a property
                        PropertyDescriptor p = TypeDescriptor.GetProperties(this)[key];
                        if (p == null)
                            //No property found return null
                            return null;
                        //Property found return value
                        if (!String.IsNullOrEmpty(indx))
                            return p.GetValue(this);

                    }
                    //Attribute found in _data return value
                    if (String.Compare(key, "EntityId", true)==0)
                        return this.EntityID;

                    return _data[key].Value;
                }
                //Looking for an attribute of a related entity.
                //superKey is the name of the attribute of this entity that links to the sub entity
                //subkey is the name of the attribute on the linked entity being looked for
                String superKey = key.Substring(0, i);
                String subKey = key.Substring(i + 1);
                AttributeCollectionBase ent = null;



                bS = superKey.IndexOf('[');
                bE = superKey.IndexOf(']');
                indx = String.Empty;
                if (bS > 0 && bE > 0)
                {
                    if (bE == superKey.Length - 1)
                    {

                        indx = superKey.Substring(bS + 1, (bE - bS) - 1);
                        superKey = superKey.Substring(0, bS);
                        //return GetIndexedValue(key, indx);
                        ent = GetIndexedValue(superKey, indx) as AttributeCollectionBase;
                        return ent[subKey];
                    }
                } else
                {
                    ent = _data[superKey].Value as AttributeCollectionBase;
                }




                if (_data.ContainsKey(superKey))
                    //if the entity is found in _data
                    ent = _data[superKey].Value as AttributeCollectionBase;
                if (ent == null)
                {
                    //check if the attribute is a property
                    PropertyDescriptor p = TypeDescriptor.GetProperties(this)[superKey];
                    if (p == null)
                        return null;
                    //Make sure it is an entity
                    ent = p.GetValue(this) as AttributeCollectionBase;
                    if (ent == null)
                        return null;
                }
                return ent[subKey];
            }
            set
            {
                /*Quicker Method*/
                //                if (!_data.ContainsKey(key)) _data.Add(key, new AttributeValue(key, value));
                //                else _data[key].Value = value;
                //
                //                OnPropertyChanged(key);
                //                return;

                /*EXTENDED METHOD*/
                int bS = 0;
                int bE = 0;
                String indx = String.Empty;
                int i = key.IndexOf(".");
                if (i < 0)
                {
                    bS = key.IndexOf('[');
                    bE = key.IndexOf(']');
                    indx = String.Empty;
                    if (bS > 0 && bE > 0)
                    {
                        if (bE == key.Length - 1)
                        {

                            indx = key.Substring(bS + 1, (bE - bS) - 1);
                            key = key.Substring(0, bS);
                            //return GetIndexedValue(key, indx);
                            SetIndexedValue(key, indx, value);
                            return;
                        }
                    }

                    //No "." Found, setting an attribute on this entity.
                    if (!_data.ContainsKey(key))
                    {
                        //Not found in _data, look for a property
                        PropertyDescriptor p = TypeDescriptor.GetProperties(this)[key];
                        if (p != null)
                        {
                            //property found. Set it.
                            p.SetValue(this, value);
                            OnPropertyChanged(key);
                            return;
                        }
                        //No property found, add to _data
                        _data.Add(key, new AttributeValue(key, value));
                    }
                    //found in _data, set value.
                    _data[key].Value = value;
                    OnPropertyChanged(key);
                    return;
                }

                String superKey = key.Substring(0, i);
                String subKey = key.Substring(i + 1);
                AttributeCollectionBase ent = null;



                bS = superKey.IndexOf('[');
                bE = superKey.IndexOf(']');
                indx = String.Empty;
                if (bS > 0 && bE > 0)
                {
                    if (bE == superKey.Length - 1)
                    {

                        indx = superKey.Substring(bS + 1, (bE - bS) - 1);
                        superKey = superKey.Substring(0, bS);
                        //return GetIndexedValue(key, indx);
                        ent = GetIndexedValue(superKey, indx) as AttributeCollectionBase;
                        ent[subKey] = value;
                        return;
                    }
                }

                PropertyDescriptor td = null;
                //"." found, looking for an attribute of related entitybase
                //First find the related entity.
                if (_data.ContainsKey(superKey))
                {
                    //it was in _data
                    ent = _data[superKey].Value as AttributeCollectionBase;
                }
                else
                {
                    //It was not in _data, look for a property
                    td = TypeDescriptor.GetProperties(this)[superKey];
                    if (td != null)
                    {
                        //We found a property by the correct name, Ensure that it is an EntityBase
                        ent = td.GetValue(this) as AttributeCollectionBase;
                        if (ent == null)
                            return;
                    }
                }
                if (ent == null)
                {
                    //No related entity was found. Create one and add it to _data.
                    //We know it won't collide with a property because we already threw an error if a property was found
                    //that couldn't be used, and ent wouldn't be null if a property was found that could be used.
                    ent = new AttributeCollection();
                    _data[superKey] = new AttributeValue(superKey, ent);
                }
                ent[subKey] = value;
                OnPropertyChanged(subKey);
            }
        }
        public void SetData(String key, object data)
        {
            if (_data.ContainsKey(key))
                _data[key].Value = data;
            else
                _data.Add(key, new AttributeValue(key, data));
        }
        public object PreviousValue(String key)
        {
            int i = key.IndexOf(".");
            if (i < 0)
            {
                //No "." found, so it is looking for an attribute of this entity
                if (!_data.ContainsKey(key))
                {
                    throw new Exception("Can not return previous value of a property.");
                }
                //Attribute found in _data return value
                return _data[key].PrevValue;
            }
            //Looking for an attribute of a related entity.
            //superKey is the name of the attribute of this entity that links to the sub entity
            //subkey is the name of the attribute on the linked entity being looked for
            String superKey = key.Substring(0, i);
            String subKey = key.Substring(i + 1);
            AttributeCollectionBase ent = null;

            if (_data.ContainsKey(superKey))
                //if the entity is found in _data
                ent = _data[superKey].Value as AttributeCollectionBase;
            if (ent == null)
            {
                //check if the attribute is a property
                PropertyDescriptor p = TypeDescriptor.GetProperties(this)[superKey];
                if (p == null)
                    return null;
                //Make sure it is an entity
                ent = p.GetValue(this) as AttributeCollectionBase;
                if (ent == null)
                    return null;
            }
            return ent[subKey];
        }

        public T DataAs<T>(string key)
        {
            int i = key.IndexOf(".");
            if (i < 0)
            {
                if (!_data.ContainsKey(key)) return default(T);
                return _data[key].ValueAs<T>();
            }
            String superKey = key.Substring(0, i);
            String subKey = key.Substring(i + 1);
            AttributeCollectionBase ent = null;
            if (_data.ContainsKey(superKey))
                ent = _data[superKey].Value as AttributeCollectionBase;
            if (ent == null)
                return default(T);
            return ent.DataAs<T>(subKey);

        }
        /*
        //public dynamic DataAs(string key, Type type)
        // {
        //    var v = _data[key];
        //    return v.ValueAs(type);
        //}

        public virtual bool Load(params EntityAttributeValue[] key) { return Load(key.ToList()); }
        public abstract bool Load(IEnumerable<EntityAttributeValue> keys);

        /// <summary>
        /// When using a filter, it is possible to join to other tables which means that
        /// additional information
        /// </summary>
        /// <param name="row"></param>
        /// <param name="persisted"></param>
        /// <param name="filterState"></param>
        /// <returns></returns>
        public virtual bool FilterLoad(DataRow row, Boolean persisted, Query.EntityQuery.FilterState filterState)
        {
            if (row == null) return false;
            String alias = filterState.Aliases[this.GetType()];
            foreach (EntityAttributeValue eav in _data.Values)
                if (eav._isPersisted && row.Table.Columns.Contains(String.Concat(alias, "_", eav.AttributeName)))
                    this[eav.AttributeName] = row[String.Concat(alias, "_", eav.AttributeName)];
            Dirty = false;
            _deleted = false;
            _persisted = persisted;
            return true;
        }
        public virtual bool FilterLink(Query.EntityQuery.FilterResults results)
        {
            return true;
        }
        public virtual bool Load(DataRow row)
        {
            if (row == null) return false;
            foreach (EntityAttributeValue eav in _data.Values)
                if (eav._isPersisted && row.Table.Columns.Contains(eav.AttributeName))
                    this[eav.AttributeName] = row[eav.AttributeName];
            Dirty = false;
            _deleted = false;
            return true;
        }
        public virtual bool Load(DataRow row, Boolean persisted)
        {
            bool ret = Load(row);
            if (ret)
                _persisted = persisted;
            return ret;
        }

        public virtual bool Commit()
        {
            return Commit(DataAccess.DBO);
        }
        public virtual bool Commit(IDataAccessObject DAO)
        {
            if (!_persisted)
                if (!_deleted)
                    return Create(DAO);
            if (!Dirty)
                if (!_deleted)
                    return true;
            if (_deleted)
                DoDelete(DAO);
            return Update(DAO);
        }
        public virtual bool Delete()
        {
            _deleted = true;
            return true;
        }
        protected abstract bool Create(IDataAccessObject dao);
        protected abstract bool Update(IDataAccessObject dao);
        protected abstract bool DoDelete(IDataAccessObject dao);

        public virtual bool BulkUpdate(EntityList<EntityBase> entities, List<DisplayAttribute> fields)
        {
            return BulkUpdate(DataAccess.DBO, entities, fields);
        }
        public abstract bool BulkUpdate(IDataAccessObject dao, EntityList<EntityBase> entities, List<DisplayAttribute> fields);
        */
        #region INotifyPropertyChange
        protected void OnPropertyChanged(String prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region GetAttributes
        public System.ComponentModel.AttributeCollection GetAttributes()
        {
            return new System.ComponentModel.AttributeCollection();
            //throw new NotImplementedException();
        }

        public string GetClassName()
        {
            return "CLass";
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            //throw new NotImplementedException();
            return new TypeConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
           // return new System.ComponentModel.Design.MultilineStringEditor();
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(new EventDescriptor[0]);
            //throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection ret;
            List<PropertyDescriptor> props = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(this.GetType()))
                props.Add(pd);

            foreach (AttributeValue eav in _data.Values)
            {
                IndexItemDescriptor iid = new IndexItemDescriptor(eav.AttributeName, eav.AttributeName, eav.Type, eav.IsReadOnly);
                //if (eav.HasPresentableAttribute)
                //{
                // Presentable p = new Presentable();
                // p.FriendlyName = eav.FriendlyName;
                // p.ReqPermission = eav.ReqPermission;

                // iid.AddAttribute(p);
                //}
                props.Add(iid);
            }


            ret = new PropertyDescriptorCollection(props.ToArray());
            return ret;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion


        #region Object Method overrides

        public override bool Equals(object obj)
        {
            AttributeCollectionBase o = obj as AttributeCollectionBase;
            if (o == null) return false;
            //return o.EntityID == EntityID;
            if (this.GetType() != obj.GetType()) return false;
            foreach (String keyName in KeyAttributes)
            {
                //                if (this[keyName] == null)
                //                    return false;
                if (this[keyName] == null)
                    return o[keyName] == null;
                if (!this[keyName].Equals(o[keyName]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 7;
            foreach (String keyName in KeyAttributes)
                hash = hash * 7 + (this[keyName] ?? "NULL").GetHashCode();
            return hash;
        }

        #endregion

        public void ResetValue(bool persistedOnly)
        {
            foreach (AttributeValue eav in _data.Values)
            {
                if (eav.isRelated)
                {
                    if (eav.Value != null)
                    {
                        (eav.Value as AttributeCollectionBase).ResetValue(persistedOnly);
                        eav.ResetValue();
                        OnPropertyChanged(eav.AttributeName);
                    }
                }
                else if (!persistedOnly || eav._isPersisted)
                {
                    eav.ResetValue();//this[eav.AttributeName] = eav._prevValue;// eav.ResetValue();
                    OnPropertyChanged(eav.AttributeName);
                }
            }
        }


        public bool Deserialize(String JsonString)
        {
            throw new NotImplementedException();
            /*
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();

            _data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, EntityAttributeValue>>(JsonString, settings);
            return false;
             */
        }

        public String Serialize() { return Serialize(true); }
        public String Serialize(bool persistedOnly)
        {
            throw new NotImplementedException();
            /*
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
            if (persistedOnly)
                settings.Converters.Add(new PersistedJsonConverter());
            return Newtonsoft.Json.JsonConvert.SerializeObject(_data, settings);
             */
        }


        public void CreateArray1D(String Name, int L1)
        {
            this[Name] = new object[L1];
        }
        public void CreateArray2D(String name, int L1, int L2)
        {
            this[name] = new object[L1, L2];
        }
        public void CreateArray3D(String name, int L1, int L2, int L3)
        {
            this[name] = new object[L1, L2,L3];
        }
        public void CreateArray4D(String name, int L1, int L2, int L3, int L4)
        {
            this[name] = new object[L1, L2,L3,L4];
        }
        public void CreateArray5D(String name, int L1,int L2,int L3,int L4,int L5)
        {
            this[name] = new object[L1,L2,L3,L4,L5];
        }

        public void CreateList(String name)
        {
            this[name] = new List<Object>();
        }

        public void Merge(AttributeCollection with)
        {
            foreach (var attr in with.Attributes)
            {
                if (!_data.ContainsKey(attr.AttributeName))
                {
                    this[attr.AttributeName] = attr.Value;
                    continue;
                }
                var cVal = this[attr.AttributeName];
                if (cVal == null)
                {
                    this[attr.AttributeName] = attr.Value;
                    continue;
                }
                if (cVal.GetType() == typeof(AttributeCollection))
                {
                    if (attr.Value.GetType() == typeof(AttributeCollection))
                    {
                        ((AttributeCollection)cVal).Merge((AttributeCollection)attr.Value);
                    }
                    else
                    {
                        this[attr.AttributeName] = attr.Value;
                    }
                }
                else
                {
                    this[attr.AttributeName] = attr.Value;
                }
            }
        }
    }

}
