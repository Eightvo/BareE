using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Calculator
{
    /// <summary>
    /// Implement this delegate to create new functions available to expressions.
    /// </summary>
    /// <param name="parameters">Decimal input values to function.</param>
    /// <returns>Decimal result of function.</returns>
    public delegate object CalculatorHelperFunction(params object[] parameters);

    /// <summary>
    /// Defines a Function to be available to the calculator.
    /// </summary>
    public class CalculatorHelperFunctionInfo
    {
        /// <summary>
        /// Number of Variables the function expects.
        /// Null Indicates that an unknown number of variables are expected
        /// </summary>
        public int? ParameterCount { get; set; }
        /// <summary>
        /// The Code implementation of the function that process the arguments and returns a value.
        /// </summary>
        public CalculatorHelperFunction Function { get; set; }

        /// <summary>
        /// Creates a new Calculator Function Defintion
        /// </summary>
        /// <param name="parmCount">Number of Values expected as input</param>
        /// <param name="func">The Code Implementation that process the arguments and returns a value</param>
        public CalculatorHelperFunctionInfo(int? parmCount, CalculatorHelperFunction func)
        {
            ParameterCount = parmCount;
            Function = func;
        }
    }
}
