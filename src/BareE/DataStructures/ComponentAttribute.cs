using System;

namespace BareE.DataStructures
{
    /// <summary>
    /// Currently all components are treated Identically. 
    /// This enumeration exists to facilitate Component Handling extensibility
    /// </summary>
    [Flags]
    public enum ComponentFlags
    {
        None = 0,
    }

    /// <summary>
    /// Marks a structure as a component type to allow the component system to be utilized for this structure.
    /// </summary>
    public class ComponentAttribute : Attribute
    {
        private static int _componentTypeID;
        /// <summary>
        /// This value is Dynamic. A Component Type ID is only guarenteed to remain static for the lifetime of the application.
        /// </summary>
        public int CTypeID;

        /// <summary>
        /// Reserved for extensibility.
        /// </summary>
        public ComponentFlags Flags;

        /// <summary>
        /// Reservered for scripting extensibility.
        /// </summary>
        public String Name;

        /// <summary>
        /// They Typedata for the type this attribute is attached to.
        /// </summary>
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