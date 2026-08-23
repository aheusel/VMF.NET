# Changelog

Notable changes per release. Earlier releases are listed on the
[GitHub releases page](https://github.com/aheusel/VMF.NET/releases).

## Unreleased

### Added

- **Recursive change listeners now work.** A listener registered on a root sees changes anywhere
  in the subtree that root contains. Previously a change on a contained object reached no
  listener at all.
- **Read-only views can observe changes.** `readOnly.Vmf().Changes()` returns the wrapped
  object's manager instead of throwing, and reflective access through a read-only view works —
  including `AddChangeListener` on a property obtained from it. Writes are still refused.
- **`VList.AddRange(IEnumerable<T>)` and `VList.RemoveAll(params int[])`**, each raising a single
  change carrying every affected element rather than one change per element.
- **Settable `[Container]` properties.** A model can declare a container property
  `{ get; set; }`, letting containment be driven from the child (`child.Parent = p`, or `null`
  to detach). A `[Container]` with no declared opposite gets no setter.

### Behaviour changes

- `IChanges.AddListener(listener, recursive: false)` now means what it says. Previously the flag
  was recorded and ignored, so every listener behaved recursively — which was invisible because
  no subtree change ever arrived. Code that passed `false` and relied on seeing everything will
  now see less; the default (`AddListener(listener)`) is still recursive.
- A listener on a root now receives changes from contained objects. Anything counting changes on
  a root of a non-trivial object graph will see more of them.
- `ReadOnly*Impl.Vmf().Changes()` no longer throws `InvalidOperationException`.
- Reflection through a read-only view no longer throws "Cannot access property without an
  instance"; reads and listeners work, and writes throw "Cannot modify unmodifiable object".
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
