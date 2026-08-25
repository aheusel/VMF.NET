# VMF.NET.TestSuite

Behavioural test suite for VMF.NET. This is the .NET counterpart of the Java project's
[`test-suite`](https://github.com/miho/VMF/tree/master/test-suite) module: tests here run against
**generated** code, with the source generator wired in as an analyzer.

For generator-level tests that do *not* need generated code (model analysis, template rendering,
compile-gates over model source text), see **`VMF.NET.Tests`** instead — that is the counterpart
of Java's `core/src/test` plus `VMFGeneratorTest`.

> **The port is complete.** Every portable Java fact has a running counterpart and nothing is
> skipped. `vmf/VMFGenerateRuns` splits across five classes here — four behavioural ones in this
> project, plus `VMFGenerateRunsValidationTests` in `VMF.NET.Tests`, because a model VMF must
> *reject* cannot sit in a compiled project.
>
> Two area folders hold a model and no test: `VmfTest/NoPropertiesTest/` and
> `VmfTest/CompletePropertyOrderTest/`. That matches Java, where both live under the models-only
> `vmftests` root and are exercised through `VMFGenerateRuns` rather than by a test class of their
> own. See [`../../devdoc/java-parity-roadmap.md`](../../devdoc/java-parity-roadmap.md).

## Layout

```
VMF.NET.TestSuite/
  *Tests.cs           .NET-native tests (not ports)      namespace VMF.NET.TestSuite
  Models/*.cs         models for those tests             namespace VMF.NET.TestSuite.Models.VmfModel
  VmfTest/<Area>/     ports of the Java test-suite       namespace VMF.NET.TestSuite.VmfTest.<Area>
```

**A model lives in a `.VmfModel` namespace**, mirroring Java's `vmfmodel` package; the generator
emits the public API into the namespace above it. So `…VmfTest.Containment.VmfModel` declares the
model and `…VmfTest.Containment` is where `IParent` lands — which is where the tests are.

`VmfTest/` mirrors the Java package tree so it is obvious which parts have a Java equivalent.

### Inside an area folder

Each area folder holds the model, the tests, and any behaviour delegates:

```
VmfTest/Containment/
  ContainmentModel.cs    namespace …VmfTest.Containment.VmfModel   <- the model: build input
  ContainmentTest.cs     namespace …VmfTest.Containment            <- [Fact] tests
```

The tests sit in the namespace the API is generated into, so they need no `using` to see it, and
the area is one self-contained unit — which is what lets `IParent` here and `IParent` in another
area coexist.

Behaviour delegates and any plain types the model references (enums, .NET classes) go in a
**separate file in the parent namespace** — `VFlowDelegates.cs`, `DevComTypes.cs` — because they
reference the *generated* types. Java arranges it the same way.

`VmfTest/Containment/ContainmentModel.cs`:

```csharp
using VMF.NET.Runtime;
using VMF.NET.Runtime.Attributes;

namespace VMF.NET.TestSuite.VmfTest.Containment.VmfModel;

[VmfEquals(EqualsType.All)]
interface Parent
{
    string? Name { get; set; }
    [Contains("Child.Parent")] Child[] Children { get; }
}

[VmfEquals(EqualsType.All)]
interface Child
{
    string? Name { get; set; }
    [Container("Parent.Children")] Parent? Parent { get; }
}
```

**Model interfaces carry no `I` prefix**, as in Java — the generator adds it. The test below still
says `IParent`, because that is the *generated* name; renaming the model changed no test file.

(A model written `IParent` also still generates `IParent`, since the rule leaves an existing
`I`+capital alone. That affordance exists so an older model migrates by moving its namespace
alone; new models should not use it.)

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

### Folders do not matter; namespaces do

The generator groups by namespace only (`VmfSourceGenerator.Execute` reads
`iface.ContainingNamespace`), and the project compiles the SDK default `**/*.cs` glob. A
`VmfModel/` subfolder is fine if you want the visual split — what counts is the `namespace` line.

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
| package `…<area>.vmfmodel` | namespace `…<Area>.VmfModel` |
| `interface Parent` | `interface Parent` — or `IParent`; both generate `IParent` |
| `getName()` / `setName(x)` | property `Name { get; set; }` |
| `Parent.newInstance()` / `newBuilder()` | `IParent.NewInstance()` / `IParent.NewBuilder()` |
| `@Contains(opposite="parent")` | `[Contains("IChild.Parent")]` |
| `@Contains` (no opposite) | `[Contains]` |
| `@Container(opposite="child")` | `[Container("IParent.Children")]` |
| a container's generated `setParent(x)` | generated too — nothing to declare |
| `@Refers(opposite="…")` | `[Refers("IOther.Prop")]` |
| `@GetterOnly` / `@IgnoreEquals` / `@IgnoreToString` | `[GetterOnly]` / `[IgnoreEquals]` / `[IgnoreToString]` |
| `@Immutable` / `@InterfaceOnly` / `@ExternalType` | `[Immutable]` / `[InterfaceOnly]` / `[ExternalType]` |
| `@DelegateTo(className="…")` | `[DelegateTo(typeof(…))]` |
| a delegate's `on<Type>Instantiated()` hook | `On<Type>Instantiated()` — same name, `I` stripped from the interface |
| `implements DelegatedBehavior<Foo>` | `: IDelegatedBehavior<IFoo>` — declare it **once**, at whichever model type suits it |
| JUnit `@Test` | xUnit `[Fact]` |
| `assertThat(actual, equalTo(expected))` | `Assert.Equal(expected, actual)` — **argument order flips** |
| `Assert.assertTrue(x)` | `Assert.True(x)` |
| `contains(a, b)` | `Assert.Equal(new[] { a, b }, list)` — **exactly these, in this order** |
| `hasItem(a)` / `hasItems(a, b)` | `Assert.Contains(a, list)` — membership only |
| `not(hasItem(a))` | `Assert.DoesNotContain(a, list)` |

The model interface itself carries no attribute and is not `partial`: it is build input. The
generator emits the public interface — the members, `NewInstance`, `NewBuilder`, `Clone`,
`AsReadOnly` and the `Builder` type — into the namespace above.

### Fidelity rules

The project goal is **behavioural identity with Java VMF wherever C# permits it** — someone
moving a model across should meet as few surprises as possible. See "Design goal" in
[`../../devdoc/java-parity-roadmap.md`](../../devdoc/java-parity-roadmap.md). The suite is how
that is measured, so a port is a translation, not a rewrite. Deviating silently makes the suite
claim coverage it does not have, and a failing ported fact is evidence of a real divergence
rather than of a bad port. So:

- **Port every statement.** Including ones that look redundant. `HorsesTest.horseTest` originally
  lost its last seven lines — the second tournament, and the assertion that a horse attends two
  of them, which is the point of tournaments being references rather than containment. Nothing
  failed; it was simply dropped, and the suite reported green.
- **Preserve assertion strength.** Hamcrest `contains(...)` asserts the exact sequence;
  translating it to a couple of `Assert.Contains` calls silently drops both the ordering and the
  count. Use the table above.
- **Preserve literals**, including non-ASCII ones. "Horst Müller" is not "Horst Mueller".
- **Give a skipped fact a real body.** `[Fact(Skip = "…")]` over an empty body is not a ported
  fact — it is a note. Write the body as it will run, then add the skip, so that un-skipping is
  a one-line change and the fact has been shown to be expressible at all.
- **Where a deviation is unavoidable, say so at the top of the file** with a `DEVIATION:` note
  giving the reason, as the model ports do.

Java's `println` calls assert nothing and are dropped. Java's assertion *messages* survive as
trailing comments, since xUnit's `Assert.Equal` takes no message argument.

### Verifying a port: read the two side by side

**Counting does not work.** A sweep comparing assertions-per-file was tried and found nothing;
reading the files afterwards found five damaged areas. It cannot work, for three separate
reasons:

- Weakening *raises* the count. `contains(a, b)` is one Java assertion; the (wrong)
  `Assert.Contains` × 2 that replaced it is two.
- Ports legitimately add assertions — several Java facts assert nothing at all, or only the
  detach half — so a dropped statement nets out against an addition.
- Excluding files that contain a skip, to avoid false positives, excludes exactly the files
  most likely to be damaged.

So the check is: open the Java fact and the C# fact together and read them statement by
statement. There is no cheaper substitute, and the cost is small next to the alternative — a
suite that reports green while asserting less than it appears to.

**Recency is not safety.** The `UnparserModelTest` block was dropped days *after* these rules
were written, by someone who knew them. Verify new ports too.

### Container setters

Generated, as Java generates them — nothing to declare in the model. The setter detaches from the
current container and then attaches by driving the **opposite** property, so containment is
established in exactly one place regardless of which side the caller used. Setting it to `null`
detaches. A `[Container]` with no declared opposite gets none — there is nothing to drive.

### Narrowed properties

Java narrows a property covariantly by overriding its getter with a narrower return type.
Translate it by re-declaring the property with `new`:

```csharp
[InterfaceOnly]
interface WithLocation      { [GetterOnly] Location? Location { get; } }

[InterfaceOnly]
interface WithLocationX : WithLocation
{
    [GetterOnly] new LocationX? Location { get; }   // `new`, not an override
}
```

C# has no covariant override for an interface property, so the redeclaration hides the base
member and the compiler asks for the intent to be stated. The generated implementation carries
the member at the narrowed type and satisfies each wider declaration with a forwarding explicit
implementation, so both views see the same object at their own type.

Two limits, both C#'s:

- **A collection cannot be narrowed.** `VList<T>` is invariant, so no forwarding implementation
  can exist; the generator reports it. Java allows it only because its properties are arrays.
- **A narrowed setter rejects a value that does not fit**, with `InvalidCastException` at the
  assignment. Java stores it and throws at the next narrowed read instead.

Re-declaring at the *same* type is a different thing and needs no special handling — it is how a
model resolves `CS0229` or restates `[PropertyOrder]`.

### Porting a behaviour delegate

Delegates translate almost literally. Three things to know:

**Declare `IDelegatedBehavior<T>` once.** Pick the same `T` Java's delegate picks — a supertype,
or `IVObject`, is fine, and the generator casts to whatever the class declares. Before M6 a
delegate had to implement the interface once per model type that used it; that is no longer
needed, and a port carrying several is stale.

**A type-level `[DelegateTo]` requires the hook.** `[DelegateTo]` on the interface makes the
generated constructor call `On<Type>Instantiated()`, so the delegate must declare it or the
generated code will not compile. `ICodeEntity` calls `OnCodeEntityInstantiated` — Java's exact
string, with the interface's leading `I` dropped. The hook is where Java's models register
change listeners, and it is often the only reason a model behaves as its test expects.

**One instance per delegate class per object.** The constructor hook and every delegated method
share it, so state a delegate stores in the hook is still there when a method reads it.

Methods on an interface carrying a type-level `[DelegateTo]` need no attribute of their own —
they inherit that behaviour class. Delegations also inherit: a subtype gets a body for a
supertype's delegated method without re-declaring it, and a re-declaration overrides.

## Porting the tests

Test classes live in the same area folder and namespace as the model, named after the Java
class (`ContainmentTest.cs`). `VmfTest/Containment/` is the worked reference.

### JUnit/hamcrest -> xUnit

> API-shape differences — `Optional<T>` versus a nullable reference, getters versus
> properties, `type()` versus `ModelType()` — are catalogued in
> [`../../devdoc/differences-to-java.md`](../../devdoc/differences-to-java.md). They explain
> why a ported assertion is sometimes shorter than its Java original.

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
