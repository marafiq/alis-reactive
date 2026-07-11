# FusionAccordion Typed API Coverage Matrix

Status: audited.

Generated from current public typed Fusion API under:

```text
Alis.Reactive.Fusion/Components/FusionAccordion
```

This matrix is fail-closed. A row with `unproven`, `pending`, or a missing
trace/mapping/Playwright link means the component is not audited.

| Public API | Kind | Source | Raw Trace Row | Primitive Map Row | Vertical Slice Row | Playwright DSL Proof | Status |
|---|---|---|---|---|---|---|---|
| `FusionAccordionExpandedArgs` | event-payload-contract | `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.opening_the_care_team_section_shows_its_content_and_names_it_as_open` | row-proven |
| `FusionAccordionExpandedArgs.Index` | event-payload-property | `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.opening_a_different_section_names_that_section_not_the_first` | row-proven |
| `FusionAccordionExpandedArgs.IsExpanded` | event-payload-property | `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.closing_the_open_section_shows_no_section_open` | row-proven |
| `Expanded` | event-selector | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.opening_the_care_team_section_shows_its_content_and_names_it_as_open` | row-proven |
| `EnableItem(this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)` | method | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.confirming_the_care_agreement_unlocks_the_agreement_section` | row-proven |
| `ExpandItem(this ComponentRef<FusionAccordion, TModel> self, bool isExpand, int index)` | method | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.opening_my_care_plan_summary_expands_the_care_team_section` | row-proven |
| `FusionAccordion(#if NET48 this HtmlHelper<TModel> html, #else this IHtmlHelper<TModel> html, #endif ReactivePlan<TModel> plan, string elementId, Action<AccordionBuilder> build)` | method | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.the_care_plan_opens_showing_its_three_sections` | row-proven |
| `Reactive` | event-selector | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionReactiveExtensions.cs` | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | [behavioral-coverage.json](behavioral-coverage.json) | `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion.opening_the_care_team_section_shows_its_content_and_names_it_as_open` | row-proven |
