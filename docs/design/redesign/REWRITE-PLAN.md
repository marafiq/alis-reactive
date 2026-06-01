# REWRITE-PLAN — the one page

> Read THIS. The other 54 docs are depth-on-demand. If this page and a longer doc
> disagree on the **cutover model**, this page wins. Full authority on everything
> else: `REWRITE-SPEC.md`.

## Goal
Rewrite Alis.Reactive to **1.0.0**, clean break, **zero feature loss**.
**DONE = 1.0.0 released** — not "compiles," not "tests pass."

## Cutover model — DECIDED (owner, 2026-06-01): single tree, clean slate
**`delete-all → commit → rebuild-all`** on branch `cleanbreakbutrc1`. No swap. No
`Alis.Reactive.v1/`. No two trees coexisting — ever. *(A swap was rejected: it
entangles old and new until the cut becomes practically impossible. A clean break
makes the rebuild mechanical — IF the specs truly captured the facts. That "if" is
what Phases A–C below exist to guarantee.)*

| | |
|---|---|
| **DELETE** (implementation = disposable) | all P1–P8 framework C# source, the `runtime/` TS, `PlanTypeGenerator`/codegen, the old sandbox views. Commit the empty slate. |
| **KEEP** (the proof + the spec) | the **oracle** (1168 Playwright + 192 vitest), the 54 redesign docs, `archive-history/`, `Directory.Build.props` / `.slnx` / CI. |
| **REBUILD** | the framework, mechanically, module-by-module from the frozen specs, each module gated by its certificate + the oracle. |

**Honest caveat on "keep the tests":** the Playwright **assert** tier (user-visible
DOM/text/focus) survives **frozen** and is the zero-feature-loss proof. What gets
rebuilt *with* everything else is the sandbox **views** those tests drive
(re-authored against the new DSL) + the **UPDATABLE** plan-shape assertions, which
migrate into the new C# domain unit tests. So "keep the tests" ≠ literally
zero-touch — the behavior bar is frozen, the authoring surface under it is rebuilt.

## The spine — 5 phases, ordered, none skippable (Phase D = HTML simulators = CUT)
**A** names + defaults → **B** 12 modules + seams + 8-project layout →
**C** prove each module's determinism math to a **GREEN certificate** *(the unlock
token — no production line for a module before its cert is green)* →
**E** blind-rebuild each module from spec alone *(proves the proof forces the code)* →
**F** mechanical code in 7 dependency waves → **onboard-42** Syncfusion →
**fresh-clone** verify → **pack 1.0.0**.

## The 3 invariants that make it safe
1. **Oracle** — 1168 Playwright + 192 vitest. The frozen user-visible tier = zero
   feature loss. If it goes red, the **rewrite** is wrong, never the test.
2. **Gates** (each writes a SHA-bound transcript to disk; missing transcript = RED) —
   G1 build `net48`+`net10` · G2 C# + reverse-coverage · G3 drift (hand-authored
   `plan.ts`, `--check` only) · G4 vitest · G5 byte-stability · G6 perf · G-SURFACE ·
   G-MATH-100 · G-FRESH-CLONE · behavior-oracle + completeness.
3. **One closed matrix row per commit** — progress is reported only from committed,
   all-green work. No production code before its module's green certificate.

## Build order (7 waves)
Shape → Kind → Value → Condition+Request → Reaction+Trigger →
Component+Slot+Validation+Plugin → Plan.
*(There is a real `Reaction→Slot→Plan` cycle → build interface-first / impl-second;
see `REWRITE-SPEC.md` §3/§4.)*

## Where to go deeper
`REWRITE-START-PROMPT.md` (paste as message 1) → `REWRITE-SPEC.md` (the authority) →
`00-START-HERE.md` (the map) → the rest on demand. Archived/superseded material is
under `docs/archive-history/` and is off the critical path.
