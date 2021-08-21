using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    [JsonObject]
    public class AttributeValue
    {

        public override string ToString()
        {
            return String.Format("[{2}]{0}={1}", _attributeName, _value, Type);
        }

        #region Factory Metods
        public static AttributeValue Define(String name) { return Define(name, false, false, typeof(Object)); }
        public static AttributeValue Define(String name, Type type) { return Define(name, false, false, type); }

        public static AttributeValue Define(String name, bool isKey) { return Define(name, isKey, isKey, typeof(Object)); }
        public static AttributeValue Define(String name, bool isKey, Type type) { return Define(name, isKey, isKey, type); }

        public static AttributeValue Define(String name, bool isKey, bool isIdentity) { return Define(name, isKey, isIdentity, typeof(Object)); }
        public static AttributeValue Define(String name, bool isKey, bool isIdentity, Type type)
        {
            AttributeValue eav = new AttributeValue(name, null);
            eav._isKey = isKey;
            eav._isIdentity = isIdentity;
            eav._isPersisted = true;
            eav._type = type;
            return eav;
        }



        public static AttributeValue Relate(String name)
        {
            AttributeValue eav = new AttributeValue(name, null);
            eav.isRelated = true;
            eav._type = typeof(AttributeCollectionBase);
            return eav;
        }

        /// <summary>
        /// ....
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AttributeValue Literal(String name, String value)
        {
            AttributeValue eav = new AttributeValue(name, value);
            eav.IsLiteral = true;
            if (value != null)
                eav._type = value.GetType();
            return eav;
        }

        public static AttributeValue Literal(String name, Object value)
        {
            AttributeValue eav = new AttributeValue(name, value);
            eav.IsLiteral = true;
            if (value != null)
                eav._type = value.GetType();
            return eav;
        }
        /// <summary>
        /// Field is read from table, and inserted on creation.
        /// Updates will not modify this field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AttributeValue ReadOnly(String name) { return ReadOnly(name, null); }
        public static AttributeValue ReadOnly(String name, Type type)
        {
            AttributeValue eav = new AttributeValue(name, null);
            eav.IsReadOnly = true;
            eav._isPersisted = true;
            eav._type = type;
            return eav;
        }
        #endregion

        /// <summary>
        /// Used in joins to allow the value to be used as a column name rather than
        /// the value of the entity by that name.
        /// </summary>
        [JsonIgnore]
        public bool IsLiteral = false;
        /// <summary>
        /// This is a remenant of an attempt to standardize relationships between
        /// entities... when stadarized relationships are complete this should be
        /// refactored out.
        /// </summary>
        [JsonIgnore]
        public bool isRelated = false;
        /// <summary>
        /// Will not use this field in update operations, but will be used
        /// in Insert/Select statments.
        /// </summary>
        [JsonIgnore]
        public bool IsReadOnly = false;

        internal bool _dirty;
        /// <summary>
        /// Has the value changed since it was last in a 'clean' state
        /// values are clean when they are read from the database
        /// </summary>
        [JsonIgnore]
        public bool Dirty
        {
            get
            {
                if (_value != null && _prevValue != null && _value.Equals(_prevValue)) return false;
                if (_value != null && _value.GetType() == typeof(String))
                    if (String.IsNullOrEmpty(_value.ToString()) && String.IsNullOrEmpty((_prevValue ?? "").ToString())) return false;
                return _dirty;
            }
            set
            {
                if (value == false) _prevValue = Value;
                _dirty = value;
            }
        }
        protected String _attributeName;
        /// <summary>
        /// The dictionary Key for this attribute. Should not share the name of a property.
        /// </summary>
        //[JsonPropertyAttribute]
        public String AttributeName { get { return _attributeName; } }
        protected Object _value;

        /// <summary>
        /// Glyph of the attribute stored as an object. 
        /// </summary>
        //[JsonPropertyAttribute]
        public virtual Object Value
        {
            get { return _value; }
            set
            {
                if (_type == null)
                    if (value != null) _type = value.GetType();
                    else _type = typeof(Object);
                if (_value == value) return;
                //if (Dirty == false)
                _prevValue = _value;
                _value = value;
                Dirty = true;
            }
        }

        [JsonIgnore]
        public virtual Object PrevValue
        {
            get { return _prevValue; }
        }

        [JsonIgnore]
        public Boolean HasPresentableAttribute { get; set; }
        String _friendlyName;
        [JsonIgnore]
        public String FriendlyName
        {
            get { if (String.IsNullOrEmpty(_friendlyName)) return _attributeName; return _friendlyName; }
            set { _friendlyName = value; }
        }

        /// <summary>
        /// Place holder for attribute specific security.
        /// </summary>
        [JsonIgnore]
        public String ReqPermission { get; set; }

        /// <summary>
        /// EAV Equivalent to Presentable property attribute.
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="reqPermission"></param>
        /// <returns></returns>
        public AttributeValue SetPresentableAttribute(String friendlyName, String reqPermission)
        {
            FriendlyName = friendlyName;
            ReqPermission = reqPermission;
            HasPresentableAttribute = true;
            return this;
        }

        public AttributeValue SetPresentableAttribute(String friendlyName)
        {
            return SetPresentableAttribute(friendlyName, String.Empty);
        }

        public AttributeValue SetPresentableAttribute()
        {
            return SetPresentableAttribute(_attributeName.Replace("_", " "), String.Empty);
        }
        Type _type;
        /// <summary>
        /// Expected type of value of attribute. If not defined, determines type
        /// from object stored. Explicity setting type helps determine type when value
        /// is null.
        /// </summary>
        [JsonIgnore]
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    if (_value == null)
                        return typeof(Object);
                    else return _value.GetType();
                }
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Force a value and condsider it clean.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value)
        {
            _value = value;
            _prevValue = value;
            Dirty = false;
        }

        internal Object _prevValue;

        internal bool _isKey;
        internal bool _isIdentity;
        internal bool _isPersisted;
        /// <summary>
        /// Used in where clauses, also used to compare entities. 
        /// Two entites are considered equal if all the values for the key
        /// attributes are equal.
        /// </summary>
        [JsonIgnore]
        public bool IsKey { get { return _isKey; } }

        public AttributeValue(String attributeName, object value)
        {
            _attributeName = attributeName;
            _prevValue = value;
            Value = value;
        }
        public T ValueAs<T>()
        {
            if (Value == null || Value == DBNull.Value) return default(T);
            if (typeof(T).IsEnum)
            {
                object o;
                if (Enum.TryParse(typeof(T), Value.ToString(),true, out o))
                    return ((T)o);
            }
            return (T)Convert.ChangeType(Value, typeof(T));
        }

        public void ResetValue()
        {
            Value = _prevValue;
            _dirty = false;
        }

        /// <summary>
        /// This column is stored in the database.
        /// </summary>
        [JsonIgnore]
        public bool Persisted
        {
            get
            {
                return _isPersisted;
            }
        }
    }

}
