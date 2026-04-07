# Migration Strategy: VMF Java to C#

## Recommended Approach: Roslyn Source Generator

The C# port should use **Roslyn Source Generators** as the code generation mechanism. This is the closest equivalent to VMF's Gradle plugin + Velocity template approach, with native IDE integration.

## Key Design Decisions

### Code Generation: String-Based Templating
The Source Generator will use **string-based templating** (e.g., Scriban) to produce C# source, mirroring VMF's use of Apache Velocity. This keeps the generation logic readable and close to the Java original. Revisit only if profiling reveals a concrete performance problem.

### Event System: Native C# Events + ChangesManager
- Generated objects implement `INotifyPropertyChanged` / `INotifyCollectionChanged` for direct WPF/WinForms/MAUI data binding.
- `IDisposable` replaces VMF's `Subscription.unsubscribe()` pattern (`using` support for automatic cleanup).
- A `ChangesManager` class (equivalent to Java's `ChangesImpl`) manages recursive containment-tree listener registration internally. The user-facing API remains nearly identical to Java:
  ```csharp
  using var sub = parent.Vmf().Changes().AddListener(change => { ... }, recursive: true);
  ```

## Phase 1: Runtime Library (`VMF.Runtime`)

**Priority: HIGHEST** - All generated code depends on this.

### Step 1.1: Core Interfaces

Port the public runtime interfaces as a NuGet package `VMF.Runtime`:

```
VMF.Runtime/
  IVObject.cs
  IVmf.cs
  IContent.cs
  IChanges.cs
  IReflect.cs
  IProperty.cs
  IVmfType.cs
  IBuilder.cs
  IChange.cs
  ITransaction.cs
  IChangeListener.cs
  IMutable.cs
  IReadOnly.cs
  IImmutable.cs
  IDelegatedBehavior.cs
  DelegatedBehaviorBase.cs
  ITraversalListener.cs
  VIterator.cs
  IAnnotation.cs
  ModelVersion.cs
```

### Step 1.2: VList Implementation

Extend `ObservableCollection<T>` with VMF-specific hooks (containment management, cross-reference management, element-level callbacks). This preserves native `INotifyCollectionChanged` for UI binding while adding VMF semantics.

Key requirements:
- Fire granular change events (add, remove, replace with indices)
- Support subscription-based listeners that return `IDisposable`
- Support element-level callbacks (for containment auto-management)
- Thread safety considerations (VMF Java is single-threaded)

### Step 1.3: Internal Runtime

Port internal classes:
- `VMFPropertyChangeSupport` -> Use `INotifyPropertyChanged` pattern
- `ChangesImpl` -> Change recording engine
- `ReflectImpl` -> Reflection API implementation
- `VObjectInternal` / `VObjectInternalModifiable` -> Internal interfaces for generated code

## Phase 2: Core Annotations/Attributes (`VMF.Core`)

Port as a NuGet package `VMF.Core`:

```
VMF.Core/
  Attributes/
    ContainsAttribute.cs
    ContainerAttribute.cs
    RefersAttribute.cs
    ImmutableAttribute.cs
    InterfaceOnlyAttribute.cs
    VmfModelAttribute.cs
    VmfEqualsAttribute.cs
    DefaultValueAttribute.cs
    RequiredAttribute.cs
    GetterOnlyAttribute.cs
    IgnoreEqualsAttribute.cs
    IgnoreToStringAttribute.cs
    PropertyOrderAttribute.cs
    DelegateToAttribute.cs
    ExternalTypeAttribute.cs
    DocAttribute.cs
    VmfAnnotationAttribute.cs
    SyncWithAttribute.cs
  EqualsType.cs
  ContainmentType.cs
  PropType.cs
```

**Key difference from Java**: C# attributes can use `typeof(T)` for type references instead of string class names.

## Phase 3: Code Generator (Roslyn Source Generator)

Port as a NuGet package `VMF.SourceGenerator`:

### Step 3.1: Model Analysis

Replace Java reflection-based analysis with Roslyn symbol analysis:

| Java Approach | C# Roslyn Approach |
|---|---|
| `Class<?>` + reflection | `INamedTypeSymbol` + semantic model |
| `Method.getAnnotation()` | `symbol.GetAttributes()` |
| `Method.getReturnType()` | `IPropertySymbol.Type` |
| `Class.getInterfaces()` | `INamedTypeSymbol.Interfaces` |
| `Method.getName()` | `IPropertySymbol.Name` |
| Package name from class | Namespace from symbol |

The `Model`, `ModelType`, `Prop`, `Implementation`, etc. classes can be ported almost directly - they are just data structures. Replace the `Class<?>` constructor parameter with `INamedTypeSymbol`.

**Key advantage**: C# interfaces already have properties, so no getter-to-property-name conversion is needed.

### Step 3.2: Template Engine

Options for code generation:
1. **String interpolation / StringBuilder** - Simplest, most debuggable
2. **Scriban** - Template engine similar to Velocity
3. **T4 templates** - Build-time templates (less flexible)
4. **Source code building** - Use `SyntaxFactory` (verbose but type-safe)

**Recommendation**: Use **string interpolation with helper methods**. Velocity templates are complex but the actual code patterns are repetitive. C# raw string literals (`"""..."""`) make this clean.

### Step 3.3: Multi-Pass Model Construction

Port the 7-pass model construction from `Model.java`:

```csharp
public class VmfModel
{
    public static VmfModel Create(IEnumerable<INamedTypeSymbol> interfaces)
    {
        // Pass 0: Classify types (external, model, interface-only, immutable)
        // Pass 1: Resolve containment (@Contains/@Container)
        // Pass 2: Resolve cross-references (@Refers)
        // Pass 3: Resolve inheritance
        // Pass 4: Sync info + property IDs
        // Pass 5: Collect inherited properties and delegations
        // Pass 6: Interface properties + all inherited types
        // Pass 7: Validation
    }
}
```

### Step 3.4: Generated Code Output

For each model type, generate:
- Public interface: `IMyType.g.cs`
- Read-only interface: `IReadOnlyMyType.g.cs`
- Implementation: `MyTypeImpl.g.cs` (internal)
- Read-only implementation: `ReadOnlyMyTypeImpl.g.cs` (internal)
- Builder: nested in interface or separate `MyTypeBuilder.g.cs`

Model-level:
- Switch interface: `ISwitchFor<Model>Model.g.cs`
- Listener: `IListenerFor<Model>Model.g.cs`
- Model API: `<Model>ModelApi.g.cs`

## Phase 4: Serialization (`VMF.Json`)

Port `vmf-jackson` to use `System.Text.Json`:
- Custom `JsonConverter<T>` for VMF types
- Polymorphic serialization via `[JsonDerivedType]`
- JSON Schema generation

## Phase 5: Build Integration

Options:
1. **Roslyn Source Generator** (recommended) - NuGet package, zero configuration
2. **MSBuild Task** - More control, similar to Gradle plugin approach
3. **dotnet tool** - CLI code generator (like the Java `VMF.generate()` API)

### NuGet Package Structure

```
VMF.Runtime.nupkg        <- Runtime library
VMF.Core.nupkg           <- Attributes for model definition
VMF.SourceGenerator.nupkg <- Source generator (references Core)
VMF.Json.nupkg           <- JSON serialization support
```

### User Project Setup

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="VMF.SourceGenerator" Version="1.0.0" />
    <!-- Runtime and Core are transitive dependencies -->
  </ItemGroup>
</Project>
```

## Migration Risks and Challenges

### 1. Reflection vs. Roslyn Symbols
VMF heavily uses Java reflection (`Class`, `Method`, `Annotation`). Roslyn source generators operate on `ISymbol` types. The API is different but equally capable. The multi-pass model construction logic ports well.

### 2. Observable Collections
Java's `VList` has specific semantics around element-level callbacks for containment. `ObservableCollection<T>` covers most needs but may require custom wrapping for containment management hooks.

### 3. Velocity Templates -> C# Code Generation
The 45+ Velocity templates contain complex conditional logic. They need to be translated to C# code-generation logic. This is the most labor-intensive part but is straightforward (just tedious).

### 4. Property Change Semantics
Java uses `PropertyChangeSupport` (JavaBeans). C# has `INotifyPropertyChanged`. The generated setter pattern needs adaptation:
- Java: `propertyChanges.firePropertyChange("name", oldVal, newVal)`
- C#: `PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)))`

### 5. No Package-Level Annotations
Java allows `@VMFModel` on any interface in the package. C# doesn't have namespace-level attributes. Alternative: use a dedicated marker interface or assembly-level attribute.

### 6. Containment Opposite Resolution
VMF uses string-based opposite property names (`opposite="parent"`). In C#, this could use `nameof()` for compile-time safety: `[Contains(Opposite = nameof(IChild.Parent))]` — but this requires the referenced type to be visible, which may create circular reference issues.

### 7. Thread Safety
VMF Java appears to be designed for single-threaded use. C# developers may expect thread safety. Document threading model clearly.

## Estimated Effort by Phase

| Phase | Complexity | Notes |
|---|---|---|
| Phase 1: Runtime | Medium | Well-defined interfaces, known patterns in C# |
| Phase 2: Attributes | Low | Straightforward port |
| Phase 3: Source Generator | High | Core complexity - model analysis + code generation |
| Phase 4: Serialization | Medium | Jackson -> System.Text.Json mapping |
| Phase 5: Build Integration | Low | Roslyn source generators handle this natively |

## Testing Strategy

Port the test suite from `test-suite/`:
- Model definitions become C# interfaces
- Test assertions map directly (JUnit -> xUnit/NUnit)
- Generated code behavior should be identical

Key test categories:
- Property access, default values
- Containment (add/remove children, parent auto-update)
- Cross-references (bidirectional linking)
- Cloning (deep/shallow)
- Equals/hashCode (all strategies)
- Change recording, undo/redo
- Immutability constraints
- Interface-only inheritance
- Delegation
- Reflection API
- Builder pattern
- Serialization round-trips
