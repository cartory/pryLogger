using System;
using System.Linq;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ArxOne.MrAdvice.Advice;

using pryLogger.src.Logger.Loggers;
using pryLogger.src.Logger.ErrNotifiers;

namespace pryLogger.src.Logger.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogAttribute : Attribute, IMethodAdvice
    {
        public static readonly LogAttribute Instance = new LogAttribute();

        private static readonly List<LogEvent> RootLogs = new List<LogEvent>();
        private static readonly Stack<LogEvent> CurrLogs = new Stack<LogEvent>();

        public static LogEvent Current => CurrLogs.PeekOrDefault();
        private static LogEvent CurrentRoot => RootLogs.LastOrDefault();


        private static string[] LoggersFileNames;
        private static ILogger[] Loggers { get; set; }
        private static ErrNotifier[] ErrNotifiers { get; set; }

        public EventType? Events { get; private set; }

        public LogAttribute() : base() { }
        public LogAttribute(EventType events) : this() => Events = events;

        public LogAttribute SetLoggers(params ILogger[] loggers)
        {
            if (loggers.Length > 0)
            {
                Loggers = loggers;
                LoggersFileNames = loggers
                    .Select(l => l.FileNames)
                    .SelectMany(arr => arr)?.ToArray();
            }

            return this;
        }

        public LogAttribute SetErrorNotifiers(params ErrNotifier[] errorNotifiers)
        {
            ErrNotifiers = errorNotifiers.Length > 0 ? errorNotifiers : ErrNotifiers;
            return this;
        }

        private T UseLog<T>(Func<Tuple<LogEvent, LogEvent>> getTupleLog, Func<LogEvent, T> getReturnValue)
        {
            lock (this)
            {
                int index = -1;
                var tuple = getTupleLog();
                LogEvent log = tuple.Item1;
                LogEvent currLog = tuple.Item2;

                if (currLog != null)
                {
                    currLog.GetEvents().Add(log);
                }
                else
                {
                    RootLogs.Add(log);
                    index = RootLogs.Count - 1;
                }

                CurrLogs.Push(log);
                log.Start();

                var result = getReturnValue(log);
                log.Returns = result;

                log.Stop();
                CurrLogs.PopOrDefault();

                if (index >= 0)
                {
                    RootLogs.RemoveAt(index);

                    if (Loggers != null) 
                    {
                        Parallel.ForEach(Loggers, logger =>
                        {
                            logger.Log(log.FilterByEventType(Events ?? logger.Events));
                        });
                    }

                    if (log.HasError(out LogEvent errLog))
                    {
                        if (ErrNotifiers != null) 
                        { 
                            Parallel.ForEach(ErrNotifiers, errNotifier =>
                            {
                                errNotifier
                                    .GetByIntervalMinutes()?
                                    .SetFileNames(LoggersFileNames)
                                    .Notify(ErrNotification.FromLog(errLog));
                            });
                        }
                    }
                }

                return result;
            }
        }

        public void Advise(MethodAdviceContext context)
        {
            UseLog(() =>
            {
                Dictionary<string, object> parameters = context.GetParamsAsDictionary();
                string methodName = $"{context.TargetType.FullName}.{context.TargetMethod.Name}";

                LogEvent log = new LogEvent(methodName, Environment.StackTrace);
                LogEvent currLog = CurrentRoot?.GetFather(log);

                if (parameters.Count > 0)
                {
                    log.Params = parameters;
                }

                return Tuple.Create(log, currLog);
            }, (_) =>
            {
                context.Proceed();
                return context.HasReturnValue ? context.ReturnValue : null;
            });
        }

        public void UseLog(Action<LogEvent> action) 
        {
            UseLog(log =>
            {
                action(log);
                return 0;
            });
        }

        public T UseLog<T>(Func<LogEvent, T> getResult)
        {
            return UseLog(() =>
            {
                int level = 0;
                var stackTrace = Environment.StackTrace
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(stack =>
                    {
                        stack = stack.Substring(3);
                        if (Regex.IsMatch(stack.ToLower(), @"^(.+\.)+uselog\[\w+\]\(func`2", RegexOptions.IgnoreCase))
                        {
                            level++;
                        }

                        return stack;
                    })
                    .Where(st => !Regex.IsMatch(st, @"^(.+\.)+<>.*:line \d+$", RegexOptions.IgnoreCase))
                    .Where(st => !Regex.IsMatch(st, @"^(system\.environment|arxone|prylogger)\.", RegexOptions.IgnoreCase))
                    .ToList();

                string currMethod = stackTrace.FirstOrDefault() ?? "void";
                currMethod = Regex.Replace(currMethod, @"′?\(.*\) in .*:line \d+$", string.Empty, RegexOptions.IgnoreCase) + $"[{level}]";

                stackTrace = stackTrace
                    .Where(st => !Regex.IsMatch(st, @"(:line \d+|Object\[\] \))$", RegexOptions.IgnoreCase))
                    .ToList();

                stackTrace.Insert(0, currMethod);
                LogEvent log = new LogEvent(currMethod, stackTrace.ToArray());
                LogEvent currLog = level > 1 ? Current : CurrentRoot?.GetFather(log);

                return Tuple.Create(log, currLog);
            }, getResult);
        }
    }
}
