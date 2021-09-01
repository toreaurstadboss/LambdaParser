using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace ToreAurstadIT.LambdaParser
{
    /// <summary>
    /// Lambda表达式的解析器
    /// </summary>
    static public class ExpressionParser
    {
        #region all Parse()

        /// <summary>
       /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public LambdaExpression Parse(string lambdaCode, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, null, false,null, namespaces);
        }

        /// <summary>
      /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public LambdaExpression Parse(string lambdaCode, Type defaultInstance, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, defaultInstance, false,null, namespaces);
        }

        /// <summary>
     /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public LambdaExpression Parse(string lambdaCode, Type defaultInstance,Type[] paramTypes, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, defaultInstance, false, paramTypes, namespaces);
        }

        /// <summary>
        /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "delegate Type"> Entrusted type </ param>
        /// <param name = "first type is default instance"> Whether the first type is the default instance </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public LambdaExpression Parse(Type delegateType, string lambdaCode, params string[] namespaces)
        {
            return ParseCore<Delegate>(delegateType, lambdaCode, null, false,null, namespaces);
        }

       /// <summary>
        /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "delegate Type"> Entrusted type </ param>
        /// <param name = "first type is default instance"> Whether the first type is the default instance </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public LambdaExpression Parse(Type delegateType, string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return ParseCore<Delegate>(delegateType, lambdaCode, null, firstTypeIsDefaultInstance, null, namespaces);
        }

        /// <summary>
       /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, false, null, namespaces);
        }

        /// <summary>
       /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, firstTypeIsDefaultInstance, null, namespaces);
        }

        #endregion

        #region all Compile()

       /// <summary>
        /// Analyze the lambda expression code and compile into commission
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Delegate Compile(string lambdaCode, params string[] namespaces)
        {
            return Parse(lambdaCode, namespaces).Compile();
        }

    /// <summary>
        /// Analyze the lambda expression code and compile into commission
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Delegate Compile(string lambdaCode, Type defaultInstance, params string[] namespaces)
        {
            return Parse(lambdaCode, defaultInstance, namespaces).Compile();
        }

     /// <summary>
        /// Analyze the lambda expression code and compile into commission
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "delegate Type"> Entrusted type </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Delegate Compile(Type delegateType, string lambdaCode, params string[] namespaces)
        {
            return Parse(delegateType, lambdaCode, namespaces).Compile();
        }

        /// <summary>
      /// Analyze the lambda expression code and compile into commission
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "delegate Type"> Entrusted type </ param>
        /// <param name = "first type is default instance"> Whether the first type is the default instance </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public Delegate Compile(Type delegateType, string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return Parse(delegateType, lambdaCode, firstTypeIsDefaultInstance, namespaces).Compile();
        }

    /// <summary>
        /// Analyze the lambda expression code and compile into commission
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public TDelegate Compile<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return Parse<TDelegate>(lambdaCode, namespaces).Compile();
        }

       /// <summary>
        /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static public TDelegate Compile<TDelegate>(string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return Parse<TDelegate>(lambdaCode, firstTypeIsDefaultInstance, namespaces).Compile();
        }

        #endregion

        #region all Exec()

     /// <summary>
        /// With instance as context, execute code
        /// ($ 0 indicates instance, ($ 0 can be omitted); $ 1 indicates the first object of Objects; $ 2 indicates the second object of Objects ....)
        /// </ summary>
        /// <typeParam name = "t"> Return the result type </ typeParam>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "namespaces"> Introduced namespace </ param>
        /// <param name = "objects"> Parameter object </ param>
        /// <returns></returns>
        static public T Exec<T>(object instance, string code, string[] namespaces, params object[] objects)
        {
            object[] allObjs = new object[objects.Length + 1];
            allObjs[0] = instance;
            Array.Copy(objects, 0, allObjs, 1, objects.Length);

            object[] inputObjs = new object[objects.Length + 2];
            inputObjs[1] = inputObjs[0] = instance;
            Array.Copy(objects, 0, inputObjs, 2, objects.Length);

            // 从allObjs得到：[objectTypeName] $1,[objectTypeNameTypeName] $2...
            string lambdaParams = string.Join(",", allObjs.Select((m, i) => "$" + i).ToArray());
            Type[] paramTypes = inputObjs.Select(m => m.GetType()).ToArray();

            string newCode = string.Format("({0})=>{1}",lambdaParams,code);

            return (T)Parse(newCode, instance.GetType(), paramTypes, namespaces).Compile().DynamicInvoke(inputObjs);
        }
        
     /// <summary>
        /// With instance as context, execute code
        /// ($ 0 indicates instance, ($ 0 can be omitted); $ 1 indicates the first object of Objects; $ 2 indicates the second object of Objects ....)
        /// </ summary>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "namespaces"> Introduced namespace </ param>
        /// <param name = "objects"> Parameter object </ param>
        /// <returns> </ returns>
        static public object Exec(object instance, string code, string[] namespaces, params object[] objects)
        {
            return Exec<object>(instance, code, namespaces, objects);
        }

        #endregion

        #region private method.内部方法

      /// <summary>
        /// Analyze Lambda Expression Code
        /// </ summary>
        /// <param name = "lambda code"> Lambda expression code. Such as: m => m.to string () </ param>
        /// <param name = "namespaces"> Namespace set </ param>
        static private LambdaExpression ParseCore<TDelegate>(Type delegateType, string lambdaCode, Type defaultInstanceType, bool firstTypeIsDefaultInstance,Type[] paramTypes, params string[] namespaces)
        {
            ExpressionParserCore<TDelegate> parser = new ExpressionParserCore<TDelegate>(delegateType, lambdaCode, defaultInstanceType,paramTypes, firstTypeIsDefaultInstance);
            if (namespaces != null && namespaces.Length > 0)
            {
                parser.Namespaces.AddRange(namespaces);
            }
            return parser.ToLambdaExpression();
        }

        #endregion
    }
}
