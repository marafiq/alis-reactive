# TS Runtime — Plan Executor

Root CLAUDE.md is authoritative. This directory is the browser executor of
framework-generated plans. Load `solid-ts-audit` for structural work.

## Posture

The runtime executes; it does not defend. Trust framework-generated plans — no
preflight, plan validators, claims, rejects, registries, fallback paths, or
speculative recovery for shapes the typed DSL already controls. Errors belong
at true external boundaries only: DOM lookup, browser APIs, network,
component/plugin lookup, malformed non-framework JSON. Fail loud at the source.

- DOM-lookup leaves speak DOM's language (`HTMLElement | null`); domain code
  uses undefined. No `??` hedges where a type or contract can carry the truth.
- `getElementById` only — the plan carries every ID. Wide queries are justified
  solely for plan discovery and self-stamped cleanup; the architecture test's
  allowlist is the only registry of exceptions.
- Vendor knowledge lives in exactly three roles: per-vendor driver, per-vendor
  event adapter, vendor component modules. Everything else stays vendor-blind.
- Sync reactions stay sync. Async is reserved for HTTP, remote triggers, user
  decisions, and partial injection.
- One value path: `ValueExpression` reads every source. A second resolver is a
  design smell — two operator evaluators already exist (conditions vs validation
  rules) and can diverge; do not add a third.
- SOLID lenses for structural work: SRP ("who requests changes?"), OCP (one
  switch case + `assertNever`), LSP (no vendor checks downstream), ISP (narrow
  exports), DIP (depend inward).

## Component lifecycle tripwire

Onboarding, auditing, or upgrading a Syncfusion component requires ZERO changes
in this directory. If you find yourself editing here during component work,
stop — the plan is missing information; fix the C# descriptor.

## Harness

`npm run typecheck` (regenerates the plan contract first), `npm test` (vitest +
architecture enforcement). Rebuild bundles before any Playwright run.
`types/plan.ts` is generated from the C# plan domain — never hand-edit it.
