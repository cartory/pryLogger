using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.ErrorNotifier.MailNotifier;

namespace pryLogger.src.Log.Attributes
{
    /// <summary>
    /// A collection of extension methods for working with stacks.
    /// </summary>
    public static class StackExtensions
    {
        /// <summary>
        /// Removes and returns the top item from the stack if it exists, otherwise returns the default value of the type.
        /// </summary>
        /// <typeparam name="T">The type of items stored in the stack.</typeparam>
        /// <param name="stack">The stack to pop from.</param>
        /// <returns>The top item from the stack if it exists, otherwise the default value of the type.</returns>
        public static T PopOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Pop() : default;

        /// <summary>
        /// Returns the top item from the stack if it exists, otherwise returns the default value of the type without removing it.
        /// </summary>
        /// <typeparam name="T">The type of items stored in the stack.</typeparam>
        /// <param name="stack">The stack to peek from.</param>
        /// <returns>The top item from the stack if it exists, otherwise the default value of the type.</returns>
        public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Peek() : default;
    }

    /// <summary>
    /// An abstract attribute class for logging and error handling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class LogAttribute : Attribute, IMethodAdvice
    {
        /// <summary>
        /// A list of root log events.
        /// </summary>
        protected static readonly List<LogEvent> RootLogs = new List<LogEvent>();

        /// <summary>
        /// A stack of current log events.
        /// </summary>
        protected static readonly Stack<LogEvent> CurrentLogs = new Stack<LogEvent>();

        /// <summary>
        /// Gets or sets the error mail notifier for handling error notifications.
        /// </summary>
        public static MailErrorNotifier MailErrorNotifier { get; protected set; } = new MailErrorNotifier();

        /// <summary>
        /// Sets the error notifier to be used for error notifications.
        /// </summary>
        /// <param name="errorNotifier">The error notifier implementation.</param>
        public static void SetErrorNotifier(MailErrorNotifier errorNotifier) => MailErrorNotifier = errorNotifier;

        /// <summary>
        /// Gets the current log event associated with the advised method.
        /// </summary>
        public static LogEvent CurrentLog => CurrentLogs.PeekOrDefault();

        /// <summary>
        /// Gets the current root log event.
        /// </summary>
        public static LogEvent CurrentRootLog => RootLogs.LastOrDefault();

        /// <summary>
        /// Gets or sets the mail error notifier for handling error notifications.
        /// </summary>
        protected MailErrorNotifier LocalMailErrorNotifier { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttribute"/> class.
        /// </summary>
        protected LogAttribute() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttribute"/> class with the specified <paramref name="mailTo"/> <paramref name="copyTo"/>.
        /// </summary>
        /// <param name="mailTo">The recipient email address.</param>
        /// <param name="copyTo">Optional email addresses to copy notifications to.</param>
        /// <returns>The updated MailConnection instance.</returns>
        protected LogAttribute(string mailTo, params string[] copyTo) : base()
        {
            this.LocalMailErrorNotifier = MailErrorNotifier;
            this.LocalMailErrorNotifier.MailConnection.SetMailTo(mailTo, copyTo);
        }

        /// <summary>
        /// An abstract method to be implemented by derived classes for logging and error notification.
        /// </summary>
        /// <param name="log">The log event to handle.</param>
        public abstract void LogAndNotify(LogEvent log);

        /// <summary>
        /// Executes the provided action delegate with logging and error handling.
        /// </summary>
        /// <param name="callback">The action delegate to execute.</param>
        public virtual void UseLog(Action<LogEvent> callback)
        {
            UseLog(log =>
            {
                callback(log);
                return 0;
            });
        }

        /// <summary>
        /// Executes the provided function delegate with logging and error handling.
        /// </summary>
        /// <typeparam name="T">The return type of the callback function.</typeparam>
        /// <param name="callback">The function delegate to execute.</param>
        /// <returns>The result of the callback function.</returns>
        public virtual T UseLog<T>(Func<LogEvent, T> callback)
        {
            lock (this)
            {
                int level = 0;
                int index = -1;

                var stackTrace = Environment.StackTrace
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(stack =>
                    {
                        stack = stack.Trim().Substring(3);

                        if (Regex.IsMatch(stack.ToLower(), @"^(.+\.)+uselog\[\w+\]\(func`2"))
                        {
                            level++;
                        }

                        return stack;
                    })
                    .Where(st => !Regex.IsMatch(st.ToLower(), @"^(.+\.)+<>.*:line \d+$"))
                    .Where(st => !Regex.IsMatch(st.ToLower(), @"^(system\.environment|arxone|prylogger)\."))
                    .ToList();

                string currMethod = stackTrace.FirstOrDefault() ?? "void";
                currMethod = Regex.Replace(currMethod, @"′?\(.*\) in .*:line \d+$", string.Empty) + $"[{level}]";

                stackTrace = stackTrace
                    .Where(st => !Regex.IsMatch(st, @"(:line \d+|Object\[\] \))$"))
                    .ToList();

                stackTrace.Insert(0, currMethod);
                LogEvent logEvent = new LogEvent(currMethod, stackTrace.ToArray());
                LogEvent currLog = level > 1 ? CurrentLog : CurrentRootLog?.GetFather(logEvent);

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
                logEvent.Start();

                var result = callback(logEvent);

                logEvent.Finish();
                CurrentLogs.PopOrDefault();

                if (index >= 0)
                {
                    RootLogs.RemoveAt(index);
                    LogAndNotify(logEvent);
                }

                logEvent.Returns = result;
                return result;
            }
        }

        /// <summary>
        /// Retrieves the names and values of the parameters of the advised method.
        /// </summary>
        /// <param name="context">The method advice context.</param>
        /// <returns>A dictionary containing parameter names and their values.</returns>
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

                    if (attr is LogRename logAttr)
                    {
                        paramName = logAttr.Name;
                    }
                }

                if (!ignoreAttribute)
                {
                    paramsNameValue.Add(paramName, context.Arguments[index]);
                }
            }

            return paramsNameValue;
        }

        /// <summary>
        /// Advises the advised method with logging and error handling.
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            lock (this)
            {
                string method = $"{context.TargetType.FullName}.{context.TargetMethod.Name}";

                LogEvent logEvent = new LogEvent(method, Environment.StackTrace);
                Dictionary<string, object> parameters = GetMethodParams(context);

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

                    logEvent.Start();
                    context.Proceed();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                if (context.HasReturnValue)
                {
                    logEvent.Returns = onErrorReturn ?? context.ReturnValue;
                }

                logEvent.Finish();
                CurrentLogs.PopOrDefault();

                if (index >= 0)
                {
                    RootLogs.RemoveAt(index);
                    LogAndNotify(logEvent);
                }
            }
        }
    }
}
