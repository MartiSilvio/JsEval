using System;
using System.Collections.Generic;
using System.Dynamic;
using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;
using NUnit.Framework;

namespace JsEval.Test.Core
{
    public enum TestStatus
    {
        Unknown,
        Ready,
        Running,
        Completed
    }

    public class TestInterop
    {
        [JsEvalFunction("AcceptStatusList")]
        public int AcceptStatusList(object raw)
        {
            Console.WriteLine("Raw type: " + raw?.GetType());

            if (raw is IEnumerable<object> enumerable)
            {
                int count = 0;
                foreach (var item in enumerable)
                {
                    Console.WriteLine($"Item: {item} ({item?.GetType()})");
                    count++;
                }

                return count;
            }

            return 0;
        }
    }

    [TestFixture]
    public class JsEvalInteropTests
    {
        [SetUp]
        public void Setup()
        {
            // Register test method for evaluation
            JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(TestInterop));
        }

        [Test]
        public void Should_Convert_JsArray_To_EnumList()
        {
            dynamic pars = new ExpandoObject();
            pars.statuses = new object[] { "Ready", "Running" }; // ✅ critical fix

            var result = JsEvalEngine.Evaluate("AcceptStatusList(pars.statuses)", pars);

            Assert.That(result, Is.EqualTo(2));
        }
    }
}