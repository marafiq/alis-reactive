# Grid Judgment Calls: dataStateChange Clear Filtering Method

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

## Clear-Filtering Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed | accepted | visible event metadata; this row does not require gathering it into the request body |
| `action.requestType` | observed as `refresh` | accepted through shared action metadata | condition/display/request classification use case; clear filtering uses refresh, not filtering |
| `action.name` | observed as `actionBegin` | accepted through shared action metadata | stable action event metadata |
| `action.type` | absent | not accepted for this row | clear filtering does not emit it in this trace; do not infer from filtering rows |
| `action.cancel` | absent | not accepted for this row | clear filtering does not emit it in this trace; do not infer from filtering rows |
| `action.action` | absent | not accepted for this row | filtering-apply settings action is not emitted by clear filtering |
| `action.currentFilteringColumn` | absent | not accepted for this row | no current filtering column remains after clear |
| `action.currentFilterObject` | observed as `null` | excluded for this row | settings/internal filter object; no typed C# use case when clearing |
| `action.columns` | observed as empty array | excluded for this row | settings/internal filter collection; no typed C# use case when clearing |
| `where` | absent | excluded for this row | clearing filters omits the descriptor; do not model as an empty array |
| `search` | absent | not accepted for this row | belongs to searching row |
| `group` | absent | not accepted for this row | belongs to grouping row |
| `sorted` | absent | not accepted for this row | belongs to sorting/grouping rows |

## Future Reconsideration Rule

Menu clear-filter gestures, toolbar/filterbar clear gestures, and clear filtering combined with sort/search/group state may be reconsidered only through focused rows that prove a real typed DSL use case and behavior proof.
