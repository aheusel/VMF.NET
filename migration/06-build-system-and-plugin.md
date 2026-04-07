# Build System & Plugin Architecture

## Gradle Plugin (`eu.mihosoft.vmf`)

File: `gradle-plugin/src/main/groovy/eu/mihosoft/vmf/gradle/plugin/VMFPlugin.groovy`

### What It Does

1. Applies the `java` plugin
2. Optionally applies IntelliJ IDEA plugin for IDE source root management
3. For each source set (main, test, etc.):
   - Creates a `vmfCompile` configuration with `vmf` and `vmf-runtime` dependencies
   - Adds a `vmf` source directory set (`src/<sourceSet>/vmf/`) with `**/*.java` filter
   - Creates a `vmfCompileModelDef` task (JavaCompile) to compile model interfaces
   - Creates a `vmfGenModelSources` task (CompileVMFTask) to run VMF code generation
   - Output goes to `build/generated-src/vmf-<sourceSet>/`
   - Wires tasks: `vmfCompileModelDef` -> `vmfGenModelSources` -> `compileJava`
   - Creates a `vmfClean` task to delete generated files
4. Adds `vmf-runtime` as an `implementation` dependency

### Source Layout

```
src/
  main/
    vmf/        <- VMF model definitions (Java interfaces)
      eu/mihosoft/myapp/vmfmodel/
        MyModel.java
    java/       <- Hand-written code + delegation classes
  test/
    vmf/        <- Test model definitions
    java/       <- Test code
build/
  generated-src/
    vmf-main/   <- Generated code (auto-added to Java source set)
  vmf-modeldef-compiled/
    vmf-main/   <- Compiled model .class files (intermediate)
```

### CompileVMFTask

The task:
1. Compiles model interfaces from the `vmf` source set to `.class` files
2. Creates a `URLClassLoader` with the compiled classes + vmf classpath
3. Loads `eu.mihosoft.vmf.VMF` class via the classloader
4. For each changed model package, calls `VMF.generate(outputFolder, classLoader, packageName)`

### Plugin Version Management

The VMF version is embedded in a generated `Constants.groovy` file at plugin build time.

## Maven Plugin

Located at `maven-plugin/`. Provides equivalent functionality for Maven builds.

## Usage Example (build.gradle)

```groovy
plugins {
    id "eu.mihosoft.vmf" version "0.2.9.4"
}

// Optional: IntelliJ integration
buildscript {
    ext.vmfPluginIntelliJIntegration = true
}
```

## C# Migration Considerations for Build System

In C#, the equivalent of the Gradle plugin + code generator pattern would be a **Roslyn Source Generator**:

| VMF Java | C# Equivalent |
|---|---|
| Gradle plugin | NuGet package with MSBuild targets, or Roslyn Source Generator |
| `src/main/vmf/` source set | Additional `<Compile>` items or `<AdditionalFiles>` |
| JavaCompile of model interfaces | Roslyn compilation of model interfaces |
| Velocity template engine | T4 templates, Scriban, or direct string building |
| ClassGraph scanning | Roslyn `INamedTypeSymbol` analysis |
| JCompiler (in-memory compilation) | `CSharpCompilation` API |
| `vmfGenModelSources` task | `ISourceGenerator.Execute()` or MSBuild task |

A Roslyn Source Generator approach would be the most natural C# equivalent, as it:
- Runs at compile time (like VMF's Gradle task)
- Has full access to the compilation model (like VMF's reflection-based analysis)
- Generates source that participates in the compilation
- Integrates with IDEs natively (IntelliSense, etc.)
