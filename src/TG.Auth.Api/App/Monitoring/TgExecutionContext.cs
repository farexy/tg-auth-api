using System;
using System.Threading;
using Serilog;

namespace TG.Auth.Api.App.Monitoring
{
    /// <summary>
    /// Execution context uses <see cref="AsyncLocal{T}"/> to hold ambient context.
    /// IMPORTANT: <see cref="AsyncLocal{T}"/> works only top down, i.e. if you set a value in a child task, the parent task and other execution flow branches will NOT share the same context!
    /// That's why you should set needed properties as soon you have corresponding values for them.
    /// </summary>
    public static class TgExecutionContext
    {
        private static AsyncLocal<string> _traceIdentifier = new AsyncLocal<string>();

        public static string? TraceIdentifier => _traceIdentifier.Value;

        /// <summary>
        /// Tries to set the trace identifier.
        /// </summary>
        /// <param name="traceIdentifier">Trace identifier.</param>
        /// <param name="force">If existing trace ID should be replaced (set to <c>true</c> ONLY if you receive and handle traced entities in a constant context)!</param>
        /// <returns></returns>
        public static bool TrySetTraceIdentifier(string traceIdentifier, bool force = false)
        {
            return TrySetValue(nameof(TraceIdentifier), traceIdentifier, _traceIdentifier, string.IsNullOrEmpty, force);
        }

        private static bool TrySetValue<T>(
            string contextPropertyName,
            T newValue,
            AsyncLocal<T> ambientHolder,
            Func<T, bool> valueInvalidator,
            bool force)
            where T : IEquatable<T>
        {
            if (newValue.Equals(default) || valueInvalidator.Invoke(newValue))
            {
                return false;
            }

            var currentValue = ambientHolder.Value;
            if (force || currentValue is null || currentValue.Equals(default) || valueInvalidator.Invoke(currentValue))
            {
                ambientHolder.Value = newValue;
                return true;
            }
            else if (!currentValue.Equals(newValue))
            {
                Log.Error($"Tried to set different value for {contextPropertyName}, but it is already set for this execution flow - " +
                    $"please, check the execution context logic! Current value: {currentValue} ; rejected value: {newValue}");
            }

            return false;
        }
    }
}
