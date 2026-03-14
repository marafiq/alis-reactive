# TYPE-SAFETY-027: Invalid data-trace Attribute Value Makes active=NaN, All Traces Emit

## Status: Type Safety Gap — Inverted Behavior

## Files
- `Scripts/auto-boot.ts:12` — unsafe `as TraceLevel` cast
- `Scripts/trace.ts:9-10` — `setLevel` does not validate input

## How to Reproduce

1. Set an invalid trace level in the plan element:
   ```html
   <script type="application/json" data-alis-plan data-trace="debg">...</script>
   ```
2. `auto-boot.ts:12`: `const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;`
3. `traceLevel` is `"debg"` (a typo for `"debug"`).
4. `trace.setLevel("debg")` is called.
5. `trace.ts:10`: `active = LEVELS["debg"]` — `LEVELS` has no `"debg"` key → `active = undefined`.
6. `undefined` is coerced to `NaN` in numeric context.
7. In `emit()` (line 33): `if (level > active) return;` — every comparison with `NaN` returns `false`.
8. `level > NaN` is ALWAYS `false`, so the guard NEVER returns. **ALL trace messages emit at ALL levels.**
9. The console is flooded with every trace message the framework produces.

This is the **exact opposite** of the expected behavior for an invalid level. The developer likely intended to disable tracing (since they didn't type a valid level), but instead got maximum verbosity.

## Deep Reasoning: Why This Is a Real Issue

This is a JavaScript `NaN` propagation bug combined with a TypeScript type safety gap. The `as TraceLevel` cast on line 12 is a **type assertion** (not a runtime check) — it tells the compiler "trust me, this is a TraceLevel" without verifying the value at runtime.

The `LEVELS` record is typed as `Record<TraceLevel, number>`, which means TypeScript believes any key access will return a `number`. But at runtime, a key that doesn't exist in the object returns `undefined`, which becomes `NaN` when used in numeric comparisons.

The NaN comparison quirk is one of JavaScript's most notorious footguns: `NaN > x` is `false` for ALL values of `x`, including `NaN > Infinity`, `NaN > 0`, and even `NaN > NaN`. This means the `if (level > active) return;` guard in `emit()` never triggers, and every trace message is emitted.

## How Fixing This Improves the Codebase

1. **Validate in setLevel**: Check that the level is a known key: `if (!(level in LEVELS)) { console.warn('[alis] unknown trace level: ' + level); return; }`.
2. **Or validate in auto-boot**: Check the attribute value before calling `setLevel`.
3. **Correct behavior for typos**: An invalid level either logs a warning and stays at the current level, or defaults to `"off"` — never to maximum verbosity.

## How This Fix Will Not Break Existing Features

- All existing usages pass valid `TraceLevel` strings. The validation only catches invalid values.
- The fix adds a guard before the assignment — valid levels are processed identically.
- The sandbox app hardcodes `data-trace="trace"` (a valid level) — unaffected.
- Playwright tests use `WaitForTraceMessage` which relies on trace being at the "trace" level — this level is valid and continues to work.
