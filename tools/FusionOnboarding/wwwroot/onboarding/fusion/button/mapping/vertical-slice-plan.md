# FusionButton Vertical Slice Plan

Status: audited. The slice onboards only the post-render runtime surface; initial
render configuration stays on the Syncfusion `ButtonBuilder`. `FusionButton` is a
non-input display/action component, so it implements no input/form registration.

## Slice files and the member each carries

| Vertical slice file | Members | Primitive-map row |
|---|---|---|
| `Alis.Reactive.Fusion/Components/FusionButton/FusionButton.cs` | the `FusionButton : FusionComponent` type (runtime object join target) | — |
| `Alis.Reactive.Fusion/Components/FusionButton/FusionButtonHtmlExtensions.cs` | `FusionButton(...)` render helper (wraps `ButtonBuilder`, carries the component id) | render row |
| `Alis.Reactive.Fusion/Components/FusionButton/FusionButtonBuilder.cs` | the `IHtmlContent` carrier that renders the Syncfusion button and holds the plan + element id | render row |
| `Alis.Reactive.Fusion/Components/FusionButton/FusionButtonExtensions.cs` | `SetContent`, `SetDisabled`, `SetIcon`, `SetCssClass`, `SetPrimary`, `SetToggle`, `Click`, `FocusIn`, `Content`, `Disabled`, `CssClass`, `IsPrimary`, `IsToggle` | the property-write, property-read, and method rows |
| `Alis.Reactive.Fusion/Components/FusionButton/FusionButtonIconPosition.cs` | `FusionButtonIconPosition` enum (`Left/Right/Top/Bottom`) consumed by `SetIcon` | `SetIcon` row |

## Why no event slice

Button's only EJ2 event is `created`, a builder-owned DOM-native lifecycle hook
with no focused Senior Living payload use case. The slice therefore has no
`Events/` directory and no `FusionButtonEvents`/`FusionButtonReactiveExtensions`.
A reactive plan does not subscribe to a Button DOM click through the Fusion slice;
it drives the button's runtime state and methods from other controls. This is
recorded so a later session does not add a Button event without a proven use
case.

## Why non-input

Button emits no form value and binds to no model property. It is referenced by a
developer-chosen DOM id via `p.Component<FusionButton>(elementId)`. There is no
`InputBoundField` overload and no input registration, unlike `FusionRating`.

## Builder-owned, not re-wrapped

`content`, `cssClass`, `disabled`, `iconCss`, `iconPosition`, `isPrimary`,
`isToggle` all have `ButtonBuilder` methods for initial render
(`discovery/mvc-builder-coverage.md`). The slice adds typed reactive writes/reads
ONLY because the Daily Wellness Check-In journey mutates and reads them after
render. `enableHtmlSanitizer`, `enableRtl`, `enablePersistence`, `htmlAttributes`,
and `type` remain builder-only — no post-render behavior was proven for them.
