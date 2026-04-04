# ADR-0003: Reject Rubber-Stamp Reviewing

- Date: 2026-04-04
- Status: recorded
- Scope: review process across all layers

## Bad Decision

Prior review rounds did not surface serious design defects early enough, including reflection-based metadata, dual-lane authoring, and weak test architecture.

## Why It Was Wrong

That is not merely a communication issue.
It means the review process was not acting as a real quality gate.

## Violated Rules

- `AGENTS.md` section 10: Reviewer Roles
- `AGENTS.md` section 11: Decision Gate

## Impact Introduced

- allowed bad architecture to survive longer than it should
- created false confidence from passing builds/tests
- increased total rework

## Corrective Decision

Require strict reviewer role packets, mandatory review outputs, explicit allowed decision outcomes, and repeated review rounds until defects are closed or disproven.

## Proof Of Correction

- reviewer role charters expanded in `AGENTS.md`
- mandatory review packet defined in `AGENTS.md`
- review output format and decision outcomes defined in `AGENTS.md`

## Follow-Up Obligations

- keep open findings live until fixed or disproven
- record future review-process failures here as ADRs
