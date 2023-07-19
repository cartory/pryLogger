using System;
using System.Text;

using System.Threading.Tasks;
using System.Collections.Generic;

using Oracle.ManagedDataAccess.Client;
using pryLogger.src.ErrorNotifier.MailNotifier;

using pryLogger.src.Db;
using pryLogger.src.Rest;

using pryLogger.src.Rest.Xml;
using pryLogger.src.Log.Attributes;

using pryLogger.src.Log.Strategies;
using pryLogger.src.Db.ConnectionManager;

namespace ConsoleApp461
{
    internal class Program
    {
        public static void onException(Exception ex) 
        {
            Console.WriteLine($"catchException : {ex.Message}");
        }

        [ConsoleLog(nameof(onException))]
        static void level1([LogRename("customParam")]string test = "lala land") 
        {
            level2();
            throw new Exception();
            //var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("SELECT * FROM ganadero.USUARIOS u FETCH FIRST 10 ROWS ONLY");
        }

        [ConsoleLog(nameof(onException))]
        static string level2() 
        {
            string url = "http://172.16.1.211:8080/topazinterpretedws/tokenbuilder";
            var Headers = new Dictionary<string, object>()
            {
                { "Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"TOP1:SYSTEMS1"))}"}
            };

            var res = RestClient.Fetch(new RestRequest(url) 
            {
                Headers = Headers
            }, rest => rest.Content?.ToXml());

            return res;
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
