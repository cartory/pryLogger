
# pryLogger

This sample project demonstrates the usage of the `pryLogger` library for event logging and error handling in a C# application. The application makes REST API calls and configures email notifications in case of errors.

## Requirements

- **Connection Database**: Ensure you have an database set up with a valid connection string, in this case is with Oracle

## Usage Example

```csharp
public static void onException(Exception ex) 
{
    Console.WriteLine($"catchException : {ex.Message}");
}

[ConsoleLog(nameof(onException))]
static void level1([LogRename("customParam")]string test = "lala land") 
{
    level2();
    //var dt = ConnectionManager.Instance.GetConnection<OracleConnection>().SelectQuery("SELECT * FROM table u FETCH FIRST 10 ROWS ONLY");
}

[ConsoleLog(nameof(onException))]
static void level2() 
{
    throw new Exception();
}

static readonly ConnectionManager<OracleConnection, OracleConnectionStringBuilder> instance = ConnectionManager<OracleConnection, OracleConnectionStringBuilder>.Instance;

static void Main(string[] args)
{
    string connectionString = "databaseConnectionString";
    MailErrorNotifier mailErrorNotifier = new MailErrorNotifier("host=nomail.bg.com.bo; port=8888; from=cari@test; to=cari@test.com; repo=repo.git");
    instance.SetConnectionString(connectionString);

    //ConsoleLog.SetErrorNotifier(mailErrorNotifier);
    level1();
    Console.ReadKey();
}