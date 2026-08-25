# Changelog

Notable changes per release. Earlier releases are listed on the
[GitHub releases page](https://github.com/aheusel/VMF.NET/releases).

## Unreleased

### Changed — model declaration (breaking)

**A model is now declared by its namespace, not by an attribute.** An interface in a namespace
whose last segment is `VmfModel` is a model type; the public API is generated into the namespace
above it. This mirrors Java, where the model lives in a `vmfmodel` package and VMF generates into
the package above.

```csharp
// before
namespace MyApp;

[VmfModel]
public partial interface IParent
{
    string? Name { get; set; }
    [Contains("IChild.Parent")] VList<IChild> Children { get; }
}

// after
namespace MyApp.VmfModel;

interface IParent
{
    string? Name { get; set; }
    [Contains("IChild.Parent")] IChild[] Children { get; }
}
```

**Migrating a model:** move it to a `.VmfModel` namespace, then delete `[VmfModel]`, `partial` and
`public`, and rewrite collection properties as arrays (see below). Nothing else changes — not the
attributes, not the scalar property types, not the opposite strings. The generated name is `I` +
the model's name **unless it already begins with `I` and a capital**, so a model named `IParent`
still produces `IParent` and no consumer code moves. The ported test suite migrated this way
without a single test file being edited.

Two things must move out of the model file into the parent namespace: **behaviour delegate
classes** and any **plain types the model references** (enums, .NET classes). They refer to the
generated types, and Java keeps them in the generated-into package for the same reason.

A model interface may now be written Java-style — `interface Parent` — and still produces
`IParent`.

- **`[VmfModel]` is no longer a marker.** It survives only as optional model-wide configuration
  (`[VmfModel(Equality = …)]` sets the default for a whole namespace); delete every bare use.
- **A settable `[Container]` no longer needs `{ get; set; }`.** The setter is generated whenever
  the container has an opposite, as Java generates one unconditionally.
- **Model interfaces are no longer `partial`** and need not be `public` — `internal`, C#'s default
  at namespace scope, matches Java's package-private model interfaces.

### Changed — collections are declared as arrays (breaking)

**A multi-valued property is now declared as an array, as in Java.** The generator produces the
`VList<T>` property from it:

```csharp
// before (model)                      // after (model)
VList<IChild> Children { get; }        IChild[] Children { get; }
```

The generated API is unchanged — still `VList<IChild> Children { get; }` — so **no consumer code
moves**; only model files do.

The point is that the model no longer names the collection type, so the generated API is free to
change it without breaking code written against it. That is also why naming it directly
(`VList<T>`, `IList<T>`, `List<T>`, …) is now an **error** rather than a second accepted spelling:
left tolerated, such a property would silently classify as a plain single-valued reference. The
diagnostic names the array to write instead.

Arrays are the notation for **properties**. A delegated *method*'s return type is passed through
as written, so a method returning a collection still says `VList<T>` — Java behaves the same way.

Variance is unaffected: `VList<T>` is invariant, so a collection property still cannot be narrowed
in a subtype.

### Changed — `Vmf()` is the property `VMF` (breaking)

**`obj.Vmf()` becomes `obj.VMF`**, and the three accessors it returns become properties too:

```csharp
// before                                  // after
obj.Vmf().Reflect().PropertyByName("X")    obj.VMF.Reflect.PropertyByName("X")
obj.Vmf().Changes().AddListener(f)         obj.VMF.Changes.AddListener(f)
obj.Vmf().Content().ContentEquals(other)   obj.VMF.Content.ContentEquals(other)
```

All four read state and take no arguments, which is a property in C#. `IVmf.Behavior<T>()` stays
a method, because a property cannot be generic; so does `VmfType.Reflect()`, which is a different
member on a different type.

The name does not collide with the `VMF` namespace: a qualified type name such as
`VMF.NET.Runtime.IVmf` is resolved in type position, where member lookup is not consulted.
A model declaring its own property named `Vmf`/`VMF` would collide with `IVObject.VMF`, the same
way one named `Type` collides with `ModelType()`.

### Fixed — IntelliSense in projects that reference the generator by project reference

A project feeding Scriban to the compiler from a target hooked `BeforeTargets="CoreCompile"` got no
IntelliSense in Visual Studio: design-time builds do not run `CoreCompile`, so the generator was
loaded without its dependency, failed, and produced no types — while `dotnet build` worked. The
in-repo test suite did this; consumers using the NuGet package were never affected, since NuGet
wires `analyzers/dotnet/cs` itself.

### Fixed — model discovery

- **An unrelated interface could be turned into a VMF model.** Discovery used to accept any
  interface carrying a VMF attribute, matched by **simple name only**. `Required`,
  `DefaultValue` and `Doc` collide with `System.ComponentModel.DataAnnotations.RequiredAttribute`,
  `System.ComponentModel.DefaultValueAttribute` and anyone else's `Doc`, so a validated DTO in the
  same project silently generated a full implementation. Measured: five files from an interface
  with no VMF attribute anywhere. Attribute matching now requires the
  `VMF.NET.Runtime.Attributes` namespace, and discovery no longer looks at attributes at all.
- **A model interface carrying no attribute was invisible**, which is why `[VmfModel]` was
  mandatory in the common case. A plain `interface Named { string Name { get; set; } }` now works.

### Added

- **`VMF.NET` metapackage.** One reference replaces the two the setup used to need, matching
  Java's single `apply plugin: 'eu.mihosoft.vmf'`:

  ```xml
  <PackageReference Include="VMF.NET" Version="…" />
  ```


- **Recursive change listeners now work.** A listener registered on a root sees changes anywhere
  in the subtree that root contains. Previously a change on a contained object reached no
  listener at all.
- **Read-only views can observe changes.** `readOnly.VMF.Changes` returns the wrapped
  object's manager instead of throwing, and reflective access through a read-only view works —
  including `AddChangeListener` on a property obtained from it. Writes are still refused.
- **`VList.AddRange(IEnumerable<T>)` and `VList.RemoveAll(params int[])`**, each raising a single
  change carrying every affected element rather than one change per element.
- **Settable `[Container]` properties.** A model can declare a container property
  `{ get; set; }`, letting containment be driven from the child (`child.Parent = p`, or `null`
  to detach). A `[Container]` with no declared opposite gets no setter.
- **Reflection metadata is populated.** `Reflect.Annotations()` (and `AnnotationByKey` /
  `AnnotationsByKey`) return the type's annotations instead of always being empty;
  `Reflect.AllTypes()` returns every model type instead of just the one asked about; and
  `VmfType.SuperTypes()` is populated.
- **Static type reflection.** Every generated model interface gains `static VmfType ModelType()`,
  and `VmfType.Reflect()` gives reflection without an instance. Reading metadata works; anything
  needing an object — `Get`, `Set`, `Unset`, `IsSet`, listeners — throws. (Java calls this
  `type()`; C# cannot, because a model may declare a property named `Type`.)
- **Per-instance default values.** `VmfProperty.SetDefault(value)` sets a property's default for
  one object. A property that was unset stays unset and follows the new default, so its value
  changes with it. Containment properties refuse it.

- **A change fires when a child's container changes.** Attaching a child to a containment
  property, or detaching it, now raises a property change on the *child* naming its container
  property. It is reported to listeners on the child only, and is not recorded — the containment
  change belongs to the container and is recorded there.

- **Nested builders may be passed unbuilt.** `With*` accepts `T.Builder` (and
  `params T.Builder[]`) and builds them during `Build()`, so a builder handed over can still be
  modified afterwards.
- **Collection default values.** `[VmfDefaultValue]` now applies to collection properties; the
  list is seeded from the expression on first access, and a list still holding exactly its
  default reports `IsSet` as false.
- **`VListChangeEvent.Source`** — the list a change happened on, so a listener can reach and
  modify the list it is observing.
- **A type-level `[DelegateTo]` now runs an instantiation hook.** Placing `[DelegateTo]` on the
  model interface makes the generated constructor create the delegate and call
  `On<TypeName>Instantiated()` on it — where `TypeName` is the interface with its leading `I`
  stripped, so `ICodeEntity` calls `OnCodeEntityInstantiated`. This is the model's hook for
  running code at instantiation, such as registering a change listener. A type-level
  `[DelegateTo]` also supplies the behaviour class for methods on that interface that carry no
  `[DelegateTo]` of their own.

  **This is a breaking change for models that already carry a type-level `[DelegateTo]`:** the
  delegate class must now declare the hook method, or the generated code will not compile.

- **Covariant property narrowing.** A subtype may re-declare an inherited property at a narrower
  type — `ILocation` to `ILocationX` to `ILocationXY`, or `object?` to `int?`. The generated
  implementation carries the member at the narrowed type and satisfies every interface that
  declares it wider with a forwarding explicit implementation, so `gCode1.Location` is an
  `ILocationXY`, `((IWithLocation)gCode1).Location` is an `ILocation`, and both are the same
  object. Reflection reports the narrowed type.

  Declare the narrowing with `new` on the model interface: C# has no covariant *override* for an
  interface property, so the redeclaration hides the base member and the compiler asks for the
  intent to be stated. A **collection** property cannot be narrowed — `VList<T>` is invariant —
  and the generator now reports that rather than emitting code that will not compile.

### Changed

- **`[DelegateTo]` is inherited.** A subtype now gets a body for a delegated method declared on
  a supertype, instead of the method having to be re-declared with its own attribute on every
  concrete type. Where both declare one, the subtype's wins. Type-level delegations inherit the
  same way, and exactly one survives per type — the nearest in the hierarchy.
- **One delegate instance per behaviour class per object**, rather than one per delegated method,
  so a delegate can keep state between calls and the constructor hook shares it with the methods.
  `SetCaller` is called once, when the delegate is created.
- **The `IDelegatedBehavior<T>` cast now reads `T` from the delegate class** rather than assuming
  the type being generated. A delegate written against a supertype — or against `IVObject` —
  serves every subtype, so it no longer has to implement `IDelegatedBehavior<T>` once per model
  type that uses it.

### Fixed

- **Container properties were told apart by type instead of by identity.** An object is contained
  through at most one container property, and which one is recorded in the container property id.
  The generated getter tested the *container's runtime type* instead, so a type declaring two
  container properties that name the same containing type could not tell them apart: both
  reported the container, and only one of them was true. The detach path had the same flaw and
  was worse — it removed the object from **every** containment of a matching type, including the
  one it was being added to, so moving an object between two lists of the same container left it
  in neither list while still reporting a container. Both now key on the property id, as Java
  does, and reflection agrees with the getter.

- **`Clone()` and `DeepCopy()` collapsed distinct objects into one.** The identity map that keeps
  a doubly-reached object from being copied twice compared keys with `Equals`, so under content
  equality two *distinct* but content-equal objects were treated as the same key. Cloning a graph
  containing content-equal siblings silently lost objects and shared what the original did not.
- **Cross-reference lists accepted duplicates.** Adding the same element repeatedly left several
  entries; a cross-reference holds one reference per element.
- **`VMF.Content` iteration visited some objects more than once.** `Iterator()` and `Stream()`
  defaulted to `UniqueProperty`, which visits each *property* once and so emits a node once per
  reference to it — a 21-node tree streamed as 41 entries. Both now default to `UniqueNode`, each
  object exactly once, matching Java. Pass `IterationStrategy.UniqueProperty` explicitly for the
  old behaviour.
- **A missing `@vmf-type` discriminator.** Whether to write the discriminator depends on any
  supertype of the serialised type being used as a property type somewhere in the model, but the
  check could not see the properties of any type other than the object's own, so it answered "no"
  whenever the supertype was used on a *different* type. Values written that way could not be read
  back into a slot typed as the supertype. Serialising a subtype standalone now carries the
  discriminator.
- **JSON schema generation no longer depends on unrelated types.** `definitions` covered every
  model type, so one type carrying a malformed schema annotation broke schema generation for every
  other type in the same namespace. It now covers only the types the schema actually references.

### Behaviour changes

- `IChanges.AddListener(listener, recursive: false)` now means what it says. Previously the flag
  was recorded and ignored, so every listener behaved recursively — which was invisible because
  no subtree change ever arrived. Code that passed `false` and relied on seeing everything will
  now see less; the default (`AddListener(listener)`) is still recursive.
- A listener on a root now receives changes from contained objects. Anything counting changes on
  a root of a non-trivial object graph will see more of them.
- `ReadOnly*Impl.VMF.Changes` no longer throws `InvalidOperationException`.
- Reflection through a read-only view no longer throws "Cannot access property without an
  instance"; reads and listeners work, and writes throw "Cannot modify unmodifiable object".
- **`ToString()` now renders Java's shape**: `{"@type":"Type", "prop": …}` with the type as a
  member rather than a prefix, every scalar quoted, and `{skipping recursion}` as the cycle
  marker. Properties are ordered by custom index or by name, independently of the reflection
  order. Anything parsing the old format will need updating.
- `IChangeInternal.IsCrossRefChange` and `IsContainmentChange` were removed — both unreachable,
  on an internal interface, with no callers.
- `IVObjectInternal` gained `GetChangesManager`, and `IVObjectInternalModifiable` lost
  `SetModelToChanges` — it was never called, and routing changes up the container chain makes it
  obsolete. This affects hand-written implementations of those internal contracts only.

## 0.2.1

A fix release. Nineteen defects, found by porting the Java VMF `test-suite` into
`VMF.NET.TestSuite` and running it against the generator. No API was removed and no
attribute renamed, so upgrading from 0.2.0 needs no source changes — but several fixes
correct behaviour that was previously wrong, so read *Behaviour changes* below.

### Fixed — code generation

Eight shapes that the Java test-suite models exercise produced code that did not compile,
or metadata that was silently wrong:

- **`[InterfaceOnly]` base types.** The implementation emitted explicit `Clone()`/`AsReadOnly()`
  for every inherited type although the interface never declares them (CS0539), and six
  template sites referenced an `…Impl` class that an interface-only type never gets (CS0246).
- **`[GetterOnly]` on a mutable type.** Reflective `SetPropertyValueById` and the builder's
  `Build()` assigned through a property that has no setter (CS0200). Both now assign the
  backing field; `ApplyTo()` skips the property.
- **Diamond inheritance.** A property reachable only transitively (`C : A, B`, both extending
  `Root`) was dropped and left unimplemented (CS0535).
- **Property ids.** Inherited properties shared one `PropertyInfo` instance, so each type's id
  assignment overwrote the others, producing duplicate switch cases and duplicate locals.
- **Re-declared (`new`) properties.** Only the declaring interface received an explicit
  read-only implementation, leaving the base member unimplemented (CS0738).
- **Containment lookup.** `FindAllPropsThatContainType` matched both inheritance directions, so
  a base type emitted cleanup casting `this` to a derived element type (CS0030).
- **Delegation.** A `void` delegated method produced `System.Void` as its return type (CS0673),
  and overloaded delegated methods collided on a single generated field (CS0102).
- **`ToString()` over a collection of value types** used `x?.ToString()` on a non-nullable
  element (CS0023).

Also fixed:

- **`[VmfEquals]` was silently ignored.** The extractor read a named `Equality` argument, which
  the attribute does not expose, so a per-type equality strategy never took effect.
- **Property ordering for a re-declared property.** Re-declaring a property is how a subtype
  restates `[PropertyOrder]`, but the restated indices had no effect: the property was
  collected during the inherited pass and kept its position in the base. A property re-declared
  on the deriving type is now ordered with that type's own properties.

### Fixed — runtime

- **Stack overflow on bidirectional cross-references.** Setting one side of a single-valued
  cross-reference recursed until the process died. The generated setter assigned its backing
  field only *after* syncing the opposite, so the `ReferenceEquals` guard still saw the old
  value and the two setters bounced forever.
- **Listeners silently orphaned.** `Vmf()` built a new `VmfImpl` on every call and `Changes()`
  stores its `ChangesManager` on that instance, so a second `Vmf().Changes()` replaced the
  first manager and every listener registered earlier stopped receiving events. Registering one
  listener directly and one through a `VmfProperty` was enough to trigger it. `Vmf()` is now
  memoised per object.
- **`ToString()` was unstable across clones.** The cycle marker for an already-visited node
  embedded the object's identity hash code, so an object and its clone could never produce the
  same string even when structurally identical. A revisited node is now marked with its ordinal
  in traversal order.
- **Content equality did not exist for `EqualsType.Instance` types.** `Content().ContentEquals()`
  fell back to reference equality. A content comparison is now always generated.
- **The model-wide equality default was stomped.** `[VmfModel]` doubles as the per-interface
  marker and as the carrier of the model-wide default, so every bare `[VmfModel]` reset the
  default to `Instance` and a deliberate `Equality = …` declaration was lost (last one wins).
- **Cross-references were compared for `Instance` types.** The property filter keyed on the
  strategy being exactly `ContainmentAndExternal`.
- **Hashing a collection of value types** used `item?.GetHashCode()` on a non-nullable element
  (CS0023) — latent until content hashing began being generated for every type.
- **Cross-reference echoes were recorded twice.** Setting one side also sets the opposite; that
  second update is an echo of the same logical change, but both objects recorded a change of
  their own. Echoes are now marked where they are induced, reported to listeners, and not
  recorded.
- **A list cross-reference echo fired no event at all.** The generated list listener returned
  early on the crossref `EventInfo` to break the cascade, which suppressed the change event
  along with the recursion. It now reports the echo before returning, so both sides fire.

### Added

- **Parameterless `[Container]`.** The same gap `[Contains]` had in 0.1.5; the analyzer and
  templates already handled an absent opposite.

### Behaviour changes

All of these are the *intended* behaviour and match Java VMF, but they differ from 0.2.0:

- `Content().ContentEquals()` on an `EqualsType.Instance` type now compares content instead of
  references.
- An `Instance` type no longer includes cross-references in its content comparison. Only
  `EqualsType.All` considers them.
- A model that sets `Equality = …` on one `[VmfModel]` interface now actually gets that default
  model-wide. Under 0.2.0 the declaration was usually lost, leaving `Instance`.
- Setting one side of a cross-reference now records one change rather than two, and a list
  cross-reference fires an event on both sides.
- A subtype's `[PropertyOrder]` on a re-declared property now takes effect, changing the order
  in which `Reflect().Properties()` visits it.
- The `ToString()` cycle marker changed from `"Type@<identity-hash>"` to `"Type@<ordinal>"`.
- `IVObjectInternal` gained `VmfContentEquals`/`VmfContentHashCode`, so nested comparisons
  cascade content semantics. This affects hand-written implementations of that internal
  contract only.

### Testing

`VMF.NET.IntegrationTests` was renamed to **`VMF.NET.TestSuite`**, mirroring the Java project's
`test-suite` module, and all 39 Java model areas plus 26 Java test classes were ported into it.
The suite runs 252 facts, with 20 skipped; each skip names the capability it waits on. See
[`devdoc/java-parity-roadmap.md`](devdoc/java-parity-roadmap.md).

> **Correction (2026-08-23).** This section originally said "all 30 Java test classes" were
> ported and that the skip count was therefore the measured parity gap. Both were wrong. The
> Java suite has three source roots and the port audit walked one, so 30 further facts —
> `vmf/VMFGenerateRuns` (25) and `events_undo_redo/UndoRedoWithContainmentTest` (5) — were
> never ported and never counted. Nothing about the 0.2.1 code changes; the claim about how
> thoroughly it was verified does. The roadmap carries the reconciled inventory.

### Known gaps

Carried forward and tracked in the roadmap: type-level annotations and static type reflection,
recursive change listeners, undo/redo, covariant property narrowing, inherited and type-level
delegation, and builder-accepting `With*` overloads. Reflective set/unset and inherited default
values should be read as unverified rather than working — see the correction above.
