using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.ErrorNotifier;
using pryLogger.src.ErrorNotifier.MailNotifier;
using ArxOne.MrAdvice.Annotation;
using System.Dynamic;

namespace pryLogger.src.Attributes
{
    public static class StackExtensions 
    {
        public static T PopOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Pop() : default;
        public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Peek() : default;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class LogAttribute : Attribute
    {
        protected static readonly List<LogEvent> RootLogs = new List<LogEvent>();
        protected static readonly Stack<LogEvent> CurrentLogs = new Stack<LogEvent>();

        public static IErrorNotifier ErrorNotifier { get; protected set; }
        protected static T GetInstance<T>() where T : LogAttribute, new() => new T();
        public static void SetErrorNotifier(IErrorNotifier errorNotifier) => ErrorNotifier = errorNotifier;

        public static LogEvent CurrentLog => CurrentLogs.PeekOrDefault();
        public static LogEvent CurrentRootLog => RootLogs.LastOrDefault();

        protected string OnExceptionName { get; set; }

        protected LogAttribute() : base() { }
        protected LogAttribute(string onExceptionName) : base() => this.OnExceptionName = onExceptionName;

        public abstract void LogAndNotify(LogEvent logEvent);
        public abstract void OnAdvice(string methodName, Action<LogEvent> callback);
        public abstract T OnAdvice<T>(string methodName, Func<LogEvent, T> callback);

        protected virtual Dictionary<string, object> GetMethodParams(MethodAdviceContext context)
        {
            ParameterInfo[] parameters = context.TargetMethod.GetParameters();
            Dictionary<string, object> paramsNameValue = new Dictionary<string, object>();

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameterInfo = parameters[index];

                bool ignoreAttribute = false;
                string paramName = parameterInfo.Name;
                var attributes = parameterInfo.GetCustomAttributes(false);

                foreach (var attr in attributes)
                {
                    Type attrType = attr.GetType();

                    if (attr is LogIgnore)
                    {
                        ignoreAttribute = true;
                        break;
                    }

                    if (attr is LogRename)
                    {
                        paramName = ((LogRename)attr).Name;
                    }
                }

                if (!ignoreAttribute)
                {
                    paramsNameValue.Add(paramName, context.Arguments[index]);
                }
            }

            return paramsNameValue;
        }
    }
}
