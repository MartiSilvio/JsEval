using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
{
    [TestFixture]
    public class JsEvalHigherOrderTests
    {
        [TestCase("[1, 2, 3].map(x => x * 2)", new object[] { 2, 4, 6 })]
        [TestCase("[5, 10, 15].filter(x => x > 7)", new object[] { 10, 15 })]
        [TestCase("[1, 2, 3, 4].reduce((a, b) => a + b, 0)", 10)]
        [TestCase("[1, 2, 3].some(x => x > 2)", true)]
        [TestCase("[1, 2, 3].every(x => x < 5)", true)]
        [TestCase("[1, 2, 3].find(x => x > 1)", 2)]
        public void Should_Evaluate_HigherOrder_Array_Functions(string expression, object expected)
        {
            var result = JsEvalEngine.Evaluate(expression);

            if (expected is object[] expectedArray)
            {
                CollectionAssert.AreEqual(expectedArray, (System.Collections.IEnumerable)result!);
            }
            else
            {
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void Should_Evaluate_Inline_Arrow_Function()
        {
            var result = JsEvalEngine.Evaluate("(x => x * x)(5)");
            Assert.That(result, Is.EqualTo(25));
        }

        [Test]
        public void Should_Evaluate_Function_Returning_Function()
        {
            var result = JsEvalEngine.Evaluate(@"
                function multiplier(factor) {
                    return x => x * factor;
                }
                let double = multiplier(2);
                double(4);
            ");
            Assert.That(result, Is.EqualTo(8));
        }
    }
}