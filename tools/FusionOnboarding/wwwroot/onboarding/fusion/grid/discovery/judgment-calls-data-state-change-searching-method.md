# Grid Judgment Calls: dataStateChange Searching Method

Status: active for this row. Discovery records every observed field; public C# accepts only fields with clear typed use cases and predictable contracts.

## Rule

Raw discovery is exhaustive. Public C# DSL is selective.

Accept a field only when all are true:

- the field has a stable payload shape for this trigger variant;
- the field has a clear Fusion DSL use case;
- the field can be represented with a typed contract, not `object`, `dynamic`, or stringly path access;
- existing primitives can consume it without adding onboarding-specific primitives;
- Playwright can prove its behavior through the typed Fusion DSL.

If a field is excluded, record the exact reason here so the next pass does not rediscover and guess.

## Searching Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed and declared by `DataStateChangeEventArgs` | accepted | custom-binding response must include records and count |
| `search` | observed as array | accepted as whole typed array | central server searching use case |
| `search[].fields` | observed as string array | accepted | server search field scope |
| `search[].key` | observed as `Memory` | accepted | central search text use case |
| `search[].operator` | observed as `contains` | accepted | server search operator use case |
| `search[].ignoreCase` | observed as `true` | accepted | server search comparison use case |
| `search[].ignoreAccent` | observed as `false` | accepted | stable scalar from search settings |
| `action.requestType` | observed | accepted through shared action metadata | condition/display/request classification use case |
| `action.name` | observed | accepted through shared action metadata | stable observer-injected action event metadata |
| `action.type` | observed | accepted through shared action metadata | stable action phase metadata |
| `action.searchString` | observed as `Memory` | excluded for this row | duplicates `search[].key`; no distinct typed DSL use case proven |
| `action.cancel` | absent | not accepted for this row | search does not emit it in this trace; do not infer from other action variants |
| `where` | absent | not accepted for this row | belongs to filtering row |
| `group` | absent | not accepted for this row | belongs to grouping row |
| `sorted` | absent | not accepted for this row | sorting plus searching requires a combination row |

## Future Reconsideration Rule

`action.searchString`, toolbar input search behavior, toolbar clear-search, and
search combined with other state may be reconsidered only through focused rows
that prove a distinct typed DSL use case and behavior proof. Method-trigger
`ClearSearch()` is covered by its own clear-search row.
