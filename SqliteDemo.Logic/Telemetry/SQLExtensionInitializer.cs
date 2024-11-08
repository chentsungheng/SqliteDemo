using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace SqliteDemo.Logic.Telemetry
{
    public class SQLExtensionInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is not DependencyTelemetry supportedTelemetry)
            {
                return;
            }
            if (!supportedTelemetry.Type.Equals("SQL", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            if (supportedTelemetry.TryGetOperationDetail(typeof(SqliteCommand).Name, out var command))
            {
                if (command is not SqliteCommand sqlCommand)
                {
                    return;
                }

                foreach (DbParameter parameter in sqlCommand.Parameters)
                {
                    var value = parameter.Value;
                    value ??= DBNull.Value;

                    var name = parameter.ParameterName.StartsWith('@') ? parameter.ParameterName : $"@{parameter.ParameterName}";

                    supportedTelemetry.Properties.Add(name, value.ToString());
                }
            }
        }
    }
}
