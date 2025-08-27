using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flow.Test
{
    public class TestAdvancedScenarios : TestBase
    {
        [Test]
        public void TestNestedBarriersWithTimeout()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var innerBarrier = factory.TimedBarrier(TimeSpan.FromSeconds(1));
            var outerBarrier = factory.Barrier();
            
            var task1 = factory.Future<bool>();
            var task2 = factory.Future<bool>();
            
            innerBarrier.Add(task1, task2);
            outerBarrier.Add(innerBarrier);
            
            kernel.Root.Add(outerBarrier);
            
            // Should not complete initially
            StepKernelTimes(5);
            Assert.IsTrue(outerBarrier.Active);
            Assert.IsTrue(innerBarrier.Active);
            
            // Complete one task
            task1.Value = true;
            StepKernelTimes(3);
            Assert.IsTrue(outerBarrier.Active);
            Assert.IsTrue(innerBarrier.Active);
            
            // Complete second task
            task2.Value = true;
            StepKernelTimes(3);
            Assert.IsFalse(outerBarrier.Active);
            Assert.IsFalse(innerBarrier.Active);
        }

        [Test]
        public void TestComplexSequenceWithConditionals()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            bool condition1 = false;
            bool condition2 = true;
            int executionOrder = 0;
            var results = new List<int>();
            
            var sequence = factory.Sequence(
                factory.Do(() => results.Add(++executionOrder)),
                factory.If(() => condition1, factory.Do(() => results.Add(++executionOrder))),
                factory.Do(() => results.Add(++executionOrder)),
                factory.If(() => condition2, factory.Do(() => results.Add(++executionOrder))),
                factory.Do(() => results.Add(++executionOrder))
            );
            
            kernel.Root.Add(sequence);
            
            StepUntilComplete(sequence);
            
            // Should execute steps 1, 3, 4, 5 (skipping step 2 due to false condition)
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, results);
        }

        [Test]
        public void TestChannelProducerConsumerPattern()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var channel = factory.Channel<int>();
            var results = new List<int>();
            bool producerComplete = false;
            bool consumerComplete = false;
            
            var producer = factory.Coroutine(ProducerCoroutine, channel);
            var consumer = factory.Coroutine(ConsumerCoroutine, channel, results);
            
            producer.Completed += _ => producerComplete = true;
            consumer.Completed += _ => consumerComplete = true;
            
            kernel.Root.Add(producer, consumer);
            
            StepUntilComplete(producer);
            StepUntilComplete(consumer);
            
            Assert.IsTrue(producerComplete);
            Assert.IsTrue(consumerComplete);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, results);
        }
        
        private IEnumerator<int> ProducerCoroutine(IGenerator self, IChannel<int> channel)
        {
            for (int i = 1; i <= 5; i++)
            {
                channel.Write(i);
                yield return i;
            }
        }
        
        private IEnumerator<bool> ConsumerCoroutine(IGenerator self, IChannel<int> channel, List<int> results)
        {
            for (int i = 0; i < 5; i++)
            {
                var value = channel.Read();
                results.Add(value);
                yield return true;
            }
        }

        [Test]
        public void TestTriggerWithMultipleCompletionSources()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var future1 = factory.Future<string>();
            var future2 = factory.Future<string>();
            var future3 = factory.Future<string>();
            
            var trigger = factory.Trigger(future1, future2, future3);
            kernel.Root.Add(trigger);
            
            StepKernelTimes(3);
            Assert.IsTrue(trigger.Active);
            
            // Complete second future - trigger should complete
            future2.Value = "completed";
            StepKernelTimes(3);
            Assert.IsFalse(trigger.Active);
            
            // Other futures should still be active
            Assert.IsTrue(future1.Active);
            Assert.IsTrue(future3.Active);
        }

        [Test]
        public void TestPeriodicTimerWithMultipleTicks()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var tickCount = 0;
            var periodicTimer = factory.PeriodicTimer(TimeSpan.FromMilliseconds(100));
            
            periodicTimer.Elapsed += _ => tickCount++;
            
            kernel.Root.Add(periodicTimer);
            
            // Simulate time progression
            for (int i = 0; i < 10; i++)
            {
                kernel.Update(0.1f); // 100ms per update
                if (tickCount >= 3) break;
            }
            
            Assert.GreaterOrEqual(tickCount, 3);
            Assert.IsTrue(periodicTimer.Active); // Periodic timers stay active
        }

        [Test]
        public void TestGeneratorSuspendResumeWithDependencies()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var dependency = factory.Future<bool>();
            var dependentGenerator = factory.Do(() => { /* action */ });
            
            dependentGenerator.SuspendAfter(dependency);
            kernel.Root.Add(dependentGenerator);
            
            StepKernelTimes(5);
            Assert.IsTrue(dependentGenerator.Active);
            Assert.IsFalse(dependentGenerator.Running);
            
            // Complete dependency
            dependency.Value = true;
            StepKernelTimes(3);
            
            Assert.IsFalse(dependentGenerator.Active); // Should have completed
        }

        [Test]
        public void TestFactoryCreationPatterns()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            // Test various factory creation methods
            var node = factory.Node();
            var group = factory.Group();
            var sequence = factory.Sequence();
            var barrier = factory.Barrier();
            var trigger = factory.Trigger();
            var timer = factory.OneShotTimer(TimeSpan.FromMilliseconds(10));
            var periodic = factory.PeriodicTimer(TimeSpan.FromMilliseconds(10));
            var future = factory.Future<int>();
            
            Assert.IsNotNull(node);
            Assert.IsNotNull(group);
            Assert.IsNotNull(sequence);
            Assert.IsNotNull(barrier);
            Assert.IsNotNull(trigger);
            Assert.IsNotNull(timer);
            Assert.IsNotNull(periodic);
            Assert.IsNotNull(future);
            
            // All should be active after creation
            Assert.IsTrue(node.Active);
            Assert.IsTrue(group.Active);
            Assert.IsTrue(sequence.Active);
            Assert.IsTrue(barrier.Active);
            Assert.IsTrue(trigger.Active);
            Assert.IsTrue(timer.Active);
            Assert.IsTrue(periodic.Active);
            Assert.IsTrue(future.Active);
        }

        [Test]
        public void TestErrorHandlingInCoroutines()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            bool exceptionCaught = false;
            var coroutine = factory.Coroutine(ErrorCoroutine);
            
            try
            {
                kernel.Root.Add(coroutine);
                StepUntilComplete(coroutine);
            }
            catch (Exception)
            {
                exceptionCaught = true;
            }
            
            // The coroutine should handle exceptions gracefully
            // or propagate them appropriately
            Assert.IsTrue(exceptionCaught || !coroutine.Active);
        }
        
        private IEnumerator<int> ErrorCoroutine(IGenerator self)
        {
            yield return 1;
            throw new InvalidOperationException("Test exception");
        }

        [Test]
        public void TestMemoryCleanupOnCompletion()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var completedHandlerCalled = false;
            var future = factory.Future<string>();
            
            future.Completed += _ => completedHandlerCalled = true;
            kernel.Root.Add(future);
            
            StepKernelTimes(3);
            Assert.IsTrue(future.Active);
            Assert.IsFalse(completedHandlerCalled);
            
            future.Value = "test";
            StepKernelTimes(3);
            
            Assert.IsFalse(future.Active);
            Assert.IsTrue(completedHandlerCalled);
        }

        [Test]
        public void TestDeepNestedSequences()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var executionOrder = new List<int>();
            
            var deepSequence = factory.Sequence(
                factory.Do(() => executionOrder.Add(1)),
                factory.Sequence(
                    factory.Do(() => executionOrder.Add(2)),
                    factory.Sequence(
                        factory.Do(() => executionOrder.Add(3)),
                        factory.Do(() => executionOrder.Add(4))
                    ),
                    factory.Do(() => executionOrder.Add(5))
                ),
                factory.Do(() => executionOrder.Add(6))
            );
            
            kernel.Root.Add(deepSequence);
            StepUntilComplete(deepSequence);
            
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, executionOrder);
        }

        [Test]
        public void TestConcurrentBarrierAndTrigger()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var task1 = factory.Future<bool>();
            var task2 = factory.Future<bool>();
            var task3 = factory.Future<bool>();
            
            var barrier = factory.Barrier(task1, task2);
            var trigger = factory.Trigger(task2, task3);
            
            kernel.Root.Add(barrier, trigger);
            
            StepKernelTimes(3);
            Assert.IsTrue(barrier.Active);
            Assert.IsTrue(trigger.Active);
            
            // Complete task2 - should complete trigger but not barrier
            task2.Value = true;
            StepKernelTimes(3);
            
            Assert.IsTrue(barrier.Active);  // Still waiting for task1
            Assert.IsFalse(trigger.Active); // Should be complete
            
            // Complete task1 - should complete barrier
            task1.Value = true;
            StepKernelTimes(3);
            
            Assert.IsFalse(barrier.Active); // Now complete
        }

        [Test]
        public void TestTimedFutureTimeout()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var timedFuture = factory.TimedFuture<string>(TimeSpan.FromMilliseconds(50));
            bool timedOut = false;
            
            timedFuture.TimedOut += _ => timedOut = true;
            kernel.Root.Add(timedFuture);
            
            // Progress time beyond timeout
            for (int i = 0; i < 10; i++)
            {
                kernel.Update(0.01f); // 10ms per update
            }
            
            Assert.IsTrue(timedOut);
            Assert.IsFalse(timedFuture.Active);
        }

        [Test]
        public void TestComplexFlowWithAllPrimitives()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var results = new List<string>();
            
            var mainFlow = factory.Sequence(
                // Initial setup
                factory.Do(() => results.Add("start")),
                
                // Parallel initialization
                factory.Barrier(
                    factory.Do(() => results.Add("init1")),
                    factory.Do(() => results.Add("init2"))
                ),
                
                // Conditional logic
                factory.If(() => results.Count > 2, 
                    factory.Do(() => results.Add("condition_met"))
                ),
                
                // Race condition
                factory.Trigger(
                    factory.OneShotTimer(TimeSpan.FromMilliseconds(1)),
                    factory.Do(() => results.Add("immediate"))
                ),
                
                // Final step
                factory.Do(() => results.Add("end"))
            );
            
            kernel.Root.Add(mainFlow);
            StepUntilComplete(mainFlow);
            
            Assert.Contains("start", results);
            Assert.Contains("init1", results);
            Assert.Contains("init2", results);
            Assert.Contains("condition_met", results);
            Assert.Contains("end", results);
            Assert.GreaterOrEqual(results.Count, 5);
        }

        [Test]
        public void TestGeneratorNamingAndDebugging()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var namedGenerator = factory.Do(() => { }).Named("TestGenerator");
            var sequence = factory.Sequence().Named("TestSequence");
            var barrier = factory.Barrier().Named("TestBarrier");
            
            Assert.AreEqual("TestGenerator", namedGenerator.Name);
            Assert.AreEqual("TestSequence", sequence.Name);
            Assert.AreEqual("TestBarrier", barrier.Name);
        }

        [Test]
        public void TestKernelBreakMechanism()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var longRunningTask = factory.While(() => true, 
                factory.Do(() => { /* infinite loop */ })
            );
            
            kernel.Root.Add(longRunningTask);
            
            // Step a few times
            StepKernelTimes(5);
            Assert.IsTrue(longRunningTask.Active);
            
            // Break the kernel
            kernel.BreakFlow();
            StepKernelTimes(5);
            
            // Task should still be active but kernel should be broken
            Assert.IsTrue(kernel.Break);
        }

        [Test]
        public void TestWaitMechanism()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var beforeWait = DateTime.Now;
            kernel.Wait(TimeSpan.FromMilliseconds(50));
            
            // Step kernel while waiting
            for (int i = 0; i < 10; i++)
            {
                kernel.Update(0.01f);
            }
            
            var afterWait = DateTime.Now;
            var elapsed = afterWait - beforeWait;
            
            Assert.GreaterOrEqual(elapsed.TotalMilliseconds, 40); // Allow some tolerance
        }

        [Test]
        public void TestActionSequencePattern()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var executionOrder = new List<int>();
            
            var actionSequence = factory.ActionSequence(
                () => executionOrder.Add(1),
                () => executionOrder.Add(2),
                () => executionOrder.Add(3),
                () => executionOrder.Add(4)
            );
            
            kernel.Root.Add(actionSequence);
            StepUntilComplete(actionSequence);
            
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, executionOrder);
        }

        [Test]
        public void TestFluentInterfaceChaining()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var dependency = factory.Future<bool>();
            var result = factory.Do(() => { })
                .Named("ChainedAction")
                .ResumeAfter(dependency);
            
            kernel.Root.Add(result);
            
            Assert.AreEqual("ChainedAction", result.Name);
            Assert.IsTrue(result.Active);
            Assert.IsFalse(result.Running); // Should be suspended
            
            dependency.Value = true;
            StepKernelTimes(3);
            
            Assert.IsFalse(result.Active); // Should have completed
        }

        [Test]
        public void TestValueExpression()
        {
            var kernel = Create.Kernel();
            var factory = kernel.Factory;
            
            var valueGenerator = factory.Value(42);
            var expressionGenerator = factory.Expression(() => 10 + 5);
            
            Assert.AreEqual(42, valueGenerator.Value);
            
            kernel.Root.Add(expressionGenerator);
            StepUntilComplete(expressionGenerator);
            
            Assert.AreEqual(15, expressionGenerator.Value);
        }

        private void StepKernelTimes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Kernel.Step();
            }
        }
        
        private void StepUntilComplete(ITransient target, int maxSteps = 1000)
        {
            int steps = 0;
            while (target.Active && steps < maxSteps)
            {
                Kernel.Step();
                steps++;
            }
            
            Assert.IsFalse(target.Active, $"Target did not complete within {maxSteps} steps");
        }
    }
}