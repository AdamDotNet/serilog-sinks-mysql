# Serilog.Sinks.MySQL
A [Serilog](https://github.com/serilog/serilog) sink that writes to MySQL

**Package** - N/A
| **Platforms** - .NET 4.5, .NET Standard 1.3

#### Code

```csharp
string connectionString = "Uid=serilog;Pwd=serilog;Server=localhost;Port=3306;Database=logs;";
string tableName = "logs";

// Used columns (Key is a column name) 
// Column type is writer's constructor parameter
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(DbType.String) },
    { "message_template", new MessageTemplateColumnWriter(DbType.String) },
    { "level", new LevelColumnWriter(true, DbType.String) },
    { "raise_date", new TimestampColumnWriter(DbType.DateTimeOffset) },
    { "exception", new ExceptionColumnWriter(DbType.String) },
    { "properties", new LogEventSerializedColumnWriter(DbType.String) },
    { "props_test", new PropertiesColumnWriter(DbType.String) },
    { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.Raw, DbType.String) }
};

var logger = new LoggerConfiguration()
			        .WriteTo.MySQL(connectionString, tableName, columnWriters)
			        .CreateLogger();
```

#### License

**Apache 2.0**

Forked from [b00ted/serilog-sinks-postgresql](https://github.com/b00ted/serilog-sinks-postgresql)