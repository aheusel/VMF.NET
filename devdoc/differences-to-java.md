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

That generates `MyApp.IParent`, which is what your code uses. Being in the namespace is the whole
declaration: **no attribute marks a model type**, exactly as no annotation marks one in Java.

| Java | VMF.NET |
|---|---|
| `src/main/vmf/…/vmfmodel/Parent.java` | any file, `namespace MyApp.VmfModel` |
| package-private `interface Parent` | `interface Parent` — `internal` by default, the same default |
| generated `Parent` in the parent package | generated `IParent` in the parent namespace |
| `apply plugin: 'eu.mihosoft.vmf'` | `<PackageReference Include="VMF.NET" />` |

The generated name is `I` + the model's name, unless the model already starts with `I` followed by
a capital — so a model named `IParent` still yields `IParent`.

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

### `type()` is called `ModelType()`

*See [`java-parity-roadmap.md`](java-parity-roadmap.md), "M5 design note".*

Java generates `static Type type()` on every model interface. C# cannot use that name: a model may
declare a property called `Type` (VFlow's `WithType` does), and a method cannot share a name with
a property. The static entry point is `static VmfType ModelType()`.

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
interface IWithLocationX : IWithLocation
{
    [GetterOnly] new ILocationX? Location { get; }
}
```

The generated implementation carries the member at the narrowed type and satisfies each wider
declaration with a forwarding explicit implementation, so both views see the same object at their
own type. Two limits remain: a **collection** cannot be narrowed (see above), and a narrowed
setter rejects a value that does not fit with `InvalidCastException` at the assignment, where Java
stores it and throws at the next narrowed read.

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
| `interface Parent` | `interface Parent` — plain, in a `.VmfModel` namespace; generates `IParent` |
| `getName()` / `setName(x)` | `Name { get; set; }` |
| `Parent.newInstance()` / `newBuilder()` | `IParent.NewInstance()` / `IParent.NewBuilder()` |
| `@Contains` / `@Container` / `@Refers` | `[Contains]` / `[Container]` / `[Refers]` |
| `@GetterOnly` / `@IgnoreEquals` / `@Immutable` | `[GetterOnly]` / `[IgnoreEquals]` / `[Immutable]` |
