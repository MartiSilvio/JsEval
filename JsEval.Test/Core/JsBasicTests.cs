using JsEval.Core;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsBasicTests
    {
        [TestFixture]
        public class JsEvalArithmeticTests
        {
            [TestCase("1 + 2", 3)]
            [TestCase("1 + 2 * 3", 7)]
            [TestCase("(1 + 2) * 3", 9)]
            [TestCase("10 - 3", 7)]
            [TestCase("20 / 4", 5)]
            [TestCase("2 ** 3", 8)] // ES6 exponentiation
            [TestCase("100 % 7", 2)]
            [TestCase("3.5 + 2.5", 6.0)]
            [TestCase("4.5 * 2", 9.0)]
            [TestCase("0.1 + 0.2", 0.30000000000000004)] // floating-point quirk
            public void Should_Evaluate_Arithmetic_Expressions(string expression, object expected)
            {
                var result = JsEvalEngine.Evaluate(expression);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class JsEvalLogicalExpressionTests
        {
            [TestCase("true && false", false)]
            [TestCase("true || false", true)]
            [TestCase("!true", false)]
            [TestCase("!false", true)]
            [TestCase("1 < 2", true)]
            [TestCase("5 >= 10", false)]
            [TestCase("3 == '3'", true)] // loose equality
            [TestCase("3 === '3'", false)] // strict equality
            [TestCase("null == undefined", true)]
            [TestCase("null === undefined", false)]
            [TestCase("5 != '5'", false)]
            [TestCase("5 !== '5'", true)]
            public void Should_Evaluate_Logical_And_Comparison_Expressions(string expression, object expected)
            {
                var result = JsEvalEngine.Evaluate(expression);
                Assert.That(result, Is.EqualTo(expected));
            }
        }
    }
}