using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Messages
{

    [MessageAttribute("EmitException")]
    public struct EmitException
    {
        public EmitException(Exception e) : this(e, null)
        {

        }
        public EmitException(Exception e, Object context)
        {
            Exception = e;
            Context = context;
        }
        public object Context;
        public Exception Exception;
    }

    public enum EmittedTextType
    {
        Normal=0,
        Warning=1,
        
    }
    [MessageAttribute("EmitText")]
    public struct EmitText
    {
        public EmittedTextType Type;
        public string Text;
        public EmitText(String txt) { Text = txt;  Type = EmittedTextType.Normal; }
        public static EmitText Normal(String txt) { return new EmitText(txt); } 
        public static EmitText Warning(String txt) { return new EmitText() { Type= EmittedTextType.Warning, Text=txt }; }
    }
}
