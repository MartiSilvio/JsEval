using JsEval.Core;
using JsEval.Core.Error;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalCancellationTests
    {
        [Test]
        public void Evaluate_ThrowsJsEvalException_WhenTokenIsCanceledBeforeEvaluation()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var ex = Assert.Throws<JsEvalException>(() =>
                JsEvalEngine.Evaluate("1 + 1", cancellationToken: tokenSource.Token));

            Assert.That(ex.InnerException, Is.TypeOf<Jint.Runtime.ExecutionCanceledException>());
        }

        [Test]
        public void Evaluate_ThrowsJsEvalException_WhenCanceledDuringLongLoop()
        {
            var tokenSource = new CancellationTokenSource(millisecondsDelay: 50);

            var ex = Assert.Throws<JsEvalException>(() =>
                JsEvalEngine.Evaluate(@"
                    let sum = 0;
                    for (let i = 0; i < 1e8; i++) {
                        sum += i;
                    }
                    sum;", cancellationToken: tokenSource.Token));

            Assert.That(ex.InnerException, Is.TypeOf<Jint.Runtime.ExecutionCanceledException>());
        }
    }
}