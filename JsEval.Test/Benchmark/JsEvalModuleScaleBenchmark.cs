using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using JsEval.Core;
using JsEval.Core.Attributes;
using JsEval.Core.Registry;

namespace JsEval.Test.Benchmark
{
    [TestFixture]
    public class JsEvalModuleScaleBenchmark
    {
        [Test]
        public void ScaleModulesAndFunctions_InstanceMethods()
        {
            const int functionsPerModule = 20;
            for (var moduleCount = 100; moduleCount <= 1000; moduleCount += 100)
            {
                var asmName = new AssemblyName($"DynamicJsEvalModules_{moduleCount}");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(asmName.Name!);

                JsEvalFunctionRegistry.ClearAll();
                for (var modIndex = 0; modIndex < moduleCount; modIndex++)
                {
                    var moduleName = $"Module{modIndex}";
                    var typeBuilder = moduleBuilder.DefineType(
                        moduleName,
                        TypeAttributes.Public | TypeAttributes.Class
                    );
                    var moduleAttrCtor = typeof(JsEvalModuleAttribute).GetConstructor(new[] { typeof(string) })!;
                    var moduleAttr = new CustomAttributeBuilder(moduleAttrCtor, new object[] { moduleName });
                    typeBuilder.SetCustomAttribute(moduleAttr);

                    var funcAttrCtor = typeof(JsEvalFunctionAttribute).GetConstructor(new[] { typeof(string) })!;
                    for (var funcIndex = 0; funcIndex < functionsPerModule; funcIndex++)
                    {
                        var functionName = $"f{modIndex}_{funcIndex}";
                        var methodBuilder = typeBuilder.DefineMethod(
                            functionName,
                            MethodAttributes.Public,
                            typeof(int),
                            Type.EmptyTypes
                        );
                        var il = methodBuilder.GetILGenerator();
                        il.Emit(OpCodes.Ldc_I4, modIndex + funcIndex);
                        il.Emit(OpCodes.Ret);
                        var funcAttr = new CustomAttributeBuilder(funcAttrCtor, new object[] { functionName });
                        methodBuilder.SetCustomAttribute(funcAttr);
                    }

                    var generated = typeBuilder.CreateType()!;
                    JsEvalFunctionRegistry.RegisterFunctionsFromType(generated);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                JsEvalEngine.Evaluate("0");

                var sw = Stopwatch.StartNew();
                JsEvalEngine.Evaluate("Module0.f0_0()");
                sw.Stop();

                var totalFunctions = moduleCount * functionsPerModule;
                TestContext.WriteLine(
                    $"Modules: {moduleCount}, Functions/module: {functionsPerModule}, " +
                    $"Total functions: {totalFunctions}, Time: {sw.Elapsed.TotalMilliseconds:F2} ms"
                );
            }
        }
    }
}
