# hw.Helper.Core

> **Source-code-only NuGet package** — Helper classes providing essential extensions for everyday C# programming.

[![NuGet](https://img.shields.io/nuget/v/hw.Helper.Core)](https://www.nuget.org/packages/hw.Helper.Core)

## Overview

`hw.Helper.Core` is a collection of carefully crafted C# utility classes and extension methods that cover common programming patterns: LINQ extensions, string manipulation, lazy value caching, file system abstraction, and a powerful debug/trace framework.

The package is distributed as **source code** (contentFiles), so all classes are compiled directly into your project with full IntelliSense and debuggability.

## Installation

```
dotnet add package hw.Helper.Core
```

## Namespaces

| Namespace | Contents |
|---|---|
| `hw.Helper` | LINQ, string, file, cache extensions |
| `hw.DebugFormatter` | Dump, trace, assert infrastructure |

---

## Features

### LINQ Extensions (`hw.Helper.LinqExtension`)

Rich set of extension methods for `IEnumerable<T>`, `IList<T>`, and related types.

**Splitting & grouping**

```csharp
// Split a sequence at separator elements
var parts = items.Split(x => x == delimiter);

// Split keeping the separator at begin or end of sub-list
var parts = items.Split(x => x == delimiter, SeparatorTreatmentForSplit.BeginOfSubList);
```

**Searching & filtering**

```csharp
int? idx = items.IndexWhere(x => x.IsMatch);

// Flexible First/Single with custom exceptions
var item = items.Top(x => x.IsActive, emptyException: () => new NotFoundException());

// Null-safe result
var item = items.Top(); // returns default if empty
```

**Min/Max with nullable results**

```csharp
int? max = numbers.Maxx; // null if empty
int? min = numbers.Minn;
```

**Object graph traversal**

```csharp
// Walk a linked list or tree
var chain = node.Chain(n => n.Parent);

// Depth-first hierarchy traversal
var all = root.SelectHierarchical(n => n.Children);

// Transitive closure of a relation
var closure = items.Closure(item => item.Dependencies);

// Circuit/cycle detection
bool ok = items.IsCircuitFree(item => item.Dependencies);
```

**Joining & merging**

```csharp
// Full outer join by key
var merged = left.Merge(right, l => l.Id, r => r.Id);
// → IEnumerable<Tuple<TKey, TLeft?, TRight?>>

// Flatten nested sequences, skipping nulls
var flat = nestedSequences.ConcatMany();
```

**Null handling**

```csharp
T result = nullable.AssertNotNull();   // throws on null
T result = nullable.AssertValue();     // for Nullable<T>

IEnumerable<T> single = obj.NullableToArray(); // [] or [obj]
```

**Other utilities**

```csharp
bool found = value.In(a, b, c);       // set membership
string text = items.Stringify(", ");   // join to string
string text = items.Stringify(", ", showNumbers: true);
```

---

### String Extensions (`hw.Helper.StringExtender`)

```csharp
// Indentation
string indented = code.Indent(count: 2);
string indented = code.Indent(tabString: "\t");

// Repetition
string line = "-".Repeat(40);

// Surround with matching brackets — auto-detects closing bracket
string s = content.Surround("(");   // → "(content)"
string s = content.Surround("{");   // → "{content}" or multi-line indented

// Quoting
string literal  = value.Quote();        // → "\"hello\""
string csLiteral = value.CSharpQuote(); // → proper C# string literal with escapes

// Case conversion
string camel = "MY_FIELD_NAME".UnderScoreToCamelCase(); // → "MyFieldName"
string title = "hELLO".ToLowerFirstUpper();              // → "Hello"

// Path helpers
string path = head.PathCombine("sub", "file.txt");
SmbFile file = path.ToSmbFile();

// Splitting by fixed column widths
var columns = line.Split(10, 20, 30); // → IEnumerable<string>

// Pattern matching
bool ok = input.Matches(@"^\d+$");

// Alignment
var aligner = 3.StringAligner(); // 3-column aligner
string formatted = line.Format(aligner);
```

---

### Value Cache (`hw.Helper.ValueCache<T>`)

Thread-aware lazy evaluation with explicit invalidation support.

```csharp
public class MyClass
{
    readonly ValueCache<ExpensiveResult> _cache;

    public MyClass()
    {
        _cache = new ValueCache<ExpensiveResult>(ComputeResult);
    }

    public ExpensiveResult Result => _cache.Value;

    public void Invalidate() => _cache.IsValid = false;

    ExpensiveResult ComputeResult() { /* ... */ }
}
```

- `Value` — evaluates on first access, then caches
- `IsValid` — get or set; setting `false` clears the cache
- `IsBusy` — detects recursive evaluation (throws on re-entry)
- Debugger-friendly: shows current value or `"Evaluation pending"` / `"Unknown"`

---

### File System Abstraction (`hw.Helper.SmbFile`)

A unified wrapper for files and directories with auto-directory-creation support.

```csharp
// Create from path string
SmbFile file = "C:\\data\\output.txt".ToSmbFile();

// Read / write text
string? content = file.String;
file.String = "new content";

// Read / write bytes
byte[] data = file.Bytes;

// File metadata
bool   exists    = file.Exists;
bool   isDir     = file.IsDirectory;
long   size      = file.Size;
string name      = file.Name;
string fullPath  = file.FullName;
string ext       = file.Extension;
Version? ver     = file.FileVersion;

// Directory operations
SmbFile[] items     = file.Items;           // direct children
IEnumerable<SmbFile> all = file.RecursiveItems(); // recursive

// Path composition
SmbFile sub = parent / "subfolder" / "file.txt"; // operator /

// Copy, move, delete
file.CopyTo(destination);
file.Move(newPath);
file.Delete(recursive: true);

// Source-location helpers (compile-time caller info)
string?  srcPath = SmbFile.SourcePath;
SmbFile? srcFile = SmbFile.SourceFile;
```

---

### Debug Formatter (`hw.DebugFormatter`)

#### `Tracer` — Logging, assertions, and debug output

#### Visual Studio Output Window integration

All log output is formatted in the Visual Studio clickable format:

```
C:\myproject\MyClass.cs(42,13,42,35): Debug: MyClass.MyMethod
```

A double-click on such a line in the VS Output Window navigates directly to the corresponding source position. The format follows the pattern:

```csharp
public const string VisualStudioLineFormat =
    "{fileName}({lineNumber},{columnNumber},{lineNumberEnd},{columnNumberEnd}): {tagText}: ";
```

The tag can be one of the `FilePositionTag` enum values: `Debug`, `Output`, `Query`, `Test`, `Profiler`.

You can generate position strings manually:

```csharp
// Current call site (uses stack frame introspection)
string header = Tracer.MethodHeader(FilePositionTag.Debug);
string header = Tracer.MethodHeader(FilePositionTag.Test, showParam: true);

// From a specific stack frame
string pos = Tracer.FilePosition(stackFrame, FilePositionTag.Debug);

// From explicit line/column info
string pos = Tracer.FilePosition(fileName, lineNumber, columnNumber1, tag);

// Full stack trace, one clickable line per frame
string trace = Tracer.StackTrace(FilePositionTag.Debug);
```

`FlaggedLine` and `Log` write a clickable line to the output automatically:

```csharp
"something happened".Log();
"something happened".Log(LogLevel.Debug);
"entering method".FlaggedLine(FilePositionTag.Debug, showParam: true);
```

#### Assertions

```csharp
(x > 0).Assert();
(x > 0).Assert(() => $"Expected positive, got {x}");
obj.AssertIsNotNull();
obj.AssertIsNull();
obj.Assert<ExpectedType>();   // type-check assertion
```

On failure, the assertion logs a clickable source position and either breaks into the debugger (if attached) or throws `AssertionFailedException`.

#### Reflection-based dump

```csharp
string s = Tracer.Dump(anyObject);      // full reflection dump
string s = Tracer.DumpData(anyObject);  // fields/properties only
string s = obj.LogDump();               // extension method variant
string s = "value".DumpValue(myObj);   // → "value = <dump>"
```

#### Conditional debugger break

```csharp
condition.ConditionalBreak();
Tracer.UnconditionalBreak("reason");
```

---

### Output configuration (`Configuration` / `Tracer.Dumper`)

The dump output is fully configurable via `Tracer.Dumper.Configuration.Handlers`. Out of the box, `IList`, `IDictionary`, `ICollection`, `Type`, `string`, `Exception`, enums, and primitives are handled automatically.

#### Log level

```csharp
Writer.LogLevel = LogLevel.Debug;       // show all
Writer.LogLevel = LogLevel.Information; // default
Writer.LogLevel = LogLevel.None;        // suppress all
```

#### Custom dump for a specific type

```csharp
Tracer.Dumper.Configuration.Handlers.Add<MyType>(
    dump: (type, obj) => $"MyType({obj.Id})"
);
```

#### Custom member filter (exclude properties from reflection dump)

```csharp
Tracer.Dumper.Configuration.Handlers.Add<MyType>(
    memberCheck: (member, obj) => member.Name != "InternalState"
);
```

#### Match by type predicate

```csharp
Tracer.Dumper.Configuration.Handlers.Add(
    typeMatch: t => t.Namespace?.StartsWith("MyApp.Internal") == true,
    dump: (type, obj) => obj.ToString() ?? "null"
);
```

#### Priority: `Force` vs `Add`

Handlers are checked in registration order. Use `Force` to prepend a handler so it takes precedence over existing ones:

```csharp
Tracer.Dumper.Configuration.Handlers.Force<MyType>(
    dump: (type, obj) => "overrides everything"
);
```

#### Redirect output with `TextWriters`

By default, output goes to `Console`. To fan out to multiple targets (e.g. a file and the console simultaneously), use `TextWriters`:

```csharp
var fileWriter = new StreamWriter("debug.log");
Console.SetOut(new TextWriters(Console.Out, fileWriter));
```

`TextWriters` flattens nested instances automatically, so combining multiple `TextWriters` never causes duplicate output.

#### `Dumpable` — Base class for debuggable objects

Inherit from `Dumpable` to get automatic reflection-based dump support in the debugger.

```csharp
public class MyNode : Dumpable
{
    public string Name { get; set; }

    [DisableDump]
    public MyNode? Parent { get; set; }   // excluded from dump
}
```

- `DebuggerDumpString` — shown in the VS debugger tooltip
- `d` — short alias for quick debugger evaluation
- `Dump()` — recursive-safe string representation
- `DumpData()` — field/property dump via reflection

#### Not-implemented stubs

Use these instead of `throw new NotImplementedException()` to get a clickable source position and a debugger break:

```csharp
// In a static context
protected void MyMethod(int x, string y)
    => Dumpable.NotImplementedFunction(x, y);

// In an instance method (includes "this" in the dump)
protected void MyMethod(int x, string y)
    => NotImplementedMethod(x, y);
```

Both log the method name, all parameter values, and break into the debugger if attached.

#### Method call tracing (`StartMethodDump` / `ReturnMethodDump` / `ReturnVoidMethodDump`)

For detailed step-through debugging of complex methods, wrap the body with the method dump trio. Tracing only activates when the debugger is attached and tracing is enabled for the current call frame.

```csharp
protected MyResult Compute(int x, string y)
{
    StartMethodDump(trace: true, x, y);  // logs entry + parameters
    try
    {
        var result = DoWork(x, y);
        return ReturnMethodDump(result); // logs return value + breaks
    }
    finally
    {
        EndMethodDump();
    }
}

// For void methods:
protected void Execute(int x)
{
    StartMethodDump(trace: true, x);
    try
    {
        DoWork(x);
        ReturnVoidMethodDump();
    }
    finally
    {
        EndMethodDump();
    }
}
```

Pass `trace: false` to `StartMethodDump` to disable tracing for a specific call without removing the instrumentation. `Dumpable.IsMethodDumpTraceInhibited` overrides all frames globally when set.

---

#### `DumpableObject` — Base class with object identity

`DumpableObject` extends `Dumpable` with a unique numeric object ID assigned at construction. This makes it easy to track specific instances across log output.

```csharp
public class MyNode : DumpableObject
{
    protected override string GetNodeDump() => $"MyNode({Name})";
}
```

Each instance gets a stable `ObjectId` (auto-incrementing integer). The debugger display shows `MyNode(foo).42i` — type info plus the object ID.

**Targeted breakpoints by object ID:**

```csharp
// Break execution whenever ObjectId 42 or 99 is touched
node.StopByObjectIds(42, 99);
```

This is useful for tracking down when a specific instance is created or accessed, without setting conditional breakpoints manually in the IDE.

**`NodeDump`** — short identifying string shown in the debugger, composed of `GetNodeDump()` + `.{ObjectId}i`. Override `GetNodeDump()` to customize the label.

**`AdditionalNodeInfoAttribute`** — marks a property to be shown after the node title in custom debugger visualizers:

```csharp
[AdditionalNodeInfo(nameof(NodeDump))]
public class MyNode : DumpableObject { ... }
```

#### Dump control attributes

| Attribute | Effect |
|---|---|
| `[DisableDump]` | Exclude property/field from dump |
| `[EnableDump]` | Force inclusion (override class-level setting) |
| `[EnableDumpExcept(value)]` | Include unless value matches given constant |
| `[DumpToString]` | Use `ToString()` instead of reflection |
| `[Dump("MethodName")]` | Use a named method for the dump |
| `[DumpClass]` | Apply dump settings at class level |

#### `FunctionCache<TKey, TValue>` — Memoization

```csharp
var cache = new FunctionCache<string, int>(key => ExpensiveCompute(key));
int result = cache["myKey"]; // computed once, then cached
```

---

### Other Utilities

**`EnumEx`** — Enum helpers including attribute access:
```csharp
var attr = myEnumValue.GetAttribute<DescriptionAttribute>();
```

**`ReflectionExtender`** — Type name helpers:
```csharp
string name = typeof(List<int>).PrettyName(); // → "List<Int32>"
```

**`DateTimeExtender` / `DateRange`** — DateTime formatting and ranges.

**`ReplaceVariablesExtension`** — Template variable substitution in strings.

**`LocationProviderAttribute`** — Marks methods that provide caller location information.

---

## Requirements

- .NET 4.7.2 or later / .NET 6+ / .NET 9
- JetBrains.Annotations (for `[PublicAPI]`, `[ContractAnnotation]` support)

## License

See repository for license details.

## Repository

[github.com/hahoyer/HWClassLibrary.cs](https://github.com/hahoyer/HWClassLibrary.cs)


# Issues and Co.
- Report to project website on github (https://github.com/hahoyer/HWClassLibrary.cs)
- issues and feedback: https://github.com/hahoyer/HWClassLibrary.cs/issues
- [![Support me on ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/G2G4BH6WX)

