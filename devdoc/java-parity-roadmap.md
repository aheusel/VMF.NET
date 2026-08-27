# Java test-suite parity — roadmap

**Goal:** the .NET suite covers what the Java project's `test-suite` module covers.
**Status:** every milestone done. **0 skipped facts**, and **101 of Java's 103 facts ported** —
the 2 that are not belong to `ModelDiffTest`, whose feature VMF.NET does not implement. See
*The parity gap* below.
> Work continued past this roadmap. What came after — the build-setup rework that made the
> model build input rather than API — is recorded in
> [`system_constraints.md`](system_constraints.md) as **C-6**, not here.
**Last updated:** 2026-08-25.

> Companion docs: [`source-generator-dependencies.md`](source-generator-dependencies.md),
> and [`differences-to-java.md`](differences-to-java.md) — the reader's catalogue of what
> differs from Java and what to write instead.
> Suite layout and porting conventions: [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

## Design goal: behavioural identity with Java VMF

**Someone moving a model from Java VMF to VMF.NET should meet as few surprises as possible.**
Where the two can behave the same, they must. This is the standard the parity suite measures
against, and it decides how a difference is classified:

- **C# forces it** — compile-time member resolution, `CS0229` on a member inherited from two
  unrelated interfaces, an invariant `VList<T>` where Java has covariant arrays. Unavoidable.
  Document it in a `DEVIATION:` note at the top of the file and move on. Be slow to put anything
  here: "covariant property narrowing" sat in this list until M7, and it was never true — C#
  cannot *override* a property's type, but `new` plus a forwarding explicit implementation gives
  the same observable behaviour.
- **We chose it** — anything else. Treat it as a **defect to fix**, not a preference to defend,
  even when VMF.NET's behaviour is arguably nicer. "Nicer but different" is exactly the surprise
  this goal exists to prevent.
- **Surface conventions** — `Name` rather than `name`, `IParent` rather than `Parent`,
  `[Contains]` rather than `@Contains`. Predictable, mechanical, and expected by anyone writing
  C#. Not a divergence.

A ported fact that fails is therefore evidence of a **behavioural divergence**, not evidence of a
bad port. Reach for "the Java test is wrong here" only after reading Java's implementation.

Verify against Java's *implementation*, not against its tests. The tests state what the authors
chose to pin; the implementation states what actually happens. `IsSet` is the worked example:
the test says an untouched property with a declared default reports `false`, and reading
`_vmf_isSetById` shows why — it is a pure comparison against the current default, with no
"was assigned" flag anywhere. That single line settles a whole family of questions the tests
leave open, such as what happens when you assign the default explicitly (still "not set").

### Known divergences this goal puts in scope

| Divergence | Status |
|---|---|
| `ToString` shape — Java uses an `@type` member and alphabetical properties; VMF.NET puts the type outside the braces and orders as declared | **Align with Java** (M9). Previously filed as "a decision"; under this goal it is a gap |
| ~~`Annotations()` exposes VMF.NET's own `vmf:property:containment-info`, which Java does not~~ | **Not a divergence — the entry was wrong.** Measured 2026-08-25: Java emits it too, on every property, with identical values. See [`VMF.NET.JavaProbe`](../../VMF.NET.JavaProbe/README.md) |
| Cross-reference lists accept duplicates; Java keeps one reference | Defect (M9) |
| `IsSet` on a **collection with no declared default** uses `Count > 0`, where Java compares against the default | **Measured 2026-08-25, and kept.** Java does report an empty such collection as *set* — in fact as a constant `true` nothing can change, whose partner `unset()` throws `NullPointerException`. Collections that *do* declare a default already agree. Knowingly not following C-1, because the Java behaviour is a defect rather than a design |
| ~~A settable `[Container]` needs `{ get; set; }` in the model~~ | **Closed by C-6.** It was filed as C#-forced, and it was not — it followed from the model interface being the public API, which is no longer true. A good example of how much rests on one architectural premise |
| ~~Container properties were told apart by the container's runtime **type**, where Java uses the container property **id**~~ | **Fixed.** Found while porting VFlow's connect logic. Two container properties naming the same containing type were indistinguishable, and the detach path removed an object from every list of a matching type — including the one it was joining | 

## Correction, 2026-08-23

An earlier revision of this document claimed "all 30 Java test classes ported" and "the
inventory is closed". **Both were wrong**, and the headline fact counts derived from them were
too small. What the re-audit found:

- The Java suite has **three** source roots under `src/test/java/eu/mihosoft`, not one. The
  original audit walked only `vmftest`. It missed `vmf/VMFGenerateRuns` — **25 behavioural
  facts** that drive generated code through `VMFTestShell`, covering getter/setter, clone,
  read-only, containment, `ToString`, method delegation, reflective set/unset, inherited default
  values, and the negative models. None of its six model areas (`test1`, `test2`,
  `reflectiontest`, `delegationtest`, `completepropertyordertest`, `nopropertiestest`) has a C#
  test class, although all six have their models ported.
- `events_undo_redo/UndoRedoWithContainmentTest` (**5 facts**) was never ported either.
  `VmfTest/EventsUndoRedo/` holds the model and no test. Section C below did track those facts,
  but they never reached the headline numbers, because those counted only facts that exist in
  the suite.
- The class count itself was wrong: 29 classes carry `@Test`, not 30.

The invariant this document relies on — **skip count = parity gap** — therefore did not hold: a
fact that was never ported is invisible, where a skipped one is not. Closing that is M2″, and it
comes before further feature work.

## Where things stand

*Re-measured 2026-08-25, by path, across all three Java roots.*

| | Java `test-suite` | VMF.NET |
|---|---|---|
| Model areas | 40 | **39 ported**; the gap is `diff` |
| Test classes | 31 with an active `@Test` (+1 disabled upstream) | **31 under `VmfTest/`, plus 1 in `VMF.NET.Tests`** |
| Facts | **103 active**; 101 ported | **96 under `VmfTest/`** + **5** validation facts in `VMF.NET.Tests` |

Suite totals today: **338 passing**, 0 skipped, 0 failing (241 TestSuite + 97 Tests).

Two Java `@Test`s are excluded by upstream, not by us, and both were checked rather than assumed:

- `vmftest/resources/MemoryResourceSetTest` — **entirely commented out** since a 2019 TODO about
  OS portability, so 0 active facts. It exercises `MemoryResourceSet`, VMF's internal
  code-generation I/O; VMF.NET has no counterpart because Roslyn's `AddSource` replaces that layer
  wholesale. This is the class that makes 32 files contain `@Test` while only 31 carry a live one.
- `VMFGenerateRuns.testGetterOnlyInterfaceOnlyAsCommonInterface` — commented out with upstream's
  own note, *"already covered in test src test/vmf/getteronly"*, which we do port.

Counting `@Test` naively gives 106; only 103 are live. The difference is those three commented-out
occurrences, and a count that misses the distinction reports a parity gap that does not exist.

Java's two `complex/vflow` classes are ported as one `VFlowTest`, and `vmf/VMFGenerateRuns`
distributes across seven: four behavioural classes in the TestSuite, and its model-validation
facts across `VMFGenerateRunsValidationTests`, `ModelAnalyzerTests` and `GeneratorCompilesTests`
in `VMF.NET.Tests` — a model VMF must *reject* cannot sit in a compiled project.

Some C# facts have no Java counterpart and are extra coverage rather than parity: four
cross-reference regression facts guarding the recursion fix,
`UnparserModelTest`'s from-the-child-side variant, five `VList` batch-operation unit tests,
eight `ModelAnalyzerTests` facts pinning the delegation-inheritance rules M6 introduced and the
narrowing rules M7 introduced, five `ContainerPropertyIdTests` facts guarding the
container-property-identity fix, and three `ModelDiscoveryTests` facts pinning what makes an
interface a model type (C-6).

### The parity gap: one class, 2 facts

**101 of Java's 103 facts have a running counterpart.** Reconciled by path across all three roots
on 2026-08-25 — the table lives in
[`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

The gap is **`vmftest/diff/ModelDiffTest`**, and it is not a porting gap: Java's `ModelDiff`
(graph diff / apply / merge) has no VMF.NET equivalent, so there is nothing to write a test
against. Recorded under *Not implemented* in
[`differences-to-java.md`](differences-to-java.md).

This is also a correction. The section previously read "the parity gap: none" on the strength of
"every **portable** Java fact has a counterpart" — where *portable* silently excused a class whose
feature we had never built. A gap hidden inside a qualifier is exactly what the skip-count
convention exists to prevent.

That convention still stands for everything else: a fact blocked on a missing capability is kept
as `[Fact(Skip = "…")]` with the capability named **and its real body**, so the skip count is the
parity gap and un-skipping is never a rewrite. `ModelDiffTest` has no skipped placeholder because
its model type does not exist either — worth adding one if `ModelDiff` is ever scheduled.

The `VMFGenerateRuns` overlap is now measured rather than estimated. Of its nine
model-validation facts, **four were already covered** by `ModelAnalyzerTests` and the five that
were not are added; all five pass, so the analyzer already enforced every rule. They were a
coverage gap, not a capability gap.

Two Java classes are deliberately **not** ported: `MemoryResourceSetTest` (a Java I/O
abstraction with no .NET counterpart, commented out upstream) and `VMFGeneratorTest` (covered by
`GeneratorCompilesTests`). `VMFGenerateRuns` is **not** in that category — it was missed.

### Blocked facts by milestone

| Waiting on | Facts | Areas |
|---|---|---|
| ~~Delegation, type-level and inherited (M6)~~ | ~~2~~ 0 | parentcontainment01 — both active |
| ~~Covariant narrowing (M7)~~ | ~~1~~ 0 | propertyinheritance — active |

A blocked fact is kept as `[Fact(Skip = "...")]` with the missing capability named **and its
real body**, so the skip count is the parity gap and un-skipping is not a rewrite. Where the
body needs an API that does not exist, those lines are commented out behind a `NEEDS` marker
rather than the body being left empty.

## Gap inventory

Everything below is evidence-backed: either a fact that fails, or an API the audit shows is
never wired. Fact counts outside the change/reflection rows are estimates.

### A. Unwired plumbing

Declared, sometimes implemented, never called. This is the dominant pattern.

| Member | Consequence | Milestone |
|---|---|---|
| `IChange.Apply(target)` | **Kept.** Java declares `Change.apply(VObject)` as public API and never calls it internally either, so this matches the reference. A coverage gap, not a wiring gap |
| `GetContainerPropertyId` | **Kept and now load-bearing.** Java uses it in nine places, and M8's detach event names its property by this id |
| `ITraversalListener.Traverse` | **Kept.** Java exposes `TraversalListener` as public API. Coverage gap |
| ~~`IChangeInternal.IsCrossRefChange` / `IsContainmentChange`~~ | **Deleted.** Both were unreachable: `IChangeInternal` is `internal`, so they were not public API a consumer could reach, and nothing called them. Java has an `isCrossRefChange` but no containment equivalent. `IsCrossRefEchoChange`, which M4a wired, stays |

### B. Missing capabilities

**All closed.** Every row here blocked at least one fact, and the suite now skips none — so the
skip count corroborates the table rather than relying on it.

| Capability | Blocks | Milestone |
|---|---|---|
| ~~Inherited `[DelegateTo]` — only methods declared on the type itself get a body~~ | **DONE (M6).** 2 facts un-skipped, 5 models de-deviated | M6 |
| ~~Covariant property narrowing~~ | **DONE (M7).** The last skipped fact un-skipped, 5 models de-deviated | M7 |
| ~~Collection default values~~ | **DONE (M9).** | M9 |
| ~~Cross-reference lists accept duplicates (Java keeps one reference)~~ | **DONE (M9).** | M9 |
| ~~Builder-accepting `With*` overloads (Java passes unbuilt nested builders)~~ | **DONE (M9).** | M9 |
| ~~`VListChangeEvent.Source`, so a listener can mutate the list it observes~~ | **DONE (M9).** | M9 |
| ~~`ToString` renders a different shape from Java's~~ | **DONE (M9).** Aligned with Java: `@type` member, alphabetical properties | M9 |
| ~~Clone and original are content-equal but traverse differently~~ | **DONE (M9).** Never a traversal problem: `Clone`/`DeepCopy` used an `Equals`-keyed identity map, so content-equal siblings collapsed into one object | M9 |

### C. Resolved

Undo/redo was listed here as unknown: an API with zero tests. Verified in M8 — **undo works**, on
scalar changes, list adds and list removes, and over a 19,681-node graph undone in reverse. Four
unit tests now pin it. `IChange.Apply` remains uncalled and is the redo/replay half; no ported
fact needs it, so it moves to M9's wire-or-delete list.

## Parity statement

What the suite proves, as of M7.

**Every portable Java fact has a running counterpart.** 101 portable facts, 0 skipped. Every
milestone on this roadmap is done.

**Deliberate, permanent differences.** These are forced by C# and will not close:

| Difference | Why |
|---|---|
| `GetModelType()` rather than Java's `type()` | a model may declare a property named `Type`, and a method cannot share a name with a property |
| A member inherited from two unrelated interfaces must be re-declared | `CS0229` |
| `Name` rather than `name`, `IParent` rather than `Parent`, `[Contains]` rather than `@Contains` | surface convention, not behaviour |
| Read-only write attempts fail at compile time, not at runtime | C# resolves members statically |
| A narrowed property needs `new` on the model interface, and a **collection** cannot be narrowed at all | C# has no covariant override for an interface property, and `VList<T>` is invariant where Java's properties are covariant arrays |

**Known differences that are not forced.** Both were open questions until 2026-08-25, when they
were settled against a real Java run — see [`VMF.NET.JavaProbe`](../../VMF.NET.JavaProbe/README.md):

- `IsSet` on a collection with **no declared default**: ours reports an empty list as unset, Java
  reports it as set. **Confirmed, and deliberately kept.** Java's value is a constant `true` that
  no operation can change, and its `unset()` throws `NullPointerException`. Collections that do
  declare a default already agree, so the divergence is one cell wide.
- ~~`Annotations()` includes VMF.NET's own bookkeeping entries, which Java does not~~ —
  **the entry was wrong; there is no difference.** Java emits `vmf:property:containment-info` on
  every property and `vmf:type:immutable` on immutable types, with values identical to ours.
  `vmf:type:interface-only` is unreachable in both, since neither generates an implementation for
  an interface-only type.

**Coverage gaps, not wiring gaps.** `IChange.Apply` and `ITraversalListener.Traverse` are public
API that Java also exposes and also never calls internally. They have no ported fact behind them.

## Milestones

| # | Milestone | Content | Done when |
|---|---|---|---|
| ~~M2′~~ | ~~Finish the port~~ | **PARTIAL.** Ported every test class under the `vmftest` root — 26 classes, 62 active and 20 blocked at the time. Claimed to have closed the inventory; it had not, because it never looked at the other two roots. M2″ finishes the job | superseded by M2″ |
| ~~M3~~ | ~~Release 0.2.1~~ | **DONE.** 19 defects fixed since 0.2.0, two crash-class; notes in [`CHANGELOG.md`](../CHANGELOG.md) | published to NuGet |
| ~~M4a~~ | ~~Cross-reference echo classification~~ | **DONE.** The induced side is marked with `IsCrossRefEcho` while it is updated, so its change is tagged `crossref-echo`; `ProcessChange` reports echoes but does not record them | all 3 cross_ref facts active |
| ~~M4b~~ | ~~Change propagation~~ | **DONE.** Changes route up the container chain; recursive flag honoured; read-only observation; `AddRange`/`RemoveAll`; settable `[Container]` | `recursivelistener01`, `observableprop`, `unparsermodel` green |
| ~~M2″~~ | ~~Close the port~~ | **DONE.** Ported `VMFGenerateRuns` (25, split behavioural/validation) and `UndoRedoWithContainmentTest` (5); measured the validation overlap (4 of 9 already covered, 5 added, all pass); gave all 13 empty skipped facts real bodies; restored fidelity in `HorsesTest` and wrote the porting rules down | every Java fact has a counterpart; skip count = parity gap again |
| ~~M5~~ | ~~Reflection metadata~~ | **DONE.** Type-level annotations read on demand; static reflection over a generated prototype plus a per-namespace type registry answering `AllTypes`/`SuperTypes`; per-instance default values; `IsPolymorphic` workaround retired, which fixed a missing `@vmf-type` discriminator | 8 facts un-skipped |
| ~~M6~~ | ~~Inherited delegation~~ | **DONE.** Type-level `[DelegateTo]` is a constructor delegation calling `On<Type>Instantiated()` and supplies the class for undecorated methods; delegations inherit, deduped by signature with own-first; the cast reads `T` off the delegate class; one delegate instance per class per object | 2 facts un-skipped, 5 models de-deviated |
| ~~M7~~ | ~~Covariant narrowing~~ | **DONE.** Public member at the narrowed type plus one forwarding explicit implementation per declaring interface, typed from the interface that declares it; a narrowed collection is rejected, since `VList<T>` is invariant | last fact un-skipped, 5 models de-deviated |
| ~~M8~~ | ~~Undo/redo~~ | **DONE.** A change now fires when a child's container changes, reported locally and not recorded, as Java does; undo verified working and given tests; content iteration defaults to `UniqueNode` | `events_undo_redo` and vflow green |
| ~~M9~~ | ~~Tail + audit~~ | **DONE.** `ToString` aligned with Java; clone identity fixed; cross-reference duplicates rejected; `VListChangeEvent.Source`; builder-accepting `With*`; collection defaults; wire-or-delete settled; parity statement written | documented parity statement |

### Sequencing rationale

- **Port before feature work.** Every area ported so far has changed the plan, and the M2″
  correction is the sharpest example: 30 facts were invisible because the audit that produced
  this document walked one of three Java source roots. Porting is cheap relative to being wrong
  about what is left.
- **No release until the ported suite runs green with zero skips.** This replaces the earlier
  "release early" rationale, which argued that the crash-class bugs in shipped 0.2.0 — the
  cross-reference stack overflow and the silent listener orphaning — should not wait for the
  whole roadmap. That reasoning shipped 0.2.1, but it was resting on a fact count that silently
  excluded 30 unported facts, so "the suite passes" meant less than it appeared to. A release
  states that the implementation matches the reference; it cannot state that while facts are
  skipped or missing. 0.2.1 is published and NuGet does not allow withdrawal, only unlisting.
  Everything since M3 accumulates under `Unreleased` in [`CHANGELOG.md`](../CHANGELOG.md).
- **Group by subsystem, not by symptom.** The gaps are not independent features; they are two or
  three unfinished subsystems. One design decision per subsystem resolves several symptoms.
- **M4 before M5** (done). It blocked the most facts and held the remaining correctness risk.
  The reflection gaps are missing *data*, which is inert until something reads it.
- **M2″ before M5.** `VMFGenerateRuns` covers reflective set/unset and inherited default values,
  which is M5 and M9 territory. Scoping those milestones before seeing what those facts actually
  assert would repeat the mistake this correction documents.

## M9 plan — the tail

Five blocked facts, four wire-or-delete decisions, and the parity statement.

### 1. `ToString` format (1 fact, possibly 2)

Java, from `impl/to-string.vm`:

```
{"@type":"Parent", "children": [{"@type":"Child", "name": "Luke"}], "elements": [], "name": "Father"}
```

- the type is a **member**, `{"@type":"Name"`, not a prefix outside the braces
- every scalar is **quoted**, including numbers and nested model objects (which Java wraps in
  quotes even though that is not valid JSON — reproduce it, it is the reference behaviour)
- collections are bracketed and unquoted
- container properties and `[IgnoreToString]` properties are skipped
- the cycle marker is the constant **`{skipping recursion}`**

That last point may also close the FSM clone fact, which is the one item in the inventory with
no explanation. Our marker embeds the node's ordinal in traversal order, so if a clone traverses
differently the strings differ — which is exactly the symptom. Java's constant marker cannot show
a traversal-order difference at all. **Check this rather than assume it**: if FSM goes green, the
underlying traversal difference is still there and should be recorded as a known difference
rather than silently closed.

Property **order** is the open question. Java's expected string reads `children, elements, name`
for a `Parent extends Named`, which is neither our inherited-first order nor obviously anything
else. It cannot be read off the Java repo — that area's code is generated at test time — so
implement the format, run the fact, and decide from the diff.

### 2. Cross-reference lists reject duplicates (1 fact)

Adding the same element three times must leave one. The generated code already guards the
opposite side; the list being added to does not.

### 3. `VListChangeEvent.Source` (1 fact)

Java's `evt.source()` gives the list back so a listener can mutate it while observing.

### 4. Builder-accepting `With*` overloads (1 fact)

Java passes UNBUILT nested builders and builds them lazily on `build()`.

### 5. Collection default values (1 fact)

Java expresses these as a Java expression evaluated at construction.

### 6. Wire or delete

`IChangeInternal.IsContainmentChange`, `GetContainerPropertyId`, `ITraversalListener.Traverse`,
`IChange.Apply`. Each is implemented and unreachable. Decide per member against Java: wire it if
Java's behaviour depends on it, delete it if it is ours alone. No fact rides on any of them.

### 7. Parity statement

A table of what the suite proves, what is deliberately different and why, and what remains.

## M5 design note — reflection metadata

Read from Java's implementation (`runtime/.../core/Type.java`, `internal/ReflectImpl.java`) rather
than from its tests, per the design goal. Four parts, in ascending order of cost.

### 1. Type-level annotations — one line

`ReflectImpl.Annotations()` returns an empty list and waits for `SetAnnotations`, which nothing
calls. Java's does not have a setter at all: `annotations()` reads
`parent._vmf_getAnnotations()` on demand. `IVObjectInternal.GetAnnotations()` already exists here
and already returns the generated `_VMF_OBJECT_ANNOTATIONS`, so the fix is to read it and delete
`SetAnnotations`. Unblocks the 3 `annotations` facts.

### 2. Static type reflection — a prototype instance

Java generates `static Type type()` on each model interface, and `Type` holds the model's
`Class`. `Type.reflect()` lazily builds a **prototype instance** — `modelClass.getMethod(
"newInstance").invoke(null)` — and returns `prototype.vmf().reflect()` with `staticOnly = true`.
`superTypes()` uses the same prototype, reading `_vmf_getSuperTypeNames()` and wrapping each name
in a `Type`.

So static reflection is not a separate metadata path: it is ordinary instance reflection over a
throwaway instance, with the instance-dependent operations disabled. `staticOnly` and
`EnsureInstanceAccess` are already built here for exactly this and are currently unreachable —
this is what makes them reachable.

VMF.NET should do the same, with one deliberate difference: **pass a factory delegate rather than
reflect over a name.** Java needs `Class.forName` because its `Type` only carries a name; our
generator can emit `() => IFoo.NewInstance()` directly at the point where the `VmfType` is
created. Same behaviour, no runtime reflection, no failure mode when a name cannot be resolved.

Interface-only types have no `NewInstance`, so they get no factory and `Reflect()` on them throws
— matching Java, where `getPrototype` fails for the same reason (less tidily: it prints a stack
trace and then dereferences null).

Unblocks `staticreflection` and the `observableprop` static fact.

### 3. Per-instance default values

Bigger than "add `VmfProperty.SetDefault`". Java keeps a per-instance `_VMF_DEFAULT_VALUES` array
initialised to null; `getDefaultValueById` returns the entry if set and the compile-time default
otherwise; and `setDefault` writes the entry and then, **if the property was unset, calls
`unsetById`** so an unset property follows its default when the default moves. That is why the
Java fact expects `setDefault("abc")` to leave `isSet()` false *and* change the value to `"abc"`.

Needs: the per-instance array in the generated impl, `GetDefaultValueById` consulting it,
`SetDefaultValueById` implementing the write-then-maybe-unset dance, and `VmfProperty.SetDefault`
exposing it. Containment properties must refuse it, as Java's does explicitly.

Unblocks the 3 `reflectiontest` facts.

### 4. Retire the `IsPolymorphic` workaround

`VmfJsonConverter` decides whether to write `@vmf-type` via `VmfTypeUtils.IsPolymorphic`, which
walks `AllTypes()` and `SuperTypes()` — both currently degenerate (`AllTypes()` returns just the
one type, `SuperTypes()` is always empty). Once 2 populates them, revisit it. Not fact-blocking;
do it last and only if it genuinely simplifies.

## M7 design note — covariant property narrowing

Java narrows a property down an inheritance chain: `WithLocation.getLocation()` returns
`Location`, `WithLocationX` overrides it to return `LocationX`, `GCode1` to return `LocationXY`.
The reflected property type is the narrowed one, which is what `propertyInheritanceTest01`
asserts.

**Java's rule is "most derived", not "narrowest".** `Implementation.collectAndSetProperties` puts
the type's own properties first and adds inherited ones only when the name is not already present
— it never compares types. It does not have to: the Java compiler has already rejected a
covariant override that widens, so most-derived *is* narrowest. VMF.NET's `CollectAllProperties`
resolves the same way, and a build with the narrowing restored confirmed it — the public property
came out at `ILocationXY` on its own. **The analysis side needed no change at all**; the gap was
entirely in the generated code.

**What C# forbids is narrower than the deviation note claimed.** There is no covariant *override*
of an interface property. But there is `new`, which declares a second member hiding the first so
that both exist, and there are explicit interface implementations, which satisfy each declaring
interface at its own type. One backing property at the narrowed type plus one forwarding explicit
implementation per interface that declares it wider reproduces Java's observable behaviour:
`gCode1.Location` is an `ILocationXY`, `((IWithLocation)gCode1).Location` is an `ILocation`, and
they are the same object.

The generator already emitted that shape on the read-only side — `readonly_prop_impls` exists
because a re-declaration leaves the base member unimplemented — but typed every explicit
implementation at the *winning* property's type, which is only correct while nothing narrows. So:

1. Each explicit implementation is typed from the interface that **declares** it.
2. The mutable side gets the same loop, which it never needed before because the public property
   always matched every declaration.

**The model still writes `new`.** The model interface is the public API here, so the narrowing is
the model author's declaration, exactly as the covariant `getLocation()` override is in Java. What
changes is that the generator now produces an implementation that compiles.

**Where a narrowed setter lands.** Every base in this model declares `location` `@GetterOnly`, so
every forwarding implementation is get-only. When a base does declare a setter at a wider type,
the forwarding setter has to narrow the incoming value and throws `InvalidCastException` if it
does not fit. Java stores it and throws `ClassCastException` at the next narrowed read instead —
both reject it, ours at the assignment rather than a step later.

**Collections cannot narrow.** Java's properties are arrays and arrays are covariant, so
`Statement[]` narrowing to `SubStatement[]` works there. `VList<T>` is invariant, so no forwarding
implementation can exist, and the generator reports it rather than emitting code that will not
compile. No ported model does this.

## M6 design note — delegation

Read from Java's model layer (`core/.../ModelType.initDelegations`, `Implementation
.initPropertiesImportsAndDelegates`, `DelegationInfo`) rather than from its tests, per the design
goal. Four behaviours, three of them Java's and one that only exists because C# needs a cast.

### 1. A type-level `[DelegateTo]` is a *constructor* delegation

`DelegationInfo.newInstance(model, clazz)` turns `@DelegateTo` on the interface into a delegation
whose method name is `"on" + SimpleName + "Instantiated"`. The generated constructor creates the
delegate, calls `setCaller(this)`, and then calls that hook. It is the model's only chance to run
code at instantiation, and `parentcontainment01` is built entirely on it: the hook registers a
change listener, and *that listener* is what populates `parent`. Nothing else in that model
mentions containment at all.

VMF.NET names the hook `On<SimpleName>Instantiated`, where `SimpleName` is the model interface
with its leading `I` stripped — the same rule `ImplClassName` already uses, and the same string
Java produces (`ICodeEntity` → `OnCodeEntityInstantiated`).

### 2. A type-level `[DelegateTo]` also supplies the class for undecorated methods

`DelegationInfo.newInstance(model, m, cD)` falls back to `cD.fullTypeName` whenever the method
itself carries no annotation. `delegationinherit`'s `CircuitDevice.process()` and `consume()`
depend on exactly this — the Java source even comments the two lines "uses constructor delegation
info". A method that has neither its own attribute nor a type-level one is left alone; Java raises
an error there, but in C# an interface method may carry a default implementation, so silence is
the correct .NET reading.

### 3. Delegations are inherited

`ModelType` collects only *declared* members — which is why reading it alone suggested there was
nothing to inherit. `Implementation` is where inheritance happens: it appends every supertype's
delegations after the type's own, then keeps one entry per **signature**
(`name(t1;t2)`, or `constructor-()` for the hook). Own-first plus first-wins means a redeclaration
in the concrete type overrides the inherited one, and — because every constructor delegation
shares the signature `constructor-()` — **exactly one survives per implementation**, the nearest
in the hierarchy.

### 4. The `IDelegatedBehavior<T>` cast reads `T` off the delegate class

Ours alone. Java needs no cast: the field is declared at the delegate's own type, and
`setCaller`'s parameter type comes with it — so the delegate picks `T`, and any caller that
satisfies it works. VMF.NET has to cast, and it was casting to the type being *generated*, which
inverts the relationship: an inherited delegate then had to implement `IDelegatedBehavior<T>` once
per concrete subtype. That is why four ported models carried a delegate with two or three
redundant interface implementations.

The declaring type looks like the obvious replacement and is still wrong. Java's own delegates
prove it: `CircuitDeviceDelegate` is a `DelegatedBehavior<Device>` while the `@DelegateTo` sits on
`CircuitDevice`, and `ControlFlowChildNodeDelegate` is a `DelegatedBehavior<VObject>` — the root
interface. So the generator reads `T` from the delegate class's own `IDelegatedBehavior<T>`
(`SymbolExtractor.ResolveCallerType`) and casts to that. Statically resolved, no runtime
reflection, and the same delegate sources compile.

A cast rather than an unqualified `field.SetCaller(this)` because `SetCaller` has a default
interface implementation: a delegate that does not override it — as
`CircuitDeviceDelegate` does not — is only reachable through the interface in C#.

One consequence worth stating: **one field per delegate class, not per method.** Java's `varName`
indexes the delegate *type*, so the constructor hook and every delegated method on an object share
a single delegate instance, and `setCaller` runs once at creation. A delegate that keeps state
between calls — which `parentcontainment01`'s does, holding its caller for the listener, and which
`delegationtest`'s `constructorCalled()` reads back — depends on that.

## M4b design note — notify *up* the container chain

Measured starting point (probe, 2026-08-23), which corrects the skip note on
`RecursiveVsNonRecursiveListenerTest`: a change on a descendant currently reaches **neither**
listener kind — 0 recursive, 0 non-recursive. Containment itself is correct
(`GetContainer() == root`), but `__vmf_changes` is null on every descendant, and each generated
setter fires through `__vmf_changes?.Fire…`, so a descendant fires nothing at all. The recursive
flag being ignored in `ProcessChange` is the *second* half of the gap and only becomes
observable once changes reach the root's manager.

**The obvious fix is push-down, and it is the wrong one.** `SetModelToChanges` exists and is
never called, which suggests the intended design: when `Changes()` is called on a root, push
that manager into every contained descendant, and maintain it as the graph mutates. Rejected:

- Each object has exactly **one** `__vmf_changes` field. Pushing a root's manager into a child
  overwrites whatever manager the child already had, silently orphaning every listener
  registered directly on that child. That is the 0.2.1 `Vmf()` orphaning bug reintroduced one
  level down. Java does not have this problem because it keeps a *list* of
  `PropertyChangeListener`s per object; matching that would mean turning `__vmf_changes` into a
  collection and rewriting all six fire sites.
- It needs attach/detach bookkeeping at four sites (list add, list remove, scalar set old,
  scalar set new) plus a subtree walk on every containment change — bookkeeping that can drift
  out of sync with the actual graph.

**Instead, walk up at fire time.** A change notifies its own object's manager and every manager
found by following `GetContainer()` to the root, deduplicated. This is strictly less machinery
and is correct by construction:

- No attach/detach bookkeeping exists to drift, because **reachability *is* the container
  chain**. Detaching a node stops its events with no extra code — which is exactly what
  `registerUnregisterSimpleProperties` asserts.
- A child keeps its own manager *and* reaches its root's. No orphaning is possible.
- Recursive vs non-recursive falls out for free: the owner's own manager sees a change whose
  `Object` **is** the owner, so both listener kinds fire; a manager found further up sees a
  change whose `Object` is *not* its owner, so only recursive listeners fire.

The cost is O(containment depth) per change instead of O(1). Depth is small in practice, and
the alternative pays a subtree walk per containment change instead.

Consequence for the audit: `SetModelToChanges` is not merely unwired, it is **obsolete** under
this design — the walk replaces it. It is removed rather than left as dead plumbing.

### Work items

| # | Item | Un-skips |
|---|---|---|
| 1 | Notify up the container chain; honour the recursive flag in `ProcessChange` | `recursivelistener01` (1) |
| 2 | Read-only observation: `ReadOnlyVmfImpl.Changes()` delegates to the mutable object's manager, and read-only reflection stops marking itself `staticOnly` | `observableprop` (2) |
| 3 | `VList.AddRange` / `RemoveAll(params int[])` raising **one** event carrying several elements | `observableprop` (1) |
| 4 | Settable `[Container]` property | `recursivelistener01` (1), `unparsermodel` (1) |

Item 2 note: read-only reflection currently calls `ReflectImpl.SetStaticOnly(true)`, which makes
every reflective operation throw "Cannot access property without an instance". That conflates
*read-only* with *instance-less*: a read-only wrapper does have an instance. Writes stay refused
by the existing mechanism — the read-only impl is `IVObjectInternal`, not
`IVObjectInternalModifiable`, so `VmfProperty.Set` throws "Cannot modify unmodifiable object".
The `staticOnly` flag stays, reserved for its real purpose in M5 (`Type.type().reflect()`), where
there genuinely is no instance. No test depends on read-only reflection throwing.

## The audit that produced this

Most of section A came from asking a single question mechanically: **which members are never
accessed anywhere?** — searching source *and* the Scriban templates, since generated code is
authored there.

```
for every member declared in VMF.NET.Runtime (excluding attribute members,
which Roslyn reads reflectively, and constructors):
    count occurrences of `.<Member>` across **/*.cs and **/*.sbn, excluding obj/ and bin/
    zero  =>  nothing in the repo or in generated code ever calls it
```

Worth re-running after each milestone. Two cautions learned the hard way:

- **Properties and bare calls produce false positives.** `IsSet` and `NextListElement` looked
  unimplemented but are not; `Caller` is a `protected` hook for user subclasses. Verify each hit
  by reading it.
- **Distinguish unwired from untested.** `AnnotationsByKey`, `ContentHashCode`,
  `Unset`, `Reset` have no in-repo caller, but they are public API — a coverage gap, not a
  wiring gap. (`Cursor`, listed here as `Iterator`, is now covered by `ContentTraversalTests`.)

The audit is also a corrective. The cross-reference recording gap was first diagnosed, from a
failing test alone, as needing new design; the audit showed the classification helpers already
existed and were simply never called. Read the code before concluding something is missing.

### The inventory audit, and how it failed

Section A audits *this* repo. The parity inventory audits the **Java** side, and that is the one
that went wrong: it enumerated Java test classes under
`test-suite/src/test/java/eu/mihosoft/vmftest` and treated the result as complete. There are
three roots — `vmftest`, `vmftests` (models only) and `vmf` — and the third holds 26 facts.

Enumerate from the file system, not from an assumed layout, and reconcile the totals:

```
# every Java class carrying facts, all roots, keyed by PATH not class name
#   (`ContainmentTest` and `ToStringTest` each exist in two packages -- keying by
#    class name silently collapses them and undercounts by 12 facts)
find test-suite/src/test/java -name '*.java' | xargs grep -c '@Test'

# reconcile against the port, per area:
#   Java facts = C# facts - (C# facts with no Java counterpart) + (unported Java facts)
```

The reconciliation is the part that catches this. A per-area table that has to balance makes a
missing class impossible to overlook; a bare count of what was ported cannot, because the thing
it is missing is precisely what it does not enumerate.
