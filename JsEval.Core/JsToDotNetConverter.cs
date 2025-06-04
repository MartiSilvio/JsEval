using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using System.Collections;

namespace JsEval.Core
{
    public class JsToDotNetConverter(Engine engine) : DefaultTypeConverter(engine)
    {
        private readonly Engine _engine = engine;

        public override object? Convert(object? value, Type type, IFormatProvider formatProvider)
        {
            if (TryConvert(value, type, formatProvider, out var converted))
                return converted;

            if (value == null || (value is JsValue js && js.IsUndefined()))
            {
                const string message = "Cannot read property of undefined";
                throw new JavaScriptException(JsValue.FromObject(_engine, message));
            }

            throw new ArgumentException($"Object of type '{value?.GetType()}' cannot be converted to '{type}'");
        }


        public override bool TryConvert(object? value, Type type, IFormatProvider formatProvider, out object converted)
        {
            converted = null!;

            switch (value)
            {
                case null:
                    return type.IsClass || Nullable.GetUnderlyingType(type) != null;

                case object[] jsArray when TryGetEnumerableElementType(type, out var elementType):
                {
                    var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                    foreach (var item in jsArray)
                    {
                        if (!TryConvert(item, elementType, formatProvider, out var convertedItem))
                            return false;

                        typedList.Add(convertedItem);
                    }

                    if (type.IsArray)
                    {
                        var array = Array.CreateInstance(elementType, typedList.Count);
                        typedList.CopyTo(array, 0);
                        converted = array;
                        return true;
                    }

                    if (type.IsInstanceOfType(typedList) || type is { IsInterface: true, IsGenericType: true } &&
                        type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        converted = typedList;
                        return true;
                    }

                    try
                    {
                        converted = System.Convert.ChangeType(typedList, type, formatProvider);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                default:
                {
                    if (type.IsEnum)
                    {
                        try
                        {
                            if (value is string s)
                            {
                                converted = Enum.Parse(type, s, ignoreCase: true);
                                return true;
                            }

                            var underlying =
                                System.Convert.ChangeType(value, Enum.GetUnderlyingType(type), formatProvider);
                            converted = Enum.ToObject(type, underlying);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    return base.TryConvert(value, type, formatProvider, out converted);
                }
            }
        }


        private static bool TryGetEnumerableElementType(Type type, out Type elementType)
        {
            elementType = null!;

            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            var match = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (match != null)
            {
                elementType = match.GetGenericArguments()[0];
                return true;
            }

            return false;
        }
    }
}