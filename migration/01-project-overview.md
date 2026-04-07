# VMF Project Overview

## What VMF Is

VMF (Visual Modeling Framework) is a lightweight Java code-generation framework. Users define **annotated Java interfaces** in a special `vmfmodel` subpackage, and VMF generates complete, production-ready implementation classes including:

- Property getters/setters with change notification
- Builder pattern API
- Deep and shallow cloning
- equals() / hashCode() (configurable strategies)
- toString()
- Read-only wrapper types
- Immutable types
- Object graph traversal (iterators and streams)
- Containment tree management (parent/child ownership)
- Cross-references (bidirectional linking)
- Undo/redo via change recording and transactions
- Reflection API
- Delegation (custom behavior injection)
- Jackson serialization (JSON, XML, YAML) via separate module
- JSON Schema generation

## Repository Structure

```
VMF/
  core/           -> vmf (code generator + annotations)
  runtime/        -> vmf-runtime (runtime library that generated code depends on)
  gradle-plugin/  -> Gradle plugin that integrates VMF into build pipeline
  jackson/        -> vmf-jackson (serialization support via Jackson)
  maven-plugin/   -> Maven plugin (alternative to Gradle)
  test-suite/     -> Comprehensive integration tests
  vmfedit/        -> Visual editor tooling
```

### Module Dependency Graph

```
gradle-plugin --> core (vmf)
                    |
                    v
               runtime (vmf-runtime) <-- generated user code
                    |
                    v
               vcollections (eu.mihosoft.vcollections)
```

- **core** depends on: velocity-legacy (template engine), classgraph (classpath scanning), jcompiler (in-memory Java compilation)
- **runtime** depends on: vcollections (observable list implementation `VList`)
- **jackson** depends on: runtime + Jackson libraries

## How It Works (Pipeline)

1. **User writes model interfaces** in `src/main/vmf/` (package must end with `.vmfmodel`)
2. **Gradle plugin** compiles these interfaces to `.class` files, then invokes `VMF.generate()`
3. **`VMF.generate()`** uses ClassGraph to scan for interfaces in the package, loads them via classloader
4. **`Model` class** performs multi-pass analysis of the interfaces:
   - Pass 0: Separate external types from model interfaces; create `ModelType` for each interface
   - Pass 1: Resolve containment relationships (`@Contains` / `@Container`)
   - Pass 2: Resolve cross-references (`@Refers`)
   - Pass 3: Resolve inheritance (`implements` on interfaces)
   - Pass 4: Resolve sync info and property IDs
   - Pass 5: Collect implementation properties, imports, and delegates
   - Pass 6: Initialize interface properties and all inherited types
   - Pass 7: Validation (immutability constraints, delegation consistency, etc.)
5. **`CodeGenerator`** uses Apache Velocity templates to generate Java source files for each `ModelType`:
   - `interface.vm` -> Public interface (e.g., `MyType.java`)
   - `read-only-interface.vm` -> Read-only interface (e.g., `ReadOnlyMyType.java`)
   - `implementation.vm` -> Mutable implementation (e.g., `impl/MyTypeImpl.java`)
   - `read-only-implementation.vm` -> Read-only wrapper implementation
   - `immutable-interface.vm` / `immutable-implementation.vm` -> For `@Immutable` types
   - Various model-level files: switch interfaces, listener interfaces, walker, cloneable internal

## Version & Build

- Java 11+ required (targets Java 11)
- Gradle 9.0.0 wrapper
- License: Apache 2.0
- Author: Michael Hoffer (info@michaelhoffer.de), Goethe Center for Scientific Computing

## Key External Dependencies

| Dependency | Purpose | C# Equivalent Consideration |
|---|---|---|
| Apache Velocity (legacy fork `eu.mihosoft.ext.velocity.legacy`) | Template engine for code generation | Scriban, T4, or Razor templates |
| ClassGraph (`io.github.classgraph`) | Classpath scanning to find model interfaces | Roslyn source generators or reflection |
| JCompiler (`eu.mihosoft.jcompiler`) | In-memory Java compilation of model interfaces | Roslyn CSharpCompilation |
| VCollections (`eu.mihosoft.vcollections`) | Observable list (`VList`) with change events | ObservableCollection<T> |
| vjavax.observer | Subscription pattern for event listeners | IDisposable / event unsubscription |
