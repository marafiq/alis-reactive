# Alis.Reactive — Green-Field Rewrite Goal Prompt (1.0.0)

> You are starting a **fresh session with no prior context**. This document is your
> complete, self-contained charter. Read it once top-to-bottom, then execute it to
> **1.0.0 released — without owner intervention**. Everything you need — the spine,
> the spec, the oracle, the gates, the commands, the folder layout — is named here
> with exact paths. You build the system that *prevents* mistakes; you do not
> *promise* their absence. A working, 100%-stable product (RC1) already exists in
> this repo as your differential reference oracle, so you are **never flying blind**.

---

## 0. THE SPINE — DESIGN-AND-PROVE-FIRST, CODE-LAST (the master sequence)

This is the governing philosophy of the entire effort. It overrides convenience,
overrides the old in-place blueprint where they conflict, and is **a mandatory
ordered sequence — no phase is skippable, no phase reorderable.** Production code is
**LAST**. Go *harder* than any prior pass: design and prove until the design is
inevitable, then the code is mechanical.

```
PHASE A  NAIL THE LANGUAGE      grammar · vocabulary · names + DEFAULTS → PL-expert standard, decided UP FRONT
   ↓     (gate: THE NAMING TEST — a cold .NET dev reads the name and is right in one breath)
PHASE B  DEEP MODULES + SEAMS   narrow interface, deep implementation (Ousterhout) — NO CODE
   ↓     (gate: one author seam + one node family + one runtime reader per concept, acyclic graph)
PHASE C  PROVE THE MATH         every variant to 100%, per-module + every seam — the DETERMINISM GATE
   ↓     (gate: a GREEN per-module certificate, proved BY EXECUTABLE property/equivalence harnesses run against real source, is the UNLOCK token for that module's code)
PHASE E  BLIND DEVELOPERS       use the DSL COLD — blind builder + blind BDD reviewer
   ↓     (gate: zero inventions / zero divergences; a TODO needing a design decision = spec incomplete)
PHASE F  PRODUCTION CODE        module-by-module, dependency-ordered, gate-green-per-commit
```

The spine is **five phases — A → B → C → E → F.** (Phase D, the HTML simulators, is
**CUT.** A browser drawing proves nothing — it is a non-falsifiable second
implementation in English, and the prior `governance-simulation.html` dashboard actively
*misled* by showing 0 gaps against 15 real escapes. The PROOF tool is **MATH** — the
formalization at `docs/design/redesign/08-determinism-formalization.md` plus the
executable property/equivalence harnesses run against real linked source: **that is the
"simulation that proves."** The TEST tool is **Matt-Pocock-style pure-behavior TDD**
(Directive 17). Any human-legible diagram is an **optional artifact auto-derived from the
Phase C certificate + the Phase F green slice** — never a gate, never a STOP authority,
never on the unlock path.)

**The three theses that make this work — internalize them before anything else.**

1. **DESIGN-AND-PROVE-FIRST, CODE-LAST.** Before a single line of production code:
   (A) nail the language — grammar, vocabulary, names, AND good DEFAULTS, decided up
   front as deliberate Phase A artifacts; (B) cut deep modules and the right seams;
   (C) prove the math to 100% — the proof is the formalization
   (`08-determinism-formalization.md`) executed by property/equivalence harnesses run
   against real source, NOT a browser visualization; (E) have blind developers use the
   DSL cold to prove learnability, clarity, and correctness. **Only when all four hold
   does Phase F begin.** If a name fails the cold read-test, if a matrix row cannot be
   written from source, or if a blind builder must make a design decision to fill a
   TODO — **STOP, read more DSL source, harden the spec, and only
   then proceed.** Jumping to code is the one failure mode that voids the whole
   effort. **Code is the mechanical part ONLY.** If an implementer finds a module HARD
   to write, that is not a coding problem — it is a **DEFECT REPORT against the proof**:
   kick it back to design (Phase C), harden the spec until the body is obvious, and only
   then resume. **Hard code = unfinished proof.** Difficulty in Phase F is always a
   signal that an earlier phase did not finish, never a signal to push through.

2. **ZERO MISTAKES COME FROM GUARDRAILS, NOT PROMISES.** Reliability is *engineered*,
   never asserted. You build the system that prevents mistakes: unbypassable gates
   (§G), the RC1 behavior oracle as a differential fence (§F-Oracle), drift detection,
   fresh-clone verification, blind review, the math-as-a-gate-before-code, positive
   quality that leads to ownership, and deliberate planning (one closed matrix row per
   commit). **Calibrate every claim to literally-observed evidence.** Never report
   "verified / done / works / tested / green" beyond what was actually checked.
   Separate, in writing, (a) what you verified, (b) what you assumed, (c) what remains
   unchecked. Overclaiming a green gate is worse than admitting a gap, because the
   owner is not in the loop to catch it.

3. **RC1 IS THE DIFFERENTIAL REFERENCE ORACLE.** A working, 100%-stable product
   already exists in this repo. Every feature, DSL surface, and behavior of the
   green-field rewrite is **differentially provable** against it. The behavior oracle
   below — **1168 Playwright + 192 vitest** (counts re-verified in this repo: 1168
   `[Test]` across 133 files, 192 `it/test` across 28 files) — plus the RC1 product
   itself is the absolute zero-feature-loss proof. Parity with RC1 is a continuous
   guardrail at *every* phase, not a final checkpoint.

The existing artifacts you BUILD ON (do not recreate; extend):

- Language: `docs/design/redesign/03-naming.md` (the naming test + 7 ordered principles + the banned-generic-word guard).
- Deep modules + seams: `docs/design/redesign/02-micro-modules.md` (the 12-module cut, the acyclic graph, the god-file inventory), `docs/design/redesign/00-design.md`, `docs/design/redesign/01-connectivity-graph.md`.
- Math: `docs/design/redesign/05-determinism-proof.md` (375/375 per-overload census), `docs/design/redesign/06-determinism-confidence.md` (independent re-census / three adversary passes — see its `Adversary findings` heading), `docs/design/redesign/07-determinism-certificate.md` (Shape algebra laws + 4 clean-cut seams), `docs/design/redesign/08-determinism-formalization.md` (the formalization — the PROOF tool the executable harnesses run against real linked source).
- Blind developers: the dogfood protocol under `docs/design/redesign/dogfood/` + the `bdd-testing` skill at `.claude/skills/bdd-testing/SKILL.md`.
- ~~Simulator~~ (CUT — Phase D removed): `docs/design/redesign/playground/` is **retired as a gate**. The cytoscape design-graph is at most an OPTIONAL human-legible diagram auto-derived from the Phase C certificate + the Phase F green slice; it carries no STOP authority and is never on the unlock path. The proof lives in the executable harnesses, not a browser drawing.

---

## A. MISSION · DONE · FRAME

### Mission
**Rewrite the Alis.Reactive framework green-field, from scratch, in a NEW FOLDER,
and ship 1.0.0 — autonomously, with zero feature loss.**

The framework is one data flow:

```
Frozen DSL (cshtml)  ->  Rich C# Plan Domain  ->  Hand-authored TS Plan Contract  ->  Runtime executor (browser)
```

C# never executes browser behavior — it serializes *intent* as a plan. TypeScript
never invents information the plan does not carry — it is a *dumb composable
executor*. **The plan is the only contract.** JSON-schema-as-contract is retired.
The live contract is the C# plan domain plus a **hand-authored, self-documenting
`runtime/types/plan.ts`** under a strict TS linter, guarded by a build-failing
**drift detector** (not a generator — see Directive 1).

### DONE = 1.0.0 RELEASED.
Not "compiles." Not "tests pass." **1.0.0 released**: all 12 modules built from
green certificates, the full RC1 behavior oracle green in a fresh clone, all 42
remaining Syncfusion components onboarded at full public API, NuGet packed and
verified, and the autonomous gate system demonstrably green end-to-end.

### FRAME (non-negotiable)
- **NEW FOLDER, everything from scratch.** A separate green-field root (e.g.
  `Alis.Reactive.v1/`) with its own `.slnx`. **No backward-compatibility worry.** Do
  not edit the old tree to "evolve" it; build the new one and prove it against the
  old as oracle.
- **Runs AUTONOMOUSLY and works WITHOUT the owner's help.** Self-driving agents +
  process + airtight gates that *truly work*, not paper. The owner approves nothing
  mid-flight; the gates are the approval.
- **ZERO FEATURE LOSS — not one feature less.** The 1168 Playwright + 192 vitest
  oracle and the RC1 product are the proof. **Named must-haves that must NOT drop:**
  the **Element builder** (`AddClass/RemoveClass/ToggleClass/SetText/SetHtml/Show/Hide`),
  the **Syncfusion typed Template** (`FusionTemplate.Create<TModel>()` →
  `Text<TProp>/Span<TProp>/Img<TProp>` model-bound bindings), **MORE DOM events in the
  builder** (`blur`, `input`, `focus`, etc. — see Directive 11/§B-must-haves), plus
  everything the oracle covers.
- **The project / solution organization is DECIDED deliberately and logically up
  front** (§E). Structure is never "discovered while coding."
- **NO INTERNAL EXPOSURE ACROSS PROJECTS — BANNED.** Zero `InternalsVisibleTo`. Strict
  per-project encapsulation. Each project's contract is provable from its **public
  surface alone** (this is what lets blind judges and external consumers verify
  against the real contract). The current tree's 4 IVT leaks
  (`Alis.Reactive.csproj` lines 69-72: Fusion, Native, FluentValidator,
  PlanTypeGenerator) are the exact debt the rewrite deletes.
- **DEEPEN / rewrite CLAUDE.md** as the operating standard for the new code (§F).

---

## B. THE 17 DIRECTIVES (first-class mandates, each with a grounded target)

Each directive is a mandate. Each names a concrete, source-grounded target. None is
optional. The OWNER VISION overrides the old in-place blueprint where they conflict.

### Directive 1 — KILL ALL TS CODEGEN; hand-author self-documenting TS under a STRICT linter (gate G3).
Remove auto-generation. **Delete** `PlanContractGenerator.cs` (~1170 lines, emits
`plan.ts`), the `tools/PlanTypeGenerator` console tool, the `generate:plan-types` npm
step (currently runs before `build:runtime` AND `typecheck`), and the
`InternalsVisibleTo(...PlanTypeGenerator)` it depends on. `plan.ts` becomes the
**hand-authored source of truth**: composable, *dumb* (pure executor), self-documenting
with **minimal** comments, under a **strict TS linter**. **KEEP and re-point** the
drift **detector** (`ContractDriftGate.cs` already does line-diff detection) so G3 runs
the **`--check` diff over the COMMITTED `plan.ts`** (`ContractDriftGate.Check` wired into
`dotnet test`, exits 1 with a diff) — NOT `generate:plan-types && tsc` (which
regenerated-then-typechecked its own output, the FM14 tautology); `git diff --exit-code
-- runtime/types/plan.ts` must also be clean. It *flags* C#↔TS mismatch on the
Kind/op-token surface and **fails the build** — but never regenerates. Document the
strict LLM-driven process for reflecting C# domain-model changes into the hand-authored
TS.
> **The strict linter (gate G3):** typescript-eslint `strictTypeChecked` +
> `stylisticTypeChecked`; `any` = **ERROR**; `no-floating-promises`, `no-unsafe-*`,
> `explicit-module-boundary-types`, `consistent-type-imports`, `no-non-null-assertion`;
> **`import/no-cycle`** to mechanically enforce the acyclic deep-module graph. tsconfig:
> `strict` + `noUncheckedIndexedAccess` + `exactOptionalPropertyTypes` +
> `verbatimModuleSyntax` + `isolatedModules`. **COMPOSABILITY:** small pure functions,
> narrow per-module exports, dependencies injected as callbacks (the `ArrayOpEngine`
> pattern), no side-effecting module top-level, no class where a function suffices.
> Premise correction (do not repeat as fact): "the runtime has more comments than
> code" is **false** today — comments are <10% of non-blank lines in every runtime
> folder. The hand-authored-TS goal stands on composability + a drift detector, not
> on a comment-ratio. Do not chase comment reduction as the win.

### Directive 2 — DSL stays COMPILE-TIME-CORRECT with PHANTOM BUILDERS — SACRED.
Invalid authoring is a **compile error**, never a runtime throw. Keep
`TypedSource<TProp>` → `ConditionSourceBuilder<TModel,TProp>` (typed operators
`Eq(TProp)`, `Between(TProp,TProp)`, source-vs-source), expression-only `InputField`,
the Element builder. **Convert these residual runtime throws to compile errors:**
`Standalone.Then` (today `ConditionContinuation.cs:138` throws); ElseIf/Else ordering
(`BranchBuilder.cs:80-92`); missing HTTP verb (`HttpRequestBuilder.cs:125`). Fix the
Element builder return-type asymmetry (`SetText(TypedSource)`→`ElementBuilder` vs
`SetText(string)`→`PipelineBuilder` is a least-surprise wart).

### Directive 3 — BRING DEFAULTS to the HTTP/gather surface; DISCOVER the real quirks.
Defaults already present to keep: gather param-name-from-member
(grep `DefaultPayloadName` — use site in `GatherExtensions.cs`, definition
`internal string DefaultPayloadName => _readMember;` in `TypedComponentSource.cs`);
JSON body default (grep `_bodyFormat = RequestBodyFormat.Json` in `HttpRequestBuilder.cs`). **NEW default to design:** **400 ProblemDetails
auto-surface** — today an error with no `OnError` route is *silently swallowed*
(`http.ts:269`); design a built-in default that reads `ProblemDetails.errors` into the
validation summary. **DISCOVER and design-away or document the unstated quirks**
(many exist; do not assume these are all): GET silently drops the body
(`http-fetch.ts:53`); empty JSON body → no `Content-Type`/no body
(`http-fetch.ts:61`); `WhileLoading`/`Finally` are single-slot Clear+Add — a second
call *silently replaces* the first (grep `_whileLoading.Clear`/`_finally.Clear` in
`HttpRequestBuilder.cs`); FormData request snapshot is `{}` (grep
`if (bodyIsFormData) return {};` in function `requestPayloadSnapshotFrom`, `http.ts`);
exact-status route then "any" fallback (grep
`routeMatchesStatus(status)) ?? routes.find(routeMatchesAnyStatus)` in
`routeResponseRoutes`, `http.ts`). Read the actual DSL + HTTP/gather source and enumerate the full
quirk list before designing the defaults.

### Directive 4 — IMPROVE THE GRAMMAR so the learning curve drops.
Conditions, arrays, HTTP must read clearly and compose naturally. Grounded targets:
collapse the **three** overload-shapes of `And/Or` (typed-source vs `(payload,path)`
vs nested-lambda — they overload-collide and are undiscoverable); rationalize the
6-way presence vocabulary (`Truthy/Falsy/IsNull/NotNull/IsEmpty/NotEmpty`); add
`MaxLength` to pair `MinLength`; reconsider LINQ-mimicking array names that imply full
LINQ works (only the fixed op set binds — `ReactiveArray` is deliberately *not*
`IEnumerable/IQueryable`); add `Min/Max/Average` to the array ops; reconsider modeling
`Confirm` as a "condition" when it is a user-decision async guard.

### Directive 5 — BETTER DSL NAMES, the way PROGRAMMING-LANGUAGE EXPERTS work.
Orthogonal, least-surprise, pronounceable, no cryptic abbreviations, consistent
vocabulary. Apply the **7 ordered naming principles** from `03-naming.md` (name the
thing not the GoF pattern; one verb/noun gloss; screaming intent over generic words
with the **banned-word guard** {artifact, contribution, claim, reject, fallback,
registry, lifecycle, Manager, Helper, Util, Info, Data, Context}; one-concept-one-name
across C#/JSON/TS/tests/docs; name size tracks foundational-ness; lane/purity in the
name when load-bearing; rename ONLY when a name lies, collides, or needs a paragraph —
never for novelty). **THE NAMING TEST is the gate** (grep `## The Naming Test` in `03-naming.md`; the gate
prose follows the heading): show the
name *alone*, cold, to a .NET dev who never saw the codebase; if they say what it does
in one breath and are right, it passes; otherwise rename. Each name IS the
coverage-matrix row's domain term — zero translation between "the module a dev opens"
and "the concept they were thinking about." **Do not churn surviving good names**
(`Shape`, `Kind`, `IdGenerator`, `PlanDocument`, `ConditionGraph`, `ReactionGraph`,
`ValueExpression`, `evaluateValue`, `assertNever`, `TypedSource<T>`).

### Directive 6 — SIMPLE design throughout.
No god-objects, no singletons, no fallbacks, no registries-as-control-flow, no magic
sentinels. Keep the browser-object family cleanly split — the
`BrowserObject`/`BrowserObjects`/`BrowserObjectId`/`BrowserObjectContract` types
(`BrowserObjectContract.cs` is 426 lines; grep `internal sealed class BrowserObjectContract`)
already carry the vendor-neutral object model and must not recollapse into one god-file.
Dissolve the surviving facades: `ValueExpression.cs` (590) facade + 4-type read
indirection (grep `public abstract class ValueExpression`); `PipelineBuilder` (4
partials — grep `partial class PipelineBuilder<TModel>` across `PipelineBuilder*.cs`);
`evaluate.ts` (204 lines — grep `export function evaluateValue`); `orchestrator.ts` (504).
Three serialization strategies → one. Dual condition evaluators → one. **Absence is a real variant**
(`NoRequestInput`, `Shape.None`, `InputBinding.None`), never `null`,
`[JsonIgnore(WhenWritingNull)]`, or `?? fallback`.

### Directive 7 — DOMAIN MODEL is an OPEN, VENDOR-NEUTRAL, DOCUMENTED, PUBLIC CONTRACT.
Any external consumer onboards **their own** component by following the contract.
Today the onboarding machinery is `internal` (`InputComponentRegistrationProfile`,
`ComponentRegistration` members, `ComponentProperty<T>`/`ComponentMethod`,
`ComponentEventOnboarding`), so an external assembly cannot implement a slice without
`InternalsVisibleTo` — which the FRAME bans. **Promote a documented PUBLIC onboarding
surface** (the `ComponentProperty/ComponentMethod` factories, the registration
profile, the event-wiring helper, `IComponent/IInputComponent/IAppLevelComponent`) so
a third-party assembly onboards a vendor component with **zero internal leaks**.
Syncfusion and Native become the *first consumers* of this public contract, not
privileged friends. Plan-node construction stays locked by **internal constructors
behind public factory/builder entry points** (`Html.*`, `PlanBuildContext`,
`IComponent`) — the API is frozen by internal ctors, not by friend assemblies.

### Directive 8 — MATH FIRST: every variant PROVEN to 100% BEFORE a single line.
The determinism math is the **gate that precedes implementation** (this is Phase C).
Today only **Shape** has the full universal-law treatment (the E/M/A/C/P/S/F law
families proven in `07-determinism-certificate.md` under `## 2. The Shape domain
algebra`, two independent harnesses + repro + adversary passes, dogfood 54/54 —
grep `54/54` in `docs/design/redesign/dogfood/shape/README.md`). The **375/375
per-overload census** (Triggers 10, Reactions 28, Conditions 52, Values 5, HTTP 47,
Arrays 19, Validation 102, Components 14, Slots 5, Plugins 65, App-level 28) + the 4
DET pipeline laws + the 4 clean-cut seam verdicts are the **foundation to EXTEND**.
For **each** of the 12 modules, replicate Shape's 5-artifact pattern: (i) a per-module
law set (totality, determinism, idempotence/shape-once, the merge/compose algebra,
exhaustiveness on its Kind union); (ii) a property/equivalence harness compiled
against **real** linked source + an independent second harness + a generator-coverage
tally; (iii) an adversary pass (the 3-judge pattern) yielding a reproduced witness or
clean; (iv) a dogfood build-from-spec-alone closing every invented gap with a fixture;
(v) the per-module **certificate** — and **only a green certificate unlocks that
module's production code.** The **interaction layer** adds one clean-cut verdict per
dependency edge (shape-once across the seam, lowering-matches-reader, plan-carried
lane, Kind-exhaustive) plus a whole-product composition proof that no many-to-one
collision exists anywhere. **Counts are PER-OVERLOAD**, never band rollups that fake
100%. *Math characterizes structure; whether a structural property is a bug depends on
the domain invariant — read the consumer first* (see the merge-union invariant in §C).

### Directive 9 — PERFORMANCE of `Html.RenderPlan` is a first-class goal.
`ReactivePlan.Render()` (`ReactivePlan.cs:90`) = `ResolveAll` → `BuildPlan()`
(snapshots 4 collections) → `PlanSerializer.Serialize` with static-cached
`JsonSerializerOptions`; polymorphism is ONE generic `PlanNodeDiscriminator<T>`
converter (grep `class PlanNodeDiscriminator<T>` in `Serialization/PlanNodeDiscriminator.cs`)
applied via 18 `[JsonConverter(typeof(PlanNodeDiscriminator<...>))]` attribute
registrations across the plan-model files — its single `Write` override delegates by
`value.GetType()` re-dispatch (verify with `grep -rn "JsonConverter(typeof(PlanNodeDiscriminator<" --include=*.cs` → 18; there are zero hand-written per-type converters). **There is zero
benchmark infrastructure today.** First move: a **BenchmarkDotNet** project over
`Render()` for representative plans (small / medium / wizard-sized), measuring
allocations + the `GetType()` re-dispatch cost, and deciding whether a System.Text.Json
**source-generated `JsonSerializerContext`** / `[JsonPolymorphic]`+`[JsonDerivedType]`
beats the current generic-converter + `GetType()` re-dispatch approach. Record an
allocation budget and the source-gen-vs-runtime-converter decision in CLAUDE.md.

### Directive 10 — SKILLS WITH PROCESS: efficient, faster, repeatable.
Especially the component-onboarding skill — mechanical, repeatable vertical slice.
Grounded fixes: **co-locate all onboarding skills in-repo** (the category skills
`onboard-fusion-input/display/app-level` currently live only in the user's global
`~/.claude/skills/` — a fresh autonomous clone gets the router but not the slice
templates; this *blocks autonomy*). **Add a scaffolding generator**
(`scaffold-fusion-slice.mjs`) that emits the 7-file slice skeleton + sandbox view stub
+ Playwright stub from resolved facts `{class, category, jsNamespace, builderName,
valueMemberPath}`, leaving only proven member rows to fill. Add a one-shot inspection
orchestrator (discover → inspect-surface → inspect-event-payload → single candidate
matrix). **Keep the browser-proof gates** (the 7 Automation Gates, raw-HTML probe) —
they are the correctness core; speed comes from scaffolding + inspection, never from
skipping proof.

### Directive 11 — ONBOARD ALL Syncfusion components at FULL public API (minus existing).
**EXCLUDE the 53 already onboarded** (51 `Alis.Reactive.Fusion/Components/` dirs + 2
AppLevel: FusionConfirm, FusionToast — both re-verified present in this repo).
**Onboard the 42 remaining** top-level SF EJ2 components that have an MVC builder,
covering the **full public API** of each: AccumulationChart, AppBar, BarcodeGenerator,
BlockEditor, Calendar, Chart, Chart3D, ChatUI, CircularChart3D, CircularGauge,
DashboardLayout, DataMatrixGenerator, Diagram, DocumentEditor, DocumentEditorContainer,
Fab, FileManager, Gantt, HeatMap, ImageEditor, LinearGauge, Maps, Overview, PdfViewer,
ProgressBar, QRCodeGenerator, QueryBuilder, RangeNavigator, Ribbon, Signature,
Skeleton, Smithchart, Sparkline, SpeechToText, SpeedDial, Splitter, Spreadsheet,
StockChart, Timeline, TreeGrid, TreeMap, TreeView. (Recompute the backlog
deterministically before starting: universe = all `T:*Builder` type members
(`grep -oE '<member name="T:[^"]*Builder"'` → 101) in `Syncfusion.EJ2.xml` — the
NuGet artifact at `~/.nuget/packages/syncfusion.ej2.aspnet.core/<ver>/lib/netstandard2.0/Syncfusion.EJ2.xml`,
NOT a repo file — minus the nested settings builders enumerated below minus the
onboarded set, mapped via each `FusionXxxHtmlExtensions.cs` factory leaf name. The XML
has no top-level/nested marker, so the nested-vs-component split is this prompt's manual
exclusion, not a readable property. Most leaves use the `EJS()` factory — grep
`EJS().` in the `*HtmlExtensions.cs` files (49 of 51); the 2 SmartXxx components
(FusionSmartPasteButton, FusionSmartTextArea) render via raw
`new ej.buttons.SmartPasteButton(...)` / `new ej.inputs.SmartTextArea(...)` initializers
instead.) Do **not** re-onboard nested settings builders (GridColumn, MenuItem, Pager, etc.). Zero TS runtime / schema
/ core descriptor changes during onboarding — if you reach for one, the slice is
missing information.

### Directive 12 — TESTS = PURE BEHAVIOR.
Stable, well-organized, vertical slices. **If a test changes because the
implementation changed, it is a BAD test; tests change only when user-visible
behavior changes.** The 1168 Playwright (133 files, already foldered by behavior
domain with `When...`/behavior-named methods) + 192 vitest oracle survives a
green-field implementation swap *because* it tests behavior, not internals. **The
impurity to fix:** Playwright tests that assert plan-JSON *structure* (e.g.
`CoreBehaviors/WhenPlanBoots.cs` asserts `Does.Contain("page-ready")`,
`behaviors.length == 4`) are implementation-coupled — **move plan-shape assertions
into C# domain unit tests** (where shape *is* the behavior under test) and keep
Playwright purely on rendered DOM + interaction outcome. **The TS test approach is
Matt-Pocock-style pure-behavior TDD** (Directive 17): red fixture first, behavior under
test stated as one sentence, the test changes ONLY when user-visible behavior changes,
never when an internal type or module moves.

### Directive 13 — BETTER NuGet + multi-TFM + asset delivery + local dev loop.
Keep the sound `net48;net10.0` same-DSL multi-target (21 `#if NET48` files today across
Alis.Reactive (5) + Alis.Reactive.Native (16) — verify with `grep -rln "#if NET48"
--include=*.cs` from the repo root; do not abandon net48). Keep `Directory.Build.props` centralization (`VersionPrefix`,
`AlisAssetsDist` single npm→pack handoff). **Fix:** consolidate the two near-duplicate
targets files (`AlisReactiveAssets.targets` 78 lines vs `AlisReactiveBrowserAssets.targets`
44 lines) into one; improve asset delivery (dist → NuGet `build\`+`buildTransitive\` →
consumer `wwwroot`); improve the local dev loop (watch:runtime / watch:design-system /
dotnet watch — framework TS/CSS edits need only a browser refresh). The green-field
solution org is decided up front (§E).

### Directive 14 — NOTHING SHALLOW: keep the sound parts, complete the parked wins.
**KEEP (do not regress):** deterministic collision-free `IdGenerator`
(`{Namespace_TypeName}__{MemberPath}`, no DOM scanning, all vendors → same id);
shape-once egress (single `applyShape` engine, `runtime/core/shape-convert.ts`);
vendor-neutral "everything is a JS object" model (`BrowserObjectContract`:
properties/methods/events; vendor knowledge isolated to `domain/component-driver.ts`
(the sole vendor-dispatch hub — grep `componentDriver`/`resolveRoot`/`wireFusion`/`wireNative`)
which delegates to the per-vendor leaf wirers `resolution/event-fusion.ts` /
`resolution/event-native.ts` (grep `export function wire`); `resolver.ts` is a
vendor-agnostic delegate, not an isolation site (`resolveVendorRoot` no longer exists,
replaced by `ComponentDriver.resolveRoot`)); same-DSL `#if NET48` multi-target; the dumb two-lane runtime
executor (`execute.ts` switch + `assertNever`, sync-void / async-Promise by
plan-carried kind); static-cached `JsonSerializerOptions`; the `kind`-discriminator
polymorphism; the drift **detector** (re-point, do not delete).
**COMPLETE the parked wins** (each confirmed still unbuilt): **ReactionLane as a
carried plan fact** — `SequenceReaction`/`BranchReaction` (grep
`public sealed class SequenceReaction`/`BranchReaction` in `ReactionGraph.cs`) today
emit only their `Kind` discriminator (`sequence`/`branch`) and carry NO lane field;
the sync/async distinction currently lives in the TS runtime executor, not the C# plan
model. The win is to STAMP `sync`/`async` onto those C# reactions and route the runtime
on `reaction.lane`, then **delete every `instanceof Promise` probe**
(`execute.ts:287,314`; `conditions.ts:70,89,106`; grep `result instanceof Promise` in
function `handleClick` of `native-action-link.ts`);
**`Standalone.Then` compile-time absence** (no `Then` method at all); **active-plan
singleton removal** — delete `activeRuntimePlan` + all `reset*ForTests`, thread
`ActivePlan` explicitly into `executeReaction`; **`StartsWhen` public-symmetric
widening** (flag if it conflicts with the spec's intended surface — the proven-safe
direction is contraction); **per-module folder+namespace homes** (the 12-module
collapse — §E); **Fix 1: WholePayload/WholeElement as distinct node KINDS** killing
the `responseBody`/`elementValue` magic-member collision
(`ValueExpression.cs:379-403`, `plan.ts:783-799`, and the read path in `evaluate.ts` —
grep `readsWholePayload`/`readsWholeElement` / `function readFromPayload`, NOT a line
range; the file is 204 lines) — **this is
what makes the spec's 100% true-in-source; land it early.** Plus Fixes 2-4 (full
`Shape.FromClrType` table; array→JSON egress obeys shape-once; app-level fixed ids as
one shared C#→TS constant).

### Directive 15 — CONTEXTUAL per-micromodule CLAUDE.md.
Each of the **12 module folders carries its OWN `CLAUDE.md`** — module-specific context
(its laws, its Kind union, its seam/interface boundary, its named fixtures, its
do/do-not) — **in addition to the deepened root CLAUDE.md** (§F). Contextual memory lives
next to the module a dev opens, not only at the root. (§E records this in the project
layout; §F lists it as a CLAUDE.md edit.)

### Directive 16 — RELIABLE HOOKS: write NEW, TESTED hooks (do NOT re-enable the broken ones).
Hooks "never worked when supposed to" — the owner **disabled** them because they **fired
unreliably, fired at the WRONG lifecycle time, and CORRUPTED THE CONTEXT** (dumping hook
output into the conversation). Re-enabling the existing `commit-requires-relevant-tests` /
`merge-requires-all-tests` rules would re-introduce that corruption — **they go on the KILL
LIST.** **Fix: author NEW hooks engineered against those exact three failure modes, each one
TESTED before it is trusted:**
- **Right trigger, right time** — a precise matcher + the correct lifecycle event
  (PreToolUse / PostToolUse / PreCommit as the guard demands); it never fires on tools or
  moments it does not guard.
- **Fast and SILENT — the anti-corruption rule** — emit NOTHING to the conversation on
  success, and at most a **single bounded line** on failure. ALL real output (command, exit
  code, stdout) goes to a **SHA-bound transcript file on disk**, never into the context. A
  hook may never flood or inject content into the conversation.
- **Each hook ships with a firing TEST** proving it (a) FIRES when it should, (b) STAYS
  SILENT when it should not, and (c) BOUNDS its output. **A hook is not trusted until its
  firing test is green** — an untested hook is treated as not present.
- **A hook that did not fire is itself a RED gate** — a meta-check asserts the SHA-bound
  transcript EXISTS for the commit SHA; a missing transcript fails the fence.
The hooks to write: a **commit gate** (tests ran for touched code), a **merge gate** (full
suite green), and the **`oracle-frozen-assertion-guard` PreCommit hook** (FM8) blocking any
diff that weakens a `[FrozenBehavior]` value unless the commit carries an explicit
`ORACLE-EDIT:` note touching only `[OracleInternal]` assertions. An LLM persona's SIGN-OFF is
accepted ONLY alongside the machine-captured transcripts. (Wired in §D under RELIABLE HOOKS.)

### Directive 17 — TS TESTS = Matt-Pocock-style pure-behavior TDD.
The TS test approach is Matt-Pocock-style TDD: red fixture first; the behavior under test
stated as one sentence; the test asserts user-visible behavior (per Directive 12), never
an internal type/module shape — so the 192-vitest oracle survives a green-field
implementation swap. Add this as a co-located in-repo skill (Directive 10). The TEST tool
is this TDD; the PROOF tool is the MATH (Directive 8 + `08-determinism-formalization.md`)
— two distinct instruments, not interchangeable.

---

## C. THE BEHAVIOR ORACLE (the differential fence) — THE GOLDEN RULE

Three layers, all green before any push, verified bottom-up. The RC1 product itself is
the fourth: drive the **real public DSL** through the **real `Render()`** (the
`determinism-domain` dogfood pattern over 30+ plans) as a continuous parity guardrail.

| Layer | Count (this repo, re-verified) | What it asserts |
|-------|--------------------------------|-----------------|
| Playwright (`tests/Alis.Reactive.PlaywrightTests`) | **1168** across 133 files | What the **user sees** — DOM state, visible text, focus, disabled state, gather body |
| vitest (`Alis.Reactive.Assets/runtime/__tests__`) | **192** across 28 files | Runtime read-path behavior in jsdom |
| typecheck / drift detector | detector-based | C# plan domain ↔ hand-authored `plan.ts` agreement |

**THE GOLDEN RULE — two test populations, two policies (conflating them is the
cardinal trap):**

- **(a) USER-VISIBLE behavior assertions are FROZEN.** Visible DOM/text/focus/disabled
  state and the one justified non-visible exception (`request.PostData` on
  gather/HTTP) are the contract residents depend on. **If a behavior assertion fails,
  the rewrite is wrong — never the test.** Never silently weaken, delete, or rewrite a
  behavior assertion to make a rewrite pass.
- **(b) Plan-shape / oracle-internal assertions are DELIBERATELY UPDATABLE — but only
  as a DOCUMENTED edit tied to a concrete win, never a quiet edit to dodge red.** ~50
  of the 133 Playwright files read `#plan-json` for vendor/kind strings; vitest
  plan-kind assertions and `plan.ts` type names are oracle internals. A fresh-design
  win (new `lane` field, new `whole-payload` kind) *will* change these — update them
  deliberately, documented, tied to the win.

**Boot contract** (or the entire oracle reports false): the runtime emits the boot
marker + `[alis:boot] booted` trace line with **zero console errors** (every test ends
`AssertNoConsoleErrors()`). Playwright self-hosts on a random free port — do **not**
pre-start a sandbox on 5220. A `TimeoutException` that passes on isolated re-run is a
machine-load flake, not a product bug.

**The merge-union invariant you must NOT "fix":** `merge(object{a}, object{b}) =
object{a,b}` (a closed **union**) is CORRECT, not a lattice join. `TryMergeContracts`
(grep `internal static bool TryMergeContracts` in `ShapeContractCompatibility.cs`; called
from `BrowserObjectContract.cs`) is shape-contract compatibility merging — it picks the
most specific compatible `Shape` for a browser-object member and returns an explicit
conflict when none satisfies both contracts; collision-free deterministic `IdGenerator`
IDs guarantee same-id ⟹ same-shape ⟹ the `==` fast path always hits; the non-equal branches are
robustness-only and never exercised with differing shapes. Document this invariant in
the merge module so no maintainer imposes join semantics. Do not make merge
associative (M5) or a lattice (C1).

---

## D. AUTONOMOUS GATES — HARDENED LEAN GOVERNANCE (per phase and per module)

Reliability is **engineered, not promised.** The owner is out of the loop, so "a persona
will be careful" is not a control — only a machine-checked, SHA-bound, independently
reproducible witness is. **A prose catch is not a catch.** Every governance element below
maps to at least one real, traced failure mode (FM1–FM17 in
`docs/design/redesign/governance-gaps.md`); anything mapping to none is CEREMONY and was
cut. **All gates green before every commit, before starting the next module, and at the
final 1.0.0 cut.** All commands from the green-field root.

### Phases — A, B, C, E, F (Phase D CUT)
The spine is **five phases.** Phase D (HTML simulators) is **CUT entirely** — FM17 proved
the simulator is a non-oracle (a hand-built second implementation in English, the exact
FM17 risk), and its only real STOP ("an undrawable flow = an unwritten matrix row") is
already owned by the Phase C math gate (G-MATH-100 cannot certify a row it never
enumerated). Any human-legible diagram is an OPTIONAL doc produced FROM the Phase C
certificate + the Phase F green slice — never a gate, never a STOP authority, never on the
unlock path.

### Phase gates (the spine's order is enforced by these)
- **Phase A gate:** every load-bearing name passes THE NAMING TEST (cold .NET-dev read,
  one-breath-correct); the banned-generic-word guard is clean; one-concept-one-name
  holds across C#/JSON/TS/tests/docs; **the good DEFAULTS (Directive 3) and the
  grammar/vocabulary improvements (Directive 4) are decided here, deliberately, as Phase
  A artifacts** — never discovered later while coding.
- **Phase B gate:** every module has one author seam + one node family + one runtime
  reader; the dependency graph is acyclic and layered (verified against
  `02-micro-modules.md`); no shallow module survives (a type that only carries
  parameters, hides a branch, needs a paragraph, or maps to no DSL node is inlined or
  deleted). The B→C handoff records each public seam signature for **G-SURFACE**.
- **Phase C gate (the unlock token):** the module's per-module certificate is GREEN,
  **proved BY EXECUTABLE property/equivalence harnesses run against real linked source —
  the harnesses ARE the simulation that proves; there is no browser visualization in the
  proof.** Laws-hold (universal, machine-checked against real/linked source) +
  adversary-clean + dogfood-build-from-spec-alone-green + per-overload census X/X where
  **X is MACHINE-DERIVED** (see G-MATH-100); plus the clean-cut verdict for every
  dependency edge it introduces. **The math is authored by ARCHITECT personas and
  re-proven by BLIND developers (Phase E) — proof by those who do not see the
  implementation, so the certificate is forced by the spec, not fitted to code. No
  production line for a module is written until its certificate is green.**
- **Phase E gate:** a blind builder hits green from spec + fixtures alone with zero
  inventions / zero divergences (an independent judge diffs against real source
  line-by-line to confirm behavior is *forced*, not merely green); a blind BDD reviewer
  confirms each test is one-sentence-the-role-would-say, traces to a criterion, fails
  when behavior breaks, uses real interactions only, and survives internal refactoring.
  Coldhand runs as a sub-agent whose entire input is `{one module's spec, its named
  fixtures}` and emits a **blind-context provenance manifest** (literal file/section list
  + transcript hash proving zero reads of `Alis.Reactive/**`, other specs, matrices,
  prior scratch, or coaching); SIGN-OFF requires Crossjudge reproducing that the manifest
  is spec+fixtures ONLY — absent/impure ⇒ rejected as "not actually blind" (FM16).
- **Artifact-checkpoint barrier (every A–E exit):** each A–E exit criterion must WRITE
  the deliverable to its path AND git-commit it (an artifact-commit naming the artifact)
  BEFORE the handoff crosses — so a large certificate/proof/spec lost mid-generation is
  never silently re-burned (FM12). Design-doc / proof / spec commits are **exempt** from
  `commit-requires-relevant-tests`. Gatekeeper owns this cadence across all phases.

### The 4 personas (down from 6 — Greenweld + Flowwright removed)
- **Gatekeeper** — owns the unbypassable spine order + the per-commit fence; with the
  owner out of the loop, the gates ARE the approval; owns the net48 leg; orchestrates the
  per-slice fresh-clone + commit-identity + transcript checks; owns the YIELD LEDGER
  (FM15 — a per-gate/persona/handoff record of the UNIQUE reproduced failure-witness each
  element caught; any element with zero unique catches, or whose every catch an earlier
  automated gate already makes, is flagged CEREMONY and cut or justified in writing).
- **Crossjudge** — independent reproduce-before-accept (FM5); a finding is a reproducible
  witness, not an opinion, and is rejected if it cannot be reproduced; spot-injects a
  known mismatch to prove a gate CAN go red (FM10); runs the adversary panel on the proof
  denominator (FM13); re-derives the frozen-vs-updatable classification rather than
  trusting the editor's self-label (FM8/FM9). The frozen-vs-updatable POLICY is
  Crossjudge's (the only uniquely-Greenweld thing, reassigned here).
- **Buildhand** — the Phase F implementer, weakest authority — writes only against a
  green certificate; escalates a design-decision-in-a-fill rather than invent (FM4);
  **cannot report from uncommitted edits** (FM5).
- **Coldhand** — the blind builder — learnability / spec-completeness (FM16) +
  impl-coupled-test review (FM8), with the Crossjudge-inspected context-provenance
  manifest above.

### Per-module code gates (Phase F) — the machine-checked witnesses
- **G1-BUILD `dotnet build`** — BOTH TFMs (`net48;net10.0`). A net10-only API in the plan
  domain without an `#if NET48` shim fails the net48 leg. Only the Fusion project is
  net10-only. (G1's unique value is the C# leg of FM2 + the net48 leg; FM6 is NOT owned
  here — it bites at the behavior-oracle precondition.)
- **G2-CSHARP-TESTS `dotnet test`** — the C# unit suite (domain fixtures; the one write
  path; FM8 where shape IS the behavior under test). **Carries the reverse-coverage
  census assertion (FM4):** every public verb/overload, plan-node `Kind`, and runtime
  switch-case in the slice must cite a specific matrix row (`file:line` in
  `04-matrix-*.md`); any symbol or branch with no citation FAILS, and the implemented
  surface count must EQUAL the matrix count — an inequality in EITHER direction is red
  (closes the under-coverage-only asymmetry that today catches loss but not creep).
- **G3-DRIFT `npm run typecheck`** — the re-pointed **drift DETECTOR** + `tsc --noEmit` on
  both tsconfigs (strict linter — see G3 hardening in Directive 1/5). **REWRITTEN per
  FM14:** `typecheck` runs the `--check` diff over the **committed** `plan.ts`
  (`ContractDriftGate.Check` wired into `dotnet test`, exits 1 with a diff on mismatch),
  NOT `generate:plan-types && tsc` (which regenerated-then-typechecked its own output, a
  tautology that auto-absorbed drift). There is **no `generate:plan-types` step** —
  Directive 1. A renamed C# property the hand-authored `plan.ts` no longer matches fails
  here; `git diff --exit-code -- runtime/types/plan.ts` must also be clean.
- **G4-VITEST `npm test`** — vitest/jsdom (the read path, a distinct layer; FM8),
  both workspaces.
- **G5-BYTE-STABILITY** — re-serialize the same plan twice → identical camelCase bytes
  (shape-once + single `PlanSerializer` ownership). **Converted from an eyeballed
  `Console.WriteLine` into `[Test]` cases inside `dotnet test` (exit code, not stdout)
  per FM10**, with a COUNT GUARD (`Assert.That(Plans.Length, Is.EqualTo(EXPECTED))`) + a
  frozen distinct-kind-token set so a shrunk catalog fails RED, and a default-path
  mutation negative control proving the compare is non-vacuous. `git status` stays clean
  (all bundler outputs gitignored; a `dist/`/`wwwroot/` bundle in `git status` is a bug,
  do not `git add` it).
- **G6-RENDER-PERF** — BenchmarkDotNet over `Render()` stays within the recorded
  allocation budget (Directive 9). No other gate catches a perf regression.
- **G-SURFACE (NEW)** — FM3: a public verb that smuggles infrastructure in its signature
  ships behavior-invisibly (byte-identical plan JSON passes every behavioral gate).
  (1) PUBLIC-SURFACE PARITY vs RC1 — PublicApiAnalyzers `PublicAPI.Shipped.txt` diff; any
  public verb whose parameter list gains an infra-typed parameter fails; (2) a
  NetArchTest/Roslyn architecture test asserting **no public DSL signature references an
  infrastructure type** (no `connectionString`/`HttpClient`/`vendorRoot`/`baseUrl`+`path`
  leak). Gatekeeper-owned, Crossjudge-re-run.
- **G-MATH-100** — FM13: the certificate is the GATED artifact and the census is
  **MACHINE-DERIVED** — a committed harness reflects the public DSL surface from the real
  assembly and EMITS the denominator; the certificate's X/X must EQUAL that reflected
  count (kills the hand-summed 375 and the families-not-rows class). Add a
  **COMPOSITE-VARIANT scope row per cross-module graph edge** (Confirm.And across
  Conditions×Value, Into(whole-payload) across HTTP×Value×Slot, …), each with its own
  one-output proof; the gate REJECTS a certificate whose composite-row count is below the
  labelled cross-module edge count. Any `RESOLVED-BY-REDESIGN` row stays a **RED
  sub-certificate** until the resolving fix actually ships in built source.
- **G-FRESH-CLONE** — FM1 + FM7, **moved INTO the per-commit fence** (was "periodically /
  at the cut," too late): before G1, delete `dist/`, run `npm ci && npm run build:all &&
  dotnet build`, and require `dist/` to repopulate — so reliance on a gitignored/untracked
  input goes red on the slice that introduced it. Plus a **commit-identity preflight** at
  worktree creation and as the first step of every gate run (replaces the file-presence
  proxy): assert `HEAD == feature-branch tip` (fetched fresh), `rev-list --count
  feature..HEAD == 0`, and `git cat-file -e HEAD:<feature-only-path>` for a manifest of
  paths absent on main.
- **Behavior-oracle gate** (merged G-ORACLE-SLICE + G-RC1-PARITY — one Playwright
  invocation per slice, not two) — FM9 + FM6 + FM11. Playwright against **freshly built**
  assets with two documented assertion tiers (frozen user-visible vs updatable
  plan-shape). **FM6 precondition (owned here, not in G1):** a fixture pre-step `npm run
  build:all`, plus a **served-bundle-hash check** — embed a content-hash build-stamp and
  have the readiness probe assert the served `/scripts/alis-reactive.dev.js` hash equals
  the just-built source hash (a stale gitignored bundle returns HTTP 200 fine, so a 200
  probe is not enough). **FM11 protocol (owned here):** a PRE-gate orphan sweep BY
  PROCESS NAME (`pkill -f Alis.Reactive.SandboxApp; pkill -f 'chromium.*--headless'` —
  NOT by port 5220, which the random-port fixture never uses), an in-gate filtered retry
  of only the failed tests from the TRX with an orphan sweep between attempts (pass-on-retry
  = flake/green), and a finally-block process-tree kill in the fixture. A full `dotnet
  build` precedes the suite (a Core-only build = 852/852 fail in ~30s = a stale-build
  artifact, not a regression/flake — FM6a).
- **G-ORACLE-COMPLETENESS (NEW)** — FM9, the prime directive's machine guard: (1) a
  FROZEN ORACLE MANIFEST snapshotting RC1's **1168** behavior-assertions keyed by
  behavior-id; the green-field tree must contain a **non-skipped, executing test for
  EVERY id** or the gate fails (closes "never-ported" — `DONE` asserts
  `green_field_count == 1168`, not "the tests physically ported in"); (2) RC1-SOURCED
  MATRIX COMPLETENESS — G-MATH-100 fails if any reflection-extracted RC1 DSL variant has
  no matrix row (closes "never-written-row"); (3) the classification of any edited
  manifest test is **re-derived by Crossjudge** from the assertion target
  (DOM/text/focus/disabled/PostData ⇒ policy-(a) FROZEN), never self-asserted by the
  editor (closes misclassification). Gatekeeper-owned, Crossjudge-run at every integrate
  and hard at the cut.

### RELIABLE HOOKS — NEW + TESTED, proven-to-fire, never context-corrupting (FM5/FM8; Directive 16)
Hooks "never worked when supposed to" because the owner DISABLED them — they fired
unreliably, at the WRONG lifecycle time, and **CORRUPTED THE CONTEXT** (dumped output into the
conversation). **Fix, and the standard going forward — write NEW, TESTED hooks; a hook is a
guardrail only if its firing is itself verified AND it never pollutes the context:**
- **KILL the old `commit-requires-relevant-tests` / `merge-requires-all-tests` rules and write
  NEW replacements** — precise matcher + correct lifecycle event; SILENT on success, one
  bounded line on failure; all real output to a SHA-bound transcript on disk, never the
  context; and a **firing TEST** (fires-when-it-should, silent-when-not, output-bounded) that
  must be green before the hook is trusted. Do NOT re-enable the broken ones.
- **Every gate writes a SHA-bound transcript file** — command + exit code + stdout tail,
  keyed to the commit SHA. Gatekeeper's commit-fence refuses any slice lacking a fresh
  **exit-0** transcript per gate; an LLM persona's SIGN-OFF is accepted ONLY alongside
  the machine-captured transcripts it must have produced (closes the relocate-the-overclaim-to-the-judge
  escape — FM5).
- **A hook that did not fire is itself a RED gate.** A meta-check asserts the SHA-bound
  transcript EXISTS for the commit SHA — a missing transcript fails the fence exactly
  like a non-zero exit code would.
- **Add the `oracle-frozen-assertion-guard` PreCommit hook (NEW)** — FM8: it BLOCKS any
  diff weakening a `[FrozenBehavior]` expected value unless the commit carries an explicit
  `ORACLE-EDIT:` note AND touches only `[OracleInternal]` assertions. (Tag assertions
  `[FrozenBehavior]` vs `[OracleInternal]` and SPLIT the ~50 mixed Playwright files so no
  file holds both.)

### The autonomous loop (no owner in the loop) — Verify-and-land
**Buildhand builds one slice → Crossjudge verifies (independent, reproduce-before-accept)
→ Verify-and-land (integrate + commit collapsed into ONE atomic stage — no failure mode
lives uniquely between integrate and commit).** A finding is a **reproducible witness, not
an opinion**, and is rejected if it cannot be reproduced; the slice lands only on green;
commit **one closed matrix row per slice** carrying the SHA-bound evidence bundle
(transcripts, byte-compare, perf, the matrix-row id). No lone-wolf rewrites; never start
slice N+1 before slice N has a focused proof AND a commit. Refuse to rubber-stamp; hold the
work accountable with **zero false positives**.

### Worktree & branch discipline (autonomy-critical)
- Build on an **explicit feature-branch worktree** based on `origin/<branch>` (fresh
  fetch). **Never** use the local ref; **BAN** the main-based agent-isolation worktree
  (`isolation:'worktree'` silently bases on main HEAD for any branch ahead of main and
  yields catastrophic false negatives — it once reported "no README/scripts/Assets" on a
  761-commits-ahead branch; the documented 2026-03-28 / 2026-05-31 recurrences are FM7).
  ```bash
  git fetch origin
  git worktree add --detach .worktrees/green-field-rewrite <feature-branch>
  ```
- **Verify base by the G-FRESH-CLONE commit-identity preflight, NOT the file-presence
  proxy** (a main-based tree passes a README/scripts check, which is exactly how FM7
  escaped twice): assert `HEAD == feature-branch tip` (fetched fresh), `rev-list --count
  feature..HEAD == 0`, and `git cat-file -e HEAD:<feature-only-path>` for a manifest of
  paths absent on main; require `node_modules/dist` ABSENT (truly fresh).
- Delete old code only after the new module passes all gates; never strand the oracle
  on a half-migrated module. **Progress is reported only from committed, all-green
  work.**

---

## E. KEEP-vs-REWRITE + PROJECT / NAMESPACE / ENCAPSULATION MAP (decided up front)

### The new-folder project layout (the structural prize)
A separate green-field root with **one `.slnx`** and per-project boundaries chosen by
**deployment + dependency layering** (NOT one-assembly-per-module — that balloons NuGet
count and re-couples via friend access). ~8 projects:

| Project | TFM / PackageId | Holds |
|---------|------------------|-------|
| **P1 Alis.Reactive** | `net48;net10.0` / `AlisReactive` | The vendor-NEUTRAL plan domain + 3 kernels/spine + author builders + serialization + Razor authoring. **Internal folders = per-concept namespaces:** `Alis.Reactive.Shape`, `.Kind`, `.Value`, `.Condition`, `.Reaction`, `.Request`, `.Trigger`, `.Component`, `.Slot`, `.Plan` + `.Builders` / `.Razor` / `.Serialization`. The per-concept collapse done as **real namespaces**, not technical-layer folders. |
| **P2 Alis.Reactive.FluentValidator** | `net48;net10.0` / `AlisReactive.FluentValidator` | Validation authoring (`ReactiveValidator<T>`/`ClientRule`/`WhenField` + render-time binder). |
| **P3 Alis.Reactive.Native** | `net48;net10.0` / `AlisReactive.Native` | Native component vertical slices. |
| **P4 Alis.Reactive.Fusion** | `net10.0` only / `AlisReactive.Fusion` | Syncfusion component vertical slices (incl. the typed Template). |
| **P5 Alis.Reactive.DesignSystem** | / `AlisReactive.DesignSystem` | Design-system CSS. |
| **P6 Alis.Reactive.Assets** | `netstandard2.0`, `IsPackable=false` | Hosts the `runtime/` TS + dist bundles. **Slot is RUNTIME-ONLY** and lives here (`runtime/lifecycle/`). |
| **P7 Alis.Reactive.NativeTagHelpers** | | Tag helpers. |
| **P8 Alis.Reactive.Analyzers** | `netstandard2.0` | Analyzers (`IsPackable=false`). |

Tools: a benchmark project (Directive 9). **No `tools/PlanTypeGenerator`** — codegen is
killed (Directive 1). P2/P3/P4 depend on P1 via `ProjectReference` and consume **only
P1's public domain contract**.

### Module → build order (dependency-ordered, the 7 waves)
Single-thread: `1 Shape · 2 Kind · 3 Value · 4 Condition · 5 Request · 6 Component
(pulled forward — stub `BrowserObjectId`/`BrowserObjectContract` TYPES first) ·
7 Reaction · 8 Trigger · 9 Slot · 10 Validation · 11 Plugin · 12 Plan`. As waves:
W1 Shape · W2 Kind · W3 Value · W4 Condition+Request · W5 Reaction+Trigger ·
W6 Component+Slot+Validation+Plugin · W7 Plan. A module is coded only after every module
it depends on is green. The apparent Slot→Plan / Reaction→Slot cycle is NOT a cycle:
Slot depends only on the `PlanDocument` TYPE; Plan reaches Slot behavior only through
the Reaction `inject` handler at runtime.

### Strict encapsulation — NO InternalsVisibleTo
P1 exposes a deliberate, documented, **vendor-neutral PUBLIC DOMAIN CONTRACT** (the
open contract any external consumer onboards against — Directive 7). Plan-node creation
stays locked by **internal constructors behind public factory/builder entry points**.
Fusion/Native/FluentValidator use ONLY that public contract via `ProjectReference` —
**ZERO `<InternalsVisibleTo>`.** This replaces today's IVT-to-4-assemblies +
public-types-with-internal-ctors-across-assemblies hack.

### Stable vs reshapable namespaces
- **STABLE — consumer `@using` surface, must NOT break:** `Alis.Reactive.Native.{Components,AppLevel,Extensions}`,
  `Alis.Reactive.Fusion.{Components,AppLevel}`, `Alis.Reactive.Builders.Requests`, the
  `Html.*` extensions, `NativeTagHelpers.*`, `ReactivePlan<TModel>` in `Alis.Reactive`.
- **FREELY RESHAPABLE — internal, zero consumer `@using`:** the `PlanModel` / non-Requests
  `Builders` / `Serialization` namespaces dissolve into the per-concept module
  namespaces. Over-caution (treating every namespace as load-bearing) is itself a
  failure mode.

### Resolve before relocating
`scaffold/Condition.md` §4 proposes a NEW `Condition/` folder that conflicts with the
in-place "no new folders" rule but is **consistent under the green-field per-concept
rule** — apply per-concept folders to **all 12 modules uniformly** (that is the prize;
`PlanModel/` itself dissolves).

### Per-module CLAUDE.md (Directive 15)
**Each of the 12 per-concept module folders carries its OWN `CLAUDE.md`** — module-specific
context (its laws, its Kind union, its seam, its named fixtures, its do/do-not) — in
ADDITION to the deepened root CLAUDE.md (§F). Contextual memory lives next to the module a
dev opens, not only at the root.

### Do NOT regress (the sound foundations — §0 thesis 3 keeps them honest)
The KEEP list in Directive 14. In particular: do **not** revive a reflective generator,
a hand-authored contract *without* a drift gate, JSON-schema-as-contract, or
`AssertSchemaValid`; do **not** delete the drift detector; do **not** abandon net48; do
**not** churn surviving good names.

---

## F. DEEPEN CLAUDE.md (an explicit early task — Phase A/B work)

Before Phase F, **rewrite the green-field CLAUDE.md** as the operating standard for the
new code. Required edits:
- **Contract section:** replace "Generated TypeScript types come from the C# plan domain
  via PlanTypeGenerator / regenerate `runtime/types/plan.ts`" with "**`plan.ts` is
  hand-authored, self-documenting TS under a strict linter; the re-pointed drift
  DETECTOR runs `--check` and fails the build on Kind/op-token mismatch against the
  COMMITTED `plan.ts` but NEVER regenerates**"; document the LLM-driven reflection
  process for propagating C# domain changes into TS.
- **Build & Run:** remove the codegen npm steps (`generate:plan-types`); replace with
  hand-authored + strict lint + drift-`--check`-over-committed-`plan.ts` (FM14) +
  typecheck + `git diff --exit-code -- runtime/types/plan.ts`.
- **Strict TS lint (Directive 1, gate G3):** record the typescript-eslint
  `strictTypeChecked` + `stylisticTypeChecked` config (`any` = ERROR; `no-floating-promises`,
  `no-unsafe-*`, `explicit-module-boundary-types`, `consistent-type-imports`,
  `no-non-null-assertion`, `import/no-cycle` to mechanically enforce the acyclic deep-module
  graph) and the tsconfig (`strict` + `noUncheckedIndexedAccess` + `exactOptionalPropertyTypes`
  + `verbatimModuleSyntax` + `isolatedModules`); record the composability rules — small pure
  functions, narrow per-module exports, dependencies injected as callbacks (the ArrayOpEngine
  pattern), no side-effecting module top-level, no class where a function suffices.
- **Reliable hooks (Directive 16):** the old `commit-requires-relevant-tests` +
  `merge-requires-all-tests` rules are KILLED (they fired at the wrong time + corrupted
  context); NEW tested replacements (precise trigger, silent-on-success, output to a
  transcript not the context, each with a passing firing-test); every gate writes a SHA-bound
  exit-code+stdout transcript; a hook that did not fire is itself a red gate (a meta-check
  asserts the transcript exists for the commit SHA); the `oracle-frozen-assertion-guard`
  PreCommit hook is wired (FM8).
- **Namespace map:** add the explicit DSL-entry-vs-internal map (§E) and **forbid
  `InternalsVisibleTo`**.
- **Render-perf section:** BenchmarkDotNet over `Render()`, the allocation budget, the
  source-gen-vs-manual-converter decision recorded.
- **Test law (Directive 12):** tests change ONLY on user-visible behavior; plan-shape
  assertions live in C# domain tests, never Playwright; the TS test approach is
  Matt-Pocock-style pure-behavior TDD (Directive 17).
- **Layer model + per-module CLAUDE.md (Directive 15):** the 12 per-module folder+namespace
  homes; **each of the 12 module folders carries its OWN `CLAUDE.md`** with
  module-specific context (its laws, its Kind union, its seam, its named fixtures), in
  ADDITION to this deepened root CLAUDE.md.
- Keep "**source is the requirement; docs are historical**" front-and-center: the
  determinism certificate (07) is **stale vs code** (it mentions `WriteOnlyPolymorphicConverter`
  once, asserting ×18; code has ONE generic `PlanNodeDiscriminator<T>` converter applied
  via 18 `[JsonConverter(typeof(PlanNodeDiscriminator<...>))]` registrations — zero manual
  per-type converters; `grep -rn "JsonConverter(typeof(PlanNodeDiscriminator<" --include=*.cs`
  → 18). Re-verify every count against source before acting.

---

## G. ARTIFACTS, COMMANDS & EXECUTION (so a cold session runs unaided to 1.0.0)

### Phase-ordered execution plan
1. **Phase A — Language + defaults.** Read `03-naming.md`; produce the final naming sheet
   for all 12 modules; pass THE NAMING TEST on every load-bearing name; resolve the
   Directive 4/5 grammar+name improvements AND decide the good DEFAULTS (Directive 3) —
   deliberately, up front, as Phase A artifacts (Directive 2). Deepen the root CLAUDE.md
   and stub the 12 per-module CLAUDE.md files (§F, Directive 15).
2. **Phase B — Deep modules + seams.** Read `02-micro-modules.md` + `00/01`; finalize
   the 12-module cut, the acyclic graph, the project layout (§E), the per-module
   §File-Layout; record each public seam signature for G-SURFACE. No code.
3. **Phase C — Math (the proof tool).** Read `05/06/07/08`; extend Shape's 5-artifact
   pattern to every module + every seam; produce a GREEN per-module certificate as the
   unlock token, **proved BY EXECUTABLE property/equivalence harnesses run against real
   linked source (the formalization in `08-determinism-formalization.md` is the proof
   tool — that is the "simulation that proves"; there is NO browser visualization in the
   proof).** The census denominator is MACHINE-DERIVED (G-MATH-100); add a composite-variant
   row per cross-module edge. **The math proof is authored by ARCHITECT personas and
   re-proven by BLIND developers — proof by those who do NOT see the implementation,** so
   the certificate is forced by the spec, never fitted to the code.
   *(Phase D — HTML simulators — is **CUT.** A browser drawing proves nothing; its only
   real STOP is already owned by G-MATH-100. Any human-legible diagram is an OPTIONAL doc
   auto-derived from the Phase C certificate + the Phase F green slice — never a gate.)*
4. **Phase E — Blind developers.** Run the dogfood blind-builder per module (spec +
   fixtures only, with a Crossjudge-inspected context-provenance manifest — FM16) + the
   blind BDD reviewer; harden the spec on every divergence. **This is the re-proof leg of
   Phase C: developers who never see the implementation rebuild from the math + spec
   alone, and zero divergence is the proof that the math holds independent of any one
   author.**
5. **Phase F — Code.** Per module, in dependency order, the mechanical loop below; all
   per-module gates (G1–G6, G-SURFACE, G-MATH-100, G-FRESH-CLONE, the behavior-oracle
   gate, G-ORACLE-COMPLETENESS, the transcript fence) green per commit; one closed matrix
   row per commit. Then onboard the 42 SF components (Directive 11). Then the fresh-clone
   final verification + NuGet pack → **1.0.0**.

### The mechanical loop per module (Phase F)
0. Confirm all dependency modules are green AND this module's Phase C certificate is
   green. 1. Write the pass-protocol row: `Close matrix row: <DSL call> -> <domain term>
   -> <runtime>`. 2. Paste the named fixtures FIRST (red — that red IS the spec).
3. Create files at the exact §File-Layout paths. 4. Paste the compile-ready skeleton.
5. Fill each TODO with the obvious body the §Input→Output rule dictates — **no design
   decision lives in a fill**; if you reach for one, STOP, read DSL source, harden the
   spec + add one named fixture, resume. **Code is the mechanical part ONLY: a HARD fill
   is a DEFECT REPORT against the proof — kick the module back to Phase C, do not push
   through. Hard code = unfinished proof.** 6. Drift-check + typecheck the moment the C#
   plan shape changes. 7. Run G1-G6 green; commit the one closed row.

### Commands (green-field root)
```bash
# First run (fresh clone / worktree)
npm ci
npm run build:all                                 # MUST finish before the sandbox starts
dotnet run --project Alis.Reactive.SandboxApp     # → http://localhost:5220

# Daily dev loop (3 terminals): watch:runtime · watch:design-system · dotnet watch

# Static checks & tests
npm run typecheck     # G3 — drift detector + tsc --noEmit (NO generate step)
npm run lint
npm test              # G4 — vitest both workspaces
dotnet build          # G1 — BOTH net48 + net10
dotnet test           # G2 — C# unit suite

# Playwright (after npm run build:all + a FULL dotnet build):
dotnet build tests/Alis.Reactive.PlaywrightTests
dotnet test  tests/Alis.Reactive.PlaywrightTests --logger "console;verbosity=detailed"
# first run only:
pwsh tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium

# Pack NuGet (does NOT invoke npm)
npm run build:all
dotnet build --configuration Release
dotnet pack <core>.csproj --configuration Release --no-build --output ./nupkgs -p:Version=1.0.0
```

### Where everything lives (in the existing repo, to read/extend; rebuild green-field)
- Spine artifacts: `docs/design/redesign/` (00-08, scaffold/, dogfood/). The `playground/`
  tree is **retired** (Phase D cut).
- Blueprint/specs/playbook/fixtures: `docs/design/redesign/02-micro-modules.md`,
  `docs/design/redesign/03-naming.md`, `docs/design/redesign/scaffold/{_playbook,_fixtures,<Module>}.md`.
- Math (the PROOF tool): `docs/design/redesign/05/06/07/08-*.md`,
  `docs/design/redesign/dogfood/` — `08-determinism-formalization.md` + the executable
  property/equivalence harnesses ARE the simulation that proves.
- ~~Simulator~~ (CUT — Phase D removed): `docs/design/redesign/playground/{index.html,design-graph.js}`
  is a retired, OPTIONAL human-legible diagram — never a gate, never a STOP authority,
  never on the unlock path. (`governance-simulation.html`'s dashboard misled — 0 gaps
  shown vs 15 real escapes — and is not a control.)
- Behavior oracle to preserve: `tests/Alis.Reactive.PlaywrightTests/` (1168/133),
  `Alis.Reactive.Assets/runtime/__tests__/` (192/28).
- RC1 reference source (the differential oracle): the current tree under
  `Alis.Reactive/`, `Alis.Reactive.Fusion/`, `Alis.Reactive.Native/`,
  `Alis.Reactive.Assets/runtime/`.
- Skills to co-locate + extend (Directive 10): `.claude/skills/onboard-fusion-component/`
  + the global category skills (move in-repo); add the Matt-Pocock-style pure-behavior
  TDD skill (Directive 17). The `frontend-design` / `playground` skills are **CUT** with
  Phase D — they are not on the critical path.

---

**Begin with the SPINE, in order — five phases, A → B → C → E → F.** Phase A: nail the
language, the names, and the good DEFAULTS up front. Phase B: cut the deep modules and
seams. Phase C: prove the math to a green certificate per module — the unlock token,
proved BY executable harnesses against real source (the math is the proof tool; Phase D /
HTML simulators is CUT). Phase E: let blind developers use the DSL cold. **Only then**
Phase F: build, module-by-module, dependency-ordered, every gate green per commit, against
the RC1 oracle as your fence. Onboard all 42 Syncfusion components. Verify in a fresh
clone. Pack and release **1.0.0**. You are never flying blind — RC1 is the truth, the
gates are the proof, the spec is the spec. Build it.
