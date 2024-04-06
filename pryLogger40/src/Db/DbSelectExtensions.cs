using System;
using System.Data;

using System.Data.Common;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace pryLogger.src.Db
{
    public static class DbSelectExtensions
    {
        public static DataTable SelectQuery(this DbCommand command, string sql, params DbParameter[] parameters)
        {
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            DataTable dataTable = new DataTable(Guid.NewGuid().ToString());

            using (var reader = command.LogExecuteReader())
            {
                dataTable.Load(reader);
            }

            return dataTable;
        }

        public static DataTable SelectQuery(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            return conn.Query(command => command.SelectQuery(sql, parameters));
        }

        public static DataTable[] SelectQueries(this DbCommand command, params string[] sqls)
        {
            var datatables = new DataTable[sqls.Length];

            for (int i = 0; i < sqls.Length; i++)
            {
                datatables[i] = command.SelectQuery(sqls[i]);
            }

            return datatables;
        }

        public static DataTable[] SelectQueries(this DbConnection conn, params string[] sqls)
        {
            return conn.Query(command => command.SelectQueries(sqls));
        }

        public static Dictionary<string, object>[] ToDictionary(this DataTable dataTable)
        {
            return dataTable.ToObject<Dictionary<string, object>>();
        }

        public static T[] ToObject<T>(this DataTable dataTable) where T : class
        {
            var items = new T[dataTable.Rows.Count];

            for (int i = 0; i < items.Length; i++)
            {
                DataRow row = dataTable.Rows[i];
                Dictionary<string, object> item = new Dictionary<string, object>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column.ColumnName];
                    item.Add(column.ColumnName, DBNull.Value.Equals(value) ? null : value);
                }

                items[i] = JObject.FromObject(item).ToObject<T>();
            }

            return items;
        }
    }
}
