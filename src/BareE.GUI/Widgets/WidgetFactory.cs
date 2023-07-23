using BareE.DataStructures;

using SixLabors.ImageSharp.Formats;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI.Widgets
{
    public static class WidgetFactory
    {
        private static List<Type> _widgetTypes=new List<Type>();
        private static Object[] _NoArgs_=new object[0];
        static WidgetFactory()
        {
            _widgetTypes = Assembly.GetExecutingAssembly().GetTypes().Where( x => typeof(WidgetBase).IsAssignableFrom(x)).ToList();
        }
        private static Type ResolveWidgetType(String typeName)
        {
            var tempLst = _widgetTypes.Where(x => String.Compare(x.FullName, typeName, true) == 0).ToList();
            if (typeName.IndexOf(".") == -1)
                tempLst = _widgetTypes.Where(x => String.Compare(x.Name, typeName, true) == 0).ToList();

            if (tempLst.Count == 1)
                return tempLst[0];
            if (tempLst.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Ambigious typename.");
                foreach (var v in tempLst)
                    sb.AppendLine(v.FullName);

                throw new Exception(sb.ToString());
            }
            throw new Exception($"Type {typeName} not found");
        }
        public static WidgetBase CreateWidget(AttributeCollection def)
        {
            var typeName = (String)def["Type"];
            Type widgetType;
            widgetType= ResolveWidgetType(typeName);
            WidgetBase wBase = (WidgetBase)Activator.CreateInstance(widgetType,_NoArgs_);
            
            wBase.ReadAttributes(def);
            var children = (Object[])def["Children"];
            //wBase.Children = new List<WidgetBase>();
            if (children != null)
            {
                foreach (var child in children)
                {
                    wBase.AddChild(CreateWidget((AttributeCollection)child));
                }
            }
            return wBase;
        }
    }
}
