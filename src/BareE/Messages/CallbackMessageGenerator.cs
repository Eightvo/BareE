using System;

namespace BareE.Messages
{
    public class CallbackMessageGenerator : IMessageGenerator
    {
        public CallbackMessageGenerator(Func<object, object> callback)
        {
            Callback = callback;
        }
        Func<object, object> Callback;
        public object GenerateMessage(object o)
        {
            if (Callback != null)
                return Callback(o);
            return null;
        }
    }
}
