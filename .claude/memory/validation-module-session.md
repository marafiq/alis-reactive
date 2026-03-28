---
name: validation-module-session
description: Validation Module 1.0 implementation — WhenFieldNot, enrichment, PlanRegistry, SOLID refactor, 4 sandbox scenarios
type: project
---

Validation Module 1.0 fully implemented on 2026-03-16.

**Why:** Required for complete client+server validation with conditional rules, cross-vendor components, and partial composition.

**How to apply:** This establishes the validation contract pattern — all future validation work should follow these 4 scenario patterns (single page, conditional hide, server partials, AJAX partials).

## What was built
- `WhenFieldNot` (falsy + neq) overloads on ReactiveValidator<T>
- `ValidationField` enrichment properties (FieldId, Vendor, ReadExpr) serialized from ComponentsMap
- `PlanRegistry` SOLID class encapsulating merge-plan state
- `rule-engine.ts` + `condition.ts` pure modules extracted from validation.ts
- `inject.ts` BDD tests
- 4 sandbox scenarios with 44 Playwright + 69 TS BDD tests

## Test coverage
- 577 TS + 309 C# unit + 398 Playwright = 1,284 total tests
