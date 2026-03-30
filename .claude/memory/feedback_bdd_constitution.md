---
name: BDD Constitution
description: Mandatory contract cascaded to EVERY parallel agent working on Playwright tests — non-negotiable BDD values for senior living software, research-backed rules
type: feedback
---

## BDD Constitution — Alis.Reactive Playwright Tests

This document is law for every agent that touches Playwright tests. Read it before writing a single line.

### Why This Exists

This framework serves senior living communities. Residents depend on the software built with it.
A missed validation rule means wrong medication. A broken condition means hidden care alerts.
A flaky test that passes when it shouldn't means a bug ships to a facility where an 85-year-old
resident's care plan depends on it.

**Every test you write is a contract with that resident.**

### The Cardinal Rule — NEVER Change the Framework

You are NOT ALLOWED to change any framework code (Alis.Reactive, Alis.Reactive.Native,
Alis.Reactive.Fusion, Alis.Reactive.FluentValidator, Scripts/). Not even one line.

If a test fails:
1. Open headed browser, verify the behavior manually
2. If the BEHAVIOR is wrong → you found a REAL BUG → report it, keep the test as-is
3. If the TEST is wrong → fix the test to match the actual correct behavior
4. NEVER change framework code to make a test pass

Even if you're 100% certain the test is valid and it fails — report the bug.
The test is the specification. The framework is what's being tested.

---

### The Five Definitive Rules of a Good BDD Test

These rules are distilled from Dan North (originator of BDD), Gojko Adzic ("Specification by
Example"), Lisa Crispin & Janet Gregory ("Agile Testing"), and the Cucumber community's
collective wisdom on what separates tests that catch bugs from tests that pass meaninglessly.

#### Rule 1: A Test Describes a BEHAVIOR, Not an Implementation

> "BDD is about conversations, not testing." — Dan North

A good BDD test answers: **"What should the user SEE when they DO something?"**
A bad BDD test answers: "What method gets called when this event fires?"

```
BAD:  domready_trigger_fires_sequential_reaction
      (describes HOW the framework works internally)

GOOD: page_shows_resident_name_on_load
      (describes WHAT the user sees)

BAD:  set_prop_mutation_writes_value_to_ej2_instance
      (tests an implementation detail — if we change the internal mechanism, this breaks
       even though the behavior is unchanged)

GOOD: selecting_care_level_updates_billing_amount
      (tests the OUTCOME — if we refactor internals, this still passes as long as the
       user sees the right billing amount)
```

**The litmus test:** If I refactor the internals without changing what the user sees,
does this test still pass? If NO → you're testing implementation, rewrite it.

#### Rule 2: A Test Must Be INDEPENDENTLY Understandable

> "Each scenario should be able to be understood in isolation." — Gojko Adzic

A test reader (developer, QA, product owner) must understand what this test proves
WITHOUT reading other tests, without reading the source code, without understanding
the framework internals.

**The test name + the assertion must tell the full story.**

```
BAD:  test_03_after_setup()
      (requires reading test_01 and test_02 to understand)

GOOD: empty_veteran_id_shows_required_error_when_veteran_checkbox_is_checked()
      (I know exactly what happens, what the precondition is, and what I expect to see)
```

**Requirements:**
- Test name describes the scenario AND the expected outcome
- No dependency on other tests' side effects
- No shared mutable state between tests
- Each test navigates to a fresh page

#### Rule 3: A Test Must FAIL When the Behavior Breaks

> "A test that never fails is a test that never helps." — Lisa Crispin

The entire point of a test is to CATCH REGRESSIONS. A test that always passes regardless
of what you change is worse than no test — it gives false confidence.

**How to verify Rule 3:** After writing a test, ask yourself:
- If someone removes the validation rule, does this test fail? → Good.
- If someone breaks the resolver, does this test fail? → Good.
- If someone swaps the vendor root resolution, does this test fail? → Good.
- If someone renames an internal variable, does this test fail? → BAD — rewrite.

**The mutation test:** Imagine someone introduces a bug. Does your test catch it?
If you can't name a specific bug your test would catch, the test has no value.

#### Rule 4: A Test Must Use REAL Interactions, Not Shortcuts

> "The best acceptance tests exercise the system from the outside, the way a user would."
> — Gojko Adzic, "Specification by Example"

BDD tests prove the system works FOR USERS. Users don't call `page.evaluate()`.
Users don't inject values via JavaScript. Users CLICK, TYPE, SELECT, and SUBMIT.

**Requirements:**
- Click buttons like a user clicks buttons
- Type into fields like a user types into fields
- Select from dropdowns like a user selects
- Wait for visual indicators, not internal state
- Assert what the user SEES (text, visibility, CSS class), not internal variables
- Real Playwright browser, real Kestrel server
- No mocking HTTP responses, no mocking components, no jsdom heuristics

**The user test:** Could a QA person sitting at a browser reproduce exactly what
this test does? If your test uses an API that a browser user can't access, it's
testing infrastructure, not behavior.

#### Rule 5: A Test Must Be REVIEWED by a Blind BDD Expert

Every test — even if it passes — must be reviewed by another agent that has ONLY:
- The BDD Constitution (this document)
- The test code
- Access to the running application (headed browser)

The reviewer does NOT see:
- The implementation plan
- The conversation history
- Why the test was written this way

**The blind review protocol:**
1. After writing tests, dispatch a review agent with ONLY the BDD Constitution and the test file
2. The reviewer evaluates each test against all 5 rules
3. The reviewer opens the page in a headed browser and manually verifies:
   - Does the test assert what a user would actually see?
   - Does the test name accurately describe the behavior?
   - Would this test catch a real regression?
4. If the reviewer flags a test as violating any rule → rewrite the test
5. Tests that pass code but fail blind review are NOT shipped

**Why blind review:** The author knows too much. They know the implementation, they know
why they wrote the test, they rationalize weak assertions. A blind reviewer has fresh eyes
and judges purely against the BDD rules.

---

### Naming Convention (Enforced)

| Element | Pattern | Example |
|---------|---------|---------|
| Folder | `PascalCase` concern | `AllModulesTogether/Cascading/` |
| Test class | `When{BehaviorHappens}` | `WhenParentSelectionFiltersDependentList` |
| Test method | `snake_case_scenario_with_outcome` | `empty_veteran_id_shows_required_error_when_veteran_checked` |
| No vendor prefix | Folder conveys vendor | `Components/Fusion/WhenDateSelected.cs` |
| No "Using" prefix | Describe behavior | `WhenSwitchToggles` (not WhenUsingSwitch) |

### The 7-Behavior Contract (Every Component Test)

```
1. RENDERS              — component shows with correct initial state
2. INTERACTS            — user action fires event, pipeline executes
3. VALIDATES            — invalid input shows error inline
4. CONDITIONALLY VALIDATES — condition toggle enables/disables rule
5. LIVE-CLEARS          — valid correction clears error without re-submit
6. GATHERS              — component value collected into POST body
7. SUBMITS              — valid form → server responds → UI updates
```

### No-Hack Rules

- NEVER add `Thread.Sleep` or arbitrary `Task.Delay`
- NEVER weaken assertions (`ToContainText` when you mean `ToHaveText`)
- NEVER skip, ignore, or comment out failing tests
- NEVER change framework code to make a test pass
- NEVER add `try/catch` around assertions to swallow failures
- NEVER use `[Retry]` attributes to mask flakiness
- NEVER use `page.evaluate()` to bypass UI interactions
- NEVER pass-hack a test by asserting something trivially true

### How to Cascade This to Subagents

When dispatching a parallel agent, include this preamble:

> You are writing/reviewing BDD Playwright tests for Alis.Reactive — a framework serving
> senior living communities. Read the BDD Constitution at
> `memory/feedback_bdd_constitution.md` before writing any test. Five Rules:
> (1) Behavior not implementation, (2) Independently understandable,
> (3) Fails when behavior breaks, (4) Real interactions not shortcuts,
> (5) Blind BDD review required — even passing tests get reviewed.
> Cardinal Rule: NEVER change framework code. If a valid test fails, report the bug.

### How to Dispatch Blind BDD Reviewer

After writing tests, dispatch a separate agent:

> You are a blind BDD test reviewer. You have ONLY this constitution and the test file.
> You have NO context about why tests were written or what the implementation looks like.
> Open the page in headed browser. For each test, evaluate against the 5 BDD Rules:
> (1) Does it describe behavior, not implementation? (2) Is it independently understandable?
> (3) Would it fail if the behavior broke? (4) Does it use real user interactions?
> (5) Does the test name + assertion tell the full story?
> Flag any test that violates ANY rule. Be rigorous — passing is not enough.
> For each flag, provide: (1) the exact test method name, (2) which rule it violates,
> (3) EVIDENCE — quote the assertion or test name that violates the rule, (4) what the
> test SHOULD look like to comply. No vague feedback — real reasoning, real evidence.

**Author's obligation:** When the blind reviewer flags a test, you must either:
1. **Accept and fix** — if the evidence is valid, rewrite the test. Don't argue.
2. **Defend with evidence** — if you disagree, provide counter-evidence:
   show the headed browser screenshot, quote the assertion, explain WHY
   the test catches a real regression. "I think it's fine" is not a defense.

Both sides must produce REAL reasoning backed by REAL evidence.
The goal is better tests, not winning arguments.
