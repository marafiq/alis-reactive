# Grid Judgment Calls: dataStateChange Sorting

Status: active for this row. Discovery records every observed field; C# DSL accepts only fields with clear typed use cases and predictable contracts.

## Rule

Raw discovery is exhaustive. Public C# DSL is selective.

Do not onboard every discovered payload branch just because it exists. Accept a field only when all are true:

- the field has a stable payload shape for the row's trigger variants or the variant split is explicitly modeled;
- the field has a clear Fusion DSL use case;
- the field can be represented with a typed contract, not `object`, `dynamic`, or stringly path access;
- existing primitives can consume it without adding onboarding-specific primitives;
- Playwright can prove its behavior through the typed Fusion DSL.

If a field is excluded, record the exact reason here so the next pass does not rediscover and guess.

## Sorting Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed in method-fired and header-click traces | accepted | stable scalar event metadata; useful for proving observer-injected event identity |
| `skip` | observed in both traces | accepted | stable paging/data-state scalar; already used by server request gather |
| `take` | observed in both traces | accepted | stable paging/data-state scalar; already used by server request gather |
| `requiresCounts` | observed in both traces and declared by Syncfusion `DataStateChangeEventArgs` | accepted | clear server/custom-binding use case: response must include records and count |
| `sorted` | observed in both traces as array of `{ name, direction }` | accepted | central sorting use case; consumed as whole typed array through existing event gather primitive |
| `sorted[].name` | observed in both traces | accepted as item contract | stable item key; useful server-side for field sort |
| `sorted[].direction` | observed in both traces | accepted as item contract | stable item key; useful server-side for sort direction |
| `where` | absent in sorting-only traces | not accepted for this row | belongs to filtering row; must be proven by filtering trigger trace |
| `search` | absent in sorting-only traces | not accepted for this row | belongs to searching row; must be proven by searching trigger trace |
| `group` | absent in sorting-only traces | not accepted for this row | belongs to grouping row; must be proven by grouping trigger trace |
| `action.requestType` | observed in both traces and declared by `GridActionEventArgs` inheritance | accepted | clear condition/display/request classification use case |
| `action.columnName` | observed in both traces and declared by `SortEventArgs` | accepted | clear sorting column use case |
| `action.direction` | observed in both traces and declared by `SortEventArgs` | accepted | clear sorting direction use case |
| `action.name` | observed in both traces | accepted | stable observer-injected action event metadata; useful for proving action event identity |
| `action.type` | observed in both traces and declared by `GridActionEventArgs` | accepted | stable action phase metadata, e.g. `actionBegin` |
| `action.cancel` | observed in both traces and declared by `GridActionEventArgs` | accepted for read only | clear typed boolean; mutation/cancel behavior requires separate row before exposing mutation helper |
| `action.target` | `null` in method-fired trace; DOM `TH` in header-click trace | excluded from public typed C# payload | browser-owned DOM object; no clear typed C# DSL use case for this row; exposing `object`/`dynamic` would pollute the DSL |

## Future Reconsideration Rule

`action.target` may be reconsidered only through a new row that proves a predictable typed scalar use case, such as a safe column identifier already available as `action.columnName`. Do not expose the DOM element itself.
