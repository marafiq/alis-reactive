# Pure Gold Refactor Tasks

Sidecar-verified follow-up tasks only; no implementation in this branch unless explicitly selected later.

## Inventory Syncfusion Locator Knowledge Before Extraction
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/DateTimePickerLocator.cs:30` tag-qualified popup locators are required because Syncfusion reuses `#{id}_options`.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/MultiSelectLocator.cs:9` MultiSelect selection does not raise `change` until blur.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/SwitchLocator.cs:19` the clickable switch target is a wrapper because the component ID belongs to a hidden checkbox.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/FusionSplitButtonLocator.cs:44` class-state probes are part of the component test surface, not just raw locator convenience.
- Developer pain: The extension assembly contains real Syncfusion Playwright interaction knowledge, but it is spread across component locators so a maintainer cannot see the reusable rules before editing or extracting them.
- Refactor task: Add a short Syncfusion Playwright locator pattern inventory that classifies current locators by popup ID convention, click target, commit event, hidden/native input relationship, class-state probe, and first safe extraction candidate.
- INVEST check: Independent=yes, design-only and can be done before code moves; Negotiable=yes, exact file/location and column names can change; Valuable=makes later extraction auditable instead of speculative; Estimable=small, one inventory pass over locator files; Small=no behavior or API changes; Testable=review check that the inventory cites each `*Locator.cs` group it classifies and distinguishes Syncfusion rules from Alis Reactive Plan lookup rules.
- Non-goals: Do not create an external package, move code, rename locators, or normalize component behavior in this task.

## Separate Alis Adapters From Syncfusion Locators
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/PagePlan.cs:25` reads emitted Reactive Plan JSON before resolving component locators.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/PagePlan.cs:61` exposes model-expression helpers that instantiate Syncfusion locators from resolved component IDs.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/ComponentScope.cs:9` encodes the framework-specific `{TypeScope}__{PropertyName}` component ID format.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/FusionComboBoxLocator.cs:13` the locator itself only needs `IPage` plus `componentId`.
- Developer pain: Reusable Syncfusion Playwright primitives and Alis-specific plan/binding adapters currently live at the same level, so a future library extraction could accidentally publish framework ID and plan JSON assumptions as component API.
- Refactor task: Split the test helper assembly internally into an Alis adapter layer (`PagePlan`, `ComponentScope`, binding-path helpers) and component-ID-based Syncfusion locator layer, preserving existing test call sites or adding thin compatibility aliases.
- INVEST check: Independent=yes, boundary cleanup can happen without changing popup or calendar behavior; Negotiable=yes, folder names, namespaces, and compatibility shape can be chosen later; Valuable=clarifies what is reusable outside this repo and what is framework glue; Estimable=small-to-medium, mostly file moves/namespace/API visibility review; Small=no runtime or generated plan changes; Testable=run `scripts/playwright.sh --filter "FullyQualifiedName~WhenPlanDrivesComponentDiscovery|FullyQualifiedName~WhenAllComponentsGatherIntoOnePost"` and review that locator constructors still accept explicit component IDs.
- Non-goals: Do not publish a package, change Reactive Plan JSON parsing, change component ID generation, or rewrite component tests broadly.

## Extract First Popup List Selection Primitive
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/FusionComboBoxLocator.cs:35` waits for popup visibility, waits for an exact item, clicks stably, then waits for the popup to hide.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/AutoCompleteLocator.cs:59` repeats the same visible item selection and hidden-popup sequence.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/DropDownListLocator.cs:34` adds the same sequence with an extra body click because Syncfusion keeps popup/focus state internally.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/FusionDropDownTreeLocator.cs:36` shares the same timing pattern but needs a tree-row click target instead of a plain list item.
- Developer pain: Popup selection timing and stable-click behavior are duplicated across list-like controls, so fixing one flaky Syncfusion interaction requires remembering every local variant.
- Refactor task: Introduce one internal popup-list selection helper for simple list controls and apply it first to AutoComplete, ComboBox, and DropDownList while leaving DropDownTree and MultiSelect variants explicit.
- INVEST check: Independent=yes, covers one locator family without touching calendar or adapter boundaries; Negotiable=yes, helper can accept locator factories or small options; Valuable=centralizes the highest-repeat Syncfusion popup timing rule; Estimable=small, three locator updates plus proof tests; Small=excludes tree and multi-select edge cases from the first slice; Testable=run `scripts/playwright.sh --filter "FullyQualifiedName~WhenAutoCompleteSuggests|FullyQualifiedName~WhenAutoCompleteFiltersRemotely|FullyQualifiedName~WhenDropdownItemSelected|FullyQualifiedName~WhenUsingFusionComboBox"`.
- Non-goals: Do not hide component-specific popup suffixes, include DropDownTree or MultiSelect in the first extraction, or change test assertions.

## Extract Calendar Month Navigation Primitive
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/DatePickerLocator.cs:66` implements month navigation with title parsing, prev/next clicks, and title-change polling.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/DateTimePickerLocator.cs:88` repeats the same calendar navigation logic for the date portion of DateTimePicker.
- Evidence: `tests/Alis.Reactive.Playwright.Extensions/DateRangePickerLocator.cs:105` repeats the month-navigation loop again while adding range-specific left/right calendar and apply-button behavior.
- Developer pain: Calendar navigation flake fixes or title parsing changes must be copied across date components, but DateRangePicker also has enough extra behavior that blind deduplication would be risky.
- Refactor task: Extract an internal Syncfusion calendar navigator for month title parsing, prev/next navigation, title-change waiting, and day-cell selection; apply it first to DatePicker and DateTimePicker, leaving DateRangePicker range/apply behavior for a later slice.
- INVEST check: Independent=yes, date-only extraction can be proven without list controls; Negotiable=yes, helper name and day-selection API can be shaped during implementation; Valuable=removes duplicated Syncfusion calendar timing logic; Estimable=small, two locator updates and one shared helper; Small=does not change DateRangePicker in the first pass; Testable=run `scripts/playwright.sh --filter "FullyQualifiedName~WhenDateSelected|FullyQualifiedName~WhenDateTimeSelected"`.
- Non-goals: Do not change date formats, popup suffixes, range apply timing, or typed-text fallback behavior.
