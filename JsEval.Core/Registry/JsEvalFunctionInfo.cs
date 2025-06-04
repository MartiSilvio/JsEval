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
            else
            {
                var target = provider?.GetService(DeclaringType) ?? Activator.CreateInstance(DeclaringType);
                return Delegate.CreateDelegate(delegateType, target!, Method);
            }
        }

        public object? Invoke(IServiceProvider? provider, object?[] args)
        {
            object? target = null;

            if (!Method.IsStatic)
            {
                target = provider?.GetService(DeclaringType) ?? Activator.CreateInstance(DeclaringType);
                if (target == null)
                    throw new InvalidOperationException($"Cannot create instance of {DeclaringType.FullName}.");
            }

            var parameters = Method.GetParameters();
            var convertedArgs = new object?[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var expectedType = parameters[i].ParameterType;
                var actualArg = args.Length > i ? args[i] : Type.Missing;

                if (actualArg == null)
                {
                    convertedArgs[i] = expectedType.IsValueType && Nullable.GetUnderlyingType(expectedType) == null
                        ? Activator.CreateInstance(expectedType)
                        : null;
                }
                else
                {
                    try
                    {
                        convertedArgs[i] = ConvertArgument(actualArg, expectedType);
                    }
                    catch (Exception ex)
                    {
                        throw new JsEvalException(
                            $"Cannot convert argument at position {i} from '{actualArg.GetType()}' to '{expectedType}'",
                            ex);
                    }
                }
            }

            return Method.Invoke(target, convertedArgs);
        }

        private static object? ConvertArgument(object arg, Type targetType)
        {
            if (targetType.IsInstanceOfType(arg))
                return arg;

            if (targetType.IsEnum)
                return Enum.Parse(targetType, arg.ToString()!);

            if (targetType == typeof(Guid))
                return Guid.Parse(arg.ToString()!);

            if (targetType == typeof(string[]))
                return ((IEnumerable<object>)arg).Select(o => o.ToString()).ToArray();

            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType()!;
                var array = ((IEnumerable<object>)arg).Select(item => ConvertArgument(item, elementType)).ToArray();
                var resultArray = Array.CreateInstance(elementType, array.Length);
                for (var i = 0; i < array.Length; i++)
                    resultArray.SetValue(array[i], i);
                return resultArray;
            }

            if (targetType.IsClass && arg is IDictionary<string, object> dict)
            {
                var instance = Activator.CreateInstance(targetType)!;
                foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (dict.TryGetValue(prop.Name, out var val))
                    {
                        var converted = ConvertArgument(val, prop.PropertyType);
                        prop.SetValue(instance, converted);
                    }
                }

                return instance;
            }

            return Convert.ChangeType(arg, targetType);
        }
    }
}