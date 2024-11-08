using Microsoft.Data.Sqlite;
using SqliteDemo.Model;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("SqliteDemo.Test")]
namespace SqliteDemo.Logic.Base
{
    /// <summary>
    /// 商業邏輯介面
    /// </summary>
    public interface IBusinessLogic : IDisposable
    {
        /// <summary>
        /// Log紀錄器
        /// </summary>
        ILogRecorder LogRecorder { get; }

        /// <summary>
        /// 應用程式設定
        /// </summary>
        AppSettings Settings { get; }

        /// <summary>
        /// 是否自動釋放Recorder
        /// </summary>
        bool IsAutoDisposeRecorder { get; }
    }

    public abstract class BusinessLogic : IBusinessLogic
    {
        public ILogRecorder LogRecorder { get; private set; }

        protected IBusinessLogicFactory BusinessLogicFactory { get; private set; }

        protected bool IsDisposed { get; private set; }

        public AppSettings Settings { get; private set; }

        public bool IsAutoDisposeRecorder { get; private set; }

        /// <summary>
        /// 取得系統設定
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="JsonException"></exception>
        public static AppSettings GetAppSettings()
        {
            var settingPath = Path.Combine(AppContext.BaseDirectory, AppSettings.FILE_NAME);

            if (!File.Exists(settingPath))
            {
                throw new FileNotFoundException($"Could not find {AppSettings.FILE_NAME}", AppSettings.FILE_NAME);
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingPath));

            return settings ?? throw new JsonException($"{typeof(AppSettings).Name} convert failed.");
        }

        protected BusinessLogic(IBusinessLogicFactory BusinessLogicFactory, AppSettings? Settings = null, ILogRecorder? LogRecorder = null)
        {
            IsDisposed = false;
            IsAutoDisposeRecorder = LogRecorder == null;

            this.Settings ??= Settings ?? GetAppSettings();
            this.LogRecorder = LogRecorder ?? new LogRecorder(this.Settings);
            this.BusinessLogicFactory = BusinessLogicFactory;
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (LogRecorder != null && Disposing && IsAutoDisposeRecorder)
            {
                LogRecorder.Dispose();
            }

            IsDisposed = Disposing;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 取得商業邏輯
        /// </summary>
        /// <typeparam name="TLogic">邏輯型別</typeparam>
        /// <returns></returns>
        protected virtual TLogic CreateLogic<TLogic>()
        {
            return BusinessLogicFactory.GetLogic<TLogic>(Settings, LogRecorder);
        }

        /// <summary>
        /// 取得資料導向邏輯
        /// </summary>
        /// <typeparam name="TLogic">邏輯型別</typeparam>
        /// <returns></returns>
        protected virtual TLogic CreateDataLogic<TLogic>()
        {
            return BusinessLogicFactory.GetLogic<TLogic>(Settings, LogRecorder, null);
        }

        /// <summary>
        /// 取得物件屬性
        /// </summary>
        /// <param name="Model">物件</param>
        /// <returns></returns>
        protected virtual IDictionary<string, string> GetProperties(object Model)
        {
            var space = GetType().Namespace;
            var properties = new Dictionary<string, string>
            {
                { "_Namespace", space ?? string.Empty },
                { "_TypeName", GetType().Name }
            };

            if (Model == null)
            {
                return properties;
            }

            properties.Add("JSON", JsonSerializer.Serialize(Model));

            if (Model.GetType().Name.Contains("List"))
            {
                return properties;
            }

            foreach (var property in Model.GetType().GetProperties())
            {
                var value = property.GetValue(Model);
                var data = value == null ? string.Empty : value.ToString();

                properties.Add(property.Name, data ?? string.Empty);
            }

            return properties;
        }

        protected static Exception GetOriginalException(Exception Exception)
        {
            if (Exception.InnerException == null)
            {
                return Exception;
            }

            var original = Exception.InnerException;

            while (original.InnerException != null)
            {
                original = original.InnerException;
            }

            return original;
        }

        /// <summary>
        /// 紀錄例外事件
        /// </summary>
        /// <param name="Exception">例外事件</param>
        /// <returns></returns>
        protected virtual Exception LogException(Exception Exception)
        {
            if (Exception is SqliteException sql)
            {
                LogRecorder.Write(sql);
                return sql;
            }
            if (Exception is DbException db)
            {
                LogRecorder.Write(db);
                return db;
            }

            var original = GetOriginalException(Exception);

            LogRecorder.Write(original);

            return original;
        }

        /// <summary>
        /// 紀錄事件
        /// </summary>
        /// <param name="Message">事件訊息</param>
        protected virtual void LogEvent(string Message)
        {
            LogRecorder?.Write(Message);
        }

        /// <summary>
        /// 紀錄事件
        /// </summary>
        /// <param name="Message">事件訊息</param>
        /// <param name="Model">事件物件</param>
        protected virtual void LogEvent(string Message, object Model)
        {
            LogRecorder?.Write(Message, GetProperties(Model));
        }
    }
}
