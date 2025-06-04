using JsEval.Core;

namespace JsEval.Test.Core
{
    [TestFixture]
    public class JsEvalDestructuringTests
    {
        [TestCase("let [a, b] = [1, 2]; a + b;", 3)]
        [TestCase("let {x, y} = {x: 10, y: 5}; x * y;", 50)]
        [TestCase("let [a,,c] = [1, 2, 3]; a + c;", 4)]
        [TestCase("let {a: first, b: second} = {a: 3, b: 7}; first + second;", 10)]
        public void Should_Evaluate_Basic_Destructuring(string script, object expected)
        {
            var result = JsEvalEngine.Evaluate(script);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("let arr = [1, 2, 3]; let copy = [...arr]; copy.length;", 3)]
        [TestCase("let obj = { a: 1, b: 2 }; let newObj = { ...obj, c: 3 }; newObj.c;", 3)]
        [TestCase("function sum(...nums) { return nums.reduce((a, b) => a + b, 0); } sum(1, 2, 3);", 6)]
        [TestCase("let [head, ...tail] = [10, 20, 30]; tail.length;", 2)]
        public void Should_Evaluate_Spread_And_Rest_Syntax(string script, object expected)
        {
            var result = JsEvalEngine.Evaluate(script);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Destructure_Nested_Objects()
        {
            var result = JsEvalEngine.Evaluate(@"
                let data = { user: { name: 'Alice', age: 30 } };
                let { user: { name } } = data;
                name;
            ");
            Assert.That(result, Is.EqualTo("Alice"));
        }

        [Test]
        public void Should_Spread_Objects_With_Overrides()
        {
            var result = JsEvalEngine.Evaluate(@"
                let base = { a: 1, b: 2 };
                let override = { b: 3, c: 4 };
                let merged = { ...base, ...override };
                merged.b;
            ");
            Assert.That(result, Is.EqualTo(3));
        }
    }
}