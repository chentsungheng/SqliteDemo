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
    /// �Ұʵ{��
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// �����i�J�I
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = BusinessLogic.GetAppSettings();
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // �^���Ұʿ��~
            builder.WebHost.CaptureStartupErrors(true);

            // �]�wJSON
            builder.Services.AddControllers().AddJsonOptions(configure =>
            {
                configure.JsonSerializerOptions.PropertyNamingPolicy = null;                    // �O���r���j�g
                configure.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());  // �r���ܦC�|
            });

            // �]�wApplicationInsights
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

            // �]�wAPI Versioning
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddApiVersioning(option =>
            {
                option.ReportApiVersions = true;                    // �^��������T
                option.DefaultApiVersion = new ApiVersion(1, 0);    // �w�]API������
                option.AssumeDefaultVersionWhenUnspecified = true;  // �����Ѫ����h�ϥιw�]����
            }).AddMvc().AddApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV";                  // ���w�����榡
                option.SubstituteApiVersionInUrl = true;            // �ҥ�URL�������N
            });

            // �]�wSwagger API Versioning
            builder.Services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            // �]�wSwagger Example Assembly
            builder.Services.AddSwaggerExamplesFromAssemblies(typeof(Program).Assembly);
            // �]�wSwagger���;�
            builder.Services.AddSwaggerGen(option =>
            {
                // ���wAPI Operation ID
                option.CustomOperationIds(selector =>
                {
                    var split = "_";
                    var method = selector.HttpMethod ?? string.Empty;
                    var route = selector.RelativePath ?? string.Empty;

                    return $"{method}{split}{route.Replace("{", string.Empty).Replace("}", string.Empty).Replace("/", split).Replace("-", split).Replace(".", split)}";
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // ���wAPI�y�z��� & ���O
                if (File.Exists(xmlPath))
                {
                    option.IncludeXmlComments(xmlPath);
                    option.EnableAnnotations();
                    option.ExampleFilters();
                }
            });

            // �]�w�ۭq����
            builder.Services.AddTransient<IBusinessLogicFactory, BusinessLogicFactory>();

            const string Swagger = "swagger";

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // �]�wSwagger API Versioning
            app.UseSwagger(option =>
            {
                option.PreSerializeFilters.Add((document, request) =>
                {
                    var url = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;

                    // �[�J���}���|
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
