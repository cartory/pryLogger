
# pryLogger

This sample project demonstrates the usage of the `pryLogger` library for event logging and error handling in a C# application. The application makes REST API calls and configures email notifications in case of errors.

## Requirements

- **Connection Database**: Ensure you have an database set up with a valid connection string, in this case is with Oracle

## Usage Example

```csharp
public class Program
{
    public static void onException(Exception ex) 
    {
        Console.WriteLine($"catchException : {ex.Message}");
    }

    [ConsoleLog(nameof(onException))]
    static void level1([LogRename("customParam")]string test = "lala land") 
    {
        level2();
        //var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("select query. ...");
        throw new Exception();
    }

    [ConsoleLog(nameof(onException))]
    static void level2() 
    {
        string url = "url";
        var body = new {};
        var res = RestClient.Fetch<object>(new RestRequest(url) 
        {
            Method = "POST",
            Content = Soap.CreateXmlRequest("urlRequest", null, body, Declarations.UTF8),
            ContentType = "text/xml; charset=UTF-8",
        }, rest =>
        {
            object result  = Soap.FromXmlResponse("response", rest.Content);
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