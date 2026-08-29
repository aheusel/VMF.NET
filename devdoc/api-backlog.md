# API backlog — deferred improvements

**Status:** open · **Opened:** 2026-08-25 · **Deferred from:** 0.3.0

Improvements identified but deliberately **not** made. Each entry says what it is, why it is worth
doing, and what already exists to build it on.

Everything here is **additive** — none of it breaks a caller. That is why it could be deferred at
all, where the rest of the 0.3.0 API work could not: a breaking change is cheap before a release
and expensive after one, an addition costs the same either way.

---

## Traversal and LINQ

0.3.0 replaced `Content.Stream()` with `Content.Traverse()`, an ordinary `IEnumerable<IVObject>`
that composes with LINQ, and kept `Content.Cursor()` for modifying a graph mid-walk. See
[`differences-to-java.md`](differences-to-java.md), *Graph traversal is a LINQ sequence*.

These five would take it further. Ordered by value.

### 1. `Ancestors()`, `AncestorsAndSelf()`, `Root()`

**The biggest gap: there is no way to walk *up*.** Today that means a hand-rolled loop, and it has
to be written per model, because the container property has a different name on every type:

```csharp
// today
var chain = new List<IVObject>();
for (var p = node.Flow; p != null; p = p.Flow) chain.Add(p);   // 'Flow' is model-specific
```

Feasible generically: `IVObjectInternal` already exposes `GetParentIndices()`, and the container
back-pointer is a plain model-typed property, so the chain can be walked without knowing its name.

Completes the vocabulary a .NET developer expects beside `Descendants` — LINQ-to-XML pairs
`Descendants`/`DescendantsAndSelf` with `Ancestors`/`AncestorsAndSelf`.

### 2. `Descendants()`

The root-excluding containment walk, i.e. `Traverse(IterationStrategy.ContainmentTree)` minus
self. One line over what exists.

Worth it mainly so the containment case reads properly without the caller passing an enum, and so
`Descendants()`/`Ancestors()` land as a matched pair rather than one direction being a named
method and the other an argument.

### 3. Extension methods over `IEnumerable<IVObject>`

So traversal composes off a **sequence**, not only off a single object:

```csharp
flow.Nodes.Descendants().OfType<Port>()
```

This is what LINQ-to-XML does (`IEnumerable<XElement>.Descendants()`), and it is what collapses
two or three separate queries into one chain. Needs (1) and (2) first, since it is the same
operations lifted over a sequence.

### 4. Depth or path during traversal

`Traverse()` flattens the graph, so "how deep is this object" and "how did I reach it" cannot be
answered in LINQ at all — you have to drop to `Cursor` and track it yourself.

A `TraverseWithDepth()` yielding `(IVObject Object, int Depth)`, or depth exposed on the cursor,
would cover a class of queries that is currently simply unavailable. Check whether `VmfIterator`
already tracks depth internally before designing this — it maintains an iterator stack, so it may
be close to free.

### 5. An analyzer for the `from T x in` trap

Query syntax lets the element type be written inline, and it does **not** mean what a reader
expects:

```csharp
from Node n in content.Traverse()   // lowers to .Cast<Node>(), NOT .OfType<Node>()
select n.Name;
```

`Traverse()` yields a mixed graph, so `Cast` hits the first non-`Node` and throws. Verified
2026-08-25: `InvalidCastException`.

An analyzer suggesting `.OfType<T>()` would turn a runtime failure into a build-time one. Weigh
the cost: a Roslyn analyzer is real machinery for one mistake, and it also enlarges the
compiler-host dependency surface that **C-2** constrains. Documenting it — already done in
`differences-to-java.md` — may be the proportionate answer.

---

## Considered and rejected

- **`IContent : IEnumerable<IVObject>`**, so `foreach (var o in obj.VMF.Content)` works directly.
  Rejected: `IContent` also carries `DeepCopy`, `ContentEquals` and `ContentHashCode`, so making
  it a sequence conflates *the content API* with *the content*. `XElement` is not
  `IEnumerable<XElement>` for the same reason.
- **A generic `Traverse<T>()`.** That is `Stream<T>()` returning under a new name. It was deleted
  in 0.3.0 precisely because `OfType<T>` already is that operation — the old implementation was
  literally `Stream().OfType<T>()`.

---

## Not an API question, but open

`ModelDiff` — graph diff / apply / merge — is implemented in Java VMF and **not** in VMF.NET. It
is the only unported class in the Java test suite. Recorded under *Not implemented* in
[`differences-to-java.md`](differences-to-java.md).
