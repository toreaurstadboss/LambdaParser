using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;

namespace ToreAurstadIT.LambdaParser
{
   /// <summary>
    /// Lambda expression parser core class
    /// </ summary>
    /// <typeparam name = "tdlegate"> </ typeparam>
    internal class ExpressionParserCore<TDelegate>
    {
        #region fields.字段

        private CodeParser _codeParser;

        private Type _delegateType;

        private Type _defaultInstanceType;
        private ParameterExpression _defaultInstanceParam;

        private Type[] _paramTypes;

        private bool _firstTypeIsDefaultInstance;

      /// <summary>
        /// store parameters
        /// </ summary>
        private List<ParameterExpression> _params = new List<ParameterExpression>();

    /// <summary>
        /// Store the priority of the operator
        /// </ summary>
        static private Dictionary<string, int> _operatorPriorityLevel = new Dictionary<string, int>();

      /// <summary>
        /// Store the implicit conversion level of the digital type
        /// </ summary>
        static private Dictionary<Type, int> _numberTypeLevel = new Dictionary<Type, int>();

        #endregion


        #region properties.Attributes

        /// <summary>
/// Introduced namespace set.
        /// </summary>
        public List<string> Namespaces { get; private set; }

        #endregion


        #region ctor.Constructor

        static ExpressionParserCore()
        {
            //initializationoperatorPriorityLevel
            _operatorPriorityLevel.Add("(", 100);
            _operatorPriorityLevel.Add(")", 100);
            _operatorPriorityLevel.Add("[", 100);
            _operatorPriorityLevel.Add("]", 100);

            _operatorPriorityLevel.Add(".", 13);
            _operatorPriorityLevel.Add("function()", 13);
            _operatorPriorityLevel.Add("index[]", 13);
            _operatorPriorityLevel.Add("++behind", 13);
            _operatorPriorityLevel.Add("--behind", 13);
            _operatorPriorityLevel.Add("new", 13);
            _operatorPriorityLevel.Add("typeof", 13);
            _operatorPriorityLevel.Add("checked", 13);
            _operatorPriorityLevel.Add("unchecked", 13);
            _operatorPriorityLevel.Add("->", 13);

            _operatorPriorityLevel.Add("++before", 12);
            _operatorPriorityLevel.Add("--before", 12);
            _operatorPriorityLevel.Add("+before", 12);
            _operatorPriorityLevel.Add("-before", 12);
            _operatorPriorityLevel.Add("!", 12);
            _operatorPriorityLevel.Add("~", 12);
            _operatorPriorityLevel.Add("convert()", 12);
            _operatorPriorityLevel.Add("sizeof", 12);

            _operatorPriorityLevel.Add("*", 11);
            _operatorPriorityLevel.Add("/", 11);
            _operatorPriorityLevel.Add("%", 11);
            _operatorPriorityLevel.Add("+", 10);
            _operatorPriorityLevel.Add("-", 10);
            _operatorPriorityLevel.Add("<<", 9);
            _operatorPriorityLevel.Add(">>", 9);
            _operatorPriorityLevel.Add(">", 8);
            _operatorPriorityLevel.Add("<", 8);
            _operatorPriorityLevel.Add(">=", 8);
            _operatorPriorityLevel.Add("<=", 8);
            _operatorPriorityLevel.Add("is", 8);
            _operatorPriorityLevel.Add("as", 8);
            _operatorPriorityLevel.Add("==", 7);
            _operatorPriorityLevel.Add("!=", 7);
            _operatorPriorityLevel.Add("&", 6);
            _operatorPriorityLevel.Add("^", 6);
            _operatorPriorityLevel.Add("|", 6);
            _operatorPriorityLevel.Add("&&", 5);
            _operatorPriorityLevel.Add("||", 5);
            _operatorPriorityLevel.Add("?", 5);
            _operatorPriorityLevel.Add("??", 4);
            _operatorPriorityLevel.Add("=", 4);
            _operatorPriorityLevel.Add("+=", 4);
            _operatorPriorityLevel.Add("-=", 4);
            _operatorPriorityLevel.Add("*=", 4);
            _operatorPriorityLevel.Add("/=", 4);
            _operatorPriorityLevel.Add("%=", 4);
            _operatorPriorityLevel.Add("&=", 4);
            _operatorPriorityLevel.Add("|=", 4);
            _operatorPriorityLevel.Add("^=", 4);
            _operatorPriorityLevel.Add(">>=", 4);
            _operatorPriorityLevel.Add("<<=", 4);

            // 初始化_numberTypeLevel
            _numberTypeLevel.Add(typeof(byte), 1);
            _numberTypeLevel.Add(typeof(short), 2);
            _numberTypeLevel.Add(typeof(ushort), 3);
            _numberTypeLevel.Add(typeof(int), 4);
            _numberTypeLevel.Add(typeof(uint), 5);
            _numberTypeLevel.Add(typeof(long), 6);
            _numberTypeLevel.Add(typeof(ulong), 7);
            _numberTypeLevel.Add(typeof(float), 8);
            _numberTypeLevel.Add(typeof(double), 9);
            _numberTypeLevel.Add(typeof(decimal), 10);
        }

     /// <summary>
        /// Construct the parser of Lambda expression
        /// </ summary>
        /// <param name = "code"> Lambda expression code. Such as: m => m.to string () </ param>
        internal ExpressionParserCore(Type delegateType,string code,  Type defaultInstanceType, Type[] paramTypes, bool firstTypeIsDefaultInstance)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            this._codeParser = new CodeParser(code);
            this._defaultInstanceType = defaultInstanceType;
            this._firstTypeIsDefaultInstance = firstTypeIsDefaultInstance;
            this._paramTypes = paramTypes;
            this.Namespaces = new List<string>();
            if (delegateType != null)
            {
                this._delegateType = delegateType;
            }
            else
            {
                this._delegateType = typeof(TDelegate);
            }

          // Judgment No to specify a specific delegation type
            if (firstTypeIsDefaultInstance && this._delegateType.IsSubclassOf(typeof(MulticastDelegate)))
            {
                MethodInfo methodInfo = _delegateType.GetMethod("Invoke");
                if (methodInfo != null)
                {
                    ParameterInfo firstParam = methodInfo.GetParameters().FirstOrDefault();
                    if (firstParam != null)
                    {
                        this._defaultInstanceType = firstParam.ParameterType;
                    }
                }
            }

            if (this._defaultInstanceType != null)
            {
                _defaultInstanceParam = Expression.Parameter(this._defaultInstanceType, "___DefaultInstanceParam");

              // Add a default parameter
                this._params.Insert(0, this._defaultInstanceParam);
            }
        }

        #endregion


        #region method.method

        /// <summary>
        ///Convert intoLambdaExpression
        /// </summary>
        /// <returns></returns>
        public LambdaExpression ToLambdaExpression()
        {
            // Get the parameter type of the commission
            if (this._paramTypes == null)
            {
                MethodInfo methodInfo = _delegateType.GetMethod("Invoke");
                if (methodInfo != null)
                {
                    this._paramTypes = methodInfo.GetParameters().Select(m => m.ParameterType).ToArray();
                }
            }

            int paramIndexPrefix = 0;
            if (_defaultInstanceType != null)
            {
                paramIndexPrefix = 1;
            }

            //Check if there is a lambda pre-character (such as: m =>)
            string val = _codeParser.ReadString();
            bool hasLambdaPre = false;
            if (val == "(")
            {
                string bracketContent = GetBracketString(true);
                if (bracketContent != null)
                {
                    string lambdaOperator = _codeParser.ReadString();
                    if (lambdaOperator == "=>")
                    {
                        hasLambdaPre = true;

                        //Parsing parameters
                        string[] paramsName = bracketContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < paramsName.Length; i++)
                        {
                            string[] typeName = paramsName[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            Type paramType;
                            string paramName;
                            if (typeName.Length == 1)
                            {
                                paramType = this._paramTypes != null ? this._paramTypes[i + paramIndexPrefix] : typeof(object);
                                paramName = paramsName[i];
                            }
                            else
                            {
                                paramType = GetType(typeName[0]);
                                if (paramType == null)
                                {
                                    throw new ParseUnfindTypeException(typeName[0], this._codeParser.Index);
                                }
                                paramName = typeName[1];
                            }
                            this._params.Add(Expression.Parameter(paramType, paramName));
                        }
                    }
                }
            }
            else if (char.IsLetter(val[0]) || val[0] == '_')
            {
                // Parsing parameters
                string lambdaOperator = _codeParser.ReadString();
                if (lambdaOperator == "=>")
                {
                    hasLambdaPre = true;
                    this._params.Add(Expression.Parameter(this._paramTypes != null ? this._paramTypes[0 + paramIndexPrefix] : typeof(object), val));
                }
            }

            // Recover the Parser to the initial status if there is no Lambda preamble (such as: m =>)
            if (!hasLambdaPre)
            {
                _codeParser.RevertPosition();
            }

            bool isCloseWrap;
            Expression expression = ReadExpression(0, null, out isCloseWrap);

            // Specific delegate
            if (this._delegateType.IsSubclassOf(typeof(MulticastDelegate)))
            {
                return Expression.Lambda(_delegateType, expression, this._params.ToArray());
            }
            //Not specific delegate
            else
            {
                return Expression.Lambda(expression, this._params.ToArray());
            }
        }

        /// <summary>
        /// Read Expression. May cause recursive。
        /// </summary>
        /// <param name = "priority level"> Priority of the current operation </ param>
        /// <param name = "wrap start"> Brand start (if any) </ param>
        /// <param name = "is closed wrap"> Does the symbol end value </ param>
        /// <returns></returns>
        private Expression ReadExpression(int priorityLevel, string wrapStart, out bool isClosedWrap)
        {
            // initialization
            isClosedWrap = false;
            string val = this._codeParser.ReadString();
            if (val == null)
            {
                return null;
            }
            char firstChar = val[0];
            Expression currentExpression = null;

            /********************** (Start)First reading, one yuan operation or an object **************************/
            // number
            if (char.IsDigit(firstChar))
            {
                // Digital analysis
                object constVal = ParseNumber(val);
                currentExpression = Expression.Constant(constVal);
            }
            // Non-numeric
            else
            {
               // letter or character
                switch (val)
                {
                    #region case "null":
                    case "null":
                        currentExpression = Expression.Constant(null);
                        break;
                    #endregion

                    #region case "true":
                    case "true":
                        currentExpression = Expression.Constant(true);
                        break;
                    #endregion

                    #region case "false":
                    case "false":
                        currentExpression = Expression.Constant(false);
                        break;
                    #endregion

                    //case "void":
                    //    currentExpression = Expression.Constant(typeof(System.Void));
                    //    break;

                    #region case "sizeof":
                    case "sizeof":
                        {
                            string str = GetBracketString(false);
                            Type type = GetType(str);
                            currentExpression = Expression.Constant(System.Runtime.InteropServices.Marshal.SizeOf(type));
                        }
                        break;
                    #endregion

                    #region case "typeof":
                    case "typeof":
                        {
                            //string str = GetBracketString(false);
                            _codeParser.ReadSymbol("(");
                            Type type = ReadType(null);
                            _codeParser.ReadSymbol(")");

                            currentExpression = Expression.Constant(type, typeof(Type));
                        }
                        break;
                    #endregion

                    #region case "new":
                    case "new":
                        {
                            //Get type
                            Type type = ReadType(_codeParser.ReadString());

                            // is there an array
                            string bracketStart = _codeParser.ReadString();
                            if (bracketStart == "(")
                            {
                                // Get parameters
                                List<Expression> listParam = ReadParams("(", true);

                                // Get constructor
                                ConstructorInfo constructor = type.GetConstructor(listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                currentExpression = Expression.New(constructor, listParam);

                                // Member initialization / set initialization
                                if (_codeParser.PeekString() == "{")
                                {
                                    _codeParser.ReadString();

                                    // The test is: member initialization OR set initialization
                                    var position = _codeParser.SavePosition();
                                    string str = _codeParser.ReadString();
                                    if (str != "}")
                                    {
                                        bool isMemberInit = (_codeParser.ReadString() == "=");
                                        _codeParser.RevertPosition(position);

                                        // Member initialization
                                        if (isMemberInit)
                                        {
                                            List<MemberBinding> listMemberBinding = new List<MemberBinding>();
                                            string memberName;
                                            while ((memberName = _codeParser.ReadString()) != "}")
                                            {
                                                _codeParser.ReadSymbol("=");

                                                MemberInfo memberInfo = type.GetMember(memberName)[0];
                                                MemberBinding memberBinding = Expression.Bind(memberInfo, ReadExpression(0, wrapStart, out isClosedWrap));
                                                listMemberBinding.Add(memberBinding);

                                                // comma
                                                string comma = _codeParser.ReadString();
                                                if (comma == "}")
                                                {
                                                    break;
                                                }
                                                ParseException.Assert(comma, ",", _codeParser.Index);
                                            }
                                            currentExpression = Expression.MemberInit((NewExpression)currentExpression, listMemberBinding);
                                        }
                                        // Collection initialization
                                        else
                                        {
                                            List<Expression> listExpression = new List<Expression>();
                                            while (true)
                                            {
                                                listExpression.Add(ReadExpression(0, wrapStart, out isClosedWrap));

                                                //comma
                                                string comma = _codeParser.ReadString();
                                                if (comma == "}")
                                                {
                                                    break;
                                                }
                                                ParseException.Assert(comma, ",", _codeParser.Index);
                                            }
                                            currentExpression = Expression.ListInit((NewExpression)currentExpression, listExpression);
                                        }
                                    }
                                }
                            }
                            else if (bracketStart == "[")
                            {
                                string nextStr = _codeParser.PeekString();

                                // Reading []
                                List<Expression> listLen = new List<Expression>();
                                if (nextStr == "]")
                                {
                                    _codeParser.ReadString();
                                }
                                else
                                {
                                    listLen = ReadParams("[", true);
                                }

                                // The array initialization in {}
                                string start = _codeParser.PeekString();
                                if (start == "{")
                                {
                                    List<Expression> listParams = ReadParams("{", false);
                                    currentExpression = Expression.NewArrayInit(type, listParams);
                                }
                                else
                                {
                                    currentExpression = Expression.NewArrayBounds(type, listLen);
                                }
                            }
                            else
                            {
                                throw new ParseUnknownException(bracketStart, _codeParser.Index);
                            }
                        }
                        break;
                    #endregion

                    #region case "+":
                    case "+":
                        // Ignore preamp +
                        return ReadExpression(priorityLevel, wrapStart, out isClosedWrap);
                    #endregion

                    #region case "-":
                    case "-":
                        currentExpression = Expression.Negate(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;
                    #endregion

                    #region case "!":
                    case "!":
                        currentExpression = Expression.Not(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;
                    #endregion

                    #region case "~":
                    case "~":
                        currentExpression = Expression.Not(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;
                    #endregion

                    #region case "(":
                    case "(":
                        {
                            CodeParserPosition position = _codeParser.SavePosition();
                            string str = GetBracketString(true);
                            Type type = GetType(str);

                            // Find type, as type conversion processing
                            if (type != null)
                            {
                                currentExpression = Expression.Convert(ReadExpression(GetOperatorLevel("convert()", true), wrapStart, out isClosedWrap), type);
                            }
                            // No type is found, as used for priority
                            else
                            {
                                _codeParser.RevertPosition(position);

                                //Assign a new IS CLOSED WRAP variable
                                bool newIsClosedWrap;
                                currentExpression = ReadExpression(0, val, out newIsClosedWrap);
                            }
                        }
                        break;
                    #endregion

                    #region case ")":
                    case ")":
                        {
                            // End an is closed Wrap variable
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    #region case "]":
                    case "]":
                        {
                            // End a is closed Wrap variable
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    #region case "}":
                    case "}":
                        {
                            // End a is closed Wrap variable
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    #region case ".":
                    case ".":
                        {
                            //todo:?
                            //return null;
                            throw new ParseUnknownException(".", this._codeParser.Index);
                        }
                    #endregion

                    #region case ",":
                    case ",":
                        {
                            return ReadExpression(priorityLevel, wrapStart, out isClosedWrap);
                        }
                    #endregion

                    #region default:
                    default:
                        {
                            // Head char is a letter or underscore
                            if (char.IsLetter(firstChar) || firstChar == '_' || firstChar == '$')
                            {
                                // By default instance method call
                                if (_defaultInstanceType != null && _codeParser.PeekString() == "(")
                                {
                                    //Get parameters
                                    List<Expression> listParam = ReadParams("(", false);

                                    MethodInfo methodInfo = _params[0].Type.GetMethod(val,
                                        listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                    currentExpression = Expression.Call(_params[0], methodInfo, listParam.ToArray());
                                }
                                // Parameter OR Class OR Default Instance
                                else
                                {
                                    ParameterExpression parameter;
                                    //parameter
                                    if ((parameter = this._params.SingleOrDefault(m => m.Name == val))
                                        != null)
                                    {
                                        currentExpression = parameter;
                                    }
                                    // Default instance properties
                                    else if (this._defaultInstanceType != null &&
                                        (this._defaultInstanceType.GetProperty(val) != null
                                            || this._defaultInstanceType.GetField(val) != null))
                                    {
                                        currentExpression = Expression.PropertyOrField(_params[0], val);
                                    }
                                    //kind
                                    else
                                    {
                                        Type type = ReadType(val);

                                        _codeParser.ReadSymbol(".");
                                        string strMember = _codeParser.ReadString();
                                        string strOperator = _codeParser.PeekString();

                                        // Static method
                                        if (strOperator == "(")
                                        {
                                            //Get parameters
                                            List<Expression> listParam = ReadParams(strOperator, false);

                                            if (parameter != null)
                                            {
                                                MethodInfo methodInfo = parameter.Type.GetMethod(strMember,
                                                    listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                                currentExpression = Expression.Call(parameter, methodInfo, listParam.ToArray());
                                            }
                                            else
                                            {
                                                MethodInfo methodInfo = type.GetMethod(strMember, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                                                    listParam.ConvertAll<Type>(m => m.Type).ToArray(), null);
                                                currentExpression = Expression.Call(methodInfo, listParam.ToArray());
                                            }
                                        }
                                        // Static member(PropertyOrField)
                                        else
                                        {
                                            if (parameter != null)
                                            {
                                                currentExpression = Expression.PropertyOrField(Expression.Constant(parameter), strMember);
                                            }
                                            else
                                            {
                                                //Find attributes first
                                                PropertyInfo propertyInfo = type.GetProperty(strMember, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                                if (propertyInfo != null)
                                                {
                                                    currentExpression = Expression.Property(null, propertyInfo);
                                                }
                                                //Find a field without finding the property
                                                else
                                                {
                                                    FieldInfo fieldInfo = type.GetField(strMember, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                                    if (fieldInfo == null)
                                                    {
                                                        throw new ParseUnknownException(strMember, _codeParser.Index);
                                                    }
                                                    currentExpression = Expression.Field(null, fieldInfo);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // Head char is not a letter or underscore
                            else
                            {
                                switch (firstChar)
                                {
                                    #region case '\"':
                                    case '\"':
                                        {
                                            string str = _codeParser.DefineString;
                                            currentExpression = Expression.Constant(str);
                                        }
                                        break;
                                    #endregion

                                    #region case '@':
                                    case '@':
                                        {
                                            string str = _codeParser.DefineString;
                                            currentExpression = Expression.Constant(str);
                                        }
                                        break;
                                    #endregion

                                    #region case '\'':
                                    case '\'':
                                        {
                                            string str = _codeParser.DefineString;
                                            currentExpression = Expression.Constant(str[0]);
                                        }
                                        break;
                                    #endregion

                                    default:
                                        {
                                            throw new ParseUnknownException(val, _codeParser.Index);
                                        }
                                }
                            }
                        }
                        break;
                    #endregion
                }
            }
            /********************** (End)First reading, one yuan operation or an object **************************/


            /********************** (Start) Second (n) read, will be binary or three yuan operation **********************/
            int nextLevel = 0;
           // If the is close wrap is false (returned directly), and the priority of the next operator is greater than the current priority, the next one is calculated.
            while ((isClosedWrap == false) && (nextLevel = TryGetNextPriorityLevel()) > priorityLevel)
            {
                string nextVal = _codeParser.ReadString();

                switch (nextVal)
                {
                    #region case "[":
                    case "[":
                        {
                            // Indexer Access
                            bool newIsClosedWrap;
                            if (currentExpression.Type.IsArray)
                            {
                                currentExpression = Expression.ArrayIndex(currentExpression, ReadExpression(0, "[", out newIsClosedWrap));
                            }
                            else
                            {
                                string indexerName = "Item";

                                object[] atts = currentExpression.Type.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                                DefaultMemberAttribute indexerNameAtt = (DefaultMemberAttribute)atts.SingleOrDefault();
                                if (indexerNameAtt != null)
                                {
                                    indexerName = indexerNameAtt.MemberName;

                                    PropertyInfo propertyInfo = currentExpression.Type.GetProperty(indexerName);
                                    MethodInfo methodInfo = propertyInfo.GetGetMethod();

                                    //Get parameters
                                    List<Expression> listParam = ReadParams(nextVal, true);

                                    currentExpression = Expression.Call(currentExpression, methodInfo, listParam);
                                }
                            }
                        }
                        break;
                    #endregion

                    #region case "]":
                    case "]":
                        {
                            if (wrapStart != "[")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _codeParser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case ")":
                    case ")":
                        {
                            if (wrapStart != "(")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _codeParser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case "}":
                    case "}":
                        {
                            if (wrapStart != "{")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _codeParser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case "+":
                    case "+":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);

                            //One of them is a String type
                            if (currentExpression.Type == typeof(string) || right.Type == typeof(string))
                            {
                                // Call string.concat method
                                currentExpression = Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) }),
                                    Expression.Convert(currentExpression, typeof(object)), Expression.Convert(right, typeof(object)));
                            }
                            else
                            {
                                AdjustNumberType(ref currentExpression, ref right);
                                currentExpression = Expression.Add(currentExpression, right);
                            }
                        }
                        break;
                    #endregion

                    #region case "-":
                    case "-":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Subtract(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "*":
                    case "*":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Multiply(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "/":
                    case "/":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Divide(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "%":
                    case "%":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Modulo(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<<":
                    case "<<":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LeftShift(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">>":
                    case ">>":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.RightShift(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">":
                    case ">":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.GreaterThan(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<":
                    case "<":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LessThan(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">=":
                    case ">=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.GreaterThanOrEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<=":
                    case "<=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LessThanOrEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "==":
                    case "==":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Equal(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "!=":
                    case "!=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.NotEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ".":
                    case ".":
                        {
                            string strMember = _codeParser.ReadString();
                            string strOperator = _codeParser.PeekString();
                            // 方法
                            if (strOperator == "(")
                            {
                                // Get parameters
                                List<Expression> listParam = ReadParams("(", false);

                                MethodInfo methodInfo = currentExpression.Type.GetMethod(strMember, listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                currentExpression = Expression.Call(currentExpression, methodInfo, listParam.ToArray());
                            }
                            // PROPERTY or FIELD
                            else
                            {
                                currentExpression = Expression.PropertyOrField(currentExpression, strMember);
                            }
                        }
                        break;
                    #endregion

                    #region case "is":
                    case "is":
                        {
                            Type t = ReadType(null);
                            currentExpression = Expression.TypeIs(currentExpression, t);
                        }
                        break;
                    #endregion

                    #region case "as":
                    case "as":
                        {
                            Type t = ReadType(null);
                            currentExpression = Expression.TypeAs(currentExpression, t);
                        }
                        break;
                    #endregion

                    #region case "^":
                    case "^":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.ExclusiveOr(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "&":
                    case "&":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.And(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "|":
                    case "|":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Or(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "&&":
                    case "&&":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.AndAlso(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "||":
                    case "||":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.OrElse(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "?":
                    case "?":
                        {
                            Expression first = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            _codeParser.ReadSymbol(":");
                            Expression second = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Condition(currentExpression, first, second);
                        }
                        break;
                    #endregion

                    #region case "??":
                    case "??":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            Expression test = Expression.Equal(currentExpression, Expression.Constant(null, currentExpression.Type));
                            currentExpression = Expression.Condition(test, right, currentExpression);
                        }
                        break;
                    #endregion

                    default:
                        throw new ParseUnknownException(nextVal, _codeParser.Index);
                }
            }
            /********************** (End)Second (n) read, will be binary or three yuan operation **********************/

            return currentExpression;
        }

        /// <summary>
        /// Parsing numbers
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static object ParseNumber(string val)
        {
            object constVal;
            switch (val[val.Length - 1])
            {
                case 'l':
                case 'L':
                    constVal = long.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'm':
                case 'M':
                    constVal = decimal.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'f':
                case 'F':
                    constVal = float.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'd':
                case 'D':
                    constVal = double.Parse(val.Substring(0, val.Length - 1));
                    break;

                default:
                    if (val.IndexOf('.') >= 0)
                    {
                        var numberFormat = new NumberFormatInfo();
                        numberFormat.NumberDecimalSeparator = ".";
                        if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                        {
                            numberFormat.NumberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                            val = val.Replace(".", numberFormat.NumberDecimalSeparator);
                        }

                        constVal = double.Parse(val, numberFormat);
                    }
                    else
                    {
                        constVal = long.Parse(val);
                        if ((long)constVal <= (long)int.MaxValue && (long)constVal >= (long)int.MinValue)
                        {
                            constVal = (int)(long)constVal;
                        }
                    }
                    break;
            }
            return constVal;
        }

        /// <summary>
        /// Adjust the type of the numerical operation
        /// (If an int and a double, turn int to double)
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void AdjustNumberType(ref Expression left, ref Expression right)
        {
            if (left.Type == right.Type)
            {
                return;
            }

            int leftLevel = _numberTypeLevel[left.Type];
            int rightLevel = _numberTypeLevel[right.Type];

            if (leftLevel > rightLevel)
            {
                right = Expression.Convert(right, left.Type);
            }
            else
            {
                left = Expression.Convert(left, right.Type);
            }
        }

        /// <summary>
     /// Read the parameters in the method call
        /// </ summary>
        /// <param name = "priority level"> Priority of the current operation </ param>
        /// <returns> </ returns>
        private List<Expression> ReadParams(string startSymbol, bool hasReadPre)
        {
            //Read the front bracket
            if (!hasReadPre)
            {
                _codeParser.ReadSymbol(startSymbol);
            }

            // Read parameters
            List<Expression> listParam = new List<Expression>();
            bool newIsClosedWrap = false;
            while (!newIsClosedWrap)
            {
                Expression expression = ReadExpression(0, startSymbol, out newIsClosedWrap);
                if (expression == null)
                {
                    break;
                }
                listParam.Add(expression);
            }
            return listParam;
        }

        /// <summary>
    /// Read the string in the parentheses
        /// </ summary>
        /// <param name = "HAS READ PRE"> Do you have read the front parentheses </ param>
        /// <returns> </ returns>
        private string GetBracketString(bool hasReadPre)
        {
            // Save the restore point
            CodeParserPosition position = _codeParser.SavePosition();

           // read(
            if (!hasReadPre)
            {
                _codeParser.ReadSymbol("(");
            }

            // Read the intermediate content
            StringBuilder sb = new StringBuilder();
            string str = null;
            while ((str = this._codeParser.ReadString(false)) != ")")
            {
                //Read (, it means that the brackets have nested, restore, return null
                if (str == "(")
                {
                    _codeParser.RevertPosition(position);
                    return null;
                }

                sb.Append(str);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the priority of the next operation. -1 means no operation。
        /// </summary>
        /// <returns></returns>
        private int TryGetNextPriorityLevel()
        {
            string nextString = _codeParser.PeekString();
            if (string.IsNullOrEmpty(nextString) || nextString == ";" || nextString == "}" || nextString == "," || nextString == ":")
            {
                return -1;
            }

            return GetOperatorLevel(nextString, false);
        }

        /// <summary>
        /// Get the priority of the operator, the higher the priority, the higher the priority
        /// </summary>
        /// <param name="operatorSymbol">Operator</param>
        /// <param name="isBefore">Whether the front operator (one yuan)</param>
        /// <Returns> Priority </ RETURNS>
        static private int GetOperatorLevel(string operatorSymbol, bool isBefore)
        {
            switch (operatorSymbol)
            {
                case "++":
                case "--":
                    operatorSymbol += isBefore ? "before" : "behind";
                    break;

                case "+":
                case "-":
                    operatorSymbol += isBefore ? "before" : null;
                    break;
            }
            return _operatorPriorityLevel[operatorSymbol];
        }

        /// <summary>
        ///Read type
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private Type ReadType(string val)
        {

            Type type = null;
            string strVal;
            if (string.IsNullOrEmpty(val))
            {
                strVal = _codeParser.ReadString();
            }
            else
            {
                strVal = val;
            }

            while (type == null)
            {
                //Reading parameters
                if (_codeParser.PeekString() == "<")
                {
                    _codeParser.ReadString();
                    List<Type> listGenericType = new List<Type>();
                    while (true)
                    {
                        listGenericType.Add(ReadType(null));
                        if (_codeParser.PeekString() == ",")
                        {
                            _codeParser.ReadString();
                        }
                        else
                        {
                            break;
                        }
                    }
                    _codeParser.ReadSymbol(">");

                    strVal += string.Format("`{0}[{1}]", listGenericType.Count,
                        string.Join(",", listGenericType.Select(m => m.FullName).ToArray()));
                }

                type = GetType(strVal);
                if (type == null)
                {
                    bool result = _codeParser.ReadSymbol(".", false);
                    if (!result)
                    {
                        throw new ParseUnfindTypeException(strVal, _codeParser.Index);
                    }
                    strVal += "." + _codeParser.ReadString();
                }
            }
            return type;
        }

        /// <summary>
        /// Get the type object according to the type name
        /// </summary>
    /// <param name = "type name"> Type name. It can be short-written: such as int, string </ param>
        /// <returns></returns>
        private Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // Nullable
            bool isNullable = false;
            if (typeName.EndsWith("?"))
            {
                isNullable = true;
                typeName = typeName.Substring(0, typeName.Length - 1);
            }
            else if(_codeParser.PeekString() == "?")
            {
                isNullable = true;
                _codeParser.ReadString();
            }

            Type type;

            switch (typeName)
            {
                case "bool":
                    type = typeof(bool);
                    break;

                case "byte":
                    type = typeof(byte);
                    break;

                case "sbyte":
                    type = typeof(sbyte);
                    break;

                case "char":
                    type = typeof(char);
                    break;

                case "decimal":
                    type = typeof(decimal);
                    break;

                case "double":
                    type = typeof(double);
                    break;

                case "float":
                    type = typeof(float);
                    break;

                case "int":
                    type = typeof(int);
                    break;

                case "uint":
                    type = typeof(uint);
                    break;

                case "long":
                    type = typeof(long);
                    break;

                case "ulong":
                    type = typeof(ulong);
                    break;

                case "object":
                    type = typeof(object);
                    break;

                case "short":
                    type = typeof(short);
                    break;

                case "ushort":
                    type = typeof(ushort);
                    break;

                case "string":
                    type = typeof(string);
                    break;

                default:
                    {
                      // First, Type Name is a full name of the class.
                        type = GetTypeCore(typeName);

                     // Nothing to find all the namespace to match again
                        if (type == null)
                        {
                            foreach (string theNamespace in this.Namespaces)
                            {
                                type = GetTypeCore(theNamespace + "." + typeName);
                                if (type != null)
                                {
                                    break;
                                }

                        //// Find instant stop, don't continue to find (if there are two namespaces under the two namespaces, it will always return the first one, not an error)         if (type != null)
                        //        {
                        //            break;
                        //        }
                            }
                        }
                    }
                    break;
            }

            if (isNullable && type != null)
            {
                type = typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        private static IEnumerable<T> SkipLastN<T>(IEnumerable<T> source, int n)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            var cache = new Queue<T>(n + 1);

            do
            {
                if (hasRemainingItems = it.MoveNext())
                {
                    cache.Enqueue(it.Current);
                    if (cache.Count > n)
                        yield return cache.Dequeue();
                }
            } while (hasRemainingItems);
        }


       /// <summary>
        // / Object of the type according to the type name
        /// </ summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private Type GetTypeCore(string typeName)
        {
            Assembly[] listAssembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in listAssembly)
            {
                if (assembly != Assembly.GetExecutingAssembly())  // Ignore the current assembly (Tore AurStad it.lambda Parser)
                {
                    Type type = assembly.GetType(typeName, false, false);
                    if (type != null)
                    {
                        return type;
                    }
                    if (typeName.Contains("."))
                    {
                        string typeNamePrefix = string.Join(".", SkipLastN(typeName.Split('.').ToArray(), 1).ToArray()); //check if expression atom is an enum value.
                        type = assembly.GetType(typeNamePrefix);
                        if (type != null)
                        {
                            return type;
                        }
                    }


                }
            }
            return null;
        }

        #endregion
    }
}
