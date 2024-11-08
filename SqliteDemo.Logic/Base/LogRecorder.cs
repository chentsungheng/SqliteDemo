using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using SqliteDemo.Logic.Telemetry;
using SqliteDemo.Model;

namespace SqliteDemo.Logic.Base
{
    /// <summary>
    /// 遙測紀錄器
    /// </summary>
    public interface ILogRecorder : IDisposable
    {
        /// <summary>
        /// 紀錄例外
        /// </summary>
        /// <param name="Exception">例外事件</param>
        void Write(Exception Exception);

        /// <summary>
        /// 紀錄例外
        /// </summary>
        /// <param name="Exception">例外事件</param>
        /// <param name="Properties">屬性</param>
        void Write(Exception Exception, IDictionary<string, string> Properties);

        /// <summary>
        /// 紀錄資訊
        /// </summary>
        /// <param name="Message">訊息</param>
        void Write(string Message);

        /// <summary>
        /// 紀錄資訊
        /// </summary>
        /// <param name="Message">訊息</param>
        /// <param name="Properties">屬性</param>
        void Write(string Message, IDictionary<string, string> Properties);
    }

    public class LogRecorder : ILogRecorder
    {
        private TelemetryClient? _telemetry;
        private TelemetryConfiguration? _config;
        private DependencyTrackingTelemetryModule? _module;

        public LogRecorder(AppSettings Settings)
        {
            ArgumentNullException.ThrowIfNull(Settings);

            if (string.IsNullOrEmpty(Settings.Stage))
            {
                throw new InvalidOperationException($"{nameof(Settings.Stage)} is invalid.");
            }

            if (Settings.ApplicationInsights != null && !string.IsNullOrEmpty(Settings.ApplicationInsights[Settings.Stage].ConnectionString))
            {
                // 建立遙測設定
                _config = TelemetryConfiguration.CreateDefault();
                _config.ConnectionString = Settings.ApplicationInsights[Settings.Stage].ConnectionString;
                _config.TelemetryInitializers.Add(new SQLExtensionInitializer());
                _config.TelemetryInitializers.Add(new RoleNameInitializer());
                _config.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
                // 啟用追蹤模組
                _module = new DependencyTrackingTelemetryModule
                {
                    EnableSqlCommandTextInstrumentation = true
                };
                _module.Initialize(_config);

                _telemetry = new TelemetryClient(_config);
            }
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing)
            {
                return;
            }

            _telemetry?.Flush();
            _config?.Dispose();
            _module?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Write(Exception Exception)
        {
            _telemetry?.TrackException(Exception);
        }

        public virtual void Write(Exception Exception, IDictionary<string, string> Properties)
        {
            _telemetry?.TrackException(Exception, Properties);
        }

        public virtual void Write(string Message)
        {
            _telemetry?.TrackTrace(Message, SeverityLevel.Information);
        }

        public virtual void Write(string Message, IDictionary<string, string> Properties)
        {
            _telemetry?.TrackTrace(Message, SeverityLevel.Information, Properties);
        }
    }
}
