# API coverage audit

*Run 2026-09-01, in two passes: the runtime assemblies (`VMF.NET.Runtime`, `VMF.NET.Json`), then
the generated API. Tracked by [issue #2](https://github.com/aheusel/VMF.NET/issues/2).*

## Why

Five defects shipped in 0.3.0 and 0.3.1 that were reachable from ordinary use, and none was found
by the test suite. They were found by writing tutorials. The suite is a 1:1 port of Java's, so
anything Java exercises only in *its* tutorials has no test to port and ends up untested here too
— see [*What 101/103 does and does not tell you*](../src/VMF.NET.TestSuite/README.md).

That explains why those five were missed. It does not say whether they were the only ones. This
audit answers that by enumeration rather than by waiting for the next failure.

## Method

Deliberately mechanical, so it can be re-run and disagreed with:

1. Reflect over the built `VMF.NET.Runtime.dll` and `VMF.NET.Json.dll` and dump every public
   member as `Type|kind|Member` (public types only, declared members only, property accessors and
   constructors excluded).
2. Concatenate every hand-written `.cs` under `src/VMF.NET.TestSuite` and `src/VMF.NET.Tests`,
   excluding `obj/` and `bin/`.
3. Report every member whose name does not appear as a whole word in that text.

**Both directions are approximate, and in known ways.** A common name (`Value`, `Add`, `Name`)
matches something unrelated and reports coverage that may not exist — so the *covered* side is
optimistic. A member called only from generated code, which lives in `obj/`, reports as
uncovered — so the *uncovered* side over-reports. The uncovered list is therefore a set of
candidates to look at by hand, not a verdict. That hand pass is the actual audit; the script only
narrows it.

## Result

**242 public members. 69 unreferenced by any test at the start, 51 now.**

The 69 fell into four groups:

| group | count | verdict |
|---|---|---|
| `Runtime.Internal.*` | 29 | Not actionable. This is the contract generated code implements — `IVObjectInternal`, `VmfTypeRegistry`, `ChangeNotification`. Every generated type implements it, so every test in the suite exercises it; the grep cannot see that because the callers are in `obj/`. |
| Attribute properties | 5 | Not actionable. `ContainsAttribute.Opposite` and friends are read by the *generator*, from attribute syntax via Roslyn, never through the CLR property. The property exists for API completeness. |
| Exercised indirectly, not asserted directly | 17 | Acceptable, listed below. |
| **Genuinely untested, user-facing** | **18** | **Closed — tests added.** |

## What was closed

Each of these was implemented, wired up, reachable from user code, and had nothing exercising it:

| API | now covered by |
|---|---|
| `ITraversalListener` — `OnEnter`, `OnExit`, both static `Traverse` overloads | `ContentTraversalTests` |
| `VIterator.Of`, `Current`, `IsAddSupported`, `Reset` | `ContentTraversalTests` |
| `IterationStrategy.UniqueProperty` | `ContentTraversalTests` |
| `IChange.Apply` (replay onto another object), `IChange.IsUndoable` | `UndoTests` |
| `IContent.ContentHashCode` | `ReadOnlyReflectionTests` |
| `VList.EventInfo` → `VListChangeEvent.EventInfo` | `VObjectsTests` |
| `VmfJsonConverterFactory.WithTypeAlias`, `VmfJsonSchemaGenerator.WithTypeAlias` | `SchemaGenerationTests` |

Nothing broke. All eighteen behaved as intended — the value here is that they now stay that way,
not that a sixth defect turned up.

The type aliases are worth singling out. Asked whether the serializer honoured them, I read the
code, saw `_factory.TypeAliasesReverse`, and reported "already correct, no work needed". That was
right, but it was an assertion from reading rather than a measurement — the exact habit that let
the five defects through. It now has three tests, including one that pins the schema's `$ref`
targets, its `@vmf-type` enum values, and its `definitions` keys all to the alias the serializer
actually writes.

## What remains, and why

17 members, all exercised indirectly by tests that assert the behaviour they contribute to rather
than calling them by name:

- `VmfTypeUtils.ShouldSerialize` / `IsContainedProperty` / `IsContainerProperty` /
  `IsImmutableType` / `FieldName` / `GetFieldName` / `GetSubTypes` — every JSON serialization and
  schema test runs through these. `ShouldSerialize` in particular is asserted by the
  what-is-and-is-not-serialized tests; `IsImmutableType` was *added* by one of the 0.3.2 fixes and
  is covered by the schema tests that fix produced.
- `VmfJsonConverterFactory.CanConvert` / `CreateConverter` — called by `System.Text.Json` itself
  on every JSON test.
- `VmfJsonNaming.Default` / `Resolve` — the naming tests assert the resulting field names.
- `IReflect.AllTypes` — the schema generator's definitions walk depends on it.
- `VListChangeEvent.CreateRemoveEvent` / `CreateSetEvent` — called by `VList` itself; any
  list-change test exercises them.
- `VmfSchemaKeys.UniqueItems` — the behaviour is tested via the literal key string; only the
  constant is unreferenced.
- `VmfJsonConverterFactory.WithTypeAliases` — the bulk form of the now-tested singular.
- `ITraversalListener.IgnoreNullObjects` — a default interface property no implementation
  overrides.

These are judgement calls, not oversights. Each is reachable only through something already
asserted, so a direct test would restate an existing one. The two worth revisiting if the surface
grows are `WithTypeAliases` and `IgnoreNullObjects`, which are genuinely unexercised but trivial.

## The generated API

*Second pass, same day. The first pass covered the runtime assemblies and explicitly did not
claim this.*

The generated API — `NewInstance`, `NewBuilder`, and the rest — is emitted into the **consumer's**
assembly, so reflecting over `VMF.NET.Runtime` cannot see it. Recovered instead from a built
consumer, `VMF.NET.TestSuite.dll`, which contains 660 generated model types.

Separating generator-emitted members from model-declared properties needs no hand-maintained
list: count how many distinct model types declare each name. Infrastructure appears on nearly all
of them; a model's own property appears on a handful. The split is unambiguous:

| member | on N model types | |
|---|---|---|
| `Clone` | 522 | mutable **and** read-only interfaces |
| `AsReadOnly`, `Builder`, `GetModelType`, `NewBuilder`, `NewInstance` | 273 | every instantiable model type |
| `AsModifiable` | 249 | read-only interfaces only |
| `Builder.Build`, `Builder.ApplyFrom`, `Builder.ApplyTo` | 273 | |
| `Builder.With<Property>` | per property | |
| — | — | — |
| `Name` | 95 | *a model property, not infrastructure* |
| `Parent` | 62 | *ditto* |

The gap between 249 and 95 is the whole answer — there is no conditionally-emitted infrastructure
hiding among the model properties. `VMF` does not appear because it is declared on `IVObject` and
inherited, so it belongs to the first pass.

**Every one of those members is exercised by the suite.** `Builder` reads as unused only because
the nested type is never named — it is obtained from `NewBuilder()`.

### The gap it found

Presence is not coverage, so the thin ones were read rather than counted. `GetModelType` (a
dedicated `StaticReflectionTest`), `Clone`, and `AsReadOnly` are properly asserted. One was not:

**A supertype's builder applies only that supertype's state.** `ApplyFrom`/`ApplyTo` were covered,
but *every* call site used the builder of the same type — where "copies the properties" and
"copies only this type's properties" cannot be told apart. The selective behaviour had no test.

That is exactly the semantics VMF-Tutorial-05 exists to teach, and its absence has already cost
something: the pre-0.3.0 port of that tutorial had been flattened into a single interface,
destroying the lesson, and nothing caught it. Now pinned by
`InheritanceCodegenTests.ASupertypeBuilder_AppliesOnlyTheSupertypesProperties`, with a
same-type-builder contrast test beside it so the assertion cannot be satisfied vacuously.

## What this audit does not measure — demonstrated, not hypothetical

**Reached is not covered.** The method asks whether a member is exercised. It cannot ask whether
its *cases* are.

One day after this audit was written, a crash was reported from a real model:
`VmfTypeUtils.GetSubTypes` threw for any model namespace containing an `[InterfaceOnly]` type,
breaking schema generation for every model-typed property in it. `GetSubTypes` is on the
*"exercised indirectly"* list above — every polymorphic schema test runs through it — and that was
accurate. It was thoroughly exercised, and thoroughly broken, because no test had ever put an
interface-only type in a namespace under schema generation.

So the honest reading of "51 unreferenced, 19 closed" is: **no member of the public surface is
now completely unexercised.** That is a floor, not a ceiling. It says nothing about input
combinations, and a defect needs only one uncovered combination.

Worth keeping in view when this document is cited as evidence.

## Deliberately out of scope

**`VMF.NET.Core` and `VMF.NET.SourceGenerator`.** A public-member audit would measure the wrong
thing here. These are compile-time internals whose contract is *model source text in, diagnostics
and generated files out*, and that is how `VMF.NET.Tests` tests them — 98 tests driving a
`GeneratorDriver` over model source. Counting which of `ModelAnalyzer`'s public methods a test
names would say nothing about whether the generator behaves correctly.

## Result, both passes

**19 untested user-facing members found and closed** — 18 in the runtime surface, one behaviour in
the generated surface. No defect turned up: everything already worked. The value is that the
question is now answered by enumeration, and that these stay working.

## Re-running it

The dumper is ~60 lines of reflection and was written as a throwaway. Rebuild it from the method
above rather than hunting for it; the value is in the hand pass over the candidate list, which no
script produces. The one gap in the second pass was found by *reading* the thin call sites, not by
the counting — presence is not coverage, and only the hand pass sees the difference.
