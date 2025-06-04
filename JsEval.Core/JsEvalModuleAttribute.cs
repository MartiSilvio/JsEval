namespace JsEval.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class JsEvalModuleAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}