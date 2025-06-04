using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Jint;
using JsEval.Core.Registry;

// ReSharper disable InconsistentNaming

namespace JsEval.Core
{
    public static class JsEvalEngine
    {
        private const int RECURSION_LIMIT = 100;
        private const int MEMORY_LIMIT_BYTES = 2_000_000;
        private static readonly TimeSpan TIMEOUT_INTERVAL = TimeSpan.FromSeconds(2);

        private static readonly string[] BLOCKED_GLOBALS =
        [
            "eval",
            "Function",
            "constructor",
            "AsyncFunction"
        ];

        static JsEvalEngine()
        {
            var assembly = Assembly.GetExecutingAssembly();
            JsEvalFunctionRegistry.RegisterFunctionsFromAssembly(assembly);
        }

        public static object? Evaluate(
            string expression,
            ExpandoObject? pars = null,
            IServiceProvider? serviceProvider = null,
            JsEvalOptions? options = null)
        {
            try
            {
                using var engine = new Engine(cfg => cfg
                    .Strict()
                    .TimeoutInterval(options?.TimeoutInterval ?? TIMEOUT_INTERVAL)
                    .LimitMemory(options?.MemoryLimitBytes ?? MEMORY_LIMIT_BYTES)
                    .LimitRecursion(options?.RecursionLimit ?? RECURSION_LIMIT)
                );

                foreach (var name in BLOCKED_GLOBALS)
                {
                    engine.SetValue(name,
                        new Action(() => throw new JsEvalException($"{name} is disabled in this environment.")));
                }

                if (pars != null)
                {
                    engine.SetValue("pars", pars);
                }

                foreach (var kv in JsEvalFunctionRegistry.GetAllFunctions())
                {
                    engine.SetValue(kv.Key, kv.Value.CreateDelegate(serviceProvider));
                }

                foreach (var module in JsEvalFunctionRegistry.GetAllModules())
                {
                    var jsModule = new Dictionary<string, Delegate>();
                    foreach (var kv in module.Value)
                    {
                        jsModule[kv.Key] = kv.Value.CreateDelegate(serviceProvider);
                    }

                    engine.SetValue(module.Key, jsModule);
                }

                var result = engine.Evaluate(expression);
                return result.ToObject();
            }
            catch (Exception ex)
            {
                throw new JsEvalException("An error occurred while evaluating the JsEval expression.", ex);
            }
        }
    }

    public class JsEvalOptions
    {
        /// <summary>
        /// If null, defaults to <see cref="JsEvalEngine.RECURSION_LIMIT"/>.
        /// </summary>
        public int? RecursionLimit { get; set; }

        /// <summary>
        /// If null, defaults to <see cref="JsEvalEngine.MEMORY_LIMIT_BYTES"/>.
        /// If zero (0) or negative, the memory limit is ignored (no cap is applied).
        /// </summary>
        public long? MemoryLimitBytes { get; set; }

        /// <summary>
        /// If null, defaults to <see cref="JsEvalEngine.TIMEOUT_INTERVAL"/>.
        /// </summary>
        public TimeSpan? TimeoutInterval { get; set; }
    }
}
