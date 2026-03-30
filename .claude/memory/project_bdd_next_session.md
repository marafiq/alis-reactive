---
name: BDD Next Session — Vertical Slice Parallelism Fix
description: 5 flaky Playwright tests that pass individually but fail under parallel execution — need vertical slice refactoring to eliminate shared-state interference
type: project
---

## Status

**PR:** https://github.com/marafiq/alis-reactive/pull/9
**Branch:** `feature/sandbox-bdd-reorganization` in `.worktrees/sandbox-bdd-reorg/`
**Results:** 664/669 Playwright (99.3%), all 1635 non-Playwright pass
**ej2_instances in executable code:** 0

## 4 Failing Tests (exact names from full parallel run 2026-03-23)

### ComponentGather FormData POST (3 tests — 33s timeouts)
File: `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/Workflows/WhenAllComponentsGatherIntoOnePost.cs`
- `form_data_post_echo_shows_facility_id`
- `form_data_post_echo_shows_hidden_fields`
- `form_data_post_echo_shows_resident_name`

All 3 use `FillAllRequiredFields()` + `SubmitFormDataAndWaitForEcho()`. They pass individually
but fail under parallel execution. Need vertical slice refactoring — each test should fill
only the fields IT verifies, not all 13 fields.

### Cascading (1 test — 6s)
File: `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/Cascading/WhenParentSelectionFiltersDependentList.cs`
- `selecting_different_city_updates_selected_city_display`

Passes individually. The DDL ArrowDown fires intermediate change events when navigating
items. Under parallel execution, these intermediate cascades may interfere.
Needs investigation — may need vertical slice refactoring.

## Remaining Steps
- Step 4: Blind BDD review of changed test files
- Step 5: All tests pass + push

## Current Parallelism Config
```csharp
// GlobalUsings.cs
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(8)]
```

## Key Locator Patterns (verified working)
- **DatePicker:** icon click → calendar popup → navigate months → click day cell
- **TimePicker:** icon click → time list popup → click `li[data-value]`
- **DateTimePicker:** `div#{id}_options` for calendar, `ul#{id}_options` for time list
- **DateRangePicker:** icon → dual calendar → click start/end → Apply button
- **DropDownList:** body click (clear focus) → icon click → ArrowDown → Enter
- **MultiSelect:** wrapper click → popup `#{shortName}_popup` → item click → body click (blur to fire change)
- **InputMask:** `FillAsync` (NOT PressSequentially — digits get lost in mask)
- **AutoComplete:** `TypeAndSelect(partial, fullText)` — type + click popup item
- **MCCB:** `Select(text)` — type + Enter
