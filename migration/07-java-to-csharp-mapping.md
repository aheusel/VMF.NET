# Java to C# Feature Mapping

## Language-Level Mappings

### Model Definition Input

| VMF Java | C# Equivalent |
|---|---|
| Java interface | C# interface |
| `@annotation` on interface | C# `[Attribute]` on interface |
| Getter method `String getName()` | C# property declaration `string Name { get; }` |
| `boolean isActive()` | `bool IsActive { get; }` |
| `List<T>` / `T[]` return type | `IList<T>` or custom `VList<T>` |
| Package `*.vmfmodel` | Namespace `*.VmfModel` (or custom convention) |
| `extends` (interface inheritance) | `: IBaseType` (interface inheritance) |

### Example Model Translation

**Java (VMF input):**
```java
package eu.mihosoft.tutorial.vmfmodel;

import eu.mihosoft.vmf.core.*;

@VMFModel(equality = VMFEquals.EqualsType.ALL)
interface Parent {
    @Contains(opposite = "parent")
    Child[] getChildren();
    
    String getName();
    
    @DefaultValue("42")
    int getValue();
}

interface Child {
    @Container(opposite = "children")
    Parent getParent();
    
    @Required
    String getLabel();
}
```

**C# (equivalent input):**
```csharp
namespace Eu.Mihosoft.Tutorial.VmfModel;

using Eu.Mihosoft.Vmf.Core;

[VmfModel(Equality = EqualsType.All)]
public interface IParent
{
    [Contains(Opposite = "Parent")]
    IList<IChild> Children { get; }
    
    string Name { get; }
    
    [DefaultValue("42")]
    int Value { get; }
}

public interface IChild
{
    [Container(Opposite = "Children")]
    IParent? Parent { get; }
    
    [Required]
    string Label { get; }
}
```

### Generated Code

| VMF Java Generated | C# Equivalent |
|---|---|
| `public interface MyType extends VObject` | `public interface IMyType : IVObject` |
| `class MyTypeImpl implements MyType` | `internal class MyTypeImpl : IMyType` |
| `ReadOnlyMyType` interface | `IReadOnlyMyType` interface |
| `ReadOnlyMyTypeImpl` wrapper | `ReadOnlyMyTypeImpl` wrapper |
| `MyType.newInstance()` | `MyType.NewInstance()` or `new MyType()` |
| `MyType.newBuilder()` | `MyType.NewBuilder()` |
| `obj.vmf().content()` | `obj.Vmf().Content()` |
| `obj.vmf().changes()` | `obj.Vmf().Changes()` |
| `obj.vmf().reflect()` | `obj.Vmf().Reflect()` |
| `readOnly.asModifiable()` | `readOnly.AsModifiable()` (returns deep copy) |
| `Builder.applyFrom(src)` | `Builder.ApplyFrom(src)` |
| `Builder.applyTo(target)` | `Builder.ApplyTo(target)` |
| `property.isSet()` | `property.IsSet` |
| `property.unset()` | `property.Unset()` |

## Runtime Library Mappings

### Core Types

| Java Runtime | C# Equivalent |
|---|---|
| `VObject` | `IVObject` |
| `VMF` (API accessor) | `IVmf` |
| `Content` | `IContent` |
| `Changes` | `IChanges` |
| `Reflect` | `IReflect` |
| `Property` | `IProperty` |
| `Type` | `IVmfType` (avoid conflict with `System.Type`) |
| `Builder` | `IBuilder` |
| `Change` | `IChange` |
| `Transaction` | `ITransaction` |
| `ChangeListener` | `Action<IChange>` or `IChangeListener` |
| `Subscription` | `IDisposable` |
| `DelegatedBehavior<T>` | `IDelegatedBehavior<T>` |
| `Mutable` | `IMutable` |
| `ReadOnly` | `IReadOnly` |
| `Immutable` | `IImmutable` |

### Collection Types

| Java | C# |
|---|---|
| `VList<T>` (eu.mihosoft.vcollections) | `ObservableCollection<T>` or custom `VList<T>` |
| List change listeners | `INotifyCollectionChanged` / `CollectionChanged` event |
| `java.util.List<T>` | `IList<T>` |

**Important**: VMF's `VList` supports per-element change listeners for containment/reference management. `ObservableCollection<T>` provides `CollectionChanged` events but may need extension for VMF's specific needs (e.g., element-level containment callbacks).

### Property Change Notification

| Java | C# |
|---|---|
| `PropertyChangeSupport` / `PropertyChangeListener` | `INotifyPropertyChanged` / `PropertyChanged` event |
| `VMFPropertyChangeSupport` | Custom implementation or extend `INotifyPropertyChanged` |
| Subscription-based listener | `event PropertyChangedEventHandler` + unsubscription |

### Cloning

| Java | C# |
|---|---|
| `obj.clone()` | `obj.Clone()` via `ICloneable` or custom interface |
| `obj.vmf().content().deepCopy()` | `obj.Vmf().Content().DeepCopy()` |
| `obj.vmf().content().shallowCopy()` | `obj.Vmf().Content().ShallowCopy()` |

### Equals/HashCode

| Java | C# |
|---|---|
| `equals(Object)` / `hashCode()` | `Equals(object)` / `GetHashCode()` |
| `obj.vmf().content().equals(other)` | `obj.Vmf().Content().Equals(other)` |
| Identity-based (default) | `ReferenceEquals` (default) |
| Content-based (CONTAINMENT_AND_EXTERNAL) | Custom `IEqualityComparer<T>` or override |

### Streams and Iteration

| Java | C# |
|---|---|
| `Stream<VObject>` | `IEnumerable<IVObject>` |
| `stream().filter().map()` | LINQ `.Where().Select()` |
| `VIterator` | `IEnumerator<IVObject>` or custom iterator |

## Annotations to Attributes

| Java Annotation | C# Attribute |
|---|---|
| `@Contains(opposite="parent")` | `[Contains(Opposite = "Parent")]` |
| `@Container(opposite="children")` | `[Container(Opposite = "Children")]` |
| `@Refers(opposite="books")` | `[Refers(Opposite = "Books")]` |
| `@Immutable` | `[Immutable]` |
| `@InterfaceOnly` | `[InterfaceOnly]` |
| `@VMFModel(equality=...)` | `[VmfModel(Equality = ...)]` |
| `@VMFEquals(EqualsType.ALL)` | `[VmfEquals(EqualsType.All)]` |
| `@DefaultValue("42")` | `[DefaultValue("42")]` |
| `@Required` | `[Required]` (or use `System.ComponentModel.DataAnnotations.Required`) |
| `@GetterOnly` | `[GetterOnly]` |
| `@IgnoreEquals` | `[IgnoreEquals]` |
| `@IgnoreToString` | `[IgnoreToString]` |
| `@PropertyOrder(index=N)` | `[PropertyOrder(N)]` |
| `@DelegateTo(className="...")` | `[DelegateTo(typeof(MyBehavior))]` (use `Type` instead of string) |
| `@ExternalType(pkgName="...")` | `[ExternalType(Namespace = "...")]` |
| `@Doc("...")` | XML doc comments `/// <summary>...</summary>` |
| `@Annotation(key="k", value="v")` | `[VmfAnnotation(Key = "k", Value = "v")]` |
| `@SyncWith(opposite="...")` | `[SyncWith(Opposite = "...")]` |

## Primitive Type Mappings

| Java | C# |
|---|---|
| `int` | `int` |
| `long` | `long` |
| `float` | `float` |
| `double` | `double` |
| `boolean` | `bool` |
| `char` | `char` |
| `byte` | `byte` |
| `short` | `short` |
| `String` | `string` |
| `Integer` (boxed) | `int?` (nullable) or `int` |

## Serialization (Jackson Module)

| Java | C# |
|---|---|
| Jackson (`ObjectMapper`) | `System.Text.Json` or `Newtonsoft.Json` |
| `VMFJacksonModule` | Custom `JsonConverter<T>` implementations |
| JSON Schema generation | Use NJsonSchema or manual generation |
| Polymorphic serialization | `[JsonDerivedType]` (.NET 7+) or custom converters |
