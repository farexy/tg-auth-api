using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DiagnosticAdapter;
using Serilog;
using Serilog.Events;

namespace TG.Auth.Api.App.Monitoring
{
    /// <summary>
    /// Application Insights Kubernetes diagnostic source observer for self-diagnostic logs which are then redirected to our log system.
    /// It can help to detect problems with the Application Insights Kubernetes software if it doesn't work as expected (see more <see href="https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/develop/docs/SelfDiagnostics.MD">here</see>).
    /// Initial sources are taken from <see href="https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/develop/src/ApplicationInsights.Kubernetes/Debuggings/ApplicationInsightsKubernetesDiagnosticObserver.cs">ApplicationInsights-Kubernetes library</see>.
    /// </summary>
    internal class AppInsightsKubernetesDiagnosticObserver
    {
        private readonly LogEventLevel _minimumLevel;

        /// <summary>
        /// Create an instance of <see cref="AppInsightsKubernetesDiagnosticObserver"/>
        /// </summary>
        /// <param name="minimumLevel">Minimum level at which logs will be written to our system (<see cref="LogEventLevel.Warning"/> is the default one).</param>
        public AppInsightsKubernetesDiagnosticObserver(LogEventLevel? minimumLevel = null)
        {
            _minimumLevel = minimumLevel ?? LogEventLevel.Warning;
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Critical))]
        public void OnLogCritical(string content)
        {
            Write(LogEventLevel.Fatal, content);
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Error))]
        public void OnLogError(string content)
        {
            Write(LogEventLevel.Error, content);
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Warning))]
        public void OnLogWarning(string content)
        {
            Write(LogEventLevel.Warning, content);
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Information))]
        public void OnLogInfo(string content)
        {
            Write(LogEventLevel.Information, content);
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Debug))]
        public void OnLogDebug(string content)
        {
            Write(LogEventLevel.Debug, content);
        }

        [DiagnosticName(nameof(DiagnosticLogLevel.Trace))]
        public void OnLogTrace(string content)
        {
            Write(LogEventLevel.Verbose, content);
        }

        private void Write(LogEventLevel level, string content)
        {
            if (level >= _minimumLevel)
            {
                Log.Write(level, content);
            }
        }
    }
}
