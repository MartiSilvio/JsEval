namespace JsEval.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JsEvalFunctionAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}