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

Checkout the [tutorial](https://github.com/aheusel/VMF.NET.Tutorials) projects.

Add the NuGet package to your project:

```xml
<PackageReference Include="VMF.NET" Version="*" />
```

> Replace `*` with a specific version for reproducible builds. See [NuGet](https://www.nuget.org/packages/VMF.NET) for the latest.

Define your model as C# interfaces in a namespace ending in `.VmfModel`. That namespace is what
marks them as a model — no attribute is needed, and the interfaces need not be `public` or
`partial`, because the model is **build input**, not the API you use:

```csharp
using VMF.NET.Runtime.Attributes;

namespace MyApp.VmfModel;

interface Parent
{
    string? Name { get; set; }

    // A multi-valued property is an array here; the generated API exposes it as VList<Child>.
    [Contains("Child.Parent")]
    Child[] Children { get; }
}

interface Child
{
    int Value { get; set; }

    [Container("Parent.Children")]
    Parent? Parent { get; }
}
```

The generated interface keeps the name you gave it — `Parent` generates `Parent`, and `IParent`
generates `IParent` if you prefer C#'s convention. Only the implementation name drops a leading
`I`, since `IParentImpl` would be a class named like an interface.

VMF.NET generates the public API into the namespace **above** the model — `MyApp` here. That is
what your code uses:

```csharp
using MyApp;

// Create via factory method
var parent = Parent.NewInstance();
parent.Name = "Root";

// Or use the builder
var child = Child.NewBuilder()
    .WithValue(42)
    .Build();

parent.Children.Add(child);

// Containment is tracked automatically
Console.WriteLine(child.Parent == parent); // True

// Change listeners
parent.VMF.Changes.AddListener(change => {
    Console.WriteLine($"Changed: {change.PropertyName}");
});

// Deep clone
var copy = parent.Clone();

// Read-only wrapper
ReadOnlyParent ro = parent.AsReadOnly();
```

The source generator runs automatically on every build — no task to invoke.

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
Parent restored = JsonSerializer.Deserialize<Parent>(json, options)!;
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

The test suite includes 334 tests across the generator and behavioural projects.
