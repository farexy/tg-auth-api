using TG.Core.App.Configuration.Monitoring;
using TG.Core.ServiceBus.Builders;

namespace TG.Auth.Api.Extensions
{
    public static class ServiceBusExtensions
    {
        public static ServiceBusConfigurationBuilder ConfigureSbTracing(this ServiceBusConfigurationBuilder builder)
        {
            return builder.ConfigureTracing(
                () => TgExecutionContext.TraceIdentifier,
                traceId => TgExecutionContext.TrySetTraceIdentifier(traceId));
        }
    }
}