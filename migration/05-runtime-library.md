# Runtime Library (`vmf-runtime`)

## Overview

The runtime library provides the base interfaces and internal implementations that generated code depends on. Located at `runtime/src/main/java/eu/mihosoft/vmf/runtime/core/`.

## Public API Interfaces

### `VObject` - Base Interface

All generated model types implement `VObject`:

```java
public interface VObject {
    VMF vmf();           // Access to VMF API
    VObject clone();     // Deep clone
    VObject asReadOnly(); // Read-only wrapper
}
```

### `VMF` - API Access Point

```java
public interface VMF {
    Content content();         // Object graph traversal, cloning, equals
    Changes changes();         // Change recording, undo/redo
    Reflect reflect();         // Runtime reflection
    <T extends VObject> Behavior<T> behavior();  // Behavior/delegation API
}
```

### `Content` - Object Graph Operations

```java
public interface Content {
    VIterator iterator();                              // Depth-first traversal
    VIterator iterator(VIterator.IterationStrategy s); // With strategy
    Stream<VObject> stream();                          // Stream traversal
    Stream<VObject> stream(VIterator.IterationStrategy s);
    <T extends VObject> Stream<T> stream(Class<T> type); // Type-filtered
    <T extends VObject> Stream<T> stream(Class<T> type, VIterator.IterationStrategy s);
    <T> T deepCopy();     // Deep clone
    <T> T shallowCopy();  // Shallow clone
    boolean equals(Object o); // VMF content-based equality
    int hashCode();           // VMF content-based hash
}
```

**Iteration strategies** (`VIterator.IterationStrategy`):
- `UNIQUE_PROPERTY` - visits each property once (default)
- `UNIQUE_NODE` - visits each node once
- Other strategies may exist

### `Changes` - Change Recording & Undo/Redo

```java
public interface Changes {
    Subscription addListener(ChangeListener l);              // Listen to changes
    Subscription addListener(ChangeListener l, boolean recursive);
    void start();              // Start recording
    void stop();               // Stop recording
    void startTransaction();   // Begin transaction
    void publishTransaction(); // Commit transaction
    VList<Change> all();       // All recorded changes
    VList<Transaction> transactions(); // All transactions
    void clear();              // Clear recorded changes
    ModelVersion modelVersion();
    boolean isModelVersioningEnabled();
}
```

### `Change` and `PropertyChange` - Change Events

```java
public interface Change {
    VObject object();           // The affected object
    String propertyName();      // Name of changed property
    ChangeType getType();       // PROPERTY or LIST
    long getTimestamp();         // Nanosecond timestamp
    boolean isUndoable();       // Whether undo() is possible
    void undo();                // Revert this specific change
    void apply(VObject target); // Apply change to another object
    
    // Discriminated access - exactly one is present:
    Optional<PropertyChange> propertyChange(); // For scalar property changes
    Optional<VListChangeEvent<Object>> listChange(); // For collection changes
    
    enum ChangeType { PROPERTY, LIST }
}

public interface PropertyChange {
    Object oldValue();
    Object newValue();
}
// VListChangeEvent<T> (from vcollections): added/removed elements, indices,
// toStringWithDetails() for verbose description
```

### `Reflect` - Runtime Reflection

```java
public interface Reflect {
    List<Property> properties();                      // All properties
    Optional<Property> propertyByName(String name);   // Lookup by name
    Type type();                                      // Type metadata
    List<Annotation> annotations();                   // Type-level annotations
    Optional<Annotation> annotationByKey(String key); // Annotation lookup by key
}
```

### `Property` - Property Metadata and Access

```java
public interface Property {
    String getName();
    Type getType();
    Object get();                       // Reflective getter
    void set(Object value);             // Reflective setter
    boolean isSet();                    // True if explicitly set (vs default value only)
    void unset();                       // Reset to default value, mark as unset
    boolean isReadOnly();
    boolean isModelType();
    boolean isListType();
    List<Annotation> annotations();
    Optional<Annotation> annotationByKey(String key); // Annotation lookup
    Subscription addChangeListener(ChangeListener l); // Per-property change listener
}
```

### `Type` - Type Metadata

```java
public interface Type {
    String getName();
    String getFullName();
    boolean isModelType();
    boolean isImmutable();
    boolean isInterfaceOnly();
    List<Type> superTypes();
    List<Property> properties();
    // ...
}
```

### `Builder` - Marker Interface

```java
public interface Builder {
    // Marker interface for generated builders
}
```

### `Behavior` and `DelegatedBehavior<T>`

```java
public interface DelegatedBehavior<T extends VObject> {
    void setCaller(T caller);
}

public abstract class DelegatedBehaviorBase<T extends VObject> implements DelegatedBehavior<T> {
    protected T caller;
    
    @Override
    public void setCaller(T caller) {
        this.caller = caller;
    }
}
```

### `Mutable` and `ReadOnly` - Marker Interfaces

```java
public interface Mutable extends VObject { }
public interface ReadOnly extends VObject { }
public interface Immutable extends VObject { }
public interface InterfaceOnly { }
```

### Supporting Types

| Interface | Purpose |
|---|---|
| `TraversalListener` | Callback for entering/leaving nodes during traversal |
| `VIterator` | Iterator for object graph traversal |
| `Annotation` | Runtime annotation (key-value pair) |
| `Method` | Method metadata for reflection |
| `ModelVersion` | Version counter for change tracking |
| `Transaction` | Group of changes that can be undone together |
| `ChangeListener` | Functional interface for change notifications |
| `VObjects` | Utility methods for VObject operations |

## Internal Implementation Classes

Located in `runtime/core/internal/`:

| Class | Purpose |
|---|---|
| `VObjectInternal` | Internal interface for VMF object internals |
| `VObjectInternalModifiable` | Extends `VObjectInternal` with mutation support |
| `VMFPropertyChangeSupport` | Property change event management (similar to Java Beans PropertyChangeSupport) |
| `ChangesImpl` | Implementation of `Changes` interface |
| `ChangeInternal` | Internal change representation |
| `PropChangeImpl` | Property change implementation |
| `ListChangeImpl` | List change implementation |
| `ReflectImpl` | Reflection API implementation |
| `AnnotationImpl` | Annotation implementation |
| `ModelVersionImpl` | Model version counter |

## External Dependency: VCollections

The `VList<T>` type from `eu.mihosoft.vcollections` is central:

```java
public interface VList<T> extends List<T>, Observable {
    static <T> VList<T> newInstance(List<T> backingList);
    // Observable list with change events
    // Supports add/remove listeners
}
```

`VList` fires change events when elements are added, removed, or replaced. VMF uses these events for:
- Containment management (auto-set parent on add, auto-clear on remove)
- Cross-reference management
- Change recording

## External Dependency: vjavax.observer

Provides the `Subscription` interface for event listener management:

```java
public interface Subscription {
    void unsubscribe();
}
```

## Model Diff

`runtime/core/diff/ModelDiff.java` provides model comparison utilities.
