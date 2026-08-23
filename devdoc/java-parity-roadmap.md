# Java test-suite parity — roadmap

**Goal:** the .NET suite covers what the Java project's `test-suite` module covers.
**Status:** models ported; **all Java test classes ported (M2' complete)**. **Last updated:** 2026-08-23.

> Companion doc: [`source-generator-dependencies.md`](source-generator-dependencies.md).
> Suite layout and porting conventions: [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

## Where things stand

| | Java `test-suite` | `VMF.NET.TestSuite` |
|---|---|---|
| Model areas | 39 | **39 — all ported** |
| Test classes | 30 (excl. 2 deliberate non-ports) | **30 — all ported** |
| Facts | ~80 | **62 active, 20 blocked** |

Suite totals today: **252 passing**, 20 skipped, 0 failing (182 + 70).

The inventory is now closed: every Java fact has been examined. The 20 blocked facts are the
parity gap, and each names the capability it waits on.

Two Java classes are deliberately **not** ported: `MemoryResourceSetTest` (tests a Java I/O
abstraction with no .NET counterpart, and is commented out upstream) and `VMFGeneratorTest`
(already covered by `GeneratorCompilesTests` in `VMF.NET.Tests`).

### Blocked facts by milestone

| Waiting on | Facts | Areas |
|---|---|---|
| Change propagation / observation (M4b) | 6 | recursivelistener01 (2), observableprop (3), unparsermodel (1) |
| Reflection metadata + static entry (M5) | 5 | annotations (3), staticreflection (1), propertyinheritance (1) |
| Delegation, type-level and inherited (M6) | 2 | parentcontainment01 (2) |
| Undo / container-change events (M8) | 2 | vflow (2) |
| Builder ergonomics and VList API (M9) | 3 | builders (1), horses (1), unparsermodel (1) |
| Clone traversal order (needs investigation) | 1 | fsm (1) |
| Covariant narrowing (M7) | 1 | propertyinheritance, counted above |

A blocked fact is kept as `[Fact(Skip = "...")]` with the missing capability named, so the
**skip count is the parity gap** rather than the fact quietly disappearing.

## Gap inventory

Everything below is evidence-backed: either a fact that fails, or an API the audit shows is
never wired. Fact counts outside the change/reflection rows are estimates.

### A. Unwired plumbing

Declared, sometimes implemented, never called. This is the dominant pattern.

| Member | Consequence | Milestone |
|---|---|---|
| `IChangeInternal.IsContainmentChange` | implemented, still never called | M9 (wire or delete) |
| `ChangesManager._listenerEntries` recursive flag | recorded, then ignored by `ProcessChange`; recursive and non-recursive listeners behave identically | M4a |
| `IVObjectInternalModifiable.SetModelToChanges` | never called, so a manager attached to a root never reaches contained descendants | M4b |
| `ReflectImpl.SetAnnotations` | `Reflect().Annotations()` always empty — the data (`_VMF_OBJECT_ANNOTATIONS`) is generated but never read | M5 |
| `VmfType.SetSuperTypes` | `SuperTypes()` always empty — `_VMF_SUPER_TYPE_NAMES` likewise generated and never read | M5 |
| `IChange.Apply(target)` | implemented, never called — suggests undo/redo is partly built rather than absent | M8 |
| `GetContainerPropertyId`, `UnsetById`, `SetDefaultValueById`, `ITraversalListener.Traverse` | implemented/declared, unreachable | M9 (decide: wire or delete) |

### B. Missing capabilities

| Capability | Blocks | Milestone |
|---|---|---|
| Change observation through a read-only view (`ReadOnly*Impl.Vmf().Changes()` throws) | 2 facts | M4b |
| Batch list removal (Java `VList.removeAll(int...)` raises ONE change carrying several elements) | 1 fact | M4b |
| Settable `[Container]` property — never generated | 1 fact | M4b |
| Static type reflection (Java `Type.type().reflect()`) | 1 fact | M5 |
| Inherited `[DelegateTo]` — only methods declared on the type itself get a body | ~5 facts, 4 deviated models | M6 |
| Covariant property narrowing — C# interfaces cannot override a property type | ~3–6 facts, 5 deviated models | M7 |
| Collection default values | 1 fact | M9 |
| Cross-reference lists accept duplicates (Java keeps one reference) | 1 fact | M9 |
| Builder-accepting `With*` overloads (Java passes unbuilt nested builders) | 1 fact | M9 |
| `VListChangeEvent.Source`, so a listener can mutate the list it observes | 1 fact | M9 |
| A change event when a child's container changes (`SetContainer` fires none) | 1 fact | M8 |
| Clone and original are content-equal but traverse differently, so they do not serialise identically | 1 fact | investigate |

### C. Unknown

Undo/redo has an API (`IChange.Undo`, `ITransaction.Undo`, `IsUndoable`) and **zero** tests.
`TransactionImpl.Undo()` does call `_changes[i].Undo()`, so some of it is built. Verify before
scoping. Blocks `events_undo_redo` (5 facts).

## Milestones

| # | Milestone | Content | Done when |
|---|---|---|---|
| ~~M2′~~ | ~~Finish the port~~ | **DONE.** All 30 Java test classes ported; 62 active, 20 blocked | inventory closed |
| ~~M3~~ | ~~Release 0.2.1~~ | **DONE.** 19 defects fixed since 0.2.0, two crash-class; notes in [`CHANGELOG.md`](../CHANGELOG.md) | published to NuGet |
| ~~M4a~~ | ~~Cross-reference echo classification~~ | **DONE.** The induced side is marked with `IsCrossRefEcho` while it is updated, so its change is tagged `crossref-echo`; `ProcessChange` reports echoes but does not record them | all 3 cross_ref facts active |
| **M4b** | Change propagation | Propagate `SetModelToChanges` down containment as the graph mutates; read-only observation; batch list removal; settable container | `recursivelistener01`, `observableprop` green |
| **M5** | Reflection metadata | Populate `AllTypes`/`SuperTypes`/`Annotations`; add a static entry point. Then retire the `IsPolymorphic` call-site workaround from 0.1.4 | `annotations`, `staticreflection` green |
| **M6** | Inherited delegation | Generate bodies for inherited `[DelegateTo]`; type-level delegation | 4 models de-deviated |
| **M7** | Covariant narrowing | Public property at the narrowest type + forwarding explicit implementations per declaring interface | 5 models de-deviated |
| **M8** | Undo/redo | Verify what exists, then finish | `events_undo_redo` green |
| **M9** | Tail + audit | Collection defaults; decide wire-or-delete on the dead members; negative models as compile-gates; parity audit table | documented parity statement |

### Sequencing rationale

- **Port before feature work.** Every area ported so far has changed the plan. M2′ is cheap and
  completes the inventory before anything expensive is committed to.
- **Release early (M3).** Two crash-class bugs — a stack overflow on bidirectional
  cross-references, and silent listener orphaning — are in shipped 0.2.0. They should not wait
  for the whole roadmap.
- **Group by subsystem, not by symptom.** The gaps are not independent features; they are two or
  three unfinished subsystems. One design decision per subsystem resolves several symptoms.
- **M4 before M5.** It blocks the most facts and holds the remaining correctness risk. The
  reflection gaps are missing *data*, which is inert until something reads it.

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
