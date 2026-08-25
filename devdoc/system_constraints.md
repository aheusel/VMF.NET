# System constraints

**Status:** current · **Last updated:** 2026-08-25 · **Applies to:** all of VMF.NET

> The register of what VMF.NET **must** adhere to. A constraint here is binding: code, packaging
> and design decisions are checked against it, and a change that violates one is a defect
> regardless of how convenient it is.

## How to use this file

Each constraint has an **ID**, the rule itself, **why** it exists, and **how to verify** it. Cite
the ID (`C-3`) in commit messages and design notes rather than restating the reasoning.

A constraint is added only once it has been *established* — a measurement, a reference to Java's
implementation, or an explicit decision by the project owner. Note which, and when. If a
constraint is later found not to hold, mark it withdrawn with the reason rather than deleting it;
the history is what stops it being re-argued.

| Status | Meaning |
|---|---|
| **Binding** | In force. Violating it is a defect |
| **Provisional** | Agreed in principle, details still being worked out |
| **Withdrawn** | No longer applies; kept with the reason |

---

## C-1 · Behavioural identity with Java VMF

**Binding.** Established as the project's design goal by the owner.

Someone moving a model from Java VMF to VMF.NET must meet as few surprises as possible. Where the
two can behave the same, they must.

A difference is classified as exactly one of:

- **C# forces it** — unavoidable. Document it in a `DEVIATION:` note at the top of the file. Be
  slow to put anything here; "covariant property narrowing" sat in this category until M7 and was
  never true.
- **We chose it** — treat as a **defect to fix**, not a preference to defend, even where
  VMF.NET's behaviour is arguably nicer.
- **Surface convention** — `Name` not `name`, `IParent` not `Parent`. Not a divergence.

**Verify:** against Java's *implementation*, not its tests. The tests state what the authors chose
to pin; the implementation states what actually happens.

**See:** [`java-parity-roadmap.md`](java-parity-roadmap.md) (design goal, parity statement),
[`differences-to-java.md`](differences-to-java.md).

---

## C-2 · Usable on older .NET toolchains

**Binding.** Stated design goal.

VMF.NET must remain usable with older .NET versions and toolchains, not only the newest SDK.

The sharp edge is `VMF.NET.SourceGenerator`, which runs **inside the compiler host**. It resolves
only assemblies shipped in `analyzers/dotnet/cs/` plus whatever the host already provides —
NuGet's dependency graph does not apply at load time. So a build-time dependency that carries
extra assembly references can break consumers on older SDKs, and on .NET Framework-hosted Roslyn
as used by VS/MSBuild, even when everything builds locally.

**Verify:** before changing any `VMF.NET.SourceGenerator` dependency, its TFM, or its
`LangVersion` — inspect the candidate's **IL-level assembly references**, then pack and build a
real consumer under an **older SDK**. Testing only on the newest SDK produces false "safe to
upgrade" verdicts. Suppress unreachable audit advisories per-ID via `NuGetAuditSuppress` rather
than upgrading past the compatibility limit.

**See:** [`source-generator-dependencies.md`](source-generator-dependencies.md).

### C-2.1 · Scriban is pinned to 5.x

**Binding.** Measured 2026-08-22.

Scriban 6+ adds a hard `System.Text.Json` assembly reference to its `netstandard2.0` build, which
the compiler host must then supply. Scriban 7 fails outright on an .NET 8 SDK.

### C-2.2 · Target frameworks

**Binding.**

| Project | TFM | Why |
|---|---|---|
| `VMF.NET.Core` | `netstandard2.0` | loaded into the compiler host |
| `VMF.NET.SourceGenerator` | `netstandard2.0` | Roslyn analyzer requirement |
| `VMF.NET.Runtime` | `net6.0` | consumer-facing runtime |
| `VMF.NET.Json` | `net6.0` | consumer-facing |

Test projects target the current SDK and are not bound by this.

---

## C-3 · ~~The model interface is the public API~~

**Withdrawn 2026-08-25**, superseded by [C-6](#c-6--the-model-is-build-input-not-api). Kept
because it explains why several older decisions look the way they do, and because a constraint
that is re-argued from scratch tends to come back.

What it cost while it held: `[VmfModel]` on nearly every interface, `partial` on all of them, and
`{ get; set; }` on a settable `[Container]`. All three are gone.

Java's `vmfmodel` package is throwaway input: VMF reads it and generates a *separate* set of
public interfaces elsewhere. VMF.NET has no such redirection — the `[VmfModel]` interface is
`partial`, the generator adds members to **it**, and consumers use that same interface.

Consequences that follow from this and must not be "fixed" in isolation:

- a settable `[Container]` must be declared `{ get; set; }` in the model, because a partial
  interface cannot add a setter to a property already declared `{ get; }`;
- a narrowed property must be declared `new`;
- the model interface is read as API, so what it declares is what a user sees.

---

## C-4 · A namespace is the model boundary

**Binding.** Matches Java's package boundary.

The generator groups model interfaces by namespace and analyses each namespace as an independent
model. Two areas may both declare `IParent` without colliding. A model type that references a type
in another namespace is not resolved as a model type.

---

## C-5 · Release discipline

**Binding.** Decided by the owner, superseding an earlier "release early" rationale.

**No release while the ported suite has skipped or missing facts.** A release states that the
implementation matches the reference; it cannot state that while facts are skipped. The skip count
must equal the parity gap, which requires that every Java fact has a counterpart — a fact that was
never ported is invisible, where a skipped one is not.

**Verify:** full solution green, zero skipped, and the parity inventory re-derived rather than
trusted.

---

## C-6 · The model is build input, not API

**Binding.** Decided by the owner 2026-08-25, superseding [C-3](#c-3--the-model-interface-is-the-public-api).

Setting up a VMF.NET project must be as close to setting up a Java VMF project as the two build
systems allow. Java's arrangement is adopted:

- **The model lives in a `.VmfModel` sub-namespace**, mirroring Java's `…vmfmodel` package. Being
  there *is* the declaration — no attribute marks a model type.
- **The generator emits the public API into the parent namespace.** `MyApp.VmfModel.Parent`
  produces `MyApp.IParent`. Model interfaces are build input: `internal` by default, as Java's are
  package-private by default, and never the type a consumer holds.
- **Generated name** = `I` + model name, unless the model name already begins with `I` followed by
  an uppercase letter, in which case it is used unchanged.

What this settles:

| Was | Now |
|---|---|
| `[VmfModel]` on (almost) every interface | nothing — the namespace declares it |
| model interfaces `public partial` | plain `interface`, internal by default |
| settable `[Container]` needs `{ get; set; }` | setter always generated, as Java does |
| discovery sniffs for any VMF attribute | namespace decides |

### Why the previous rule had to go

Attribute-sniffing was unsound **in both directions**, and no extension of the attribute list could
fix it:

- it **missed** a plain model interface carrying no attribute — Java's `Test2.Named` is exactly
  that, so `[VmfModel]` was mandatory in the common case;
- it **over-matched**, because attribute names were compared without their namespace. Measured
  2026-08-25: an unrelated `ICustomerDto` using `[Required]` from
  `System.ComponentModel.DataAnnotations`, with no VMF attribute anywhere, generated five files
  including a full implementation.

**Verify:** an interface outside a model namespace generates nothing, whatever attributes it
carries; an interface inside one generates its API, whether or not it carries any.

### Still C#-forced after this

A narrowed property in the model file still wants `new` — `CS0108` is a warning, not an error, so
it compiles either way, but C# has no covariant override for an interface property. Suppressing
`CS0108` for model folders is deliberately out of scope.
