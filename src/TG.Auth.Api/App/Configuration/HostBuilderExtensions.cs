using System;
using System.Diagnostics;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using TG.Auth.Api.App.Monitoring;

namespace TG.Auth.Api.App.Configuration
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureTgKeyVault(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                var appConfiguration = cfg.Build();
                cfg.AddAzureKeyVault(
                    new SecretClient(new Uri(appConfiguration.GetConnectionString("KeyVault")),
                        new DefaultAzureCredential(false)),
                    new KeyVaultSecretManager());
            });

        public static IHostBuilder ConfigureTgLogging(this IHostBuilder hostBuilder, string serviceName)
        {
            return hostBuilder.ConfigureLogging((host, logging) =>
            {
                if (!host.HostingEnvironment.IsDevelopment() || !Debugger.IsAttached)
                {
                    logging.ClearProviders();

                    var logLevelSwitch =
                        new LoggingLevelSwitch(host.Configuration.GetValue<LogEventLevel>("Serilog:MinimumLevel"));

                    logging.Services.AddSingleton(logLevelSwitch);

                    var logAnalyticsSection = host.Configuration.GetSection("LogAnalytics");
                    var analyticsWorkspaceId = logAnalyticsSection.GetValue<string>("WorkspaceId");
                    var analyticsAuthenticationId = logAnalyticsSection.GetValue<string>("PrimaryKey");
                    // todo remove
                    Console.WriteLine("Auth id " + analyticsAuthenticationId);

                   Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(host.Configuration)
                        .MinimumLevel.ControlledBy(logLevelSwitch)
                        .Enrich.WithProperty(MonitoringConstants.Environment, host.HostingEnvironment.EnvironmentName)
                        .Enrich.WithProperty(MonitoringConstants.ServiceName, serviceName)
                        .Enrich.FromLogContext()
                        .Enrich.WithDynamicProperty(MonitoringConstants.TranceIdentifierLogProperty, () => TgExecutionContext.TraceIdentifier)
                        .WriteTo.Console(new JsonFormatter())
                        .WriteTo.AzureAnalytics(analyticsWorkspaceId, analyticsAuthenticationId)
                        .CreateLogger();
                    logging.AddSerilog();
                }
            });

        }
    }
}