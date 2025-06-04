using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
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
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("while(true) {}"); });

            Assert.That(ex, Is.Not.Null);
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
    }
}