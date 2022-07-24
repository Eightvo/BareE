using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{

    public class AttributeCollection : AttributeCollectionBase
    {
        public static AttributeCollection EmptyEntity = new AttributeCollection();

        [JsonIgnore]
        public override DataAccessContainer DataAccess
        {
            get { return null; }
            set { }
        }

        protected override void InitializeData()
        {
            base.InitializeData();
        }

        
        public AttributeCollection() : base()
        {
            DataAccess = null;
            InitializeData();
        }

        public AttributeCollection(AttributeCollectionBase from)
            : base()
        {
            InitializeData();
            UseData(from);
            //foreach (AttributeValue eav in from.Attributes)
            //    this[eav.AttributeName] = eav.Value;
        }

        public AttributeCollection(AttributeCollectionBase from, List<AttributeDisplay> map)
            : base()
        {
            InitializeData();
            foreach (AttributeDisplay attr in map)
                this[attr.Title] = from[attr.Attribute];
        }

        public bool HasAttribute(string v)
        {
            if (v.Contains("."))
            {
                var dIndx = v.IndexOf(".");
                var key = v.Substring(0, dIndx);
                var subKey = v.Substring(dIndx + 1);
                if (_data.ContainsKey(key))
                    return false;
                var subEnd = DataAs<AttributeCollection>(key);
                if (subEnd == null)
                    return false;
                return subEnd.HasAttribute(subKey);
            }
            return _data.ContainsKey(v);
        }
    }
}
