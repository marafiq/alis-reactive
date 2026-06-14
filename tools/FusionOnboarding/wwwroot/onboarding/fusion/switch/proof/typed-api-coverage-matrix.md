# FusionSwitch Typed API Coverage Matrix

Status: unproven.

Generated from current public typed Fusion API under:

```text
Alis.Reactive.Fusion/Components/FusionSwitch
```

This matrix is fail-closed. A row with `unproven`, `pending`, or a missing
trace/mapping/Playwright link means the component is not audited.

| Public API | Kind | Source | Raw Trace Row | Primitive Map Row | Vertical Slice Row | Playwright DSL Proof | Status |
|---|---|---|---|---|---|---|---|
| `FusionSwitchChangeArgs` | event-payload-contract | `Alis.Reactive.Fusion/Components/FusionSwitch/Events/FusionSwitchOnChanged.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.turning_care_alerts_off_tells_the_resident_alerts_are_paused` | row-proven |
| `FusionSwitchChangeArgs.Checked` | event-payload-property | `Alis.Reactive.Fusion/Components/FusionSwitch/Events/FusionSwitchOnChanged.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.turning_care_alerts_back_on_tells_the_resident_to_pick_channels` | row-proven |
| `FusionSwitchChangeArgs.IsInteracted` | event-payload-property | `Alis.Reactive.Fusion/Components/FusionSwitch/Events/FusionSwitchOnChanged.cs` | pending | pending | pending | pending | unproven |
| `Changed` | event-selector | `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchEvents.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.turning_care_alerts_off_tells_the_resident_alerts_are_paused` | row-proven |
| `SetChecked(this ComponentRef<FusionSwitch, TModel> self, bool isChecked)` | method | `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.pausing_all_alerts_turns_the_master_switch_off` | row-proven |
| `Value(this ComponentRef<FusionSwitch, TModel> self)` | method | `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.saving_posts_each_switch_value_to_the_server` | row-proven |
| `FusionSwitch(this InputBoundField<TModel, bool> setup, Action<SwitchBuilder> build)` | method | `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchHtmlExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.preferences_open_showing_the_resident_saved_toggles` | row-proven |
| `Reactive` | event-selector | `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchReactiveExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Switch.WhenSwitchToggles.turning_care_alerts_off_tells_the_resident_alerts_are_paused` | row-proven |
