using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;
using System.Collections.Generic;
using System.Linq;
using JsEval.Core.Error;
using NUnit.Framework;

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
        [TestCase("echoListSum([1, 2, 3, 4, 5])", 15)]
        [TestCase("echoColor('Red')", "Red")]
        [TestCase("echoColor('Green')", "Green")]
        [TestCase("echoColor('Blue')", "Blue")]
        [TestCase("echoColorList(['Red', 'Green', 'Blue'])", 3)]
        public void Should_Map_JavaScript_Arguments_To_CSharp_Types(string expression, object expected)
        {
            var result = JsEvalEngine.Evaluate(expression);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Throw_When_Type_Cannot_Be_Mapped()
        {
            var ex = Assert.Throws<JsEvalException>(() => JsEvalEngine.Evaluate("echoInt('not-a-number')"));
            Assert.That(ex, Is.Not.Null);
        }
    }

    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public class Person
    {
        public string Name { get; set; } = "";
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
        public static string EchoUserName(Person person) => person.Name;

        [JsEvalFunction("echoListSum")]
        public static int EchoListSum(IEnumerable<int> numbers) => numbers.Sum();

        [JsEvalFunction("echoColor")]
        public static string EchoColor(Color color) => color.ToString();

        [JsEvalFunction("echoColorList")]
        public static int EchoColorList(IEnumerable<Color> colors) => colors.Count();
    }
}