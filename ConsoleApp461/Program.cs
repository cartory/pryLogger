using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using pryLogger.src.LogStrategies.File;
using pryLogger.src.ErrorNotifier.MailNotifier;
using Oracle.ManagedDataAccess.Client;

using pryLogger.src.Db;
using pryLogger.src.Db.ConnectionManager;
using pryLogger.src.Log.LogStrategies;

namespace ConsoleApp461
{
    internal class Program
    {
        public static void onException(Exception ex) 
        {
            Console.WriteLine($"catchException : {ex.Message}");
        }

        [ConsoleLog]
        static void level1() 
        {
            var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("SELECT * FROM ganadero.USUARIOS u FETCH FIRST 10 ROWS ONLY");
            Console.WriteLine("level1a");
            //level2();
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
            string connectionString = "DATA SOURCE=172.16.1.20/BGDB; PASSWORD=!7Kcht2!; USER ID=GANADERO";
            MailErrorNotifier mailErrorNotifier = new MailErrorNotifier("host=nomail.bg.com.bo; port=25; from=cari@bg.com.bo; to=plcaricari@bg.com.bo; repo=https://gitlab.bg.com.bo/desarrollo/bga/servicios-windows/servicioconciliaciontcytpp");
            ConnectionManager.Instance.SetConnectionString<OracleConnectionStringBuilder>(connectionString);

            ConsoleLog.SetErrorNotifier(mailErrorNotifier);

            //FileLog.SetConnectionString(@"maxFiles=10");

            level1();
            Console.ReadKey();
        }
    }
}
