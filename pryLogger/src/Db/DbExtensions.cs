using System;
using System.Data;
using System.Linq;

using System.Data.Common;
using System.Collections.Generic;

using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Db
{
    public static partial class DbExtensions
    {
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
                LogAttribute.CurrentLog?.AddDbEvent(dbEvent);

                using (conn)
                {
                    dbEvent.Start();
                    conn.Open();

                    using (DbCommand command = conn.CreateCommand())
                    {
                        result = func(command);
                    }

                    conn.Close();
                    dbEvent.Finish();
                }

                return result;
            }
            catch (Exception e)
            {
                dbEvent.Finish();
                Console.WriteLine($"errorOnQuery<{typeof(T).Name}> : {e.Message}");
                throw;
            }
        }

        public static DataTable SelectQuery(this DbCommand command, string sql)
        {
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            DataTable dataTable = new DataTable(Guid.NewGuid().ToString());

            using (var reader = command.LogExecuteReader())
            {
                dataTable.Load(reader);
            }

            return dataTable;
        }

        public static DataTable[] SelectQueries(this DbCommand command, params string[] sqls)
        {
            DataTable[] dataTables = new DataTable[sqls.Length];

            for (int index = 0; index < sqls.Length; index++)
            {
                dataTables[index] = command.SelectQuery(sqls[index]);
            }

            return dataTables;
        }

        public static DataTable SelectQuery(this DbConnection conn, string sql)
        {
            return conn.SelectQueries(sql).FirstOrDefault();
        }

        public static DataTable[] SelectQueries(this DbConnection conn, params string[] sqls)
        {
            return conn.Query(command => command.SelectQueries(sqls));
        }
    }

    public static partial class DbExtensions
    {
        public static Dictionary<string, object> ToDictionary(this DbParameterCollection @params)
        {
            var keys = new Dictionary<string, object>();

            if (@params != null) 
            { 
                foreach (DbParameter param in @params)
                {
                    keys.Add(param.ParameterName, param.Value);
                }
            }

            return keys;
        }

        public static int LogExecuteNonQuery(this DbCommand command)
        {

            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            int affectedRows = command.ExecuteNonQuery();
            query.Finish();

            return affectedRows;
        }

        public static object LogExecuteScalar(this DbCommand command)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            object result = command.ExecuteScalar();
            query.Finish();

            return result;
        }

        public static DbDataReader LogExecuteReader(this DbCommand command)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            var result = command.ExecuteReader();
            query.Finish();

            return result;
        }

        public static DbDataReader LogExecuteReader(this DbCommand command, CommandBehavior behavior)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            var result = command.ExecuteReader(behavior);
            query.Finish();

            return result;
        }
    }

    public static partial class DbExtensions
    {
        public static Dictionary<string, object>[] ToDictionary(this DataTable dataTable)
        {
            var items = new Dictionary<string, object>[dataTable.Rows.Count];

            for (int i = 0; i < items.Length; i++)
            {
                DataRow row = dataTable.Rows[i];
                var item = new Dictionary<string, object>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column.ColumnName];
                    item.Add(column.ColumnName, DBNull.Value.Equals(value) ? null : value);
                }

                items[i] = item;
            }

            return items;
        }

        public static T[] CastTo<T>(this DataTable dataTable) where T : new()
        {
            var items = new T[dataTable.Rows.Count];
            var properties = typeof(T).GetType().GetProperties();

            for (int i = 0; i < items.Length; i++)
            {
                T item = new T();
                DataRow row = dataTable.Rows[i];

                foreach (var property in properties)
                {
                    if (!dataTable.Columns.Contains(property.Name))
                    {
                        continue;
                    }

                    var value = row[property.Name];
                    var propertyValue = DBNull.Value.Equals(value) ? null : Convert.ChangeType(value, property.PropertyType);

                    property.SetValue(item, propertyValue, null);
                }

                items[i] = item;
            }

            return items;
        }
    }
}
