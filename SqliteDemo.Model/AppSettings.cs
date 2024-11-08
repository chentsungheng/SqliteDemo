using System.Collections.Generic;

namespace SqliteDemo.Model
{
    public class ApplicationInsightSettings
    {
        public string ConnectionString { get; set; } = default!;
    }

    public class SqliteDatabaseSettings
    {
        public string Path { get; set; } = default!;
    }

    public class AppSettings
    {
        public static readonly string FILE_NAME = "appsettings.json";

        public string Stage { get; set; } = default!;

        public IDictionary<string, ApplicationInsightSettings> ApplicationInsights { get; set; } = default!;

        public IDictionary<string, SqliteDatabaseSettings> SqliteDatabase { get; set; } = default!;
    }
}
