# Grid Runtime Row: Remote Response Shape

Status: proven for this row. Raw EJ2 discovery and focused typed DSL
Playwright proof are complete for whole-response custom binding through
`SetDataSource(ResponseBody<TResponse>)`. The component audit remains open.

## Row Boundary

`s.Component<FusionGrid>("residents-grid").SetDataSource(json)` where `json` is
the whole HTTP success response body shaped as `{ result, count }`.

This row proves only the whole-response custom-binding lane. It does not prove:

- `SetDataSource(ResponseBody<TResponse>, path)`;
- `SetDataSource(eventPayload, path)`;
- DataManager/adaptor binding;
- nested data-source property paths;
- MVC builder-owned initial `dataSource`.

## Evidence

- Probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-remote-response-shape.html`
- Trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-remote-response-shape.trace.json`
- C# DSL source: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs`
- Sandbox DSL: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml`
- Playwright proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action`

## Raw EJ2 Result

The raw probe instantiates `ej.grids.Grid` directly. It starts with a
custom-binding data source shaped as `{ result, count }`, then triggers
`dataStateChange` through `grid.sortColumn("name", "Ascending", false)`.

The trace proves:

- `dataStateChange` emits `skip`, `take`, `requiresCounts`, and `sorted`;
- the row handler computes a whole response object with own keys `count` and
  `result`;
- assigning that object to `grid.dataSource` preserves `count`, `result.length`,
  and the first result row;
- visible rows change from `Charlie, Alpha` to `Alpha, Bravo`;
- the pager text remains `1 of 2 pages (4 items)`, proving the `count` value is
  consumed by the Grid custom-binding surface.

## Primitive Mapping

Use existing primitives only:

- HTTP success response body scope: `ResponseBody<TResponse>`;
- whole response-body value read:
  `ValueExpression.ReadWholePayload(source.Scope)`;
- component property set: `SetReaction` targeting `dataSource`.

No new primitive is justified by this row.

## Typed DSL Proof

Command:

```text
scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"
```

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-203627.trx`.

The test proves the typed Fusion DSL posts real Grid data-state input, receives
HTTP 200 with `{ result, count }`, parses `result.length == 10` and
`count == 200`, then asserts the first returned row name is rendered in the
Grid and the pager displays `200 items` after `SetDataSource(json)`. The same
proof asserts the generated plan uses `member=responseBody` with an empty path,
which is the canonical whole-payload primitive shape.
