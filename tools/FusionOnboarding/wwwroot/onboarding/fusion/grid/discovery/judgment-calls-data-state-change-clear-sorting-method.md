# Grid Judgment Calls: dataStateChange Clear Sorting Method

Status: active for this row. Discovery records every observed field; public C#
accepts only fields with clear typed use cases and predictable contracts.

## Rule

Raw discovery is exhaustive. Public C# DSL is selective.

Accept a field only when all are true:

- the field has a stable payload shape for this trigger variant;
- the field has a clear Fusion DSL use case;
- the field can be represented with a typed contract, not `object`, `dynamic`, or stringly path access;
- existing primitives can consume it without adding onboarding-specific primitives;
- Playwright can prove its behavior through the typed Fusion DSL.

If a field is excluded, record the exact reason here so the next pass does not
rediscover and guess.

## Clear-Sorting Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed | accepted | visible event metadata; this row does not require gathering it into the request body |
| `action.requestType` | observed as `sorting` | accepted through shared action metadata | condition/display/request classification use case; clear sorting reports the sorting action lane |
| `action.name` | observed as `actionBegin` | accepted through shared action metadata | stable action event metadata |
| `action.type` | observed as `actionBegin` | accepted through shared action metadata | stable action event metadata for this row |
| `action.target` | observed as `null` | excluded for this row | browser-owned/gesture-owned target has no typed C# use case when clearing method-trigger sorting |
| `action.columnName` | absent | not accepted for this row | clear sorting does not emit it in this trace; do not infer from sort-apply rows |
| `action.direction` | absent | not accepted for this row | clear sorting does not emit it in this trace; do not infer from sort-apply rows |
| `action.cancel` | absent | not accepted for this row | clear sorting does not emit it in this trace; do not infer from sort-apply rows |
| `sorted` | absent | excluded for this row | clearing sorting omits the descriptor; do not model as an empty array |
| `where` | absent | not accepted for this row | belongs to filtering row |
| `search` | absent | not accepted for this row | belongs to searching row |
| `group` | absent | not accepted for this row | belongs to grouping row |
| `aggregates` | absent | not accepted for this row | no source-backed or behavior-backed public use case in this row |

## Future Reconsideration Rule

Clear sorting combined with active filter, search, grouping, multi-sort state,
column-menu gestures, or grouped-sort state may be reconsidered only through
focused rows that prove a real typed DSL use case and behavior proof.
