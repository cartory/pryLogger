using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.ErrorNotifier;

namespace pryLogger.src.Attributes
{
    public static class StackExtensions 
    {
        public static T PopOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Pop() : default;
        public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Peek() : default;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class LogAttribute : Attribute, IMethodAdvice
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

        public virtual void OnAdvice(string methodName, Action<LogEvent> callback) 
        {
            this.OnAdvice(methodName, log =>
            {
                callback(log);
                return 0;
            });
        }

        public virtual T OnAdvice<T>(string methodName, Func<LogEvent, T> callback) 
        {
            int index = -1;
            LogEvent logEvent = new LogEvent(methodName, Environment.StackTrace);
            LogEvent currLog = CurrentRootLog?.GetFather(logEvent, isLambdaLog: true);

            if (currLog != null)
            {
                currLog.GetEvents().Add(logEvent);
            }
            else
            {
                RootLogs.Add(logEvent);
                index = RootLogs.Count - 1;
            }

            CurrentLogs.Push(logEvent);
            var result = callback(logEvent);

            logEvent.Finish();
            CurrentLogs.PopOrDefault();

            if (index >= 0)
            {
                LogAndNotify(logEvent);
                RootLogs.RemoveAt(index);
            }

            logEvent.Returns = result;
            return result;
        }

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

        public void Advise(MethodAdviceContext context)
        {
            lock (this)
            {
                string method = $"{context.TargetType.FullName}.{context.TargetMethod.Name}";

                LogEvent logEvent = new LogEvent(method, Environment.StackTrace);
                Dictionary<string, object> parameters = this.GetMethodParams(context);

                int index = -1;
                object onErrorReturn = null;

                CurrentLogs.Push(logEvent);
                LogEvent currLog = CurrentRootLog?.GetFather(logEvent);

                if (currLog != null)
                {
                    currLog.GetEvents().Add(logEvent);
                }
                else
                {
                    RootLogs.Add(logEvent);
                    index = RootLogs.Count - 1;
                }

                try
                {
                    if (parameters.Count > 0)
                    {
                        logEvent.Params = parameters;
                    }

                    context.Proceed();
                }
                catch (Exception e)
                {
                    Type type = context.TargetType;
                    MethodInfo methodInfo = type.GetMethod(this.OnExceptionName);

                    if (methodInfo == null) throw;

                    logEvent.Error = Error.FromException(e);
                    onErrorReturn = methodInfo.Invoke(context.Target, new object[] { e });
                }

                if (context.HasReturnValue)
                {
                    logEvent.Returns = onErrorReturn ?? context.ReturnValue;
                }

                logEvent.Finish();
                CurrentLogs.PopOrDefault();

                if (index >= 0)
                {
                    LogAndNotify(logEvent);
                    RootLogs.RemoveAt(index);
                }
            }
        }
    }
}
