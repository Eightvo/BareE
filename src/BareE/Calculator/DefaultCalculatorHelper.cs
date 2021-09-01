using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Calculator
{

    /// <summary>
    /// This is a set of variables and Functions always available to all calculators.
    /// Variables: pi
    ///            e
    /// Functions: abs - Absolute value
    ///            ceiling - The Least Integer grater then the value.
    ///            floor - The Greatest Integer less then the value.
    ///            sqrt - The square root of the value.
    ///            min - The minimum value in a list of values.
    ///            max - The maximium value in a list of values.
    ///            sum - The Sum of a list of values.
    /// </summary>
    public class DefaultCalculatorHelper : ICalculatorHelper
    {
        public String Name { get { return "Default"; } }
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

        public DefaultCalculatorHelper()
        {
            _variables = new Dictionary<String, Object>(StringComparer.CurrentCultureIgnoreCase)
                             {
                                {"pi", (decimal) Math.PI},
                                {"e", (decimal) Math.E}
                             };

            _functions = new Dictionary<string, CalculatorHelperFunctionInfo>(StringComparer.CurrentCultureIgnoreCase)
                             {
                                 {"abs", new CalculatorHelperFunctionInfo(1, Abs)},
                                 {"ceiling", new CalculatorHelperFunctionInfo(1, Ceiling)},
                                 {"floor", new CalculatorHelperFunctionInfo(1, Floor)},
                                 {"sqrt", new CalculatorHelperFunctionInfo(1, Sqrt)},
                                 {"min", new CalculatorHelperFunctionInfo(2, Min)},
                                 {"max", new CalculatorHelperFunctionInfo(2, Max)},
                                 {"sum", new CalculatorHelperFunctionInfo(null, Sum)},
                                 {"sin", new CalculatorHelperFunctionInfo(1,Sine)},
                                 {"asin", new CalculatorHelperFunctionInfo(1,ASine)},
                                 {"cos", new CalculatorHelperFunctionInfo(1,Cos)},
                                 {"acos", new CalculatorHelperFunctionInfo(1,ACos)},
                                 {"tan", new CalculatorHelperFunctionInfo(1,Tan)},
                                 {"atan", new CalculatorHelperFunctionInfo(1,Atan)}
                             };
        }
        public CalculatorHelperFunctionInfo GetFunction(string Name)
        {
            if (!_functions.ContainsKey(Name)) return null;
            return _functions[Name];
        }

        public object? Map(string term)
        {
            if (!_variables.ContainsKey(term))
                return null;
            return _variables[term];
        }

        public object Abs(params object[] parameters)
        {
            return Math.Abs((double)parameters[0]);
        }
        public object Ceiling(params object[] parameters)
        {
            return Math.Ceiling((double)parameters[0]);
        }
        public object Floor(params object[] parameters)
        {
            return Math.Floor((double)parameters[0]);
        }
        public object Sqrt(params object[] parameters)
        {
            return (decimal)Math.Sqrt((double)parameters[0]);
        }
        public object Min(params object[] parameters)
        {
            return Math.Min((double)parameters[0], (double)parameters[1]);
        }
        public object Max(params object[] parameters)
        {
            return Math.Max((double)parameters[0], (double)parameters[1]);
        }

        public object Sum(params object[] parameters)
        {
            var tot = 0m;
            foreach (var d in parameters)
                tot += (Decimal)d;
            return tot;
        }
        public object Sine(params object[] parameters) { return (decimal)Math.Sin((double)parameters[0]); }
        public object ASine(params object[] parameters) { return (decimal)Math.Asin((double)parameters[0]); }
        public object Cos(params object[] parameters) { return (decimal)Math.Cos((double)parameters[0]); }
        public object ACos(params object[] parameters) { return (decimal)Math.Acos((double)parameters[0]); }
        public object Tan(params object[] parameters) { return (decimal)Math.Tan((double)parameters[0]); }
        public object Atan(params object[] parameters) { return (decimal)Math.Atan((double)parameters[0]); }
    }
}
