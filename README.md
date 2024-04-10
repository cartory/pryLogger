# pryLogger

This sample project demonstrates the usage of the `pryLogger` library for event logging and error handling in a C# application. The application makes REST API calls and configures email notifications in case of errors.

## Requirements

- **Connection Database**: Ensure you have an database set up with a valid connection string, in this case is with Oracle

## Usage Example

```
using System;
using System.Linq;
using System.Text;

using pryLogger.src.Logger.Loggers;
using pryLogger.src.Logger.Loggers.FileLogger;

using pryLogger.src.Logger.Attributes;

using pryLogger.src.Logger.ErrNotifiers.MailErrNotifier;
using pryLogger.src.Logger.ErrNotifiers.RestErrNotifier;

namespace pryLoggerConsole45
{
    class Program
    {
        [Log]
        static string Hello([LogParam("personName")] string name)
        {
            try
            {
                throw new Exception("test exception");
            }
            catch (Exception e)
            {
                LogAttribute.Current?.SetException(e);
            }

            return $"Hello, {name}";
        }

        static void Main(string[] args)
        {
            string fileConnectionString = "filename=log.txt; maxlines=1000";
            string restConnectionString = "url=http://localhost:3000; method=post;";
            string mailConnectionString = "host=smtp.gmail.com; port=587; from=cartoryy@gmail.com; to=cartoryy@gmail.com; password=...; ssl=true";

            var mailErrNotifier = MailErrNotifier.FromConnectionString(mailConnectionString);
            var fileLogger = new FileLogger(fileConnectionString);

            LogAttribute.Instance
                .SetLoggers(ConsoleLogger.Instance, fileLogger)
                .SetErrorNotifiers(mailErrNotifier, RestErrNotifier.FromConnectionString(restConnectionString));

            Console.WriteLine("BEGIN");
            Hello("pryLogger40");
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}
