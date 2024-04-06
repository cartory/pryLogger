using System;
using System.Data;

using System.Data.Common;
using pryLogger.src.Logger.Attributes;

namespace pryLogger.src.Db
{
    public static class DbLogExtensions
    {
        public static void Log(this DbCommand command, Action action)
        {
            command.Log(() =>
            {
                action();
                return 0;
            });
        }

        public static T Log<T>(this DbCommand command, Func<T> func)
        {
            T result;
            var query = new DbQueryEvent(command.CommandText, command.Parameters.ToDictionary());
            LogAttribute.Current?.GetLastDbEvent()?.GetQueries().Add(query);

            query.Start();
            result = func();
            query.Stop();

            return result;
        }

        public static int LogExecuteNonQuery(this DbCommand command) => command.Log(command.ExecuteNonQuery);

        public static object LogExecuteScalar(this DbCommand command) => command.Log(command.ExecuteScalar);

        public static DbDataReader LogExecuteReader(this DbCommand command) => command.Log(command.ExecuteReader);

        public static DbDataReader LogExecuteReader(this DbCommand command, CommandBehavior commandBehavior)
        {
            return command.Log(() => command.ExecuteReader(commandBehavior));
        }
    }
}
