using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalArgumentMappingTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            JsEvalFunctionRegistry.ClearAll();
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(ArgumentMappingFunctions));
        }

        [TestCase("echoInt(42)", 42)]
        [TestCase("echoDouble(3.14)", 3.14)]
        [TestCase("echoBool(true)", true)]
        [TestCase("echoBool(false)", false)]
        [TestCase("echoString('test')", "test")]
        [TestCase("echoNullable(null)", "null")]
        [TestCase("echoArrayLength(['a', 'b', 'c'])", 3)]
        [TestCase("echoUserName({ Name: 'Silvio' })", "Silvio")]
        public void Should_Map_JavaScript_Arguments_To_CSharp_Types(string expression, object expected)
        {
            var result = JsEvalEngine.Evaluate(expression);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Throw_When_Type_Cannot_Be_Mapped()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("echoInt('not-a-number')"); });

            Assert.That(ex, Is.Not.Null);
        }
    }

    public class ArgumentMappingFunctions
    {
        [JsEvalFunction("echoInt")]
        public static int EchoInt(int x) => x;

        [JsEvalFunction("echoDouble")]
        public static double EchoDouble(double x) => x;

        [JsEvalFunction("echoBool")]
        public static bool EchoBool(bool x) => x;

        [JsEvalFunction("echoString")]
        public static string EchoString(string x) => x;

        [JsEvalFunction("echoNullable")]
        public static string EchoNullable(string? x) => x ?? "null";

        [JsEvalFunction("echoArrayLength")]
        public static int EchoArrayLength(string[] arr) => arr.Length;

        [JsEvalFunction("echoUserName")]
        public static string EchoUserName(User user) => user.Name;
    }
}