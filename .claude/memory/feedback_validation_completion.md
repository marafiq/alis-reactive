---
name: feedback_validation_completion
description: Critical design feedback for validation module completion — coerceAs, cross-property, dates, BDD test quality, no dead code
type: feedback
---

## Validation Module Completion — User Feedback (2026-03-19)

### Schema Design Principles
- `coerceAs` on ValidationRule is derived from `TProperty` — the property expression gives you the type at C# compile time. NEVER guess or infer at runtime.
- `field` on ValidationRule for cross-property comparisons — same deterministic ID system as everything else. TModel → prop expression → type known → ID predictable. No new "peer" concept — it's just reading another component with a known ID.
- Schema must be DETERMINISTIC — no fallbacks, no silent drops. Every FV validator either extracts to a client rule or is explicitly documented as server-only.
- Rules schema is the heart — design from the schema outward, not implementation inward.

### Cross-Property Comparisons
- Same `min`/`max`/`gt`/`lt` rule types — `constraint` for fixed value, `field` for cross-property. They're mutually exclusive.
- Cross-property reads the other field using the SAME mechanism as everything else: binding path → enriched fieldId → resolveRoot → walk(readExpr). No scanning, no new concepts.

### Date Handling
- `coerceAs: "date"` uses existing `toDate()` from `core/coerce.ts` — already handles Date objects (SF components), ISO strings, date-only strings with timezone safety.
- DateTime constraints serialized as `"YYYY-MM-DD"` (date-only) when time is midnight — `toDate()` parses as LOCAL midnight, timezone-safe.
- Senior living facilities have different timezones. The UI shows local datetime. The framework compares dates consistently via coercion. Facility timezone is the application's responsibility.
- User is conflicted about Unix timestamps vs ISO strings. Current decision: ISO strings with `toDate()` handling. May revisit.

### readExpr Should Be an Object
- User identified that `readExpr` as a string is limiting (e.g., DateRangePicker has `startDate` AND `endDate`).
- This is OUT OF SCOPE for validation completion — separate plan needed.
- For now, DateRangePicker uses `readExpr: "startDate"` (primary) and separate StartDate()/EndDate() typed sources.

### Empty/Null Extraction
- `Empty()` and `Null()` SHOULD be extracted. `Empty()` validates the value IS empty — legitimate rule (e.g., "if not employed, salary must be empty"). Extract as `"empty"` rule.
- PrecisionScale, IsInEnum, IsEnumName — server-only, explicitly documented.

### BDD Test Quality — CRITICAL
- **DO NOT test implementation** — test USER BEHAVIORS.
- Bad: "coerce is called with date type"
- Good: "when nurse selects admission date before 2020, min date error shows on blur"
- Bad: "peerReader returns raw value"
- Good: "when discharge date is before admission date, cross-property error shows"
- There is a whole skill for writing proper tests (superpowers:test-driven-development). USE IT.
- 100% extraction coverage means every FV rule type has a test — both unconditional AND inside WhenField().
- Playwright tests must be in a NEW parallel fixture, not pollute existing.

### No Dead Code, No Fallbacks
- Replace `Number()` completely with `coerce()` — don't leave old code paths.
- If `coerceAs` is not set, the comparison should use a deterministic default (number for backward compatibility), but this should be explicit in the schema, not a silent fallback.
- `peerReader.readPeer()` should return `unknown` (raw value) not `string` — the rule engine coerces via `coerceAs`.

### Native Compound Components (Radio Group + CheckList)
- Inline init scripts (same pattern as SF) — NO DOM scanning modules.
- `isInteracted` property on canonical element.
- Change events bubble with proper guard (`e.target.type !== "radio"` prevents infinite loop).
- Works with partials because script runs where rendered.

### Live Re-Validation
- Industry standard: clear on input, re-validate on blur/change.
- `evaluateField()` extracted from orchestrator — shared between `validate()` (submit) and `revalidateField()` (blur).
- `wireLiveValidation()` (renamed from `wireLiveClearing()`) wires both handlers.

### What's Completed This Session
- HTTP pipeline redesign (fully async, error boundaries, ResolvedFetch)
- gt/lt rule types for GreaterThan/LessThan
- Native compound component inline init
- Live re-validation on blur/change
- Conditional validation parity verified (all 11 rule types work under WhenField)
- Plan written: `docs/superpowers/plans/2026-03-19-validation-completion.md`
