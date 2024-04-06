using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Db
{
    public class DbEvent : Event
    {
        public override DateTimeOffset Starts { get; set; }
        public override double TotalMilliseconds { get; set; }

        [JsonProperty("queries", NullValueHandling = NullValueHandling.Ignore)]
        public List<DbQueryEvent> Queries { get; set; }

        public List<DbQueryEvent> GetQueries()
        {
            if (Queries == null) 
            {
                Queries = new List<DbQueryEvent>();
            }

            return Queries;
        }
    }

    public class DbQueryEvent : Event
    {
        public override DateTimeOffset Starts { get; set; }
        public override double TotalMilliseconds { get; set; }

        [JsonProperty("sql")]
        public string Sql { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }

        public DbQueryEvent(string sql) => this.Sql = sql;

        public DbQueryEvent(string sql, Dictionary<string, object> @params) : this(sql)
        {
            if (@params.Count > 0)
            {
                Params = @params;
            }
        }
    }
}
