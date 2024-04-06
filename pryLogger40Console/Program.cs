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
            string mailConnectionString = "host=smtp.gmail.com; port=587; from=cartoryy@gmail.com; to=cartoryy@gmail.com; password=dnco vkuh djja jmdi; ssl=true";

            var mailErrNotifier = MailErrNotifier.FromConnectionString(mailConnectionString);
            var fileLogger = new FileLogger(fileConnectionString);

            LogAttribute.Instance
                .SetLoggers(ConsoleLogger.Instance, fileLogger)
                .SetErrorNotifiers(RestErrNotifier.FromConnectionString(restConnectionString));

            Console.WriteLine("BEGIN");
            Hello("pryLogger40");
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}
