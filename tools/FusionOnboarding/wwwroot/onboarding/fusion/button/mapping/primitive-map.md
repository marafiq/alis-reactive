# FusionButton Authoritative Primitive Map

Status: audited. Every accepted public member maps to an existing DSL primitive.
Component onboarding adds no primitive. Each row links back to the raw EJ2 trace
row (`traces/raw-ej2-core.trace.json`) and forward to the typed C# member.

`FusionButton` is a non-input display/action component: it does not register a
form binding. It is referenced by its developer-chosen DOM id through
`p.Component<FusionButton>(elementId)`.

| JS object behavior | EJ2 evidence (trace row) | DSL primitive | Typed C# member |
|---|---|---|---|
| `button.content = v; dataBind()` | `content after set` (DOM text becomes the value) | `ComponentProperty<string>` + `self.EmitSet(property, ValueExpression)` + `EmitCall(dataBind)` | `SetContent(string)` |
| read `button.content` | `content after set` (`property` reads back) | `ComponentProperty<string>` + `self.Read(property)` | `Content()` |
| `button.disabled = v; dataBind()` | `disabled after set true` / `disabled after set false` (DOM `disabled` follows) | `ComponentProperty<bool>` + `self.EmitSet(...)` + `EmitCall(dataBind)` | `SetDisabled(bool)` |
| read `button.disabled` | `disabled after set ...` (`property` reads back) | `ComponentProperty<bool>` + `self.Read(property)` | `Disabled()` |
| `button.iconCss = c; button.iconPosition = p; dataBind()` | `icon after set` (`iconSpanClass` becomes `e-check e-icon-right`) | two `ComponentProperty<string>` writes (`iconCss`, `iconPosition`) + `EmitCall(dataBind)` | `SetIcon(string, FusionButtonIconPosition)` |
| `button.cssClass = v; dataBind()` | `cssClass after set` (DOM class gains the value) | `ComponentProperty<string>` + `self.EmitSet(...)` + `EmitCall(dataBind)` | `SetCssClass(string)` |
| read `button.cssClass` | `cssClass after set` (`property` reads back) | `ComponentProperty<string>` + `self.Read(property)` | `CssClass()` |
| `button.isPrimary = v; dataBind()` | `isPrimary after set` (`hasPrimaryClass` true) | `ComponentProperty<bool>` + `self.EmitSet(...)` + `EmitCall(dataBind)` | `SetPrimary(bool)` |
| read `button.isPrimary` | `isPrimary after set` (`property` reads back) | `ComponentProperty<bool>` + `self.Read(property)` | `IsPrimary()` |
| `button.isToggle = v; dataBind()` | `isToggle after set` (`property` true) | `ComponentProperty<bool>` + `self.EmitSet(...)` + `EmitCall(dataBind)` | `SetToggle(bool)` |
| read `button.isToggle` | `isToggle after set` (`property` reads back) | `ComponentProperty<bool>` + `self.Read(property)` | `IsToggle()` |
| `button.click()` (toggle on -> latches `e-active`) | `click latches active (toggle on)` (`hasActiveClass` true) | `ComponentMethod` + `self.EmitCall(method)` | `Click()` |
| `button.focusIn()` (moves keyboard focus) | `focusIn focuses button` (`documentActiveIsButton` true) | `ComponentMethod` + `self.EmitCall(method)` | `FocusIn()` |
| render the Syncfusion button with a stable component id | `ready` (instantiates `ej.buttons.Button`); prototype methods include `dataBind`, `click`, `focusIn` | Syncfusion `ButtonBuilder` wrapped + component id carrier | `FusionButton(...)` HTML helper |

## Value-source consumer paths

Every read source is consumed by a realistic pipeline in the Daily Wellness
Check-In journey:

- `Content()` — read on DomReady into the visible "Action ready" line, and gathered into the record-check-in POST body under `action`.
- `Disabled()` — read into a `When(...).Eq(true)` condition that routes the readiness message, and gathered under `locked`.
- `CssClass()` — gathered into the POST body under `priority`; the server confirmation reflects it.
- `IsPrimary()` — gathered under `recommended`; the confirmation phrasing reflects it.
- `IsToggle()` — gathered under `followUp`; the confirmation phrasing reflects it.

## No new primitive

`SetContent`, `SetDisabled`, `SetIcon`, `SetCssClass`, `SetPrimary`, and
`SetToggle` are property writes that chain `dataBind()` to repaint — the existing
`EmitSet` + `EmitCall` pattern. `Click` and `FocusIn` are void method calls
(`EmitCall`). `Content`, `Disabled`, `CssClass`, `IsPrimary`, `IsToggle` are
property reads (`Read`). No condition, gather, plugin, or array primitive was
added, removed, renamed, or broadened.
