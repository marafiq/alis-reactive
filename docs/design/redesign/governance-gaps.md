# Governance Gap Analysis — Rewrite to 1.0.0

This document is the synthesis of an adversarial simulation of the rewrite governance
(the DESIGN-AND-PROVE-FIRST, CODE-LAST 6-phase spine + the builder→judge→integrate→commit
autonomous loop). Each of 17 failure modes (FM1–FM17) was traced through every persona, gate,
and handoff to ask one question: **would a real defect of this kind reach 1.0.0 RELEASED?**

The runnable simulator that produced these traces is
[`governance-simulation.html`](./governance-simulation.html) — open it in a browser to step
each failure mode through the spine and watch where it is caught or where it escapes. Every
claim below was re-verified against live source on branch `all-sf-components-onboarded`
(see the inline `file:line` citations).

## The principle that judges all governance

> Complex process does NOT guarantee success — solving the problems that genuinely NEED
> process does. Every governance element must map to at least one REAL failure mode it
> prevents. An element mapping to none is **CEREMONY** and must be cut. A failure mode with
> no catching element is a **GAP** and must be closed.

Reliability here is **engineered, not promised**. The owner is explicitly not in the loop, so
"a persona will be careful" is not a control — only a machine-checked, SHA-bound, independently
reproducible witness is. The recurring root cause across the escapes below is the same:
**the governance names a catch in prose, but the gate that would mechanically enforce it either
does not run, runs against the wrong artifact, or trusts a persona claim instead of a captured
transcript.** A prose catch is not a catch.

---

## Part 1 — ESCAPES (governance GAPS that let a real defect reach 1.0.0)

Ranked by severity. Each is a defect that the simulation traced all the way to RELEASED, with
the minimal engineered fix. Two are CRITICAL (the verification apparatus itself reports green
while doing nothing), the rest are HIGH/MEDIUM.

### Critical

**FM9 — Silent feature loss reaches 1.0.0 (the prime directive is unguarded).**
The whole effort's gate is "zero feature loss," yet every fence (Greenweld policy, Crossjudge
per-slice RC1 diff, G-RC1-PARITY) only inspects behaviors that *already have a written matrix
row or a ported oracle test*. Nothing re-derives RC1's full feature set against the green-field
test set. Two escapes: (a) a live RC1 DSL variant never transcribed into a `04-matrix-*.md`
row gets no Phase-F slice, so no frozen assertion ever runs against it — `DONE` measures "full
oracle" against the tests physically *ported in*, never asserting `green_field_count == 1168`
(`REWRITE-GOAL-PROMPT.md:62`); (b) a user-visible assertion relabeled "oracle-internal
plan-shape" gets edited under policy-(b) tied to a plausible win, and nothing forces Crossjudge
to independently re-derive that it was user-visible.
**Minimal fix:** one unbypassable ORACLE-COMPLETENESS gate (Gatekeeper-owned, Crossjudge-run at
every integrate + hard at the cut): (1) a FROZEN ORACLE MANIFEST — snapshot RC1's 1168
behavior-assertions keyed by behavior-id; the green-field tree must contain a non-skipped,
executing test for EVERY id or the gate fails (closes "never-ported"); (2) RC1-SOURCED MATRIX
COMPLETENESS — G-MATH-100 fails if any reflection-extracted RC1 DSL variant has no matrix row
(closes "never-written-row"); (3) classification of an edited manifest test is re-derived by
Crossjudge from the assertion target (DOM/text/focus/disabled/PostData ⇒ policy-a FROZEN), never
self-asserted by the editor (closes misclassification).

**FM10 — Byte-stability / parity check prints PASS without ever running (verification theatre).**
G5-BYTE-STABILITY and G-RC1-PARITY are the named catchers, but their real implementation is the
dogfood console program `docs/design/redesign/dogfood/determinism-domain/Program.cs`, run by hand
via `dotnet run`, whose verdict is a `Console.WriteLine("DET-STABLE: HOLDS")` a persona eyeballs.
Verified swallow surfaces: the summary counts from a `stableFails=0` start (so `BuildCatalog()`
silently returning 0 rows prints "HOLDS over 0 plans"); `Plans = BuildCatalog()` is a static
initializer that shrinks coverage if a builder throws mid-rewrite; the only negative control is
behind an opt-in `-- selftest` arg the default `Main` never triggers. And `ContractDriftGate.cs`
has **zero test consumers** (verified: no `.cs/.sh/.yml/.json` references `ContractDriftGate`/
`HasDrift` outside the gate + generator). Crossjudge re-runs the same harness and reproduces the
same false green — reproduction confirms the printout, not that the loop ran.
**Minimal fix:** convert G5/G-RC1-PARITY into `[Test]` cases inside `dotnet test` (exit code,
not stdout); add a COUNT GUARD `Assert.That(Plans.Length, Is.EqualTo(EXPECTED))` plus a frozen
distinct-kind-token set so a shrunk catalog fails RED; wire `ContractDriftGate.Check()` into a
`[Test]` asserting `HasDrift == false` and let `File.ReadAllText` throw (never catch); run a
mutation-mode negative control on the DEFAULT path so the compare proves itself non-vacuous.

### High

**FM1 — Fresh-clone reproducibility runs too late; a gitignored/untracked dependency ships green.**
`.gitignore` ignores `dist/` (verified: `git check-ignore Alis.Reactive.Assets/dist` ⇒ ignored)
but whitelists `tools/PlanTypeGenerator/`. On the builder's polluted tree `dist/` already exists,
so the incremental `Inputs/Outputs` in `AlisReactiveBrowserAssets.targets` SKIPS `npm run
build:all`; G1 passes without proving `dist/` is regenerable. G5's git-clean check is the wrong
control — a gitignored bundle is invisible to `git status` whether present or absent. Crossjudge
re-runs the SAME gates in an equally-polluted worktree (persona-independent, not
environment-independent). G-FRESH-CLONE is the only true catch but the spine scopes it
"periodically and once at the 1.0.0 cut" — not in the per-commit fence.
**Minimal fix:** move fresh-clone reproducibility INTO the per-commit fence (gate
F-INTEGRATE→F-COMMIT). Before G1, delete `dist/`, `npm ci && npm run build:all && dotnet build`,
require `dist/` to repopulate — so reliance on an untracked input goes red on the slice that
introduced it, not at the cut.

**FM2 — A hallucinated TS-side plan symbol the C# domain never emits reaches release.**
G1 fully catches the C# leg (a nonexistent member cannot compile on `net48;net10.0`, verified
TFMs). The TS leg is not caught: the only C#↔TS cross-check is G3's drift detector, but on the
branch `ContractDriftGate.cs` has no test harness and `typecheck`/`build:runtime` both run
`generate:plan-types` FIRST (verified `package.json:9,10,16`), regenerating `plan.ts` before any
compare — so an invented `kind` that is self-consistent across hand-authored `plan.ts` + runtime
handler + the author's own vitest passes tsc, vitest, and C# tests.
**Minimal fix:** make G3 a real TS-vs-C# assertion over the COMMITTED `plan.ts`: wire
`ContractDriftGate.Check(committedPlanTs)` into `dotnet test` so it goes red when committed
`plan.ts != PlanContractGenerator.Render()`; remove the `generate:plan-types &&` prefix from
`typecheck` so `tsc` compiles the committed file; extend F-JUDGE to assert every runtime
`kind`/`assertNever` arm maps to a C# discriminator.

**FM3 — A public verb carrying infrastructure in its signature ships behavior-invisibly.**
A leaky seam (`Get(string baseUrl, string path)`, a `connectionString`/`HttpClient`/`vendorRoot`
param) is caught only at GATE_B — a single no-code human judgment with no machine artifact, and
the `deep-module-seam-design` skill that would encode the anti-pattern does not exist. Once it
passes GATE_B it is FROZEN into the Phase-E spec; the blind builder faithfully reproduces it and
G-BLIND-DEV *certifies the leak as "forced by fixtures."* No code gate inspects interface width:
a threaded value produces byte-identical plan JSON, so it passes every behavioral and determinism
gate, and RC1-parity (the loss oracle) is blind to an addition.
**Minimal fix:** one machine-checkable G-SURFACE gate (Gatekeeper-owned, Crossjudge-re-run):
(1) PUBLIC-SURFACE PARITY vs RC1 (PublicApiAnalyzers `PublicAPI.Shipped.txt` diff) — any public
verb whose parameter list gains an infra-typed parameter fails; (2) a NetArchTest/Roslyn
architecture test asserting no public DSL signature references an infrastructure type. Ship the
seam skill with a banned-parameter checklist Coldhand and Crossjudge can re-run.

**FM4 — Well-named, deterministic, correctly-laned scope creep inside a proven module ships.**
Phase A inspects load-bearing NAMES only (a clean name slips); Phase C walks matrix→code (extra
deterministic branches are invisible); Phase D draws only PROVEN rows; G1–G6 are all green for a
clean type-safe addition; RC1-parity is the *loss* oracle and produces no diff for an addition;
Crossjudge's authority is asymmetric — it REJECTS unreproduced findings, it does not COMPEL
positive enumeration. The one mandate that names FM4 ("reject out-of-spec additions") has no gate.
**Minimal fix:** operationalize it as a mechanical REVERSE-COVERAGE gate (code→matrix): every
public verb/overload, plan-node `Kind`, and runtime switch-case in the slice must cite a specific
matrix row (`file:line` in `04-matrix-*.md`); any symbol or branch with no citation FAILS.
Enforce with a per-overload census assertion — implemented-surface count == matrix count, an
inequality in EITHER direction is red (closes the under-coverage-only asymmetry that today only
catches loss).

**FM5 — "Verified/done" without a shown transcript; the overclaim relocates to the judge and escapes.**
Nothing FORCES the builder→judge transcript to exist; the two harness-level enforcers that would
block a commit/merge unless verification ran (`commit-requires-relevant-tests`,
`merge-requires-all-tests`) are both `enabled: false` (verified). So G1–G6 are "unbypassable"
only as persona instructions. F-INTEGRATE's entry is the *word* "Judge SIGN-OFF" — a verdict
token, not an inspected transcript with exit codes. The judge is itself an LLM persona; its
SIGN-OFF is also a claim, and there is no second re-runner above it. With the owner removed, the
only actor that historically caught overclaim is gone.
**Minimal fix:** flip the two hookify rules to `enabled: true`; require every gate to write a
captured-output evidence file (command + exit code + stdout tail) keyed to the commit SHA;
Gatekeeper's F-COMMIT entry refuses any slice lacking a fresh exit-0 transcript per gate.
SIGN-OFF is accepted only ALONGSIDE the machine-captured transcripts.

**FM6 — A silently stale TS/CSS runtime bundle makes new behavior never execute (sub-mode 6b).**
6a (Core-only build ⇒ 852/852 "Server did not start in 30s") IS caught by the fixture's loud
throw and the known stale-build signature. 6b is not: `dist/scripts/alis-reactive.dev.js` is a
gitignored esbuild output no C# gate triggers; G3/G4 run TS SOURCE not the bundle; G5's git-clean
is blind to a gitignored file; G-ORACLE-SLICE only asserts the bundle returns HTTP 200 (a stale
bundle returns 200 fine), and `build:all` before the suite is operator prose, not a precondition.
The gate that runs the browser doesn't guarantee a fresh bundle; the gate that guarantees a fresh
bundle (G-FRESH-CLONE) doesn't run the browser.
**Minimal fix:** make bundle freshness a PROGRAMMATIC precondition of the oracle gate — a fixture
pre-step `npm run build:all` (or assert `dist/**` mtimes newer than every `runtime/**` source,
else fail with "stale bundle"); embed a content-hash build-stamp and have the readiness probe
assert the served `/scripts/alis-reactive.dev.js` hash equals the just-built source hash.

**FM7 — A silently main-based worktree corrupts the measurement apparatus (false negatives).**
`isolation:'worktree'` silently roots at main HEAD — 761 commits behind the feature branch on
this repo (verified `git rev-list --count main..HEAD`). Every per-slice gate then runs inside the
stale tree: G1–G6 pass on stale code or report "missing module"; G-RC1-PARITY reports
base-staleness AS feature loss (a false-negative amplifier); Crossjudge honestly-but-wrongly
concludes the feature is missing (the documented 2026-03-28 / 2026-05-31 recurrences). The one
gate with the right idea, G-FRESH-CLONE, uses the FILE-PRESENCE proxy (README/scripts present),
which a main-based tree passes — and it runs only periodically.
**Minimal fix:** an unbypassable per-worktree COMMIT-IDENTITY preflight at worktree CREATION and
as the first step of every gate run: assert `HEAD == feature-branch tip` (fetched fresh),
`rev-list --count feature..HEAD == 0`, and `git cat-file -e HEAD:<feature-only-path>` for a
manifest of paths absent on main. Ban `isolation:'worktree'` for any branch ahead of main; use
`git worktree add --detach <path> <feature-branch>`.

**FM8 — A frozen behavior assertion is quietly weakened under cover of a legitimate plan-shape edit.**
~50 of 133 Playwright files MIX frozen (a) user-visible and updatable (b) plan-shape assertions.
A legitimate parked-win (new `lane` field, new `whole-payload` kind) forces a real (b)
re-baseline; under that cover a genuinely-(a) assertion in the same file is weakened to dodge red.
Every downstream gate checks only that the suite is GREEN, not WHY an expected value moved or
which population it belongs to. None of the 12 hookify rules enforces the (a)/(b) split.
**Minimal fix:** make the boundary MECHANICAL — tag assertions `[FrozenBehavior]` vs
`[OracleInternal]` and SPLIT the ~50 mixed files so no file holds both; add a hookify
`oracle-frozen-assertion-guard` (PreCommit) that BLOCKS any diff modifying a FROZEN expected
value unless the commit carries an explicit `ORACLE-EDIT:` note AND touches only OracleInternal
assertions; re-scope Coldhand's blind-BDD reviewer to run on every per-slice diff touching a
tagged oracle file.

**FM12 — A large design artifact (matrix/proof/certificate) is lost mid-generation; phase work silently re-burned.**
The only FM12 defense (the F-COMMIT resumable checkpoint) is bound to Phase F slices; Phases A–E
exit criteria require the artifact to EXIST/be RECORDED/be DRAWABLE, never to be COMMITTED before
the handoff. A Phase-C `Lawprover` emitting a 540-line certificate inline hits the context limit
mid-artifact; no gate checks deliverable durability, the next session re-derives from scratch, and
the redo is silent (the eventually-committed artifact looks identical). Worse, the two commit
hookify rules would BLOCK an emergency checkpoint commit of a test-less design doc.
**Minimal fix:** generalize the F-COMMIT checkpoint barrier to EVERY phase — each A–E exit
criterion must WRITE the deliverable to its path AND git-commit it (artifact-commit naming the
artifact) before the handoff crosses. Exempt design-doc/proof/spec commits from
`commit-requires-relevant-tests`. Give Greenweld/Gatekeeper authority over the artifact-checkpoint
cadence across all phases.

**FM13 — A self-asserted 100% census with an uncovered variant / unenumerated composite ships.**
The G-MATH-100 unlock token is `07-determinism-certificate.md`, which carries ONLY the Shape
algebra laws — the 375 per-variant census lives in `05`/`06`, not the gated file. The 375 is a
hand-summed markdown tally reconciled by `bc` (`06` fixed a stale 371→375 carry-over); no harness
enumerates the overloads from source and asserts one-output-each — the identical "counts families
not rows" property that the discarded 120/120 headline had. The question was re-scoped between
rounds (263/284 with a live collision became 375/375 by excluding dead surface and labelling 21
overloads RESOLVED-BY-REDESIGN — green against an admittedly-UNBUILT spec). And 375 is a
per-module SUM; cross-module COMPOSITE variants (Confirm.And ordering across Conditions×Value,
Into(whole-payload) across HTTP×Value×Slot) were never census rows, so a non-deterministic
composite cannot surface as "uncovered."
**Minimal fix:** make the census MACHINE-DERIVED and the certificate the GATED artifact — a
committed harness reflects the public DSL surface from the real assembly and EMITS the
denominator; the certificate's X/X must equal that reflected count (kills the hand-summed 375 and
the families-not-rows class). Add a COMPOSITE-VARIANT scope row per cross-module graph edge, with
its own one-output proof; G-MATH-100 rejects a certificate whose composite-row count is below the
labelled cross-module edge count. Bind the certificate to BUILT source — mark any
RESOLVED-BY-REDESIGN row a RED sub-certificate until the resolving fix actually ships.

**FM14 — C#/TS contract drift is auto-absorbed by a regenerate-then-typecheck tautology.**
The governance describes G3 as a comparison gate against a hand-authored `plan.ts` ("No generate
step, Directive 1") — the codebase implements the OPPOSITE. Verified: `plan.ts` is git-tracked,
its header says `<auto-generated />`, `generate:plan-types` runs WITHOUT `--check` and OVERWRITES
the tracked file, then `tsc` checks the generator's own fresh output against itself. The real
drift CHECK (`PlanTypeGenerator --check` ⇒ `ContractDriftGate.Check`) is invoked by NOTHING. So a
new C# node is auto-absorbed (regen matches), a regeneration that diverges is invisible (no
git-clean assertion on `plan.ts`), and a hand-edit is silently clobbered.
**Minimal fix:** change `typecheck` from `generate:plan-types && tsc` to
`dotnet run --project ../tools/PlanTypeGenerator -- --check runtime/types/plan.ts && tsc`
(the `--check` path already calls `ContractDriftGate.Check` and exits 1 with a diff). Add a
Gatekeeper G5 assertion: `git diff --exit-code -- runtime/types/plan.ts` is clean. Reconcile the
charter: pick ONE model (hand-authored + drift-check OR generated + git-clean-check) so the wired
gate matches the written spine.

### Medium

**FM11 — A single load-flake TimeoutException wastes the full-suite budget; orphan Chromium accumulates.**
The flake-vs-real rule is prose in the Gatekeeper charter; the gate the loop runs (`scripts/test.sh`)
has NO retry, NO `--filter`, NO orphan kill. The fixture binds a RANDOM port (verified
`GetAvailablePort()`) so orphans never cause "address already in use" — they silently accumulate as
~99% CPU, inflating timeouts into more flakes; `server.Kill(entireProcessTree:true)` runs only on a
clean teardown (verified `WebServerFixture.cs:91`) and never kills headless Chromium; the human
cleanup advice targets port 5220, which the fixture never uses. FM11 does not produce a WRONG
release (behavior/byte gates hold) — it reaches 1.0.0 as silent process debt + burned budget.
**Minimal fix:** put the prose in the gate the loop runs — a PRE-gate orphan sweep by
process-name (`pkill -f Alis.Reactive.SandboxApp; pkill -f 'chromium.*--headless'`, not by port);
an in-gate filtered retry of only the failed tests from the TRX with an orphan sweep between
attempts, treating pass-on-retry as flake/green; a finally-block kill in the fixture.

**FM15 — A ceremonial governance step that catches no failure mode rides through every loop forever.**
No persona owns process-cost-vs-yield at runtime. The only ceremony defense is a one-time
design-time self-assessment ("none found") — unfalsifiable, no adversary, never re-run.
Gatekeeper's phase-order enforcement actively PROTECTS a redundant step (it would block removing
it). A step that catches nothing always passes, so every product gate stays green and it reaches
1.0.0 — cost paid forever, attention diluted, false thoroughness inflated.
**Minimal fix:** give Gatekeeper a per-gate/persona/handoff YIELD LEDGER — the falsifiable mirror
of the coverage matrix. Each element records, across a module cycle, the UNIQUE reproduced
failure-witness it caught. Any element with zero unique catches, or whose every catch is also made
by an earlier automated gate, is flagged CEREMONY and cut or justified in writing — the same
rigor already applied to untested schema `$defs`.

**FM16 — A contaminated "blind" developer passes G-BLIND-DEV more easily, not less; learnability claim proves nothing.**
G-BLIND-DEV inspects the blind dev's OUTPUT (green, judge source-diff, BDD survival). A dev who
saw the design docs or is the same orchestrator context that ran Phases A–C passes ALL three more
easily. No gate, persona, or doc specifies HOW the orchestrator verifies blind context — "real
source forbidden" is an honor-system rule the dev self-follows and the README self-attests. There
is a G-FRESH-CLONE enforcing a clean FILESYSTEM but no analogue enforcing a clean CONTEXT WINDOW.
**Minimal fix:** a blind-context provenance manifest as a REQUIRED, Crossjudge-inspected input —
the context-window analogue of G-FRESH-CLONE. The orchestrator spawns Coldhand as a sub-agent
whose entire input is exactly {one module's spec, its named fixtures} and emits a machine-checkable
manifest (literal file/section list + transcript hash proving zero reads of `Alis.Reactive/**`,
other specs, matrices, prior scratch, or coaching). SIGN-OFF requires reproducing that the manifest
is spec+fixtures ONLY; absent/impure ⇒ rejected as "not actually blind."

**FM17 — Phase D's simulator is a second implementation in English; its named gate cannot perform the check it is credited with.**
The Phase D simulator (`playground/design-graph.js`) is 63 hand-authored `did:` prose strings
drawn as arcs, with ZERO linkage to the real bundle. The Phase D exit bar is only "every proven
matrix row is DRAWABLE" — a confidently-WRONG `did:` string is still drawable, so it passes; at
Phase D the runtime bundle does not yet exist (CODE-LAST) so GATE_D physically cannot compare to a
real runtime. The simulation matrix credits GATE_D with a "load real bundle / same-bytes" check
its spec does not contain. The wrong behavior is "proven twice" before any code runs and is caught
only later at G-ORACLE-SLICE (which is why this does not reach a WRONG release — but the named gate
is a false credit and the simulator is a permanently-drifting artifact). This is the empirical
basis for cutting Phase D (Part 2).
**Minimal fix:** if Phase D is kept, make GATE_D exercise the SAME read modules the bundle ships
(`evaluateValue`/`evaluateCondition`/`executeReaction` over the fixture's plan JSON) and render the
OBSERVED trace, not a `did:` string; bind each step to a Phase-C certified matrix-row id; correct
`governance-simulation.html` to set `FM17.primary = G-ORACLE-SLICE`. The Part 2 recommendation is
to CUT Phase D as a gate entirely.

---

## Part 2 — CEREMONY to cut (elements that catch no UNIQUE failure mode)

These pay cost on every loop iteration without uniquely catching any failure mode. Cutting them
removes drift surfaces and handoff cost with zero loss of FM coverage.

1. **Phase D (HTML Simulators) + Flowwright + the C→D and D→E handoffs + the `frontend-design`/
   `playground` skills — CUT from the critical path.** FM17 proved the simulator is a non-oracle
   ("provisional until re-observed in Phase F Playwright") that adds a hand-built second
   implementation — the exact FM17 risk. Its only real STOP ("undrawable flow = unwritten matrix
   row") is already owned by G-MATH-100 (Phase C cannot certify a row it never enumerated). Demote
   any human-legible diagram to an OPTIONAL doc produced FROM the Phase C certificate + the Phase F
   green slice — never a gate, never a STOP authority, never on the unlock path.

2. **The standalone `G-BLIND-DEV` gate object — MERGE into the Phase E exit criterion.** They are
   verbatim duplicates: two named governance objects, one check. Keep the Phase E stage (it IS the
   gate); delete the separate gate entry.

3. **FM2 (hallucinated symbol) credited to G-BLIND-DEV / Coldhand — STOP crediting it there.**
   G1-BUILD is the mechanical primary (a nonexistent member cannot compile on either TFM). The
   blind-dev's grep is strictly dominated by the compiler for this FM; double-crediting inflates the
   apparent value of Phase E. Keep blind-dev only for FM16 (learnability) and FM8 (impl-coupled test).

4. **G2 and G4 credited as FM9 (feature-loss) catchers — STOP crediting them.** A SILENTLY DROPPED
   feature has no test in G2/G4 to fail (you cannot fail a test for a path you deleted). Only the
   differential-vs-RC1 oracle catches FM9. Justify G2 on FM8 (shape assertions kept where shape IS
   behavior, keeping the Playwright oracle pure) and G4 on read-path correctness.

5. **G-ORACLE-SLICE as a gate distinct from G-RC1-PARITY — MERGE into one behavior-oracle gate.**
   Both run the same 1168-test Playwright oracle against the same freshly-built bundle for the same
   FM9/FM8. Run one behavior-oracle gate per slice with two documented assertion tiers (frozen
   user-visible vs updatable plan-shape); keep the stale-build precondition (FM6) and flake protocol
   (FM11) on that single gate. Eliminates one full redundant Playwright invocation per slice.

6. **FM6 split-ownership between G1 and the oracle gate — OWN it in one place.** The stale-build bite
   happens at the Playwright precondition; drop the FM6 mention from G1 (G1's unique value is FM2 +
   the net48 leg).

7. **FM1 credited to G5's git-clean half AND G-FRESH-CLONE — fold the git-clean line into G-FRESH-CLONE.**
   G-FRESH-CLONE is the only complete catcher (verified gitignore: `dist/` ignored,
   `tools/PlanTypeGenerator/` whitelisted). G5's git-clean catches only the narrow tracked-bundle
   sub-case. Keep G5 ONLY for its unique FM10 catch.

8. **F-JUDGE / F-INTEGRATE / F-COMMIT as three separate gated stages — COLLAPSE into one
   "Verify-and-land" stage.** No failure mode lives uniquely "between integrate and commit"; they are
   three steps of one atomic transaction. Keep the two PERSONAS (judge independence is real,
   FM5/FM10); stop modeling merge and commit as separate gated stages. Reduces three handoffs to one.

9. **Greenweld as an actor distinct from Gatekeeper + Crossjudge — MERGE.** Integrate-on-green =
   Crossjudge's reproduced-green verdict; worktree base = Gatekeeper's FM7 check; one-row-per-commit =
   a commit-message lint. The only uniquely-Greenweld thing is the frozen-vs-updatable policy, which
   is a POLICY, not a role — assign it to Crossjudge (who reviews assertion deletions anyway).

10. **G3-DRIFT as currently specified ("vs HAND-AUTHORED plan.ts, NO generate step") — REWRITE, not
    keep.** The premise is false against source (codegen is alive: `plan.ts` header says
    `<auto-generated />`, `generate:plan-types` exists and overwrites). As written it guards a
    contract that does not exist. Rewrite to RUN the generator and diff generated-vs-committed
    (FM14 fix); do not count the as-specified G3 as live FM14 coverage.

---

## Part 3 — HARDENED LEAN GOVERNANCE

The smallest set of personas, gates, and handoffs that catches every real failure mode with no
ceremony. This is the de-duplicated, gap-closed spine.

### Phases (kept) — A, B, C, E, F (Phase D CUT)

| Phase | Keep because (UNIQUE FM) |
|-------|--------------------------|
| **A — Nail the Language** (Grammarwright) | FM4 vocabulary that lies; FM12 frozen vocab handoff |
| **B — Deep Modules + Seams** (Seamsmith) | FM3 leaked infrastructure flagged at design — now backed by mechanical G-SURFACE |
| **C — Prove the Math** (Lawprover) | FM13 per-variant census — now machine-derived + composite rows + built-source binding |
| ~~D — HTML Simulators~~ | **CUT** — FM17 non-oracle; STOP already owned by G-MATH-100 |
| **E — Blind Developers** (Coldhand) | FM16 learnability; FM8 impl-coupled test survival — now with a context-provenance manifest |
| **F — Build / Verify-and-land** (Buildhand → Crossjudge → land) | the production loop; F-JUDGE/INTEGRATE/COMMIT collapsed to one stage |

### Personas (4, down from 6)

| Persona | Owns (UNIQUE FM) |
|---------|------------------|
| **Gatekeeper** | unbypassable spine order + the per-commit fence; owner-out-of-loop = the gates ARE the approval; net48-leg; orchestrates the per-slice fresh-clone + commit-identity + transcript checks; the YIELD LEDGER (FM15) |
| **Crossjudge** | independent reproduce-before-accept (FM5); spot-injects a known mismatch to prove a gate CAN go red (FM10); adversary panel on the proof denominator (FM13); frozen-vs-updatable classification re-derived not self-asserted (FM8/FM9) |
| **Buildhand** | Phase F implementer, weakest authority — writes only against a green certificate; escalates a design-decision-in-a-fill rather than invent (FM4); cannot report from uncommitted edits (FM5) |
| **Coldhand** | the blind builder — learnability/spec-completeness (FM16) + impl-coupled-test review (FM8), with a Crossjudge-inspected context-provenance manifest |

*(Greenweld + Flowwright removed — their unique value folded into Gatekeeper/Crossjudge or cut with Phase D.)*

### Gates (the machine-checked witnesses)

| Gate | UNIQUE FM caught | Hardening applied |
|------|------------------|-------------------|
| **G1-BUILD** (net48 + net10.0) | FM2 (C# leg) | unchanged — the compiler is the mechanical catch |
| **G2-CSHARP-TESTS** | FM8 (shape kept where shape IS behavior) | reverse-coverage census assertion added (FM4) |
| **G3-DRIFT** | FM14 (C#↔TS) | REWRITTEN: `--check` diff over committed `plan.ts`; `ContractDriftGate.Check` wired into `dotnet test`; `git diff --exit-code -- plan.ts` |
| **G4-VITEST** | FM8 (read-path, distinct layer) | unchanged |
| **G5-BYTE-STABILITY** | FM10 | converted to `[Test]`; COUNT GUARD + frozen kind-set; default-path mutation negative control |
| **G6-RENDER-PERF** | perf regression (no other gate) | unchanged |
| **G-SURFACE** (NEW) | FM3 | PublicAPI.Shipped.txt parity vs RC1 + NetArchTest: no infra type in a public DSL signature |
| **G-MATH-100** | FM13, FM4-cap | machine-derived denominator; composite-variant rows ≥ cross-module edges; RED until RESOLVED-BY-REDESIGN rows ship |
| **G-FRESH-CLONE** | FM1, FM7 | moved INTO the per-commit fence (delete `dist/`, `npm ci && build:all && dotnet build`); commit-identity preflight (rev-list + `cat-file -e`) replaces the file-presence proxy |
| **Behavior-oracle gate** (merged G-ORACLE-SLICE + G-RC1-PARITY) | FM9, FM6, FM11 | `build:all` + served-bundle hash precondition (FM6); orphan sweep by name + filtered TRX retry (FM11); two assertion tiers |
| **G-ORACLE-COMPLETENESS** (NEW) | FM9 | frozen 1168-id RC1 manifest, every id mapped to a non-skipped executing test; RC1-sourced matrix completeness; classification re-derived by Crossjudge |
| **Transcript / commit-fence** (hookify + evidence files) | FM5 | `commit-requires-relevant-tests` + `merge-requires-all-tests` set `enabled: true`; every gate writes a SHA-bound exit-code+stdout file; SIGN-OFF accepted only with the transcripts |
| **Artifact-checkpoint barrier** (extended F-COMMIT) | FM12 | every A–E exit requires write-file + git-commit; design-doc commits exempt from the test-required rule |
| **`oracle-frozen-assertion-guard`** (NEW hookify) | FM8 | blocks any diff weakening a `[FrozenBehavior]` expected value without an `ORACLE-EDIT:` note touching only `[OracleInternal]` |
| **YIELD LEDGER** (NEW, Gatekeeper) | FM15 | per-element unique-catch record; zero-unique-catch element flagged CEREMONY |
| **Blind-context manifest** (NEW input to Phase E) | FM16 | spec+fixtures-only input list + transcript hash proving zero forbidden reads |

### Handoffs (de-duplicated)

A→B (frozen vocabulary), B→C (fixed acyclic seams + the precise interface boundary + G-SURFACE
signature record), C→E (green machine-derived certificate + hardened spec/fixtures — *D removed*),
E→F-BUILD (blind SIGN-OFF + context-provenance manifest + 7-wave order), and ONE
**F-BUILD → Verify-and-land** handoff carrying the slice + the SHA-bound evidence bundle (transcripts,
byte-compare, perf, the matrix row id). Every A–E handoff now crosses only after the deliverable is
committed (FM12). The owner approves nothing mid-flight — the gates ARE the approval.

---

## Part 4 — REASONING: why this is problem-justified, not complex-for-its-own-sake

**Every retained element maps to at least one real, traced failure mode; every cut element mapped
to none (or only to FMs an earlier mechanical gate already catches).** This is the test the
principle demands, applied in both directions: GAPs closed with the minimal fix, CEREMONY cut.

**The lean spine is SMALLER than the original, not larger.** Phase D is cut, two personas
(Greenweld, Flowwright) are removed, three F-stages collapse to one, one redundant Playwright
invocation per slice is eliminated, and four false-credit FM mappings (FM2→blind-dev, FM9→G2/G4,
FM1→G5, FM6→G1) are removed. The NEW gates are not new ceremony — each closes a specific traced
escape (G-SURFACE↔FM3, G-ORACLE-COMPLETENESS↔FM9, the transcript fence↔FM5, the manifest↔FM16, the
ledger↔FM15) and most are a single assertion or a config flip, not a new phase or persona.

**The hardening converts prose into engineered guardrails — the recurring root cause of every
escape.** The simulation's signature finding is that the governance kept *naming* a catch that no
gate *performs*: G3 regenerates then typechecks its own output (FM14); G5/parity print a console
string a persona eyeballs (FM10); the transcript and the (a)/(b) split and the "reject scope
creep" mandate were persona habits with `enabled:false` enforcers (FM5/FM8/FM4); fresh-clone and
commit-identity ran too late or used a proxy that already failed twice (FM1/FM7); the byte census
was a `bc`-reconciled markdown tally (FM13). Each fix replaces a claim with a present-or-absent,
SHA-bound, independently re-runnable artifact: an exit code, a `git diff --exit-code`, a reflected
count, a 1168-id manifest mapping, a transcript hash. **An LLM persona's SIGN-OFF is never trusted
on its own word — it is accepted only alongside the machine-captured witnesses it must have
produced.** That is what makes the autonomous loop safe with the owner out of the loop.

**This is what makes the rewrite outcome genuinely guaranteed rather than promised.** "Zero
feature loss" stops being an aspiration the moment a dropped feature fails the manifest-mapping
gate (no executing test for its id) or the matrix-completeness gate (no row for its reflected
variant), and an edited frozen assertion fails the classification guard. "Verified" stops being an
overclaim the moment F-COMMIT refuses any slice lacking a fresh exit-0 transcript per gate. "100%
proven" stops being a self-asserted tally the moment the denominator is reflected from the real
assembly and the certificate stays RED until its rows are true-in-built-source. The simulator at
[`governance-simulation.html`](./governance-simulation.html) is the falsifiable record: every
remaining gate is one a defect of its named failure mode would turn RED before reaching 1.0.0, and
every cut element is one a defect could pass through green without consequence — which is precisely
why it earned no place in the spine.
