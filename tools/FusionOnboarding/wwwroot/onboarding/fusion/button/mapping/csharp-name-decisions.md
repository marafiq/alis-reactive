# FusionButton C# Name Decisions

Status: audited. One row per public C# member, each grounded in the exact EJ2
runtime name (`node_modules/@syncfusion/ej2-buttons/src/button/button.d.ts`) and
confirmed against the raw trace (`traces/raw-ej2-core.trace.json`). No Syncfusion
Blazor package was inspected this pass (`discovery/blazor-candidates.md` is
`not-requested`), so the EJ2 JS names are the authoritative source for the C#
vocabulary — no bridge-only Blazor behavior was imported.

| C# member | EJ2 runtime name | Kind | Why this name |
|---|---|---|---|
| `SetContent(string)` | `content` (write) | property write | `Set` + the EJ2 property name; writes the visible label and `dataBind()`s. |
| `Content()` | `content` (read) | property read | the EJ2 property name as a typed `string` source. |
| `SetDisabled(bool)` | `disabled` (write) | property write | `Set` + the EJ2 property name; writes the enabled state. |
| `Disabled()` | `disabled` (read) | property read | the EJ2 property name as a typed `bool` source. |
| `SetIcon(string, FusionButtonIconPosition)` | `iconCss` + `iconPosition` (write) | property write | one typed method writes both icon properties together; the enum mirrors EJ2 `IconPosition` (`Left/Right/Top/Bottom`). |
| `SetCssClass(string)` | `cssClass` (write) | property write | `Set` + the EJ2 property name. |
| `CssClass()` | `cssClass` (read) | property read | the EJ2 property name as a typed `string` source. |
| `SetPrimary(bool)` | `isPrimary` (write) | property write | `SetPrimary` reads cleaner than `SetIsPrimary`; maps to EJ2 `isPrimary`. |
| `IsPrimary()` | `isPrimary` (read) | property read | the EJ2 boolean property name as a typed `bool` source. |
| `SetToggle(bool)` | `isToggle` (write) | property write | `SetToggle` reads cleaner than `SetIsToggle`; maps to EJ2 `isToggle`. |
| `IsToggle()` | `isToggle` (read) | property read | the EJ2 boolean property name as a typed `bool` source. |
| `Click()` | `click()` | method | the exact EJ2 method name. |
| `FocusIn()` | `focusIn()` | method | the exact EJ2 JS method name (`button.d.ts:184`), the runtime contract for Alis. |
| `FusionButton(...)` | `ej.buttons.Button` render | HTML helper | wraps the Syncfusion `ButtonBuilder` and carries the component id. |

## Excluded from public C#

- `created` event — builder-owned lifecycle hook; carries a DOM-native `Event`; no focused Senior Living payload use case. The Fusion slice exposes no event surface for Button.
- `destroy()` — lifecycle cleanup, classified `skip` in `discovery/public-api-surface.json`.
- `enableHtmlSanitizer` — builder-owned static configuration (`builder.covered = true`); no post-render read/write proven necessary.
- `locale` — `@private` culture/i18n config (`node_modules/@syncfusion/ej2-buttons/src/button/button.d.ts:96`); the plain Button registers no localized strings, so it changes nothing visible. Excluded with evidence in `discovery/parity-accounting.json`.
