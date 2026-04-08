# VMF.NET

[![Build & Test](https://github.com/aheusel/VMF.NET/actions/workflows/build.yml/badge.svg)](https://github.com/aheusel/VMF.NET/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/VMF.NET.Runtime.svg?label=NuGet)](https://www.nuget.org/packages/VMF.NET.Runtime)
[![Docs](https://github.com/aheusel/VMF.NET/actions/workflows/docs.yml/badge.svg)](https://github.com/aheusel/VMF.NET/actions/workflows/docs.yml)

VMF.NET is a lightweight modeling framework for .NET. It translates annotated C# interfaces into powerful implementations via a Roslyn Source Generator — no separate build step, no code-gen tooling, no boilerplate. It works with .NET 6 and later.
VMF.NET is a port of the java [VMF](https://github.com/miho/VMF) framework. 

It generates/supports:

- Getters and setters
- Default values
- Containment
- Builder API
- `Equals()` and `GetHashCode()`
- Deep and shallow cloning
- Change notification (`INotifyPropertyChanged` / `INotifyCollectionChanged`)
- Undo/redo
- Object graph traversal via iterators
- Immutable types and read-only wrappers
- Delegation
- Annotations
- Reflection
- JSON serialization (`System.Text.Json`)
- JSON Schema generation

A VMF.NET model consists of annotated C# interfaces. Just define the interface and its properties — VMF.NET generates a fully functional implementation including property setters/getters, builders, change listeners, and much more:

## Using VMF.NET

Add the NuGet packages to your project:

```xml
<PackageReference Include="VMF.NET.Runtime" Version="*" />
<PackageReference Include="VMF.NET.SourceGenerator" Version="*" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

> Replace `*` with a specific version (e.g., `0.1.1`) for reproducible builds. See [NuGet](https://www.nuget.org/packages/VMF.NET.Runtime) for the latest version.

Define your model as annotated `partial` interfaces. The interfaces must be marked with `[VmfModel]` and the namespace must end with `.VmfModel`:

```csharp
using VMF.NET.Runtime.Core;

namespace MyApp.VmfModel;

[VmfModel]
public partial interface IParent
{
    [Contains(Opposite = nameof(IChild.Parent))]
    IList<IChild> Children { get; }

    string Name { get; set; }
}

[VmfModel]
public partial interface IChild
{
    [Container(Opposite = nameof(IParent.Children))]
    IParent? Parent { get; }

    int Value { get; set; }
}
```

The source generator runs automatically on every build — no task to invoke. The generated implementation is immediately available:

```csharp
// Create via factory method
var parent = IParent.NewInstance();
parent.Name = "Root";

// Or use the builder
var child = IChild.Build()
    .WithValue(42)
    .Build();

parent.Children.Add(child);

// Containment is tracked automatically
Console.WriteLine(child.Parent == parent); // True

// Change listeners
parent.Vmf().Changes().AddListener(change => {
    Console.WriteLine($"Changed: {change.PropertyName}");
});

// Deep clone
var copy = parent.Clone();

// Read-only wrapper
IReadOnlyParent ro = parent.AsReadOnly();
```

### JSON Serialization

Add the JSON package for `System.Text.Json` support:

```xml
<PackageReference Include="VMF.NET.Json" Version="*" />
```

```csharp
var options = new JsonSerializerOptions
{
    Converters = { new VmfJsonConverterFactory() },
    WriteIndented = true
};

string json = JsonSerializer.Serialize(parent, options);
IParent restored = JsonSerializer.Deserialize<IParent>(json, options)!;
```

## Building VMF.NET

### Requirements

- .NET 6 SDK or later
- Internet connection (NuGet packages are restored automatically)

### Command Line

```bash
dotnet build
dotnet test
```

### Packing

```bash
dotnet pack --configuration Release
```

## Testing VMF.NET

```bash
dotnet test --configuration Release --verbosity normal
```

The test suite includes 163 tests across unit and integration projects.
