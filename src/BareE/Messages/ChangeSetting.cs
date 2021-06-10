using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Messages
{

    [MessageAttribute("ChangeSetting")]
    public struct ChangeSetting
    {
        public String Setting { get; set; }
        public String Value { get; set; }
    }
}
