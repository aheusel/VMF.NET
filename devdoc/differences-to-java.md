# Differences to the Java version

**Status:** current · **Last verified:** 2026-08-25 · **Applies to:** all of VMF.NET

> The project's goal is **behavioural identity with Java VMF wherever C# permits it** — someone
> moving a model across should meet as few surprises as possible. This file is the catalogue of
> the surprises that remain: where the two differ, what to write instead, and why.

## What belongs here, and what does not

This is the **reference for a reader**: "Java does X, VMF.NET does Y, here is what to write."

Two neighbouring documents cover different questions, and entries should link to them rather than
restate them:

| Document | Answers |
|---|---|
| [`java-parity-roadmap.md`](java-parity-roadmap.md) | Is a given difference *forced*, a defect, or an open question? What does the ported suite prove? |
| [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md) | How do I *port a Java test* faithfully? JUnit/Hamcrest translation, fidelity rules |
| [`api-backlog.md`](api-backlog.md) | What was identified and deliberately deferred? |

A difference earns an entry here once it has been **verified against Java's implementation**, not
merely inferred from its tests. Note the date and what was read.

---

## Project layout

### The model lives in a `.VmfModel` namespace

Java puts the model in its own source root, `src/main/vmf/…/vmfmodel/`, compiles it to a throwaway
directory and generates the public API into the package above. VMF.NET does the same thing with a
namespace:

```csharp
// Model/Model.cs — build input, never shipped as API
namespace MyApp.VmfModel;

interface Parent
{
    string Name { get; set; }
    [Contains("Child.Parent")] Child[] Children { get; }
}
```

That generates `MyApp.Parent`, which is what your code uses. Being in the namespace is the whole
declaration: **no attribute marks a model type**, exactly as no annotation marks one in Java.

| Java | VMF.NET |
|---|---|
| `src/main/vmf/…/vmfmodel/Parent.java` | any file, `namespace MyApp.VmfModel` |
| package-private `interface Parent` | `interface Parent` — `internal` by default, the same default |
| generated `Parent` in the parent package | generated `Parent` in the parent namespace |
| `apply plugin: 'eu.mihosoft.vmf'` | `<PackageReference Include="VMF.NET" />` |

### The generated interface keeps the name you gave it

**You name the type; the generator does not rename it.** `Parent` generates `Parent` — the same as
Java — and `IParent` generates `IParent`, if you prefer C#'s convention. Both are supported and
neither is warned about.

Only the **implementation** name is derived, by dropping a leading `I`, because `IParentImpl`
would be a class named like an interface:

| model | interface | implementation | read-only interface |
|---|---|---|---|
| `Parent` | `Parent` | `ParentImpl` | `ReadOnlyParent` |
| `IParent` | `IParent` | `ParentImpl` | `IReadOnlyParent` |

The read-only name follows whichever convention the model chose, so a model is never half one
style and half the other.

That shared implementation name is the one thing to watch: declaring **both** `Horse` and `IHorse`
in one namespace is a clash, because both want `HorseImpl`. It is an error naming both spellings.

### An unprefixed name can collide — measured

Java never hits this, because its packages are lowercase and its BCL names differ. In C# an
unprefixed model name lands in the same namespace as everything else, and two collisions are
common enough to plan for. Measured across the 271 models of the ported test suite, **11 could
not drop the prefix**:

- **The type is named after its own namespace** (8 of them: `Account`, `Library`, `Json`,
  `Supplier`, `VFlow`, `ReflectionTest`, `GrammarModel`, `UnparserModel`). `namespace …Complex.Library`
  cannot also contain a type `Library` — `CS0118`, *is a namespace but is used like a type*. This
  is the normal shape of a domain model, not an exotic case.
- **The name collides with an implicitly-imported BCL type** (3: `Action`, `Array`, `Type`).
  `Array` was the instructive one: it did not fail in the model at all, it broke **generated code
  elsewhere**, because `Array.Empty<T>()` in a dozen generated implementations quietly resolved to
  the model's `Array` instead of `System.Array`.

So: prefer the unprefixed name, and reach for `IFoo` when the name would collide. The test suite
is written exactly that way — 260 unprefixed, 11 prefixed — which is why both spellings stay
exercised.

Behaviour delegates and any plain types the model references (enums, .NET classes) live in the
**parent** namespace, beside the generated API — as Java's delegates live in the package VMF
generates into. A model file resolves them by looking outward, so it needs no `using`.

---

## API shape

### `Optional<T>` becomes a nullable reference

*Verified 2026-08-25 against `runtime/.../core/{Reflect,Property,Change,Type}.java`.*

Java wraps "may be absent" in `Optional<T>`, which is itself never null and is interrogated with
`isPresent()` / `get()` / `orElse(...)`. VMF.NET returns a **nullable reference** instead, so
absence *is* `null` and there is no wrapper to unwrap.

| Java | VMF.NET |
|---|---|
| `Reflect.annotationByKey(String)` → `Optional<Annotation>` | `IReflect.AnnotationByKey(string)` → `IAnnotation?` |
| `Reflect.propertyByName(String)` → `Optional<Property>` | `IReflect.PropertyByName(string)` → `VmfProperty?` |
| `Property.annotationByKey(String)` → `Optional<Annotation>` | `VmfProperty.AnnotationByKey(string)` → `IAnnotation?` |
| `Change.propertyChange()` → `Optional<PropertyChange>` | `IChange.PropertyChange` → `IPropertyChange?` |
| `Change.listChange()` → `Optional<VListChangeEvent<Object>>` | `IChange.ListChange` → `VListChangeEvent?` |
| `Type.getElementTypeName()` → `Optional<String>` | `VmfType.GetElementTypeName()` → `string?` |

The last two are also methods that became properties — see *Getters become properties* below.

Translating the `Optional` idioms:

| Java | VMF.NET |
|---|---|
| `x.isPresent()` | `x != null` |
| `!x.isPresent()` / `x.isEmpty()` | `x == null` |
| `x.get()` / `x.orElseThrow()` | `x!` (after establishing it is non-null) |
| `x.orElse(fallback)` | `x ?? fallback` |
| `x.map(f).orElse(fallback)` | `x?.F() ?? fallback` |
| `x.ifPresent(f)` | `x?.F()` |

**In tests** this is why a ported assertion is often one call shorter than its Java original.
`AnnotationsTest.basicAnnotationTest` is the worked example:

```java
// Java
Assert.assertTrue(annotatedModel.vmf().reflect().annotationByKey("key 1").isPresent());
Assert.assertFalse(annotatedModel.vmf().reflect().annotationByKey("key 3").isPresent());
```

```csharp
// VMF.NET
Assert.NotNull(annotatedModel.VMF.Reflect.AnnotationByKey("key 1"));
Assert.Null(annotatedModel.VMF.Reflect.AnnotationByKey("key 3"));
```

Same question, same strength — and marginally stronger, since `Assert.NotNull` would also catch a
`null` where Java could only catch an empty `Optional`. Nothing is dropped, so this needs no
`DEVIATION:` note in a port.

`Optional.map(...).orElse(Collections.EMPTY_LIST)` collapses the same way, which is why a
multi-line Java chain sometimes ports to a single expression.

### Getters become properties

`getName()` / `setName(x)` become the property `Name { get; set; }`; `isFoo()` becomes `Foo`.
A no-argument Java method that reads state may therefore surface as a C# property — see
`Change.propertyChange()` → `IChange.PropertyChange` above.

### `type()` is called `GetModelType()`

*See [`java-parity-roadmap.md`](java-parity-roadmap.md), "M5 design note".*

Java generates `static Type type()` on every model interface. C# cannot use that name: a model may
declare a property called `Type` (VFlow's `WithType` does), and a method cannot share a name with
a property. The static entry point is `static VmfType GetModelType()`.

### Collections — same as Java: the model writes an array

A multi-valued property is declared as an **array**, exactly as in Java, and the generator
produces a `VList<T>` property:

```csharp
IConnection[] Connections { get; }      // model  (Java: Connection[] getConnections())
VList<IConnection> Connections { get; } // generated API
```

The model never names the collection type, so the generated API can change it without breaking
code written against it. Naming it directly — `VList<T>`, `IList<T>`, `List<T>`, … — is an
**error**, not a second accepted spelling; the generator says which array to write instead.

Arrays are the notation for **properties**. A delegated *method*'s return type is passed through
as written, so a method returning a collection still says `VList<T>`. Java is the same, and its
own `MiniClangModel` keeps `//ControlFlowScope[] parentScopes();` commented out directly above the
`VList` form.

What does not change is **variance**: Java's arrays are covariant, `VList<T>` is invariant, so a
collection property still **cannot be narrowed** in a subtype. The generator reports that rather
than emitting code that will not compile. The array notation is a model-authoring convention; it
does not make the generated collection covariant.

### Graph traversal is a LINQ sequence, not a `Stream`

Java's `Content` offers `stream()`, `stream(Class<T>)`, and strategy overloads of each, because
Java has no LINQ. C# does, so four members collapse to two:

| Java | VMF.NET |
|---|---|
| `content().stream()` | `Content.Traverse()` |
| `content().stream(VNode.class)` | `Content.Traverse().OfType<VNode>()` |
| `content().stream(strategy)` | `Content.Traverse(strategy)` |
| `content().iterator()` | `Content.Cursor()` |

`Traverse` yields **the object itself first**, then depth-first through every model-typed
property. Note it is a **graph** walk by default, not a containment tree: it follows `[Refers]`
cross-references and `[Container]` back-pointers too, so it can reach objects that are not
descendants. `Traverse(IterationStrategy.ContainmentTree)` is the containment-only variant.

`stream(Class<T>)` disappears because `OfType<T>` already *is* that operation — the old
`Stream<T>()` was literally implemented as `Stream().OfType<T>()`.

`Stream` was also a poor name in .NET, where `System.IO.Stream` is a byte stream.

**`Cursor()` is not a sequence, and that is deliberate.** It is the modify-while-traversing tool —
`Set`, `Add`, `IsAddSupported` — which no `IEnumerable` can express, so it survives as its own
thing. It is consumed once and does **not** implement `IEnumerable<T>`. It used to, returning
itself from `GetEnumerator()`, which meant a second `foreach` over the same instance silently
yielded nothing. Use `Traverse()` for reading.

---

### `AsModifiable()` is on the generated interface, not on `IReadOnly`

*Added 2026-08-30, after porting VMF-Tutorial-07 found it missing entirely.*

`readOnly.AsModifiable()` returns a modifiable **deep copy**, matching Java exactly —
`read-only-implementation.vm:198` is `return this.mutableObject.clone();`, and `clone()` is
`_vmf_deepCopy` over an `IdentityHashMap`, so shared references and cycles are preserved. It is
never an alias to the wrapped object, which is what makes handing out a read-only view safe.

It is declared under the same guards Java uses:

| model type | Java | VMF.NET |
|---|---|---|
| ordinary | `asModifiable()` | `AsModifiable()` |
| interface-only | absent (`#if(!interfaceOnly)`) | absent (`if !type.IsInterfaceOnly`) |
| immutable | absent — the immutable interface extends `Immutable`, not `ReadOnly` | absent — immutables take the other branch of `ReadOnlyInterface.sbn` |

**The one difference:** Java also declares `asModifiable()` on the `ReadOnly` marker interface,
so it can be called through a bare `ReadOnly` reference. VMF.NET's `IReadOnly` stays a pure
marker, for two reasons. Java's declaration is scaffolding — its body is
`throw new UnsupportedOperationException("FIXME: … This should not happen :(")`, overridden by
every generated type. And VMF.NET's immutable read-only interfaces *do* extend `IReadOnly`
where Java's extend `Immutable` instead, so declaring it there would force a throwing
implementation onto exactly the types Java excludes at compile time — trading a compile error
for a runtime one, the wrong direction.

Consequence: to call `AsModifiable()` you need the generated read-only interface
(`ReadOnlyFoo`), not `IReadOnly`. Generic code holding `IReadOnly` still has `Clone()` via
`IVObject`, which returns a read-only clone.

Note the difference from `VMF.Content.DeepCopy<T>()` on a read-only view, which copies **and
re-wraps** — it can only ever give you a read-only result. That asymmetry is why
`AsModifiable()` has to exist as its own method.

---

## Model declaration

### An opposite is named by type and property

Java names the opposite by property alone, resolving it against the property's own type:

```java
@Contains(opposite="parent")   Child[] getChildren();
@Container(opposite="children") Parent getParent();
```

VMF.NET names the type too, because the attribute argument is a plain string with no such context.
Either spelling of the type works — the model's own name, or the generated one:

```csharp
[Contains("Child.Parent")]      Child[] Children { get; }   // model names
[Contains("IChild.Parent")]     IChild[] Children { get; }  // generated names
```

### `[ExternalType]` names a type outside the model

Java's model package is compiled on its own, so a type declared elsewhere needs a stand-in:

```java
@ExternalType(pkgName = "com.example") interface Payload {}
```

VMF.NET has the same device, and it works the same way — the stand-in lives in the model
namespace, carries the attribute, and generated code references `com.example.Payload`:

```csharp
[ExternalType("Com.Example")] interface Payload { }
```

**In practice you rarely need it.** A C# model can name the real type directly, because the model
is compiled alongside it and Roslyn resolves it — which is what every ported model does, and why
`DevCom`'s enums and `ExternalTypes`' `MyType` are plain declarations in the parent namespace
rather than stand-ins. Reach for `[ExternalType]` when porting a Java model verbatim.

### `[Container]` setters are generated, as in Java

Nothing to declare. A `[Container]` with an opposite gets a setter on the generated interface, the
way Java always generates `child.setParent(p)`; setting it to `null` detaches. A `[Container]`
with no declared opposite gets none — there is nothing to drive.

This used to require `{ get; set; }` in the model. It no longer does, because the model interface
is no longer the API.

### A narrowed property needs `new`

*See [`java-parity-roadmap.md`](java-parity-roadmap.md), "M7 design note".*

Java narrows a property by overriding its getter with a narrower return type. C# has no covariant
*override* for an interface property, so the redeclaration hides the base member and the compiler
asks for the intent to be stated:

```csharp
interface WithLocationX : WithLocation
{
    [GetterOnly] new LocationX? Location { get; }
}
```

The generated implementation carries the member at the narrowed type and satisfies each wider
declaration with a forwarding explicit implementation, so both views see the same object at their
own type. Reading is Java's behaviour exactly — the wide view and the narrow view return the same
instance.

`new` itself is a **source-level** difference, not a behavioural one. Two behavioural differences
do remain. Both measured 2026-08-30 and pinned by `NarrowingTests` / `ModelDiscoveryTests`; before
that they were design notes with no test behind them.

#### 1. A narrowed setter rejects a bad value earlier than Java

Assigning through the **wide** declaration a value that does not fit the narrowed one:

```csharp
GlyphHolder wide = roundHolder;
wide.Value = boxy;              // Boxy is a Glyph, but not a Round
```

| | when it fails | what is stored |
|---|---|---|
| Java | at the next **narrowed read** | the non-fitting value |
| VMF.NET | at the **assignment**, `InvalidCastException` | nothing — the property is unchanged |

Both reject it; VMF.NET's failure is the earlier and the more local of the two, and it leaves the
object consistent rather than holding a value its own narrowed getter cannot return. Code that
relies on the Java timing — assigning a wrong value and only failing later, or never, if nothing
reads it narrowly — behaves differently here.

#### 2. A collection cannot be narrowed at all

Java narrows a collection by overriding the getter with a narrower element type. `VList<T>` is
invariant, so a narrowed declaration cannot implement the base one, and VMF.NET rejects it at
build time rather than generating a type that fails to satisfy its own interface:

```
error VMF001: Property 'RoundHolder.Values' re-declares 'GlyphHolder.Values' with a different
collection type ('…Glyph[]' -> '…Round[]'). A collection property cannot be narrowed: VList<T>
is invariant, so the base declaration cannot be implemented. Declare both at the same element
type.
```

Declare both at the same element type and narrow on read (`OfType<Round>()`) instead. This is a
hard limit of the type system, not an implementation gap — nothing in VMF.NET can lift it while
the generated API exposes `VList<T>`.

### A member inherited from two unrelated interfaces must be re-declared

C# reports `CS0229` for an ambiguous inherited member. Re-declare it with `new` at the same type;
this is bookkeeping, not narrowing, and changes no behaviour.

### `[DelegateTo]`

*See [`java-parity-roadmap.md`](java-parity-roadmap.md), "M6 design note".*

| Java | VMF.NET |
|---|---|
| `@DelegateTo(className="...")` | `[DelegateTo(typeof(...))]` |
| `implements DelegatedBehavior<Foo>` | `: IDelegatedBehavior<IFoo>` — declared **once**, at whichever model type suits it |
| `on<Type>Instantiated()` | `On<Type>Instantiated()` — same string, with the interface's leading `I` dropped |

A type-level `[DelegateTo]` requires the hook method, as in Java: the generated constructor calls
it, so its absence is a compile error.

**`ToString()` may be delegated**, as in Java:

```csharp
[DelegateTo(typeof(StoreDelegate))] string ToString();
```

The generator then emits no `ToString()` of its own — Java guards the same block with
`ModelType.isToStringMethodDelegated()` (`impl/to-string.vm`). Two details follow Java rather
than falling out for free:

- The delegating method is emitted as `public override string ToString()`. In Java
  `public String toString()` overrides on its own; in C# it would merely *hide*
  `object.ToString()`, so a base-typed reference — including `Console.WriteLine(obj)` —
  would print the structural form instead.
- The internal recursive helper (`VmfToString`) still exists and returns the delegated
  `ToString()`, so a **parent** printing this object contributes the custom representation, not
  the structural one. Java's `__vmf_toString` is `sb.append(toString())` for exactly this.

*Fixed after 0.3.0. Before that, declaring it emitted the generated `ToString()` and the
delegating one, and the generated file did not compile (CS0111). No test on either side covered
it — Java's suite does not, only its Tutorial 12 does.*

---

## Not implemented

Features Java VMF has and VMF.NET does not. Distinct from the rest of this file: these are not
differences in how something behaves, but things that are simply absent.

### `ModelDiff` — graph diff, apply and merge

*Found 2026-08-25 while reconciling the test suite by path against all three Java roots.*

Java exposes `eu.mihosoft.vmf.runtime.core.diff.ModelDiff` (239 lines):

| Java | purpose |
|---|---|
| `ModelDiff.diff(VObject a, VObject b)` → `List<Change>` | the changes that turn `a` into `b` |
| `ModelDiff.apply(VObject target, List<Change> diff)` | replay them onto a graph |
| `ModelDiff.merge(T template, T override)` → `T` | merge two graphs |
| `ModelDiff.PropChange` | a `Change` + `PropertyChange` with `undo()` |

**VMF.NET has no equivalent** — no `Diff` type in `VMF.NET.Runtime` or `VMF.NET.Core`. There is no
workaround to offer beyond comparing and assigning properties yourself; `VMF.Content` and
`VMF.Changes` are related but answer different questions (content equality, and changes *as they
happen* rather than between two arbitrary graphs).

Consequence for the parity claim: Java's `vmftest/diff/ModelDiffTest` (2 facts) is the **only**
unported class in the suite, and it is unported because the feature is missing, not because the
test resists porting. See
[`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md) for the full
reconciliation.

`ModelDiff.PropChange.undo()` suggests the diff machinery leans on the same change
infrastructure VMF.NET already has, so this is likely additive rather than structural — but that
is an impression from reading the signatures, not a verified plan.

---

## Behaviour

### Read-only violations are compile-time, not runtime

The guarantee is the same on both sides and the *observation* differs. Java's read-only interface
exposes no setter either, so plain Java would not compile the attempt — but its fact
(`VMFGenerateRuns.testReadOnlyFeature`) evaluates `roBean.setName("test")` through a **Groovy**
shell, where the failure surfaces at runtime as `MissingMethodException`. C# has no equivalent
evaluation step, so the ported fact asserts what makes the statement uncompilable: the read-only
view exposes no setter. See the note in
[`DaBeanTest.cs`](../src/VMF.NET.TestSuite/VmfTest/Test1/DaBeanTest.cs).

### `IsSet` on a collection with no declared default

Measured 2026-08-25 against a real Java run (vmf 0.2.9.7-SNAPSHOT), so this is no longer an open
question. See [`VMF.NET.JavaProbe`](../../VMF.NET.JavaProbe/README.md).

| case | Java | VMF.NET |
|---|---|---|
| collection **with** a declared default | compares against the default | `SequenceEqual` against the default — **agree** |
| collection **without** a default, non-empty | `true` | `true` (`Count > 0`) — **agree** |
| collection **without** a default, **empty** | `true` | `false` — **diverge** |
| containment collection, empty | `true` | `false` — **diverge** (containment can never declare a default) |

So the divergence is one cell wide: an **empty collection that never declared a default**.

Java's `_vmf_isSetById` is `!Objects.equals(getDefault(), get())`; for such a property the
generated default is `null` while the getter always materialises a non-null `VList`. The
comparison can therefore never hold, so `isSet()` is a **constant `true`** — `add`, `remove`,
`set(empty)` all leave it `true`, and the `unset()` that would be its natural partner throws
`NullPointerException` (`addAll((String[]) null)`).

**VMF.NET keeps `Count > 0` deliberately.** Adopting Java's answer would import a value that
cannot vary and a companion method that throws. This is the one place the parity goal (C-1) is
knowingly not followed, because the Java behaviour is a defect rather than a design. Revisit if
upstream fixes it.

### `Annotations()` and VMF's bookkeeping entries — **not a difference**

This was recorded as a divergence and was **wrong**. Measured 2026-08-25 on the same run: Java
emits the bookkeeping annotations exactly as VMF.NET does.

- `vmf:property:containment-info` — emitted by **both**, on **every** property, with the same
  values: `none`, `contained:<opposite>`, `container:<opposite>`.
- `vmf:type:immutable` — emitted by **both** on immutable types.
- `vmf:type:interface-only` — present in both code generators but **unreachable in both**: an
  interface-only type gets no implementation, and the annotation array lives in the
  implementation. Dead code on both sides.

Java's own `AnnotationsTest` is the tell: it asserts an **exact** size for type-level annotations
but **filters by key** for property-level ones — which is what you write when property annotations
carry an extra entry you do not want to count. The ported assertions filter for the same reason
Java's do, not because VMF.NET differs.

---

## Naming conventions

Mechanical, expected by anyone writing C#, and **not** divergences:

| Java | VMF.NET |
|---|---|
| `interface Parent` | `interface Parent` — plain, in a `.VmfModel` namespace; generates `Parent` |
| `getName()` / `setName(x)` | `Name { get; set; }` |
| `Parent.newInstance()` / `newBuilder()` | `IParent.NewInstance()` / `IParent.NewBuilder()` |
| `@Contains` / `@Container` / `@Refers` | `[Contains]` / `[Container]` / `[Refers]` |
| `@GetterOnly` / `@IgnoreEquals` / `@Immutable` | `[GetterOnly]` / `[IgnoreEquals]` / `[Immutable]` |
