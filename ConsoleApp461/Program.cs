using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using pryLogger.src.LogStrategies;
using pryLogger.src.LogStrategies.File;
using pryLogger.src.ErrorNotifier.MailNotifier;

namespace ConsoleApp461
{
    internal class Program
    {
        public static void onException(Exception ex) 
        {
            Console.WriteLine($"catchException : {ex.Message}");
        }

        [FileLog]
        static void level1() 
        {
            Console.WriteLine("level1a");
            level2();
        }

        [FileLog(nameof(onException))]
        static void level2() 
        {
            Console.WriteLine("level2");
            throw new Exception("test exception");
            //FileLog.New.OnAdvice("level2b", log =>
            //{
            //    try
            //    {
            //    }
            //    catch (Exception e)
            //    {
            //        log.SetException(e);
            //        Console.WriteLine("level2b");
            //    }
            //});
        }

        static void Main(string[] args)
        {
            MailErrorNotifier mailErrorNotifier = new MailErrorNotifier("host=nomail.bg.com.bo; port=25; from=cari@bg.com.bo; to=plcaricari@bg.com.bo; repo=https://gitlab.bg.com.bo/desarrollo/bga/servicios-windows/servicioconciliaciontcytpp");

            FileLog.SetConnectionString(@"maxFiles=10");
            FileLog.SetErrorNotifier(mailErrorNotifier);

            level1();
            Console.ReadKey();
        }
    }
}
