# VMF.NET — C# Port of VMF (Visual Modeling Framework)

VMF is a Java code-generation framework that turns annotated interfaces into full model implementations.
This repo is the C# port. The original Java repo lives at ~/source/VMF.

## Before starting work
- Read `progress/STATUS.md` for current state
- Read `progress/decisions.md` for design decisions
- Reference `migration/` docs as needed (don't read all at once — pick the relevant one)
- The Java source at `~/source/VMF` is the authoritative reference for behavior

## Architecture

| Project | Purpose |
|---|---|
| VMF.NET.Runtime | Public interfaces + VList + ChangesManager. Ships as NuGet package. |
| VMF.NET.Core | Model analysis — port of Java's 7-pass `Model.java`, type resolution, property parsing |
| VMF.NET.SourceGenerator | Roslyn Source Generator using Scriban templates |
| VMF.NET.Json | System.Text.Json serialization, JSON Schema generation. Ships as NuGet package. |
| VMF.NET.Tests | Unit + integration tests |

## Key design decisions
- String-based templating (Scriban) for code generation
- Native C# events (`INotifyPropertyChanged` / `INotifyCollectionChanged`) on generated objects
- `IDisposable` replaces VMF's `Subscription.unsubscribe()` (enables `using`)
- `VList<T>` extends `ObservableCollection<T>` — adds containment, cross-ref, element callback hooks
- `ChangesManager` class handles recursive containment-tree listener registration internally
- User-facing API mirrors Java VMF closely: `obj.Vmf().Changes().AddListener(...)`

## Conventions
- Target: .NET 10 (dev machine has .NET 10 SDK only; Directory.Build.props sets net10.0)
- Nullable reference types: enabled
- C# naming: PascalCase for methods, properties, types
- Java `Optional<T>` maps to nullable `T?` with pattern matching
- Each session: update `progress/STATUS.md` before ending

## CI/CD
- GitHub Actions: build + test on push, NuGet publish on release tag
- NuGet packages: VMF.NET.Runtime, VMF.NET.SourceGenerator (VMF.NET.Core is bundled into the generator)
