using SqliteDemo.Model;
using System.Text.Json;

namespace SqliteDemo.Logic.Base
{
    public abstract class BusinessLogic
    {
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
    }
}
