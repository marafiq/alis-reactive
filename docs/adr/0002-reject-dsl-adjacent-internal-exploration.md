# ADR-0002: Reject DSL-Adjacent Internal Exploration That Risks Frozen Surface

- Date: 2026-04-04
- Status: recorded
- Scope: redesign decision process

## Bad Decision

Internal exploration drifted close enough to the frozen DSL boundary that it caused justified concern that the public surface might be touched as part of cleanup.

## Why It Was Wrong

Even if the public DSL was not formally changed, the exploration was not disciplined enough around the frozen-surface rule.
That is a leadership and process failure because it creates doubt where the contract should be absolute.

## Violated Rules

- `AGENTS.md` section 3: Frozen Public DSL
- `AGENTS.md` section 11: Decision Gate

## Impact Introduced

- reduced confidence in decision discipline
- created avoidable review noise
- increased risk of architecture work drifting into the wrong layer

## Corrective Decision

Treat any refactor path that even trends toward public DSL churn as invalid.
Force redesign work to happen entirely behind the frozen surface.

## Proof Of Correction

- frozen-DSL rule strengthened in `AGENTS.md`
- reviewer role charter now requires explicit confirmation that public DSL remained frozen

## Follow-Up Obligations

- every future review packet must include the frozen-DSL constraint explicitly
- any contributor who proposes a public-surface change during this redesign must stop and redesign
