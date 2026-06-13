# Grid Judgment Calls: dataStateChange Grouping Method

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

## Grouping Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed and declared by `DataStateChangeEventArgs` | accepted | custom-binding response must include records and count |
| `group` | observed as string array and declared by `DataStateChangeEventArgs` | accepted as whole typed array | central server grouping use case |
| `groups` | observed as string array | excluded for this row | duplicates `group` and is not the declared public data-state member |
| `sorted` | observed as array with `careLevel` ascending | accepted through existing sorted state | grouping adds a sort query through Syncfusion `groupAddSortingQuery`; server may need this state |
| `sorted[].name` | observed as `careLevel` | accepted through whole typed `Sorted` array | already mapped by sorting row |
| `sorted[].direction` | observed as `ascending` | accepted through whole typed `Sorted` array | already mapped by sorting row |
| `action.requestType` | observed as `grouping` | accepted through shared action metadata | condition/display/request classification use case |
| `action.name` | observed as `actionBegin` | accepted through shared action metadata | stable observer-injected action event metadata |
| `action.type` | observed as `actionBegin` | accepted through shared action metadata | stable action phase metadata |
| `action.columnName` | observed as `careLevel` | accepted | tells which field was grouped |
| `action.preventFocusOnGroup` | observed as `false` | excluded for this row | Syncfusion internal focus flag with no server data-state or typed DSL use case |
| `action.cancel` | absent | not accepted for this row | grouping does not emit it in this trace; do not infer from other action variants |
| `where` | absent | not accepted for this row | belongs to filtering or combination rows |
| `search` | absent | not accepted for this row | belongs to searching or combination rows |
| `aggregates` | absent | not accepted for this row | grouping with aggregates requires a separate aggregate row |

## Future Reconsideration Rule

`groups`, `action.preventFocusOnGroup`, ungrouping, clear grouping, drag/drop
grouping, toggle grouping, lazy-load grouping, grouped aggregates, and grouping
combined with filtering/searching may be reconsidered only through focused rows
that prove a distinct typed DSL use case and behavior proof.
