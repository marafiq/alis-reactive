# Claude Setup Experiments — rc3 Readiness

Experiments run 2026-06-10 against the new harness (nested CLAUDE.mds, two
PreToolUse hooks, one paths-scoped rule, restructured rules corpus) before the
rc3 loop is allowed to trust any of it. Every claim below was observed in this
repo, not inferred from docs. Where docs and observation disagreed, observation
won and the guidance files were corrected.

## The setup under test

| Tier | Mechanism | Contents |
|------|-----------|----------|
| Deterministic | PreToolUse hooks (`.claude/settings.json` → `.claude/hooks/*.mjs`) | protect generated files (`runtime/types/plan.ts`, onboarding `discovery/*.json` + `traces/*.trace.json`); force `scripts/playwright.sh` over raw `dotnet test` |
| Always-loaded | root `CLAUDE.md` (517) + `process-pipeline.md` (58) + `process-task-types.md` (116) + `agent-dispatch.md` (208) | 899 lines, down from 1,126; one canonical 5-layer model; live contradictions fixed |
| Lazy | 6 nested CLAUDE.mds + `plan-contract-boundary.md` (paths-scoped) | directory invariants + boundary rituals, load on file touch |

## Experiments and observed results

| # | Experiment | Method | Observed | Implication for rc3 |
|---|-----------|--------|----------|---------------------|
| E0 | Hook scripts, 12 unit cases | piped PreToolUse JSON into both scripts | 12/12 — plan.ts and generated JSON/traces DENY; judgment `.md` files (pattern map, name decisions, event rows), status JSON, other runtime TS all pass; raw `dotnet test …Playwright…` DENY; wrapper, domain tests, build pass | the skeptic's feared failure (hook blocks same-commit pattern-map write-back) cannot occur — `.md` never matches |
| E1 | Bash hook, real tool call | a Bash command merely *embedding* the forbidden string was issued | hook BLOCKED it mid-session | hooks enforce immediately (no restart); false-positive class: string-matching sees the whole command text — echoing/quoting a forbidden command also blocks. Acceptable: rephrase and continue |
| E2 | Edit hook, real tool call | attempted Edit on `runtime/types/plan.ts` | BLOCKED with the corrected reason text | hook scripts are re-read per invocation — script fixes take effect instantly; settings.json hook wiring also took effect mid-session |
| E3 | Nested CLAUDE.md loading | read a Fusion slice file in the session that CREATED the file → nothing; fresh `claude -p` session, same read | fresh session: injected (model quoted the exact heading); creating session: NOT injected | guidance discovery is snapshotted at session start. The rc3 fresh-context-per-iteration architecture gets every guidance file correctly; a session that writes guidance never sees it itself |
| E4 | `paths:`-scoped rule | same two-session method, reading a `PlanModel/**` file vs a non-matching file | fresh session + matching file: rule injected (heading quoted); non-matching file: NOT injected; creating session: not injected | path-scoping works and does not false-fire; boundary ritual will be present exactly when plan-domain files are touched |
| E5 | rc3 entry gate dry-run | `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs` | mechanically named the state: 51 components, 1 audited (459/459 rows proven), 50 next-staged `static-discovery` | the loop's "name the next red row" entry point works today, unattended |
| E6 | Explore agent context | Explore subagent probed for CLAUDE.md content before/after reading a Fusion file | nested CLAUDE.md ATTACHED after the read | docs' "Explore skips CLAUDE.md" applies to the startup hierarchy only; on-read directory attachment still reaches research agents. Root + artifact-tree guidance corrected to say exactly this |

## Not verified (honest gaps)

- Compaction re-attachment of skills (first 5,000 tokens, 25k shared budget) —
  docs-only claim, will be observed naturally during long rc3 sessions.
- Whether hooks fire for tool calls made by subagents — not yet tested; rc3
  read-only sweeps don't Edit/Write, so exposure is low. Test before any
  write-capable fan-out.
- `paths:` rule loading inside subagents — untested.
- The artifact-tree hook guard ran only against unit pipes + the real tree's
  paths, not yet against a full attended onboarding row. First attended rc3
  rows should confirm zero friction before the loop runs unattended
  (skeptic condition, accepted).

## Decisions taken from the judge pass (drift = top criterion)

- Adopted: two hooks (deterministic > prose); dissolve `process-layers.md`
  (unique content moved to `plan-contract-boundary.md`, `runtime/CLAUDE.md`,
  task-types docs section — both judges agreed); fix four live drift instances
  (4-vs-5 layer models → one canonical 5-layer in root; stale `resolver.ts`
  claim → three vendor roles; 8-step vs 9-step checklist → root Rule 3
  canonical; `PlanTypeGenerator` → `PlanContractGenerator`); dedupe BDD/plan-regen/
  war-story/protocol duplicates to one home each; root slimmed 561→517 with
  worked examples and the aspirations ledger moved to `.claude/memory/` canon.
- Rejected (skeptic won): lazy-converting `process-pipeline`/`process-task-types`/
  `agent-dispatch` — their rules are event-triggered (routing, speed gate,
  dispatch, escalation), not path-triggered; lazy loading would arrive too late.
  Revisit after rc3's verifier has earned trust.
- Deferred, recorded: `modern-csharp` skill split (1,272 lines vs 500 guidance)
  via progressive disclosure; deep root shrink toward ~210; AGENTS.md
  line-level dedupe (Codex consumer); `bdd-testing` skill description cleanup.
