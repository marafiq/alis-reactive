# New-session START PROMPT — Alis.Reactive 1.0.0 green-field rewrite

> Paste this as the first message of the new session. It bootstraps full context with zero rework.

---

You are continuing the **Alis.Reactive green-field rewrite to 1.0.0** — a clean break that loses **zero features**. Branch `cleanbreakbutrc1`. Before doing anything, READ, in order:

1. `docs/design/redesign/REWRITE-SPEC.md` — the master spec (16 sections + Step-0 verified defects + gap map). **This is the plan.**
2. Memory (auto-loaded; re-read if needed): `project_onboarding_architecture`, `project_rewrite_design_insights_monoid_render`, `session_dsl_ratification_state`, `project_rewrite_freeze_sequence`, `feedback_dsl_naming_philosophy`.
3. Root `CLAUDE.md` + `.claude/rules/*.md` (operating standard, layered harness, agent dispatch).

## Mission
DSL → Rich **public** Plan Domain → **hand-authored `plan.ts`** (NO codegen) → **dumb** runtime executor. The C#→TS sync middle layer is REMOVED: no `PlanContractGenerator`, no `tools/PlanTypeGenerator`, no `generate:plan-types`; `plan.ts` is hand-authored under a strict linter and **drift-`--check`ed** (never regenerated). DONE = 1.0.0 released, all 14 decisions honored, the **1168 Playwright + 192 vitest** oracle green in a fresh clone. The guarantee of correctness is the GATES, not assertion.

**Cutover = single tree, clean slate** (owner-decided): `delete-all → commit → rebuild-all` on `cleanbreakbutrc1` — NO `Alis.Reactive.v1/`, NO swap, NO coexisting trees (a swap entangles old+new until impossible). Survivors: the oracle, these specs, `archive-history/`, the `.slnx`/CI plumbing. **Read `REWRITE-PLAN.md` FIRST** — it is the one-page commit of the plan + this cutover.

## DO STEP 0 FIRST (source-verified BLOCKING defects — fix before AST/determinism)
- **B1** `evaluate.ts` is 204 lines; fix the out-of-EOF citations in `04-matrix-http-arrays-values`/`05`/`06` (cited 287-300) to the real sites `188/199/203` (`08` already correct).
- **B2** `ContractDriftGate.Check()` calls `PlanContractGenerator.Render()` — redesign G3: reflected **public-surface manifest** diffed against committed `plan.ts` + non-vacuity negative control; never `Render()`, never plan.ts-vs-itself.
- **B3** `02-micro-modules` cites non-existent `ComponentObject.cs` — rewrite vs `BrowserObject*.cs`.
- **B4** `02-micro-modules` Kind-module thesis inverted — flip to hand-authored `plan.ts` + drift-`--check`.
- **B5** `11` #2 cite wrong — gather widening is `Include`-only (`Header :81`/`RouteParam :131` already `TypedSource`).

## The 14 binding decisions (see §1 of the spec for full text + carriers)
1 public domain, ZERO IVT (delete `Alis.Reactive.csproj:69-72`). 2 component/plugin = typed JS-object API (Property/Function/Command/Event + Shape; arrays first-class). 3 **vendor DROPPED** → root-accessor path + event-delivery-mode (`DomEvent`|`CallbackArgs`); name prefixes survive, runtime switch dies. 4 onboard ONCE via DI, plan is consumer, serialize only referenced. 5 ids framework-controlled (merge join key); string ids only for gather/DOM-target; `IdFor(expr)` read-only. 6 model-less "pure view" plan (no `TModel`, no `InputField`). 7 free-monoid carrier = IMMUTABLE cons list; keyed concerns stay maps. 8 byte-stable ⇒ cacheable; value-free plans; partials = fragment cache + monoid concat. 9 KILL codegen; hand-author `plan.ts`; drift-`--check`. 10 target BOTH `net48` + `net10.0`. 11 `FocusOut→Blur`, `SetText` KEEP, callbacks `(value,p)`; ratify/park `#34 FromDom`, `#36 AsPluginSource`. 12 fix the 6 grill defects. 13 NO BLOAT + Matt-Pocock TDD. 14 freeze sequence below.

## Freeze sequence (no production code before its module's green certificate)
Step 0 (above) → (1) grill→ratify the DSL with the 1/3/4/9 conflict-fixes landed → (2) re-cut + FREEZE the AST/UI (extraction harness re-derives all ~540 citations; fix 47→48; resolve Blur collision; state net48 globally) → (3) re-cut determinism + **machine-re-derive** the 375 census → (4) lock the 8-project layout → (5) author tests/skills → (6) implement module-by-module, **one closed matrix row per commit**, no production line until that module's green certificate. Spine **A→B→C→E→F** (A=names · B=module cut + seams + **interface-first build order** — the naive linear wave order is INVALID, there is a `Reaction↔Slot↔Plan` cycle, see SPEC §3/§4 — + project layout · C=determinism certificate · E=blind dogfood · F=code; **D**=cut HTML simulators). No C# unit-test project exists yet — create `Alis.Reactive.*.Tests` in step (5).

## Tooling (green-field root; `build:all` before sandbox)
- First run: `npm ci` → `npm run build:all` → `dotnet run --project Alis.Reactive.SandboxApp` (→ http://localhost:5220).
- Daily: `npm run watch:runtime` / `watch:design-system` / `dotnet watch`.
- Static+test: `npm run typecheck` (**CURRENTLY** `generate:plan-types && tsc --noEmit`, `Alis.Reactive.Assets/package.json:16` — it REGENERATES `plan.ts`, exactly what D9 forbids; the drift `--check` gate **G3 does not exist yet**, D9 builds it — do NOT run typecheck day-1 expecting the gate), `npm run lint`, `npm test` (= G4 vitest both workspaces), `dotnet build` (= G1, BOTH net48+net10), `dotnet test` (= G2).
- Playwright: after `build:all` + a FULL `dotnet build`, `dotnet build tests/Alis.Reactive.PlaywrightTests` then `dotnet test … --logger "console;verbosity=detailed"`; first-run `pwsh …/net10.0/playwright.ps1 install --with-deps chromium`. The fixture starts its own sandbox on a random port.
- Strict TS: `typescript-eslint strictTypeChecked + stylisticTypeChecked`, `any`=ERROR, `no-floating-promises`, `import/no-cycle`; `tsconfig strict + noUncheckedIndexedAccess + exactOptionalPropertyTypes + verbatimModuleSyntax + isolatedModules`.
- Sandbox hygiene: kill stale `lsof -ti:5220 | xargs kill -9` / `pkill -f Alis.Reactive.SandboxApp`; rebuild stale bundles `npm run build:all`.
- Worktree/branch: base feature worktrees on `origin/<branch>` (fresh fetch); verify base by commit-identity preflight, not file presence.

## Hard rules (CLAUDE.md — non-negotiable)
DSL source is the requirement (not docs/tests/memory). One closed matrix row per commit; no progress from uncommitted edits. Zero `InternalsVisibleTo`. Public domain, `internal` ctors behind public factories. Plan carries all behavior; runtime is a dumb executor with boundary-only errors (no preflight/rollback/fallback/registry/claims inside the generated graph). **Vendor is NOT a runtime concept** — root-accessor + delivery-mode are plan data. Tests assert ONLY user-visible behavior (Matt-Pocock); plan-shape assertions live in C# domain unit tests. **NO BLOAT** — reproducing any dissolved god-file (`ValueExpression` 590, `PipelineBuilder` 4-partials, `evaluate.ts` 204, `orchestrator.ts` 504, dual evaluators, two plugin builders) = the rewrite has failed. POCs go in TEMP, never the repo. Never propose `window`-globals or a runtime registry. Verify findings at source before acting (a subagent's lead is not evidence). Calibrate every "done/verified" to what was literally checked.

## Gates (each writes a SHA-bound transcript to disk; missing transcript = RED)
G1-BUILD(net48+net10), G2-CSHARP(+reverse-coverage), G3-DRIFT(manifest vs committed plan.ts + non-vacuity control; never Render()), G4-VITEST, G5-BYTE-STABILITY(re-serialize-twice + count guard + frozen kind-set + mutation control), G6-RENDER-PERF(BenchmarkDotNet budget), G-SURFACE(PublicAPI.Shipped.txt + NetArchTest), G-MATH-100(machine-derived denominator), G-FRESH-CLONE, behavior-oracle + G-ORACLE-COMPLETENESS(frozen 1168-id manifest → every id a non-skipped test). Hooks: all 12 currently `enabled:false`; KILL `commit-requires-relevant-tests`/`merge-requires-all-tests`; ship NEW tested SILENT-on-success hooks + `oracle-frozen-assertion-guard`.

## Oracle (zero-feature-loss fence, verified)
1168 Playwright `[Test]` (133 files) + 192 vitest `it/test`. FROZEN user-visible tier must survive the implementation swap unchanged; UPDATABLE plan-shape assertions migrate into C# domain unit tests.

## Working style
Use the Workflow tool for substantive multi-step work (decompose → adversarially verify → synthesize); verify every lead at source; report only committed, verified progress. Ask the owner well-articulated questions ONE at a time, problem stated plainly first. Do not reproduce bloat; do not weaken a frozen oracle assertion without an explicit `ORACLE-EDIT:` note.

**Begin by reading `REWRITE-SPEC.md`, then execute Step 0.**
