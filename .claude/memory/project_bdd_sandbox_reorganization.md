---
name: BDD Sandbox Reorganization
description: Major initiative to reorganize Sandbox views and Playwright tests into concern-based hierarchy with BDD behavior-focused naming — status and remaining work
type: project
---

## Status: P2 Done, P3 Partially Done — ej2_instances removal IN PROGRESS

**PR:** https://github.com/marafiq/alis-reactive/pull/9
**Branch:** `feature/sandbox-bdd-reorganization`
**Worktree:** `.worktrees/sandbox-bdd-reorg/`
**Total tests:** 2,305 (1092 TS + 543 C# + 670 Playwright) — ALL PASS

## Completed

- Phase 1-4: Full reorganization (39 controllers, ~50 models, 57 views, 50 tests) ✓
- Phase 5: BDD enhancement — 107 new tests ✓
- Phase 6: Blind BDD review — 5 reviewers, 209 tests reviewed ✓
- P1 fixes: 8 implementation tests deleted, 5 rewritten to behavior ✓
- P2 fixes: PostData→echo-panel (3), DispatchEvent→Blur (1), test renames (2) ✓
- 8 Fusion locator classes created + PagePlan factory methods ✓
- Drawer tests (7) + Todo tests (7) — new test files ✓
- WhenMultiFieldFormSubmits: ej2_instances→real NumericTextBox gestures (2 fixes) ✓
- Cleanup: 4 orphaned files deleted, DatePicker/ renamed to FusionDatePicker/ ✓

## Remaining: ej2_instances Removal (HARD RULE: no scripts, real interactions only)

### ComponentGather `FillAllRequiredFields()` — 22 tests affected
SF date/time pickers don't accept typed values via Playwright `FillAsync` or `PressSequentiallyAsync`.
SF components only update their internal `ej2_instances[0].value` through their OWN popup UI.
**Need:** Proper popup-based calendar locators:
- DatePicker: click calendar icon → navigate month → click date cell
- TimePicker: click time icon → select from time list popup
- DateTimePicker: combined calendar + time popup
- DateRangePicker: dual calendar popup → click start → click end
- MultiColumnComboBox: open popup → click row in multi-column grid
- InputMask: investigate if PressSequentially works with mask pattern active
- RichTextEditor: contenteditable selector fixed (`xpath=..` to parent, `[contenteditable='true']`)
- MultiSelect: popup ID uses shortened name (not full scope) — locator fixed

### Cascading DropDownList — 20 tests use `showPopup()` via ej2
SelectCountry/SelectCity helpers use `EvaluateAsync` to call `showPopup()`.
Wrapper click + type + Enter was flaky (5/20 failures).
**Need:** Find reliable real-gesture for SF DropDownList open (test manually first).

### Cross-vendor validation test — still uses `WaitForTimeoutAsync(500)`
Need positive assertion before absence check.

### Drawer X button — test removed, needs timing investigation
The `#alis-drawer-close` button exists but Playwright click times out.
May need to wait for CSS transition to complete.

## Key Learning: SF Components and Playwright

**CRITICAL:** Syncfusion EJ2 components do NOT set their internal `value` property from:
- `FillAsync` (programmatic fill — sets input.value but not ej2.value)
- `PressSequentiallyAsync` (keystrokes — SF doesn't parse typed dates on blur)
- Standard DOM events (input, change)

The ONLY ways to set ej2 value:
1. `ej2_instances[0].value = x; dataBind()` — **ANTI-PATTERN, violates BDD Rule 4**
2. Use the component's POPUP UI — click calendar, navigate, select date — **CORRECT BDD approach**

**User mandate:** Scripts are anti-pattern. Playwright BDD tests MUST use real browser interactions. No exceptions. No hacks.

## Rules for Next Session

- `memory/feedback_bdd_constitution.md` — 5 rules + cardinal rule
- `memory/feedback_bdd_framework_primitives.md` — framework primitives in views
- **Test manually in browser FIRST** before writing Playwright locator code
- Build popup-based calendar interaction, verify in headed browser, THEN automate
- All tests must pass before any commit
