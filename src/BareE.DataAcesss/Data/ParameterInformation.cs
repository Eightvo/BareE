using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("{Name}={Value}}")]
    public class ParameterInformation
    {
        public String Name;
        public Object Value { get; set; }
        public String Type { get; set; }
        public bool IsNull { get { return Value == null || Value.GetType()==typeof(DBNull); } }
        public T ValueAs<T>()
        {
            T ret = default(T);
            if (IsNull) return ret;
            ret = (T)Convert.ChangeType(Value, typeof(T));
            return ret;
        }
        public ColumnInformation Column;

        bool _isQuoted = false;
        public bool IsQuoted { get { if (Column == null) return _isQuoted; return Column.isQuoted; } }

        public ParameterInformation(ColumnInformation column, String name, object value)
        {
            Column = column;
            Name = name;
            Value = value;
            //Type = column.DataType;
        }

        public ParameterInformation(String name, object value)
        {
            Column = null;
            Name = name;
            Value = value;
        }
        public ParameterInformation(String name, object value, String dataType)
        {
            Column = null;
            Name = name;
            Value = value;
            Type = dataType;
        }
        public ParameterInformation(String name, object value, bool isQuoted)
        {
            Column = null;
            Name = name;
            Value = value;
            _isQuoted = isQuoted;
        }

        //public ParameterInformation(String name, object value, String type)
       // {
        //    Column = null;
        //    Name = name;
        //    Value = value;
        //    Type = type;
       // }


    }

    [DebuggerDisplay("{Direction} {Type} {Name}")]
    public class SPParameterDefinition
    {
        public String Name { get; set; }
        public String Type { get; set; }
        public ParameterDirection Direction { get; set; }
        public int Size { get; set; }
        public bool IsNullable { get { return true; } }
        public SPParameterDefinition(String name, String type, ParameterDirection direction, int size)
        {
            Name = name;
            Type = type;
            Direction = direction;
            Size = size;
        }
    }
}

