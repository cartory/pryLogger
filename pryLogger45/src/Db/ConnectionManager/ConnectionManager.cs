using System;
using System.Data;
using System.Linq;

using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pryLogger.src.Db.ConnectionManager
{
    public class ConnectionManager<Connection, ConnectionStringBuilder>
        where Connection : DbConnection
        where ConnectionStringBuilder : DbConnectionStringBuilder
    {
        protected readonly Queue<Guid> Tickets = new Queue<Guid>();
        protected readonly ObservableList<Connection> Connections = new ObservableList<Connection>();

        public ConnectionStringBuilder ConnectionString { get; protected set; }

        protected int MaxPoolSize
        {
            get
            {
                string[] keys = new string[] { "MaxPoolSize", "MaximumPoolSize" };
                var properties = ConnectionString?.GetType().GetProperties();

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        if (keys.Contains(property.Name))
                        {
                            int maxPoolSize = (int)property.GetValue(ConnectionString, null);
                            return maxPoolSize;
                        }
                    }
                }

                return 1;
            }
        }

        public ConnectionManager(string connectionString) => SetConnectionString(connectionString);
        public ConnectionManager(ConnectionStringBuilder connectionStringBuilder) => ConnectionString = connectionStringBuilder;

        public virtual void SetConnectionString(string connectionString)
        {
            this.ConnectionString = (ConnectionStringBuilder)Activator.CreateInstance(
                type: typeof(ConnectionStringBuilder),
                args: new object[] { connectionString }
            );
        }

        public virtual void Clear()
        {
            Tickets.Clear();
            Connections.RemoveAll(conn =>
            {
                conn.Dispose();
                return true;
            });
        }

        protected virtual void NotifyFreeConnection(DbConnection conn, int index)
        {
            conn.Dispose();
            Connections.RemoveAt(index);

            if (Tickets.Count > 0)
            {
                var ticket = Tickets.Dequeue();

                Connections.Notify(ticket);
                Connections.RemoveListener(ticket);
            }
        }

        protected virtual Connection GetFreeConnection(int index)
        {
            Connection conn = (Connection)Activator.CreateInstance(
                type: typeof(Connection),
                args: new object[] { this.ConnectionString.ConnectionString }
            );

            conn.StateChange += (sender, e) =>
            {
                if (e.CurrentState == ConnectionState.Closed)
                {
                    NotifyFreeConnection(conn, index);
                }
            };

            return conn;
        }

        public virtual Connection GetConnection()
        {
            lock (this)
            {
                var taskConn = this.GetConnectionAsync();

                try
                {
                    taskConn.Wait();
                    return taskConn.Result;
                }
                catch (Exception)
                {
                    taskConn.Dispose();
                    throw;
                }
            }
        }

        public virtual Task<Connection> GetConnectionAsync()
        {
            int index = -1;
            Connection conn;
            var promise = new TaskCompletionSource<Connection>();

            try
            {
                if (Connections.Count < MaxPoolSize)
                {
                    index = Connections.Count;
                    conn = GetFreeConnection(index);

                    Connections.Add(conn);
                    promise.SetResult(conn);
                }
                else
                {
                    var ticket = Guid.NewGuid();
                    Tickets.Enqueue(ticket);

                    Connections.AddListener(ticket, () =>
                    {
                        index = Connections.Count;
                        conn = GetFreeConnection(index);

                        Connections.Add(conn);
                        promise.SetResult(conn);
                    });
                }
            }
            catch (Exception e)
            {
                if (index > -1 && index < Connections.Count)
                {
                    conn = Connections[index];
                    NotifyFreeConnection(conn, index);
                }

                promise.SetException(e);
            }

            return promise.Task;
        }
    }
}