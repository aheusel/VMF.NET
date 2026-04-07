# VMF Model Definition API (Input Format)

## Overview

Users define models as **Java interfaces** placed in a package ending with `.vmfmodel`. Properties are defined as getter methods following JavaBean conventions. Annotations control behavior.

## Package Convention

```java
package eu.mihosoft.myapp.vmfmodel;  // MUST end with .vmfmodel
```

The generated code is placed in the parent package (`eu.mihosoft.myapp`), with implementations in a `.impl` subpackage.

## Property Declaration

Properties are declared as getter methods. The return type determines the property type.

```java
interface MyModel {
    // Primitive property
    int getValue();
    
    // String property
    String getName();
    
    // Boolean property (uses "is" prefix)
    boolean isActive();
    
    // Model-type property (reference to another VMF type)
    OtherModel getOther();
    
    // Collection property (List<T> or T[])
    MyItem[] getItems();         // array syntax
    List<MyItem> getItems();     // List syntax (equivalent)
}
```

**Property name derivation**: `getValue()` -> property name `value`, `isActive()` -> `active`

**Only `java.util.List<T>` is supported** as a collection type. Arrays (`T[]`) are automatically converted to `VList<T>`.

## Complete Annotation Reference

### Type-Level Annotations

| Annotation | Target | Purpose |
|---|---|---|
| `@VMFModel(equality=...)` | One interface per model | Model-wide configuration. Sets default equals strategy. |
| `@Immutable` | Interface | Type is immutable - only getters, no setters, state set via builder only. Only allows immutable property types. |
| `@InterfaceOnly` | Interface | Only generates an interface, no implementation class. Cannot be instantiated. Used as abstract base types. |
| `@ExternalType(pkgName="...")` | Interface | Declares a type that exists outside the model. VMF uses its FQN but does not generate code for it. |
| `@VMFEquals(EqualsType.ALL\|CONTAINMENT_AND_EXTERNAL\|INSTANCE)` | Interface | Per-type override of equals/hashCode strategy. |
| `@Doc("...")` | Interface or Method | Custom documentation string added to generated Javadoc. |
| `@Annotation(key="...", value="...")` | Interface or Method | Repeatable. Adds custom key-value annotations accessible via reflection API. |
| `@DelegateTo(className="...")` | Interface (type-level) | Constructor delegation - delegate class called on object creation. |

### Property-Level Annotations

| Annotation | Target | Purpose |
|---|---|---|
| `@Contains(opposite="propName")` | Getter method | This property **contains** (owns) the referenced object(s). Establishes parent-child containment. Opposite is optional. |
| `@Container(opposite="propName")` | Getter method | This property references the **container** (parent). Opposite points to the `@Contains` property. Makes property read-only. |
| `@Refers(opposite="propName")` | Getter method | Bidirectional cross-reference (not containment). Both sides must declare `@Refers`. |
| `@DefaultValue("expression")` | Getter method | Default value expression (Java code literal, e.g., `"\"hello\""`, `"42"`, `"new ArrayList<>()"`) |
| `@Required` | Getter method | Property is required (for validation and builder). |
| `@GetterOnly` | Getter method | Only generate getter, no setter. Only valid on `@InterfaceOnly` or `@Immutable` types. |
| `@IgnoreEquals` | Getter method | Exclude this property from equals/hashCode computation. |
| `@IgnoreToString` | Getter method | Exclude this property from toString output. |
| `@PropertyOrder(index=N)` | Getter method | Custom ordering of properties (all or none must be annotated). |
| `@DelegateTo(className="...")` | Getter method (actually on non-getter methods) | Delegate custom method calls to specified class. |
| `@SyncWith(opposite="propName")` | Getter method | Synchronize property values bidirectionally. |

### Custom Methods (Delegation)

Non-getter, non-setter methods on interfaces become **delegated methods**:

```java
interface Calculator {
    int getA();
    int getB();
    
    @DelegateTo(className="com.example.CalcBehavior")
    int computeSum();
}
```

The delegation class must implement `DelegatedBehavior<T>`:

```java
public class CalcBehavior implements DelegatedBehavior<Calculator> {
    private Calculator caller;
    
    @Override
    public void setCaller(Calculator caller) {
        this.caller = caller;
    }
    
    public int computeSum() {
        return caller.getA() + caller.getB();
    }
}
```

## Containment Model

Containment defines a tree structure (each object has at most one parent):

```java
interface Parent {
    @Contains(opposite = "parent")
    Child[] getChildren();
}

interface Child {
    @Container(opposite = "children")
    Parent getParent();
}
```

**Key semantics**:
- An object can only be contained in one parent at a time
- Setting a new parent automatically removes from old parent
- `@Contains` without opposite is allowed (container cannot navigate back)
- `@Container` without opposite means the property is read-only with no explicit containment link

## Inheritance

Interfaces can extend other model interfaces:

```java
@InterfaceOnly
interface Named {
    @GetterOnly
    String getName();
}

interface Person extends Named {
    int getAge();
}

@Immutable
interface ImmutablePerson extends Named {
    int getAge();
}
```

**Constraints**:
- Mutable types cannot extend `@Immutable` types
- `@Immutable` types can only extend other `@Immutable` or `@InterfaceOnly` (with getters only) types
- All interfaces must be in the same `vmfmodel` package

## Equals Strategies

Configured via `@VMFModel` or `@VMFEquals`:

| Strategy | Behavior |
|---|---|
| `INSTANCE` (default) | Java identity-based (`==`) |
| `CONTAINMENT_AND_EXTERNAL` | Compares contained and external (non-model) properties only |
| `ALL` | Compares all properties |

Properties annotated with `@IgnoreEquals` are always excluded. Container properties are always excluded.
