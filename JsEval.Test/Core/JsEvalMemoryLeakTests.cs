using System.Diagnostics;
using JsEval.Core;
using JsEval.Core.Registry;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalMemoryLeakTests
    {
        // A simple JS snippet that allocates some objects but returns a small primitive.
        // In a real‐world scenario, you might call generateLargeString(), or build an array, etc.
        private const string AllocateSmallObjectsJs = @"
            (function () {
                // Allocate a JS object with a moderate number of properties
                var obj = {};
                for (var i = 0; i < 5000; i++) {
                    obj['prop' + i] = i;
                }
                return 42; // return a small primitive so C# side doesn’t hold onto a big result
            })()
        ";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Just in case any previous functions/modules remain
            JsEvalFunctionRegistry.ClearAll();
        }

        [Test]
        public void Repeated_Evaluations_Do_Not_Leak_Memory()
        {
            // If you want to measure total process memory:
            var process = Process.GetCurrentProcess();

            // Do one initial run to “warm up” and let any JIT/initialization happen:
            JsEvalEngine.Evaluate(AllocateSmallObjectsJs);

            // Force a full GC, wait a moment, then measure baseline:
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Thread.Sleep(100);

            var baseline = process.PrivateMemorySize64;
            TestContext.WriteLine($"[Baseline PrivateMemorySize64] {baseline:N0} bytes");

            // Now do N iterations, and periodically re‐measure:
            const int totalIterations = 200;
            const int measureEvery = 25;
            var maxObserved = baseline;

            for (var i = 1; i <= totalIterations; i++)
            {
                JsEvalEngine.Evaluate(AllocateSmallObjectsJs);

                if (i % measureEvery == 0)
                {
                    // Force a GC again to clean up any short‐lived things
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    Thread.Sleep(50); // give the OS a moment

                    process.Refresh();
                    var current = process.PrivateMemorySize64;
                    maxObserved = Math.Max(maxObserved, current);
                    TestContext.WriteLine($"Iteration {i}: PrivateMemorySize64 = {current:N0} bytes");
                }
            }

            // After the loop, allow some wiggle‐room. CI runners tend to report larger process deltas.
            var isCi = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);
            var allowedDelta = isCi ? 32L * 1024 * 1024 : 5L * 1024 * 1024; // 32 MB on CI, 5 MB locally

            Assert.LessOrEqual(
                maxObserved - baseline,
                allowedDelta,
                $"Process memory grew by more than {allowedDelta:N0} bytes after {totalIterations} evaluations. Possible leak?"
            );
        }
    }
}
