using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
{
    [TestFixture]
    public class JsEvalEdgeCaseTests
    {
        [TestCase("null", null)]
        [TestCase("undefined", null)]
        [TestCase("0", 0)]
        [TestCase("''", "")]
        [TestCase("'   '", "   ")]
        [TestCase("!!0", false)]
        [TestCase("!!1", true)]
        [TestCase("!!''", false)]
        [TestCase("!!'a'", true)]
        [TestCase("!![]", true)]
        [TestCase("!!{}", true)]
        [TestCase("1 / 0", double.PositiveInfinity)]
        [TestCase("'5' * 2", 10)]
        [TestCase("'5' + 2", "52")]
        [TestCase("typeof null", "object")]
        public void Should_Evaluate_Common_Edge_Cases(string expression, object? expected)
        {
            var result = JsEvalEngine.Evaluate(expression);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Evaluate_NaN()
        {
            var result = JsEvalEngine.Evaluate("0 / 0");
            Assert.That(result, Is.TypeOf<double>());
            Assert.That(double.IsNaN((double)result!));
        }

        [Test]
        public void Should_Evaluate_Large_String_Concatenation()
        {
            var result = JsEvalEngine.Evaluate("'x'.repeat(10000)");
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(((string)result!).Length, Is.EqualTo(10000));
        }

        [Test]
        public void Should_Throw_On_Invalid_Syntax()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("let = 5"); });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Throw_On_Infinite_Loop()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("while(true) {}"); });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Throw_On_NonExistent_Function()
        {
            var ex = Assert.Throws<JsEvalException>(() => { JsEvalEngine.Evaluate("nonExistentFunction()"); });

            Assert.That(ex, Is.Not.Null);
        }
    }
}