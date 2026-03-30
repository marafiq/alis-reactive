---
name: bdd-test-session
description: Progress of BDD Playwright test redesign session — 1,034 tests, 297 Playwright, zero DSL changes
type: project
---

BDD Playwright test redesign completed across this session.

**Why:** User plans major refactors and needs test harness that catches real regressions.
Shallow "page loads" tests replaced with deep multi-step user journey scenarios.

**How to apply:** All future Playwright tests must follow the deep BDD pattern:
- snake_case method names (all lowercase)
- Multi-step user journeys, not single assertions
- Test user intent, not framework plumbing
- State-cycle tests (set→clear→reset) catch stale-state bugs
- Exact payload round-trip verification for HTTP tests
- CSS class coherence checks (AddClass/RemoveClass ordering)
- WhileLoading spinner lifecycle verification
- Cross-vendor tests prove vendor-agnostic architecture

**Key numbers:** 57 commits, 1,034 total tests (297 Playwright), 55s parallel at 8 fixtures.
Zero DSL/TS/schema/descriptor changes. All constraints honored.

**Components onboarded:** NativeDatePicker (DateTime typed access), FusionDatePicker, FusionTimePicker, FusionComboBox.

**Remaining work:** Conditions page still has 55 raw onclick buttons (view fix needed).
Remaining component onboarding: NativeTimePicker, NativeNumericInput, NativeTextArea, NativeRadioButton, FusionDateTimePicker, FusionDateRangePicker, FusionMultiSelectDropdown, FusionInputMask, FusionColorPicker, FusionRichTextEditor.
