using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Db
{
    /// <summary>
    /// Extension methods for working with database-related log events.
    /// </summary>
    public static class DbLogEventExtensions
    {
        /// <summary>
        /// Adds a database event to the log's list of events.
        /// </summary>
        /// <param name="log">The log event to which the database event will be added.</param>
        /// <param name="dbEvent">The database event to add.</param>
        public static void AddDbEvent(this LogEvent log, DbEvent dbEvent) => log.GetEvents().Add(dbEvent);

        /// <summary>
        /// Gets the last database event from the log's list of events.
        /// </summary>
        /// <param name="log">The log event from which to retrieve the last database event.</param>
        /// <returns>The last database event or null if none exists.</returns>
        public static DbEvent GetLastDbEvent(this LogEvent log) => log.Events?.FindLast(e => e is DbEvent) as DbEvent;
    }

    /// <summary>
    /// Represents a database-related event with information about elapsed time and executed queries.
    /// </summary>
    public class DbEvent : IEvent
    {
        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        [JsonIgnore]
        public DateTimeOffset Starts { get; set; }

        [JsonProperty("queries", NullValueHandling = NullValueHandling.Ignore)]
        public List<DbQuery> Queries { get; set; }

        /// <summary>
        /// Adds a database query to the list of queries within this database event.
        /// </summary>
        /// <param name="query">The database query to add.</param>
        public void AddQuery(DbQuery query)
        {
            this.Queries = Queries ?? new List<DbQuery>();
            Queries.Add(query);
        }

        /// <summary>
        /// Starts recording the timestamp when this database event begins.
        /// </summary>
        public void Start() => Starts = DateTimeOffset.Now;

        /// <summary>
        /// Finishes recording the elapsed time for this database event.
        /// </summary>
        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }

    /// <summary>
    /// Represents a database query with information about elapsed time, SQL statement, and parameters.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the DbQuery class with a SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement for the query.</param>
        public DbQuery(string sql) => this.Sql = sql;

        /// <summary>
        /// Initializes a new instance of the DbQuery class with a SQL statement and parameters.
        /// </summary>
        /// <param name="sql">The SQL statement for the query.</param>
        /// <param name="params">The parameters for the query.</param>
        public DbQuery(string sql, Dictionary<string, object> @params)
        {
            this.Sql = sql;
            if (@params?.Count > 0)
            {
                this.Params = @params;
            }
        }

        /// <summary>
        /// Starts recording the timestamp when this database query begins.
        /// </summary>
        public void Start() => Starts = DateTimeOffset.Now;

        /// <summary>
        /// Finishes recording the elapsed time for this database query.
        /// </summary>
        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }
}
