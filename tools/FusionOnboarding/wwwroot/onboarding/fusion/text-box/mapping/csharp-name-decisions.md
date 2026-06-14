# TextBox C# Name Decisions

Status: active and proven. The `FusionTextBox` public C# names are decided and
implemented: the `FusionTextBox(...)` render helper, the `Input`, `Changed`,
`Focus`, and `Blur` event selectors with their typed payloads
(`FusionTextBoxInputArgs`, `FusionTextBoxChangeArgs`, `FusionTextBoxFocusArgs`,
`FusionTextBoxBlurArgs`), the typed `SetValue(string?)` write, the `FocusIn()`
and `FocusOut()` method calls, the `AddAppendIcon(string)` append-icon call, and
the `Value()` read source. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.PreferredName).FusionTextBox(b => ...)` render helper -> TextBox field bound to a string model property.

Close matrix row: `tb.Reactive(e => e.Input/Changed/Focus/Blur, ...)` -> typed payloads `FusionTextBoxInputArgs`/`FusionTextBoxChangeArgs`/`FusionTextBoxFocusArgs`/`FusionTextBoxBlurArgs`.

Close matrix row: `SetValue(string?)`, `FocusIn()`, `FocusOut()`, `AddAppendIcon(string)`, `Value()` -> typed TextBox runtime members.

## Name Decisions

| EJ2 source name | C# public name | Decision rationale |
| --- | --- | --- |
| `input` event (`InputEventArgs`) | `FusionTextBoxEvents.Input` / `FusionTextBoxInputArgs` | EJ2 event name kept; payload named for the component and event. `value`/`previousValue` become `Value`/`PreviousValue`. |
| `change` event (`ChangedEventArgs`) | `FusionTextBoxEvents.Changed` / `FusionTextBoxChangeArgs` | Selector reads `Changed` (developer voice for "committed text changed"); payload exposes `Value`, `PreviousValue`, `IsInteracted`. |
| `focus` event (`FocusInEventArgs`) | `FusionTextBoxEvents.Focus` / `FusionTextBoxFocusArgs` | EJ2 name kept; payload exposes the `Value` present at focus. |
| `blur` event (`FocusOutEventArgs`) | `FusionTextBoxEvents.Blur` / `FusionTextBoxBlurArgs` | EJ2 name kept; payload exposes the `Value` present at blur. |
| `value` property | `SetValue(string?)` / `Value()` | Write paired with `dataBind` repaint; read returns a typed `string` source. Matches the established Fusion verb pair. |
| `focusIn()` method | `FocusIn()` | Direct EJ2 method, PascalCased. |
| `focusOut()` method | `FocusOut()` | Direct EJ2 method, PascalCased. |
| `addIcon(position, icons)` method | `AddAppendIcon(string iconCssClass)` | `ComponentMethod.Mapped("addAppendIcon","addIcon")` fixes the `"append"` position so the public API carries a single typed intent instead of an open `position` string. |

## Excluded Name Decisions

| EJ2 source name | Decision | Reason |
| --- | --- | --- |
| `addAttributes({[k]:string})` | no public C# name | arbitrary string->string attribute dictionary; stringly surface barred from a typed slice (`discovery/parity-accounting.json`). |
| `removeAttributes(string[])` | no public C# name | arbitrary attribute-name string array; stringly surface barred from a typed slice (`discovery/parity-accounting.json`). |
| `readonly` | no public C# name | builder-owned initial render; runtime toggle deferred, the disable case served by builder-owned `enabled` (`discovery/parity-accounting.json`). |
| `created`, `destroyed`, `destroy()` | no public C# name | lifecycle-only; `destroy` classified `skip` in `discovery/public-api-surface.json`; no typed payload. |
| builder-owned properties (`placeholder`, `showClearButton`, `floatLabelType`, `multiline`, `cssClass`, `enabled`, `width`, templates, `autocomplete`, `type`, `enablePersistence`, initial `value`) | no runtime C# name | `discovery/public-api-surface.json` marks each `builder.covered = true`; initial render stays on `TextBoxBuilder`. |

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source types: `InputEventArgs`, `ChangedEventArgs`, `FocusInEventArgs`, `FocusOutEventArgs` (events), `TextBox` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md`
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnInput.cs`, `.../FusionTextBoxOnChanged.cs`, `.../FusionTextBoxOnFocus.cs`, `.../FusionTextBoxOnBlur.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextBox/Index.cshtml`

## Name Decision Outcome

Every accepted name is taken from the EJ2 source of record captured in the
discovery artifacts and the raw trace, PascalCased for C#, with the one mapped
name (`AddAppendIcon`) recorded because it fixes the `position` argument. No name
is invented from memory or docs alone.
