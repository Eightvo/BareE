using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public class AttributeDisplay
    {
        /// <summary>
        /// A human readable title for the attribute
        /// </summary>
        public String Title { get; set; }
        /// <summary>
        /// The name of the entity attribute value that contains the DATA value
        /// </summary>
        public String Attribute { get; set; }
        /// <summary>
        /// The name of the entity attribute value that contains the DISPLAY value
        /// </summary>
        public String Display { get; set; }
        public Type Type { get; set; }
        public AttributeDisplay(String attribute, Type type) : this(attribute, attribute, type) { }
        public AttributeDisplay(String attribute) : this(attribute, attribute, typeof(String)) { }
        public AttributeDisplay(String attribute, String title) : this(attribute, title, typeof(String)) { }
        public AttributeDisplay(String attribute, String title, Type type) : this(attribute, title, attribute, type) { }
        public AttributeDisplay(String attribute, String title, String display, Type type)
        {
            Title = title;
            Attribute = attribute;
            Display = display;
            Type = type;
        }
    }

}
