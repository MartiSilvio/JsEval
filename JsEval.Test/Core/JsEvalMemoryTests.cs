using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Error;
using JsEval.Core.Registry;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalMemoryTests
    {
        // Helper class: exposes two JS-callable functions that return large strings.
        private static class MemoryTestFunctions
        {
            // ~10_000_000 characters (~20 MB of UTF-16 data)
            [JsEvalFunction("generateLargeString")]
            public static string GenerateLargeString()
            {
                const int size = 10_000_000;
                return new string('L', size);
            }

            // ~500_000 characters (~1 MB of UTF-16 data)
            [JsEvalFunction("generateMediumString")]
            public static string GenerateMediumString()
            {
                const int size = 500_000;
                return new string('M', size);
            }
        }

        // A simple ES5-compatible snippet that builds an array of 1 000 000 integers
        // and returns its length. Using ES5 syntax avoids “unexpected token” errors in Jint.
        private const string BuildLargeArrayLengthJs = @"
            (function () {
                var arr = [];
                for (var i = 0; i < 1000000; i++) {
                    arr.push(i);
                }
                return arr.length;
            })()
        ";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Clear any previously registered functions/modules, then register our two helpers.
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(MemoryTestFunctions));
        }

        // ----------------------------------------------------------------------
        // TESTS FOR C#-SIDE STRING ALLOCATIONS (which Jint does NOT count toward MEMORY_LIMIT_BYTES)
        // ----------------------------------------------------------------------

        [Test]
        public void DefaultLimit_Allows_GenerateLargeString()
        {
            // By default, MEMORY_LIMIT_BYTES = 2 000 000 (≈2 MB).
            // generateLargeString() returns ~20 MB of data, but Jint does NOT count pure
            // C# string allocations toward that limit. So this should succeed.
            var result = JsEvalEngine.Evaluate("generateLargeString()");
            Assert.That(result, Is.InstanceOf<string>(), "Default memory limit does not reject a pure string allocation.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "Default limit: returned string must have exactly 10 000 000 characters.");
        }

        [Test]
        public void DefaultLimit_Allows_GenerateMediumString()
        {
            // generateMediumString produces ~1 MB, which is within the default 2 MB limit.
            var result = JsEvalEngine.Evaluate("generateMediumString()");
            Assert.That(result, Is.InstanceOf<string>(), "Expected a string result under default memory limit.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(500_000),
                "Default limit: should return exactly 500 000 characters.");
        }

        [Test]
        public void ExplicitLowLimit_Allows_GenerateLargeString()
        {
            // Even if we set MemoryLimitBytes to 5 000 000 (≈5 MB),
            // generateLargeString() (~20 MB) still succeeds because Jint does NOT count pure
            // C# string allocations against the JS-heap limit.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 5_000_000
            };

            var result = JsEvalEngine.Evaluate("generateLargeString()", serviceProvider: null, options: options);
            Assert.That(result, Is.InstanceOf<string>(), "Low memory limit does not block a pure string allocation.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "Explicit low limit: returned string must have 10 000 000 characters, since strings aren’t counted.");
        }

        [Test]
        public void ExplicitHighLimit_Allows_GenerateLargeString()
        {
            // Even with a high limit (e.g., 25 000 000 bytes), behavior is the same:
            // C# string returns are not counted. We just document that it passes.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 25_000_000
            };

            var result = JsEvalEngine.Evaluate("generateLargeString()", serviceProvider: null, options: options);
            Assert.That(result, Is.InstanceOf<string>(), "High memory limit returns the string as expected.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "High limit: returned string must have 10 000 000 characters.");
        }

        [Test]
        public void ZeroMemoryLimit_Ignored_Allows_GenerateLargeString()
        {
            // MemoryLimitBytes = 0 means “ignore any cap.” A ~20 MB string should still succeed.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 0
            };

            var result = JsEvalEngine.Evaluate("generateLargeString()", serviceProvider: null, options: options);
            Assert.That(result, Is.InstanceOf<string>(), "Zero limit is treated as “no cap,” returning the string.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "With limit=0, returned string must have 10 000 000 characters.");
        }

        [Test]
        public void NegativeMemoryLimit_TreatedAsZero_Allows_GenerateLargeString()
        {
            // Negative MemoryLimitBytes is treated like zero: ignore cap, so string succeeds.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = -1
            };

            var result = JsEvalEngine.Evaluate("generateLargeString()", serviceProvider: null, options: options);
            Assert.That(result, Is.InstanceOf<string>(), "Negative limit becomes “no cap,” so string returns.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "With limit<0, returned string must have 10 000 000 characters.");
        }

        [Test]
        public void NoMemoryLimitSpecified_UsesDefault_Allows_GenerateLargeString()
        {
            // Omitting JsEvalOptions uses default behavior: since default does NOT reject pure strings,
            // it should succeed here as well.
            var result = JsEvalEngine.Evaluate("generateLargeString()", serviceProvider: null, options: null);
            Assert.That(result, Is.InstanceOf<string>(), "Omitting options uses default—and default allows large strings.");
            var str = (string)result!;
            Assert.That(str.Length, Is.EqualTo(10_000_000),
                "Omitting options: returned string must have 10 000 000 characters.");
        }

        // ----------------------------------------------------------------------
        // TESTS FOR JS-SIDE HEAP ALLOCATIONS (arrays/objects), which Jint DOES count toward MEMORY_LIMIT_BYTES
        // ----------------------------------------------------------------------

        [Test]
        public void DefaultLimit_Should_Fail_On_BuildLargeArrayLength()
        {
            // By default, MEMORY_LIMIT_BYTES = 2 000 000 (≈2 MB).
            // Building a JS array of 1 000 000 integers should exceed that and throw.
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate(BuildLargeArrayLengthJs);
            });

            Assert.That(ex, Is.Not.Null, "Default memory limit should reject a large JS-heap allocation.");
        }

        [Test]
        public void ExplicitLowLimit_Should_Fail_On_BuildLargeArrayLength()
        {
            // Lower the limit to 1 000 000 bytes (≈1 MB). Still too low for a 1 000 000-element array.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 1_000_000
            };

            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate(BuildLargeArrayLengthJs, serviceProvider: null, options: options);
            });

            Assert.That(ex, Is.Not.Null, "Low memory limit should reject building a 1 000 000-element JS array.");
        }

        [Test]
        public void ExplicitHighLimit_Should_Succeed_On_BuildLargeArrayLength()
        {
            // Bump the memory limit high enough for a JS array of 1 000 000 integers:
            // Using 200_000_000 bytes (≈200 MB) to exceed the ~100 MB actual usage.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 200_000_000 // 200 MB
            };

            var result = JsEvalEngine.Evaluate(BuildLargeArrayLengthJs, serviceProvider: null, options: options);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<double>(), "When memory limit is high, returning the length should succeed.");
                Assert.That((double)result!, Is.EqualTo(1_000_000),
                    "The JS array should contain exactly 1 000 000 elements.");
            });
        }

        [Test]
        public void ZeroMemoryLimit_Ignored_Allows_BuildLargeArrayLength()
        {
            // MemoryLimitBytes = 0 → ignore cap. Building a 1 000 000-element array should succeed.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = 0
            };

            var result = JsEvalEngine.Evaluate(BuildLargeArrayLengthJs, serviceProvider: null, options: options);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<double>(), "Zero memory limit should be treated as “no cap,” allowing large array.");
                Assert.That((double)result!, Is.EqualTo(1_000_000),
                    "With limit=0, the JS array should have 1 000 000 elements.");
            });
        }

        [Test]
        public void NegativeMemoryLimit_TreatedAsZero_Allows_BuildLargeArrayLength()
        {
            // Negative MemoryLimitBytes is treated like zero: ignore cap, so large array succeeds.
            var options = new JsEvalOptions
            {
                MemoryLimitBytes = -5
            };

            var result = JsEvalEngine.Evaluate(BuildLargeArrayLengthJs, serviceProvider: null, options: options);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<double>(), "Negative limit behaves as “no cap,” allowing large array.");
                Assert.That((double)result!, Is.EqualTo(1_000_000),
                    "With negative limit, the JS array should have 1 000 000 elements.");
            });
        }

        [Test]
        public void NoMemoryLimitSpecified_UsesDefault_Fails_On_BuildLargeArrayLength()
        {
            // Omitting JsEvalOptions uses default: building the array should fail under 2 MB cap.
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate(BuildLargeArrayLengthJs, serviceProvider: null, options: null);
            });

            Assert.That(ex, Is.Not.Null, "Default (no options) should reject large JS-heap array.");
        }
    }
}
