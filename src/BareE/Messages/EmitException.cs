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
        public EmitException(Exception e):this(e,null)
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
}
