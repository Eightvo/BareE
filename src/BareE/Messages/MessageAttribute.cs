using System;

namespace BareE.Messages
{
    /// <summary>
    /// An attribute used to mark a class as a message type such that it can be used by the messaging system.
    /// </summary>
    public class MessageAttribute : Attribute
    {
        private static int _messageTypeID;
        public int MTypeID = ++_messageTypeID;
        public String Name;
        public Type OriginatingType;

        public MessageAttribute(String name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"[{MTypeID}] {Name}";
        }
    }
}