# Generated Code Architecture

## Files Generated Per Model Type

For each interface `MyType` in the model, VMF generates the following (for a mutable, non-interface-only type):

| Generated File | Location | Purpose |
|---|---|---|
| `MyType.java` | `pkg/` | Public interface with getters, setters, `newInstance()`, `newBuilder()`, `type()` |
| `ReadOnlyMyType.java` | `pkg/` | Read-only interface (getters only), extends `VObject` |
| `MyTypeImpl.java` | `pkg/impl/` | Mutable implementation class (package-private) |
| `ReadOnlyMyTypeImpl.java` | `pkg/impl/` | Read-only wrapper delegating to `MyTypeImpl` |

For `@Immutable` types:
| Generated File | Location | Purpose |
|---|---|---|
| `MyType.java` | `pkg/` | Interface with getters only + `newBuilder()` |
| `MyTypeImpl.java` | `pkg/impl/` | Immutable implementation |

For `@InterfaceOnly` types:
| Generated File | Location | Purpose |
|---|---|---|
| `MyType.java` | `pkg/` | Interface only, no `newInstance()` |
| `__VMF_TYPE_MyType.java` | `pkg/impl/` | Internal type interface for raw property access |

## Model-Level Files

Per model (package), VMF also generates:

| File | Purpose |
|---|---|
| `SwitchFor<Model>Model.java` | Type-switch visitor interface with `case<TypeName>(T obj)` methods |
| `ReadOnlySwitchFor<Model>Model.java` | Read-only variant of the switch |
| `ListenerFor<Model>Model.java` | Traversal listener interface |
| `ReadOnlyListenerFor<Model>Model.java` | Read-only traversal listener |
| `<Model>Model__VMF_API.java` | Model-level API class |
| `impl/VCloneableInternal.java` | Internal cloning interface |

## Generated Interface Structure

```java
// For interface MyType:
public interface MyType extends VObject, Cloneable, Mutable {
    
    // Property getters
    String getName();
    int getValue();
    VList<Child> getChildren();
    
    // Property setters (not generated for @Immutable, @GetterOnly, or @Container)
    MyType setName(String name);
    MyType setValue(int value);
    
    // Delegated methods
    int computeSomething();
    
    // Static factory
    static MyType newInstance() { ... }
    
    // Static builder access
    static MyType.Builder newBuilder() { ... }
    
    // Type metadata
    static Type type() { ... }
    
    // Read-only conversion
    ReadOnlyMyType asReadOnly();
    
    // Cloning
    MyType clone();
    
    // VMF API access (inherited from VObject)
    // VMF vmf();
    
    // Nested Builder class
    public static class Builder implements eu.mihosoft.vmf.runtime.core.Builder {
        Builder withName(String name);
        Builder withValue(int value);
        MyType build();
        // append* methods for collection properties
        Builder appendChildren(Child... items);
        // Selective property transfer (works across inheritance hierarchy)
        Builder applyFrom(MyType source);  // Copy properties from instance to builder
        Builder applyTo(MyType target);    // Copy properties from builder to instance
    }
    
    // Nested Behavior interface (for delegation)
    interface Behavior { ... }
}
```

## Generated Implementation Structure

```java
// impl/MyTypeImpl.java (simplified)
class MyTypeImpl implements MyType, __VMF_TYPE_MyTypeImpl, VObjectInternalModifiable, VCloneableInternal {

    // Backing fields for properties
    private String __vmf_prop_name;
    private int __vmf_prop_value;
    private VList<Child> __vmf_prop_children;
    
    // Delegate instances (for @DelegateTo)
    private MyBehavior __vmf_delegate_0;
    
    // Property change support
    private VMFPropertyChangeSupport propertyChanges;
    
    // Constructor
    public MyTypeImpl() {
        // Initialize delegates
        // Set default values
    }
    
    // Getters with lazy initialization for collections
    @Override
    public VList<Child> getChildren() {
        if (__vmf_prop_children == null) {
            __vmf_prop_children = VList.newInstance(...);
            // Setup containment listeners
        }
        return __vmf_prop_children;
    }
    
    // Setters with:
    // - Change notification (fires PropertyChangeEvent)
    // - Containment management (remove from old parent, set new parent)
    // - Cross-reference management
    @Override
    public MyType setName(String name) {
        // fire property change
        // set value
        return this;
    }
    
    // VMF API implementation
    @Override
    public VMF vmf() { ... }
    
    // equals/hashCode/toString (strategy-dependent)
    // clone (deep copy with graph traversal)
    // Internal methods for property access by ID
    
    // _vmf_getPropertyValueById(int id)
    // _vmf_setPropertyValueById(int id, Object value)
    // _vmf_getPropertyNames()
    // _vmf_getPropertyTypes()
}
```

## Property Naming Convention

- Field: `__vmf_prop_<propertyName>`
- Delegate: `__vmf_delegate_<index>`
- Internal methods use `_vmf_` prefix

## Collection Properties

Collection properties use `VList<T>` (from vcollections), which is an observable list. The implementation:

1. Lazily initializes the VList on first access
2. For `@Contains` collections: adds list-change listeners that manage containment (set parent on add, clear parent on remove)
3. For `@Refers` collections: adds listeners that manage bidirectional references
4. Wraps in unmodifiable list for read-only interfaces

## Read-Only Wrappers

`ReadOnlyMyTypeImpl` wraps a `MyTypeImpl` instance:
- Delegates all getters to the wrapped instance
- Returns read-only versions of model-type properties
- Returns unmodifiable collections
- Throws on any mutation attempt
- `asModifiable()` returns a deep copy of the underlying mutable object (new independent instance)

## Cloning

Deep clone traverses the containment tree:
- Contained objects are recursively cloned
- Non-contained model references are re-linked to cloned counterparts if they exist in the same graph
- Immutable objects are shared (not cloned)
- External types are shallow-copied

## Property Change Events

Every setter fires a `PropertyChangeEvent` via `VMFPropertyChangeSupport`:
- Contains old and new values
- Listeners can be added per-object or recursively across the graph
- Used internally for undo/redo (change recording)

## Type IDs

Each `ModelType` gets a unique `typeId` (incremented by 2 to account for read-only variant). Each property within an implementation gets a `propId` (sequential index). These IDs enable efficient property access by index rather than name.
