# rc3 Goal Execution — Onboard/Audit/Upgrade SF Components

The working document for executing
`docs/superpowers/goals/onboard-or-audit-or-upgrade-sf-components-with-100-percent-behavior-coverage.md`.
A fresh session starts here. Every fact below was verified in this repo on 2026-06-11
(commands stated). Nothing is recorded in the present tense unless it exists on disk —
deliverables that do not exist yet are work items with acceptance evidence, never
descriptions.

## Loop entry contract (deterministic, fresh-context safe)

1. Read the goal file above — it is the per-iteration prompt (4,000-char hard budget,
   owner-refined; do not edit it to add pointers — pointers live here).
2. Read the spine skill BY PATH: `.claude/skills/onboard-fusion-component/SKILL.md`.
   Its `disable-model-invocation: true` is deliberate and stays: explicit path-loading is
   more deterministic than description-matching, and the loop is the only consumer.
3. Name the next red row mechanically (never self-assess):

   ```bash
   node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs
   ```

4. Exit gate per component (a row is closed only when this passes):

   ```bash
   node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component <name>
   ```

5. Git + committed artifacts are the only memory. Judgment precedent is written to
   `tools/FusionOnboarding/wwwroot/onboarding/fusion/_skill/pattern-map.md` in the same
   commit as the row it explains.

Reality check the goal's compressed phrasing against disk: the artifact tree, gate
chain, fail-closed matrix + verifier, pattern map, and primitive map EXIST (the spine).
The orchestrator, mode flags, package-resolved versions, and CI verifier sweep DO NOT
exist yet — they are stories S1/S7 below. Do not hunt for them; build them.

## Verified current state (2026-06-11)

| Fact | How verified |
|---|---|
| 51 components inventoried; 1 audited (459/459 matrix rows proven); 50 at next stage `static-discovery` | status reporter run — `static-discovery` is valid first work, not a blocker: the gate chain's first stage produces those artifacts |
| Both PreToolUse hooks enforce: `plan.ts`/generated JSON+traces deny; judgment `.md` files pass; raw Playwright `dotnet test` denies; `scripts/playwright.sh` passes | piped PreToolUse JSON into both hook scripts (4 cases re-run) |
| Skill resolution is canon-only for DSL/fusion names | 9 stale Alis-specific user-level skills archived (list below); refreshed skill list shows single entries |
| Generator is ONE name everywhere: `PlanContractGenerator` | `npm run typecheck` green through `tools/PlanContractGenerator/`; regenerated `plan.ts` byte-identical; `dotnet build` (slnx) 0 warnings 0 errors |
| No orchestrator / `--mode` flag / CI verifier sweep | `ls` of skill scripts; `grep -rln verify-fusion .github/` empty |
| Versions hardcoded in spine SKILL.md examples (`33.2.10`, `32.2.8` at lines ~269–346) | grep — removed by S1, whose acceptance includes a zero-pins grep |

## Fixes executed 2026-06-11 (verified, committed working tree)

1. **Stale skill contamination removed.** `~/.claude/skills/` carried Apr-2026 copies of
   `onboard-fusion-{component,input,display,app-level}` and `syncfusion-slice` teaching
   stringly `self.Set("prop", v)` / `self.Call("method")` (forbidden by root CLAUDE.md),
   plus `conditions-dsl`, `http-pipeline`, `reactive-dsl`, `solid-ts-audit` — older
   duplicates (Mar vs May) of the project skills under the same names; one name must
   have one home. All nine moved to
   `~/.claude/skills/_archive-alis-2026-06-11/` (restore = move back). Without this, an
   autonomous iteration auto-loads the stale skill on its first onboarding task — the
   canon never auto-loads (step 2 above).
2. **Entry-point vocabulary now matches the canon.** Root `CLAUDE.md` skills row and
   `.claude/rules/process-task-types.md` now say artifact gate chain + fail-closed
   verifier instead of "7-file vertical slice" (the stale skills' vocabulary).
3. **Generator rename realized on disk** (was prose-only — an aspiration recorded as
   fact; `high-quality-bar-tasks.md` T5 tracked it as "one concept, three names"):
   `tools/PlanContractGenerator/PlanContractGenerator.csproj`, assembly
   `Alis.Reactive.PlanContractGenerator` (matches `InternalsVisibleTo` in
   `Alis.Reactive.csproj`), runner namespace `Alis.Reactive.ContractGeneration` — the
   runner must not shadow the domain class `Alis.Reactive.PlanModel.PlanContractGenerator`
   (CS0234 proved it). Updated: `Alis.Reactive.slnx`, `Alis.Reactive.Assets/package.json`,
   `.claude/rules/plan-contract-boundary.md` `paths:`, `.claude/memory/quality-principles.md`.
   Verified: typecheck green, `plan.ts` byte-identical, solution build 0/0. Closes the
   generator row of T5.
4. **Goal file restored byte-identical to the owner's version** (3,994 chars). An interim
   edit that trimmed owner phrasing to fit a pointer was reverted — wrong trade. The
   disambiguation that edit attempted lives in this document instead (entry contract,
   reality check).
5. **Correction found by a blind fresh-session probe (2026-06-11).** This session had
   claimed the archived `reactive-dsl` copy "advertised removed ServerPush/SignalR
   triggers" — an inference from a description diff, never read at source. DSL source
   says both exist: `Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:65,77`
   (ServerPush overloads), `:102` (SignalR hub trigger), `runtime/types/plan.ts:444`
   (`kind: "server-push"`), sandbox usage in `HttpPipeline/RealTime/Index.cshtml`.
   Open skill gap: the project `.claude/skills/reactive-dsl/SKILL.md` documents neither
   trigger — add their rows the next time that skill is touched.

## Epics — the framework author says a sentence

**E1 — "Onboard the {component} into Alis.Reactive."** I receive a branch: the
component's public JS surface, minus private and DOM members, as a typed API in its
vertical slice; no duplicates; every member traced from raw EJ2 to a green senior-living
journey.

**E2 — "Audit the {component}."** Same end state for a component that already ships:
every existing public member classified proven / unproven / wrong name / duplicate /
missing proof; wrong surface corrected; pattern map updated.

**E3 — "Upgrade."** After a Syncfusion package bump: the named drift list first —
breaking / semantic / additive / cosmetic — then every breaking row re-proven. The audit
report names every changed public C# member, so package consumers read exactly what
broke.

Epic verification, all three: say the sentence in a fresh session; the run completes
unattended — one row per iteration, accepts from S3, exit from the verifier — and the
only human step is branch review. The branch shows verifier green, journeys green,
status reporter "closed", one row per commit, transcripts and commit ranges linked from
the component's `proof/audit-report.md`.

## Engine stories — actor: the automation engineer

Every story obeys: no new core DSL primitives, no TS runtime changes, no string member
names in public slice APIs.

### S1 — I run one command for any mode

`run-fusion-lifecycle.mjs --mode onboard|audit|upgrade --component <n>|--all` gives me
the next red row or "closed". It lives beside the existing spine scripts, written in
their style (exemplar: `report-fusion-onboarding-status.mjs`); it resolves package,
version, and d.ts/JS/XML paths through `discover-syncfusion-component.mjs`; onboard and
audit route to the status reporter, upgrade routes to the S6 driver. The skill rename
(goal Done 1) lands in this commit, with root CLAUDE.md and rules updated together.

Accept:
- Prints the next red row, or "closed", with no human input.
- Unknown component → error naming the discovery command to run.
- Works unchanged after installing a different SF version.

Verify:
- grid audit run (459/459 rows) is green and its output is identical to calling
  `verify-fusion-artifact-gates.mjs` directly — the orchestrator adds routing, never
  new truth.
- A not-yet-discovered component run names the missing artifact by path.
- grep over SKILL.md + scripts finds zero SF version numbers.

### S2 — I read one label per JS member, with the rule that decided it

`write-fusion-discovery-artifacts.mjs` (it already writes `public-api-surface.json`)
labels every member from d.ts + shipped JS + probe: candidate, DOM, private, or
builder-owned, plus a rule id. The rules are data: one committed file in the spine,
`references/member-label-rules.md`, seeded from grid's 459 judged rows. A new exclusion
is a new rule row there, never an inline special case.

Accept:
- d.ts member count == rows in `public-api-surface.json`. Nothing dropped silently.
- Two runs produce identical files.

Verify:
- Grid (~326 members) run twice, diff empty.
- A known private member is excluded with its rule; a known public method is a candidate.
- Hide one d.ts member from the input → the count check fails.

### S3 — I read the gate's accept/exclude decision with its evidence

Five checks per candidate, each writing its evidence row to the component's
`mapping/gate-evidence.json` (one row per member per check, naming the source: corpus
file path, trace key, Blazor XML line): used in the real Alis app (paths from
`tools/FusionOnboarding/usage-corpus.config.json`), used via plugins, present in the
Blazor package, stable across versions and traces, fits an existing DSL primitive. All
five agree → accepted. Anything else → excluded with the evidence. Owner flips at branch
review become override rows in the pattern map. No corpus config → nothing auto-accepts.
This story's commit also replaces the skill's judgment-questions section with these five
checks — that removes the current contradiction between goal and skill.

Accept:
- grid replay (459 human-proven rows) reproduces the audited accept/exclude set, or
  names the evidence for each difference.
- No accept with fewer than five evidence rows.

Verify:
- Run on grid + one new component; read the evidence rows.
- Delete one evidence row → that accept is refused.
- Delete the corpus config → zero accepts, run still completes.

### S4 — I get one typed C# member per accepted row; onboarding twice changes nothing

Each accepted row closes trace → primitive map → slice → proof. The C# imitates the
exemplar slices — `FusionAutoComplete` (input), `FusionTooltip` (display),
`FusionSchedule` extensions (method returns, multi-arg calls) — and the coverage matrix
is regenerated by `write-fusion-typed-api-coverage.mjs`, never hand-edited. No
duplicates means: one C# member per JS path (overloads keyed by owner+signature),
nothing the MVC builder already owns, and running onboard twice changes nothing.

Accept:
- Coverage matrix: every public slice member has exactly one row; verifier green.
- No slice member repeats a builder-owned static.
- Second onboard run on the same component+version → empty `git diff`.
- Typed members only; the plugin boundary is the one intentional exception.

Verify:
- Regenerate the matrix, run the verifier.
- Grep `Alis.Reactive.Fusion/Components/**` for public string-member APIs → zero.
- Run onboard twice, show the empty diff.

### S5 — I see every accepted member as one step of the component's journey

The journey is one sandbox screen per component (`Alis.Reactive.SandboxApp`,
HTTP + SQLite backed for stateful components) and one Playwright file per journey under
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/{Component}/`. Each accepted
member is a step of that screen's journey, one step per test; each step drives the page
as the journey's user would and asserts what that user sees, through the typed DSL.
Every member links raw trace → discovery → primitive map → slice → journey step.
Writable payload props (`cancel`) get the action-did-NOT-happen step. Each event variant
proven per trigger. Exclusions carry proof too.

Accept:
- Any missing link → verifier red, row named.
- Every journey step exists for its member; every member has its step.

Verify:
- Remove one trace link from a proven grid row → verifier red names it; restore → green.
- Blank one did-NOT-happen assertion → its Playwright test fails.

### S6 — After a bump, I read the named drift list before anything is fixed

`--mode upgrade` regenerates discovery + traces into `staging/` under the component
tree, diffs against the committed artifacts — they ARE the baseline — using the same
normalizer the trace runner already applies (ports, timestamps), classifies each change
breaking / semantic / additive / cosmetic, and lists them by name. A breaking change on
an accepted member turns the matrix red and names the C# member; the row re-enters the
chain. Cosmetic changes reopen nothing. Additive members become new candidates, never
auto-accepts.

Accept:
- Replays the known v33 ChipList/Mention drifts from pre-bump baselines, correctly
  classified.
- An accepted member whose trace shape changed stays red until its row re-closes — a
  contract break cannot pass silently.

Verify:
- Baseline replay equals the known v33 list (committed fixture).
- Rename one payload key in staging → breaking, red, member named.
- Ordering-only change in staging → cosmetic, nothing reopened.

### S7 — Every push verifies every component

One job in `.github/workflows/ci.yml` runs the verifier over all components on every
push. A red row fails the job and names component + row + missing gate.

Verify: break one row on a test branch → CI red names it; revert → green. CI log link
recorded in the audit report.

## Settled in grilling (2026-06-11)

- Automate: one sentence in, finished component out; only branch review is human.
- Nothing invented: onboard what Syncfusion ships, minus private and DOM noise, through
  the frozen DSL.
- Proof is a senior-living user journey running green in Playwright through the
  vertical slice. One journey step, one test.
- Diagnosis: if the test is logically right, go back to step zero — was the HTML trace
  and discovery correct — and walk forward.
- Upgrade breaks loudly: drift list and audit report name every changed public C#
  member. No shims.
- Gate questions settle by replay against grid's 459 proven rows, not debate.

## Resolved decisions (do not relitigate in future sessions)

- Goal file: owner text byte-identical; pointers and disambiguation live HERE.
- `disable-model-invocation: true` on the spine skill: keep (explicit path-load).
- Skill judgment section: replaced by the five checks in the S3 commit — no interim edits.
- Generator name: `PlanContractGenerator` everywhere (done; see fix 3).
- Archived user-level skills: restore only if a non-Alis project needed them (none do —
  all nine are Alis-specific).

## The one open owner input

Path(s) to the real Alis app source (and any plugin corpus) for oracle (1), to be placed
in `tools/FusionOnboarding/usage-corpus.config.json` when available. Until then the gate
runs fail-closed as defined in S3 — nothing blocks the other stories.
