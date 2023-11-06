using System;
using System.Text;
using System.Timers;
using System.Collections.Generic;

using LiteDB;

namespace pryLogger.src.Log.Strategies.Lite
{
    /// <summary>
    /// Provides settings for LiteDB log storage.
    /// </summary>
    public class LiteSettings
    {
        /// <summary>
        /// Gets or sets the timer used for deleting old log entries.
        /// </summary>
        public Timer Timer { get; private set; }

        /// <summary>
        /// Gets or sets the maximum count of log entries to keep.
        /// </summary>
        public int MaxCount { get; private set; } = 10 << 10;

        /// <summary>
        /// Gets or sets the connection string for LiteDB.
        /// </summary>
        public ConnectionString Connection { get; private set; }

        /// <summary>
        /// Sets the maximum count of log entries to keep.
        /// </summary>
        /// <param name="maxCount">The maximum count of log entries to keep.</param>
        /// <returns>The <see cref="LiteSettings"/> instance for method chaining.</returns>
        public LiteSettings SetMaxCount(int maxCount)
        {
            this.MaxCount = maxCount;
            return this;
        }

        /// <summary>
        /// Sets the connection string for LiteDB.
        /// </summary>
        /// <param name="connectionString">The connection string for LiteDB.</param>
        /// <returns>The <see cref="LiteSettings"/> instance for method chaining.</returns>
        public LiteSettings SetConnectionString(string connectionString)
        {
            this.Connection = new ConnectionString(connectionString);
            return this;
        }

        /// <summary>
        /// Sets the interval for deleting old log entries.
        /// </summary>
        /// <param name="interval">The time interval for log deletion.</param>
        public void SetDeleteLogsInterval(TimeSpan interval)
        {
            Timer?.Dispose();
            Timer = new Timer()
            {
                Enabled = true,
                AutoReset = true,
                Interval = interval.TotalMilliseconds
            };

            Timer.Elapsed += (_, __) =>
            {
                try
                {
                    using (LiteDatabase lite = new LiteDatabase(Connection))
                    {
                        var logs = lite.GetCollection<LogEvent>();

                        if (logs.Count() > MaxCount)
                        {
                            logs.DeleteAll();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"errorOnDeleteLogsInterval {e.Message}");
                }
            };

            Timer.Start();
        }
    }
}
