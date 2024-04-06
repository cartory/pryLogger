using System;
using System.IO;
using System.Linq;

using pryLogger.src.Rest;
using pryLogger.src.Logger.Attributes;

using Newtonsoft.Json;

namespace pryLogger.src.Logger.ErrNotifiers.RestErrNotifier
{
    public class RestErrNotifier : ErrNotifier
    {
        public RestConnectionString RestConnectionString { get; private set; }

        public static RestErrNotifier FromConnectionString(string connectionString)
        {
            return new RestErrNotifier(connectionString);
        }

        public RestErrNotifier(string connectionString)
        {
            RestConnectionString = RestConnectionString.FromConnectionString(connectionString);
        }

        public override void Notify(ErrNotification err, bool throwException = false)
        {
            string messageTo = $"{RestConnectionString.Method} {RestConnectionString.Url}";

            try
            {
                var json = new
                {
                    err,
                    files = FileNames?
                    .Where(path => File.Exists(path))
                    .GroupBy(Path.GetFileName)
                    .ToDictionary(g => g.Key, g =>
                    {
                        byte[] fileBytes = File.ReadAllBytes(g.Last());
                        return Convert.ToBase64String(fileBytes);
                    })
                };

                var currentLog = LogAttribute.Current;
                int eventsCount = currentLog?.Events?.Count ?? 0;

                RestClient.Fetch(new RestRequest(RestConnectionString.Url)
                { 
                    ContentType = "application/json",
                    Method = RestConnectionString.Method,
                    Content = JsonConvert.SerializeObject(json),
                    Headers = RestConnectionString.Headers?.ToDictionary(k => k.Key, k => (object)k.Value),
                }, res => Console.WriteLine($"{nameof(RestErrNotifier)} {messageTo} OK"));

                currentLog?.Events?.RemoveAt(eventsCount - 1);

                if (currentLog?.Events?.Count < 1)
                {
                    currentLog.Events = null;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{nameof(RestErrNotifier)} {messageTo} ERROR {e.Message}");
                if (throwException) throw;
            }
        }
    }
}
