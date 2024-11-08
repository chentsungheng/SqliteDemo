using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace SqliteDemo.WebApplication
{
    /// <summary>
    /// Swagger設定選項
    /// </summary>
    public class SwaggerConfigureOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        /// <summary>
        /// Swagger設定建構子
        /// </summary>
        /// <param name="Provider">API版本描述</param>
        public SwaggerConfigureOptions(IApiVersionDescriptionProvider Provider) => _provider = Provider;

        /// <summary>
        /// 取得標題
        /// </summary>
        /// <param name="Value">字串</param>
        /// <returns></returns>
        private static string GetTitle(string? Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }

            return Value.Replace('.', ' ');
        }

        /// <summary>
        /// 設定
        /// </summary>
        /// <param name="options">Swagger選項</param>
        public void Configure(SwaggerGenOptions options)
        {
            var title = GetTitle(typeof(Program).Namespace);

            foreach (var version in _provider.ApiVersionDescriptions)
            {
                var info = new OpenApiInfo
                {
                    Version = version.ApiVersion.ToString(),
                    Title = title,
                    Description = $"Sqlite Demo 專用"
                };
                var builder = new StringBuilder(info.Description);

                if (version.IsDeprecated)
                {
                    builder.Append(" (注意! 此版本已不再維護)");
                    info.Description = builder.ToString();
                }
                else
                {
                    builder.Clear();
                }

                options.SwaggerDoc(version.GroupName, info);
            }
        }
    }
}
