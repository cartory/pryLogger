using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.Attributes;
using pryLogger.src.ErrorNotifier;

namespace pryLogger.src.LogStrategies
{
    public partial class ConsoleLog : LogAttribute
    {
        public static ConsoleLog New => GetInstance<ConsoleLog>();

        public ConsoleLog() : base() { }
        public ConsoleLog(string onExceptionName) : base(onExceptionName) { }

        public override void OnAdvice(string methodName, Action<LogEvent> callback)
        {
            this.OnAdvice(methodName, log =>
            {
                callback(log);
                return 0;
            });
        }

        public override T OnAdvice<T>(string methodName, Func<LogEvent, T> callback)
        {
            int index = -1;
            LogEvent logEvent = new LogEvent(methodName, Environment.StackTrace);
            LogEvent currLog = CurrentLog?.GetFather(logEvent, isLambdaLog: true);

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

        public override void LogAndNotify(LogEvent logEvent)
        {
            Console.WriteLine($"[{logEvent.Start:s}] {logEvent.ToJson()}");

            try
            {
                if (logEvent.HasError(out LogEvent errLog)) 
                {
                    ErrorNotifier?.Notify(ErrorNotification.FromLogEvent(logEvent, errLog));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnLogAndNotify : {e.Message}");
            }
        }
    }

    public partial class ConsoleLog : IMethodAdvice
    {
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
