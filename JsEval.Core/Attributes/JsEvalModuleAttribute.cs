namespace JsEval.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class JsEvalModuleAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}