# Source-generator dependencies (and why Scriban is held at 5.x)

**Status:** current · **Last verified:** 2026-08-22 · **Applies to:** `VMF.NET.SourceGenerator`

> **TL;DR — do not "fix" the NuGet audit warnings by upgrading Scriban.**
> Scriban is deliberately held at `5.*`. Versions 6+ add a hard `System.Text.Json`
> assembly reference that breaks code generation for anyone not building on the newest
> SDK. The reported advisories are not reachable from VMF.NET and are suppressed
> individually in `Directory.Build.props`.

## What this is about

`VMF.NET.SourceGenerator` is the build-system extension that generates the model
implementations. It is a Roslyn source generator that renders the `.sbn` templates in
`src/VMF.NET.SourceGenerator/Templates/` (embedded as resources) using
[Scriban](https://github.com/scriban/scriban).

That places Scriban in an unusual position: it is a **compile-time** dependency loaded into
the **compiler host**, not a normal runtime library. That distinction drives everything below.

## Why the compiler host matters

A Roslyn analyzer/generator is loaded into the compiler's analyzer load context. It can only
resolve:

1. assemblies shipped inside the package's `analyzers/dotnet/cs/` folder, and
2. assemblies the **host process** already provides.

NuGet's dependency graph is *not* used to resolve an analyzer's dependencies at load time.
Declaring a dependency in the `.nuspec` does **not** make it available to the generator.

`VMF.NET.SourceGenerator.nupkg` ships exactly three assemblies:

```
analyzers/dotnet/cs/Scriban.dll
analyzers/dotnet/cs/VMF.NET.Core.dll
analyzers/dotnet/cs/VMF.NET.SourceGenerator.dll
```

So any assembly Scriban references must come from the host. The host runtime is determined by
the **SDK/toolchain doing the build**, not by the consumer's `TargetFramework`.

## The constraint

Scriban's `netstandard2.0` build gained a `System.Text.Json` reference after the 5.x line:

| Scriban | IL-level assembly references (netstandard2.0)                        | Requires from host |
|---------|----------------------------------------------------------------------|--------------------|
| 5.12.1  | `Microsoft.CSharp`, `netstandard`, `System.Threading.Tasks.Extensions` | *nothing extra*    |
| 6.6.0   | …plus `System.Text.Json 8.0.0.0`                                       | STJ ≥ 8            |
| 7.2.6   | …plus `System.Text.Json 10.0.0.0`                                      | STJ ≥ 10           |

Measured by building a consumer project against the packed analyzer:

| Build SDK        | Scriban 5.12.1 | Scriban 6.6.0 | Scriban 7.2.6 |
|------------------|----------------|---------------|---------------|
| .NET 8 (8.0.202) | works          | works         | **fails**     |
| .NET 10 (10.0.204)| works         | works         | works         |

The Scriban 7 failure is a hard build break for the consumer:

```
CSC : error VMF002: Error generating code for 'M.IBox': Could not load file or assembly
'System.Text.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'.
```

Followed by `CS0117: 'IBox' does not contain a definition for 'NewInstance'` — i.e. **no code
is generated at all**.

### Why not 6.x as a middle ground

6.6.0 is compatible with both SDKs tested, but it is not worth the move:

- it still leaves the **critical** advisory open (GHSA-5wr9-m6jw-xx44 is patched in 7.0.0), so
  it would not even clear the IDE warning, and
- it introduces a `System.Text.Json 8.0.0.0` binding, trading away host compatibility for
  nothing.

### .NET Framework-hosted Roslyn (reasoned, not measured)

Visual Studio / `msbuild.exe` run the compiler on .NET Framework. `System.Text.Json` is **not**
part of the .NET Framework, so Scriban 6.x/7.x would need it shipped alongside the analyzer
there as well. This case was **not** empirically tested — but it is a further reason to prefer
the dependency-free 5.x build.

**Only Scriban 5.x has zero extra assembly references, so it is the only version that loads on
every compiler host.** That is what preserves the design goal of supporting older .NET
toolchains.

## About the security advisories

NuGet audit reports 14 advisories against Scriban 5.12.1 (one critical, several high). They are
suppressed **individually** by URL in `Directory.Build.props` — package auditing stays enabled
for every other package and any future advisory. No `NoWarn`, no `NuGetAudit=false`.

Why they are not reachable here:

- every advisory concerns rendering **untrusted templates** — sandbox escape (e.g. the critical
  `TypedObjectAccessor` cache bypass on `TemplateContext` reuse) or denial of service (loop-limit
  bypass, unbounded string generation);
- VMF.NET renders only templates **embedded in this repository**, compiled into the generator
  assembly. Templates are never user-supplied;
- model metadata (type names, property names, annotation values) enters the template as **data**,
  never as template source — it is not re-parsed as Scriban syntax;
- it runs at compile time, in the developer's own build.

An attacker would need to control the template text to exploit any of these, and cannot.

> This is a statement about *reachability in VMF.NET's usage*, not a claim that the advisories
> are invalid. Anyone reusing this generator with caller-supplied templates must re-evaluate.

## Runbook — re-verifying the matrix

Do **not** conclude "safe to upgrade" from a build on the newest SDK alone; that is the one host
where Scriban 7 happens to work. Reproduce like this:

1. Check the candidate's real assembly references (nuspec dependencies are not enough):

   ```powershell
   $fs = [IO.File]::OpenRead("$env:USERPROFILE\.nuget\packages\scriban\<ver>\lib\netstandard2.0\Scriban.dll")
   $pe = [System.Reflection.PortableExecutable.PEReader]::new($fs)
   $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
   $md.AssemblyReferences | % { $a=$md.GetAssemblyReference($_); "$($md.GetString($a.Name)) $($a.Version)" }
   ```

2. Build **then** pack to a local feed (`pack --no-build` alone fails; the `None` items need
   build output):

   ```bash
   dotnet build VMF.NET.sln -c Release
   dotnet pack  VMF.NET.sln -c Release --no-build -o /tmp/feed
   ```

3. Build a consumer that pins an **older** SDK via `global.json`
   (`{ "sdk": { "version": "8.0.202", "rollForward": "disable" } }`), referencing
   the `VMF.NET` metapackage from that feed, with a model interface in a `.VmfModel` namespace
   and a call to `NewInstance()`.

**Two traps that produce false results:**

- **NuGet cache poisoning.** Two builds of the same package id+version have different content but
  the same identity, so the global cache serves whichever landed first. Delete
  `~/.nuget/packages/vmf.net.sourcegenerator` (and `vmf.net.runtime`) between runs, or use
  distinct version numbers.
- **Stale incremental build.** Changing only the `Scriban` version in a `.csproj` can leave a
  previously-compiled `VMF.NET.SourceGenerator.dll` in `obj/`, pairing a generator built against
  one Scriban with another Scriban's DLL — which surfaces as
  `Method not found: Scriban.Template.Parse(...)`. Wipe `src/*/obj` and `src/*/bin` between
  variants.

## When to revisit

- Scriban ships a **5.x patch** for these advisories → upgrade within 5.x, drop the matching
  suppressions.
- Scriban **drops** the `System.Text.Json` dependency from its `netstandard2.0` build → re-run
  the matrix and upgrade.
- VMF.NET explicitly **drops support** for older toolchains → the constraint disappears; also
  consider shipping the required assemblies in `analyzers/dotnet/cs/`.

## Related

- `Directory.Build.props` — the `NuGetAuditSuppress` entries.
- `src/VMF.NET.SourceGenerator/TemplateRenderer.cs` — the only file that uses Scriban.
  (`VMF.NET.Core` previously carried an unused `Scriban` `PackageReference`; it was removed.)
