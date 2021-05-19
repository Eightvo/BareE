using System;

namespace BareE.Messages
{
    [MessageAttribute("ConsoleInput")]
    public struct ConsoleInput
    {
        public ConsoleInput(String txt) { System = false;Text = txt; }
        public bool System;
        public String Text;
    }
}
