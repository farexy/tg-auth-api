using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace TG.Auth.Api.App.Monitoring
{
    /// <summary>
    /// Service collection extensions for logging and monitoring
    /// </summary>
    public static class ServiceCollectionMonitoringExtensions
    {
        private const string EnvironmentVariableValueSource = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// Adds the Serlog logging with a Console sink by default and possiblity to load parameters from <paramref name="configuration"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="serviceName">Name of service.</param>
        /// <returns>Service collection with gloabally-shared logger.</returns>
        public static IServiceCollection AddConfiguredTgLogging(this IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            var logLevelSwitch = new LoggingLevelSwitch(configuration.GetValue<LogEventLevel>("Serilog:Debug"));

            services.AddSingleton(logLevelSwitch);

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .Enrich.WithProperty(MonitoringConstants.Environment, Environment.GetEnvironmentVariable(EnvironmentVariableValueSource))
                .Enrich.WithProperty(MonitoringConstants.ServiceName, serviceName)
                .Enrich.FromLogContext()
                .Enrich.WithDynamicProperty(MonitoringConstants.TranceIdentifierLogProperty, () => TgExecutionContext.TraceIdentifier)
                .WriteTo.Console(new JsonFormatter());

            return services.AddLogger(loggerConfiguration);
        }

        /// <summary>
        /// Adds the Serlog logging with a Console and Log Analytics logs.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="workspaceId">Azure Log Analytics workspace ID.</param>
        /// <param name="authenticationId">Azure Log Analytics authentication ID.</param>
        /// <param name="serviceName">Name of service.</param>
        /// <returns>Service collection with gloabally-shared logger.</returns>
        public static IServiceCollection AddAzureLogAnalyticsTgLogging(this IServiceCollection services, string workspaceId,
            string authenticationId, string serviceName)
        {
            var logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);

            services.AddSingleton(logLevelSwitch);

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .Enrich.WithProperty(MonitoringConstants.Environment, Environment.GetEnvironmentVariable(EnvironmentVariableValueSource))
                .Enrich.WithProperty(MonitoringConstants.ServiceName, serviceName)
                .Enrich.WithDynamicProperty(MonitoringConstants.TranceIdentifierLogProperty, () => TgExecutionContext.TraceIdentifier)
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.AzureAnalytics(workspaceId, authenticationId);

            return services.AddLogger(loggerConfiguration);
        }

        /// <summary>
        /// Adds Azure Application Insights metrics enriched with Kubernetes information with a possiblity to load parameters from <paramref name="configuration"/>.
        /// Instrumentation key can be provided either in configuration or using the APPINSIGHTS_INSTRUMENTATIONKEY environment variable.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection with Application Insights services enabled.</returns>
        public static IServiceCollection AddKubernetesTgApplicationInsights(this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.AddApplicationInsightsTelemetry(configuration);
            }
            else
            {
                services.AddApplicationInsightsTelemetry();
            }

            return services.EnrichTgApplicationInsightsWithKubernetes();
        }

        /// <summary>
        /// Adds the parametrized Azure Application Insights metrics enriched with Kubernetes information.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="instrumentationKey">Azure instrumentation key.</param>
        /// <returns>Service collection with Application Insights services enabled.</returns>
        public static IServiceCollection AddKubernetesTgApplicationInsights(this IServiceCollection services,
            string instrumentationKey)
        {
            return services.AddApplicationInsightsTelemetry(instrumentationKey).EnrichTgApplicationInsightsWithKubernetes();
        }

        private static IServiceCollection EnrichTgApplicationInsightsWithKubernetes(this IServiceCollection services)
        {
            var observer = new AppInsightsKubernetesDiagnosticObserver(LogEventLevel.Warning);
            ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);
            services.AddApplicationInsightsKubernetesEnricher(opts => { opts.InitializationTimeout = TimeSpan.FromMinutes(2); });

            return services;
        }

        private static IServiceCollection AddLogger(this IServiceCollection services, LoggerConfiguration loggerConfiguration)
        {
            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSingleton(Log.Logger);
            // services.AddConfigurationRefreshHandler<MonitoringOptions, MonitoringOptionsRefreshHandler>();
            return services;
        }
    }
}
