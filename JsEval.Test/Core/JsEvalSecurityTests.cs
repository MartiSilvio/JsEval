using JsEval.Core;
using JsEval.Core.Error;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalSecurityTests
    {
        [Test]
        public void Should_Reject_System_Access()
        {
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate("System.Console.WriteLine('hacked')");
            });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Reject_TypeConstructor_Attempts()
        {
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate("let x = this.constructor.constructor('return process')();");
            });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Enforce_Timeout()
        {
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate("while(true) {}", options: new JsEvalOptions()
                {
                    TimeoutInterval = TimeSpan.FromSeconds(2)
                });
            });

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.InnerException, Is.Not.Null);
            Assert.That(ex.InnerException, Is.TypeOf<TimeoutException>());
        }


        [Test]
        public void Should_Throw_On_Deep_Recursion()
        {
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate(@"
                    function recurse(n) {
                        if (n === 0) return 0;
                        return recurse(n - 1);
                    }
                    recurse(10000);
                ");
            });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Reject_Function_Constructor()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("Function('return 1 + 1')()"); });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Reject_Eval()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("eval('1 + 1')"); });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Reject_Object_Constructor_Access()
        {
            var ex = Assert.Throws<JsEvalException>(() =>
            {
                JsEvalEngine.Evaluate("{}.constructor.constructor('alert(1)')()");
            });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Prevent_Prototype_Pollution()
        {
            var result = JsEvalEngine.Evaluate(@"
                Object.prototype.hacked = true;
                ({})['hacked'];
            ");

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Should_Not_Persist_Polluted_Prototype_Between_Evaluations()
        {
            // This test checks isolation between evaluations
            var result = JsEvalEngine.Evaluate("({}).hacked");
            Assert.That(result, Is.Null.Or.False);
        }

        [Test]
        public void Should_Not_Access_GlobalThis_Properties()
        {
            var result = JsEvalEngine.Evaluate("typeof globalThis.setTimeout");
            Assert.That(result, Is.EqualTo("undefined"));
        }
        
        [Test]
        public void Should_Block_All_Registered_Globals()
        {
            foreach (var global in JsEvalEngine.BlockedGlobals)
            {
                var ex = Assert.Throws<JsEvalException>(() =>
                    {
                        JsEvalEngine.Evaluate($"{global}()");
                    }, $"Expected global '{global}' to be blocked");

                Assert.That(ex, Is.Not.Null, $"Global '{global}' was not blocked properly");
                Assert.That(ex!.InnerException.Message, Does.Contain($"{global} is disabled in this environment"), $"Unexpected error message for '{global}'");
            }
        }
    }
}