# CLAUDE.md Expert Review — 3 Perspectives

Generated 2026-03-28 by 3 independent review agents.

---

## Senior Architect Review

### Top Issues (by severity)

1. **Command kinds table incomplete** — Lists only `dispatch` + `mutate-element`. Missing `mutate-event`, `validation-errors`, `into` (5 total in codebase).

2. **Trigger kinds incomplete** — Plan example shows only `dom-ready | custom-event`. Missing `component-event`, `server-push`, `signalr` (5 total).

3. **Reaction kinds undocumented** — Only `sequential` shown. Missing `conditional`, `http`, `parallel-http` (4 total). PipelineBuilder modes map to these but this is never explained.

4. **`[JsonDerivedType]` claim is wrong** — Rule 3 says new commands need `[JsonDerivedType]`. Actual code uses `[JsonConverter(typeof(WriteOnlyPolymorphicConverter<Command>))]`.

5. **`root.ts` description incomplete** — Omits multi-plan composition (`composeInitialPlans()`), confirm/action-link initialization, and side-effect imports (drawer, loader).

6. **`execute.ts` description misleading** — Says it contains `executeCommand()`. Actually `commands.ts` is a separate file with all command dispatch logic. `commands.ts` is absent from key files table.

7. **Section 8 (Component Architecture) disproportionate** — 130 lines (18% of doc). Overlaps with Layer 2/3 tables. Should be extracted to a reference doc.

8. **No async/error model documented** — Runtime is fully async (`executeReaction` returns `Promise<void>`). Error propagation, guard evaluation async behavior, and assertNever handling never mentioned.

---

## Testing Expert Review

### Top Issues

1. **4 test projects completely missing from pre-commit section**: `Analyzers.Tests` (13 tests), `DesignSystem.Tests` (32 tests), `NativeTagHelpers.Tests` (17 tests), `Net48.SmokeTest`.

2. **Port 5220 claim is wrong** — `WebServerFixture.cs` uses `GetAvailablePort()` (random port), not hardcoded 5220.

3. **`DumpLogsOnFailure()` does not exist** — Actual mechanism is `TearDown()`. Screenshot path is `TestResults/playwright-traces/`, not `TestResults/screenshots/`.

4. **`Alis.Reactive.Playwright.Extensions` library undocumented** — 16 component-specific locator classes (`AutoCompleteLocator`, `DropDownListLocator`, etc.) critical for writing new Playwright tests.

5. **Parallel execution model undocumented** — `ParallelScope.Fixtures`, `LevelOfParallelism(2)`, `[NonParallelizable]` on specific tests. Directly affects debugging.

6. **No "how to write a new test" recipe** — Rule 3 lists the checklist but not the mechanics for each layer.

7. **Suggest consolidating pre-commit into a single script** — `scripts/pre-commit-gate.sh` pattern already proven by `sonar-analyze.sh`.

---

## Developer Experience (DX) Expert Review

### Top Issues

1. **No "Quick Start" section** — Zero instructions for `npm install`, `dotnet restore`, running the sandbox, or what URL to open. A new dev can't get productive in 30 minutes.

2. **Missing `watch`, `watch:css`, `lint` from build commands** — These are the daily development commands and they're invisible.

3. **Rule 8 should be extracted** — 129 lines of component architecture embedded in rules. Replace with 3-line summary + link.

4. **Three overlapping workflow sections** — Feedback Loop, Cross-Layer Changes, Pre-Commit Verification cover the same ground. Consolidate into one "Development Workflow."

5. **"SOLID loop" is misleading** — SOLID means five specific OOP principles. The concept is actually "Three-Layer Loop" or "Descriptor-Plan-Runtime Loop."

6. **Internal jargon in architecture section** — "BindExpr", "ExecContext", "bracket notation", "phantom types" are contributor-level terms, not user-facing.

7. **No cookbook recipes** — Only one code example. Add 3-4 common task recipes: show/hide element, load server data, react to dropdown change.

8. **AI instructions mixed with human docs** — Rules 1 (worktrees), 9 (root cause), 12 (API surface), 13 (Playwright) read as AI prompt constraints. Separate them.

9. **Two-phase boot explained 3 times** — Lines 150, 158-165, and 331-335. Pick one location.

10. **Missing projects in Projects table** — `Alis.Reactive.Analyzers`, `Alis.Reactive.NativeTagHelpers` are real projects but invisible.

---

## Cross-Cutting Themes

All 3 reviewers independently flagged:
- **Incomplete enumerations** (commands, triggers, reactions, test projects)
- **Section 8 is too long** for a rules section
- **Workflow duplication** (3 overlapping workflow sections)
- **Missing projects** in the Projects table
- **No getting-started path** for new developers
