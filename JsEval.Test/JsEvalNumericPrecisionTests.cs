using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
{
    [TestFixture]
    public class JsEvalNumericPrecisionTests
    {
        [TestCase("0.1 + 0.2", 0.30000000000000004)]
        [TestCase("0.3 - 0.1", 0.19999999999999998)]
        [TestCase("0.1 + 0.7 + 0.2", 1.0)]
        [TestCase("1.005 * 100", 100.5)]
        [TestCase("1.005 * 100 / 100", 1.005)]
        [TestCase("Math.round(1.005 * 100) / 100", 1.0)]
        [TestCase("1e+21 + 1", 1e+21)]
        [TestCase("1e-21 + 1", 1.000000000000000000001)]
        [TestCase("0.0000001 + 0.0000002", 0.0000003)]
        [TestCase("Math.pow(2, 53)", 9007199254740992.0)]
        [TestCase("Math.pow(2, -53)", 1.1102230246251565e-16)]
        [TestCase("Number.MAX_VALUE", double.MaxValue)]
        [TestCase("Number.MIN_VALUE", double.Epsilon)]
        [TestCase("Number.POSITIVE_INFINITY", double.PositiveInfinity)]
        [TestCase("Number.NEGATIVE_INFINITY", double.NegativeInfinity)]
        public void Should_Handle_Precision_Edge_Cases(string expression, object expected)
        {
            var result = JsEvalEngine.Evaluate(expression);

            if (expected is double expectedDouble && result is double actualDouble)
            {
                Assert.That(actualDouble, Is.EqualTo(expectedDouble).Within(1e-12));
            }
            else
            {
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void Should_Handle_Negative_Zero()
        {
            var result = JsEvalEngine.Evaluate("-0");
            Assert.That(result, Is.TypeOf<double>());

            var bits = BitConverter.DoubleToInt64Bits((double)result!);
            Assert.That(bits < 0, "Expected a negative zero representation");
        }

        [Test]
        public void Should_Handle_Rounding_And_Truncating()
        {
            Assert.That(JsEvalEngine.Evaluate("Math.floor(2.9)"), Is.EqualTo(2));
            Assert.That(JsEvalEngine.Evaluate("Math.ceil(2.1)"), Is.EqualTo(3));
            Assert.That(JsEvalEngine.Evaluate("Math.round(2.5)"), Is.EqualTo(3));
            Assert.That(JsEvalEngine.Evaluate("Math.trunc(-2.9)"), Is.EqualTo(-2));
        }

        [Test]
        public void Should_Detect_Unsafe_Integer_Overflow()
        {
            var result = JsEvalEngine.Evaluate("Number.MAX_SAFE_INTEGER + 1 === Number.MAX_SAFE_INTEGER + 2");
            Assert.That(result, Is.EqualTo(true));
        }
    }
}