using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
{
    [TestFixture]
    public class JsEvalFunctionTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(TestFunctions));
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(UserModule));
        }

        [TestCase("add(2, 3)", 5)]
        [TestCase("greet('Silvio')", "Hello, Silvio!")]
        public void Should_Invoke_TopLevel_Functions(string expression, object expected)
        {
            var result = JsEvalEngine.Evaluate(expression);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Invoke_TopLevel_Function_Returning_Object()
        {
            var result = JsEvalEngine.Evaluate("getUser('alice')");
            Assert.That(result, Is.Not.Null);

            dynamic user = result!;
            Assert.That(user.Name, Is.EqualTo("alice"));
        }

        [Test]
        public void Should_Invoke_Module_Function()
        {
            var result = JsEvalEngine.Evaluate("User.GetByKey('mark')");
            Assert.That(result, Is.Not.Null);

            dynamic user = result!;
            Assert.That(user.Name, Is.EqualTo("mark"));
        }
    }

    public class User
    {
        public string Name { get; set; } = default!;
    }

    public class TestFunctions
    {
        [JsEvalFunction("add")]
        public static int Add(int a, int b) => a + b;

        [JsEvalFunction("greet")]
        public static string Greet(string name) => $"Hello, {name}!";

        [JsEvalFunction("getUser")]
        public static User GetUser(string name) => new() { Name = name };
    }

    [JsEvalModule("User")]
    public static class UserModule
    {
        [JsEvalFunction("GetByKey")]
        public static User GetByKey(string key) => new() { Name = key };
    }
}