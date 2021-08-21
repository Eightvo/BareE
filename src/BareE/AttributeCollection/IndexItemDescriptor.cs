using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    class IndexItemDescriptor : PropertyDescriptor
    {
        private readonly String _key;
        private readonly Type _type;
        private readonly bool _readOnly = false;
        private List<Attribute> _attColl = new List<Attribute>();
        public IndexItemDescriptor(String header, String key, Type type, bool readOnly)
            : this(header, key, type)
        {
            _readOnly = readOnly;
        }
        public IndexItemDescriptor(String header, String key, Type type)
            : base(header, new Attribute[0])
        {
            _key = key;
            _type = type;
        }
        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(AttributeCollectionBase); }
        }

        public override object GetValue(object component)
        {
            return ((AttributeCollectionBase)component)[_key];
        }

        public override bool IsReadOnly
        {
            get { return _readOnly; }
        }

        public override Type PropertyType
        {
            get { return _type; }
        }

        public override void ResetValue(object component)
        {
            ((AttributeCollectionBase)component)._data[_key].ResetValue();
        }

        public override void SetValue(object component, object value)
        {
            ((AttributeCollectionBase)component)[_key] = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;// throw new NotImplementedException();
        }

        public void AddAttribute(Attribute att)
        {
            _attColl.Add(att);
        }
        public override System.ComponentModel.AttributeCollection Attributes
        {
            get
            {
                return new System.ComponentModel.AttributeCollection(_attColl.ToArray());
            }
        }
    }
}
