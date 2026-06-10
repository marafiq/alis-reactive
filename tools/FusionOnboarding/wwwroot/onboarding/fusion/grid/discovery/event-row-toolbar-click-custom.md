# Grid Event Row: toolbarClick Custom Item

Status: discovery, mapping, and focused typed DSL Playwright proof passed for
this row. The component audit remains open.

## Row Boundary

`toolbarClick` fired by clicking a custom Grid toolbar button.

This row covers a normal custom toolbar button rendered in the Grid toolbar.
Built-in toolbar actions, search toolbar behavior, column chooser, responsive
toolbar overflow, cancel mutation/default-action prevention, disabled items,
template items, keyboard activation, and adaptive toolbar menus require
separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local toolbar event type: `node_modules/@syncfusion/ej2-navigations/src/toolbar/toolbar.d.ts:52`
- Syncfusion local Grid toolbar trigger: `node_modules/@syncfusion/ej2-grids/src/grid/actions/toolbar.js:493`
- Syncfusion local Grid toolbar post-trigger default action switch: `node_modules/@syncfusion/ej2-grids/src/grid/actions/toolbar.js:504`
- Custom toolbar probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-toolbar-click-custom.html`
- Custom toolbar trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-toolbar-click-custom.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-toolbar-click-custom.md`

## Discovery Result

The probe instantiates EJ2 Grid directly with a custom toolbar item, waits for
that item to render, clicks it, and records the `toolbarClick` payload.

The deterministic trace file hash was stable across reruns:

`ae497fb3fa79e771e87a11e207b3ad0dea2a8b446e5db6b27b51b3d2fc4648b9`

## Observed Payload

The custom toolbar click variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `cancel` | boolean | `false` | accepted as read flag for this row |
| `item` | object | toolbar item object | accepted selectively through `Item.Id` and `Item.Text` |
| `item.id` | string | `emailStatements` | accepted |
| `item.text` | string | `Email Statements` | accepted |
| `item.tooltipText` | string | `Email statements` | discovered but excluded for this row |
| `item.prefixIcon` | string | `e-icons e-send-1` | discovered but excluded for this row |
| `item.suffixIcon` | string | empty string | discovered but excluded for this row |
| `item.disabled` | boolean | `false` | discovered but excluded for this row |
| `item.visible` | boolean | `true` | discovered but excluded for this row |
| `item.type` | string | `Button` | discovered but excluded for this row |
| `item.align` | string | `Left` | discovered but excluded for this row |
| `name` | string | `toolbarClick` | accepted |
| `originalEvent` | PointerEvent | click event targeting the toolbar button | excluded browser-owned object |

## C# DSL Judgment Boundary

Public C# accepts only the stable typed command identity and event metadata:

- `FusionGridToolbarClickArgs.Item`
- `FusionGridToolbarItem.Id`
- `FusionGridToolbarItem.Text`
- `FusionGridToolbarClickArgs.Cancel`
- `FusionGridToolbarClickArgs.Name`

Do not add public `OriginalEvent` from this row. It is a browser-owned
`PointerEvent`.

Do not add toolbar item `TooltipText`, `PrefixIcon`, `SuffixIcon`, `Disabled`,
`Visible`, `Type`, or `Align` from this row. They are discovered, but this row
does not prove a clear C# DSL behavior need beyond branching by stable command
id and showing/using command text.

Do not claim cancel mutation/default-action prevention from this row. The trace
proves the `cancel` flag is present and readable. A built-in toolbar action row
must prove mutation semantics before the DSL claims cancellation behavior.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnToolbarClick.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted custom toolbar row:

- `FusionGridEvents.ToolbarClick` maps event name `toolbarClick`
- `FusionGridToolbarClickArgs.Item.Id` maps `item.id`
- `FusionGridToolbarClickArgs.Item.Text` maps `item.text`
- `FusionGridToolbarClickArgs.Cancel` maps `cancel`
- `FusionGridToolbarClickArgs.Name` maps `name`

## Typed DSL Proof

Passed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow"`

TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182943.trx`

The proof clicks the real custom toolbar item through the typed Fusion page,
branches on `args.Item.Id`, asserts visible `args.Item.Id`, `args.Item.Text`,
`args.Cancel`, and `args.Name`, and asserts no console errors.
