# Java test-suite parity — roadmap

**Goal:** the .NET suite covers what the Java project's `test-suite` module covers.
**Status:** models ported; test port in progress. **Last updated:** 2026-08-23.

> Companion doc: [`source-generator-dependencies.md`](source-generator-dependencies.md).
> Suite layout and porting conventions: [`../src/VMF.NET.TestSuite/README.md`](../src/VMF.NET.TestSuite/README.md).

## Where things stand

| | Java `test-suite` | `VMF.NET.TestSuite` |
|---|---|---|
| Model areas | 39 | **39 — all ported** |
| Test classes | 30 (excl. 2 deliberate non-ports) | 6 ported |
| Facts | ~80 | 41 ported: **32 active, 9 skipped** |

Suite totals today: **227 passing**, 9 skipped, 0 failing.

Two Java classes are deliberately **not** ported: `MemoryResourceSetTest` (tests a Java I/O
abstraction with no .NET counterpart, and is commented out upstream) and `VMFGeneratorTest`
(already covered by `GeneratorCompilesTests` in `VMF.NET.Tests`).

### Ported so far

| Area | Facts | Active | Blocked |
|---|---|---|---|
| `containment` | 11 | 11 | — |
| `equals` | 14 | 14 | — |
| `lazyinit` | 2 | 2 | — |
| `cross_ref` | 3 + 4 regression | 4 | 3 |
| `observableprop` | 5 | 1 | 4 |
| `recursivelistener01` | 2 | 0 | 2 |

A blocked fact is kept as `[Fact(Skip = "...")]` with the missing capability named, so the
**skip count is the parity gap** rather than the fact quietly disappearing.

## Gap inventory

Everything below is evidence-backed: either a fact that fails, or an API the audit shows is
never wired. Fact counts outside the change/reflection rows are estimates.

### A. Unwired plumbing

Declared, sometimes implemented, never called. This is the dominant pattern.

| Member | Consequence | Milestone |
|---|---|---|
| `IChangeInternal.IsCrossRefChange` / `IsContainmentChange` | implemented and never called, so `ChangesManager` cannot tell a cross-reference echo from an initiating change and records both | M4a |
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

### C. Unknown

Undo/redo has an API (`IChange.Undo`, `ITransaction.Undo`, `IsUndoable`) and **zero** tests.
`TransactionImpl.Undo()` does call `_changes[i].Undo()`, so some of it is built. Verify before
scoping. Blocks `events_undo_redo` (5 facts).

## Milestones

| # | Milestone | Content | Done when |
|---|---|---|---|
| **M2′** | Finish the port | ~19 facts across 14 small areas: `tostring`, `builders`, `getteronly`, `immutabletypes`, `propertytype`, `defaultvaluesandbuilders`, `ignoretostring`, `annotations`, `staticreflection`, `propertyorder`, `test1`, `test2`, `delegationtest`, `nopropertiestest`, and the 5 small `complex/*` | inventory complete; no unexamined Java fact remains |
| **M3** | **Release 0.2.1** | 13 defects fixed since 0.2.0, two crash-class | published |
| **M4a** | Change classification | Wire `IsCrossRefChange` into `ProcessChange`; honour the recursive flag | cross-ref recording facts pass |
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
