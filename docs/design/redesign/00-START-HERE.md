# 00 — START HERE (the cold-session entry point)

> ⚠️ **AUTHORITY — read this first.** After this map was written, the 2026-06-01 session produced the
> consolidated **`REWRITE-SPEC.md`** (the authoritative plan — a Step-0 of source-verified blocking
> defects + an App-A correction map) and **`REWRITE-START-PROMPT.md`** (the actual message-1 for the new
> session). **Where this file and `REWRITE-SPEC.md` differ, the SPEC wins.** Read
> `REWRITE-START-PROMPT.md` → `REWRITE-SPEC.md` FIRST; use this file for the KILL / SURVIVE / build-wave
> detail. Two known supersessions are flagged inline below: the **Slot↔Plan cycle** (§3) and the
> **375 census** (§1/§5).

> You are a **fresh session with zero prior context.** This is the orientation map.
> It is the map: which docs to read in what order, the execution sequence and its gates,
> the module build order, what we KILL, and what we KEEP. Read this once top to bottom,
> then read the docs in the READING ORDER below. After that you can execute the whole
> rewrite to **1.0.0 released** with **zero questions** to the owner. The owner is out of
> the loop; the gates are the approval.

**The one-sentence charter.** Rewrite Alis.Reactive **green-field, from scratch, in a NEW
folder**, ship **1.0.0**, **lose zero features**, kill all the nonsense, keep only what is
worth keeping. This is a **clean rewrite, not a refactor** — do not edit the old tree to
evolve it; build the new one and prove it against the old as a differential oracle.

**The governing philosophy (memorize before reading anything else).** DESIGN-AND-PROVE-FIRST,
CODE-LAST. Production code is the **last** phase and is **mechanical**. If code is hard to
write, that is a **defect report against the proof**, not a coding problem — kick it back to
the math (Phase C), harden the spec, resume. Reliability comes from **engineered gates, not
promises.** Calibrate every "done/verified/green" to literally-observed evidence; separate in
writing (a) what you verified, (b) what you assumed, (c) what is unchecked.

---

## 1. READING ORDER — read these, in this order, for this reason

Read top to bottom. Each line says the doc, then **why it is at this position**. Do not skip;
do not reorder. (Counts and code anchors inside these docs were verified against source at
authoring time — re-verify any count against real source before you act on it.)

| # | Doc | Why you read it here |
|---|-----|----------------------|
| 1 | **`REWRITE-GOAL-PROMPT.md`** | **The charter.** The complete self-contained mission: the 5-phase spine (A→B→C→E→F), the 17 directives, the behavior oracle, the autonomous gates, the §E project layout, the commands. Everything else is detail under this. Read it whole, first. |
| 2 | **`09-dsl-naming-sheet.md`** | **The names are DECIDED.** The single authoritative naming sheet — one-concept-one-name across C#/JSON/TS/tests/docs, every name passing the cold one-breath read. This is the Phase A output. You do not re-coin names; you obey this sheet. (It absorbed the blind-naming-test fixes: `WholePayload→WholeResponseBody`, `ReactionLane→ReactionTiming`, `Static→Literal`, `Finally→OnSettled`, `EventButton→DispatchButton`, `AsSource→AsArraySource`, `PluginOperation→PluginMember`, and the `Shape` gloss tightened.) |
| 3 | **`08-determinism-formalization.md`** | **The proof tool.** The formalization that the executable property/equivalence harnesses run against real linked source. This — NOT any browser drawing — is "the simulation that proves." Phase C's green per-module certificate is built on this. |
| 4 | **`governance-gaps.md`** | **The gates.** The 17 traced failure modes (FM1–FM17), the ceremony that was cut, and the hardened lean governance (4 personas, the machine-checked gates G1–G6 + G-SURFACE/G-MATH-100/G-FRESH-CLONE/behavior-oracle/G-ORACLE-COMPLETENESS, the transcript fence). Every gate maps to a real FM; anything mapping to none was cut. This is how the owner-out-of-loop approval works. |
| 5 | **`02-micro-modules.md`** | **The 12-module cut.** The single-responsibility module list, the acyclic dependency graph, the god-file inventory each module dissolves, and the build-order waves. The structural spine of Phase B. *(Note: this doc predates the §2 naming fixes and still shows the old `WholePayload`/`responseBody` terms in its Replaces column — `09-dsl-naming-sheet.md` is the names authority; read 02 for the module cut, 09 for the names.)* |
| 6 | **`REWRITE-GOAL-PROMPT.md` §E** | **The project layout — re-read this section.** The ~8-project green-field solution (P1–P8), the per-concept internal namespaces, the strict NO-`InternalsVisibleTo` encapsulation, the stable-vs-reshapable namespace map, and the per-module `CLAUDE.md` rule. You re-read §E specifically because it is the structural blueprint Phase B finalizes. |

**Supporting docs (read on demand, not up front):**
`03-naming.md` (the naming test + 7 principles + banned-word guard — the *method* behind sheet 09);
`00-design.md` / `01-connectivity-graph.md` (deeper design + connectivity for Phase B);
`05/06/07-determinism-*.md` (the per-overload census — **machine-re-derived per `REWRITE-SPEC.md` §13, not the settled "375/375" tally it slipped to before** — plus the independent re-census and the Shape certificate; the foundation Phase C extends);
`scaffold/_playbook.md` + `scaffold/_fixtures.md` + `scaffold/<Module>.md` (per-module compile-ready skeletons + named fixtures for Phase F);
`dogfood/` (the blind-builder protocol + the Shape 54/54 dogfood pattern to replicate per module);
`extensibility-stress-test.md` (the open vendor-contract pressure test for Directive 7).

**Retired — do NOT treat as a gate (read only as historical context):**
`governance-simulation.html` and `playground/` — Phase D (HTML simulators) is **CUT**. A browser
drawing is a non-falsifiable second implementation in English; the dashboard once showed 0 gaps
against 15 real escapes. The PROOF is the math (doc #3) executed by harnesses; the TEST is
Matt-Pocock-style pure-behavior TDD. Any diagram is at most an OPTIONAL artifact auto-derived
*from* the Phase C certificate + the Phase F green slice — never a gate, never a STOP authority,
never on the unlock path.

---

## 2. EXECUTION SEQUENCE — A → B → C → E → F → onboard-42 → fresh-clone → pack-1.0.0

The spine is **five phases, ordered, none skippable, none reorderable** (Phase D is CUT). After
F come the three release stages. Each step below maps to **its gate** — the gate is the only way
the step is "done." All commands run from the **green-field root**.

| Step | What | Its gate (the unlock / the proof) |
|------|------|-----------------------------------|
| **A — Nail the language + defaults** | Apply `03-naming.md`; the final names live in `09-dsl-naming-sheet.md`; decide the good DEFAULTS (Directive 3: gather param-name, JSON-body default, the NEW 400-ProblemDetails auto-surface, the discovered HTTP quirks) and the grammar improvements (Directive 4) deliberately, up front. Deepen the root `CLAUDE.md` + stub the 12 per-module `CLAUDE.md`. | **Phase A gate:** every load-bearing name passes THE NAMING TEST (cold .NET-dev, one-breath-correct); banned-generic-word guard clean; one-concept-one-name holds across C#/JSON/TS/tests/docs; defaults + grammar decided as Phase A artifacts. **Artifact-checkpoint barrier:** write + git-commit the sheet before handoff (FM12). |
| **B — Deep modules + seams** | Finalize the 12-module cut, the acyclic graph, the §E project layout, each module's §File-Layout; record each public seam signature for G-SURFACE. **No code.** | **Phase B gate:** every module has one author seam + one node family + one runtime reader; the dependency graph is acyclic + layered (verified vs `02-micro-modules.md`); no shallow module survives. B→C handoff records each public seam for G-SURFACE. |
| **C — Prove the math (the unlock token)** | Extend Shape's 5-artifact pattern (per-module law set; harness vs real linked source + an independent 2nd harness + generator-coverage tally; adversary pass; dogfood-from-spec; the certificate) to **every** module + **every** seam. Census is **per-overload**, never band rollups. | **Phase C gate = the unlock token:** a GREEN per-module certificate, proved BY executable property/equivalence harnesses against real linked source. **G-MATH-100:** the census denominator is MACHINE-DERIVED from the real assembly and the cert's X/X must EQUAL it; one composite-variant row per cross-module edge; any `RESOLVED-BY-REDESIGN` row stays RED until the fix ships in built source. **No production line is written for a module until its certificate is green.** |
| **E — Blind developers** | Run the dogfood blind-builder per module (input = `{that module's spec, its named fixtures}` ONLY) + the blind BDD reviewer; harden the spec on every divergence. This is the **re-proof leg of C**: devs who never see the implementation rebuild from math+spec alone. | **Phase E gate:** blind builder hits green from spec+fixtures alone, zero inventions / zero divergences (an independent judge diffs vs real source line-by-line to confirm behavior is *forced*); a blind BDD reviewer confirms each test is behavior-first, traces to a criterion, fails-when-broken, real-interactions-only, refactor-survivable. **Blind-context provenance manifest** (FM16) proving zero forbidden reads, re-checked by Crossjudge. |
| **F — Production code** | Per module, in dependency order (the 7 waves §3), the mechanical loop: pass-protocol row → paste named fixtures FIRST (red = the spec) → create files at exact §File-Layout paths → paste compile-ready skeleton → fill each TODO with the obvious body the §Input→Output rule dictates. **A hard fill is a DEFECT REPORT against the proof — kick back to Phase C, do not push through.** | **Per-module code gates, all green per commit, one closed matrix row per commit:** **G1-BUILD** (`dotnet build` BOTH `net48;net10.0`); **G2-CSHARP-TESTS** (`dotnet test` + the reverse-coverage census assertion — implemented surface count == matrix count, FM4); **G3-DRIFT** (`npm run typecheck` = `ContractDriftGate.Check` `--check` diff over the COMMITTED `plan.ts` + `tsc --noEmit` strict + `git diff --exit-code -- plan.ts`; NO generate step, FM14); **G4-VITEST** (`npm test` both workspaces); **G5-BYTE-STABILITY** (re-serialize twice → identical bytes, as `[Test]` with a count guard + negative control, FM10); **G6-RENDER-PERF** (BenchmarkDotNet within the recorded allocation budget); **G-SURFACE** (PublicAPI parity vs RC1 + NetArchTest: no infra type in a public DSL signature, FM3); **G-FRESH-CLONE** (delete `dist/`, `npm ci && build:all && dotnet build`, commit-identity preflight, moved INTO the per-commit fence, FM1+FM7); **behavior-oracle gate** (Playwright vs freshly built assets, served-bundle-hash check, orphan sweep by name + filtered-TRX retry, two assertion tiers, FM9+FM6+FM11); **G-ORACLE-COMPLETENESS** (the frozen 1168-id RC1 manifest — every id maps to a non-skipped executing test, FM9); **the SHA-bound transcript fence** (FM5) + the `oracle-frozen-assertion-guard` PreCommit hook (FM8). |
| **onboard-42-Syncfusion** | Onboard the **42 remaining** top-level SF EJ2 components (exclude the 53 already onboarded) at full public API, via the mechanical 7-file vertical slice + the scaffolding generator. **Zero TS runtime / schema / core descriptor changes** — if you reach for one, the slice is missing information. | The **7 Automation Gates + raw-HTML probe** per component (the onboarding skill's correctness core), plus the same per-module code gates above. Backlog recomputed deterministically from `Syncfusion.EJ2.xml` (the NuGet artifact) minus nested settings builders minus the onboarded set. |
| **fresh-clone verification** | In a truly fresh clone/worktree (no `node_modules`, no `dist/`): `npm ci → npm run build:all → dotnet build → the full behavior oracle green`. | **G-FRESH-CLONE at the cut** + the full 1168 Playwright + 192 vitest oracle green with zero console errors (the boot contract). `green_field_count == 1168` (G-ORACLE-COMPLETENESS), not "tests physically ported." |
| **pack-1.0.0** | `npm run build:all → dotnet build -c Release → dotnet pack` each packable project at `-p:Version=1.0.0`. NuGet packed + verified, asset delivery (dist → NuGet `build\`+`buildTransitive\` → consumer `wwwroot`) proven. | **DONE = 1.0.0 RELEASED** — all 12 modules from green certificates, full RC1 oracle green in a fresh clone, all 42 SF onboarded at full public API, NuGet packed + verified, the autonomous gate system demonstrably green end-to-end. Not "compiles," not "tests pass." |

---

## 3. THE 7-WAVE MODULE BUILD ORDER (dependency-ordered)

A module is coded **only after every module it depends on is green** (its Phase C certificate
green AND its dependencies green). Single-thread order:
`1 Shape · 2 Kind · 3 Value · 4 Condition · 5 Request · 6 Component (pulled forward — stub the
`BrowserObjectId`/`BrowserObjectContract` TYPES first) · 7 Reaction · 8 Trigger · 9 Slot ·
10 Validation · 11 Plugin · 12 Plan`. As waves:

| Wave | Modules | Note |
|------|---------|------|
| **W1** | **Shape** *(kernel)* | The structural type tag + the one `applyShape` convert engine + the shape-once invariant. The only module with the full universal-law treatment today — extend its 5-artifact pattern to all others. |
| **W2** | **Kind** *(kernel)* | The one C#→TS discriminator + the hand-authored `plan.ts` + the re-pointed drift DETECTOR + `assertNever`. |
| **W3** | **Value** | The value spine: one `TypedSource<T>` → one `ValueExpression` → one `evaluateValue`; `WholeResponseBody`/`WholeElement` as distinct member-less kinds; per-op ArrayOp variants. |
| **W4** | **Condition + Request** | Condition: `When/Then/ElseIf/Else` + `ConditionGraph` + ONE compare engine. Request: the only async network lane — `Get/Post/Put/Delete` + Gather + Response + Chained + Parallel + `WhileLoading`/`OnSettled`. |
| **W5** | **Reaction + Trigger** | Reaction: the command surface + the executable action graph with `ReactionTiming` (`Sync`/`Async`) STAMPED in the plan and routed in the runtime. Trigger: `Html.On` + `StartsWhen` + `Behavior`/`BehaviorGraph`. |
| **W6** | **Component + Slot + Validation + Plugin** | Component: the `IdGenerator` id regime + the vendor-neutral browser-object contract (the sole vendor seam). Slot: SSR join by `PlanId` + browser injection by `SlotId`. Validation: `ReactiveValidator<T>`/`ClientRule`/`WhenField` reusing Condition's compare engine. Plugin: the one declaration API + one args-builder read/call surface. |
| **W7** | **Plan** *(spine)* | `PlanBuildContext` → immutable `PlanDocument` (version 3) → serialize → `root.ts` discovery → `boot.ts`, with the active plan passed **explicitly** into `executeReaction`. |

> **SUPERSEDED by `REWRITE-SPEC.md` §3/§4.** Earlier drafts called the Slot→Plan / Reaction→Slot edges
> "not a cycle." The SPEC corrects this: the source maps (`10:55-64`, `02:65-79`) carry a **real
> `Reaction→Slot→Plan` cycle** plus a literal **Slot↔Plan 2-cycle** (`10:61` Slot→Plan vs `10:64`
> Plan→Slot), so the 7-wave order above is **NOT a valid topological sort** — it is an *indicative* impl
> order once split. Acyclicity holds only at the **seam/interface** level: Phase B derives the real
> build **interface-first / impl-second** (back-edges bind to interfaces; the `Slot↔Plan` cycle breaks
> at the downward-only `Slot→Plan` edge), and **resolving this cycle is a Phase-B gate.** `import/no-cycle`
> enforces the TS seam, not the impl graph.

---

## 4. KILL LIST — the nonsense that does NOT survive (each grounded, with why)

A clean rewrite kills these on sight. None is "refactored" — each is deleted because it is the
exact debt the rewrite exists to remove.

| KILL | Where it lives today | Why it dies |
|------|----------------------|-------------|
| **PlanContractGenerator / all TS codegen** | `PlanContractGenerator.cs` (~1170 lines, emits `plan.ts`); `tools/PlanTypeGenerator` console tool; the `generate:plan-types` npm step (runs before `build:runtime` AND `typecheck`) | Codegen is killed (Directive 1). `plan.ts` becomes the **hand-authored**, self-documenting source of truth under a strict TS linter. The re-pointed drift **DETECTOR** (`ContractDriftGate.Check`, `--check` over the COMMITTED `plan.ts`) flags C#↔TS mismatch and fails the build, but **never regenerates** — the regenerate-then-typecheck tautology auto-absorbed drift (FM14). |
| **The HTML simulators (Phase D)** | `playground/{index.html,design-graph.js}`, `governance-simulation.html` | A browser drawing is a non-falsifiable **second implementation in English** — the exact FM17 risk. The simulation dashboard MISLED, showing 0 gaps against 15 real escapes. The proof is the MATH (`08`) run by harnesses; its only real STOP ("undrawable flow = unwritten matrix row") is already owned by G-MATH-100. CUT entirely from the critical path; any diagram is an optional auto-derived artifact, never a gate. |
| **JSON-schema-as-contract** | `AssertSchemaValid`, schema drift gates, any schema-first process | Retired. The live contract is the C# plan domain + the hand-authored `plan.ts` guarded by the drift detector. Do NOT revive `reactive-plan.schema.json` as the contract, do NOT add `AssertSchemaValid`. |
| **Dual condition evaluators** | `conditions.ts` (21 ops) vs `sync-condition.ts` (4 ops) divergence; `ValueEvaluator` DI threaded through 8 fns | Two engines for one concept can diverge. Collapse to ONE compare engine; `conditions.ts` becomes a thin confirm/async wrapper delegating to the same sync core. |
| **The responseBody / elementValue sentinel collision** | `ValueExpression.cs:379-403`, `plan.ts`, the read path in `evaluate.ts` (`readsWholePayload`/`readsWholeElement` / `readFromPayload`) | The magic sentinels `responseBody`/`elementValue` collided with the real DSL properties `ResponseBody`/`ElementValue` — the one live many-to-one D1 determinism violation. Replace with two **distinct, member-less node KINDS**: `WholeResponseBody` (`kind:"whole-response-body"`) and `WholeElement` (`kind:"whole-element"`). **Land this early — it is what makes the spec's "100%" true-in-source.** |
| **The activeRuntimePlan singleton + instanceof-Promise probes** | hidden mutable `activeRuntimePlan` singleton + the 4 `reset*ForTests` functions shipped in production; `result instanceof Promise` probes at `execute.ts:287,314`, `conditions.ts:70,89,106`, and in `native-action-link.ts` `handleClick`; `crossedAsyncBoundary` re-detection | Hidden global state + re-detecting sync/async at runtime is fragile and untestable. Thread `ActivePlan` **explicitly** into `executeReaction`; stamp `ReactionTiming` (`Sync`/`Async`) onto each C# reaction node and route the runtime on the carried tag — then DELETE every `instanceof Promise` probe (restores D3). Remove all `reset*ForTests` from production. |
| **The 4 InternalsVisibleTo leaks** | `Alis.Reactive.csproj` lines 69-72 (Fusion, Native, FluentValidator, PlanTypeGenerator) | The FRAME BANS `InternalsVisibleTo`. Each project's contract must be provable from its **public surface alone**. P1 exposes a deliberate vendor-neutral PUBLIC domain contract; Fusion/Native/FluentValidator consume it via `ProjectReference` only. (The PlanTypeGenerator IVT dies with codegen.) |
| **The GOD-files** | `ValueExpression.cs` (590) + the 4-type `ValueRead→ValueReadTarget→ValueReadPath→PayloadReadPath` indirection; `ComponentObject.cs` (677); `PipelineBuilder` (4 partials); `evaluate.ts` (204); `orchestrator.ts` (504); `RuntimePlan` (4 classes in one + per-read rebuild) | Each is a facade hiding branches behind one name. Dissolve into the per-concept module namespaces (§E): the 4-type read indirection flattens into the `Read` node; `PipelineBuilder` splits into focused sinks; `evaluate.ts` slims to a dispatcher + a separate array-op engine; the validation tower gets its own home out of `ComponentObject.cs`. |
| **The two duplicate plugin builders** | two parallel declaration APIs (`PluginTypeBuilder` vs `ReactivePlugin`); ~95%-identical `PluginReadBuilder`/`PluginCallBuilder`; the ~30-method arity-0..3 × member/root × function/command overload explosion | One concept, two+ implementations. Collapse to ONE plugin-declaration API + ONE args-builder-first read/call surface; collapse the synonym verbs (`Method→Function`, `Void→Command`) and rename the abstract supertype `PluginOperation→PluginMember`. |
| **The OLD broken hooks** | `commit-requires-relevant-tests` / `merge-requires-all-tests` (currently disabled) | They **fired unreliably, at the WRONG lifecycle time, and CORRUPTED THE CONTEXT** by dumping hook output into the conversation. Re-enabling them re-introduces that corruption — so they are KILLED, not re-enabled. They are **replaced by NEW, TESTED hooks** (Directive 16, see SURVIVE list): precise matcher + correct lifecycle event; SILENT on success, one bounded line on failure; ALL real output to a SHA-bound transcript on disk, never the context; each ships a firing TEST (fires-when-it-should, silent-when-not, output-bounded) and is untrusted until that test is green; a hook that did not fire is itself a RED gate (a meta-check asserts the SHA-bound transcript exists for the commit SHA). |

Also dead (zero references) and deleted in the naming sheet: the `DrawerPosition`, `ToastType`,
`ToastPosition` enums.

---

## 5. SURVIVE LIST — only what is worth keeping (do NOT regress these)

These are the sound foundations. Keep them; do not churn their names; do not reinvent them.

| SURVIVE | What it is | Why it stays |
|---------|-----------|--------------|
| **IdGenerator** | Deterministic collision-free element ids from model type + member expression (`{Namespace_TypeName}__{MemberPath}`), no DOM scanning, all vendors → the same id | The id regime that makes plan-driven (not DOM-scanned) resolution possible and guarantees same-id ⟹ same-shape. Sacred. |
| **Shape-once** | The single `applyShape`/`convertByShape` engine (`runtime/core/shape-convert.ts`) + the shape-once invariant on the gather egress path | One conversion engine, applied exactly once on egress — kills the 3 redundant re-shapings. The only module with the full universal-law proof today; the pattern every other module's proof extends. |
| **The vendor-neutral object model** | "Everything is a JS object": the `BrowserObject`/`BrowserObjects`/`BrowserObjectId`/`BrowserObjectContract` family (properties/methods/events), vendor knowledge isolated to `domain/component-driver.ts` delegating to `resolution/event-fusion.ts` / `event-native.ts` | The sole vendor seam. A third vendor touches only the driver + one `event-{vendor}.ts` leaf. Must NOT recollapse into one god-file. Becomes an OPEN, DOCUMENTED, PUBLIC contract (Directive 7) any external consumer onboards against. |
| **The determinism kernel** | The per-overload census (the "375/375" tally is **machine-re-derived per `REWRITE-SPEC.md` §13**, not settled) + the 4 DET pipeline laws + the 4 clean-cut seam verdicts + the Shape domain algebra (E/M/A/C/P/S/F law families), with two independent harnesses + adversary passes (`05/06/07/08-*.md`, `dogfood/`) | The foundation Phase C extends to every module. The proof tool. Never replaced by a browser drawing. Note: the **merge-union invariant** `merge(object{a},object{b})=object{a,b}` is CORRECT (a closed union, not a lattice join) — document it, do NOT "fix" merge into associativity/lattice semantics. |
| **The phantom-builder compile-time DSL** | `TypedSource<TProp>` → `ConditionSourceBuilder<TModel,TProp>` typed operators, expression-only `InputField`, the Element builder; invalid authoring is a **compile error**, never a runtime throw | SACRED (Directive 2). The compile-time correctness is the whole point. Convert the residual runtime throws (`Standalone.Then`, ElseIf/Else ordering, missing HTTP verb) into compile errors — tighten, never weaken. |
| **#if NET48 multi-target** | The sound `net48;net10.0` same-DSL multi-target (21 `#if NET48` files: Alis.Reactive 5 + Native 16) + `Directory.Build.props` centralization (`VersionPrefix`, `AlisAssetsDist` single npm→pack handoff) | Do NOT abandon net48. A net10-only plan-domain API without an `#if NET48` shim fails the net48 leg of G1. Only the Fusion project is net10-only. |
| **The drift detector — AS A CHECK** | `ContractDriftGate` / `ContractDriftResult` line-diff detection | KEEP and **re-point** it: it runs `--check` over the COMMITTED `plan.ts` and fails the build on Kind/op-token mismatch — but NEVER regenerates (the whole point of killing codegen while keeping the fence). This is the survivor that makes hand-authored `plan.ts` safe. |
| **The NEW tested hooks** (replacing the killed ones) | A commit gate (tests ran for touched code), a merge gate (full suite green), the `oracle-frozen-assertion-guard` PreCommit hook (FM8) | The KILL list removes the OLD broken hooks; these NEW ones are the replacement — engineered against the three exact failure modes (wrong trigger, context corruption, never-fired), each shipping a green firing-test before it is trusted, all output to SHA-bound transcripts. Also keep the other sound foundations: the dumb two-lane runtime executor (`execute.ts` switch + `assertNever`), static-cached `JsonSerializerOptions`, the `kind`-discriminator polymorphism (one generic `PlanNodeDiscriminator<T>`), and the named must-have features (Element builder, the Syncfusion typed Template, more DOM events). |

---

## 6. THE ORACLE (your fence at every phase) — the GOLDEN RULE

A working, 100%-stable product (RC1) already exists in this repo. It is your **differential
reference oracle** — you are never flying blind. Three layers, all green before every push:

| Layer | Count (re-verify against source) | Asserts |
|-------|----------------------------------|---------|
| Playwright (`tests/Alis.Reactive.PlaywrightTests`) | **1168** / 133 files | What the **user sees** — DOM, text, focus, disabled, gather body |
| vitest (`Alis.Reactive.Assets/runtime/__tests__`) | **192** / 28 files | Runtime read-path behavior in jsdom |
| typecheck / drift detector | detector-based | C# plan domain ↔ hand-authored `plan.ts` agreement |

**Two populations, two policies (conflating them is the cardinal trap):**
**(a) USER-VISIBLE behavior assertions are FROZEN** — visible DOM/text/focus/disabled + the one
justified non-visible exception (`request.PostData`). If a behavior assertion fails, **the
rewrite is wrong — never the test.** Never silently weaken one.
**(b) Plan-shape / oracle-internal assertions are DELIBERATELY UPDATABLE** — but only as a
DOCUMENTED edit tied to a concrete win (the new `timing` field, the new `whole-response-body`
kind WILL change these), never a quiet edit to dodge red. Crossjudge re-derives the
frozen-vs-updatable classification from the assertion target — the editor never self-labels.

**Boot contract:** the runtime emits the boot marker + `[alis:boot] booted` with **zero console
errors** (`AssertNoConsoleErrors()`). Playwright self-hosts on a random free port — do NOT
pre-start a sandbox on 5220. A `TimeoutException` that passes on isolated re-run is a
machine-load flake, not a product bug. Run a FULL `dotnet build` before the Playwright suite (a
Core-only build = 852/852 fail in ~30s = a stale-build artifact, not a regression).

---

## 7. WORKTREE & BRANCH DISCIPLINE (autonomy-critical)

Build on an **explicit feature-branch worktree based on `origin/<branch>` (fresh fetch)**.
**Never** use the local ref; **BAN** the main-based agent-isolation worktree (it silently bases
on main HEAD for any branch ahead of main — FM7, recurred twice).

```bash
git fetch origin
git worktree add --detach .worktrees/green-field-rewrite <feature-branch>
```

Verify the base by the **G-FRESH-CLONE commit-identity preflight, NOT a file-presence proxy**:
assert `HEAD == feature-branch tip` (fetched fresh), `rev-list --count feature..HEAD == 0`, and
`git cat-file -e HEAD:<feature-only-path>` for a manifest of paths absent on main; require
`node_modules`/`dist` ABSENT (truly fresh). Delete old code only after the new module passes all
gates; never strand the oracle on a half-migrated module. **Progress is reported only from
committed, all-green work.**

---

## 8. THE AUTONOMOUS LOOP (no owner in the loop)

**Buildhand builds one slice → Crossjudge verifies (independent, reproduce-before-accept) →
Verify-and-land (integrate + commit as ONE atomic stage).** A finding is a **reproducible
witness, not an opinion** — rejected if it cannot be reproduced. The slice lands only on green.
Commit **one closed matrix row per slice** carrying the SHA-bound evidence bundle (transcripts,
byte-compare, perf, the matrix-row id). Never start slice N+1 before slice N has a focused proof
AND a commit. No lone-wolf rewrites. Refuse to rubber-stamp; hold the work accountable with
**zero false positives.**

The 4 personas: **Gatekeeper** (the unbypassable spine order + per-commit fence + net48 leg + the
YIELD LEDGER, FM15), **Crossjudge** (reproduce-before-accept + spot-inject a known mismatch to
prove a gate CAN go red + the frozen-vs-updatable re-derivation), **Buildhand** (the Phase F
implementer, weakest authority, writes only against a green certificate, cannot report from
uncommitted edits), **Coldhand** (the blind builder — learnability + impl-coupled-test review,
with the Crossjudge-inspected context-provenance manifest).

---

**Begin with the SPINE, in order: A → B → C → E → F → onboard-42 → fresh-clone → pack-1.0.0.**
Phase A: the names are decided (doc 09) — obey them, decide the defaults. Phase B: cut the deep
modules (doc 02) + the §E layout. Phase C: prove the math (doc 08) to a green certificate per
module — the unlock token. Phase E: blind developers rebuild from spec alone. Then Phase F: code,
module-by-module in the 7 waves, every gate green per commit, against the RC1 oracle as your
fence. Onboard all 42 Syncfusion components. Verify in a fresh clone. Pack and release **1.0.0**.
You are never flying blind — RC1 is the truth, the gates are the proof, the spec is the spec.
Build it.
