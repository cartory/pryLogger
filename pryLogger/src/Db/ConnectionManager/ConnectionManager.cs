using System;
using System.Data;
using System.Linq;

using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pryLogger.src.Db.ConnectionManager
{
    /// <summary>
    /// Generic connection manager that manages a pool of database connections.
    /// </summary>
    /// <typeparam name="Connection">Type of connection to be managed, must be derived from DbConnection.</typeparam>
    /// <typeparam name="ConnectionStringBuilder">Type of connection string builder to be used, must be derived from DbConnectionStringBuilder.</typeparam>
    public class ConnectionManager<Connection, ConnectionStringBuilder>
        where Connection : DbConnection, new()
        where ConnectionStringBuilder : DbConnectionStringBuilder, new()
    {
        /// <summary>
        /// Singleton instance of the connection manager.
        /// </summary>
        public static ConnectionManager<Connection, ConnectionStringBuilder> Instance = new ConnectionManager<Connection, ConnectionStringBuilder>();

        protected readonly Queue<Guid> Tickets = new Queue<Guid>();
        protected readonly ObservableList<Connection> Connections = new ObservableList<Connection>();

        /// <summary>
        /// Gets or sets the connection string builder used by the connection manager.
        /// </summary>
        public ConnectionStringBuilder ConnectionString { get; protected set; }

        /// <summary>
        /// Gets the maximum size of the connection pool.
        /// </summary>
        protected int MaxPoolSize
        {
            get
            {
                string[] keys = new string[] { "MaxPoolSize", "MaximumPoolSize" };
                var properties = ConnectionString?.GetType().GetProperties();

                foreach (var property in properties)
                {
                    if (keys.Contains(property.Name))
                    {
                        int maxPoolSize = (int)property.GetValue(ConnectionString);
                        return maxPoolSize;
                    }
                }

                return 1;
            }
        }

        private ConnectionManager() { }

        /// <summary>
        /// Initializes a new instance of the connection manager with a connection string builder.
        /// </summary>
        /// <param name="connectionStringBuilder">The connection string builder to be used.</param>
        public ConnectionManager(ConnectionStringBuilder connectionStringBuilder)
        {
            this.ConnectionString = connectionStringBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the connection manager with a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string used to create the connection string builder.</param>
        public ConnectionManager(string connectionString)
        {
            this.ConnectionString = (ConnectionStringBuilder)Activator.CreateInstance(typeof(ConnectionStringBuilder), new object[] { connectionString });
        }

        /// <summary>
        /// Sets a new connection string for the connection manager.
        /// </summary>
        /// <param name="connectionString">The new connection string.</param>
        public virtual void SetConnectionString(string connectionString)
        {
            this.ConnectionString = (ConnectionStringBuilder)Activator.CreateInstance(typeof(ConnectionStringBuilder), new object[] { connectionString });
        }

        /// <summary>
        /// Clears all connections and tickets from the connection pool.
        /// </summary>
        public virtual void Clear()
        {
            Tickets.Clear();
            Connections.RemoveAll(conn =>
            {
                conn.Dispose();
                return true;
            });
        }

        /// <summary>
        /// Notifies when a connection's state changes to closed and frees it.
        /// </summary>
        /// <param name="conn">The connection to be freed.</param>
        /// <param name="index">The index of the connection in the pool.</param>
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

        /// <summary>
        /// Gets a free connection from the pool.
        /// </summary>
        /// <param name="index">The index of the connection in the pool.</param>
        /// <returns>The obtained free connection.</returns>
        protected virtual Connection GetFreeConnection(int index)
        {
            Connection conn = Activator.CreateInstance(typeof(Connection), this.ConnectionString.ConnectionString) as Connection;

            conn.StateChange += (sender, e) =>
            {
                if (e.CurrentState == ConnectionState.Closed)
                {
                    NotifyFreeConnection(conn, index);
                }
            };

            return conn;
        }

        /// <summary>
        /// Gets a connection from the connection pool synchronously.
        /// </summary>
        /// <returns>The obtained connection.</returns>
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

        /// <summary>
        /// Gets a connection from the connection pool asynchronously.
        /// </summary>
        /// <returns>A task representing the obtained connection.</returns>
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
