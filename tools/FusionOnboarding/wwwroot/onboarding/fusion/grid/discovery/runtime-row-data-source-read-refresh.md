# FusionGrid Runtime Row: dataSource Read, Typed Array Rebind, Refresh

Status: proven for the typed array source/dataSource read/refresh row. This row
does not close remote DataManager/adaptor, response-path overloads, event
payload-path overloads, whole response body overloads, or nested data-source
paths.

## Pass Row

Close matrix row: `p.Component<FusionGrid>(id).SetDataSource(current.Where(...).AsSource()).Refresh()` -> Grid typed array data-source rebind -> sync component property read/write plus refresh method call after initial async HTTP load.

## Raw EJ2 Evidence

- Probe: `probes/raw-ej2-data-source-read-refresh.html`
- Trace: `traces/raw-ej2-data-source-read-refresh.trace.json`

The probe instantiates `ej.grids.Grid` directly, not through Alis. It proves:

- initial `grid.dataSource` is readable as an array with two rows;
- assigning a replacement array changes `grid.dataSource`;
- calling `grid.refresh()` returns `undefined`;
- visible rows update to the replacement data after refresh.

Trace highlights:

| Trace label | Evidence |
| --- | --- |
| `ready` | visible rows are `Alpha` and `Beta`; `dataSourceLength` is `2` |
| `dataSource read before replacement` | readable array contains `Alpha` and `Beta` |
| `dataSource read after assignment before refresh` | readable array contains `Delta`, `Echo`, and `Foxtrot` |
| `refresh() return` | method return is `undefined` |
| `visible rows after refresh` | rendered Grid rows are `Delta`, `Echo`, and `Foxtrot` |

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/ArrayGrid/Index.cshtml`
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenBindingArrayToGrid.cs`

## Accepted C# Surface

| C# API | Decision | Reason |
| --- | --- | --- |
| `SetDataSource(TypedSource<TElement[]> source)` | accepted for this row | maps a typed array source into `grid.dataSource` |
| `Data<TRow>()` | accepted for this row | reads current `grid.dataSource` as a typed array source |
| `Refresh()` | accepted for this row | applies the post-render data-source replacement visibly |

## Explicit Non-Closure

This row does not prove:

- `SetDataSource(ResponseBody<TResponse>, path)`;
- `SetDataSource(ResponseBody<TResponse>)`;
- `SetDataSource(eventPayload, path)`;
- DataManager/adaptor binding;
- nested data-source property paths;
- `{ result, count }` custom-binding response shape.

Those remain separate matrix rows because their source scopes and runtime
behavior differ.

## Typed DSL Proof

Command:

```text
scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid"
```

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-201013.trx`.

The first test proves the initial HTTP response is transformed by the typed
array DSL and bound into the Grid through `SetDataSource(TypedSource<T[]>)`.
The server fixture is deliberately not name-sorted, so the visible first row
proves the typed `OrderBy` path rather than fixture order. The second test
proves `Data()` reads the current Grid `dataSource`, the array pipeline filters
it client-side, `SetDataSource(TypedSource<T[]>)` writes the filtered source
back, `Refresh()` applies it, and visible rows drop from five to three. The test
also counts requests to `/Sandbox/Components/ArrayGrid/Residents` from the
click through the visible filtered Grid assertions and asserts zero, so the
no-second-roster-request claim is directly observable across the rebind proof.
