# Onboard / Audit / Upgrade SF Components with 100% Behavior Coverage

One lifecycle capability converting Syncfusion EJ2 public API into typed Alis DSL with
**100% consequence-proof coverage of every accepted member**. Everything automated except
one quarantined, evidence-scored accept/exclude gate. Precision-first: under-accepting is
cheap (later acceptance is additive 1.x); polluting the C# API, shipping an unproven
member, or a false drift alarm is failure.

**Why:** devs expect the full public SF API; the safe rate of approach to parity IS the
adoption rate. Scale: Grid = 85 events / ~326 d.ts members vs 20 accepted typed events;
ChipList 8→4. Judgment volume, not mapping, is the cost. Flagships (Grid, ChipList,
InPlaceEditor, Schedule) target near-parity. Stakes proven 2026-06-10: v32→33 silently
broke ChipList `selectedChips` (values→indices) and Mention's popup event chain —
d.ts-compatible, typecheck-silent, caught only by behavior tests.

**Modes on one shared spine** (artifact tree, row gate chain, fail-closed matrix +
verifier, pattern-map loop, authoritative primitive matrix). Grammar misfit = stop
condition, never improvisation.
- *Onboard* (exists): one orchestrator per mode; versions derived from installed
  packages, never hardcoded; consolidate accreted matrix prose.
- *Audit* (exists): mode as a flag; verifier sweep wired into CI.
- *Upgrade* (new; traces are normalized to be diffable): regenerate discovery+traces into
  staging, diff committed baselines, classify (breaking/semantic/additive/cosmetic), emit
  a NAMED drift list; each row re-enters the gate chain. Scope O(accepted+new), never
  O(full surface). Validation: re-derive the v33 ChipList/Mention drifts from pre-bump
  baselines mechanically.

**The quarantined gate, computable — five oracles:** (1) real-product usage mining (the
Alis app is the 95%-use-case ground truth); (2) plugin usage = revealed demand; (3) Blazor
surface overlap = the vendor's own curation; (4) stability scores (presence across
versions, trace-variant determinism); (5) grammar fit. Auto-accept only on full agreement;
auto-exclude borderline with evidence recorded. Overrides feed the pattern map; thresholds
tighten until override rate is zero.

**Coverage that cannot lie:** consequence-proof per member kind, verifier-enforced. Reads
→ consumed by a realistic pipeline. Writes/void methods → visible state change. Returns →
consumed downstream. Writable payload props (`cancel` is canon) → NEGATIVE SPACE: gesture
done, member set via typed DSL, action visibly did NOT happen. Event variants proven per
trigger. Exclusions are fail-closed rows with proof. Law: every assertion must be
unsatisfiable by the defect it guards.

**Operating loop:** spine = the skill, ONE invocation per fresh session: run the status
reporter + verifier to mechanically NAME the next red row (sweep failure, backfill row,
judgment disagreement, or drift row); close exactly that row through the gate chain (trace
→ primitive map → slice → typed-DSL behavior proof); write any judgment precedent into the
pattern map IN THE SAME COMMIT; commit; end. Git + artifacts are the only memory; sessions
are disposable; the session boundary is the human checkpoint. /loop only sub-row (probe→
verify iteration), never row-to-row. Workflow fan-out only for read-only discovery sweeps
(no commits, no sandbox). /schedule only as nightly watchdog posting the red list — never
row work (unattended contradicts the judgment quarantine; cloud cold-start exceeds row
cost). Known top risk: judgment drift across sessions — precedent left in chat dies; the
same-commit write-back rule is the mitigation, override-rate the alarm. Backfill order:
flagships first; inventory names the rest. First classification is O(full surface), paid
once; steady state O(accepted+new).

**Done when:** (1) one renamed lifecycle capability, mode-flagged, zero hardcoded
versions; (2) upgrade driver re-derives the v33 drifts retroactively; (3) flagships at
near-parity, matrices green under verifier incl. negative-space rows; (4) judgment
automaton live with evidence rows + override-rate metric, both usage oracles wired;
(5) verifier sweep in CI, red rows name their gap; (6) zero new DSL primitives.

**Out of scope:** streaming; plan-contract de-generation; docs catch-up (post-release;
plugin docs must lead with the typed `Plugin` pattern).
