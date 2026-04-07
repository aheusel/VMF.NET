# VMF Complete Feature Catalog

This document catalogs every VMF feature with its implementation details, for use in planning the C# migration.

## 1. Property System

### 1.1 Simple Properties
- **Primitives**: `int`, `long`, `float`, `double`, `boolean`, `char`, `byte`, `short`
- **Boxed types**: Auto-boxed for collections
- **String**: Treated as external type
- **External types**: Any non-model Java class

**Source**: `Prop.java:171-268` - type detection from getter return type

### 1.2 Collection Properties
- **Declaration**: `List<T>` or `T[]` (array syntax is syntactic sugar)
- **Runtime type**: `VList<T>` (from `eu.mihosoft.vcollections`)
- **Only `List` supported** (not `Set`, `Map`, etc.)
- **Lazy initialization**: Collections created on first getter access

**Source**: `Prop.java:177-253`, templates `impl/getter-collection-*.vm`

### 1.3 Default Values
- Annotation: `@DefaultValue("expression")`
- Value is a Java code literal string, inserted verbatim into generated code
- Applied in constructor: `__vmf_prop_name = <expression>;`

**Source**: `Prop.java:278-280`, template `implementation.vm:78-86`

### 1.4 Property Ordering
- Annotation: `@PropertyOrder(index=N)`
- All or none properties must be annotated (no partial)
- Indices must be unique
- Default: alphabetical by property name

**Source**: `ModelType.java:186-221`

### 1.5 Required Properties
- Annotation: `@Required`
- Currently used primarily by builders/validation
- No automatic null-check enforcement in setters

## 2. Containment (Parent/Child Ownership)

### 2.1 Contains (Parent Side)
- Annotation: `@Contains(opposite = "parentPropName")`
- Opposite is optional: `@Contains` without opposite creates one-way containment
- Works on single properties and collections
- Adding a child to a new parent removes from old parent automatically

### 2.2 Container (Child Side)
- Annotation: `@Container(opposite = "childrenPropName")`
- Container property is automatically read-only
- Empty opposite: `@Container(opposite="")` - read-only property with no explicit link

### 2.3 Containment Semantics
- An object can have at most ONE container (tree structure, not graph)
- Setting `parent.setChild(c)` auto-sets `c.getParent() == parent`
- Removing from collection auto-clears container reference
- Adding to new parent auto-removes from old parent

**Source**: `Prop.java:366-438`, `ContainmentInfo.java`, templates `impl/setter-no-collection-contained.vm`, `impl/getter-collection-contained.vm`

## 3. Cross-References

### 3.1 Bidirectional References
- Annotation: `@Refers(opposite = "propName")`
- Both sides must declare `@Refers`
- Works on single properties and collections
- Adding A to B.refs auto-adds B to A.refs (and vice versa)
- Removing A from B.refs auto-removes B from A.refs

**Source**: `Prop.java:474-511`, `ReferenceInfo.java`, template `impl/setter-no-collection-cross-reference.vm`

## 4. Immutability

### 4.1 Immutable Types
- Annotation: `@Immutable` on interface
- Only getters generated (no setters)
- State set exclusively via builder pattern
- Only `@Immutable` property types allowed (no mutable refs)
- Cannot have containment (`@Contains`/`@Container`)
- Shared during deep clone (not copied)

**Source**: `ModelType.java:131-132`, `Model.java:185-225`, templates `immutable-interface.vm`, `immutable-implementation.vm`

### 4.2 Read-Only Wrappers
- Every mutable type has a `ReadOnly<Type>` interface and implementation
- Obtained via `obj.asReadOnly()`
- Delegates getters to wrapped mutable instance
- Model-type properties return their read-only versions
- Collections return unmodifiable views
- `asModifiable()` on read-only objects returns a deep copy of the underlying mutable instance

**Source**: `ReadOnlyInterface.java`, `ReadOnlyImplementation.java`, template `read-only-implementation.vm`

## 5. Interface-Only Types

- Annotation: `@InterfaceOnly`
- Only interface generated, no implementation class
- Cannot be instantiated (`newInstance()` not generated)
- Used as abstract base types for inheritance hierarchies
- Can be combined with `@GetterOnly` on properties

**Source**: `ModelType.java:132`, template `interface__vmf_type.vm`

## 6. Inheritance

- Model interfaces can `extends` other model interfaces
- Properties are inherited (collected in `Implementation.initPropertiesImportsAndDelegates()`)
- Delegations are inherited
- Constraints:
  - Mutable cannot extend `@Immutable`
  - `@Immutable` can extend `@Immutable` or `@InterfaceOnly` with `@GetterOnly` props

**Source**: `ModelType.java:351-384`, `Implementation.java:109-164`

## 7. Delegation (Custom Behavior)

### 7.1 Method Delegation
- Annotation: `@DelegateTo(className="com.example.MyBehavior")` on non-getter methods
- Delegation class must implement `DelegatedBehavior<T>`
- `setCaller(T)` called on object creation
- Methods forwarded to delegate instance

### 7.2 Constructor Delegation
- `@DelegateTo` on the interface itself (type-level)
- Delegate instantiated and `setCaller()` called in constructor
- **Instantiation callback**: If the delegate class defines `on<TypeName>Instantiated()`, it is called in the constructor after `setCaller()`

### 7.3 Special Delegations
- `equals(Object)` and `hashCode()` can be delegated (must delegate both or neither)
- `toString()` can be delegated
- `clone()` can be delegated

**Source**: `DelegationInfo.java`, `ModelType.java:283-319`, template `impl/delegation-methods.vm`

## 8. Builder Pattern

- Generated for all non-`@InterfaceOnly` types
- Nested static class `MyType.Builder`
- `MyType.newBuilder()` factory method
- `with<PropertyName>(value)` for each property
- `append<PropertyName>(items...)` for collection properties
- `build()` creates and returns the instance
- `applyFrom(instance)` copies property values from an existing instance into the builder
- `applyTo(instance)` copies property values from the builder to an existing instance
- Parent interface builders enable selective property transfer (e.g., `WithName.newBuilder().applyFrom(node1).applyTo(node2)` copies only `WithName` properties)
- For `@Immutable` types: only way to create instances

**Source**: Template `impl/builder.vm`, `interface/builder.vm`

## 9. Equals / HashCode / ToString

### 9.1 Equals Strategies
- `INSTANCE` (default): Java identity (`==`)
- `CONTAINMENT_AND_EXTERNAL`: Compare contained + external properties
- `ALL`: Compare all properties
- Per-model: `@VMFModel(equality = EqualsType.ALL)`
- Per-type: `@VMFEquals(EqualsType.ALL)`
- `@IgnoreEquals` excludes individual properties
- Container properties always excluded

### 9.2 Content-Based Equality
- Available via `obj.vmf().content().equals(other)`
- Uses `CONTAINMENT_AND_EXTERNAL` strategy regardless of model config
- Handles cycles via visited-set

### 9.3 ToString
- Generated with all properties (except `@IgnoreToString`)
- Can be delegated via `@DelegateTo`

**Source**: `VMFEquals.java`, `ModelConfig.java`, template `impl/equals.vm`, `impl/to-string.vm`

## 10. Cloning

### 10.1 Deep Clone
- `obj.clone()` or `obj.vmf().content().deepCopy()`
- Recursively clones containment tree
- Shares immutable references
- Re-links cross-references within cloned graph

### 10.2 Shallow Clone
- `obj.vmf().content().shallowCopy()`
- Copies property values without recursion
- Model-type references point to original objects

**Source**: Template `impl/clone.vm`

## 11. Change Notification & Recording

### 11.1 Property Change Events
- Every setter fires `PropertyChangeEvent` (old value, new value)
- `VMFPropertyChangeSupport` manages listeners per object
- Listeners can be added recursively across object graph

### 11.2 Change Recording
- `obj.vmf().changes().start()` begins recording
- `obj.vmf().changes().all()` returns `VList<Change>` of recorded changes
- Each `Change` supports `undo()`

### 11.3 Transactions
- `obj.vmf().changes().startTransaction()` begins transaction
- `obj.vmf().changes().publishTransaction()` commits
- Transactions group changes for batch undo

### 11.4 Model Versioning
- `obj.vmf().changes().modelVersion()` returns version counter
- Incremented on each change

**Source**: `Changes.java`, `ChangesImpl.java`, `ChangeInternal.java`, `Transaction.java`

## 12. Object Graph Traversal

### 12.1 Iterators
- `obj.vmf().content().iterator()` - depth-first traversal
- Follows containment tree
- Three iteration strategies:
  - `UNIQUE_NODE` - visit each object instance exactly once (even if referenced multiple times)
  - `UNIQUE_PROPERTY` - visit every property exactly once (default)
  - `CONTAINMENT_TREE` - visit only containment graph, ignore cross-refs and non-contained refs
- All strategies prevent cyclic paths
- Only model-type properties are visited; primitives/externals are skipped
- `@PropertyOrder` indices determine traversal order of properties within a type

### 12.2 Streams
- `obj.vmf().content().stream()` - all objects in graph
- `obj.vmf().content().stream(MyType.class)` - type-filtered stream

### 12.3 Switch / Visitor Pattern
- Generated `SwitchFor<Model>Model` interface with `case<TypeName>()` methods
- Used for type-safe dispatching across model types

### 12.4 Traversal Listeners
- `ListenerFor<Model>Model` interface with `enter()` and `leave()` callbacks per type

**Source**: `VIterator.java`, `Content.java`, templates `vmf-model-switch-interface.vm`, `vmf-model-traversal-listener-interface.vm`

## 13. Reflection API

- `obj.vmf().reflect().properties()` - list all properties at runtime
- `obj.vmf().reflect().propertyByName("name")` - lookup property by name (returns `Optional<Property>`)
- `obj.vmf().reflect().type()` - type metadata
- `Type.type()` - static access to type metadata without instance
- `Type.reflect()` - static access to reflection without instance
- `Type.superTypes()` - list parent types in inheritance hierarchy
- Property metadata includes: name, type, annotations, read-only status, list type, model type
- **Per-property change listeners**: `property.addChangeListener(c -> ...)` - subscribe to individual property changes
- **Reflective access**: `property.get()` / `property.set(value)` - read/write values reflectively
- **Set/unset tracking**: `property.isSet()` returns true if explicitly set; `property.unset()` resets to default
- **Annotation lookup**: `property.annotationByKey("key")` returns `Optional<Annotation>`

**Source**: `Reflect.java`, `ReflectImpl.java`, template `reflection/reflection.vm`

## 14. Custom Annotations

- `@Annotation(key="myKey", value="myValue")` - repeatable
- Available at runtime via reflection API: `property.annotations()` or `property.annotationByKey("key")`
- Type-level: `obj.vmf().reflect().annotations()` or `obj.vmf().reflect().annotationByKey("key")`
- Stored as `AnnotationInfo` (key-value pairs)

**Source**: `Annotation.java` (the annotation), `AnnotationInfo.java`, `AnnotationImpl.java`

## 15. External Types

- `@ExternalType(pkgName="com.external.pkg")` on interface
- Tells VMF about types outside the model
- Only the FQN is used; no code generated for external types
- External type properties treated as opaque values

**Source**: `ExternalType.java`, `Model.java:87-89`

## 16. Sync Properties

- `@SyncWith(opposite="propName")` - synchronize property values
- Bidirectional value synchronization between properties

**Source**: `SyncWith.java`, `SyncInfo.java`, `Prop.java:440-472`, template `impl/sync.vm`

## 17. Documentation

- `@Doc("description")` on types and properties
- Inserted into generated Javadoc
- Accessible through model metadata at generation time

## 18. Serialization (Jackson Module)

Located in `jackson/`:

### 18.1 VMFJacksonModule
- Jackson module that registers serializers/deserializers for VMF types
- Supports JSON, XML, YAML via Jackson
- Handles polymorphic types
- Handles containment and references

### 18.2 JSON Schema Generation
- `VMFJsonSchemaGenerator` generates JSON Schema from VMF model types
- Used for validation and automatic visual editor generation

### 18.3 Type Utilities
- `VMFTypeUtils` provides type resolution helpers for Jackson integration

**Source**: `jackson/src/main/java/eu/mihosoft/vmf/jackson/`

## 19. Visual Editor (vmfedit)

Located in `vmfedit/`:
- Visual editor tooling for VMF models
- Likely generates UI from model definitions
- Separate concern from core migration

## Test Suite Coverage

The `test-suite/` directory contains integration tests organized by feature:

| Test Area | Package |
|---|---|
| Property order | `completepropertyordertest` |
| Delegation | `delegationtest` |
| No properties | `nopropertiestest` |
| Reflection + default values | `reflectiontest` |
| Basic properties | `test1` |
| Containment + inheritance | `test2` |
| Annotations | `vmftest/annotations` |
| Builders | `vmftest/builders` |

Additional test model definitions in `src/test/vmf/` follow the standard VMF source layout.
