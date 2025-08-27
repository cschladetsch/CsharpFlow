using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flow.Test
{
    public class TestNewSystemsIntegration : TestBase
    {
        [Test]
        public void TestImprovedLoggerSystem()
        {
            var kernel = Create.Kernel();
            var consoleLogger = new Flow.Impl.FlowConsoleLogger("TestPrefix");
            
            kernel.Log = consoleLogger;
            
            Assert.AreEqual("TestPrefix", consoleLogger.LogPrefix);
            Assert.IsNotNull(consoleLogger.Formatter);
            Assert.IsNotNull(consoleLogger.Output);
            
            // Test logging doesn't throw
            Assert.DoesNotThrow(() => consoleLogger.Info("Test info message"));
            Assert.DoesNotThrow(() => consoleLogger.Warn("Test warning"));
            Assert.DoesNotThrow(() => consoleLogger.Error("Test error"));
            Assert.DoesNotThrow(() => consoleLogger.Verbose(5, "Test verbose"));
        }

        [Test]
        public void TestUnityLoggerSystem()
        {
            var unityLogger = new Flow.Impl.FlowUnityLogger("UnityTest");
            
            Assert.AreEqual("UnityTest", unityLogger.LogPrefix);
            Assert.IsNotNull(unityLogger.Formatter);
            Assert.IsNotNull(unityLogger.Output);
            
            // Test logging doesn't throw
            Assert.DoesNotThrow(() => unityLogger.Info("Unity info message"));
            Assert.DoesNotThrow(() => unityLogger.Warn("Unity warning"));
        }

        [Test]
        public void TestDisposableTransientLifecycle()
        {
            var kernel = Create.Kernel();
            var disposableTransient = new Flow.Impl.DisposableTransient();
            disposableTransient.Kernel = kernel;
            
            bool disposingEventFired = false;
            disposableTransient.Disposing += (sender, args) => 
            {
                disposingEventFired = true;
                Assert.AreEqual(Flow.DisposalReason.Explicit, args.Reason);
            };
            
            Assert.IsTrue(disposableTransient.Active);
            Assert.IsFalse(disposableTransient.IsDisposed);
            
            disposableTransient.Dispose();
            
            Assert.IsTrue(disposingEventFired);
            Assert.IsTrue(disposableTransient.IsDisposed);
            Assert.IsFalse(disposableTransient.Active);
        }

        [Test]
        public void TestDisposeAfterDependency()
        {
            var kernel = Create.Kernel();
            var dependency = kernel.Factory.Future<bool>();
            var disposable = new Flow.Impl.DisposableTransient();
            disposable.Kernel = kernel;
            
            bool disposed = false;
            disposable.Disposing += (s, e) => disposed = true;
            
            disposable.DisposeAfter(dependency);
            
            Assert.IsFalse(disposed);
            
            dependency.Value = true;
            
            Assert.IsTrue(disposed);
            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void TestWeakEventManager()
        {
            var eventManager = new Flow.Impl.WeakEventManager();
            var target = new TestEventTarget();
            bool eventHandled = false;
            
            EventHandler handler = (s, e) => eventHandled = true;
            
            eventManager.Subscribe(target, "TestEvent", handler);
            eventManager.RaiseEvent(target, "TestEvent", EventArgs.Empty);
            
            Assert.IsTrue(eventHandled);
            
            eventManager.Unsubscribe(target, "TestEvent", handler);
            eventHandled = false;
            eventManager.RaiseEvent(target, "TestEvent", EventArgs.Empty);
            
            Assert.IsFalse(eventHandled);
        }

        [Test]
        public void TestFlowExceptionHierarchy()
        {
            var kernel = Create.Kernel();
            var generator = kernel.Factory.Do(() => { });
            
            var flowException = new Flow.Impl.FlowException(Flow.FlowErrorCode.GeneratorInvalidState, generator);
            var generatorException = new Flow.Impl.GeneratorException(Flow.FlowErrorCode.GeneratorStepFailed, "Step failed", generator);
            var kernelException = new Flow.Impl.KernelException(Flow.FlowErrorCode.KernelNotInitialized, "Not initialized", kernel);
            
            Assert.IsInstanceOf<Flow.IFlowException>(flowException);
            Assert.IsInstanceOf<Flow.Impl.FlowException>(generatorException);
            Assert.IsInstanceOf<Flow.Impl.FlowException>(kernelException);
            
            Assert.AreEqual(Flow.FlowErrorCode.GeneratorInvalidState, flowException.ErrorCode);
            Assert.AreEqual(Flow.FlowErrorCode.GeneratorStepFailed, generatorException.ErrorCode);
            Assert.AreEqual(Flow.FlowErrorCode.KernelNotInitialized, kernelException.ErrorCode);
        }

        [Test]
        public void TestErrorHandlerWithRecovery()
        {
            var errorHandler = new Flow.Impl.FlowErrorHandler();
            var recovery = new Flow.Impl.DefaultErrorRecovery(maxAttempts: 2);
            
            recovery.RegisterRecoveryAction(Flow.FlowErrorCode.GeneratorInvalidState, 
                ex => ex.Context?.Kernel?.Factory?.Do(() => { /* recovery action */ }));
            
            errorHandler.RegisterRecovery<Flow.Impl.GeneratorException>(recovery);
            
            var kernel = Create.Kernel();
            var generator = kernel.Factory.Do(() => { });
            var exception = new Flow.Impl.GeneratorException(Flow.FlowErrorCode.GeneratorInvalidState, generator);
            
            var handled = errorHandler.HandleError(exception);
            
            Assert.IsTrue(handled);
        }

        [Test]
        public void TestObjectPoolBasicFunctionality()
        {
            var pool = new Flow.Impl.FlowObjectPool<TestPoolableObject>(() => new TestPoolableObject(), maxCapacity: 10);
            
            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);
            Assert.AreNotSame(obj1, obj2);
            
            obj1.TestValue = 42;
            pool.Return(obj1);
            
            var obj3 = pool.Rent();
            Assert.AreSame(obj1, obj3);
            Assert.AreEqual(0, obj3.TestValue); // Should be reset
            
            var stats = pool.GetStatistics();
            Assert.AreEqual(3, stats.TotalRented);
            Assert.AreEqual(1, stats.TotalReturned);
        }

        [Test]
        public void TestPoolManagerFunctionality()
        {
            var poolManager = new Flow.Impl.FlowPoolManager();
            
            var stringPool = poolManager.GetPool<string>();
            var listPool = poolManager.CreatePool<List<int>>(() => new List<int>(), 50);
            
            Assert.IsNotNull(stringPool);
            Assert.IsNotNull(listPool);
            
            var str = stringPool.Rent();
            var list = listPool.Rent();
            
            Assert.IsNotNull(str);
            Assert.IsNotNull(list);
            
            var stats = poolManager.GetStatistics();
            Assert.AreEqual(2, stats.TotalPools);
        }

        [Test]
        public void TestConcurrentPoolAccess()
        {
            var pool = new Flow.Impl.FlowObjectPool<TestPoolableObject>(() => new TestPoolableObject(), maxCapacity: 100);
            var objects = new System.Collections.Concurrent.ConcurrentBag<TestPoolableObject>();
            
            System.Threading.Tasks.Parallel.For(0, 50, i =>
            {
                var obj = pool.Rent();
                objects.Add(obj);
            });
            
            Assert.AreEqual(50, objects.Count);
            
            System.Threading.Tasks.Parallel.ForEach(objects, obj =>
            {
                pool.Return(obj);
            });
            
            var stats = pool.GetStatistics();
            Assert.AreEqual(50, stats.TotalRented);
            Assert.LessOrEqual(stats.TotalReturned, 50);
        }

        [Test]
        public void TestComplexErrorRecoveryScenario()
        {
            var kernel = Create.Kernel();
            var errorHandler = new Flow.Impl.FlowErrorHandler();
            var recovery = new Flow.Impl.DefaultErrorRecovery(maxAttempts: 3);
            
            int recoveryAttempts = 0;
            recovery.RegisterRecoveryAction(Flow.FlowErrorCode.GeneratorStepFailed,
                ex => 
                {
                    recoveryAttempts++;
                    return ex.Context?.Kernel?.Factory?.Do(() => { /* recovery */ });
                });
            
            errorHandler.RegisterRecovery<Flow.Impl.GeneratorException>(recovery);
            
            var generator = kernel.Factory.Do(() => { });
            var exception = new Flow.Impl.GeneratorException(Flow.FlowErrorCode.GeneratorStepFailed, generator);
            
            // First attempt should succeed
            Assert.IsTrue(errorHandler.HandleError(exception));
            Assert.AreEqual(1, recoveryAttempts);
            
            // Additional attempts should also succeed until max reached
            Assert.IsTrue(errorHandler.HandleError(exception));
            Assert.IsTrue(errorHandler.HandleError(exception));
            
            // After max attempts, should fail
            Assert.IsFalse(errorHandler.HandleError(exception));
        }

        [Test]
        public void TestLogFormatterWithStackTrace()
        {
            var formatter = new Flow.Impl.DefaultLogFormatter();
            var context = new Flow.Impl.LogContext
            {
                Prefix = "Test",
                ShowSource = true,
                ShowStack = false
            };
            
            var formatted = formatter.Format(Flow.ELogLevel.Info, "Test message", context);
            
            Assert.IsNotEmpty(formatted);
            Assert.Contains("Test:", formatted);
            Assert.Contains("Test message", formatted);
            
            var stackTrace = formatter.FormatWithStackTrace(Flow.ELogLevel.Error, "Error message", context, true);
            
            // Stack trace should be present for errors
            Assert.IsNotNull(stackTrace);
        }

        [Test]
        public void TestDisposableTransientWithTimeout()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            var disposable = new Flow.Impl.DisposableTransient();
            disposable.Kernel = kernel;
            
            var timer = factory.OneShotTimer(TimeSpan.FromMilliseconds(50));
            kernel.Root.Add(timer);
            
            bool timeoutDisposed = false;
            disposable.Disposing += (s, e) => 
            {
                if (e.Reason == Flow.DisposalReason.Completed)
                    timeoutDisposed = true;
            };
            
            disposable.DisposeAfter(timer);
            
            // Wait for timer
            StepUntilComplete(timer);
            
            Assert.IsTrue(timeoutDisposed);
            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void TestMemoryCleanupWithWeakEvents()
        {
            var eventManager = new Flow.Impl.WeakEventManager();
            var target = new TestEventTarget();
            
            EventHandler handler = (s, e) => { /* handler */ };
            eventManager.Subscribe(target, "TestEvent", handler);
            
            // Cleanup should remove dead references
            eventManager.Cleanup();
            
            // Target should still be alive, subscription should remain
            var handled = false;
            var testHandler = new EventHandler((s, e) => handled = true);
            eventManager.Subscribe(target, "TestEvent", testHandler);
            eventManager.RaiseEvent(target, "TestEvent", EventArgs.Empty);
            
            Assert.IsTrue(handled);
        }

        [Test]
        public void TestPoolStatisticsAccuracy()
        {
            var pool = new Flow.Impl.FlowObjectPool<TestPoolableObject>(() => new TestPoolableObject(), maxCapacity: 5);
            
            // Rent some objects
            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            var obj3 = pool.Rent();
            
            var stats = pool.GetStatistics();
            Assert.AreEqual(3, stats.TotalRented);
            Assert.AreEqual(0, stats.TotalReturned);
            Assert.AreEqual(3, stats.CurrentlyRented);
            
            // Return some objects
            pool.Return(obj1);
            pool.Return(obj2);
            
            stats = pool.GetStatistics();
            Assert.AreEqual(3, stats.TotalRented);
            Assert.AreEqual(2, stats.TotalReturned);
            Assert.AreEqual(1, stats.CurrentlyRented);
            Assert.AreEqual(2, stats.PooledCount);
        }

        [Test]
        public void TestErrorHandlerLogging()
        {
            var kernel = Create.Kernel();
            var logger = new TestLogger();
            kernel.Log = logger;
            
            var errorHandler = new Flow.Impl.FlowErrorHandler();
            var generator = kernel.Factory.Do(() => { });
            var exception = new Flow.Impl.GeneratorException(Flow.FlowErrorCode.GeneratorStepFailed, "Test error", generator);
            
            errorHandler.HandleError(exception);
            
            Assert.IsTrue(logger.ErrorMessages.Any(msg => msg.Contains("GeneratorStepFailed")));
            Assert.IsTrue(logger.ErrorMessages.Any(msg => msg.Contains("Test error")));
        }

        [Test]
        public void TestPoolCapacityLimits()
        {
            var pool = new Flow.Impl.FlowObjectPool<TestPoolableObject>(() => new TestPoolableObject(), maxCapacity: 2);
            
            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            var obj3 = pool.Rent();
            
            // Return all objects
            pool.Return(obj1);
            pool.Return(obj2);
            pool.Return(obj3); // This should not be stored due to capacity limit
            
            var stats = pool.GetStatistics();
            Assert.AreEqual(2, stats.PooledCount); // Only 2 should be pooled
            Assert.AreEqual(2, stats.MaxCapacity);
        }

        [Test]
        public void TestComplexDisposalChain()
        {
            var kernel = Create.Kernel();
            var root = new Flow.Impl.DisposableTransient { Kernel = kernel };
            var child1 = new Flow.Impl.DisposableTransient { Kernel = kernel };
            var child2 = new Flow.Impl.DisposableTransient { Kernel = kernel };
            
            child1.DisposeAfter(root);
            child2.DisposeAfter(root);
            
            Assert.IsFalse(child1.IsDisposed);
            Assert.IsFalse(child2.IsDisposed);
            
            root.Dispose();
            
            Assert.IsTrue(root.IsDisposed);
            Assert.IsTrue(child1.IsDisposed);
            Assert.IsTrue(child2.IsDisposed);
        }

        [Test]
        public void TestLogContextWithGenerator()
        {
            var kernel = Create.Kernel();
            var generator = kernel.Factory.Do(() => { });
            var context = new Flow.Impl.LogContext
            {
                Subject = generator,
                CurrentGenerator = generator
            };
            
            generator.Name = "TestGenerator";
            
            var formatter = new Flow.Impl.DefaultLogFormatter();
            var formatted = formatter.Format(Flow.ELogLevel.Info, "Test", context);
            
            Assert.Contains("TestGenerator", formatted);
        }

        [Test]
        public void TestMultipleErrorRecoveryStrategies()
        {
            var errorHandler = new Flow.Impl.FlowErrorHandler();
            var recovery1 = new Flow.Impl.DefaultErrorRecovery();
            var recovery2 = new Flow.Impl.DefaultErrorRecovery();
            
            errorHandler.RegisterRecovery<Flow.Impl.GeneratorException>(recovery1);
            errorHandler.RegisterRecovery<Flow.Impl.KernelException>(recovery2);
            
            var kernel = Create.Kernel();
            var genException = new Flow.Impl.GeneratorException(Flow.FlowErrorCode.GeneratorInvalidState, kernel.Factory.Do(() => { }));
            var kernelException = new Flow.Impl.KernelException(Flow.FlowErrorCode.KernelNotInitialized, kernel);
            
            // Both should be handled by their respective recovery strategies
            Assert.IsFalse(errorHandler.HandleError(genException)); // No recovery action registered
            Assert.IsFalse(errorHandler.HandleError(kernelException)); // No recovery action registered
        }

        // Helper classes for testing
        private class TestEventTarget { }
        
        private class TestPoolableObject : Flow.IPoolable
        {
            public int TestValue { get; set; }
            
            public void Reset()
            {
                TestValue = 0;
            }
            
            public bool IsPoolable => true;
        }
        
        private class TestLogger : Flow.ILogger
        {
            public List<string> ErrorMessages { get; } = new List<string>();
            public List<string> InfoMessages { get; } = new List<string>();
            
            public string LogPrefix { get; set; }
            public object LogSubject { get; set; }
            public int Verbosity { get; set; }
            public bool ShowSource { get; set; }
            public bool ShowStack { get; set; }
            
            public void Info(string fmt, params object[] args)
            {
                InfoMessages.Add(string.Format(fmt, args));
            }
            
            public void Warn(string fmt, params object[] args)
            {
                InfoMessages.Add($"WARN: {string.Format(fmt, args)}");
            }
            
            public void Error(string fmt, params object[] args)
            {
                ErrorMessages.Add(string.Format(fmt, args));
            }
            
            public void Verbose(int level, string fmt, params object[] args)
            {
                if (level <= Verbosity)
                    InfoMessages.Add($"VERBOSE: {string.Format(fmt, args)}");
            }
        }
        
        private void StepUntilComplete(ITransient target, int maxSteps = 1000)
        {
            int steps = 0;
            while (target.Active && steps < maxSteps)
            {
                Kernel.Update(0.01f);
                steps++;
            }
            
            Assert.IsFalse(target.Active, $"Target did not complete within {maxSteps} steps");
        }
    }
}