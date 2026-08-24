# Java test-suite parity — roadmap

**Goal:** the .NET suite covers what the Java project's `test-suite` module covers.
**Status:** **the port is complete** — every Java fact has a counterpart (M2″). M4a/M4b done.
**Last updated:** 2026-08-24.

> Companion doc: [`source-generator-dependencies.md`](source-generator-dependencies.md).
> Suite layout and porting conventions: [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

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
| Facts | 104; **101 portable** | **97 in the TestSuite** (75 running, 22 skipped) **+ 5 validation facts in `VMF.NET.Tests`** |

Suite totals today: **280 passing**, 22 skipped, 0 failing (200 TestSuite + 80 Tests).

Java's two `complex/vflow` classes are ported as one `VFlowTest`, and `vmf/VMFGenerateRuns`
splits across five: four behavioural classes in the TestSuite, and its model-validation facts as
`VMFGenerateRunsValidationTests` in `VMF.NET.Tests` — a model VMF must *reject* cannot sit in a
compiled project.

Some C# facts have no Java counterpart and are extra coverage rather than parity: four
cross-reference regression facts guarding the recursion fix, the `FSMTest` clone/`ToString`
split, `UnparserModelTest`'s from-the-child-side variant, and five `VList` batch-operation unit
tests.

### The parity gap: 22 facts

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
| Reflection metadata + static entry (M5) | 5 | annotations (3), staticreflection (1), observableprop (1) |
| Per-instance default values — `VmfProperty.SetDefault` (M5) | 3 | reflectiontest (3) |
| Container-change event (M8) | 4 | eventsundoredo (3), vflow (1) |
| Delegation, type-level and inherited (M6) | 2 | parentcontainment01 (2) |
| Builder ergonomics and VList API (M9) | 3 | builders (1), horses (1), unparsermodel (1) |
| Collection default values (M9) | 1 | reflectiontest (1) |
| Undo (M8) | 1 | vflow (1) |
| `ToString` format (M9 — a decision, not a bug) | 1 | test2 (1) |
| Covariant narrowing + static reflection (M7) | 1 | propertyinheritance (1) |
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
| `ReflectImpl.SetAnnotations` | `Reflect().Annotations()` always empty — the data (`_VMF_OBJECT_ANNOTATIONS`) is generated but never read | M5 |
| `VmfType.SetSuperTypes` | `SuperTypes()` always empty — `_VMF_SUPER_TYPE_NAMES` likewise generated and never read | M5 |
| `IChange.Apply(target)` | implemented, never called — suggests undo/redo is partly built rather than absent | M8 |
| `SetDefaultValueById` | generated with an empty body and never called; `VmfProperty` exposes no `SetDefault` at all. **Decided: wire it** — three ported facts need per-instance defaults | M5 |
| `GetContainerPropertyId`, `UnsetById`, `ITraversalListener.Traverse` | implemented/declared, unreachable | M9 (decide: wire or delete) |

### B. Missing capabilities

| Capability | Blocks | Milestone |
|---|---|---|
| Static type reflection (Java `Type.type().reflect()`) | 1 fact | M5 |
| Inherited `[DelegateTo]` — only methods declared on the type itself get a body | ~5 facts, 4 deviated models | M6 |
| Covariant property narrowing — C# interfaces cannot override a property type | ~3–6 facts, 5 deviated models | M7 |
| Collection default values | 1 fact | M9 |
| Cross-reference lists accept duplicates (Java keeps one reference) | 1 fact | M9 |
| Builder-accepting `With*` overloads (Java passes unbuilt nested builders) | 1 fact | M9 |
| `VListChangeEvent.Source`, so a listener can mutate the list it observes | 1 fact | M9 |
| A change event when a child's container changes (`SetContainer` fires none) | 1 fact | M8 |
| `ToString` renders a different shape from Java's: Java puts the type in an `@type` member and orders properties alphabetically, VMF.NET puts the type outside the braces and orders them as declared | 1 fact | M9 (decide) |
| Clone and original are content-equal but traverse differently, so they do not serialise identically | 1 fact | investigate |

### C. Unknown

Undo/redo has an API (`IChange.Undo`, `ITransaction.Undo`, `IsUndoable`) and **zero** tests.
`TransactionImpl.Undo()` does call `_changes[i].Undo()`, so some of it is built. Verify before
scoping.

`events_undo_redo` is now ported, and it turns out **none of its five facts calls `undo()`** —
they assert change *recording* across a containment boundary, which M4b's routing already
satisfies. Two pass; the other three are blocked on the container-change event, not on undo.
The only fact that genuinely needs undo is `vflow`'s `CreateAndUndoTest`. Undo/redo is therefore
a much smaller and much less certain part of M8 than this section assumed.

## Milestones

| # | Milestone | Content | Done when |
|---|---|---|---|
| ~~M2′~~ | ~~Finish the port~~ | **PARTIAL.** Ported every test class under the `vmftest` root — 26 classes, 62 active and 20 blocked at the time. Claimed to have closed the inventory; it had not, because it never looked at the other two roots. M2″ finishes the job | superseded by M2″ |
| ~~M3~~ | ~~Release 0.2.1~~ | **DONE.** 19 defects fixed since 0.2.0, two crash-class; notes in [`CHANGELOG.md`](../CHANGELOG.md) | published to NuGet |
| ~~M4a~~ | ~~Cross-reference echo classification~~ | **DONE.** The induced side is marked with `IsCrossRefEcho` while it is updated, so its change is tagged `crossref-echo`; `ProcessChange` reports echoes but does not record them | all 3 cross_ref facts active |
| ~~M4b~~ | ~~Change propagation~~ | **DONE.** Changes route up the container chain; recursive flag honoured; read-only observation; `AddRange`/`RemoveAll`; settable `[Container]` | `recursivelistener01`, `observableprop`, `unparsermodel` green |
| ~~M2″~~ | ~~Close the port~~ | **DONE.** Ported `VMFGenerateRuns` (25, split behavioural/validation) and `UndoRedoWithContainmentTest` (5); measured the validation overlap (4 of 9 already covered, 5 added, all pass); gave all 13 empty skipped facts real bodies; restored fidelity in `HorsesTest` and wrote the porting rules down | every Java fact has a counterpart; skip count = parity gap again |
| **M5** | Reflection metadata | Populate `AllTypes`/`SuperTypes`/`Annotations`; add a static entry point. Then retire the `IsPolymorphic` call-site workaround from 0.1.4 | `annotations`, `staticreflection` green |
| **M6** | Inherited delegation | Generate bodies for inherited `[DelegateTo]`; type-level delegation | 4 models de-deviated |
| **M7** | Covariant narrowing | Public property at the narrowest type + forwarding explicit implementations per declaring interface | 5 models de-deviated |
| **M8** | Undo/redo | Verify what exists, then finish. `IChange.Apply` is implemented and never called and `TransactionImpl.Undo` partly works, so scope this against the code, not against the API surface | `events_undo_redo` (5) and vflow (2) green |
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
