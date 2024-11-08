using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SqliteDemo.Logic;
using SqliteDemo.Logic.Base;
using SqliteDemo.Logic.Telemetry;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;

namespace SqliteDemo.WebApplication
{
    /// <summary>
    /// 啟動程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 網站進入點
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = BusinessLogic.GetAppSettings();
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // 擷取啟動錯誤
            builder.WebHost.CaptureStartupErrors(true);

            // 設定JSON
            builder.Services.AddControllers().AddJsonOptions(configure =>
            {
                configure.JsonSerializerOptions.PropertyNamingPolicy = null;                    // 保持字首大寫
                configure.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());  // 字串表示列舉
            });

            // 設定ApplicationInsights
            if (!string.IsNullOrEmpty(settings.ApplicationInsights[settings.Stage].ConnectionString))
            {
                builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
                {
                    ConnectionString = settings.ApplicationInsights[settings.Stage].ConnectionString,
                    EnableAuthenticationTrackingJavaScript = true,
                    EnableAdaptiveSampling = true
                });
                builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, options) => module.EnableSqlCommandTextInstrumentation = true);
                builder.Services.ConfigureTelemetryModule<RequestTrackingTelemetryModule>((module, options) => module.CollectionOptions.TrackExceptions = true);
                builder.Services.AddSingleton<ITelemetryInitializer, SQLExtensionInitializer>();
                builder.Services.AddSingleton<ITelemetryInitializer, RoleNameInitializer>();
            }

            // 設定API Versioning
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddApiVersioning(option =>
            {
                option.ReportApiVersions = true;                    // 回應版本資訊
                option.DefaultApiVersion = new ApiVersion(1, 0);    // 預設API版本號
                option.AssumeDefaultVersionWhenUnspecified = true;  // 未提供版本則使用預設版號
            }).AddMvc().AddApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV";                  // 指定版本格式
                option.SubstituteApiVersionInUrl = true;            // 啟用URL版本替代
            });

            // 設定Swagger API Versioning
            builder.Services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            // 設定Swagger Example Assembly
            builder.Services.AddSwaggerExamplesFromAssemblies(typeof(Program).Assembly);
            // 設定Swagger產生器
            builder.Services.AddSwaggerGen(option =>
            {
                // 指定API Operation ID
                option.CustomOperationIds(selector =>
                {
                    var split = "_";
                    var method = selector.HttpMethod ?? string.Empty;
                    var route = selector.RelativePath ?? string.Empty;

                    return $"{method}{split}{route.Replace("{", string.Empty).Replace("}", string.Empty).Replace("/", split).Replace("-", split).Replace(".", split)}";
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // 指定API描述文件 & 註記
                if (File.Exists(xmlPath))
                {
                    option.IncludeXmlComments(xmlPath);
                    option.EnableAnnotations();
                    option.ExampleFilters();
                }
            });

            // 設定自訂物件
            builder.Services.AddTransient<IBusinessLogicFactory, BusinessLogicFactory>();

            const string Swagger = "swagger";

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // 設定Swagger API Versioning
            app.UseSwagger(option =>
            {
                option.PreSerializeFilters.Add((document, request) =>
                {
                    var url = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;

                    // 加入網址路徑
                    if (!string.IsNullOrEmpty(url))
                    {
                        document.Servers.Clear();
                        document.Servers.Add(new OpenApiServer { Url = url });
                    }
                });
            });
            app.UseSwaggerUI(option =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var version in provider.ApiVersionDescriptions)
                {
                    var name = $"{typeof(Program).Namespace} v{version.ApiVersion}";

                    option.SwaggerEndpoint($"{version.GroupName}/{Swagger}.json", name);
                    option.DocExpansion(DocExpansion.List);
                }
            });

            app.MapGet("/", http => Task.Run(() =>
            {
                http.Response.Redirect(Path.Combine(http.Request.GetEncodedUrl(), Swagger));
            }));

            app.MapControllers();
            app.Run();
        }
    }
}
