using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Db
{
    public static class DbLogEventExtensions
    {
        public static void AddDbEvent(this LogEvent log, DbEvent dbEvent) => log.GetEvents().Add(dbEvent);
        public static DbEvent GetLastDbEvent(this LogEvent log) => log.Events?.FindLast(e => e is DbEvent) as DbEvent;
    }

    public class DbEvent : IEvent
    {
        [JsonIgnore]
        public DateTimeOffset Starts { get; set; }

        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        [JsonProperty("queries", NullValueHandling = NullValueHandling.Ignore)]
        public List<DbQuery> Queries { get; set; }

        public void AddQuery(DbQuery query)
        {
            this.Queries = Queries ?? new List<DbQuery>();
            Queries.Add(query);
        }

        public void Start() => Starts = DateTimeOffset.Now;

        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }

    public class DbQuery : IEvent
    {
        [JsonIgnore]
        public DateTimeOffset Starts { get; set; }

        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        [JsonProperty("sql")]
        public string Sql { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }

        public DbQuery(string sql) => this.Sql = sql;
        public DbQuery(string sql, Dictionary<string, object> @params) 
        {
            this.Sql = sql;
            if (@params?.Count > 0)
            {
                this.Params = @params;
            }
        }

        public void Start() => Starts = DateTimeOffset.Now;

        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }
}
