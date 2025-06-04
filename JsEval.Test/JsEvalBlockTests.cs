using NUnit.Framework;
using JsEval.Core;

namespace JsEval.Test
{
    [TestFixture]
    public class JsEvalBlockTests
    {
        [TestCase(@"
            let a = 2;
            let b = 3;
            a + b;", 5)]
        [TestCase(@"
            let x = 10;
            x = x * 2;
            x;", 20)]
        [TestCase(@"
            let total = 0;
            for (let i = 1; i <= 3; i++) {
                total += i;
            }
            total;", 6)]
        [TestCase(@"
            let result = '';
            if (5 > 3) {
                result = 'yes';
            } else {
                result = 'no';
            }
            result;", "yes")]
        [TestCase(@"
            let n = 3;
            let factorial = 1;
            while (n > 1) {
                factorial *= n;
                n--;
            }
            factorial;", 6)]
        public void Should_Evaluate_MultiLine_Blocks(string script, object expected)
        {
            var result = JsEvalEngine.Evaluate(script);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}