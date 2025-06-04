using System;
using JsEval.Core.Attributes;

namespace JsEval.Core.Builtins
{
    public static class MiscFunctions
    {
        [JsEvalFunction("guid")]
        public static Guid Guid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new JsEvalException("A non-empty GUID string argument is required.");

            if (!System.Guid.TryParse(value, out var parsed))
                throw new JsEvalException($"'{value}' is not a valid GUID.");

            return parsed;
        }
    }
}