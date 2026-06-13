# Grid Judgment Calls: recordClick Cell

Status: active for this row. Discovery records every observed field; public C# accepts only fields with clear typed use cases and predictable contracts.

## Rule

Raw discovery is exhaustive. Public C# DSL is selective.

Accept a field only when all are true:

- the field has a stable payload shape for this trigger variant;
- the field has a clear Fusion DSL use case;
- the field can be represented with a typed contract, not `object`, `dynamic`, or stringly path access;
- existing primitives can consume it without adding onboarding-specific primitives;
- Playwright can prove its behavior through the typed Fusion DSL.

## Payload Decisions

| Payload path | Discovery status | C# DSL decision | Reason |
| --- | --- | --- | --- |
| `name` | observed as `recordClick` | accepted | stable event metadata |
| `rowData` | observed as object | accepted as generic typed row DTO | central row-click use case |
| `rowData.*` | observed from row DTO | accepted through `TRow` properties selected by typed lambdas | developer controls row DTO shape |
| `rowIndex` | observed as `0` | accepted | stable scalar coordinate |
| `cellIndex` | observed as `1` | accepted | stable scalar coordinate |
| `cancel` | observed as `false` | excluded for this row | no cancel behavior proved; Syncfusion does not read it after `recordClick` |
| `cell` | observed DOM `TD` | excluded | browser-owned DOM object |
| `row` | observed DOM `TR` | excluded | browser-owned DOM object |
| `target` | observed DOM `TD` | excluded | browser-owned DOM object |
| `event` | observed `MouseEvent` | excluded | browser-owned event object |
| `column` | observed EJ2 column object | excluded for this row | broad vendor object; field/header may require separate typed column-source row |
| `foreignKeyData` | absent | not accepted for this row | foreign-key columns require a separate row |

## Future Reconsideration Rule

DOM fields, `event`, `column`, `cancel`, row-template clicks, command-column
clicks, checkbox cells, grouped rows, frozen columns, virtualization, and
foreign-key data may be reconsidered only through focused rows that prove a
distinct typed DSL use case and behavior proof.
