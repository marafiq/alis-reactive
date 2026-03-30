---
name: feedback_no_tech_debt
description: NEVER add tech debt under any conditions — no silent fallbacks, no string-matching hacks, no reflection workarounds, no "fix later" patterns
type: feedback
---

## No Tech Debt — Under Any Conditions

Do not add tech debt in any form. Every solution must be clean from the start.

**Why:** The user is building a framework that will onboard 100+ component vertical slices. Any hack or shortcut compounds into unmanageable debt at scale. "Good enough for now" becomes "impossible to fix later."

**How to apply:**
- No silent fallbacks or default values for missing data — throw immediately
- No string matching on type names (e.g., `GetType().Name.Contains(...)`) — create proper interfaces
- No reflection hacks — use compile-time type safety (interfaces, generics, pattern matching on known types)
- No "backward compat" shims — if the schema changes, update all consumers
- No `// TODO` or `// FIXME` — fix it now or don't write it
- If a third-party library doesn't expose the interface you need, write your own rule/validator/component that does
- If it feels like a workaround, it IS a workaround — find the clean solution
