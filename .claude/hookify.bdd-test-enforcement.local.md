---
name: bdd-test-enforcement
enabled: true
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: (PlaywrightTests.*\.cs$|\.spec\.ts$|\.test\.ts$)
  - field: new_text
    operator: regex_match
    pattern: page\.Evaluate|page\.evaluate|Thread\.Sleep|Task\.Delay|\.Retry\]|\[Retry\(|try\s*\{.*Assert|catch\s*\(.*Assert|innerHTML|dangerouslySetInnerHTML|window\.alis\.|document\.querySelector|querySelectorAll
---

**BDD anti-pattern detected in test file.**

This code violates the BDD Constitution. Five Rules:
1. Test describes BEHAVIOR, not implementation
2. Test is independently understandable
3. Test FAILS when behavior breaks
4. Test uses REAL interactions only
5. Test is blind-reviewed

**What was detected (and the fix):**

- `page.Evaluate`/`page.evaluate` — use real Playwright interactions (click, fill, select). Users do not call JavaScript.
- `Thread.Sleep`/`Task.Delay` — use Playwright's built-in waiting (WaitForSelector, Expect with timeout).
- `[Retry]` — masks flakiness. Fix the root cause instead.
- `try/catch` around assertions — swallows failures. Let assertions throw.
- `innerHTML`/`querySelector`/`querySelectorAll` — assert what users SEE (text, visibility), not DOM internals.
- `window.alis` — framework internals. Test the user-visible outcome.

**Load skill:** `bdd-testing` before writing any Playwright test.
**Read:** `memory/feedback_bdd_constitution.md` for the full BDD Constitution.
