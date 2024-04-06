using System;
using System.Net;

using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace pryLogger.src.Logger.ErrNotifiers
{
    public class ErrNotification
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonIgnore]
        public string JsonError { get; set; }

        [JsonProperty("repository")]
        public string Repository { get; set; }

        [JsonProperty("errLog")]
        public readonly LogEvent ErrLog;

        [JsonProperty("ipAddresses")]
        public string[] IpAdresses { get; set; }

        private ErrNotification(LogEvent errLog)
        {
            this.ErrLog = errLog;
            var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(ip => Regex.IsMatch(ip.ToString(), @"^\d+\.\d+\.\d+\.\d+$"))
                    .Select(ip =>
                    {
                        try
                        {
                            return $"{ip}, {Dns.GetHostEntry(ip).HostName}";
                        }
                        catch (Exception)
                        {
                            return ip.ToString();
                        }
                    })
                    .ToArray();

            IpAdresses = ipAddresses;
            JsonError = errLog.ToJson(Formatting.Indented);
            Title = $"Error Detected At {errLog.MethodName}";
            Message = $"At {Environment.CurrentDirectory}";
        }

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static ErrNotification FromLog(LogEvent errLog) => new ErrNotification(errLog);
        public static ErrNotification FromJson(string json) => JsonConvert.DeserializeObject<ErrNotification>(json);
    }
}