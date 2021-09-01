using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BareE.DataAcess.Exceptions
{
    public class TransactionException : Exception
    {
        public TransactionException() : base() { }
        public TransactionException(String msg) : base(msg) { }
        public TransactionException(String msg, Exception innerException) : base(msg, innerException) { }
    }

    public class CommandException : Exception
    {
        public CommandException() : base() { }
        public CommandException(String msg) : base(msg) { }
        public CommandException(String msg, Exception innerException) : base(msg, innerException) { }
    }
}
