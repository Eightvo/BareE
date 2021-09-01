using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Calculator
{
    public class Calculator
    {
        readonly Dictionary<String, object> _localVariables = new Dictionary<string, object>();
        readonly List<ICalculatorHelper> _helpers;

        /// <summary>
        /// Generates a new instance of the calculator with only the default calculator helper
        /// </summary>
        public Calculator() : this(new List<ICalculatorHelper>()) { }

        /// <summary>
        /// Generate a new instance of the calculator with a set of calculator helpers.
        /// </summary>
        /// <param name="helpers">A set of calculator helpers</param>
        public Calculator(List<ICalculatorHelper> helpers)
        {
            _helpers = helpers ?? new List<ICalculatorHelper>();
            _helpers.Add(new DefaultCalculatorHelper());//.Union(new List<ICalculatorHelper> {new DefaultCalculatorHelper()});
        }

        /// <summary>
        /// Generate a new instance of the calculator with a set of calculator helpers.
        /// </summary>
        /// <param name="parms"></param>
        public Calculator(params ICalculatorHelper[] parms) : this(parms.ToList())
        {

        }

        /// <summary>
        /// 
        /// List of all functions known by this calculator.
        /// </summary>
        public List<String> functionList
        {
            get
            {
                List<String> _cmds = new List<string>();
                foreach (ICalculatorHelper helper in _helpers)
                {
                    _cmds.AddRange(helper.FunctionList);
                }
                return _cmds;
            }
        }
        public CalculatorHelperFunctionInfo GetFunctionInfo(String functionName)
        {
            foreach (var helper in _helpers)
            {
                var dat= helper.GetFunction(functionName);
                if (dat != null)
                    return dat;
            }
            return null;
        }
        /// <summary>
        /// Key a list of all known variables and their values
        /// </summary>
        public List<KeyValuePair<String, object>> variableList
        {
            get
            {
                List<KeyValuePair<String, object>> ret = new List<KeyValuePair<string, object>>();
                ret = _localVariables.ToList<KeyValuePair<String, object>>();
                foreach (ICalculatorHelper helper in _helpers)
                {
                    ret.AddRange(helper.VariableList);
                }
                return ret;
            }
        }

        public void AddHelper(ICalculatorHelper helper)
        {
            if (_helpers.FirstOrDefault(f => f.Name == helper.Name) == null)
                _helpers.Add(helper);
        }

        public IEnumerable<LexerToken> ConvertInfixToRPN(IEnumerable<LexerToken> infix)
        {
            Queue<LexerToken> outputQ = new Queue<LexerToken>();
            Stack<LexerToken> opStack = new Stack<LexerToken>();
            var fList = functionList;
            foreach (var token in infix)
            {
                switch (token.Type)
                {
                    case LexerToken.LexerTokenType.Comment:
                    case LexerToken.LexerTokenType.Whitespace:
                        break;
                    case LexerToken.LexerTokenType.Identifier:
                        if (fList.Contains(token.Text))
                            opStack.Push(token);
                        else
                            outputQ.Enqueue(token);
                        break;
                    case LexerToken.LexerTokenType.Character_Literal:
                    case LexerToken.LexerTokenType.Integer_Literal:
                    case LexerToken.LexerTokenType.Real_Literal:
                    case LexerToken.LexerTokenType.String_Literal:
                        outputQ.Enqueue(token);
                        break;
                    default:
                        switch (token.Text)
                        {
                            case "(":
                                opStack.Push(token);
                                break;
                            case ")":
                                while (opStack.Peek().Text != "(")
                                {
                                    outputQ.Enqueue(opStack.Pop());
                                }
                                opStack.Pop();
                                if (opStack.Count > 0)
                                    if (fList.Contains(opStack.Peek().Text))
                                    {
                                        //var funcDat = Func
                                        outputQ.Enqueue(opStack.Pop());

                                    }
                                break;
                            case ",":
                                break;
                            default:
                                while(opStack.Count()>0 && GetOperatorPrecedence(opStack.Peek().Text)>=GetOperatorPrecedence(token.Text))
                                    outputQ.Enqueue(opStack.Pop());
                                opStack.Push(token);
                                break;
                        }
                        break;
                }
            }

            while(opStack.Count>0)
            {
                if (opStack.Peek().Text == "(")
                    throw new Exception("Unexpected");
                outputQ.Enqueue(opStack.Pop());
            }
            return outputQ.ToList();

        }
        public object Calculate(String expression)
        {
            var genericComparer = new System.Collections.Comparer(System.Globalization.CultureInfo.CurrentCulture);
            var termStack = new Stack<String>();
            var termStream = ConvertInfixToRPN(Lexer.DefaultLexer.Tokenize(expression)).GetEnumerator();

            while (termStream.MoveNext())
            {
                Console.WriteLine($"{termStream.Current.Text}");
            }
            termStream = ConvertInfixToRPN(Lexer.DefaultLexer.Tokenize(expression)).GetEnumerator();
            while (termStream.MoveNext())
            {
                if (termStream.Current.Type == LexerToken.LexerTokenType.Whitespace)
                    continue;
                if (termStream.Current.Type == LexerToken.LexerTokenType.Comment)
                    continue;

                String s = termStream.Current.Text;
                object opL;
                object opR;
                if ((termStream.Current.Type&(LexerToken.LexerTokenType.Integer_Literal | LexerToken.LexerTokenType.Real_Literal))!=0)
                {
                    termStack.Push(s);
                    continue;
                }
                switch (s)
                {
                    case "+":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                        termStack.Push(AddTerms(GetTermValue(termStack.Pop()), GetTermValue(termStack.Pop())).ToString());

                        break;
                    case "-":
                        if (termStack.Count < 1)
                            throw new InvalidOperationException("Invalid expression: " + expression);
                        if (termStack.Count < 2)
                        {
                            opR = GetTermValue(termStack.Pop());
                            opL = 0m;
                        }
                        else
                        {
                            opR = GetTermValue(termStack.Pop());
                            opL = GetTermValue(termStack.Pop());
                        }
                        termStack.Push(SubtractTerms(opR, opL).ToString());
//                        termStack.Push((opL - opR).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "*":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                        //opR = GetTermValue(termStack.Pop());
                        //opL = GetTermValue(termStack.Pop());
                        termStack.Push(MultiplyTerms(GetTermValue(termStack.Pop()), GetTermValue(termStack.Pop())).ToString());

                        //termStack.Push((opL * opR).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "/":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                        //opR = GetTermValue(termStack.Pop());
                        //opL = GetTermValue(termStack.Pop());
                        termStack.Push(DivideTerms(GetTermValue(termStack.Pop()), GetTermValue(termStack.Pop())).ToString());

                        //termStack.Push((opL / opR).ToString(CultureInfo.InvariantCulture));
                        break;
                    case "%":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                        //opR = GetTermValue(termStack.Pop());
                        //opL = GetTermValue(termStack.Pop());
                        //termStack.Push((opL % opR).ToString(CultureInfo.InvariantCulture));
                        termStack.Push(ModTerms(GetTermValue(termStack.Pop()), GetTermValue(termStack.Pop())).ToString());
                        break;
                    case "^":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                        //opR = GetTermValue(termStack.Pop());
                        //opL = GetTermValue(termStack.Pop());
                        //termStack.Push(Math.Pow((double)opL, (double)opR).ToString(CultureInfo.InvariantCulture));
                        termStack.Push(RaiseTerms(GetTermValue(termStack.Pop()), GetTermValue(termStack.Pop())).ToString());
                        break;
                  //  case "@":
                  //      if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression: " + expression);
                  //      string opRstr = termStack.Pop();
                  //      bool explode = opRstr.EndsWith("!");
                  //      if (explode)
                  //      {
                  //          opR = GetTermValue(opRstr.Substring(0, opRstr.Length - 1));
                  //          explode = (opR >= 2); //Prevent infinit recursion on xd1
                  //      }
                  //      else opR = GetTermValue(opRstr);
                  //      opL = GetTermValue(termStack.Pop());
                  //      int roll = 0;
                  //      int tot = 0;
                  //      for (int r = 0; r < opL; r++)
                  //      {
                  //          do
                  //          {
                  //              roll = (RNG.Next((int)opR) + 1);
                  //              tot += roll;
                  //          } while (explode && roll == (int)opR);
                  //      }
                  //      termStack.Push(tot.ToString());
                   //     break;
                    case "==":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        termStack.Push((opR.Equals(opL)) ? "1" : "0");
                        break;
                    case "!=":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        termStack.Push((!(opR.Equals(opL))) ? "1" : "0");
                        break;
                    case "<=":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        termStack.Push((CompareTerms(opR, opL, genericComparer) <= 0) ? "1" : "0");
                        //termStack.Push((opR <= opL) ? "1" : "0");
                        break;
                    case "<":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        //                        termStack.Push((opR != opL) ? "1" : "0");
                        termStack.Push((CompareTerms(opR, opL, genericComparer) < 0) ? "1" : "0");
                        break;
                    case ">":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        //                        termStack.Push((opR != opL) ? "1" : "0");
                        termStack.Push((CompareTerms(opR, opL, genericComparer) > 0) ? "1" : "0");

                        break;
                    case ">=":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        opL = GetTermValue(termStack.Pop());
                        //termStack.Push((opR != opL) ? "1" : "0");
                        termStack.Push((CompareTerms(opR, opL, genericComparer) >= 0) ? "1" : "0");

                        break;
                    //case "::":
                    case "=":
                        if (termStack.Count < 2) throw new InvalidOperationException("Invalid expression:" + expression);
                        opR = GetTermValue(termStack.Pop());
                        //opL=GetTermValue(terms)
                        //String vData = GetTermValue(termStack.Pop()).ToString();
                        String vName = termStack.Pop();
                        if (!_localVariables.ContainsKey(vName))
                            _localVariables.Add(vName, String.Empty);
                        _localVariables[vName] = opR.ToString();
                        termStack.Push(opR.ToString());
                        break;
                    default:

                        String term = s;
                        foreach (ICalculatorHelper helper in _helpers)
                        {
                            var helperFuncInfo = helper.GetFunction(term);
                            if (helperFuncInfo != null)
                            {
                                if (helperFuncInfo.ParameterCount == null)
                                {
                                    if (termStack.Count < 1 || termStack.Count < (Decimal)GetTermValue(termStack.Peek()))
                                        throw new InvalidOperationException("Invalid expression: " + expression);
                                }
                                else
                                {
                                    if (termStack.Count < helperFuncInfo.ParameterCount)
                                        throw new InvalidOperationException("Invalid expression: " + expression);
                                }
                                var args = new object[helperFuncInfo.ParameterCount ?? (int)((Decimal)GetTermValue(termStack.Pop()))];
                                for (int i = 0; i < args.Length; i++)
                                    args[(args.Length-1)-i] = GetTermValue(termStack.Pop());
                                termStack.Push(helperFuncInfo.Function(args).ToString());
                                goto __COMPLETED__;
                            }
                        }
                        termStack.Push(s);
                    __COMPLETED__:
                        break;
                }


            }
            /*
             * multiline comment
             */
            return GetTermValue(termStack.Pop());
            //return termStack.Pop();//GetTermValue(termStack.Pop());
        }

        private object AddTerms(object opR, object opL)
        {
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return (Decimal)opL + (Decimal)opR;
            if (opR is Decimal && opL is Decimal)
                return (Decimal)opL + (Decimal)opR;
            if (opR is String && opL is String)
                return (String)opL + (String)opR;
            throw new Exception("Unexpected");
        }
        private object SubtractTerms(object opR, object opL)
        {
            if (!(opR.GetType().IsPrimitive || opR is Decimal))
                throw new Exception("Unexpected");

            if (!(opL.GetType().IsPrimitive || opL is Decimal))
                throw new Exception("Unexpected");

            return (Decimal)opL - (Decimal)opR;
            
        }
        private object MultiplyTerms(object opR, object opL)
        {
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return (Decimal)opL * (Decimal)opR;
            if (opR is Decimal && opL is Decimal)
                return (Decimal)opL * (Decimal)opR;
            throw new Exception("Unexpected");
        }
        private object DivideTerms(object opR, object opL)
        {
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return (Decimal)opL / (Decimal)opR;
            if (opR is Decimal && opL is Decimal)
                return (Decimal)opL / (Decimal)opR;
            throw new Exception("Unexpected");
        }
        private object ModTerms(object opR, object opL)
        {
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return (Decimal)opL % (Decimal)opR;
            if (opR is Decimal && opL is Decimal)
                return (Decimal)opL % (Decimal)opR;
            throw new Exception("Unexpected");
        }
        private object RaiseTerms(object opR, object opL)
        {
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return (Decimal)Math.Pow((double)opL, (double)opR);
            if (opR is Decimal && opL is Decimal)
                return (Decimal)Math.Pow((double)opL, (double)opR);
            throw new Exception("Unexpected");
        }
        private int CompareTerms(object opR, object opL, System.Collections.Comparer compare)
        {
            if (opR.GetType() == opL.GetType()) return compare.Compare(opL, opR);
            if (opR.GetType().IsPrimitive && opL.GetType().IsPrimitive)
                return compare.Compare((Decimal)opL, (Decimal)opR);
            throw new Exception("Unexpected");
        }


        private object GetTermValue(String term)
        {
            decimal val;
            if (decimal.TryParse(term, out val)) return val;
            DateTime dtVal;
            if (DateTime.TryParse(term, out dtVal)) return dtVal;
            if (_localVariables.ContainsKey(term))
                return _localVariables[term];

            foreach (var helper in _helpers)
            {
                object pVal = helper.Map(term);
                if (pVal!=null)
                    return pVal;
            }
            return term;
        }


        private int GetOperatorPrecedence(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 0;
                case "*":
                case "/":
                case "%":
                    return 1;
                case "^":
                    return 2;
                case "@":
                    return 3;
                default:
                    return -2;
            }
        }

    }


}
