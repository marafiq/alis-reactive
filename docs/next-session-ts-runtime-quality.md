# Session: TS Runtime Quality — SonarQube + Vendor Isolation

Branch: create `fix/ts-runtime-quality` from `refactor/api-surface-xml-docs`

## Goal

Fix 3 SonarQube CRITICALs and 2 vendor isolation leaks in the TS runtime.

## Task 8: SonarQube CRITICALs

3 complexity hotspots from CoerceResult introduction:
- `conditions.ts` — cyclomatic complexity 24 (limit: 15)
- `commands.ts` — cyclomatic complexity 17 (limit: 15)
- `rule-engine.ts` — cyclomatic complexity 26 (limit: 15)

**Approach**: extraction refactors. Each switch case or nested conditional becomes its own function. The A/B audit of commands.ts (from skill review session) provides detailed analysis.

**Layers**: 3 (TS Runtime)
**Skills**: Load `solid-ts-audit` first
**Tests**: `npm test` must pass. Run `./scripts/sonar-analyze.sh` to verify quality gate.

## Task 9: Vendor Isolation Leaks

2 known leaks where vendor string checks exist outside `resolution/contracts.ts`:
- `execution/trigger.ts` — vendor-specific event-root logic
- `validation/live-clear.ts` — vendor-specific event wiring

**Approach**: Move vendor-specific logic into `resolution/contracts.ts`. Adding a third vendor must only touch `resolution/contracts.ts`. These leaks force changes across the runtime.

**Layers**: 3 (TS Runtime)
**Skills**: Load `solid-ts-audit` first
**Tests**: `npm test`, Playwright tests, architecture enforcement tests

## Context

- SonarQube: `./scripts/sonar-analyze.sh` — one-command analysis
- Issue #54 tracks the 3 CRITICALs
- Forensic index: M18 (vendor leaks), M31 (quality gate)
- Session todo: `.claude/memory/session_2026_03_28_todo.md` — Tasks 8-9
- A/B audit of commands.ts: found repeated Mutation switch (commands.ts + element.ts), decided NOT to refactor (stable, different targets)
