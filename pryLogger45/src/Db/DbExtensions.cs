using System;
using System.Data.Common;
using System.Collections.Generic;

using pryLogger.src.Logger;
using pryLogger.src.Logger.Attributes;

namespace pryLogger.src.Db
{
    public static partial class DbExtensions
    {
        public static DbEvent GetLastDbEvent(this LogEvent log)
        {
            for (int i = 0; i < log.Events?.Count; i++)
            {
                var evt = log.Events[log.Events.Count - i - 1];

                if (evt is DbEvent lastDbEvent)
                {
                    return lastDbEvent;
                }
            }

            return null;
        }

        public static Dictionary<string, object> ToDictionary(this DbParameterCollection @params)
        {
            var keys = new Dictionary<string, object>();

            foreach (DbParameter param in @params)
            {
                keys.Add(param.ParameterName, param.Value);
            }

            return keys;
        }

        public static void Query(this DbConnection conn, Action<DbCommand> action)
        {
            conn.Query(command =>
            {
                action(command);
                return 0;
            });
        }

        public static T Query<T>(this DbConnection conn, Func<DbCommand, T> func)
        {
            var dbEvent = new DbEvent();

            try
            {
                T result;
                LogAttribute.Current?.GetEvents().Add(dbEvent);

                using (conn)
                {
                    dbEvent.Start();
                    conn.Open();

                    using (DbCommand command = conn.CreateCommand())
                    {
                        result = func(command);
                    }

                    conn.Close();
                    dbEvent.Stop();
                }

                return result;
            }
            catch (Exception)
            {
                dbEvent.Stop();
                throw;
            }
        }
    }
}
