# Array DSL — recorded follow-ups (broader-framework, NOT made here)

Date: 2026-05-30
Status: RECORDED ONLY — these touch framework code outside the array-DSL slice. They were
deliberately not implemented in the quality/extension pass (scope was "only change the array
DSL; record broader wins"). Each is specified precisely so the owning layer can execute.

The array-DSL pass itself (quality fixes, per-element method calls, `AsSource()` adapter) is
complete and verified: vitest green, typecheck clean, C# build clean, ArrayOps + DomOps
Playwright slices green.

## 1. Fusion `SetDataSource(TypedSource<T[]>)` overloads — HIGH confidence, clear win

**Why a massive win:** `ReactiveArray<T>.AsSource()` already exposes a transformed array as a
`TypedSource<T[]>` (shipped in this pass). The only missing half is a component-side overload to
consume it, which unlocks binding a **client-side filtered/sorted/mapped** array straight to a
component data source **without an HTTP round-trip** — e.g. filter a loaded roster by a dropdown
selection and rebind the grid, entirely in the plan.

**Why not made here:** the component extension files live in the Fusion layer (vendor scope), not
the array-DSL scope.

**Exact change (one overload per file, identical structural pattern to the existing
`SetDataSource(ResponseBody<T>, path)` overloads — a single `EmitSet`):**

```csharp
public static ComponentRef<TComponent, TModel> SetDataSource<TModel, TElement>(
    this ComponentRef<TComponent, TModel> self, TypedSource<TElement[]> source)
    where TModel : class
    => self.EmitSet(DataSourceProperty, source.ToValueExpression());
```

Files: `FusionGridExtensions.cs`, `FusionDropDownListExtensions.cs`, `FusionMultiSelectExtensions.cs`,
`FusionAutoCompleteExtensions.cs`, `FusionMultiColumnComboBoxExtensions.cs`, `FusionKanbanExtensions.cs`
(Kanban also calls `EmitCall(DataBindMethod)` to match its existing overloads). Do NOT auto-`Refresh()`
— keep the explicit-refresh convention of the existing overloads. Schedule/PivotView are excluded
(their dataSource semantics differ — a Fusion-layer judgement).

Enables: `p.Component<FusionGrid>("grid").SetDataSource(residents.Where(r => r.Active).OrderBy(r => r.LastName).AsSource()).Refresh();`

## 2. C# sync-condition marker for array-op predicates — MEDIUM confidence, defense-in-depth

**Why a win:** `ArrayOperationExpression.Predicate` is typed `ConditionGraph?` (the broad base).
The expression compiler only ever emits `compare/all/any/not` (never `Confirm`), so a Promise-in-
filter bug is already prevented in practice — but the C# *type* doesn't enforce it. Narrowing the
predicate type to a sync-only marker makes the compiler a hard gate (matches the generated TS
`ValidationCondition` union and spec §14.2).

**Why not made here:** there is no sync-only C# condition base today; introducing one means adding a
marker interface to the shared `ConditionGraph` hierarchy (`Compare/All/Any/Not` implement it) —
a broader-framework edit. Low practical risk (compiler can't emit `Confirm`), so deferred.

**Change:** add `internal interface ISyncCondition` (or an abstract `SyncCondition` base) in
`ConditionGraph.cs`; have `CompareCondition/AllCondition/AnyCondition/NotCondition` implement it
(NOT `ConfirmCondition`); change `ArrayOperationExpression.Predicate`, the `Array*` factories, and
`ElementExpressionCompiler.CompilePredicate` return type to that marker.

## 3. Per-element side-effecting method calls (foreach reaction) — distinct concept, larger

**Why recorded:** per-element method calls land in this pass as **value projections** and are
intentionally restricted to PURE, deterministic methods (the `ElementExpressionCompiler` whitelist).
Side-effecting per-element methods — e.g. EJ2 Grid `Row.setCellValue(field, value)` for each selected
row — are NOT value projections; they are *iteration with side effects*. The value algebra is for
reading/computing, so these do not belong as an `array-op` projection.

**The right shape:** a new **per-element reaction** ("for-each") node — a reaction (not a value) that
iterates an array source, binds each element to the element scope, and runs a sub-pipeline of
reactions (set/call/dispatch) against it. This reuses the element-scope stack already in place
(`ExecutionContext.withElement`) and the existing reaction executor. It is a Layer-1→3 primitive
(new reaction kind + builder + runtime case), larger than the array-DSL value work, and belongs in
its own slice. Until then, side-effecting per-element work stays in the plugin escape hatch.

## Confirmed NON-changes (verified, for the record)

Per-element method calls required **no** change to `runtime-path.ts` (`call()`/`fn.apply` already
exists), `execution-context.ts` (`withElement` already exists), or `Source.cs` (`PayloadSource.Element()`
already exists) — the capability was inherent in the existing primitives, exactly as established.
