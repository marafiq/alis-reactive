---
name: bdd-testing
description: >
  This skill should be used when writing Playwright browser tests, TS unit tests,
  or adding new sandbox test scenarios for Alis.Reactive. Also use when the user
  asks to "write a test", "add Playwright tests", "test this component",
  "fix a failing test", or "add a test for this view". Covers full user journey
  testing, parallel fixtures, senior living domain models, and root cause debugging.
---

# BDD Testing for Alis.Reactive

## Purpose

Prevent tests that pass while the browser is broken. Every test must assert
what the USER sees, not what the function returns.

## Test Architecture

```
Layer           Runner        Asserts
─────           ──────        ───────
Playwright      NUnit         DOM state after full user journeys
TS unit         Vitest+jsdom  Runtime logic boundaries (resolver, conditions, validation)
C# unit         NUnit+Verify  Plan JSON snapshots + schema conformance
```

## Playwright Tests — Full User Journeys

### Fixture Pattern

Each scenario gets its own fixture class, own URL, parallel execution:

```csharp
[TestFixture]
public class WhenValidatingResidentAdmission : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract";
    private const string R = "Namespace_Model__";   // IdGenerator prefix

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator ErrorFor(string f) => Page.Locator($"#form span[data-valmsg-for='{f}']");
    private ILocator Input(string s) => Page.Locator($"#{R}{s}");
}
```

### Test Shape — Always a Journey

```
submit empty → see errors → fix one → resubmit → that error gone → fix all → success
```

Every submit assertion checks BOTH inline errors AND summary state.

### Assertions — What the User Sees

```csharp
await Expect(ErrorFor("Name")).ToContainTextAsync("required");   // inline visible
await Expect(SummaryDiv).ToBeHiddenAsync();                      // no phantom summary
await Expect(Result).ToHaveTextAsync("");                        // POST was blocked
```

Never use `AssertNoConsoleErrors()` as the sole assertion.

## TS Unit Tests — Boundary Values

Test every edge of pure modules (rule-engine, condition, resolver):

```
required: fails "", null, undefined, false. passes 0.
unknown rule: blocks (fail-closed).
broken regex: blocks (fail-closed).
eq on empty source: returns false (no intent).
```

DOM setup must match production (planId, summary div, error spans, component IDs).

## Domain — Senior Living

All models use: `ResidentModel`, `CareLevel`, `VeteranId`, `FacilityModel`.
Never generic "TestModel" or "User". Realistic labels, placeholders, error messages.

## When a Test Fails — Root Cause Protocol

1. **STOP.** Read the failure. Do not touch code.
2. **Trace** the full path from trigger to output.
3. **Identify the exact line** producing wrong result.
4. **Ask WHY** that line does what it does — it may be correct for another scenario.
5. **Fix root cause**, not symptom. If unsure, ask the user.

See **`references/patterns.md`** for common symptom → root cause mappings.

## Checklist

- [ ] Every Playwright test is a full journey (error → fix → success)
- [ ] Every submit checks inline AND summary
- [ ] State transitions tested (toggle, change, reload)
- [ ] Fixtures are parallel — own URL, no shared state
- [ ] Senior living domain models
- [ ] Framework primitives used (InputField, SetValidator, WhenField)

## Additional Resources

- **`references/patterns.md`** — Code patterns, fixture examples, validator scoping, AJAX partial lifecycle
