# Design Decisions Log

Append-only. Each entry records a decision, rationale, and date.

---

### 2026-04-05: String-based templating for code generation
**Decision:** Use Scriban (or similar) string templates inside the Roslyn Source Generator.
**Rationale:** Mirrors VMF's Velocity approach, keeps generation logic readable and close to the Java original. Revisit only if profiling reveals a concrete performance problem.

### 2026-04-05: Native C# events for observable model
**Decision:** Generated objects implement `INotifyPropertyChanged` / `INotifyCollectionChanged`. `IDisposable` replaces `Subscription.unsubscribe()`.
**Rationale:** Direct WPF/WinForms/MAUI data binding with zero glue code. `using` gives automatic cleanup. A `ChangesManager` class (port of `ChangesImpl`) handles recursive containment-tree subscriptions internally.

### 2026-04-05: VList extends ObservableCollection<T>
**Decision:** `VList<T> : ObservableCollection<T>` with VMF-specific hooks (containment, cross-refs, element callbacks).
**Rationale:** Inherits `INotifyCollectionChanged` for free, works with UI binding out of the box. No need to reimplement list fundamentals.

### 2026-04-05: Separate repository
**Decision:** C# port lives in its own repo (`VMF.NET`), not inside the Java VMF repo.
**Rationale:** Independent build (dotnet vs Gradle), clean CI/CD, independent versioning, NuGet packaging expects standalone project root.
