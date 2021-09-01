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
                             };

            _functions = new Dictionary<string, CalculatorHelperFunctionInfo>(StringComparer.CurrentCultureIgnoreCase)
                             {
                                 {"now", new CalculatorHelperFunctionInfo(0, Now)},
                                 {"today", new CalculatorHelperFunctionInfo(0, Today)},
                                 {"tommorrow", new CalculatorHelperFunctionInfo(0, Tommorrow)},
                                 {"yesterday", new CalculatorHelperFunctionInfo(0, Yesterday)},
                                 {"thismonth", new CalculatorHelperFunctionInfo(0, StartOfMonth)},
                                 {"nextmonth", new CalculatorHelperFunctionInfo(0, StartOfNextMonth)},
                                 {"thisweek", new CalculatorHelperFunctionInfo(0, StartOfWeek)},
                                 {"nextweek", new CalculatorHelperFunctionInfo(0, StartOfNextWeek)},
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
        public object Today(params object[] parameters)
        {
            return DateTime.Today;
        }
        public object Tommorrow(params object[] parameters)
        {
            return DateTime.Today.AddDays(1);
        }
        public object Yesterday(params object[] parameters)
        {
            return DateTime.Today.AddDays(-1);
        }
        public object StartOfMonth(params object[] parameters)
        {
            var t = DateTime.Today;
            return new DateTime(t.Year, t.Month, 1);
        }
        public object StartOfNextMonth(params object[] parameters)
        {
            DateTime t = (DateTime)StartOfMonth();
            t.AddMonths(1);
            return t;
        }
        public object StartOfWeek(params object[] parameters)
        {
            return DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        }
        public object StartOfNextWeek(params object[] parameters)
        {
            return ((DateTime)StartOfWeek()).AddDays(7);
        }

        public object DateAddDays(params object[] parameters)
        {
            var s = (DateTime)(parameters[0]);
            var d = (Decimal)(parameters[1]);
            return s.AddDays((double)d);
        }
    }
}
