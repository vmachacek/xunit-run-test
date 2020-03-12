using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;

namespace TestRunner
{
    class SmokeTestRunner
    {
        static object consoleLock = new object();

        static ManualResetEvent finished = new ManualResetEvent(false);

        static async Task Main(string[] args)
        {
            var dll = "FakeTests.dll";

            var xunit = new XunitFrontController(AppDomainSupport.Denied, dll);

            var discoverySink = new TestDiscoverySink();

            xunit.Find(true, discoverySink, TestFrameworkOptions.ForDiscovery());


            discoverySink.Finished.WaitOne();

            var cases = discoverySink.TestCases;

            if (cases.Count == 0)
            {
                throw new Exception("No test discovered in " + dll);
            }

            var messagesSink = new TestMessageSink();

            // vvv THIS IS NEVER EXECUTED vvv
            messagesSink.Runner.TestExecutionSummaryEvent += Runner_TestExecutionSummaryEvent;

            // vvv THIS IS NEVER EXECUTED vvv
            messagesSink.Runner.TestAssemblyExecutionFinishedEvent += Runner_TestAssemblyExecutionFinishedEvent;

            messagesSink.Execution.TestFinishedEvent += Execution_TestFinishedEvent;

            //run
            xunit.RunAll(messagesSink, TestFrameworkOptions.ForDiscovery(), TestFrameworkOptions.ForExecution());

            finished.WaitOne();
            finished.Dispose();
        }

        private static void Runner_TestAssemblyExecutionFinishedEvent(MessageHandlerArgs<ITestAssemblyExecutionFinished> args)
        {
            // <------ THIS IS NEVER CALLED

            lock (consoleLock)
                Console.WriteLine($"{args.Message.ExecutionSummary}");

            finished.Set(); // <------ THIS IS NEVER CALLED
        }

        private static void Execution_TestFinishedEvent(MessageHandlerArgs<ITestFinished> args)
        {
            lock (consoleLock)
                Console.WriteLine($"{args.Message.Test.DisplayName} finished");
        }

        private static void Runner_TestExecutionSummaryEvent(MessageHandlerArgs<ITestExecutionSummary> info)
        {
            lock (consoleLock)
            {
                foreach (var summary in info.Message.Summaries)
                {
                    Console.WriteLine($"{summary.Key} {summary.Value}");
                }
            }

            finished.Set();
        }
    }
}
