# Grid Judgment Calls: dataStateChange Paging

Status: active for this row. Discovery records every observed field and relevant source candidate; C# DSL accepts only fields with clear typed use cases and predictable contracts.

## Rule

Raw discovery is exhaustive. Public C# DSL is selective.

Accept a field only when all are true:

- the field has a stable payload shape for the row's trigger variants or the variant split is explicitly modeled;
- the field has a clear Fusion DSL use case;
- the field can be represented with a typed contract, not `object`, `dynamic`, or stringly path access;
- existing primitives can consume it without adding onboarding-specific primitives;
- Playwright can prove its behavior through the typed Fusion DSL.

If a field is excluded, record the exact reason here so the next pass does not rediscover and guess.

## Paging Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed in method and pager-click traces | accepted | stable scalar event metadata; useful for proving observer-injected event identity |
| `skip` | observed in both traces | accepted | central server paging use case |
| `take` | observed in both traces | accepted | central server paging use case |
| `requiresCounts` | observed in both traces and declared by Syncfusion `DataStateChangeEventArgs` | accepted | clear custom-binding use case: response must include records and count |
| `action.requestType` | observed in both traces and declared by `GridActionEventArgs` inheritance | accepted | clear condition/display/request classification use case |
| `action.currentPage` | observed in both traces and created by `page.js` paging action | accepted | central page-number use case |
| `action.previousPage` | observed in both traces and created by `page.js` paging action | accepted | useful for page transition display/conditions |
| `action.pageSize` | observed in both traces and created by `page.js` paging action | accepted | central server paging use case |
| `action.name` | observed in both traces | accepted | stable observer-injected action event metadata |
| `action.type` | observed in both traces and declared by `GridActionEventArgs` | accepted | stable action phase metadata, e.g. `actionBegin` |
| `action.cancel` | observed in both traces and declared by `GridActionEventArgs` | accepted for read only | clear typed boolean; mutation/cancel behavior requires a separate row before exposing mutation helper |
| `where` | absent in paging-only traces | not accepted for this row | belongs to filtering row |
| `search` | absent in paging-only traces | not accepted for this row | belongs to searching row |
| `group` | absent in paging-only traces | not accepted for this row | belongs to grouping row |
| `sorted` | absent in these unsorted paging traces | not accepted for this row | sorting plus paging requires a combination row before claiming carried sort state |
| `action.previousPageSize` | source adds it only for page-size changes; absent in both traces | not accepted for this row | page-size change is a distinct trigger variant and must be proven separately |
| `action.rows` | declared by `PageEventArgs`; absent in both traces | excluded from public typed payload for this row | no observed stable payload and no clear typed DSL use case yet |
| `action.target` | not observed in either paging trace | excluded from public typed payload | no discovered stable typed use case; do not add broad DOM/object fields |

## Future Reconsideration Rule

`previousPageSize`, `rows`, and any DOM/browser-owned payload branch may be
reconsidered only through a new row that proves a predictable typed use case and
an end-to-end behavior proof. Discovery must still list them; C# must not expose
them from this paging row.
