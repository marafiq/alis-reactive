---
name: bdd-testing
description: >
  Derives Playwright BDD tests from senior-living user journeys for
  Alis.Reactive. One journey = one nested vertical slice: its own domain
  model, view, controller partial, and fixture, exercised through
  PagePlan<TModel> typed locators against real-app elements only. Also routes
  Vitest and NUnit work to their layers. Use when asked to "write a test",
  "add Playwright tests", "test this component", "add a test for this view",
  "fix a failing test", or "why is this test failing".
---

# BDD Testing — Alis.Reactive

> Sensitive to behavior, insensitive to structure. — Kent Beck

Refactoring framework internals must never break a Playwright test. If a
test breaks and the page behavior is unchanged, the test was coupled to
implementation — rewrite it. If the page behavior changed, the test caught a
regression — report the bug. The slice is the test's Arrange; it changes
only with its own journey.

The structure is set; this skill does not re-derive it. The 5 rules,
7-behavior contract, nested-vertical-slice rules, and blind-reviewer protocol
live in `.claude/memory/bdd-principles.md`. Non-negotiables and failure
triage live in `tests/Alis.Reactive.PlaywrightTests/CLAUDE.md`. View rules live in
`Alis.Reactive.SandboxApp/CLAUDE.md`. What follows is the method: journey →
slice → criteria → tests.

## Nested Vertical Slices

The pattern. A suite starts from one senior-living user journey — month-end
billing, a care-ops roster, a respite-stay booking — and owns an isolated
vertical slice, nested under the same concern path in every tree, names
aligned:

```
Areas/Sandbox/Models/Components/Fusion/Grid/BillingModel.cs            journey model
Areas/Sandbox/Controllers/Components/Fusion/GridController.Billing.cs  controller partial
Areas/Sandbox/Views/Components/Fusion/Grid/Billing.cshtml              journey view
tests/.../Components/Fusion/Grid/WhenUsingFusionGridBilling.cs         fixture
```

The Grid slices are the exemplar. The journey name (`Billing`) is the join
key across the four trees — given any one file, the rest of the slice is one
glob away.

A component with many use cases fans out into many journeys, each a full
slice. Grid carries about thirty — Billing, CareOps, Directory,
PrintableRoster — thirty views, thirty fixtures. Billing and CareOps own
their model files and controller partials; the older Grid journeys share
`GridModel.cs` and predate the full pattern — they migrate as touched.
Never one view or one fixture for everything.

- The model belongs to the journey and speaks domain language: residents,
  care levels, wings, monthly rates. Similar shapes across journeys stay
  separate types (root Rule 4: duplication over abstraction).
- The view is the journey's own page. Dialog and drawer flows on a page
  belong to that page's journey.
- The data is the journey's own, seeded per browser context; no shared
  fake-data class. Parallel tests never collide.
- The fixture exercises that journey's view only.

The isolation contract and the screenshot test live in `bdd-principles.md`;
the blind reviewer checks both.

## Real-App Elements Only

Every element in the view is one a real senior-living page would carry. No
echo spans, no debug divs, no elements that exist only to be asserted.

The observable outcome of a behavior is what the role sees on a real page:

- a validation message — `_plan.ErrorFor(m => m.Prop)`
- a value in a real field or summary line — balance due, monthly total
- a panel, row, or dialog appearing or disappearing
- an app-level object the page uses anyway — Toast, Drawer, Confirm
- framework gather tests only: the POST body — `request.PostData`

If a behavior seems observable only through a synthetic element, the view is
not a real page yet. Redesign the view, not the assertion. The check is the
screenshot test: a stranger seeing the page reads a senior-living product
screen — not a test rig.

## Method

1. **Write the story before looking at any view.**
   `As a [role] / I want [feature] / So that [value]`. The role and journey
   come from the senior-living domain, not from the code.

2. **List criteria as sentences the role would say.** Unhappy paths are
   mandatory — a happy-path-only suite passes while the feature lies.

   ```
   ✓ "I can search for a physician by name and select them"
   ✓ "The system tells me which information is missing"
   ✗ "echo span updates"        — no role says this
   ✗ "ej2 value equals expected" — implementation
   ```

3. **One criterion = one test.** One When, one Then. A fill → clear → refill
   cycle may stay one test when it proves one behavior's lifecycle; split
   when the steps prove different behaviors.

4. **Locate through the plan.** Preference order: `PagePlan<TModel>`
   expressions → `ComponentScope` when the model type is unavailable → raw
   locator only for explicit-ID elements. Gestures, surfaces, and assertions
   per component: `references/gestures.md`.

5. **Prove the outcome on a real-app surface** (list above), then check each
   test: traces to a criterion, named as the role speaks, survives internal
   refactoring.

## Fixture Shape

```csharp
[TestFixture]
public class When{BehaviorHappens} : PlaywrightTestBase
{
    private PagePlan<TModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<TModel>.FromPage(Page);
    }
}
```

## Test Layers

```
"Does the role see the right thing?"    → Playwright + PagePlan<TModel>
"Does this function compute correctly?" → Vitest (boundary values)
"Does the DSL produce correct JSON?"    → NUnit + Verify (snapshots)
```

## References

- **`references/gestures.md`** — per-component gestures, surfaces, assertions
- **`references/patterns.md`** — fixture code, validation errors, POST interception
- **`references/first-principles.md`** — Dan North, Kent Beck, Ian Cooper research
