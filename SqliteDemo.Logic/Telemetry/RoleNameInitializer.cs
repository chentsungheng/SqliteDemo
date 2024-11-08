using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace SqliteDemo.Logic.Telemetry
{
    public class RoleNameInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                var space = GetType().Namespace;
                if (space == null)
                {
                    return;
                }

                telemetry.Context.Cloud.RoleName = space.Split('.').First();
            }
        }
    }
}
