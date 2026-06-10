# Grid Judgment Calls: dataStateChange Clear Search Method

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

## Clear-Search Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed | accepted | visible event metadata; this row does not require gathering it into the request body |
| `action.requestType` | observed as `searching` | accepted through shared action metadata | condition/display/request classification use case |
| `action.name` | observed as `actionBegin` | accepted through shared action metadata | stable action event metadata |
| `action.type` | observed as `actionBegin` | accepted through shared action metadata | stable action phase metadata |
| `action.searchString` | observed as empty string | excluded for this row | duplicate/derived clear signal; public use case is `ClearSearch()` plus absent top-level `search` |
| `action.cancel` | absent | not accepted for this row | clear-search does not emit it in this trace; do not infer from other variants |
| `search` | absent | excluded for this row | clearing search omits the descriptor; do not model as an empty array |
| `where` | absent | not accepted for this row | belongs to filtering row |
| `group` | absent | not accepted for this row | belongs to grouping row |
| `sorted` | absent | not accepted for this row | belongs to sorting/grouping rows |

## Future Reconsideration Rule

Toolbar clear-search, search combined with sort/filter/group state, or a distinct need for `action.searchString` may be reconsidered only through focused rows that prove a real typed DSL use case and behavior proof.
