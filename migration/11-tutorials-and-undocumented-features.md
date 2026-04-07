# Tutorials Reference & Previously Undocumented Features

## Tutorial Catalog

Source: https://github.com/miho/VMF-Tutorials (cloned to `/home/aheusel/source/VMF-Tutorials`)

| # | Title | Key Features Demonstrated |
|---|-------|--------------------------|
| 01 | Defining your First Model | Basic model: `newInstance()`, getters/setters, `toString()` |
| 02 | Change Notification API | `vmf().changes().addListener()`, `PropertyChange` vs `ListChange`, `evt.propertyChange()` / `evt.listChange()` |
| 03 | Containment References | `@Contains`/`@Container`, auto-parent management, reparenting |
| 03b | Cross References | `@Refers` bidirectional, collection cross-refs, builder with collections |
| 04 | Undo/Redo | `changes().start()`, change recording, `Change.undo()`, undo via reversed change list, `deepCopy()` for checkpoint |
| 05 | Builder API | `newBuilder().with*().build()`, multi-interface inheritance (`WithName`, `WithId`), **`applyFrom()` / `applyTo()`** |
| 06 | Object Graph Traversal & Property Order | `@PropertyOrder`, `stream(Type.class)`, **three iteration strategies**, property order affects traversal |
| 07 | Immutable Objects & ReadOnly API | `@Immutable`, builder-only init, `asReadOnly()`, **`asModifiable()`** |
| 08 | Custom Behavior & Delegation | `@DelegateTo`, `DelegatedBehavior<T>`, **`on<TypeName>Instantiated()` callback** |
| 09 | Default Values | `@DefaultValue`, **`Property.isSet()`**, **`Property.unset()`** |
| 10 | Equals & HashCode | `@VMFEquals`, `@IgnoreEquals`, `vmf().content().equals()` |
| 11 | Annotation Support | `@Annotation(key,value)`, **`Property.annotationByKey()`**, filtering by annotation |
| 12 | Cloning (Deep & Shallow) | `deepCopy()` vs `shallowCopy()`, shallow shares refs, delegation of `toString()` |
| 13 | Reflection API | `Type.type()`, `Type.reflect()`, `superTypes()`, **`Property.addChangeListener()`**, `Property.set()`, type metadata |
| 14 | Custom Documentation | `@Doc` on types and properties, FSM domain model example |
| 15 | External Types | `@ExternalType(pkgName="...")`, using types not on generator classpath |
| 16 | Maven Plugin | Maven plugin configuration (`vmf` and `vmf-test` goals) |

---

## Features Not Previously Documented

### 1. Builder `applyFrom()` / `applyTo()` Pattern

**Tutorial 05** demonstrates that builders of parent interfaces can selectively transfer property state between objects:

```java
// Node extends WithName, WithId
Node node1 = Node.newBuilder().withName("my node").withId(3).build();
Node node2 = Node.newInstance();

// Copy only WithName properties from node1 to node2
WithName.newBuilder().applyFrom(node1).applyTo(node2);
// node2.getName() == "my node", but node2.getId() is still default
```

**Key insight**: Each generated builder has `applyFrom(TypeInstance)` and `applyTo(TypeInstance)` methods. Using a parent interface's builder allows partial property copying - only properties defined in that parent interface are transferred.

**C# migration note**: Generate `ApplyFrom()`/`ApplyTo()` methods on each builder. This enables selective property copying via interface-specific builders.

### 2. Property Set/Unset State Tracking

**Tutorial 09** reveals that VMF tracks whether each property has been explicitly set:

```java
ObjectWithDefaultValues obj = ObjectWithDefaultValues.newInstance();

// Check if property has been explicitly set (vs. has default value)
boolean isSet = obj.vmf().reflect().propertyByName("name")
    .map(p -> p.isSet()).orElse(false);
// isSet == false (only has default value, never explicitly set)

obj.setName("new value");
// isSet == true

// Reset to default
obj.vmf().reflect().propertyByName("name").ifPresent(p -> p.unset());
// isSet == false again, value reverts to @DefaultValue
```

**Runtime API methods on `Property`:**
- `boolean isSet()` - true if property was explicitly set (even if set to the default value)
- `void unset()` - resets to default value and marks as unset

**C# migration note**: Track set state via a `BitArray` or `HashSet<int>` of property IDs. `unset()` maps to resetting the value and clearing the flag.

### 3. `asModifiable()` on Read-Only Objects

**Tutorial 07** shows that read-only wrappers can produce modifiable deep copies:

```java
MutableObject mutable = MutableObject.newInstance();
mutable.setValue(12);

ReadOnlyMutableObject readOnly = mutable.asReadOnly();

// Creates a deep copy of the underlying mutable object
MutableObject mutableCopy = readOnly.asModifiable();
// mutableCopy is a new, independent mutable instance
```

**C# migration note**: Generated `ReadOnly<Type>` needs an `AsModifiable()` method that returns `deepCopy()` of the wrapped mutable instance.

### 4. Per-Property Change Listeners (Reflection API)

**Tutorial 13** shows adding change listeners to individual properties via reflection:

```java
a.vmf().reflect().propertyByName("name").ifPresent(p -> {
    p.addChangeListener(c -> {
        System.out.println("Changed: " + p.get());
    });
    p.set("my new name");  // Triggers the listener
});
```

**Runtime API methods on `Property`:**
- `Subscription addChangeListener(ChangeListener listener)` - per-property listener
- `void set(Object value)` - reflective setter
- `Object get()` - reflective getter

**C# migration note**: Each generated property needs individual change listener support, not just object-level `INotifyPropertyChanged`.

### 5. Change Event Discrimination (PropertyChange vs ListChange)

**Tutorials 02, 04** demonstrate that change events have two variants:

```java
root.vmf().changes().addListener((evt) -> {
    System.out.println("Property: " + evt.propertyName());
    
    if (evt.propertyChange().isPresent()) {
        // Single property changed
        PropertyChange pc = evt.propertyChange().get();
        System.out.println("Old: " + pc.oldValue() + " -> New: " + pc.newValue());
    } else if (evt.listChange().isPresent()) {
        // Collection changed (add/remove/replace)
        VListChangeEvent<Object> lc = evt.listChange().get();
        System.out.println(lc.toStringWithDetails());
    }
});
```

**`Change` interface details:**
- `VObject object()` - the affected object
- `String propertyName()` - name of changed property
- `Change.ChangeType getType()` - `PROPERTY` or `LIST`
- `Optional<PropertyChange> propertyChange()` - present for scalar changes
- `Optional<VListChangeEvent<Object>> listChange()` - present for collection changes
- `long getTimestamp()` - nanosecond timestamp
- `boolean isUndoable()` - whether `undo()` is possible
- `void undo()` - revert this change
- `void apply(VObject target)` - apply to another object

**`VListChangeEvent<T>` (from vcollections):**
- Contains added/removed elements, indices
- `toStringWithDetails()` - verbose description of change

**C# migration note**: Model this as a discriminated union or two subclasses of `IChange` (property change vs collection change).

### 6. Annotation Query on Properties

**Tutorial 11** shows the `annotationByKey()` API for querying property annotations:

```java
// Model definition:
// @Annotation(key="api", value="input")
// Node getA();

Predicate<Property> isInput = (p) -> {
    return p.annotationByKey("api")
            .map(ann -> "input".equals(ann.getValue()))
            .orElse(false);
};

// Filter properties by annotation
node.vmf().reflect().properties().stream()
    .filter(isInput)
    .forEach(p -> System.out.println("Input: " + p.getName()));
```

**Runtime API on `Property`:**
- `Optional<Annotation> annotationByKey(String key)` - single annotation lookup
- `List<Annotation> annotations()` - all annotations

**Runtime API on type-level `Reflect`:**
- `List<Annotation> annotations()` - type-level annotations
- `Optional<Annotation> annotationByKey(String key)` - type-level annotation lookup

### 7. Type-Level `@DelegateTo` Instantiation Callback

**Tutorial 08** shows that when `@DelegateTo` is used on the interface itself (type-level), the delegate receives an instantiation callback:

```java
// In the delegation class:
public class CustomBehavior implements DelegatedBehavior<ObjectWithCustomBehavior> {
    private ObjectWithCustomBehavior caller;

    @Override
    public void setCaller(ObjectWithCustomBehavior caller) {
        this.caller = caller;
    }

    // This callback is automatically called when the object is created
    public void onObjectWithCustomBehaviorInstantiated() {
        System.out.println("object instantiated");
    }
    
    public int computeSum() {
        return caller.getA() + caller.getB();
    }
}
```

**Convention**: The callback method name is `on<TypeName>Instantiated()`. It is called in the generated constructor after `setCaller()`.

### 8. Iteration Strategies (Complete Documentation)

**Tutorial 06** documents three strategies:

| Strategy | Behavior |
|---|---|
| `UNIQUE_NODE` | Visit each object instance exactly once. If the same object is referenced from multiple properties, only the first encounter triggers a visit. |
| `UNIQUE_PROPERTY` | Visit every property exactly once, even if multiple properties reference the same object instance. (Default) |
| `CONTAINMENT_TREE` | Visit only the containment graph (objects connected via `@Contains`/`@Container`), ignoring cross-references and non-contained references. |

All strategies prevent cyclic paths.

```java
// Stream API
root.vmf().content().stream(VIterator.IterationStrategy.UNIQUE_NODE).forEach(obj -> { ... });

// Iterator API
VIterator it = root.vmf().content().iterator(VIterator.IterationStrategy.CONTAINMENT_TREE);
```

### 9. Undo/Redo Pattern (Detailed)

**Tutorial 04** demonstrates the complete undo pattern:

```java
// Start recording
root.vmf().changes().start();

// Make changes...
root.setName("#1");
root.getChildren().add(child1);
child1.setName("#2");

// Create checkpoint via deep copy
Node checkpoint = root.vmf().content().deepCopy();

// Get all recorded changes
List<Change> changes = new ArrayList<>(root.vmf().changes().all());

// Undo all changes in reverse order
Collections.reverse(changes);
changes.forEach(Change::undo);

// root is now back to its initial state
```

**Key insight**: Undo is not automatic - the user must reverse the change list manually. `Change.undo()` reverts one change at a time. Combined with `deepCopy()` this enables a full checkpoint/restore pattern.

### 10. Shallow Copy Semantics (Detailed)

**Tutorial 12** clarifies shallow vs deep copy:

```java
Store store = Store.newBuilder().withId("my store").build();
Item item1 = Item.newBuilder().withId("item 1").build();
store.getItems().add(item1);

Store deepCopy = store.vmf().content().deepCopy();
Store shallowCopy = store.vmf().content().shallowCopy();

// Deep copy: items are independent copies
deepCopy.getItems().get(0).setId("changed");
// store.getItems().get(0).getId() == "item 1" (unchanged)

// Shallow copy: items are SHARED references
shallowCopy.getItems().get(0).setId("changed");
// store.getItems().get(0).getId() == "changed" (ALSO changed!)
```

**Key insight**: Shallow copy copies the collection container (new VList) but references the same child objects. Deep copy recursively clones the containment tree.

### 11. Property Order Affects Traversal Order

**Tutorial 06** demonstrates that `@PropertyOrder` indices determine the order in which model-type properties are visited during `stream()` traversal:

```java
// child3 has index 2, child2 has index 3, child1 has index 4
// Traversal visits: root -> child3 -> child2 -> child1
root.vmf().content().stream(Node.class).forEach(
    node -> System.out.println(node.getName())
);
```

Only model-type properties are visited during graph traversal. Primitive and external-type properties are skipped.

### 12. `@VMFEquals` Without Parameters

**Tutorial 12** shows `@VMFEquals` used without parameters, which defaults to `CONTAINMENT_AND_EXTERNAL`:

```java
@VMFEquals
interface Store { ... }
```

This is distinct from `@VMFEquals(EqualsType.ALL)` or the model-level `@VMFModel(equality=...)`.
