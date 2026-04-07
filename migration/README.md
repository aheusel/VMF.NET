# VMF Java to C# Migration Documentation

This folder contains comprehensive documentation for migrating the VMF (Visual Modeling Framework) from Java to C#. These documents are designed to be consumed by an AI assistant planning and executing the migration.

## Documents

| # | Document | Purpose |
|---|---|---|
| 01 | [Project Overview](01-project-overview.md) | High-level architecture, module structure, dependencies, pipeline |
| 02 | [Model Definition API](02-model-definition-api.md) | Complete input format: annotations, property types, containment, inheritance |
| 03 | [Generated Code Architecture](03-generated-code-architecture.md) | What files are generated, their structure, naming conventions |
| 04 | [Code Generator Internals](04-code-generator-internals.md) | Multi-pass model analysis, Velocity templates, code generation flow |
| 05 | [Runtime Library](05-runtime-library.md) | All runtime interfaces and their contracts (VObject, VMF, Content, Changes, etc.) |
| 06 | [Build System & Plugin](06-build-system-and-plugin.md) | Gradle plugin architecture, source layout, build pipeline |
| 07 | [Java to C# Mapping](07-java-to-csharp-mapping.md) | Detailed type/API/pattern mappings from Java to C# |
| 08 | [Migration Strategy](08-migration-strategy.md) | Phased migration plan, risks, effort estimates |
| 09 | [Key Algorithms & Patterns](09-key-algorithms-and-patterns.md) | Core algorithms: containment management, cloning, equals, traversal, change recording |
| 10 | [Feature Catalog](10-feature-catalog.md) | Complete feature list with source code references |
| 11 | [Tutorials & Undocumented Features](11-tutorials-and-undocumented-features.md) | Tutorial catalog + features discovered from tutorials not in core docs |

## Reading Order

For migration planning, read in order: 01 -> 02 -> 03 -> 07 -> 08 -> then reference others as needed.

For deep implementation work, read: 04 -> 05 -> 09 -> 10.

## Source Repository

- VMF: https://github.com/miho/VMF
- VMF Tutorials: https://github.com/miho/VMF-Tutorials
- License: Apache 2.0
- Author: Michael Hoffer (info@michaelhoffer.de)
