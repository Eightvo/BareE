using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BareE.DataAcess
{
    [DebuggerDisplay("{Schema}.{PackageName}.{Name}")]
    public class SPInformation
    {
        public String Name { get; set; }
        public String Schema { get; set; }
        public String PackageName { get; set; }
        public List<SPParameterDefinition> Parameters { get; set; }
        protected int _inputCount=-1;
        public int InputCount
        {
            get
            {
                if (_inputCount >= 0) return _inputCount;
                _inputCount = 0;
                foreach (SPParameterDefinition def in Parameters)
                    if (def.Direction == System.Data.ParameterDirection.Input || def.Direction == System.Data.ParameterDirection.InputOutput)
                        _inputCount++;
                return _inputCount;
            }
        }

        public SPInformation(String schema, String packageName, String name)
        {
            Name = name;
            Schema = schema;
            PackageName = packageName;
            Parameters = new List<SPParameterDefinition>();
        }

    }
}
