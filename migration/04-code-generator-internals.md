# Code Generator Internals

## Entry Points

### `eu.mihosoft.vmf.VMF` (public API)

Multiple `generate()` overloads that accept:
- `Class<?>... interfaces` - pre-loaded interface classes
- `String packageName` - discovers interfaces via ClassGraph scanning
- `File... sourceFiles` - compiles source files in-memory via JCompiler, then generates
- `Resource... resources` - same as files but via VMF's resource abstraction

All overloads ultimately call `CodeGenerator.generate(ResourceSet, Class<?>...)`.

### `eu.mihosoft.vmf.core.CodeGenerator`

Uses Apache Velocity template engine. Key flow:

```
generate(ResourceSet, Class<?>...)
  -> Model.newInstance(classes)        // Build model from interfaces
  -> generateModelTypeClasses(set, model)  // Per-type code generation
  -> generateModelTypeVMFApiClasses(set, model, pkg)  // Model-level code
```

Per-type generation dispatches based on type attributes:
- `@Immutable` -> `immutable-interface.vm` + `immutable-implementation.vm`
- `@InterfaceOnly` -> `interface.vm` + `interface__vmf_type.vm` (internal type)
- Normal -> `interface.vm` + `implementation.vm` + `read-only-interface.vm` + `read-only-implementation.vm`

## Model Construction (Multi-Pass)

`Model` class file: `core/src/main/java/eu/mihosoft/vmf/core/Model.java`

### Constructor Flow (7 passes)

```
Pass 0.0: Separate @ExternalType interfaces from model interfaces
Pass 0.1: Create ModelType for each interface (validates: must be interface, same package)
           Each ModelType: parses properties, delegations, annotations, extends/implements
Pass 1:   initContainments() - resolve @Contains/@Container opposite properties
Pass 2:   initCrossRefInfos() - resolve @Refers opposite properties
Pass 3:   initImplements() - resolve interface inheritance to ModelType references
Pass 5:   initPropertiesImportsAndDelegates() - collect inherited properties and delegations
Pass 4:   initSyncInfos() + initPropIds() - resolve @SyncWith, assign property IDs
Pass 6:   initProperties on Interface + compute all inherited types
Pass 7:   Validation pass - check immutability rules, delegation consistency
```

**Note**: Pass numbers are not sequential (5 runs before 4 in code) - this is intentional in the source.

### Key Model Classes

| Class | File | Purpose |
|---|---|---|
| `Model` | `core/.../Model.java` | Root model container. Holds all types, resolves types/opposites. |
| `ModelType` | `core/.../ModelType.java` | One per interface. Holds properties, delegations, inheritance info. |
| `Prop` | `core/.../Prop.java` | One per property. Parsed from getter method. Holds type info, containment, annotations. |
| `Implementation` | `core/.../Implementation.java` | Collects all properties including inherited ones. Manages imports and delegations. |
| `Interface` | `core/.../Interface.java` | Represents the public interface. Manages interface-level properties. |
| `ReadOnlyInterface` | `core/.../ReadOnlyInterface.java` | Read-only interface metadata. |
| `ReadOnlyImplementation` | `core/.../ReadOnlyImplementation.java` | Read-only implementation metadata. |
| `ContainmentInfo` | `core/.../ContainmentInfo.java` | Container/contained relationship with opposite property reference. |
| `ReferenceInfo` | `core/.../ReferenceInfo.java` | Cross-reference (bidirectional link) metadata. |
| `SyncInfo` | `core/.../SyncInfo.java` | Synchronization info for `@SyncWith`. |
| `DelegationInfo` | `core/.../DelegationInfo.java` | Method delegation metadata (class name, method signature, params). |
| `ModelConfig` | `core/.../ModelConfig.java` | Model-wide configuration from `@VMFModel`. |
| `AnnotationInfo` | `core/.../AnnotationInfo.java` | Custom annotation key-value pair. |

### Property Type Classification (`PropType` enum)

| PropType | Meaning | Detection |
|---|---|---|
| `PRIMITIVE` | Java primitive (`int`, `boolean`, etc.) | `Class.isPrimitive()` |
| `COLLECTION` | `List<T>` or `T[]` | `Collection.isAssignableFrom()` or `isArray()` |
| `CLASS` | Any other type (String, model types, external types) | Default |

### Property Type Resolution

For collection properties:
- Generic type is extracted from `List<T>` or array component type
- `genericTypeName` stores the element type name
- `genericPackageName` stores the element package
- `getGenericType()` resolves to `ModelType` if element is a model type

For non-collection properties:
- `getType()` resolves to `ModelType` if the property type is a model type
- Returns `null` for external/primitive types

## Velocity Templates

Located at: `core/src/main/resources/eu/mihosoft/vmf/vmtemplates/`

### Template Hierarchy

```
implementation.vm              <- Main mutable implementation template
  impl/declare-props.vm        <- Field declarations
  impl/declare-delegates.vm    <- Delegate instance declarations
  impl/constructor-delegates.vm <- Delegate initialization in constructor
  impl/getter.vm               <- Getter methods (dispatches to sub-templates)
    impl/getter-collection-contained.vm
    impl/getter-collection-referenced.vm
    impl/getter-collection-simple.vm
    impl/getter-container.vm
    impl/getter-no-collection-contained.vm
    impl/getter-no-collection-referenced.vm
    impl/getter-no-collection-simple.vm
  impl/setter.vm               <- Setter methods (dispatches to sub-templates)
    impl/setter-no-collection-contained.vm
    impl/setter-no-collection-container.vm
    impl/setter-no-collection-cross-reference.vm
    impl/setter-no-collection-simple.vm
  impl/delegation-methods.vm   <- Delegated method implementations
  impl/equals.vm               <- equals/hashCode generation
  impl/to-string.vm            <- toString generation
  impl/clone.vm                <- Deep clone implementation
  impl/builder.vm              <- Builder class
  impl/sync.vm                 <- @SyncWith handling
  impl/set-references.vm       <- Reference management
  impl/set-opposite-via-reflection.vm
  impl/remove-containment-opposites.vm
  impl/remove-containment-opposites-collection.vm
  reflection/reflection.vm     <- Reflect API implementation

interface.vm                   <- Main public interface template
  interface/getter.vm
  interface/setter.vm
  interface/builder.vm
  interface/delegation-methods.vm

read-only-interface.vm
read-only-implementation.vm
immutable-interface.vm
immutable-implementation.vm
interface__vmf_type.vm         <- Internal type interface for @InterfaceOnly

vmf-model-class.vm             <- Model API class
vmf-model-switch-interface.vm  <- Type switch/visitor
vmf-model-switch-read-only-interface.vm
vmf-model-traversal-listener-interface.vm
vmf-model-traversal-listener-read-only-interface.vm
vmf-model-walker-implementation.vm
vmf-vcloneable-internal.vm
vmf-vobject-internal.vm
reflection/reflection.vm
reflection/reflection-read-only.vm
```

### Template Context Variables

Each template receives a `VelocityContext` with:
- `$type` - the `ModelType` instance
- `$modelName` - model name (derived from package)
- `$VMF_TEMPLATE_PATH` - base path for template includes
- `$VMF_RUNTIME_API_PKG` - `eu.mihosoft.vmf.runtime`
- `$VMF_IMPL_PKG_EXT` - `impl`
- `$VMF_IMPL_CLASS_EXT` - `Impl`
- `$VMF_CORE_PKG_EXT` - `core`
- `$VCOLL_PKG` - `eu.mihosoft.vcollections`
- `$StringUtil` - utility class for string operations

## Resource System

VMF uses an abstraction layer for output:

| Interface/Class | Purpose |
|---|---|
| `ResourceSet` | Factory for creating output resources |
| `Resource` | Single output file (writable, closeable) |
| `FileResourceSet` | Writes to filesystem |
| `MemoryResourceSet` | Writes to in-memory strings (for testing) |
| `JavaFileResourceSet` | Maps Java FQN to file paths |

## Package Name Transformation

The `vmfmodel` suffix is stripped from the user's package:
- Input: `eu.mihosoft.myapp.vmfmodel` 
- Generated interfaces: `eu.mihosoft.myapp`
- Generated implementations: `eu.mihosoft.myapp.impl`
