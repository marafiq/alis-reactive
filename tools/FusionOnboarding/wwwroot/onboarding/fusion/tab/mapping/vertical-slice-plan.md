# Tab Vertical Slice Plan

Status: active and proven. Every accepted `FusionTab` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds
zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionTab(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<TabBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabHtmlExtensions.cs` | Tab navigation render row |
| `FusionTabEvents.Selected` | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs` | `selected` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabReactiveExtensions.cs` | `selected` event trigger row |
| `FusionTabSelectedArgs.SelectedIndex` | `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs` | `selected.selectedIndex` payload read row |
| `FusionTabSelectedArgs.PreviousIndex` | `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs` | `selected.previousIndex` payload read row |
| `FusionTabSelectedArgs.IsSwiped` | `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs` | `selected.isSwiped` payload read row |
| `Select(this ComponentRef<FusionTab, TModel> self, int index)` | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs` | `select(index)` method row |
| `HideTab(this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)` | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs` | `hideTab(index, value)` method row |
| `SetSelectedItem(this ComponentRef<FusionTab, TModel> self, int index)` | `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs` | `selectedItem` property write row |
| component identity (vendor, registration) | `Alis.Reactive.Fusion/Components/FusionTab/FusionTab.cs` | n/a (component contract) |

## Slice File Inventory

The Tab slice follows the component isolation pattern. It does not move behavior
into shared base classes; duplication with other slices is intentional. Tab is a
non-input navigation component, so it has no input-field wrapper, no `Value()`
read, and no `SetValue()`.

- `Alis.Reactive.Fusion/Components/FusionTab/FusionTab.cs` — the sealed `FusionTab : FusionComponent` (non-input) and its component registration.
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabBuilder.cs` — the rendered-markup carrier that holds the `ReactivePlan` and `ElementId` for event wiring.
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabHtmlExtensions.cs` — the `FusionTab(...)` render helper that renders the `TabBuilder` and carries the controlled `elementId` into the plan.
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs` — the post-render members `Select`, `HideTab`, and `SetSelectedItem`.
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs` — the `Selected` event selector.
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs` — the typed `FusionTabSelectedArgs` payload with `SelectedIndex`, `PreviousIndex`, and `IsSwiped`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/TabController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tab/TabModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/Tab`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Tab/WhenTabSwitches.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Tab.WhenTabSwitches`
- Typed locator: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Tab/FusionTabLocator.cs`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Tab row is sync: the navigation render is sync component registration; the
`selected` component event trigger is sync; `Select`, `HideTab`, and
`SetSelectedItem` are sync component actions. The slice introduces no async
boundary of its own; async appears only when a developer composes a read payload
member into an HTTP pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`allowDragAndDrop`, `animation`, `cssClass`, `headerPlacement`, `items`, `loadOn`, `overflowMode`, `swipeMode`, and the rest).
- The cancelable pre-events (`selecting`), collection-mutation events (`added`, `adding`, `removed`, `removing`), drag events, the `created`/`destroyed` lifecycle events, the vendor refresh/enable family, and the `destroy` lifecycle method.
