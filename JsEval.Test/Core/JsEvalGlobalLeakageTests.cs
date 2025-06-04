using JsEval.Core;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalGlobalLeakageTests
    {
        [Test]
        public void Should_Not_Leak_Let_Variables()
        {
            var result = JsEvalEngine.Evaluate(@"
                {
                    let x = 42;
                }
                typeof x;
            ");
            Assert.That(result, Is.EqualTo("undefined"));
        }

        [Test]
        public void Should_Not_Leak_Const_Variables()
        {
            var result = JsEvalEngine.Evaluate(@"
                {
                    const y = 'secret';
                }
                typeof y;
            ");
            Assert.That(result, Is.EqualTo("undefined"));
        }

        [Test]
        public void Should_Leak_Var_Variables_To_Global_Scope()
        {
            var result = JsEvalEngine.Evaluate(@"
                {
                    var z = 100;
                }
                typeof z;
            ");
            Assert.That(result, Is.EqualTo("number")); // 'var' is function-scoped, not block-scoped
        }

        [Test]
        public void Should_Isolate_Global_Scope_Between_Evaluations()
        {
            // First script defines a global var
            JsEvalEngine.Evaluate("var polluted = 'yes';");

            // Second script should not see it
            var result = JsEvalEngine.Evaluate("typeof polluted;");
            Assert.That(result, Is.EqualTo("undefined"));
        }

        [Test]
        public void Should_Isolate_Let_Scope_In_Loops()
        {
            var result = JsEvalEngine.Evaluate(@"
                for (let i = 0; i < 3; i++) {}
                typeof i;
            ");
            Assert.That(result, Is.EqualTo("undefined"));
        }

        [Test]
        public void Should_Honor_Function_Scope()
        {
            var result = JsEvalEngine.Evaluate(@"
                function scoped() {
                    var inner = 1;
                }
                scoped();
                typeof inner;
            ");
            Assert.That(result, Is.EqualTo("undefined"));
        }
    }
}