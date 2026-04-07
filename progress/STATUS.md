# VMF.NET Migration Status

## Current Phase: Phase 4 — End-to-End Integration Testing (Complete)

## Completed
- [x] Migration documentation (11 docs in `migration/`)
- [x] Design decisions finalized (see `decisions.md`)
- [x] Project scaffold created
- [x] **Phase 1: Public runtime interfaces** — IVObject, IVmf, IContent, IChanges, IChange, IPropertyChange, IReflect, IAnnotation, ITransaction, IModelVersion, IBuilder, IBehavior/IDelegatedBehavior, ITraversalListener, marker interfaces (IMutable, IReadOnly, IImmutable, IInterfaceOnly)
- [x] **Phase 1: VmfProperty and VmfType** — reflective property/type access classes
- [x] **Phase 1: VIterator** — depth-first object graph iterator with UniqueNode/UniqueProperty/ContainmentTree strategies
- [x] **Phase 1: VList\<T\>** — ObservableCollection-based list with change events, element add/remove callbacks, and listener subscription
- [x] **Phase 1: VListChangeEvent** — typed change event (Add/Remove/Set) with factory methods
- [x] **Phase 1: Model definition attributes** — Contains, Container, Refers, Immutable, InterfaceOnly, VmfModel, VmfEquals, VmfDefaultValue, VmfRequired, GetterOnly, IgnoreEquals, IgnoreToString, PropertyOrder, DelegateTo, ExternalType, VmfAnnotation, Doc, SyncWith
- [x] **Phase 1: Internal implementations** — IVObjectInternal, IVObjectInternalModifiable, AnnotationImpl, ModelVersionImpl, PropChangeImpl, ListChangeImpl, TransactionImpl, ChangesManager, ReflectImpl, ChangeInternal
- [x] **Phase 1: Unit tests** — 25 tests covering VList, ChangesManager, AnnotationImpl, VmfType (all passing)
- [x] **Phase 2: Model representation classes** — PropertyInfo, ModelTypeInfo, ModelInfo, ContainmentInfo, ReferenceInfo, SyncInfo, DelegationInfo, AnnotationInfo, ModelConfig, ContainmentType/PropType enums
- [x] **Phase 2: SymbolData DTOs** — TypeSymbolData, PropertySymbolData, DelegationSymbolData, VmfModelData, VmfEqualsData, AnnotationData (Roslyn-independent bridge)
- [x] **Phase 2: ModelAnalyzer** — 7-pass analysis pipeline: type creation, property init, containment, cross-refs, inheritance, all-properties collection, sync/prop-IDs, transitive inheritance, validation
- [x] **Phase 2: netstandard2.0 compatibility** — replaced HashCode.Combine with manual hash, replaced Index/Range syntax with array indexing
- [x] **Phase 2: Unit tests** — 33 tests covering ModelAnalyzer (all 58 tests passing)
- [x] **Phase 3: SymbolExtractor** — Roslyn INamedTypeSymbol → TypeSymbolData conversion (reads attributes, properties, base types, delegations)
- [x] **Phase 3: VmfSourceGenerator** — IIncrementalGenerator entry point, discovers VMF-attributed interfaces, groups by namespace, runs ModelAnalyzer, renders templates
- [x] **Phase 3: TemplateRenderer** — Loads embedded Scriban templates, renders per ModelTypeInfo with helper functions (prop_type_name, field_name, etc.)
- [x] **Phase 3: Scriban templates** — Interface.sbn (partial, adds Builder/Clone/AsReadOnly/factories), ReadOnlyInterface.sbn, Implementation.sbn (full impl class), ReadOnlyImplementation.sbn (wrapper delegate)
- [x] **Phase 3: Generated code features** — Property getters/setters, containment tracking, cross-reference sync, VList lazy init with change listeners, IVObjectInternalModifiable, property change events, ChangesManager integration, Builder pattern, Clone via copy constructor with identity map, content-based Equals/HashCode with cycle detection, JSON-style ToString, ReadOnlyMappedList helper, Reflection/Content/Vmf API implementations
- [x] **Phase 3: Integration tests** — 5 tests: template rendering, compilation, content equals, builder, full Roslyn pipeline (all 63 tests passing)
- [x] **Phase 4: IntegrationTests project** — VMF.NET.IntegrationTests with source generator wired as analyzer, FlowModel (IFlow/INode/IConnection) with containment + cross-references
- [x] **Phase 4: BasicPropertyTests** (9 tests) — NewInstance, property get/set, int defaults, collection init, Builder, ApplyFrom, ApplyTo
- [x] **Phase 4: ContainmentTests** (5 tests) — container tracking, removal clears container, move between parents, Connection container, multiple children
- [x] **Phase 4: CloneEqualsChangesTests** (8 tests) — deep clone, clone independence, content equals, not-equals, ToString, change listeners for property/list changes, change recording
- [x] **Phase 4: ReadOnlyReflectionTests** (12 tests) — read-only wrapper, mapped collections, equals delegation, wrapper caching, Reflect properties/type, IVObjectInternal property access, Content.Stream, typed streaming, DeepCopy, cross-ref Sender/Receiver sync, cross-ref unset
- [x] **Phase 4: Bug fixes** — HashSet cycle detection (ReferenceEqualityComparer for VmfHashCode/VmfToString, identity-based pair keys for VmfEquals), VList copy constructor (read from VList not raw list), VIterator IList covariance (use non-generic IList), VIterator empty-list skip in Next(), model_type_indices includes collection-element-type properties
- [x] **Phase 4: All tests passing** — 97 total (63 unit + 34 integration)

## Completed (Phase 5)
- [x] **Phase 5: ShallowCopy** — `ShallowCloneInternal()` method on impl, `ContentImpl.ShallowCopy<T>()` wired, shallow copy of contained collections snapshots list before iterating to avoid concurrent modification
- [x] **Phase 5: Property annotations** — Static `_VMF_PROPERTY_ANNOTATIONS` array generated per type, includes user `[VmfAnnotation]` entries + auto containment-info annotations, `GetPropertyAnnotationsById()` returns from array
- [x] **Phase 5: Type-level annotations** — Static `_VMF_OBJECT_ANNOTATIONS` array with user annotations + auto `vmf:type:immutable`/`vmf:type:interface-only` markers, `GetAnnotations()` method on `IVObjectInternal`
- [x] **Phase 5: Behavior delegation** — Already complete from Phase 3 (constructor + method delegation via `[DelegateTo]`)
- [x] **Phase 5: NuGet packaging** — Already correctly configured (VMF.NET.Core bundled into analyzer, Scriban bundled)
- [x] **Phase 5: Integration tests** — 11 new tests (shallow copy, property annotations, type annotations, read-only delegation) — all 108 tests passing

## Completed (Phase 6)
- [x] **Phase 6: Immutable types** — `[Immutable]` types: no setters, `IImmutable` marker, `Clone()`/`AsReadOnly()` return self, builder sets fields directly, collections exposed as `IReadOnlyList<T>`, no change events, no read-only wrapper class, `IsReadOnly` returns true
- [x] **Phase 6: VmfRequired validation** — Builder.Build() throws `InvalidOperationException` if required scalar properties not set or required collections empty
- [x] **Phase 6: Warning cleanup** — Eliminated all CS0109 (unnecessary `new`) and CS0108 (hidden member) warnings in generated code, reduced total from 123 to 99
- [x] **Phase 6: Integration tests** — 18 new tests (immutable types, required validation, IImmutable marker, reflect, deep copy, builder) — all 126 tests passing

## Completed (Phase 7)
- [x] **Phase 7: PropertyOrder** — Already fully working: analyzer sorts by `[PropertyOrder]` index or alphabetical, reflection API exposes correct order
- [x] **Phase 7: Doc comments** — Already working in ReadOnlyInterface template; fixed double-doc-comment bug for documented immutable types
- [x] **Phase 7: ExternalType** — Already fully implemented: analyzer filters in pass 0.0, no code generated, properties treated as opaque
- [x] **Phase 7: Warning cleanup** — Eliminated all generated-code CS warnings: fixed container field declarations (CS0649), fixed equals/hashcode ordering for container props, added pragma disables for nullability warnings, reduced CS code warnings to 2 (both in VMF.NET.Core)
- [x] **Phase 7: Integration tests** — 6 new tests (property order, builder, doc, toString) — all 132 tests passing (63 unit + 69 integration)

## Completed (Phase 8)
- [x] **Phase 8: VObjects utility class** — Static `VObjects.Equals()` for VMF object and collection comparison using `VmfEquals` internally
- [x] **Phase 8: VMF.NET.Json package** — `System.Text.Json`-based serialization for VMF objects (port of `vmf-jackson`)
  - `VmfJsonConverterFactory` — `JsonConverterFactory` for registering on `JsonSerializerOptions`
  - `VmfJsonConverter<T>` — Serializes via reflection API (contained + immutable + external props), deserializes via builder pattern (`With*` methods)
  - `VmfTypeUtils` — Serialization decisions: skip container (child-side), skip cross-refs, include contained/immutable/external
  - `VmfJsonSchemaGenerator` — JSON Schema (draft-07) generation from VMF model types
  - Polymorphic support via `@vmf-type` discriminator field + type aliases
  - Naming policy support (e.g., `JsonNamingPolicy.CamelCase`)
- [x] **Phase 8: Gap analysis** — 95%+ feature parity; `ModelDiff` deliberately skipped (deprecated in Java)
- [x] **Phase 8: Integration tests** — 21 new tests (VObjects, JSON serialization/deserialization/round-trip, JSON Schema) — all 153 tests passing (90 integration + 63 unit)

## Completed (Phase 9)
- [x] **Phase 9: Dropped SyncWith** — Removed `SyncWithAttribute`, `SyncInfo`, analyzer sync pass, and SymbolExtractor sync extraction (deprecated in Java, zero test coverage)
- [x] **Phase 9: VmfAnnotation value extraction fix** — Fixed `ExtractAnnotation` to correctly map constructor arg to value (not key); the `VmfAnnotationAttribute(string value)` constructor takes the value, `Key` is a named property
- [x] **Phase 9: VList\<T\> hash code fix** — Fixed template to skip `IVObjectInternal` pattern for collections of non-model types (e.g., `VList<string>`) which caused CS8121
- [x] **Phase 9: JSON rename annotation** — `vmf:jackson:rename` now works in both serialization (uses renamed field name) and deserialization (maps renamed JSON fields back to builder `With*` methods via prototype reflection)
- [x] **Phase 9: JSON Schema annotation features** — Full port of annotation-driven schema from Java `VMFJsonSchemaGenerator`:
  - `vmf:jackson:schema:description` → `description`
  - `vmf:jackson:schema:constraint` → `minimum`, `maximum`, `pattern`, etc. (multiple allowed, `key=value` format, auto-parses int/double/bool)
  - `vmf:jackson:schema:format` → `format` (e.g., `hostname`, `email`, `uri`)
  - `vmf:jackson:schema:title` → `title`
  - `vmf:jackson:schema:uniqueItems` → `uniqueItems` (boolean)
  - `vmf:jackson:schema:propertyOrder` → `propertyOrder` (integer)
  - `vmf:jackson:schema:inject` → arbitrary JSON key-value injection
  - `vmf:jackson:rename` → schema uses renamed field names
  - Default values included from `[VmfDefaultValue]`
- [x] **Phase 9: Integration tests** — 11 new tests (rename serialize/deserialize/round-trip, schema description/constraints/format/title/uniqueItems/propertyOrder/default/rename) — all 163 tests passing (101 integration + 62 unit)

## In Progress
- (nothing)

## Next Up

### Phase 10: CI/CD Fix & NuGet Publish
Fix broken CI/CD workflows, then publish v0.1.0 to nuget.org.

**CI/CD issues identified (2026-04-07):**
- [ ] Wrong .NET version in both workflows (`8.0.x` → `10.0.x`) — CI build currently fails
- [ ] `publish.yml` does not run tests before publishing
- [ ] No version derived from git tag — packages always emit `1.0.0`; fix with `/p:Version=${GITHUB_REF_NAME#v}` or `MinVer`
- [ ] No `RepositoryUrl` in `Directory.Build.props` — NuGet page won't link to GitHub
- [ ] Test/IntegrationTests projects not marked `<IsPackable>false</IsPackable>` — `dotnet pack` emits junk packages
- [ ] `VMF.NET.Runtime` and `VMF.NET.Json` have no explicit `TargetFramework` — inheriting `net10.0` from `Directory.Build.props` limits consumer compatibility (consider `net6.0` or multi-targeting)
- [ ] NuGet secret `NUGET_API_KEY` must be set in GitHub repo settings before first publish

**Steps:**
1. Fix `build.yml` and `publish.yml` (dotnet 10, test-before-publish, version from tag)
2. Add `RepositoryUrl`, `PackageTags` to `Directory.Build.props`
3. Mark test projects `<IsPackable>false</IsPackable>`
4. Decide on `TargetFramework` for Runtime/Json (net6.0+ vs net10.0)
5. Add `NUGET_API_KEY` secret to GitHub repo
6. Push `v0.1.0` tag → verify packages appear on nuget.org

### Phase 11: VMF-Tutorials C# Port
Port `~/source/VMF-Tutorials` (Java) to C# as a standalone solution that consumes VMF.NET packages **from nuget.org** (not project references).

- Serves as real-world validation with a non-trivial model
- Must be a separate solution/repo pulling published NuGet packages
- Port after Phase 10 is complete and packages are live on nuget.org

## Blockers
- None

## Notes
- Target framework updated to net10.0 (system has .NET 10 SDK)
- VMF.NET.Core targets netstandard2.0 (required for source generators)
- User-written interfaces MUST be `partial` (generator extends them)
- EqualsType enum: Instance (default), ContainmentAndExternal, All
- VListChangeEvent factory methods are public (needed by tests and external consumers)
- VmfType.Create is public (for test and external type construction)
- ModelAnalyzer splits type creation and property init into separate loops so all types are resolvable during property initialization
- Type IDs increment by 2 per type to accommodate read-only variants
- ReflectImpl.SetStaticOnly is public (called by generated read-only impl code)
- TemplateRenderer is public (for direct testing without Roslyn pipeline)
- VList constructor copies items (doesn't use backing list as storage) — clone must read from VList, not raw list
- HashSet\<object\> for cycle detection must use ReferenceEqualityComparer.Instance (default uses GetHashCode which recurses)
- VmfEquals visited set uses HashSet\<long\> with identity hash pair keys to avoid GetHashCode recursion
- VIterator uses non-generic System.Collections.IList to handle VList\<T\> (IList\<T\> is invariant in C#)
- GetIndicesOfPropertiesWithModelTypeOrElementTypes includes both scalar model-type and collection-with-model-element-type properties
