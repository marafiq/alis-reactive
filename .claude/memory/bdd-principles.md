---
name: BDD Principles
description: Consolidated BDD rules for Playwright tests — 5 rules, framework primitives, 7-behavior contract, blind reviewer protocol
type: reference
---

## Cardinal Rule

Never change framework code to make a test pass. If a valid test fails, report the bug.
The test is the specification. The framework is what is being tested.

## The 5 BDD Rules

### Rule 1: Behavior, Not Implementation

A test answers: "What should the user SEE when they DO something?"

```
BAD:  domready_trigger_fires_sequential_reaction
GOOD: page_shows_resident_name_on_load

BAD:  set_prop_mutation_writes_value_to_ej2_instance
GOOD: selecting_care_level_updates_billing_amount
```

Litmus test: if you refactor internals without changing what the user sees, the test still passes.

### Rule 2: Independently Understandable

The test name + assertion tell the full story. No dependency on other tests, no shared
mutable state, no reading source code to understand what is being tested.

```
BAD:  test_03_after_setup()
GOOD: empty_veteran_id_shows_required_error_when_veteran_checkbox_is_checked()
```

Each test navigates to a fresh page.

### Rule 3: Fails When Broken

Ask: if someone removes the validation rule, does this test fail? If someone breaks the
resolver? If someone renames an internal variable — does it fail? That last one is BAD.

The mutation test: imagine someone introduces a bug. If you cannot name the specific bug
your test would catch, the test has no value.

### Rule 4: Real Interactions Only

Users click, type, select, and submit. Tests do the same.

- Click buttons, type into fields, select from dropdowns
- Wait for visual indicators, not internal state
- Assert what the user sees (text, visibility, CSS class)
- Real Playwright browser, real Kestrel server
- No `page.evaluate()`, no mocking, no jsdom heuristics

### Rule 5: Blind Reviewed

Every test is reviewed by an agent that has only the BDD principles and the test code.
Passing is not enough. See blind reviewer protocol below.

## Framework Primitives Only

Sandbox pages must use framework primitives. Never work around the framework.

- `Html.On(plan, t => t.DomReady(...))` -- never raw `<script>`
- `Html.TextBoxFor(m => m.Name)` -- never raw `<input type="text">`
- `Component<NativeButton>().Reactive(evt => evt.Click, ...)` -- never raw onclick
- `.Validate<TValidator>()` -- never manual validation logic
- `p.Element("id").SetText(...)` -- never raw DOM manipulation
- `.Gather(g => g.IncludeAll())` -- never manual form serialization
- `Html.Field(m => m.Name).Required().Label("Name")` -- never raw label/input combos

Tests must also use the public DSL, not internal constructors. Arrange with `Html.On`,
`CreatePlan()`, `Trigger()`, and builders. Never `new SequentialReaction(...)` or similar
internal types. The test exercises the same code path as production.

## 7-Behavior Contract Per Component

Every component test covers all seven:

1. RENDERS -- component shows with correct initial state
2. INTERACTS -- user action fires event, pipeline executes
3. VALIDATES -- invalid input shows error inline
4. CONDITIONALLY VALIDATES -- condition toggle enables/disables rule
5. LIVE-CLEARS -- valid correction clears error without re-submit
6. GATHERS -- component value collected into POST body
7. SUBMITS -- valid form, server responds, UI updates

## Naming Convention

| Element | Pattern | Example |
|---------|---------|---------|
| Folder | `PascalCase` concern | `AllModulesTogether/Cascading/` |
| Test class | `When{BehaviorHappens}` | `WhenParentSelectionFiltersDependentList` |
| Test method | `snake_case_scenario_with_outcome` | `empty_veteran_id_shows_required_error_when_veteran_checked` |
| No vendor prefix | Folder conveys vendor | `Components/Fusion/WhenDateSelected.cs` |

## No-Hack Rules

- No `Thread.Sleep` or arbitrary `Task.Delay`
- No weak assertions (`ToContainText` when you mean `ToHaveText`)
- No skipping, ignoring, or commenting out failing tests
- No `try/catch` around assertions to swallow failures
- No `[Retry]` attributes to mask flakiness
- No `page.evaluate()` to bypass UI interactions
- No pass-hacking by asserting something trivially true

## Blind Reviewer Protocol

After writing tests, dispatch a separate agent with only these principles and the test file.
The reviewer has no context about why tests were written or what the implementation looks like.

The reviewer evaluates each test against all 5 rules, opens the page in headed browser, and
verifies: does the test assert what a user would see? Does the name describe the behavior?
Would this test catch a real regression?

Any test that violates any rule is flagged with: (1) exact test method name, (2) which rule
is violated, (3) evidence -- quoted assertion or test name, (4) what the test should look like.

Author obligation on flags: accept and fix if evidence is valid, or defend with counter-evidence
(browser screenshot, quoted assertion, specific regression it catches). "I think it's fine"
is not a defense.

## Cascade Preamble for Subagents

> You are writing/reviewing BDD Playwright tests for Alis.Reactive -- a framework serving
> senior living communities. Read `memory/bdd-principles.md` before writing any test.
> Five Rules: (1) Behavior, (2) Independent, (3) Fails when broken, (4) Real interactions,
> (5) Blind reviewed. Cardinal Rule: never change framework code.
