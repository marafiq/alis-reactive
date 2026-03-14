# FAIL-FAST-011: Missing default: throw in TypeScript Switch Statements

## Status: Fail-Fast Violation (Rule 8) — 4 locations

## Files
1. `Scripts/commands.ts:23` — switch on `cmd.kind`
2. `Scripts/execute.ts:31` — switch on `reaction.kind` (sync path)
3. `Scripts/execute.ts:73` — switch on `reaction.kind` (async path)
4. `Scripts/gather.ts:25` — switch on gather item kind
5. `Scripts/trigger.ts:17` — switch on `trigger.kind`

## How to Reproduce

1. Add a new command kind to the schema and C# DSL (e.g., `"focus-element"`).
2. Add the TypeScript type to `types.ts`.
3. Forget to add the handler in `commands.ts`.
4. Build succeeds (TypeScript has no exhaustiveness checking without a `default: never` guard).
5. At runtime, the new command is silently ignored — no error, no trace, no indication.
6. The developer sees no effect and has no clue where to look.

Note: `conditions.ts:21` and `conditions.ts:48` already have `default: throw` — proving the pattern is known but inconsistently applied.

## Deep Reasoning: Why This Is a Real Bug

These switches operate on **deserialized JSON**, not compile-time TypeScript types. The TypeScript compiler's exhaustiveness checking only works if the switch variable is a union type AND the switch has no `default` case AND the function return type is `never` in the default position. None of these conditions are met here because:

1. The data comes from `JSON.parse()` — TypeScript trusts the type assertion but cannot verify it at runtime.
2. Plan JSON is produced by the C# DSL, which can introduce new kinds that the TS code hasn't been updated for.
3. The schema may evolve faster than the runtime handlers.

The framework's CLAUDE.md Rule 2 says: "Adding a new command requires a runtime handler." But nothing enforces this at runtime. Without `default: throw`, a missing handler is a silent no-op.

Compare with `component.ts:26` which correctly throws: `throw new Error('[alis] unknown vendor: "${vendor}"')`. This is the pattern all switches should follow.

The current inconsistency is dangerous because the modules that DO have `default: throw` (conditions, component) will catch mistakes, while the modules that DON'T (commands, execute, gather, trigger) will mask them. The developer gets a false sense of security from the fact that "some modules throw on unknown kinds."

## How Fixing This Improves the Codebase

1. **Runtime exhaustiveness**: Every switch on a discriminated kind gets `default: throw new Error(...)`.
2. **Immediate feedback**: When a new kind is added to the plan schema but a handler is forgotten, the runtime throws on first use with a clear message.
3. **Consistent with existing patterns**: `conditions.ts` and `component.ts` already do this. The fix aligns the rest.

## How This Fix Will Not Break Existing Features

- All existing command/reaction/gather/trigger kinds are already handled in the respective switches. The `default` case only fires for unknown kinds — which cannot appear in any existing plan.
- Adding `default: throw` is purely additive — it adds error handling for a code path that currently does nothing.
- If a future plan evolution introduces a new kind before the runtime is updated, the error message will say exactly which kind is missing, accelerating the fix.
