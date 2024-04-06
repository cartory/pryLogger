using System;
using System.Linq;
using System.Text;

namespace pryLogger.src.Logger.ErrNotifiers
{
    public abstract class ErrNotifier
    {
        public int IntervalMinutes { get; set; } = 1;
        public string[] FileNames { get; set; } = new string[] { };
        protected DateTimeOffset LastErrorTime { get; set; } = DateTimeOffset.MinValue;

        public abstract void Notify(ErrNotification err, bool throwException = false);

        public virtual ErrNotifier SetFileNames(params string[] filenames)
        {
            this.FileNames = filenames;
            return this;
        }

        public virtual ErrNotifier SetIntervalMinutes(int intervalMinutes)
        {
            this.IntervalMinutes = intervalMinutes;
            return this;
        }

        public virtual ErrNotifier GetByIntervalMinutes()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            TimeSpan diff = now - LastErrorTime;

            if (diff.TotalMinutes < IntervalMinutes)
            {
                return null;
            }

            LastErrorTime = now;
            return this;
        }
    }
}
