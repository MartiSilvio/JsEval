using System.Collections.Concurrent;
using System.Reflection;
using JsEval.Core.Attributes;

namespace JsEval.Core.Registry
{
    public static class JsEvalFunctionRegistry
    {
        private static readonly ConcurrentDictionary<string, JsEvalFunctionInfo> GlobalFunctions =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, Dictionary<string, JsEvalFunctionInfo>> Modules =
            new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterFunctionsFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var hasFunctions = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    .Any(m => m.GetCustomAttribute<JsEvalFunctionAttribute>() != null);

                if (hasFunctions)
                {
                    RegisterFunctionsFromType(type);
                }
            }
        }

        public static void RegisterFunctionsFromType(Type type)
        {
            var moduleAttr = type.GetCustomAttribute<JsEvalModuleAttribute>();
            var isModule = moduleAttr != null;

            var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var moduleFunctions = new Dictionary<string, JsEvalFunctionInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var method in methodInfos)
            {
                var attr = method.GetCustomAttribute<JsEvalFunctionAttribute>();
                if (attr == null) continue;

                var funcInfo = new JsEvalFunctionInfo(attr.Name, type, method);

                if (isModule)
                {
                    if (!moduleFunctions.TryAdd(attr.Name, funcInfo))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate function '{attr.Name}' in module '{moduleAttr!.Name}'.");
                    }
                }
                else
                {
                    if (!GlobalFunctions.TryAdd(attr.Name, funcInfo))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate global function '{attr.Name}' found in type '{type.FullName}'.");
                    }
                }
            }

            if (isModule && moduleAttr!.Name != null)
            {
                if (!Modules.TryAdd(moduleAttr.Name, moduleFunctions))
                {
                    throw new InvalidOperationException(
                        $"Duplicate module name '{moduleAttr.Name}' found in type '{type.FullName}'.");
                }
            }
        }
        
        public static IReadOnlyDictionary<string, JsEvalFunctionInfo> GetAllFunctions() => GlobalFunctions;

        public static IReadOnlyDictionary<string, Dictionary<string, JsEvalFunctionInfo>> GetAllModules() => Modules;

        public static void ClearAll()
        {
            GlobalFunctions.Clear();
            Modules.Clear();
        }
    }
}