using NUnit.Framework;
using JsEval.Core;
using System;
using JsEval.Core.Registry;

namespace JsEval.Test.Builtins
{
    [TestFixture]
    public class MiscFunctionTests
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(JsEval.Core.Builtins.MiscFunctions));
        }

        [TestFixture]
        public class GuidFunction
        {
            [Test]
            public void Should_Parse_Valid_Guid()
            {
                var input = "ae6f2b36-4a2d-4b88-9f96-2b1f963c02d2";
                var result = JsEvalEngine.Evaluate($"guid('{input}')");

                Assert.That(result, Is.EqualTo(Guid.Parse(input)));
            }

            [Test]
            public void Should_Throw_On_Invalid_Guid()
            {
                var ex = Assert.Throws<JsEvalException>(() =>
                {
                    JsEvalEngine.Evaluate("guid('not-a-guid')");
                });

                Assert.Multiple(() =>
                {
                    Assert.That(ex!.InnerException?.Message, Does.Contain("not-a-guid"));
                    Assert.That(ex.InnerException?.Message, Does.Contain("is not a valid GUID"));
                });
            }

            [Test]
            public void Should_Throw_On_Empty_String()
            {
                var ex = Assert.Throws<JsEvalException>(() =>
                {
                    JsEvalEngine.Evaluate("guid('')");
                });

                Assert.That(ex!.InnerException?.Message, Does.Contain("A non-empty GUID string argument is required"));
            }

            [Test]
            public void Should_Throw_On_Missing_Argument()
            {
                var ex = Assert.Throws<JsEvalException>(() =>
                {
                    JsEvalEngine.Evaluate("guid()");
                });

                Assert.That(ex!.InnerException?.Message ?? ex.Message, Does.Contain("argument"));
            }
        }
    }
}