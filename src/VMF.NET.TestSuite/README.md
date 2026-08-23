# VMF.NET.TestSuite

Behavioural test suite for VMF.NET. This is the .NET counterpart of the Java project's
[`test-suite`](https://github.com/miho/VMF/tree/master/test-suite) module: tests here run against
**generated** code, with the source generator wired in as an analyzer.

For generator-level tests that do *not* need generated code (model analysis, template rendering,
compile-gates over model source text), see **`VMF.NET.Tests`** instead — that is the counterpart
of Java's `core/src/test` plus `VMFGeneratorTest`.

## Layout

```
VMF.NET.TestSuite/
  *Tests.cs           .NET-native tests (not ports)      namespace VMF.NET.TestSuite
  Models/*.cs         models for those tests             namespace VMF.NET.TestSuite.Models
  VmfTest/<Area>/     ports of the Java test-suite       namespace VMF.NET.TestSuite.VmfTest.<Area>
```

`VmfTest/` mirrors the Java package tree so it is obvious which parts have a Java equivalent.

### Inside an area folder

Each area folder holds **both** the model interfaces and the tests, in the **same namespace**:

```
VmfTest/Containment/
  ContainmentModel.cs    namespace VMF.NET.TestSuite.VmfTest.Containment    <- [VmfModel] interfaces
  ContainmentTest.cs     namespace VMF.NET.TestSuite.VmfTest.Containment    <- [Fact] tests
```

Sharing the namespace means the tests need no `using` to see the model, and the area is one
self-contained unit — which is what lets `IParent` here and `IParent` in another area coexist.

`VmfTest/Containment/ContainmentModel.cs`:

```csharp
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Containment;

[VmfModel(Equality = EqualsType.All)]
public partial interface IParent
{
    string? Name { get; set; }
    [Contains("IChild.Parent")] VList<IChild> Children { get; }
}

[VmfModel(Equality = EqualsType.All)]
public partial interface IChild
{
    string? Name { get; set; }
    [Container("IParent.Children")] IParent? Parent { get; }
}
```

`VmfTest/Containment/ContainmentTest.cs` — same namespace, so no `using` for the model:

```csharp
using Xunit;

namespace VMF.NET.TestSuite.VmfTest.Containment;

public class ContainmentTest
{
    [Fact]
    public void Containment_is_unique()
    {
        var a = IParent.NewInstance();
        var child = IChild.NewInstance();
        a.Children.Add(child);
        Assert.Same(a, child.Parent);

        var b = IParent.NewInstance();
        b.Children.Add(child);          // containment is unique -> moves out of a
        Assert.Empty(a.Children);
        Assert.Same(b, child.Parent);
    }
}
```

### Do not reproduce Java's `vmfmodel` sub-package

In Java the model lives in `<area>.vmfmodel` and the **generated** code lands in the parent
package `<area>`, where the tests live. VMF.NET has no such redirection: a `[VmfModel]` interface
generates its implementation into **the namespace it is declared in**. Declaring models in
`...Containment.VmfModel` would force every test to write `VmfModel.IParent`.

A `VmfModel/` **subfolder** is fine if you want the visual split — just keep the `namespace` line
at the area level. Folders are irrelevant to the generator: it groups by namespace only
(`VmfSourceGenerator.Execute` reads `iface.ContainingNamespace`), and the project compiles the
SDK default `**/*.cs` glob.

## Why one namespace per area

The generator groups model interfaces **by namespace** and analyses each namespace as an
independent model (`VmfSourceGenerator.Execute` → `byNamespace` → `ModelAnalyzer.Analyze(ns, …)`),
emitting hint names as `{namespace}.{Type}.g.cs`.

So a namespace is exactly what a Java package is here: an isolation boundary. `IParent` in
`VmfTest.Containment` and `IParent` in `VmfTest.Equals` can have completely different shapes and
never collide — which is what lets the Java suite reuse `Parent`, `Child` and `Element` across
32 packages.

**Consequence:** keep each area self-contained. One namespace = one model, so a model type that
references a type in *another* namespace is not resolved as a model type (it degrades to an
external/class-typed property). Java has the same per-package boundary, so straight ports are
fine — but watch `Complex/VmfText/*`, which spans sub-packages in Java.

## Porting notes

| Java | C# |
|---|---|
| package `eu.mihosoft.vmftest.<area>` | namespace `VMF.NET.TestSuite.VmfTest.<Area>` |
| model in sub-package `<area>.vmfmodel` | **no sub-namespace** — the `[VmfModel]` interface goes directly in the area namespace |
| `interface Parent` | `public partial interface IParent` |
| `getName()` / `setName(x)` | property `Name { get; set; }` |
| `Parent.newInstance()` / `newBuilder()` | `IParent.NewInstance()` / `IParent.NewBuilder()` |
| `@Contains(opposite="parent")` | `[Contains("IChild.Parent")]` |
| `@Contains` (no opposite) | `[Contains]` |
| `@Container(opposite="child")` | `[Container("IParent.Children")]` |
| a container's generated `setParent(x)` | declare the property `{ get; set; }` — see below |
| `@Refers(opposite="…")` | `[Refers("IOther.Prop")]` |
| `@GetterOnly` / `@IgnoreEquals` / `@IgnoreToString` | `[GetterOnly]` / `[IgnoreEquals]` / `[IgnoreToString]` |
| `@Immutable` / `@InterfaceOnly` / `@ExternalType` | `[Immutable]` / `[InterfaceOnly]` / `[ExternalType]` |
| `@DelegateTo(className="…")` | `[DelegateTo(typeof(…))]` |
| JUnit `@Test` | xUnit `[Fact]` |
| `assertThat(actual, equalTo(expected))` | `Assert.Equal(expected, actual)` — **argument order flips** |
| `Assert.assertTrue(x)` | `Assert.True(x)` |

The model interface is `partial` — the generator adds `NewInstance`, `NewBuilder`, `Clone`,
`AsReadOnly` and the `Builder` type to it.

### Settable container properties

Java generates a container setter automatically, so `child.setParent(p)` and
`child.setParent(null)` are always available. VMF.NET cannot: the model interface **is** the
public API here, and a partial interface cannot add a setter to a property already declared
`{ get; }`. So a model opts in by declaring the container property settable:

```csharp
[Container("IParent.Children")]
IParent? Parent { get; set; }     // instead of { get; }
```

The generated setter detaches from the current container and then attaches by driving the
**opposite** property, so containment is established in exactly one place regardless of which
side the caller used. Setting it to `null` detaches. A `[Container]` with no declared opposite
gets no setter — there is nothing to drive.

## Porting the tests

Test classes live in the same area folder and namespace as the model, named after the Java
class (`ContainmentTest.cs`). `VmfTest/Containment/` is the worked reference.

### JUnit/hamcrest -> xUnit

| Java | C# |
|---|---|
| `@Test` | `[Fact]` |
| `assertThat(actual, equalTo(expected))` | `Assert.Equal(expected, actual)` — **argument order flips** |
| `assertThat(actual, equalTo(someModelObject))` | `Assert.Same(expected, actual)` — see note below |
| `assertThat(x, equalTo(null))` | `Assert.Null(x)` |
| `assertThat(e, isIn(list))` | `Assert.Contains(e, list)` |
| `assertThat(e, not(isIn(list)))` | `Assert.DoesNotContain(e, list)` |
| `Assert.assertTrue(x)` / `assertFalse(x)` | `Assert.True(x)` / `Assert.False(x)` |
| `Assert.fail(msg)` | `Assert.Fail(msg)` |
| expected exception | `Assert.Throws<T>(() => ...)` |

**`equalTo` on model objects.** A model without an explicit equality setting uses
`EqualsType.Instance` (reference equality), so Java's `equalTo` is identity there. Prefer
`Assert.Same` — it states the intent and does not silently change meaning if the model later
gains content equality. Use `Assert.Equal` only where the model really declares
`[VmfEquals]` / `[VmfModel(Equality = ...)]` and the test is about content.

### Two deliberate liberties

1. **Name collisions.** A Java class name that clashes with the enclosing type gets a
   suffix (`containmentTest()` -> `ContainmentTest_IsUnique`). Method names are otherwise
   PascalCased as-is so they stay greppable against the Java source.
2. **Positive pre-assertions.** Several Java facts assert only the *detach* half
   (`assertThat(ca.getElement(), equalTo(null))`), which would also pass if containment did
   nothing at all. The ports add an assertion that the attach actually happened first. This
   strengthens the fact without changing what it tests.

Anything that cannot be ported faithfully gets a `DEVIATION:` note at the top of the file,
the same as the models.

## Area mapping

Java package → C# namespace suffix (all under `VMF.NET.TestSuite.VmfTest.`).
Ports land in the matching `VmfTest/<Area>/` folder; `.gitkeep` records the source package.

| Java package (`eu.mihosoft.vmftest.`) | C# area | Java test class |
|---|---|---|
| `annotations` | `Annotations` | `AnnotationsTest` |
| `builders` | `Builders` | `BuilderTest` |
| `containment` | `Containment` | `ContainmentTest` |
| `cross_ref` | `CrossRef` | `CrossRefTest` |
| `defaultvaluesandbuilders` | `DefaultValuesAndBuilders` | `DefaultValuesAndBuildersTest` |
| `delegationinherit` | `DelegationInherit` | — (model only) |
| `equals` | `Equals` | `EqualsTest` |
| `events_undo_redo` | `EventsUndoRedo` | `UndoRedoWithContainmentTest` |
| `externaltypes` | `ExternalTypes` | `ExternalTypesTest` |
| `getteronly` | `GetterOnly` | `GetterOnlyTest` |
| `ignoretostring` | `IgnoreToString` | `ToStringTest` |
| `immutabletypes` | `ImmutableTypes` | `ImmutableTypesTest` |
| `lazyinit` | `LazyInit` | `LazyInitTest` |
| `observableprop` | `ObservableProp` | `ObservablePropTest` |
| `parentcontainment01` | `ParentContainment01` | `ContainmentTest` |
| `propertyinheritance` | `PropertyInheritance` | `PropertyInheritanceTest` |
| `propertyorder` | `PropertyOrder` | `PropertyOrderTest` |
| `propertytype` | `PropertyType` | `PropertyTypeTest` |
| `recursivelistener01` | `RecursiveListener01` | `RecursiveListenerTest` |
| `recursivelistener01.nocontainment` | `RecursiveListener01/NoContainment` | — (model only) |
| `staticreflection` | `StaticReflection` | `StaticReflectionTest` |
| `tostring` | `ToString` | `ToStringTest` |
| `complex.account` | `Complex/Account` | `AccountTest` |
| `complex.devcom` | `Complex/DevCom` | — (model only) |
| `complex.fsm` | `Complex/Fsm` | `FSMTest` |
| `complex.horses` | `Complex/Horses` | `HorsesTest` |
| `complex.library` | `Complex/Library` | `LibraryTest` |
| `complex.supplier` | `Complex/Supplier` | `SupplierTest` |
| `complex.unparsermodel` | `Complex/UnparserModel` | `UnparserModelTest` |
| `complex.vflow` | `Complex/VFlow` | `LargeFlowModelTest`, `VFlowGlobalListenerTest` |
| `complex.vmf_text.grammarmodel` | `Complex/VmfText/GrammarModel` | — (model only) |
| `complex.vmf_text.generated.json` | `Complex/VmfText/Generated/Json` | — (model only) |
| `complex.vmf_text.generated.miniclang` | `Complex/VmfText/Generated/MiniCLang` | — (model only) |

Java also has a separate `eu.mihosoft.vmftests` (**plural**) package; those are merged into the
same `VmfTest/` tree:

| Java package (`eu.mihosoft.vmftests.`) | C# area |
|---|---|
| `completepropertyordertest` | `CompletePropertyOrderTest` |
| `delegationtest` | `DelegationTest` |
| `nopropertiestest` | `NoPropertiesTest` |
| `reflectiontest` | `ReflectionTest` |
| `test1` | `Test1` |
| `test2` | `Test2` |

### Not ported

- `eu.mihosoft.vmf.VMFGeneratorTest` — generate-from-source-string then compile. Already covered
  in `VMF.NET.Tests` by `SourceGeneratorTests.FullPipeline_*` and `GeneratorCompilesTests`.
- `eu.mihosoft.vmftest.resources.MemoryResourceSetTest` — exercises Java's `MemoryResourceSet`
  I/O abstraction, which has no VMF.NET equivalent (Roslyn supplies generated sources directly).

### Notes

- `VmfTest.ToString` and `VmfTest.Equals` are namespaces named after `object` members. This is
  legal C# and was verified to compile and run; rename to `ToStringTests` / `EqualsTests` only if
  it ever causes trouble.
- The existing `*Tests.cs` at the project root are .NET-native (JSON, schema, polymorphism,
  nullable value types) and have no Java counterpart. They are deliberately left where they are.
