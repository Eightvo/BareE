using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("{Name}")]
    public class TableInformation
    {
        public String Qualifier;
        public String Name;
        Dictionary<String, ColumnInformation> _columns = new Dictionary<string, ColumnInformation>(StringComparer.InvariantCultureIgnoreCase);

        public List<ColumnInformation> Columns {get{return _columns.Values.ToList();}}

        
        public TableInformation(String name) : this("", name) { }
        public TableInformation(String qualifier, String name)
        {
            Qualifier = qualifier;
            Name = name;
        }

        public ColumnInformation this[string column]
        {
            get
            {
                if (_columns == null) return null;
                if (_columns.ContainsKey(column))
                    return _columns[column];
                return null;
            }
            set
            {
                if (!_columns.ContainsKey(column)) _columns.Add(column, null);

                if (value != null)
                    value.Parent = this;

                _columns[column] = value;
            }
        }

    }
}
