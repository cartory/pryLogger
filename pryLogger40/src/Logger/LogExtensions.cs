using System;
using System.Reflection;

using System.Collections.Generic;
using System.Text.RegularExpressions;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.Logger.Attributes;

namespace pryLogger.src.Logger
{
    internal static class StackExtensions
    {
        public static T PopOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Pop() : default;
        public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Peek() : default;
    }

    public static class LogExtensions
    {
        public static Dictionary<string, string> ToDictionary(
            this string connectionString,
            string splitter = ";", string assignment = "="
        )
        {
            connectionString = connectionString.Trim();
            Regex regex = new Regex($"^\\s*(\\w+{assignment}.+{splitter}\\s*)*(\\w+{assignment}.+)?$");

            if (!string.IsNullOrEmpty(connectionString) && regex.IsMatch(connectionString))
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string[] props = connectionString.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string prop in props)
                {
                    string[] arrProp = prop.Split(new string[] { assignment }, StringSplitOptions.RemoveEmptyEntries);
                    values.Add(arrProp[0].Trim(), arrProp[1]);
                }

                return values;
            }

            return null;
        }

        public static Dictionary<string, object> GetParamsAsDictionary(this MethodAdviceContext context)
        {
            var parameters = context.TargetMethod.GetParameters();
            var dictionaryParams = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Length; i++)
            {
                bool paramIgnore = false;
                ParameterInfo param = parameters[i];

                string paramName = param.Name ?? $"param[{i}]";
                var paramAttributes = param.GetCustomAttributes(false);

                foreach (var paramAttr in paramAttributes)
                {
                    Type paramAttrType = paramAttr.GetType();

                    if (paramAttr is LogParamIgnoreAttribute)
                    {
                        paramIgnore = true;
                        break;
                    }

                    if (paramAttr is LogParamAttribute logParam)
                    {
                        paramName = logParam.Name;
                    }
                }

                if (!paramIgnore)
                {
                    dictionaryParams.Add(paramName, context.Arguments[i]);
                }
            }

            return dictionaryParams;
        }
    }
}
