using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using pryLogger.src.ErrorNotifier.MailNotifier;
using Oracle.ManagedDataAccess.Client;

using pryLogger.src.Db;
using pryLogger.src.Db.ConnectionManager;
using pryLogger.src.Log.LogStrategies;
using pryLogger.src.Rest;
using pryLogger.src.Rest.RestXml;

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
            level2();
            var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("SELECT * FROM ganadero.USUARIOS u FETCH FIRST 10 ROWS ONLY");
        }

        [ConsoleLog(nameof(onException))]
        static void level2() 
        {
            string url = "http://172.16.1.211:8080/topazinterpretedws/tokenbuilder";
            var Headers = new Dictionary<string, object>()
            {
                { "Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"TOP1:SYSTEMS1s"))}"}
            };

            var res = RestXmlAdapter.Fetch(new RestRequest(url) 
            {
                Headers = Headers
            });
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
