using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Calculator
{
    /// <summary>
    /// Calculator Helpers are designed for 2 purposes
    /// 1) Define Global calculator variables
    /// 2) Provide additional mathmatical functions.
    /// </summary>
    public interface ICalculatorHelper
    {
        /// <summary>
        /// Map/Define Variables.
        /// </summary>
        /// <param name="term">Variable name</param>
        /// <returns>Variable Value</returns>
        object Map(String term);

        /// <summary>
        /// Map/Define functions
        /// </summary>
        /// <param name="Name">Name of function</param>
        /// <returns>Function Definition. Including Number of parameters expected and the delegate to run as function</returns>
        CalculatorHelperFunctionInfo GetFunction(String Name);

        String Name { get; }
        List<String> FunctionList { get; }
        List<KeyValuePair<String, Object>> VariableList { get; set; }

    }
}
