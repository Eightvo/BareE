using System;

namespace BareE.DataStructures
{
    [Flags]
    public enum ComponentFlags
    {
        None = 0,
    }

    public class ComponentAttribute : Attribute
    {
        private static int _componentTypeID;
        public int CTypeID;
        public ComponentFlags Flags;

        public String Name;
        public Type OriginatingType;

        public ComponentAttribute(String name) : this(name, ComponentFlags.None)
        {
        }

        public ComponentAttribute(String name, ComponentFlags componentFlags)
        {
            Flags = componentFlags;
            Name = name;
            CTypeID = ++_componentTypeID;
        }

        public override string ToString()
        {
            return $"[{CTypeID}] {Name}";
        }
    }
}