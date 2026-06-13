# Grid Runtime Row: Column Auto-Fit Methods (audit defect + fix)

Status: defect found and fixed during audit; typed DSL Playwright proof attached.

Scenario (senior-living): care staff auto-fit a wide column, then all columns, for a
readable roster. Lane: component method — void, 0 args (`AutoFitColumns`) and 1 arg
(`AutoFitColumn`), authoritative map Lane 2.

## Defect (caught by the loop, not by prior tests)

`AutoFitColumn(field)` and `AutoFitColumns()` are both EJ2 `autoFitColumns` overloads
(1 arg = one column, 0 args = all). The Fusion slice registered **both under the same
plan member name** `autoFitColumns`:

```
AutoFitAllColumnsMethod = ComponentMethod.Named("autoFitColumns");            // 0 args
AutoFitColumnMethod     = ComponentMethod.Named("autoFitColumns").WithArgs<string>(); // 1 arg
```

Using both in one plan threw at render time:

```
InvalidOperationException: Method 'autoFitColumns' registered with 1 argument(s)
but re-registered with 0 argument(s).
  at ExactMethodArgumentContract.MergeIntoExact (BrowserObjectContract.cs:265)
```

This was never caught because **no sandbox view exercised either method** — the audit
loop surfaced it the first time both were used in a focused scenario.

## Root cause

The plan's `BrowserObjectContract` merges all declarations of a member name and
requires a single argument contract. Two overloads sharing one plan member name with
different argument counts cannot merge. This is the overloaded-method rule from the
authoritative DSL map / pattern P008: distinct plan member names mapped to the same JS
path.

## Fix (component slice, existing primitive — no DSL primitive change)

```
AutoFitAllColumnsMethod = ComponentMethod.Mapped("autoFitColumnsAll", "autoFitColumns");
AutoFitColumnMethod     = ComponentMethod.Mapped("autoFitColumnsField", "autoFitColumns").WithArgs<string>();
```

Distinct plan member names (`autoFitColumnsAll`, `autoFitColumnsField`), both mapped to
the JS path `autoFitColumns`. Plan contract merge is now deterministic; the runtime
still calls `grid.autoFitColumns()` and `grid.autoFitColumns(field)`.

## Proof

Focused view `/Sandbox/Components/Grid/ColumnFit` + `WhenUsingFusionGridColumnFit`
(fresh build) measures rendered header widths: `AutoFitColumn(riskLevel)` shrinks the
Risk column below 300px while Resident stays wide; `AutoFitColumns()` then shrinks
Resident too.
