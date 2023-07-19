using System;
using System.Text;
using System.Timers;
using System.Collections.Generic;

using LiteDB;

namespace pryLogger.src.Log.LogStrategies.LiteDb
{
    public class LiteSettings
    {
        public Timer Timer { get; private set; }
        public int MaxCount { get; private set; } = 10 << 10;
        public ConnectionString Connection { get; private set; }

        public LiteSettings SetMaxCount(int maxCount) 
        {
            this.MaxCount = maxCount;
            return this;
        }

        public LiteSettings SetConnectionString(string connectionString) 
        {
            this.Connection = new ConnectionString(connectionString);
            return this;
        }

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
