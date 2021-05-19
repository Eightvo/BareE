using System;

namespace BareE.Messages
{
    public class MessageAttribute : Attribute
    {
        static int _messageTypeID;
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
