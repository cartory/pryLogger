using System;
using System.Data;
using System.Linq;

using System.Data.Common;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Db
{
    /// <summary>
    /// A static class containing database-related extension methods.
    /// </summary>
    public static partial class DbExtensions
    {
        /// <summary>
        /// Executes a database query with a specified action on the database connection.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="action">The action to execute on the database command.</param>
        public static void Query(this DbConnection conn, Action<DbCommand> action)
        {
            conn.Query(command =>
            {
                action(command);
                return 0;
            });
        }

        /// <summary>
        /// Executes a database query with a specified function on the database connection and returns a result.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="func">The function to execute on the database command.</param>
        /// <returns>The result of the database query.</returns>
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

        /// <summary>
        /// Executes a SELECT query on the database and returns the result as a DataTable.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>The result of the query as a DataTable.</returns>
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

        /// <summary>
        /// Executes multiple SELECT SQL queries and returns the results as an array of DataTables.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <param name="sqls">An array of SQL queries to execute.</param>
        /// <returns>An array of DataTables containing the results of the queries.</returns>
        public static DataTable[] SelectQueries(this DbCommand command, params string[] sqls)
        {
            DataTable[] dataTables = new DataTable[sqls.Length];

            for (int index = 0; index < sqls.Length; index++)
            {
                dataTables[index] = command.SelectQuery(sqls[index]);
            }

            return dataTables;
        }

        /// <summary>
        /// Executes a SELECT query on the database and returns the result as a DataTable.
        /// </summary>
        /// <param name="conn">The database connection to use.</param>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>The result of the query as a DataTable.</returns>
        public static DataTable SelectQuery(this DbConnection conn, string sql)
        {
            return conn.SelectQueries(sql).FirstOrDefault();
        }

        /// <summary>
        /// Executes multiple SELECT SQL queries and returns the results as an array of DataTables.
        /// </summary>
        /// <param name="conn">The database connection to use.</param>
        /// <param name="sqls">An array of SQL queries to execute.</param>
        /// <returns>An array of DataTables containing the results of the queries.</returns>
        public static DataTable[] SelectQueries(this DbConnection conn, params string[] sqls)
        {
            return conn.Query(command => command.SelectQueries(sqls));
        }

        /// <summary>
        /// Converts a collection of DbParameter objects into a dictionary of name-value pairs.
        /// </summary>
        /// <param name="params">The collection of DbParameter objects.</param>
        /// <returns>A dictionary of name-value pairs representing the parameters.</returns>
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

        /// <summary>
        /// Executes a database command and logs the query in the event log.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <returns>The number of rows affected by the query.</returns>
        public static int LogExecuteNonQuery(this DbCommand command)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            int affectedRows = command.ExecuteNonQuery();
            query.Finish();

            return affectedRows;
        }

        /// <summary>
        /// Executes a database command and logs the query in the event log, returning a scalar value.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <returns>The scalar value returned by the query.</returns>
        public static object LogExecuteScalar(this DbCommand command)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            object result = command.ExecuteScalar();
            query.Finish();

            return result;
        }

        /// <summary>
        /// Executes a database command and logs the query in the event log, returning a DbDataReader.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <returns>A DbDataReader containing the query results.</returns>
        public static DbDataReader LogExecuteReader(this DbCommand command)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            var result = command.ExecuteReader();
            query.Finish();

            return result;
        }

        /// <summary>
        /// Executes a database command and logs the query in the event log with the specified behavior, returning a DbDataReader.
        /// </summary>
        /// <param name="command">The database command to execute.</param>
        /// <param name="behavior">The CommandBehavior to use when executing the query.</param>
        /// <returns>A DbDataReader containing the query results.</returns>
        public static DbDataReader LogExecuteReader(this DbCommand command, CommandBehavior behavior)
        {
            var query = new DbQuery(command.CommandText, command.Parameters?.ToDictionary());
            LogAttribute.CurrentLog?.GetLastDbEvent()?.AddQuery(query);

            query.Start();
            var result = command.ExecuteReader(behavior);
            query.Finish();

            return result;
        }

        /// <summary>
        /// Converts a DataTable into an array of objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of objects to create.</typeparam>
        /// <param name="datatable">The DataTable to convert.</param>
        /// <returns>An array of objects of type T.</returns>
        public static T[] ToObject<T>(this DataTable datatable) where T : new()
        {
            return datatable
                .ToDictionary()
                .Select(x => JObject.FromObject(x).ToObject<T>())
                .ToArray();
        }

        /// <summary>
        /// Converts a DataTable into an array of dictionaries containing column name-value pairs.
        /// </summary>
        /// <param name="dataTable">The DataTable to convert.</param>
        /// <returns>An array of dictionaries representing the rows of the DataTable.</returns>
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
    }
}
