# FusionOnboarding — Lifecycle Artifact Tree

Root CLAUDE.md is authoritative. The artifact tree under
`wwwroot/onboarding/fusion/` is the committed evidence base for the component
lifecycle (onboard / audit / upgrade). The stage order and gate commands live in
that folder's README; the goal lives at
`docs/superpowers/goals/onboard-or-audit-or-upgrade-sf-components-with-100-percent-behavior-coverage.md`.

## Invariants

- Generated artifacts stay generated. Discovery JSON, traces, and coverage
  matrices come from the skill's scripts run against installed package sources —
  never hand-edited, never version-hardcoded. Hand-written judgment lives only
  in the named judgment artifacts (name decisions, pattern map).
- Fail-closed: every public member of a component is a matrix row — accepted
  with proof or excluded with recorded evidence. No silent omissions.
- Judgment precedent is written back to the pattern map IN THE SAME COMMIT that
  applied it. Precedent left in chat dies with the session.
- The authoritative primitive map only gains strength: rows are added or
  tightened with evidence, never loosened or special-cased to make a component
  fit. A member the map cannot express stops the row.
- Traces are normalized and diffable — they are the upgrade-mode baseline.
- Coverage that cannot lie: a row is green only when its consequence proof
  exists — reads consumed by a pipeline, writes visibly changing state,
  `cancel`-style payload members proven by the action NOT happening.

## Dispatching agents from here

Explore/Plan research subagents skip CLAUDE.md files entirely — inline the
invariants an agent needs into its prompt. Read-only sweeps must stay read-only:
no commits, no sandbox mutations.

Run the skill's gate scripts before claiming any row closed; the verifier is
the authority, not session memory.
