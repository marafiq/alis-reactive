---
name: BDD tests should not use internals
description: Tests should not directly construct internal descriptor types — use the public DSL instead
type: feedback
---

Tests ideally should not be using internals (InternalsVisibleTo). It's a violation of BDD testing principles.

**Why:** BDD tests verify behavior through the public API surface. Directly constructing internal descriptor types (e.g., `new SequentialReaction(...)`) bypasses the builder API that real devs use. Tests should exercise the same code path as production.

**How to apply:** When writing new tests, always use the DSL entry points (`Html.On`, `CreatePlan()`, `Trigger()`, builders). Existing tests that use `new SequentialReaction(...)` or similar internal constructors should be refactored to use the public API in a future session. Flag but don't block on this for now.
