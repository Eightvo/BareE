using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public class AttributeCollectionList<T> : BindingList<T>, ITypedList
         where T : AttributeCollectionBase
    {
        public List<AttributeDisplay> DisplayMembers { get; set; }
        public AttributeCollectionList() { }
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            List<PropertyDescriptor> props = new List<PropertyDescriptor>();

            foreach (AttributeDisplay prop in DisplayMembers)
            {
                IndexItemDescriptor iid = new IndexItemDescriptor(prop.Title, prop.Attribute, prop.Type);
                props.Add(iid);
            }
            return new PropertyDescriptorCollection(props.ToArray());
        }

        public void RemoveEntity(AttributeCollection ent)
        {
            bool found = false;
            int i = 0;
            for (i = 0; i < Count; i++)
            {
                if (this[i].EntityID == ent.EntityID)
                {
                    found = true;
                    break;
                }
            }
            if (found)
                this.RemoveAt(i);
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "";
        }

        public AttributeCollectionList<AttributeCollectionBase> ToEntityBaseEntityList()
        {
            AttributeCollectionList<AttributeCollectionBase> ebl = new AttributeCollectionList<AttributeCollectionBase>();
            ebl.DisplayMembers = this.DisplayMembers;
            foreach (AttributeCollectionBase b in this)
                ebl.Add(b);
            return ebl;
        }

        public static implicit operator AttributeCollectionList<AttributeCollectionBase>(AttributeCollectionList<T> entList)
        {
            AttributeCollectionList<AttributeCollectionBase> ebl = new AttributeCollectionList<AttributeCollectionBase>();
            foreach (AttributeCollectionBase b in entList)
                ebl.Add(b);
            return ebl;
        }

    }

}
