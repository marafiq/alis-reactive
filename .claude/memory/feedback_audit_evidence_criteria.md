---
name: Evidence-Based Audit Criteria
description: Strict criteria for proving any code issue before taking action — no changes without evidence, no hacks, no broken features
type: feedback
---

Before ANY code change from an audit finding, the issue MUST be proven against ALL of these criteria:

1. **What is the issue?** — Clear description of the problem
2. **Why is the issue?** — Root cause analysis
3. **Where is your evidence?** — Exact files, lines, test results
4. **Did you consider the whole system?** — C# DSL, schema, tests, views — the finding may be prevented by another layer
5. **How to reproduce?** — Concrete scenario reproducible in actual browser with Playwright tests using proper framework primitives. If NOT REPRODUCIBLE → not a real bug
6. **How to fix?** — Using framework primitives only. No hacks, no workarounds
7. **What does fixing bring?** — Tangible outcome that improves the system
8. **Does it violate framework vision?** — Plan-driven architecture, fail-fast, vertical slices
9. **Must not break any existing features** — All 2,189 tests must pass after the fix

**Why:** We got burned by agents flagging "issues" that were actually correct design decisions (e.g., sync executeCommand, window.alis.confirm, evalCondition null blocking). Evidence-first prevents wasted effort and accidental architecture reversals.

**How to apply:** When dispatching audit agents, include these 9 criteria in the prompt. When reviewing agent findings, reject anything that can't satisfy all 9. Use the 3-layer judge pattern: module readers → Integration Judge + Non-Dogmatic Judge + Evidence-Based Prosecutor.
