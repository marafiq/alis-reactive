# Playwright Behavior Tests

Root CLAUDE.md is authoritative; the test rules are `.claude/rules/tests.md`. This project proves the
shipped components and the sandbox journeys.

- A test never depends on ambient server events.
- Locators: prefer `PagePlan<TModel>` expressions (`Locators/`); raw locators only for
  explicit-ID elements.

## When a test fails

Triage in order: is the criterion right, the arrangement right, the tooling
right? All yes → verify manually in browser to classify locator bug vs app bug.
Locator bugs are fixed in `Locators/`, app bugs in the app.
Never hack the test to pass. Research a Syncfusion behavior with
`[component] [framework] playwright [behavior]`; Chrome MCP tools behave
differently from Playwright, so debug a Syncfusion component with a
Playwright diagnostic test, not by clicking in Chrome.

## Running

`scripts/playwright.sh` (use `--filter` to focus) rejects stale browser assets:
run `scripts/build.sh` after any TS change.
