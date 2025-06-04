using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalObjectLiteralTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(LiteralHelpers));
        }

        [Test]
        public void Should_Evaluate_Simple_Object_Literal()
        {
            var expression = "({ name: 'Alice', age: 30 })";
            dynamic? result = JsEvalEngine.Evaluate(expression);

            Assert.That(result.name.ToString(), Is.EqualTo("Alice"));
            Assert.That((int)result.age, Is.EqualTo(30));
        }

        [Test]
        public void Should_Evaluate_Nested_Object_Literal()
        {
            var expression = @"
            ({
                user: {
                    name: 'Bob',
                    contact: {
                        email: 'bob@example.com',
                        phone: '123-456'
                    }
                }
            })";

            dynamic? result = JsEvalEngine.Evaluate(expression);
            Assert.That(result.user.name.ToString(), Is.EqualTo("Bob"));
            Assert.That(result.user.contact.email.ToString(), Is.EqualTo("bob@example.com"));
        }

        [Test]
        public void Should_Evaluate_Object_With_Inline_Function_Calls()
        {
            var expression = "({ greeting: greet('Mona'), total: add(4, 6) })";
            dynamic? result = JsEvalEngine.Evaluate(expression);

            Assert.That(result.greeting.ToString(), Is.EqualTo("Hello, Mona!"));
            Assert.That((int)result.total, Is.EqualTo(10));
        }

        [Test]
        public void Should_Evaluate_Object_With_Array()
        {
            const string expression = "({ items: [1, 2, 3, 4] })";
            dynamic? result = JsEvalEngine.Evaluate(expression);

            Assert.That(result.items.Length, Is.EqualTo(4));
            Assert.That((int)result.items[2], Is.EqualTo(3));
        }
    }

    public static class LiteralHelpers
    {
        [JsEvalFunction("greet")]
        public static string Greet(string name) => $"Hello, {name}!";

        [JsEvalFunction("add")]
        public static int Add(int a, int b) => a + b;
    }
}
