using System;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace pryLogger.src.Logger.ErrNotifiers.RestErrNotifier
{
    public class RestConnectionString
    {
        public readonly string ConnectionString;

        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Method { get; set; } = "POST";

        public static RestConnectionString FromConnectionString(string connectionString)
        {
            return new RestConnectionString(connectionString);
        }

        public RestConnectionString(string connectionString)
        {
            ConnectionString = connectionString = connectionString.Trim();
            Regex regex = new Regex("^(get|put|post|head|patch|delete|options)$", RegexOptions.IgnoreCase);
            var values = connectionString.ToDictionary() ?? throw new ArgumentException("invalid connectionString");

            Dictionary<string, Action<string>> keysAction = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["headers"] = k => Headers = values[k].ToDictionary(",", ":"),
                ["method"] = k =>
                {
                    values[k] = values[k].Trim();
                    Match match = regex.Match(values[k]);
                    Method = match.Success ? match.Value.ToUpper() : Method;
                },
            };

            Url = values["url"].Trim();
            foreach (string key in keysAction.Keys)
            {
                if (values.ContainsKey(key))
                {
                    keysAction[key](key);
                }
            }
        }
    }
}
