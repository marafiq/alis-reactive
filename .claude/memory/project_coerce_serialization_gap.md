---
name: Coerce first-class module — session complete
description: CoerceResult pattern, ComponentRegistration.CoerceAs, Unix ms for WhenField<DateTime>, DateRangePicker readExpr "value" with DateTime[] — PR #49 ready for merge
type: project
---

## Coerce First-Class Module — PR #49

**Branch:** `feature/coerce-first-class` (worktree at `.worktrees/coerce-first-class`)
**PR:** https://github.com/marafiq/alis-reactive/pull/49
**Issue:** https://github.com/marafiq/alis-reactive/issues/48

### What Was Delivered (4 Tasks + Bug Fixes + Docs)

1. **Task 1: CoerceResult<T> pattern** — all coerce functions return Result (never throw). ESLint bans raw String(). coerceOrThrow for mutations.
2. **Task 2: ComponentRegistration.CoerceAs** — typeof(TProp) flows C# → plan JSON → TS. Enrichment propagates.
3. **Task 3: Unix ms for WhenField<DateTime>** — validation conditions serialize DateTime as Unix ms. domConditionReader uses coerceAs "date" → toDate → Unix ms string.
4. **Task 4: DateRangePicker readExpr "value"** — reads [Date, Date] from ej2.value. Model DateTime[]. Zero TS runtime changes for array handling (existing emitArray works).

**Bug fixes from PR review:**
- F2: DateOnly not handled in SerializeDateConstraint → added branch
- F3: Exception message missing fields → include all 5 fields
- F5: live-clear wiredFields.add before getElementById → moved after element check
- AddToComponentsMap idempotency check → compare CoerceAs too

**Schema:** ValidationField enriched properties declared. ValidationRule.coerceAs → $ref to shared CoercionType.
**CLAUDE.md:** Rule 3a (schema changes require failing test first). Stale refs fixed (root.ts, data-reactive-plan, design-system.css).
**Docs site:** runtime.mdx (coercion section + dependency graph), json-plan-schema.md (ComponentEntry fields), fusion-components.md (DateRangePicker rewritten).

### Test Results

| Suite | Tests | Status |
|-------|-------|--------|
| TS vitest | 1,114 | ✅ |
| C# UnitTests | 350 | ✅ |
| C# Native | 73 | ✅ |
| C# Fusion | 103 | ✅ |
| C# FluentValidator | 79 | ✅ |
| Playwright | 695 + 1 flaky | ✅ |
| **Total** | **~2,414** | **ALL PASS** |

### Open Issues for Next Session

- **#52** — Add enriched ValidationField plan to AllPlansConformToSchema test
- **#53** — Bugs found during PR review (F1 not triggerable, F5 fixed, F6-F8 by design)
- **#54** — 3 new CRITICAL complexity issues from CoerceResult pattern (conditions, commands, rule-engine) — pure refactor to extract helpers

### Key Architecture Decisions

- **Comparison vs serialization:** coerce() for comparison (timestamps, numbers), toString() for HTTP (ISO, strings). Never mixed.
- **Unix ms for DateTime conditions:** C# DateTimeOffset.ToUnixTimeMilliseconds(), JS Date.getTime(). Exact match.
- **DateRangePicker readExpr "value":** Reads [Date, Date] array. Model DateTime[]. Existing emitArray handles it. comp.StartDate()/EndDate() use hardcoded readExpr independently.
- **No DateRange struct:** DateTime[] with existing "array" coercion type is sufficient. Custom struct would need JsonConverter + FluentValidation + new coercion type — ROI not justified.
- **Result pattern:** CoerceResult<T> = { ok: true, value: T } | { ok: false, error: string }. Conditions → false on Err. Validation → show message. Mutations → coerceOrThrow. Gather → skip field.
