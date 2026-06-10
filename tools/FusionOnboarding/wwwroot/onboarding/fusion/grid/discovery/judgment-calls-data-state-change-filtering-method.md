# Grid Judgment Calls: dataStateChange Filtering Method

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

## Filtering Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed | accepted | stable scalar event metadata |
| `skip` | observed | accepted | server data-state scalar |
| `take` | observed | accepted | server data-state scalar |
| `requiresCounts` | observed and declared by `DataStateChangeEventArgs` | accepted | custom-binding response must include records and count |
| `where` | observed as array | accepted as whole typed array | central server filtering use case |
| `where[].condition` | observed on complex predicate | accepted | needed to combine recursive predicates |
| `where[].ignoreCase` | observed | accepted | server filtering comparison use case |
| `where[].ignoreAccent` | observed | accepted | stable scalar from predicate |
| `where[].predicates[]` | observed as nested array | accepted as recursive typed array | proper array primitive; no indexed paths |
| `where[].predicates[].field` | observed | accepted | server field filter use case |
| `where[].predicates[].operator` | observed | accepted | server operator use case |
| `where[].predicates[].value` | observed | accepted | server value use case for text filter row |
| `where[].isComplex` | observed and declared by `@syncfusion/ej2-data` `Predicate` | accepted | source-backed predicate node discriminator; server recursive filtering uses it to distinguish composite nodes from leaf predicates |
| `where[].predicates[].isComplex` | observed and declared by `@syncfusion/ej2-data` `Predicate` | accepted | source-backed predicate node discriminator; preserves recursive predicate shape without stringly inspection |
| `action.requestType` | observed | accepted through shared action metadata | condition/display/request classification use case |
| `action.name` | observed | accepted through shared action metadata | stable observer-injected action event metadata |
| `action.type` | observed | accepted through shared action metadata | stable action phase metadata |
| `action.cancel` | observed | accepted for read only through shared action metadata | clear typed boolean; mutation/cancel behavior requires separate row |
| `action.action` | observed as `filter` | excluded for this row | useful only if clear-filter/action subtype row proves a stable use case |
| `action.currentFilteringColumn` | observed as `status` | excluded for this row | duplicates `where[].predicates[].field`; avoid duplicate public API |
| `action.currentFilterObject` | observed as filter settings object | excluded for this row | duplicates `where`; includes settings/internal shape |
| `action.columns` | observed as filter settings columns array | excluded for this row | duplicates `where`; includes settings/internal shape |
| `matchCase` | observed only under `action.currentFilterObject` | removed from `FusionGridTextFilterCriterion` for this row | not emitted by top-level `where`; accepting it would conflate settings object with query predicate |
| `predicate` | observed only under `action.currentFilterObject` | removed from `FusionGridTextFilterCriterion` for this row | not emitted by top-level `where`; top-level query uses `condition` |
| `actualFilterValue` | observed only under `action.currentFilterObject` | excluded | internal/settings object with no server query use case |
| `actualOperator` | observed only under `action.currentFilterObject` | excluded | internal/settings object with no server query use case |
| `uid` | observed only under `action.currentFilterObject` | excluded | browser-generated column uid, not durable typed DSL |
| `isForeignKey` | observed only under `action.currentFilterObject` | excluded for this row | no clear text-filter server use case proven |
| `search` | absent | not accepted for this row | belongs to searching row |
| `group` | absent | not accepted for this row | belongs to grouping row |
| `sorted` | absent | not accepted for this row | sorting plus filtering requires a combination row |

## Future Reconsideration Rule

`action.action`, `action.currentFilteringColumn`, `action.currentFilterObject`,
`action.columns`, `matchCase`, `predicate`, and settings-only filter members may
be reconsidered only through a focused row that proves a distinct typed DSL use
case and behavior proof. Do not add broad settings objects to public event args.
