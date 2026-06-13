# Onboard / Audit / Upgrade SF Components with 100% Behavior Coverage

**Goal:** automate onboard, audit, and upgrade of Fusion components' public API into
typed Alis Reactive DSL in the relevant component vertical slice, without DOM-related or
private-member noise — deterministic for all three modes, for one component or all.
100% consequence-proof coverage of every accepted member. Fully automated — judgment is
quarantined into one computable, evidence-scored accept/exclude gate. Precision-first:
under-accepting is cheap (additive later); polluting the C# API, an unproven member, or
a false drift alarm is failure.

**Why:** devs expect the full public SF API; the safe rate of approach to parity IS the
adoption rate. Scale: Grid ≈ 326 d.ts members vs 20 accepted typed events; ChipList
8→4. Flagships (Grid, ChipList, InPlaceEditor, Schedule) target near-parity. Known
trap: SF d.ts leans on `any` — static surface cannot classify members or payloads;
runtime probe traces (HTML probes today; better mechanism welcome) are the evidence of
record.

**Modes share one spine:** artifact tree, row gate chain, fail-closed matrix +
verifier, pattern map, authoritative primitive map. Core DSL FROZEN: new typed surface
only inside component slices, through the authoritative map — which only gains strength
(rows added/tightened with evidence, never loosened to fit). Grammar misfit = stop
condition, never improvisation. Onboard and audit exist: one orchestrator, mode flags,
versions from installed packages never hardcoded, verifier sweep in CI. Upgrade is new:
regenerate discovery+traces into staging, diff normalized baselines, classify
breaking/semantic/additive/cosmetic, emit a NAMED drift list; drift rows re-enter the
gate chain; scope O(accepted+new).

**The gate, computable — mechanical filters then five oracles:** DOM-only and
private/internal members are excluded by rule before judgment runs. Then: (1)
real-product usage mining (Alis app = ground truth); (2) plugin usage = revealed
demand; (3) Blazor overlap = vendor curation; (4) stability (presence across versions,
trace determinism); (5) grammar fit. Auto-accept only on full agreement; anything less
auto-excludes with evidence. Overrides feed the pattern map; thresholds tighten toward
zero override rate.

**Coverage that cannot lie** (verifier-enforced, per member kind): reads → consumed by
a realistic pipeline; writes/void methods → visible state change; returns → consumed
downstream; writable payload props (`cancel` is canon) → NEGATIVE SPACE: gesture done,
member set via typed DSL, action visibly did NOT happen; event variants proven per
trigger; exclusions are fail-closed rows with proof. Law: every assertion unsatisfiable
by the defect it guards.

**Operating loop — fully automated, no human mid-loop:** one row per fresh-context
iteration: status reporter + verifier mechanically name the next red row; close exactly
that row through the gate chain (trace → map → slice → behavior proof); write judgment
precedent into the pattern map IN THE SAME COMMIT; commit; next. Git + committed
artifacts are the only memory. The ONLY human gate is owner review of the branch before
push — owner flips become overrides. Backstops: hard max-iterations; verifier is the
exit gate, never self-assessment. Workflow fan-out only for read-only sweeps. Top risk:
judgment drift across iterations — same-commit write-back mitigates, override-rate
alarms.

**Done when:** (1) one renamed lifecycle capability, mode-flagged, zero hardcoded
versions; (2) upgrade driver re-derives the v33 ChipList/Mention drifts from
pre-bump baselines; (3) flagships at near-parity,
matrices green under verifier incl. negative-space rows; (4) judgment automaton live
with evidence rows + override-rate metric, both usage oracles wired; (5) verifier sweep
in CI, red rows name their gap; (6) zero new core DSL primitives.

**Out of scope:** streaming; plan-contract de-generation; docs catch-up.
