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
using ArxOne.MrAdvice.Introduction;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Xml;

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
            //var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("SELECT * FROM ganadero.USUARIOS u FETCH FIRST 10 ROWS ONLY");
            throw new Exception();
        }

        [ConsoleLog(nameof(onException))]
        static void level2() 
        {
            string url = "http://172.16.1.13:8083/WSTCPP/SrvTc.asmx";
            var body = new
            {
                pStrCodigoCms = 123,
                pStrFechaExpiracion = 123,
                pStrPinBlock = 123,
                pStrUsuario = "TOP1",
                pStrCodCajero = 701
            };

            var res = RestClient.Fetch<object>(new RestRequest(url) 
            {
                Method = "POST",
                Content = Soap.CreateXmlRequest("{http://servicio.ws.bga.bo/WSTCPP/}FUN_SeteoPin", null, body, Declarations.UTF8),
                ContentType = "text/xml; charset=UTF-8",
            }, rest =>
            {
                object result  = Soap.FromXmlResponse("{http://servicio.ws.bga.bo/WSTCPP/}FUN_SeteoPinResponse", rest.Content);
                return result;
            });
        }

        static void Main(string[] args)
        {
            string connectionString = "database connection string";
            MailErrorNotifier mailErrorNotifier = new MailErrorNotifier("host=mailhost; port=1234; from=cari@test.com; to=anothermail; repo=gitrepository.git");
            ConnectionManager.Instance.SetConnectionString<OracleConnectionStringBuilder>(connectionString);

            level1();
            Console.ReadKey();
        }
    }
}
