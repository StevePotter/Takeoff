using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Web.Mvc;
using Newtonsoft.Json;

namespace Takeoff
{
    /// <summary>
    /// Contains everything needed to perform a deferred method invoke.
    /// </summary>
    public class DeferredMethodInvokeInfo
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public object[] Parameters { get; set; }
    }

    /// <summary>
    /// Contains everything needed to serialize a method invoke expression, then deserialize and actually invoke it later.  This is used to improve app performance, offloading things like logging 
    /// </summary>
    /// <remarks>
    /// Currently this only supports static methods without overloads.  Primitive argument types are preferred.
    /// </remarks>
    public static class DeferredMethodInvoking
    {

        public static DeferredMethodInvokeInfo CreateMethodInvokeParameters(Expression<Action> action)
        {
            DeferredMethodInvokeInfo invokeInfo = new DeferredMethodInvokeInfo();
            MethodCallExpression body = action.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("action");
            }
            invokeInfo.Type = body.Method.DeclaringType.AssemblyQualifiedName;
            invokeInfo.Method = body.Method.Name;

            ParameterInfo[] parameters = body.Method.GetParameters();
            if (parameters.Length > 0)
            {
                invokeInfo.Parameters = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression arg = body.Arguments[i];
                    ConstantExpression expression2 = arg as ConstantExpression;
                    if (expression2 != null)
                    {
                        invokeInfo.Parameters[i] = expression2.Value;
                    }
                    else
                    {
                        invokeInfo.Parameters[i] = CachedExpressionCompiler.Evaluate(arg);//does all the hard work of getting values 
                    }
                }
            }
            return invokeInfo;
        }

        public static void InvokeFromSerializedInvokeInfo(DeferredMethodInvokeInfo invokeInfo)
        {
            Type declaringType = Type.GetType(invokeInfo.Type);
            if (declaringType == null)
                throw new ArgumentException("Type {0} could not be found".FormatString(invokeInfo.Type));
            var method = declaringType.GetMethod(invokeInfo.Method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                throw new ArgumentException("Method {0} could not be found".FormatString(invokeInfo.Type));
            if (invokeInfo.Parameters.HasItems())
            {
                var methodParams = method.GetParameters();
                if (invokeInfo.Parameters.Length != methodParams.Length)
                    throw new ArgumentException("Parameter count mismatch.");

                for( var i = 0; i < methodParams.Length; i++ )
                {
                    if (invokeInfo.Parameters[i] == null || invokeInfo.Parameters[i].GetType() == methodParams[i].ParameterType)
                        continue;

                    //try to convert it to the proper type
                    string serializedJson = JsonConvert.SerializeObject(invokeInfo.Parameters[i]);
                    object deserializedObject = JsonConvert.DeserializeObject(serializedJson, methodParams[i].ParameterType);
                    invokeInfo.Parameters[i] = deserializedObject;
                }

                method.Invoke(null, invokeInfo.Parameters);
            }
            else
            {
                method.Invoke(null, null);
            }

        }

    }

}
