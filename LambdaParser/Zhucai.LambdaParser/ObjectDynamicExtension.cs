using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToreAurstadIT.LambdaParser.ObjectDynamicExtension
{
    static public class ObjectDynamicExtension
    {
        /// <summary>
       /// With instance as context, execute code
        /// </ summary>
        /// <typeParam name = "t"> Return the result type </ typeParam>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "namespaces"> Introduced namespace </ param>
        /// <param name = "Objects"> Parameter object (with $ 1 in Code; $ 2 means the second object ....) </ param>
        /// <returns></returns>
        static public T E<T>(this object instance, string code, string[] namespaces, params object[] objects)
            where T : class
        {
            return ExpressionParser.Exec<T>(instance,code,namespaces,objects);
        }

      /// <summary>
        /// With instance as context, execute code
        /// </ summary>
        /// <typeParam name = "t"> Return the result type </ typeParam>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "Objects"> Parameter object (with $ 1 in Code; $ 2 means the second object ....) </ param>   /// <returns></returns>
        static public T E<T>(this object instance, string code, params object[] objects)
        {
            return ExpressionParser.Exec<T>(instance, code, null, objects);
        }

    /// <summary>
        /// With instance as context, execute code
        /// </ summary>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "namespaces"> Introduced namespace </ param>
        /// <param name = "Objects"> Parameter object (with $ 1 in Code; $ 2 means the second object ....) </ param>   /// <returns></returns>
        static public object E(this object instance, string code, string[] namespaces, params object[] objects)
        {
            return ExpressionParser.Exec(instance, code, namespaces, objects);
        }

       /// <summary>
        /// With instance as context, execute code
        /// </ summary>
        /// <param name = "instance"> Execute the code with this object as context (with $ 0 in CODE, $ 0 can be omitted) </ param>
        /// <param name = "code"> Executed code </ param>
        /// <param name = "Objects"> Parameter object (with $ 1 in Code; $ 2 means the second object ....) </ param>     /// <returns></returns>
        static public object E(this object instance, string code, params object[] objects)
        {
            return ExpressionParser.Exec(instance, code, null, objects);
        }
    }
}
