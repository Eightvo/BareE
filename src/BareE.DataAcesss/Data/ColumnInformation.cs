using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("{Parent.Name}.{ColumnName}")]
    public class ColumnInformation
    {
        public TableInformation Parent { get; set; }
        public String ColumnName { get; set; }
        public bool isNullable { get; set; }
        public bool isQuoted { get; set; }
        public String DataType { get; set; }

        public ColumnInformation() : this(null, "", false, false) { }
        public ColumnInformation(String columnName, bool nullable, bool quoted):this(null,columnName,nullable,quoted){}
        public ColumnInformation(TableInformation parent, String columnName, bool nullable, bool quoted)
        {
            Parent = parent;
            ColumnName = columnName;
            isNullable = nullable;
            isQuoted = quoted;
        }
    }
}
