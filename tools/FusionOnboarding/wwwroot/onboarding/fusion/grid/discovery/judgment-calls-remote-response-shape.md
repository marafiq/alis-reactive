# Grid Judgment Calls: Remote Response Shape

Status: active for this row. Discovery records the whole Syncfusion
custom-binding response shape; public C# DSL accepts only the whole response
body lane proven here.

## Decision Matrix

| Source behavior | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `{ result, count }` assigned to `grid.dataSource` | observed in raw EJ2 trace and typed Playwright response | accept for whole response body | Syncfusion custom binding consumes both `result` rows and `count` pager total |
| `ResponseBody<TResponse>` whole-body source | existing DSL source maps to `responseBody` | keep `SetDataSource(ResponseBody<TResponse>)` | concise overload exactly matches custom-binding response body assignment |
| `ResponseBody<TResponse>, path` | not exercised by this row | keep open | path lane has different source expression and needs its own trace/proof |
| event-payload path source | not exercised by this row | keep open | event-payload lane has different value scope and must not inherit response proof |
| DataManager/adaptor source | not exercised by this row | keep open | adaptor lifecycle is separate from response-body assignment |
| nested data-source property path | not exercised by this row | keep open | nested paths need focused trace and typed consumer proof |
| MVC builder-owned initial `dataSource` | not exercised by this row | keep open | builder-owned render configuration differs from post-render response assignment |

## Boundary Rule

This row proves `SetDataSource(json)` where `json` is the whole HTTP success
body. Do not use it to close path overloads, event payload overloads,
DataManager/adaptor, nested paths, or initial builder-owned dataSource.
