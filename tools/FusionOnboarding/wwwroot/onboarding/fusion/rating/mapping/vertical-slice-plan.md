# Rating Vertical Slice Plan

Status: active and proven. Every accepted `FusionRating` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds
zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionRating(this InputBoundField<TModel, double> setup, Action<RatingBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingHtmlExtensions.cs` | Rating field render row |
| `FusionRatingEvents.ValueChanged` | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingEvents.cs` | `valueChanged` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingReactiveExtensions.cs` | `valueChanged` event trigger row |
| `FusionRatingValueChangedArgs.Value` | `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs` | `valueChanged.value` payload read row |
| `FusionRatingValueChangedArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs` | `valueChanged.previousValue` payload read row |
| `FusionRatingValueChangedArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs` | `valueChanged.isInteracted` payload read row |
| `SetValue(this ComponentRef<FusionRating, TModel> self, double value)` | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs` | `value` property write row plus `dataBind` repaint row |
| `Reset(this ComponentRef<FusionRating, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs` | `reset()` method row |
| `Value(this ComponentRef<FusionRating, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs` | `value` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionRating/FusionRating.cs` | n/a (component contract) |

## Slice File Inventory

The Rating slice follows the input-component isolation pattern. It does not move
behavior into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionRating/FusionRating.cs` — the sealed `FusionRating : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingHtmlExtensions.cs` — the `FusionRating(...)` render helper that registers the input component and renders the `RatingBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs` — the post-render members `SetValue`, `Reset`, and `Value`.
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingEvents.cs` — the `ValueChanged` event selector.
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs` — the typed `FusionRatingValueChangedArgs` payload with `Value`, `PreviousValue`, and `IsInteracted`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Rating/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/RatingController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Rating/RatingModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/Fusion/Rating`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Rating/WhenUsingFusionRating.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Rating.WhenUsingFusionRating`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Rating row is sync: the field render is sync input registration; the
`valueChanged` component event trigger is sync; `SetValue` (property set plus
`dataBind`), `Reset`, and the `Value` read are sync component actions. The slice
introduces no async boundary of its own; async appears only when a developer
composes the `Value()` source into an HTTP pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`allowReset`, `cssClass`, `disabled`, `itemsCount`, `min`, `precision`, `readOnly`, templates, and the rest).
- The per-item render event, the hover event, the `created` lifecycle event, and the `destroy` lifecycle method.
