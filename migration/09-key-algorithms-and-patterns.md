# Key Algorithms and Patterns

## 1. Containment Management

### Single-Valued Containment Setter

When setting a contained property (e.g., `parent.setChild(newChild)`):

```
1. If newChild == oldChild, return (no-op)
2. If oldChild != null:
   a. Clear oldChild's container reference (oldChild.__vmf_prop_parent = null)
3. If newChild != null:
   a. If newChild already has a parent:
      - Remove newChild from its current parent's collection/property
   b. Set newChild's container reference (newChild.__vmf_prop_parent = this)
4. Set this.__vmf_prop_child = newChild
5. Fire property change event
```

### Collection-Based Containment

When using `@Contains` on a collection (e.g., `Parent.getChildren()`):

The VList is lazily initialized with a change listener:
```
On element ADDED to list:
  1. If element already has a parent:
     a. Remove element from its current parent
  2. Set element's container reference to this parent
  
On element REMOVED from list:
  1. Clear element's container reference (set to null)
```

### Container Property (Read-Only)

`@Container` properties are read-only from the user's perspective. The container is set automatically by the containment management logic above (via internal setter or reflection).

## 2. Cross-Reference Management

For `@Refers` bidirectional references (e.g., Book.authors <-> Writer.books):

### Single-Valued Cross-Reference Setter

```
1. If newValue == oldValue, return (no-op)
2. If oldValue != null:
   a. Remove this from oldValue's opposite collection/property
3. If newValue != null:
   a. Add this to newValue's opposite collection/property
4. Set the property value
5. Fire property change event
```

**Important**: Must prevent infinite recursion when updating opposites. VMF uses flags or checks to avoid re-entrant updates.

### Collection Cross-Reference

Similar to containment but without ownership semantics:
```
On element ADDED:
  1. Add this to element's opposite collection/property (if not already present)

On element REMOVED:
  1. Remove this from element's opposite collection/property
```

## 3. Deep Clone Algorithm

Located in `impl/clone.vm` template:

```
deepClone(original, identityMap):
  1. If original is in identityMap, return mapped clone (handles cycles)
  2. Create new instance of same type
  3. Add mapping: identityMap[original] = clone
  4. For each property:
     a. If IMMUTABLE type: share reference (don't clone)
     b. If CONTAINED model type (single): recursively deepClone
     c. If CONTAINED collection: clone each element, add to clone's collection
     d. If NON-CONTAINED model type: 
        - If original reference is in identityMap, use mapped clone
        - Otherwise, keep original reference (may re-link later)
     e. If PRIMITIVE/EXTERNAL type: copy value directly
  5. Re-link non-contained cross-references within the cloned graph
  6. Return clone
```

## 4. Equals Algorithm

Located in `impl/equals.vm`:

### CONTAINMENT_AND_EXTERNAL Strategy

```
equals(a, b, visitedSet):
  1. If a == b (identity), return true
  2. If b is null or different type, return false
  3. If a is in visitedSet, return true (cycle protection)
  4. Add a to visitedSet
  5. For each property NOT marked @IgnoreEquals and NOT a @Container:
     a. If property is a contained model type:
        - Recursively compare with equals(a.prop, b.prop, visitedSet)
     b. If property is external/primitive:
        - Compare with Objects.equals(a.prop, b.prop)
     c. If property is a non-contained model type reference:
        - SKIP (not included in CONTAINMENT_AND_EXTERNAL)
  6. Return true if all comparisons passed
```

### HashCode (matching)

```
hashCode(obj, visitedSet):
  1. If obj is in visitedSet, return 0 (cycle protection)
  2. Add obj to visitedSet
  3. Combine hash of each included property
```

## 5. Object Graph Traversal

The generated walker implementation (from `vmf-model-walker-implementation.vm`) traverses the containment tree:

```
traverse(root, listener):
  1. Call listener.enter(root)
  2. For each property of root:
     a. If property is a contained model type:
        - traverse(propertyValue, listener)
     b. If property is a contained collection:
        - For each element: traverse(element, listener)
  3. Call listener.leave(root)
```

Iteration strategies control visitation:
- **UNIQUE_PROPERTY**: Visit each property path once
- **UNIQUE_NODE**: Visit each object instance once (even if reachable via multiple paths)

## 6. Change Recording

The `ChangesImpl` class records property changes:

```
onPropertyChange(event):
  1. If not recording, ignore
  2. Create Change from event (property name, old value, new value, target object)
  3. Add to changes list
  4. If in transaction, add to current transaction
  5. Notify change listeners
```

### Undo

```
change.undo():
  1. Set property back to oldValue on the original object
  2. This triggers a new change event (which may or may not be recorded)
```

### Transactions

```
startTransaction(): mark transaction boundary
publishTransaction(): package all changes since boundary into a Transaction
```

## 7. Builder Pattern

Generated builders use a fluent API:

```
Builder:
  - Fields mirror the type's properties
  - with<PropertyName>(value) methods for each property
  - append<PropertyName>(items...) for collection properties
  - build():
    1. Create new instance
    2. Set all properties from builder fields
    3. Return instance
```

For `@Immutable` types, the builder is the only way to set properties.

## 8. Read-Only Wrapper Pattern

```
ReadOnlyMyTypeImpl wraps MyTypeImpl:
  - Stores reference to mutable instance
  - Getters:
    - Primitive/external: delegate directly
    - Model type: return readOnlyVersion of the referenced object
    - Collection: return unmodifiable view with read-only element wrappers
  - No setters exposed
  - clone(): returns mutable clone (not read-only)
```

## 9. Reflection Implementation

Generated at compile time in `reflection/reflection.vm`:

```
Properties are stored as a static list:
  - Each Property knows its name, type, annotations, getter/setter lambdas
  - Property.get() calls the generated getter
  - Property.set() calls the generated setter
  
Type metadata:
  - Stored as static final fields
  - Includes supertype chain, property list, annotations
```

## 10. Property Access by ID

For performance, generated code supports accessing properties by numeric ID:

```java
Object _vmf_getPropertyValueById(int id) {
    switch(id) {
        case 0: return __vmf_prop_name;
        case 1: return __vmf_prop_value;
        // ...
    }
}

void _vmf_setPropertyValueById(int id, Object value) {
    switch(id) {
        case 0: setName((String) value); break;
        case 1: setValue((int) value); break;
        // ...
    }
}
```

This is used by the reflection API and change recording system for efficient property access.

## 11. Opposite Property Setting via Reflection

When VMF needs to set an opposite property without triggering the bidirectional update loop, it uses internal methods that bypass the public setter:

```
_vmf_set<PropertyName>_raw(value)  // Sets without triggering opposite management
```

Or uses a flag to prevent re-entrant updates:
```
if (!__vmf_updating_opposite) {
    __vmf_updating_opposite = true;
    try { opposite.setMyProp(this); }
    finally { __vmf_updating_opposite = false; }
}
```
