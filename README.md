# JsEval

JsEval is a secure and extensible JavaScript evaluation engine for .NET, built on top of [Jint](https://github.com/sebastienros/jint). While Jint provides the core ECMAScript execution capabilities, JsEval adds an application-focused layer that allows .NET applications to dynamically execute JavaScript expressions and scripts, while safely bridging to annotated C# methods and modules through a controlled and configurable runtime.

By combining the flexibility of JavaScript with the structure and type safety of C#, JsEval enables rich scripting capabilities in enterprise and embedded applications—without compromising security, isolation, or control.

---

## Overview

JsEval provides:

- Attribute-based registration of C# methods, available as global functions or within named modules
- Safe execution of JavaScript code with configurable limits
- Automatic mapping of JavaScript arguments to strongly-typed .NET parameters
- Support for dependency injection in instance-bound method resolution
- Extensibility across function registration, argument binding, and module composition

---

## Getting Started

### Define a Function

```csharp
public class MathFunctions
{
    [JsEvalFunction("add")]
    public static int Add(int a, int b) => a + b;
}
```

### Register the Function

```csharp
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(MathFunctions));
```

### Evaluate from JavaScript

```csharp
var result = JsEvalEngine.Evaluate("add(2, 3)"); // 5
```

---

## Function Registration

JsEval supports two models for exposing functions:

- Global functions, directly accessible by name
- Modular functions, grouped under a namespace

Both approaches are fully supported and can be used together.

### Global Function

```csharp
public static class Utility
{
    [JsEvalFunction("toUpper")]
    public static string ToUpper(string input) => input.ToUpper();
}
```

```csharp
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(Utility));
JsEvalEngine.Evaluate("toUpper('wovera')"); // "WOVERA"
```

### Modular Function

```csharp
[JsEvalModule("math")]
public class MathModule
{
    [JsEvalFunction("square")]
    public int Square(int x) => x * x;
}
```

```csharp
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(MathModule));
JsEvalEngine.Evaluate("math.square(6)"); // 36
```

---

## Argument Binding

JsEval automatically binds JavaScript values to C# method parameters. Supported conversions include:

- Primitives: `int`, `double`, `bool`, `string`
- Arrays of primitive types
- Nullable types
- Complex types (POCOs) mapped from object literals
- Enums (by string name)

JsEval is designed to be extensible. Additional type conversions and custom binding behavior can be introduced as needed.

### Example: Mapping to POCO

```csharp
public class User
{
    public string Name { get; set; } = default!;
}

public static class UserFunctions
{
    [JsEvalFunction("greet")]
    public static string Greet(User user) => $"Hi, {user.Name}!";
}
```

```js
greet({ Name: "Silvio" }); // "Hi, Silvio!"
```

---

## Script Evaluation

JsEval supports single-line expressions as well as full JavaScript blocks:

```js
let total = 0;
for (let i = 1; i <= 3; i++) {
  total += i;
}
total; // returns 6
```

The final value of the script is returned as the result.

---

## Dependency Injection

JsEval supports instance method invocation using a provided `IServiceProvider`. If the function is not static, the engine will resolve the class instance from the service provider.

```csharp
public class Logger
{
    private readonly AuditService _audit;
    public Logger(AuditService audit) => _audit = audit;

    [JsEvalFunction("log")]
    public string Log(string msg) => _audit.Stamp(msg);
}
```

```csharp
var services = new ServiceCollection()
    .AddSingleton<AuditService>()
    .AddTransient<Logger>()
    .BuildServiceProvider();

JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(Logger));
JsEvalEngine.Evaluate("log('hello')", services);
```

---

## Security Model

JsEval enforces execution limits and disables access to sensitive APIs. Specifically:

- `eval`, `Function`, and constructor access are blocked
- `globalThis`, `AsyncFunction`, and similar globals are inaccessible
- Recursion depth is limited (default: 100)
- Memory usage is capped (~2MB by default)
- Execution time is limited (default: 2 seconds per evaluation)

These constraints ensure that untrusted JavaScript code can be executed safely.

---

## Testing

JsEval includes comprehensive test coverage for core evaluation logic, edge cases, coercion rules, block execution, object and array mapping, scope isolation, and security enforcement.

Tests can be executed using:

```bash
dotnet test
```

---

## API Overview

```csharp
// Evaluate with optional service provider
var result = JsEvalEngine.Evaluate("add(1, 2)", provider);

// Register from a specific type
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(MyFunctions));

// Register all functions in the current assembly
JsEvalFunctionRegistry.RegisterFunctionsFromAssembly(Assembly.GetExecutingAssembly());

// Clear all registered functions and modules
JsEvalFunctionRegistry.ClearAll();
```

---

## Advanced Example

```csharp
[JsEvalModule("user")]
public class UserModule
{
    [JsEvalFunction("getByKey")]
    public User GetByKey(string key) => new User { Name = key };
}

public class GlobalTools
{
    [JsEvalFunction("now")]
    public static string Now() => DateTime.UtcNow.ToString("u");
}
```

```csharp
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(UserModule));
JsEvalFunctionRegistry.RegisterFunctionsFromType(typeof(GlobalTools));
```

```js
user.getByKey("mark").Name; // "mark"
now(); // current UTC timestamp
```
