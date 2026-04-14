# P5 — Manual Sandbox Dogfooding Checklist

**Date drafted:** 2026-04-14
**Branch:** `pre/1.0-cleanup`
**Estimated time:** 30 minutes
**Prerequisites:** sandbox app builds and runs locally on `http://localhost:5220`
**Goal:** "Would I stake a resident's medication schedule on this today?" — if you hesitate on any of the 5 scenarios, do not cut 1.0 yet.

This is the synthetic-tests-aren't-enough gate. P1-P4 prove the framework is correct on the things we wrote tests for. P5 proves the framework is correct on the things we forgot to write tests for. 30 minutes of human dogfooding catches 80% of integration bugs that synthetic tests miss.

## Setup (2 min)

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/pre-1.0-cleanup
lsof -ti:5220 | xargs kill -9 2>/dev/null
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
dotnet run --project Alis.Reactive.SandboxApp
```

Open Chrome (or whatever browser you usually run). Open DevTools (Cmd+Option+I). Switch to the **Console** tab. Make sure "Preserve log" is checked. Watch console + network throughout.

**Pass criterion for every scenario**: zero console errors, zero unexpected network 4xx/5xx, zero "did the click do nothing?" moments. If anything looks weird, stop and write down what you saw before continuing.

---

## Scenario 1 — Validation: required fields + cross-field conditions (5 min)

**Page**: `http://localhost:5220/Sandbox/Validation/AllRules`

**What to verify**: validation extraction + render-time enrichment + live-clear + submit gate. This exercises the whole validation pipeline that depends on the P1c locked nullable table (every nullable property in `Component`, `Request`, `ValidationRule`).

**Steps**:
1. Click **Submit** on the empty form. Required-field errors should appear inline next to each empty input.
2. Type into the first required field. The error for that field should clear on blur (or as you type, depending on the rule).
3. Toggle any conditional checkbox/dropdown that reveals additional fields. The newly-revealed fields should immediately participate in validation.
4. Toggle the same control back. The hidden fields' validation errors should clear.
5. Fill the form completely (with valid data). Submit. The submit should succeed (no inline errors, success state visible).

**What would fail this scenario** (but might pass synthetic tests):
- Stale validation errors that don't clear after fixing the input
- Conditional fields that don't re-validate when revealed
- Submit succeeding with an invalid field that wasn't recognized as required
- Console errors mentioning "shape", "kind", or "undefined property"

---

## Scenario 2 — HTTP pipeline: GET + chained + parallel + onAllSettled (7 min)

**Page**: `http://localhost:5220/Sandbox/HttpPipeline/Http`

**What to verify**: the entire HTTP request pipeline — single GET, chained requests (Next), parallel + OnAllSettled (which now uses the P1b NoOpReaction default), error handlers, while-loading spinners. This is the most likely place a P1b regression would hide.

**Steps**:
1. Click the **GET** button. Watch the network tab — exactly one request should fire. The response body should populate the result panel.
2. Click the **Chained GET** button. Watch the network tab — two requests should fire in sequence (the second only after the first completes).
3. Click the **Parallel GET** button. Both requests should fire concurrently. The OnAllSettled reaction should fire once both complete (verify via the on-screen "all done" indicator or whatever the page surfaces).
4. Click any **Error 422** button. The 422 handler should fire (warning class applied somewhere visible). The default success handler should NOT fire.
5. Click any button that has a `WhileLoading` spinner. The spinner should appear during the request and disappear after.

**What would fail this scenario**:
- Network tab showing extra requests (proves the runtime is dispatching twice)
- OnAllSettled never firing (proves NoOpReaction handling broke)
- 422 falling through to the success handler (proves error routing broke)
- Spinner stuck on after the request completes (proves WhileLoading state machine broke)
- Console errors mentioning "kind" or "noop"

---

## Scenario 3 — Conditional UI: show/hide based on form values (5 min)

**Page**: `http://localhost:5220/Sandbox/Validation/Contract` (or any Validation/* page that uses conditional reveal)

**What to verify**: condition evaluation + live re-evaluation when source values change. Exercises the `Condition` family the P3 wire format freeze test pinned.

**Steps**:
1. Find a checkbox or dropdown that controls the visibility of another section (e.g., "Has emergency contact" → reveals contact fields).
2. Toggle the control on. The dependent section should reveal immediately.
3. Toggle the control off. The dependent section should hide immediately.
4. Toggle on, fill the dependent fields with valid data, then toggle off. The dependent fields' values should not affect form validity (they're hidden — their rules should not fire).
5. Repeat 3 times to verify the toggle is reliable, not "works once."

**What would fail this scenario**:
- Section visible but inputs are disabled (half-state)
- Hidden section's required fields blocking submit
- Condition only fires the first time, then stops re-evaluating

---

## Scenario 4 — Multi-partial plan (5 min)

**Page**: any page that uses partial views with their own reactive plans. `http://localhost:5220/Sandbox/AllModulesTogether/Todo` is a candidate.

**What to verify**: partial plan merging — multiple plans on one page, each with its own `planId`, all merged at boot time without collision. Exercises the P3 plan envelope wire format (partId).

**Steps**:
1. Load the page. Check the DOM for multiple `<script type="application/json" data-reactive-plan>` blocks.
2. Trigger an interaction in one partial. The interaction should affect only that partial, not the others.
3. Trigger an interaction in a second partial. Same — isolated.
4. Refresh the page. Both partials should re-initialize cleanly (no double-binding, no leaked event handlers).

**What would fail this scenario**:
- An action in one partial firing reactions in another (shared state leak)
- Console error: "duplicate planId" or "reaction already bound"
- Refresh leaving stale handlers from the previous load

---

## Scenario 5 — Component slice: Fusion DropDownList or NativeRadioGroup with full event lifecycle (6 min)

**Page**: `http://localhost:5220/Sandbox/Components/Fusion/DropDownList` or `http://localhost:5220/Sandbox/Components/Native/NativeRadioGroup`

**What to verify**: a real component slice from open → select → change → blur → gather → submit. Exercises the `Component` wire format (P1c locked BindingPath/ValueMember/Container nullables) and the gather pipeline.

**Steps**:
1. Click into the component. The dropdown should open / radio group should focus.
2. Select a value. The on-screen echo (if the page has one) should update immediately.
3. Click outside to blur. No console error, no spurious request.
4. Submit a form that contains this component. Verify the gather payload (in DevTools network → request body) contains the selected value at the right field name.
5. Re-open and select a different value. Re-submit. Verify the new value is in the payload.

**What would fail this scenario**:
- Echo shows the component ID instead of the value (proves ValueMember resolution broke)
- Gather payload missing the field (proves BindingPath broke)
- Gather payload has the field name as the component ID instead of the model property name
- Console error mentioning "vendor" or "resolver"

---

## After all 5 scenarios

If every scenario passed cleanly with **zero console errors, zero unexpected network calls, zero "did that click do anything?" moments**:

- Mark P5 done in CLAUDE.md / the task tracker.
- Cut the 1.0-rc.1 tag.
- Publish to NuGet as `1.0.0-rc.1`.

If anything failed or felt off:

- Write down EXACTLY what you saw (the click, the expected behavior, the actual behavior, any console messages).
- Do NOT cut 1.0-rc.1. The whole point of P5 is "if you hesitate, don't ship."
- Open an issue or commit a regression test that reproduces the bug.
- Fix the underlying issue (no patches — root cause per CLAUDE.md).
- Re-run P5 once the fix lands.

## Time budget

- Setup: 2 min
- Scenario 1 (Validation): 5 min
- Scenario 2 (HTTP pipeline): 7 min
- Scenario 3 (Conditional UI): 5 min
- Scenario 4 (Multi-partial): 5 min
- Scenario 5 (Component slice): 6 min
- **Total: 30 min**

If a scenario takes longer than budget because you found an issue, that's fine — the discovery is the win. Don't rush past a "huh that's weird" moment.
