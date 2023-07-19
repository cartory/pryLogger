using System;
using System.Data;

using System.Linq;
using System.Data.Common;

using System.Threading.Tasks;
using System.Collections.Generic;

namespace pryLogger.src.Db.ConnectionManager
{
    public class ConnectionManager 
    {
        protected readonly Queue<Guid> Tickets = new Queue<Guid>();
        protected readonly ObservableList<DbConnection> Connections = new ObservableList<DbConnection>();

        public readonly static ConnectionManager Instance = new ConnectionManager();

        public DbConnectionStringBuilder ConnectionStringBuilder { get; protected set; }
        protected int MaxPoolSize
        {
            get 
            {
                string[] keys = new string[] { "MaxPoolSize", "MaximumPoolSize" };
                var properties = ConnectionStringBuilder?.GetType().GetProperties();

                foreach (var property in properties)
                {
                    if (keys.Contains(property.Name)) 
                    {
                        int maxPoolSize = (int)property.GetValue(ConnectionStringBuilder);
                        return maxPoolSize;
                    }
                }

                return 1;
            }
        }

        public ConnectionManager() { }
        public ConnectionManager(DbConnectionStringBuilder connectionStringBuilder)
        {
            this.ConnectionStringBuilder = connectionStringBuilder;
        }

        public void SetConnectionString(DbConnectionStringBuilder connectionStringBuilder)
        {
            this.ConnectionStringBuilder = connectionStringBuilder;
        }

        public void SetConnectionString<ConnStringBuilder>(string connectionString) where ConnStringBuilder : DbConnectionStringBuilder
        {
            this.ConnectionStringBuilder = Activator.CreateInstance(typeof(ConnStringBuilder), connectionString) as ConnStringBuilder;
        }

        public void Clear()
        {
            Tickets.Clear();
            Connections.RemoveAll(conn =>
            {
                conn.Dispose();
                return true;
            });
        }

        protected void NotifyFreeConnection(DbConnection conn, int index)
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

        protected Conn GetFreeConnection<Conn>(int index) where Conn : DbConnection
        {
            Conn conn = Activator.CreateInstance(typeof(Conn), ConnectionStringBuilder.ConnectionString) as Conn;

            conn.StateChange += (sender, e) =>
            {
                if (e.CurrentState == ConnectionState.Closed)
                {
                    NotifyFreeConnection(conn, index);
                }
            };

            return conn;
        }

        public Conn GetConnection<Conn>() where Conn : DbConnection
        {
            lock (this)
            {
                var taskConn = this.GetConnectionAsync<Conn>();

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

        public Task<Conn> GetConnectionAsync<Conn>() where Conn : DbConnection
        {
            Conn conn;
            int index = -1;
            var promise = new TaskCompletionSource<Conn>();

            try
            {
                if (Connections.Count < MaxPoolSize)
                {
                    index = Connections.Count;
                    conn = GetFreeConnection<Conn>(index);

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
                        conn = GetFreeConnection<Conn>(index);

                        Connections.Add(conn);
                        promise.SetResult(conn);
                    });
                }
            }
            catch (Exception e)
            {
                if (index > -1 && index < Connections.Count) 
                {
                    conn = Connections[index] as Conn;
                    NotifyFreeConnection(conn, index);
                }

                promise.SetException(e);
            }

            return promise.Task;
        }
    }
}
