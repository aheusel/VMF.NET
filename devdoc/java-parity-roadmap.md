# Java test-suite parity — roadmap

**Goal:** the .NET suite covers what the Java project's `test-suite` module covers.
**Status:** port complete (M2″); M4a/M4b/M5/M8 done. **Last updated:** 2026-08-24.

> Companion doc: [`source-generator-dependencies.md`](source-generator-dependencies.md).
> Suite layout and porting conventions: [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

## Design goal: behavioural identity with Java VMF

**Someone moving a model from Java VMF to VMF.NET should meet as few surprises as possible.**
Where the two can behave the same, they must. This is the standard the parity suite measures
against, and it decides how a difference is classified:

- **C# forces it** — covariant property narrowing, compile-time member resolution, `CS0229` on a
  member inherited from two unrelated interfaces. Unavoidable. Document it in a `DEVIATION:` note
  at the top of the file and move on.
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
| `Annotations()` exposes VMF.NET's own `vmf:property:containment-info`, which Java does not | Reconsider. A Java user counting annotations gets a different number |
| Cross-reference lists accept duplicates; Java keeps one reference | Defect (M9) |
| `IsSet` on a **collection** uses `Count > 0`, where Java compares against the default | **Unverified.** Java returns `null` as the default for a collection without a declared one, which would make an empty list report *set* — that reads oddly enough that it needs a probe against a real Java run before being called either way |
| A settable `[Container]` needs `{ get; set; }` in the model; Java always generates the setter | C#-forced: the model interface *is* the public API here, and a partial interface cannot add a setter to a property already declared `{ get; }` |

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

| | Java `test-suite` | VMF.NET |
|---|---|---|
| Model areas | 39 | **39 — all ported** |
| Test classes | 31; 29 portable after 2 deliberate non-ports | **31 in the TestSuite, plus 1 in `VMF.NET.Tests`** |
| Facts | 104; **101 portable** | **97 in the TestSuite** (88 running, 9 skipped) **+ 5 validation facts in `VMF.NET.Tests`** |

Suite totals today: **300 passing**, 9 skipped, 0 failing (220 TestSuite + 80 Tests).

Java's two `complex/vflow` classes are ported as one `VFlowTest`, and `vmf/VMFGenerateRuns`
splits across five: four behavioural classes in the TestSuite, and its model-validation facts as
`VMFGenerateRunsValidationTests` in `VMF.NET.Tests` — a model VMF must *reject* cannot sit in a
compiled project.

Some C# facts have no Java counterpart and are extra coverage rather than parity: four
cross-reference regression facts guarding the recursion fix, the `FSMTest` clone/`ToString`
split, `UnparserModelTest`'s from-the-child-side variant, and five `VList` batch-operation unit
tests.

### The parity gap: 9 facts

Every one is a ported fact carrying `[Fact(Skip = "…")]` with the missing capability named, so
**the skip count is the parity gap again** — the invariant M2″ set out to restore. Each skipped
fact also carries its real body; where it needs an API that does not exist yet, those calls are
commented out behind a `NEEDS` marker rather than the body being left empty.

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
| Builder ergonomics and VList API (M9) | 3 | builders (1), horses (1), unparsermodel (1) |
| Delegation, type-level and inherited (M6) | 2 | parentcontainment01 (2) |
| Collection default values (M9) | 1 | reflectiontest (1) |
| `ToString` format (M9 — align with Java) | 1 | test2 (1) |
| Covariant narrowing (M7) | 1 | propertyinheritance (1) |
| Clone traversal order (needs investigation) | 1 | fsm (1) |

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
| `IChangeInternal.IsContainmentChange` | implemented, still never called | M9 (wire or delete) |
| `IChange.Apply(target)` | implemented, still never called. Undo turned out to be complete, so this is the redo/replay half and has no fact behind it | M9 (wire or delete) |
| `GetContainerPropertyId`, `ITraversalListener.Traverse` | implemented/declared, unreachable | M9 (decide: wire or delete) |

### B. Missing capabilities

| Capability | Blocks | Milestone |
|---|---|---|
| Inherited `[DelegateTo]` — only methods declared on the type itself get a body | ~5 facts, 4 deviated models | M6 |
| Covariant property narrowing — C# interfaces cannot override a property type | 1 fact, 5 deviated models | M7 |
| Collection default values | 1 fact | M9 |
| Cross-reference lists accept duplicates (Java keeps one reference) | 1 fact | M9 |
| Builder-accepting `With*` overloads (Java passes unbuilt nested builders) | 1 fact | M9 |
| `VListChangeEvent.Source`, so a listener can mutate the list it observes | 1 fact | M9 |
| `ToString` renders a different shape from Java's: Java puts the type in an `@type` member and orders properties alphabetically, VMF.NET puts the type outside the braces and orders them as declared | 1 fact | M9 (align with Java) |
| Clone and original are content-equal but traverse differently, so they do not serialise identically | 1 fact | investigate |

### C. Resolved

Undo/redo was listed here as unknown: an API with zero tests. Verified in M8 — **undo works**, on
scalar changes, list adds and list removes, and over a 19,681-node graph undone in reverse. Four
unit tests now pin it. `IChange.Apply` remains uncalled and is the redo/replay half; no ported
fact needs it, so it moves to M9's wire-or-delete list.

## Milestones

| # | Milestone | Content | Done when |
|---|---|---|---|
| ~~M2′~~ | ~~Finish the port~~ | **PARTIAL.** Ported every test class under the `vmftest` root — 26 classes, 62 active and 20 blocked at the time. Claimed to have closed the inventory; it had not, because it never looked at the other two roots. M2″ finishes the job | superseded by M2″ |
| ~~M3~~ | ~~Release 0.2.1~~ | **DONE.** 19 defects fixed since 0.2.0, two crash-class; notes in [`CHANGELOG.md`](../CHANGELOG.md) | published to NuGet |
| ~~M4a~~ | ~~Cross-reference echo classification~~ | **DONE.** The induced side is marked with `IsCrossRefEcho` while it is updated, so its change is tagged `crossref-echo`; `ProcessChange` reports echoes but does not record them | all 3 cross_ref facts active |
| ~~M4b~~ | ~~Change propagation~~ | **DONE.** Changes route up the container chain; recursive flag honoured; read-only observation; `AddRange`/`RemoveAll`; settable `[Container]` | `recursivelistener01`, `observableprop`, `unparsermodel` green |
| ~~M2″~~ | ~~Close the port~~ | **DONE.** Ported `VMFGenerateRuns` (25, split behavioural/validation) and `UndoRedoWithContainmentTest` (5); measured the validation overlap (4 of 9 already covered, 5 added, all pass); gave all 13 empty skipped facts real bodies; restored fidelity in `HorsesTest` and wrote the porting rules down | every Java fact has a counterpart; skip count = parity gap again |
| ~~M5~~ | ~~Reflection metadata~~ | **DONE.** Type-level annotations read on demand; static reflection over a generated prototype plus a per-namespace type registry answering `AllTypes`/`SuperTypes`; per-instance default values; `IsPolymorphic` workaround retired, which fixed a missing `@vmf-type` discriminator | 8 facts un-skipped |
| **M6** | Inherited delegation | Generate bodies for inherited `[DelegateTo]`; type-level delegation | 4 models de-deviated |
| **M7** | Covariant narrowing | Public property at the narrowest type + forwarding explicit implementations per declaring interface | 5 models de-deviated |
| ~~M8~~ | ~~Undo/redo~~ | **DONE.** A change now fires when a child's container changes, reported locally and not recorded, as Java does; undo verified working and given tests; content iteration defaults to `UniqueNode` | `events_undo_redo` and vflow green |
| **M9** | Tail + audit | Collection defaults; decide wire-or-delete on the dead members; negative models as compile-gates; parity audit table | documented parity statement |

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
- **Distinguish unwired from untested.** `AnnotationsByKey`, `ContentHashCode`, `Iterator`,
  `Unset`, `Reset` have no in-repo caller, but they are public API — a coverage gap, not a
  wiring gap.

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
