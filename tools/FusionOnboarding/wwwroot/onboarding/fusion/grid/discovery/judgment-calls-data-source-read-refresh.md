# FusionGrid Judgment Calls: dataSource Read, Typed Array Rebind, Refresh

Status: active for the focused data-source typed array row.

| Candidate | Evidence | Decision | Reason |
| --- | --- | --- | --- |
| `dataSource` property read | raw trace reads `grid.dataSource` before and after assignment | accepted through `Data<TRow>()` | clear typed array source use case for client-side array transforms |
| `dataSource` property write | raw trace assigns replacement rows to `grid.dataSource`; typed DSL writes through `SetDataSource(TypedSource<T[]>)` | accepted | existing component property set primitive covers the post-render rebind |
| `refresh()` | raw trace calls `grid.refresh()` after assignment and visible rows update | accepted | existing component method-call primitive applies the rebind visibly |
| `refresh()` return value | raw trace records `undefined` | not a source | no useful return value exists for typed DSL |
| DataManager/adaptor `dataSource` | not exercised by this row | deferred | requires a focused remote/adaptor row |
| `{ result, count }` data source object | not exercised by this row | deferred | custom-binding response shape is a separate row |
| response-path/event-payload-path `SetDataSource` overloads | not exercised by this row | deferred | different value scopes; must not inherit this proof |
