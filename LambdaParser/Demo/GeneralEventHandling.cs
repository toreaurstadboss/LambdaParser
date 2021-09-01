using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using ToreAurstadIT.LambdaParser;

// From fitted with head blog：http://www.cnblogs.com/Ninputer/archive/2009/09/08/expression_tree3.html
namespace Demo
{

    public static class GeneralEventHandling
    {
        static object GeneralHandler(params object[] args)
        {
            Console.WriteLine("您的事件发生了说");
            System.Windows.Forms.MessageBox.Show("您的事件发生了说");
            return null;
        }

        // Primitive
        public static void AttachGeneralHandler(object target, EventInfo targetEvent)
        {
            //Get a delegation type of an event response program
            var delegateType = targetEvent.EventHandlerType;

            //This entrusted Invoke method has the signature information we need.
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");

            //Parameters required according to this entrusted production
            ParameterInfo[] parameters = invokeMethod.GetParameters();
            ParameterExpression[] paramsExp = new ParameterExpression[parameters.Length];
            Expression[] argsArrayExp = new Expression[parameters.Length];

            //Parameters turn into object types. Some of themselves is Object, manage him ...
            for (int i = 0; i < parameters.Length; i++)
            {
                paramsExp[i] = Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
                argsArrayExp[i] = Expression.Convert(paramsExp[i], typeof(Object));
            }

            //Call usGeneralHandler
            MethodInfo executeMethod = typeof(GeneralEventHandling).GetMethod(
                "GeneralHandler", BindingFlags.Static | BindingFlags.NonPublic);

            Expression lambdaBodyExp =
                Expression.Call(null, executeMethod, Expression.NewArrayInit(typeof(Object), argsArrayExp));

            //If there is a return value, turn the return value into the type of commissioned requirements.
            // If there is no return value, it will be there.
            if (!invokeMethod.ReturnType.Equals(typeof(void)))
            {
                //This is the case where there is a return value
                lambdaBodyExp = Expression.Convert(lambdaBodyExp, invokeMethod.ReturnType);
            }

            //Assemble together together
            LambdaExpression dynamicDelegateExp = Expression.Lambda(delegateType, lambdaBodyExp, paramsExp);

            //The expression we created is such a function:
            //(委托的参数们) => GeneralHandler(new object[] { 委托的参数们 })

            //Compile
            Delegate dynamiceDelegate = dynamicDelegateExp.Compile();

            //Finish!
            targetEvent.AddEventHandler(target, dynamiceDelegate);
        }

        // 新函数
        public static void NewAttachGeneralHandler(object target, EventInfo targetEvent)
        {
            // Entrust type of event response program
            var delegateType = targetEvent.EventHandlerType;

            //This entrusted Invoke method has the signature information we need.
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = invokeMethod.GetParameters();

            //The expression we created is such a function：
            //(委托的参数们) => (返回值类型)GeneralHandler(new object[] { 委托的参数们 })
            string lambdaCode = string.Format("({0})=>{1}GeneralEventHandling.GeneralHandler(new object[]{{{2}}})",
                string.Join(",", parameters.Select(m => m.Name).ToArray()),
                invokeMethod.ReturnType.Equals(typeof(void))?"":"("+invokeMethod.ReturnType.FullName+")",
                string.Join(",", parameters.Select(m => m.Name).ToArray()));

            Delegate dynamiceDelegate = ExpressionParser.Compile(delegateType, lambdaCode, "Demo"); // 最后一个参数Demo是命名空间

            //Finish!
            targetEvent.AddEventHandler(target, dynamiceDelegate);
        }
    }
}
