// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flow.Impl
{
    public class FlowErrorHandler : IFlowErrorHandler
    {
        private readonly ConcurrentDictionary<Type, IErrorRecovery> _recoveryStrategies 
            = new ConcurrentDictionary<Type, IErrorRecovery>();
        
        private readonly List<Exception> _handledErrors = new List<Exception>();
        private readonly object _errorLock = new object();

        public bool HandleError(FlowException exception)
        {
            if (exception == null) return false;

            lock (_errorLock)
            {
                _handledErrors.Add(exception);
            }

            // Try to recover from the error
            var exceptionType = exception.GetType();
            
            if (_recoveryStrategies.TryGetValue(exceptionType, out var recovery))
            {
                try
                {
                    if (recovery.CanRecover(exception))
                    {
                        var recoveryAction = recovery.CreateRecoveryAction(exception);
                        if (recoveryAction != null)
                        {
                            // Add recovery action to kernel
                            exception.Context?.Kernel?.Root?.Add(recoveryAction);
                            recovery.RecordRecoveryAttempt(exception, true);
                            return true;
                        }
                    }
                    recovery.RecordRecoveryAttempt(exception, false);
                }
                catch (Exception recoveryException)
                {
                    // Log recovery failure but don't throw
                    exception.Context?.Kernel?.Log?.Error(
                        $"Recovery failed for {exceptionType.Name}: {recoveryException.Message}");
                    recovery.RecordRecoveryAttempt(exception, false);
                }
            }

            // Log the error
            LogError(exception);
            return false;
        }

        public void RegisterRecovery<T>(IErrorRecovery recovery) where T : FlowException
        {
            if (recovery == null) throw new ArgumentNullException(nameof(recovery));
            _recoveryStrategies.TryAdd(typeof(T), recovery);
        }

        public void UnregisterRecovery<T>() where T : FlowException
        {
            _recoveryStrategies.TryRemove(typeof(T), out _);
        }

        public IReadOnlyList<Exception> GetHandledErrors()
        {
            lock (_errorLock)
            {
                return _handledErrors.ToArray();
            }
        }

        public void ClearErrorHistory()
        {
            lock (_errorLock)
            {
                _handledErrors.Clear();
            }
        }

        private void LogError(FlowException exception)
        {
            var logger = exception.Context?.Kernel?.Log;
            if (logger == null) return;

            logger.Error($"[{exception.ErrorCode}] {exception.Message}");
            logger.Error($"Component: {exception.ComponentName}");
            logger.Error($"Timestamp: {exception.Timestamp:yyyy-MM-dd HH:mm:ss}");
            
            if (exception.InnerException != null)
            {
                logger.Error($"Inner Exception: {exception.InnerException.Message}");
            }
        }
    }

    public class DefaultErrorRecovery : IErrorRecovery
    {
        private readonly Dictionary<FlowErrorCode, Func<FlowException, ITransient>> _recoveryActions
            = new Dictionary<FlowErrorCode, Func<FlowException, ITransient>>();
        
        private readonly Dictionary<FlowErrorCode, int> _attemptCounts 
            = new Dictionary<FlowErrorCode, int>();

        private readonly int _maxAttempts;

        public DefaultErrorRecovery(int maxAttempts = 3)
        {
            _maxAttempts = maxAttempts;
        }

        public bool CanRecover(FlowException exception)
        {
            if (!_recoveryActions.ContainsKey(exception.ErrorCode))
                return false;

            var attempts = _attemptCounts.GetValueOrDefault(exception.ErrorCode, 0);
            return attempts < _maxAttempts;
        }

        public ITransient CreateRecoveryAction(FlowException exception)
        {
            if (!_recoveryActions.TryGetValue(exception.ErrorCode, out var factory))
                return null;

            try
            {
                return factory(exception);
            }
            catch
            {
                return null;
            }
        }

        public void RecordRecoveryAttempt(FlowException exception, bool success)
        {
            var currentCount = _attemptCounts.GetValueOrDefault(exception.ErrorCode, 0);
            _attemptCounts[exception.ErrorCode] = currentCount + 1;

            if (success)
            {
                // Reset count on successful recovery
                _attemptCounts[exception.ErrorCode] = 0;
            }
        }

        public void RegisterRecoveryAction(FlowErrorCode errorCode, Func<FlowException, ITransient> recoveryFactory)
        {
            _recoveryActions[errorCode] = recoveryFactory ?? throw new ArgumentNullException(nameof(recoveryFactory));
        }

        public void UnregisterRecoveryAction(FlowErrorCode errorCode)
        {
            _recoveryActions.Remove(errorCode);
            _attemptCounts.Remove(errorCode);
        }
    }
}