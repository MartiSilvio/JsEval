using System.Linq.Expressions;
using System.Reflection;

namespace JsEval.Core.Registry
{
    public class JsEvalFunctionInfo(string name, Type declaringType, MethodInfo method)
    {
        public string Name { get; } = name;
        public Type DeclaringType { get; } = declaringType;
        public MethodInfo Method { get; } = method;

        public Delegate CreateDelegate(IServiceProvider? provider = null)
        {
            var parameterTypes = Method.GetParameters()
                .Select(p => p.ParameterType)
                .Concat([Method.ReturnType])
                .ToArray();

            var delegateType = Expression.GetDelegateType(parameterTypes);

            if (Method.IsStatic)
                return Delegate.CreateDelegate(delegateType, Method);
            var target = provider?.GetService(DeclaringType) ?? Activator.CreateInstance(DeclaringType);

            return Delegate.CreateDelegate(delegateType, target!, Method);
        }
    }
}