using BareE.Calculator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Harness
{
    /// <summary>
    /// </summary>
    public class CalendarCalculatorHelper : ICalculatorHelper
    {
        public String Name { get { return "Calendar"; } }
        public List<String> FunctionList { get { return _functions.Keys.ToList<String>(); } }
        public List<KeyValuePair<String, object>> VariableList
        {
            get
            {
                List<KeyValuePair<String, object>> ret = new List<KeyValuePair<string, object>>();
                foreach (KeyValuePair<String, object> okvp in _variables.ToList())
                    ret.Add(new KeyValuePair<string, object>(okvp.Key, okvp.Value));
                return ret;
            }
            set { }
        }

        private readonly Dictionary<String, object> _variables;
        private readonly Dictionary<string, CalculatorHelperFunctionInfo> _functions;

        public CalendarCalculatorHelper()
        {
            _variables = new Dictionary<String, Object>(StringComparer.CurrentCultureIgnoreCase)
                             {
                                {"y2k", new DateTime(2000,1,1)},
                             };

            _functions = new Dictionary<string, CalculatorHelperFunctionInfo>(StringComparer.CurrentCultureIgnoreCase)
                             {
                                 {"now", new CalculatorHelperFunctionInfo(0, Now)},
                                 {"AddDays", new CalculatorHelperFunctionInfo(2, DateAddDays)},
                             };
        }
        public CalculatorHelperFunctionInfo GetFunction(string Name)
        {
            if (!_functions.ContainsKey(Name)) return null;
            return _functions[Name];
        }

        public object Map(string term)
        {
            if (!_variables.ContainsKey(term))
                return null;
            return _variables[term];
        }

        public object Now(params object[] parameters)
        {
            return DateTime.Now;
        }
        public object DateAddDays(params object[] parameters)
        {
            var s = (DateTime)(parameters[0]);
            var d = (Decimal)(parameters[1]);
            return s.AddDays((double)d);
        }
    }
}
