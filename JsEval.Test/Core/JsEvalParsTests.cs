using System.Dynamic;
using Jint.Runtime;
using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Error;
using JsEval.Core.Registry;

namespace JsEval.Test.Core
{
    public class Child
    {
        public int Val { get; set; }
    }

    public static class ParsFunctions
    {
        [JsEvalFunction("sumArray")]
        public static int SumArray(int[] arr) => arr.Sum();

        [JsEvalFunction("getChildVal")]
        public static int GetChildVal(Child child) => child.Val;
    }

    [TestFixture]
    public class JsEvalParsTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(TestFunctions));
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(UserModule));
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(ParsFunctions));
        }

        [Test]
        public void Should_Invoke_With_Pars_Properties()
        {
            dynamic pars = new ExpandoObject();
            pars.a = 2;
            pars.b = 3;
            var result = JsEvalEngine.Evaluate("add(pars.a, pars.b)", pars);
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void Should_Invoke_Module_Function_With_Pars()
        {
            dynamic pars = new ExpandoObject();
            pars.name = "zoe";
            var result = JsEvalEngine.Evaluate("User.GetByKey(pars.name)", pars);
            Assert.That(result, Is.Not.Null);
            var user = result!;
            Assert.That(user.Name, Is.EqualTo("zoe"));
        }

        [Test]
        public void Should_Sum_Array_From_Pars()
        {
            dynamic pars = new ExpandoObject();
            pars.values = new[] { 1, 2, 3, 4 };
            var result = JsEvalEngine.Evaluate("sumArray(pars.values)", pars);
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void Should_Handle_Nested_Pars_Object()
        {
            dynamic child = new ExpandoObject();
            child.Val = 7;
            dynamic pars = new ExpandoObject();
            pars.child = child;
            var result = JsEvalEngine.Evaluate("getChildVal(pars.child)", pars);
            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        public void Should_Throw_On_Missing_Pars_Property()
        {
            dynamic pars = new ExpandoObject();
            pars.x = 1;
            var ex = Assert.Throws<JsEvalException>(
                () => JsEvalEngine.Evaluate("add(pars.y, 5)", pars)
            );
            Assert.That(ex!.InnerException, Is.TypeOf<JavaScriptException>());
        }

        [Test]
        public void Should_Invoke_TopLevel_Function_Without_Pars()
        {
            var result = JsEvalEngine.Evaluate("greet('Test')");
            Assert.That(result, Is.EqualTo("Hello, Test!"));
        }
    }
}
