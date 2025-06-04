using System.Diagnostics;
using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;

namespace JsEval.Test.Benchmark
{
    [Explicit]
    [TestFixture]
    public class JsEvalOverheadBenchmark
    {
        // --------------------------------------------------------------------------------
        // 1) “Heavy” C# method that does nontrivial work—allocates and sums 10 million ints.
        // --------------------------------------------------------------------------------
        private static class HeavyFunctions
        {
            /// <summary>
            /// Allocates an int[10_000_000], fills it with values 1…10_000_000, and returns the sum.
            /// </summary>
            public static long SumTenMillion()
            {
                const int n = 10_000_000;
                var arr = new int[n];
                long sum = 0;

                // Fill the array
                for (var i = 0; i < n; i++)
                {
                    arr[i] = i + 1;
                }

                // Sum it up
                for (var i = 0; i < n; i++)
                {
                    sum += arr[i];
                }

                return sum;
            }
        }

        // -------------------------------------------------------------------
        // 2) Expose both SumTenMillion() and a trivial NoOp() to JsEvalEngine
        // -------------------------------------------------------------------
        private static class JsExposedFunctions
        {
            [JsEvalFunction("sumTenMillion")]
            public static long SumTenMillionProxy() => HeavyFunctions.SumTenMillion();

            [JsEvalFunction("noop")]
            public static int NoOp() => 42;
        }

        // -------------------------------------------------------------------
        // 3) Register our JS-callable functions once before any tests run
        // -------------------------------------------------------------------
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(JsExposedFunctions));
        }

        // -------------------------------------------------------------------
        // 4) The NUnit test that measures direct vs. JsEval invocation times
        // -------------------------------------------------------------------
        [Test]
        public void CompareDirectVsJsEvalOverhead()
        {
            const int iterations = 1_000;

            // ------------------------
            // Warm-up: Direct Summation
            // ------------------------
            TestContext.WriteLine("Warm-up: HeavyFunctions.SumTenMillion()");
            var directWarm = HeavyFunctions.SumTenMillion();
            TestContext.WriteLine($"  (returned {directWarm})");

            // -----------------------------
            // Warm-up: JsEval Summation Call
            // -----------------------------
            TestContext.WriteLine("Warm-up: JsEvalEngine.Evaluate(\"sumTenMillion()\")");
            var jsWarm = JsEvalEngine.Evaluate("sumTenMillion()");
            TestContext.WriteLine($"  (returned {jsWarm})");
            TestContext.WriteLine("");

            // ----------------------------------------
            // A) Measure Direct Calls to SumTenMillion
            // ----------------------------------------
            var swDirectHeavy = Stopwatch.StartNew();
            long lastDirectHeavy = 0;
            for (var i = 0; i < iterations; i++)
            {
                lastDirectHeavy = HeavyFunctions.SumTenMillion();
            }
            swDirectHeavy.Stop();
            var directHeavyTotalMs = swDirectHeavy.Elapsed.TotalMilliseconds;
            var directHeavyPerCall = directHeavyTotalMs / iterations;

            // ------------------------------------------
            // B) Measure JsEval Calls to sumTenMillion()
            // ------------------------------------------
            var swJsHeavy = Stopwatch.StartNew();
            object? lastJsHeavy = null;
            for (var i = 0; i < iterations; i++)
            {
                lastJsHeavy = JsEvalEngine.Evaluate("sumTenMillion()");
            }
            swJsHeavy.Stop();
            var jsHeavyTotalMs = swJsHeavy.Elapsed.TotalMilliseconds;
            var jsHeavyPerCall = jsHeavyTotalMs / iterations;

            // ------------------------------------------------
            // C) Measure Direct Calls to NoOp (minimal work)
            // ------------------------------------------------
            var swDirectNoop = Stopwatch.StartNew();
            var lastDirectNoop = 0;
            for (var i = 0; i < iterations; i++)
            {
                lastDirectNoop = JsExposedFunctions.NoOp();
            }
            swDirectNoop.Stop();
            var directNoopTotalMs = swDirectNoop.Elapsed.TotalMilliseconds;
            var directNoopPerCall = directNoopTotalMs / iterations;

            // ---------------------------------------------------
            // D) Measure JsEval Calls to noop() (Jint dispatch)
            // ---------------------------------------------------
            var swJsNoop = Stopwatch.StartNew();
            object? lastJsNoop = null;
            for (var i = 0; i < iterations; i++)
            {
                lastJsNoop = JsEvalEngine.Evaluate("noop()");
            }
            swJsNoop.Stop();
            var jsNoopTotalMs = swJsNoop.Elapsed.TotalMilliseconds;
            var jsNoopPerCall = jsNoopTotalMs / iterations;

            // ---------------------------------------------------------------------
            // 5) Output the results to TestContext (visible in NUnit test output logs)
            // ---------------------------------------------------------------------
            TestContext.WriteLine("=== Benchmark Results (Iterations = {0}) ===", iterations);
            TestContext.WriteLine("");
            TestContext.WriteLine("Heavy Function (SumTenMillion):");
            TestContext.WriteLine(
                $"  Direct C# call:   Total = {directHeavyTotalMs:F0} ms  |  Avg = {directHeavyPerCall:F2} ms/call  |  LastResult = {lastDirectHeavy}");
            TestContext.WriteLine(
                $"  JsEval call:      Total = {jsHeavyTotalMs:F0} ms  |  Avg = {jsHeavyPerCall:F2} ms/call  |  LastResult = {lastJsHeavy}");
            TestContext.WriteLine(
                $"  Overhead ratio:   { (jsHeavyPerCall / directHeavyPerCall):F2}× slower via Jint");
            TestContext.WriteLine("");

            TestContext.WriteLine("NoOp Function (trivial):");
            TestContext.WriteLine(
                $"  Direct C# call:   Total = {directNoopTotalMs:F0} ms  |  Avg = {directNoopPerCall:F4} ms/call  |  LastResult = {lastDirectNoop}");
            TestContext.WriteLine(
                $"  JsEval call:      Total = {jsNoopTotalMs:F0} ms  |  Avg = {jsNoopPerCall:F4} ms/call  |  LastResult = {lastJsNoop}");
            TestContext.WriteLine(
                $"  Overhead ratio:   { (jsNoopPerCall / directNoopPerCall):F2}× slower via Jint");
            TestContext.WriteLine("");

            // No assertions—this test passes if no exceptions occur. 
            // The timing results are available in the test logs for analysis.
            Assert.Pass("Benchmark completed; see test output for timing details.");
        }
    }
}
