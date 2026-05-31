# Determinism Confidence — Honest Coverage vs the 100% Claim

> ## ROUND-2 FINAL (2026-05-31) — consolidated verdict against the CORRECTED criterion
>
> **What is being certified (criterion correction).** This verdict certifies the
> **redesign blueprint's** determinism — the system the matrix files describe and we
> will build — **not** current shipped source. Current source carries known bugs the
> redesign deliberately fixes; "current source is non-deterministic" is true but
> irrelevant. The right test: does the redesign matrix specify exactly **one**
> deterministic output for every **live** public DSL variant, with each current
> non-determinism resolved by a **stated redesign rule**, and dead/unconsumed surface
> **excluded** from the denominator?
>
> Round-1 (below) answered the *wrong* question — it certified current source
> (`deterministic=false` because the magic-member collision is still shipped). Against
> the corrected criterion, that collision is a current-source bug correctly labeled
> `RESOLVED-BY-REDESIGN` (Fix 1, distinct node kinds), so it does **not** defeat the
> blueprint verdict.
>
> ### Re-verified against source (this round)
> - **Live-variant census = 375 / 375 = 100%.** The proof's own Section 6 per-area
>   table sums to **375** (`10+28+52+5+47+19+102+14+5+65+28`, confirmed by `bc`). Every
>   per-area sub-table is internally `covered==total` (Plugins `1+1+1+21+10+11+12+8=65`;
>   Validation `31+38+24+3+1+2+3=102` — both verified). The proof's headline read a stale
>   **371** at lines 36, 503, 505, 521, 534 — a carry-over not recomputed after the
>   per-area tables were revised upward (same class of slip as the round-1 trigger
>   `11→10` fix). **Fixed this round: 371 → 375 at all five lines.** Verdict unchanged
>   either way — every area is X/X, zero uncovered live variants.
> - **`redesignDeterministic = true`.** The single live current-source non-determinism
>   (the `responseBody`/`elementValue` magic-member collision) is still present in
>   shipped source — verified: `ValueExpression.cs:379-380,400-403`,
>   `plan.ts:783-799` (no `whole` field), `evaluate.ts:294-300` (discriminates on
>   `member ===`, ignores `path`) — and is closed by a **stated structural redesign
>   rule** (Fix 1: `kind:"whole-payload"`/`"whole-element"` distinct node kinds, member
>   string never emitted for a whole read → collision unrepresentable). Fixes 2-4 close
>   the supporting under-pinned defaults and the id-drift surface. No current
>   non-determinism is left without a stated resolving rule.
> - **Dead surface excluded, not invented.** `DrawerPosition`, `ToastType`,
>   `ToastPosition` — verified **zero `.cs` consumers** (`grep -rwn … --include=*.cs`
>   returns nothing outside each definition file). Deleted in the redesign, excluded
>   from the live denominator. Not counted as covered; not counted as gaps.
> - **Adversary: clean of any verdict-changing finding.** Round-1's five Finding-B
>   source contradictions are all fixed (B1 single-command always sequence-wrapped; B2
>   whole-payload node-kind agreement; B3 exact-status-preferred ordering; B4 inject
>   string-only boundary throw; B5 Confirm-evaluates-first). Round-2 surfaced only
>   non-verdict-changing items (below).
>
> ### Residual gaps (none changes the verdict — tracked for build accuracy)
> 1. **(FIXED this round) Headline count slip** 371 → 375. Cosmetic; zero coverage or
>    determinism impact.
> 2. **Citation-path imprecision in the proof's validation surface.** The proof cites
>    `ReactiveValidator.cs` / `ReactiveClientRuleBuilder.cs` without the project prefix;
>    they live in `Alis.Reactive.FluentValidator/`, and `ClientValidationFieldRuleBuilder.cs`
>    in `Alis.Reactive/Validation/`. Counts are exact and reproducible
>    (`ClientValidationFieldRuleBuilder` = 31 public methods / 32 public lines;
>    `ReactiveClientRuleBuilder` = 38 public static — both confirmed). The `WhenField*`
>    family (`ReactiveValidator.cs:103-181`) is **`protected`** (the subclass-authoring
>    surface), not public — a legitimate live surface, mislabeled in the proof as a
>    public-method tally. No coverage impact.
> 3. **Known missing-feature asymmetries (do not breach redesign-determinism).**
>    `WhenFieldMaxLength` absent while `WhenFieldMinLength` exists
>    (`ReactiveValidator.cs:162`); `SetHtml` has no `ResponseBody` overload while
>    `SetText` does (`ElementBuilder.cs:76` vs `:96-120`); no bare `p.Put(url)` pipeline
>    entry today (`PipelineBuilder.Http.cs:11-42`). Each is a per-overload row or an
>    explicit labeled redesign target — none collapses two inputs to one output, so none
>    breaches the corrected criterion. Tracked so the build does not assume symmetry.
> 4. **Fix 1 (and the bare `Put(url)` entry, inline-gather for every body verb, plugin
>    arity collapse-to-args-builder) are REDESIGN TARGETS not yet built in source.** The
>    proof labels each `RESOLVED-BY-REDESIGN` / "labeled redesign". The determinism
>    certification is sound as a **spec**; it becomes true-in-source only after Fix 1
>    ships in `ValueExpression.cs` + `plan.ts` + `evaluate.ts`.
>
> **VERDICT (round 2, against the corrected criterion):** `liveVariants=375`,
> `covered=375`, `realCoveragePct=100`, `redesignDeterministic=true`,
> `realConfidence=true`. The redesign blueprint specifies exactly one deterministic
> output for every live public DSL variant; every current non-determinism is resolved
> by a stated redesign rule; dead surface is excluded; the adversary found nothing that
> changes the verdict. Confidence is in the **blueprint as a generator spec** — it
> becomes true-in-shipped-source the moment Fix 1 lands.
>
> ---

> ## ROUND-1 RE-VERIFY (2026-05-31) — recomputed after the matrix/proof fixes
>
> The matrix files (`04-matrix-*.md`) and `05-determinism-proof.md` were revised
> after this document's first pass. This section is an **independent re-census +
> adversary re-run** against the revised docs AND the actual source. It supersedes
> the per-band numbers below where they differ; the older analysis is kept for
> provenance.
>
> **Re-verified headline (per-overload authoring variant): 371 / 374 = 99.2%.**
> Independently confirmed against source. The only **3** uncovered authoring
> variants are dead public enums with **zero `.cs` consumers** (verified by
> `find … -name '*.cs' | xargs grep -wn`, empty result outside their own
> definition files):
> - `DrawerPosition` (`Alis.Reactive.Native/AppLevel/NativeDrawer/DrawerPosition.cs:6`)
> - `ToastType` (`Alis.Reactive.Fusion/AppLevel/FusionToast/ToastType.cs:6`)
> - `ToastPosition` (`Alis.Reactive.Fusion/AppLevel/FusionToast/ToastPosition.cs:6`)
>
> Toast type methods are **parameterless** (`FusionToastExtensions.cs:68-86`:
> `Success() => EmitSet(CssClassProperty, Literal("e-toast-success"))`), so the
> enums have no authoring producer. The redesign **deletes** them; correctly
> excluded from the 371 covered. There are **no** uncovered authoring variants
> with a live producer.
>
> **Determinism = FALSE (current source). realConfidence = FALSE.** Re-verified
> root cause, unchanged from the first pass and still **unfixed in source**:
>
> - **Finding A — live `responseBody`/`elementValue` magic-member collision.**
>   `ValueExpression.cs:379-380` (`WholePayloadMember="responseBody"`,
>   `WholeElementMember="elementValue"`) and `:399-403` stamp those members with
>   `Path.None`. `plan.ts:783-799` ships the sentinel with **no `whole` field**.
>   `evaluate.ts:294-300` (`readsWholePayload`/`readsWholeElement`) discriminates
>   **only** on `member === "responseBody"`/`"elementValue"`, **ignoring `path`**.
>   `ExpressionPathHelper.cs:272-276` (`CamelCase`) maps `"ResponseBody"` →
>   `"responseBody"` exactly, so a DSL path read of a property literally named
>   `ResponseBody` collapses to the whole-payload read — **two distinct DSL inputs
>   → one wire member → one runtime behavior**. No analyzer/guard exists. The
>   redesign target (distinct `kind:"whole-payload"`/`"whole-element"` nodes) IS
>   deterministic, but the collision is **present in shipped source**. This single
>   live many-to-one collision forbids `deterministic=true` and
>   `realConfidence=true` for current source.
>
> **Adversary re-run: the 5 Finding-B contradictions are now FIXED and source-grounded.**
> - **B1** single-command sequence-wrap — `04-matrix-triggers-…:136` now states
>   the reaction is *always* sequence-wrapped, not a bare node
>   (`ReactionPipelineDraft.cs:52-58,82-88`). ✔
> - **B2** inject whole-payload — both matrix files now agree on the redesign
>   `whole-payload` node kind; the triggers file explicitly says "not a
>   `whole:true` boolean" (`04-matrix-triggers-…:219,230`;
>   `04-matrix-http-…:93,244`). No `whole:true` asserted as shipped. ✔
> - **B3** OnError — `04-matrix-http-…:212` now reads "exact-status-preferred,
>   then first any-status — NOT positional first-match" and notes the prior
>   wording was wrong (`http.ts:263`). ✔
> - **B4** inject string-only boundary throw — documented at
>   `04-matrix-http-…:244` (`execute.ts:207-218`). ✔
> - **B5** Confirm evaluates first — `04-matrix-triggers-…:381` corrects the
>   impossible short-circuit (`GuardBuilder.cs:81-85`; `conditions.ts:62-72`). ✔
> - **Band E hallucination cured.** No `p.Drawer()/p.Toast(ToastType,…)` verbs
>   asserted; access is `Component<T>()` + ComponentRef extensions. No remaining
>   hallucinated/unverifiable row found.
>
> **Residual gaps (do NOT create uncovered variants; do block a clean 100%):**
> 1. **Live collision (Finding A) unfixed in source** — the sole blocker for
>    `deterministic`/`realConfidence`.
> 2. **`Shape.FromValue` matrix row under-documents the arbitrary-value path.**
>    `04-matrix-http-…:77` ("Literal — arbitrary value") summarizes only
>    "enum/Guid → string; collection → array; else any". Source `Shape.cs:74-118`
>    also yields `DateTime/DateTimeOffset/DateOnly → Date` (`:80`),
>    `Guid/TimeSpan/TimeOnly → String` (`:82`), `Nullable<T> → Nullable(inner)`
>    (`:74-76`). The complete table lives only in `05` Fix 2. Narrow: the typed
>    "Literal — scalar" row (`:75`) does cover `DateTime → Date`, so egress is
>    correct for the typed path; only the arbitrary-value (boxed `object`) row is
>    incomplete in the matrix.
> 3. **Cosmetic count slip (no coverage impact).** `04-matrix-triggers-…:393`
>    band header labels the trigger sub-band "11" while its enumerated list has 10
>    items; the proof per-area table (`05:321`) correctly says 10/10 (source: 8
>    public `TriggerBuilder` methods + component-event seam + multiple-triggers).
>    Recommend changing 11 → 10.
> 4. **Hand-tally noise in per-area counts (no coverage impact).** Independent
>    recount: `ClientValidationFieldRuleBuilder` = 31 public methods (proof A1
>    says 27); these map to existing RuleName+operand rows, so coverage is
>    unaffected. The 371/374 headline is reproducible because validation/plugins
>    are counted by rowed families, not raw method count.
>
> **Verdict (round 1):** `totalVariants=374`, `coveredVariants=371`,
> `realCoveragePct=99.2`, `deterministic=false`, `realConfidence=false`. The
> matrix/proof are now an honest, source-grounded, near-complete per-overload
> generator spec (99.2%) that correctly labels every current-vs-redesign delta.
> `realConfidence` stays **false** for exactly one reason: the redesign's
> headline determinism fix (Fix 1, the whole-payload/whole-element node kinds) is
> **not yet built in source**, so the live magic-member collision still exists.
>
> ---

> **Purpose.** `05-determinism-proof.md` asserts **120/120 = 100% deterministic
> coverage of the public DSL, 0 non-deterministic, 0 gaps**. This document does
> not trust that headline. It consolidates a code-grounded variant census (every
> overload read from source with file:line) and three independent adversary
> passes, re-opens the cited source to verify the load-bearing claims, and
> computes the **real** coverage.
>
> **Verdict.** `realConfidence = false`. The 100% claim is **not earned against
> current source**. It is earned only against an **unbuilt redesign** that the
> design docs themselves describe in future tense ("the redesign closes…",
> "Redesign output:"). Three concrete defects remain in the source the proof
> claims to verify against:
>
> 1. **Live non-determinism still in source** — two distinct DSL inputs collapse
>    to one wire member and one runtime behavior (`responseBody`/`elementValue`
>    magic-member collision). This alone forbids any 100%/deterministic claim.
> 2. **Matrix rows that contradict source** — the proof counts rows as "covered"
>    whose asserted wire shape or behavior the source does not produce
>    (`whole:true` that does not exist; "no sequence wrapper for one node" that is
>    always wrapped; "first match wins" that is actually exact-status-preferred).
> 3. **Real coverage of *deterministic rows* is ~92.6%, not 100%** — 21 distinct
>    source variants have no dedicated matrix row (folded under generic axes).
>
> All three adversaries returned **not clean**. Therefore: `deterministic =
> false`, `realConfidence = false`.

---

## How the real number is computed

The census enumerated the public DSL surface from source as discrete *variants*
(one per overload / token / node factory) and marked each as covered (a dedicated
deterministic matrix row exists, named by file:line) or uncovered (no dedicated
row — the variant is only implicitly subsumed under a generic axis). The
adversary passes then checked whether the rows that ARE present actually match
source.

Two different numbers result, and they must not be conflated:

- **`05-determinism-proof.md` counts 120 "named public feature families."** That
  is a coarse band-level rollup (e.g. "set text from value source" is one cell
  even though source has 4 SetText overloads with materially different lowerings).
  At that granularity it can call itself 120/120.
- **The variant census counts 284 discrete source variants.** At the granularity a
  code generator must emit (each overload is a distinct method with a distinct
  lowering), **263 are covered by a deterministic row and 21 are not** → **92.61%**.

The honest figure is the variant figure. A generator driven off the matrix emits
per-variant code; band-level rollups hide the overloads it must still write.

### Per-area variant counts (covered / total, with file:line anchors)

| Area | Covered | Total | Real % | Uncovered variants (file:line) |
|---|---|---|---|---|
| Triggers | 10 | 10 | 100.0% | — |
| Conditions | 23 | 23 | 100.0% | — |
| Values + Arrays | 32 | 33 | 97.0% | element-scope per-element method read (`InvokeElement`) + whitelist gate — `ValueExpression.cs:108-112`; `ElementExpressionCompiler.cs:140-154` |
| Validation | 71 | 75 | 94.7% | two `ClientRulesFrom` variants (`ReactiveValidator.cs:80,92`); `ClientRuleEach.SetValidator`/`.AtLeastOne` (`ReactiveClientRuleBuilder.cs:60,67`); `ReactiveClientRules` server+client paired surface incl. nullable-struct overloads (`ReactiveClientRuleBuilder.cs:101,117,149,308`); `EqualTo`/`NotEqual` literal unconstrained vs constrained peer (`ClientValidationFieldRuleBuilder.cs:95,108`) |
| HTTP | 44 | 47 | 93.6% | `AsFormData` body-format row (`HttpRequestBuilder.cs:65`); `Put(url, gather)` inline-gather verb (`PipelineBuilder.Http.cs:31`); `Include(refId,name)` for DISPLAY components (`GatherExtensions.cs:36`) |
| Reactions | 58 | 65 | 89.2% | `Component<T>` app-level (`PipelineBuilder.cs:136`); `Component<T,TOther>` cross-model (`:114`); `SetText(ResponseBody,path)` (`ElementBuilder.cs:76`); `SetText/SetHtml(source,path)` event-payload (`ElementBuilder.cs:65,106`); `Put(url,gather)` (`PipelineBuilder.Http.cs:31`); `DispatchPayloadBuilder` typed-literal Set ×3 (`DispatchPayloadBuilder.cs:43,56,69`); nested-path dispatch + conflict throw (`DispatchPayloadBuilder.cs:88,118,142`) |
| Components | 14 | 17 | 82.4% | dead public enums `DrawerPosition`/`ToastType`/`ToastPosition` (`DrawerPosition.cs:6`, `ToastType.cs:6`, `ToastPosition.cs:6`); per-slice `.Reactive()` signature variance incl. 2 overloads on `FusionSmartTextArea` (`FusionSmartTextAreaReactiveExtensions.cs:11,22`); slices with NO `.Reactive` (`FusionButton`, `FusionSmartPasteButton`, `NativeActionLink`) |
| Slots + Plugins | 11 | 14 | 78.6% | `RegisterPlugin(ReactivePlugin)`/`RegisterPlugin<T>()` (`ReactivePlan.cs:66,73`); plugin Property read-only + name-collision invariant (`PluginTypeBuilder.cs:27`; `PluginContract.cs:189,246`); ~56 arity declaration overloads (`ReactivePlugin.cs:61-132`; `PluginTypeBuilder.cs:59-216`) |
| **TOTAL** | **263** | **284** | **92.61%** | 21 uncovered |

**Real coverage = 263 / 284 = 92.61% of discrete source variants.** Not 100%.

> Note the census's own per-area `covered` field is a smaller hand-tally than the
> feature `variantCount` sum in two areas (it counts rolled cells, not every
> overload). The table above uses the census's reported `covered` counts and its
> explicit `uncovered` lists, which is the conservative reading. Even on the more
> generous band rollup, the three adversary defects below still force
> `deterministic = false`.

---

## Adversary findings (all three passes returned NOT clean)

Each finding below was re-verified by re-opening the cited source for this
document.

### A. LIVE NON-DETERMINISM still present in source — disqualifies any 100% claim

**`responseBody` / `elementValue` magic-member collision** — verified.

- Whole-payload and whole-element reads are encoded as the magic member strings
  `member:"responseBody"` and `member:"elementValue"`, `path` forced to `Path.None`
  (`Alis.Reactive/PlanModel/ValueExpression.cs:379-380,399-403`). Confirmed: the
  consts `WholePayloadMember="responseBody"` / `WholeElementMember="elementValue"`
  are exactly there.
- The runtime discriminator checks **only** `expression.member === "responseBody"` /
  `"elementValue"` and **ignores `path`**, returning the entire root unwalked
  (`runtime/core/evaluate.ts:287-300`). Confirmed.
- A legal public-DSL path read of a response/event/element property literally named
  `ResponseBody` camelCases to exactly `responseBody`
  (`ExpressionPathHelper.CamelCase`, `ExpressionPathHelper.cs:272-276`;
  `PayloadTypedSource.ToValueExpression` → `ReadPayload(_source, payloadPath, Shape)`
  at `PayloadTypedSource.cs:28-32`). Confirmed: `ToEventPath` returns the bare
  camelCase path and that becomes the `member`.
- The generated TS contract still ships the sentinel:
  `WholePayloadReadExpression { member: "responseBody" }` /
  `WholeElementReadExpression { member: "elementValue" }`
  (`runtime/types/plan.ts:783-795`). Confirmed. There is **no `whole` field**.

**Consequence:** `p.When(success, x => x.ResponseBody).Eq(...)`,
`s.Element("e").SetText(json, r => r.ResponseBody)`, and
`arr.Where(x => x.ElementValue == ...)` each produce a node the runtime treats as a
whole-payload/whole-element read — **two distinct DSL inputs collapse to one wire
member and one runtime behavior**, and the field read silently returns the whole
object instead of the `.ResponseBody` sub-field. No analyzer or build-time guard
rejects this (grep finds only the const definitions). This is a many-to-one input
collision = genuine non-determinism, present in the source the proof verifies
against. **This single fact forbids `deterministic=true` and `realConfidence=true`.**

### B. Matrix rows that CONTRADICT source (counted as "covered" but wrong)

These rows exist, so the band rollup calls them covered — but their asserted output
or behavior is not what source produces. A generator copying them emits wrong code.

1. **"Single command → no sequence wrapper for one node"** —
   `04-matrix-triggers-reactions-conditions.md:132` says `BuildReaction` "returns it
   directly (no wrapper)" and the output is "the bare node." Source:
   `FlushPendingSyncReactions` (`ReactionPipelineDraft.cs:82-88`) **unconditionally**
   wraps any pending sync reactions in `ReactionGraph.Sequence(...)` (only guard is
   `Count==0`), so `_orderedBlocks=[Sequence([node])]` and `BuildReaction`
   (`:52-58`) returns `_orderedBlocks[0]` = the Sequence. Actual output is
   `{"kind":"sequence","steps":[{node}]}`, not the bare node. **The most basic
   reaction row mis-states the wire shape.** Verified.

2. **inject output `whole:true`** —
   `04-matrix-triggers-reactions-conditions.md:185` asserts
   `value:{"kind":"read","from":{…"scope":"success"},"whole":true}` as **current
   fact (no redesign qualifier)**. No `whole` field exists in source; `Into` emits
   `ReadWholePayload(Success)` → `member:"responseBody"` (`ValueExpression.cs:379`,
   `plan.ts:783-786`). The sibling file
   `04-matrix-http-arrays-values.md:93-94,106-107` correctly prefixes the **same**
   shape with **"Redesign output:"** and says the sentinel "**become[s]** an explicit
   variant" — i.e. future tense. **The two matrix files disagree, and the triggers
   file asserts an unbuilt shape as shipped.** Verified.

3. **OnError "first match wins"** — `04-matrix-http-arrays-values.md:180`
   Good-default says "first match wins," and the row narrative says "exact-status
   route preferred, else any" — internally contradictory. Source
   `routeResponseRoutes` (`runtime/execution/http.ts:263`) is
   `routes.find(exactStatus) ?? routes.find(anyStatus)` = **exact-status-preferred,
   then first any**, NOT positional first-match. An any-status route authored
   *before* `OnError(404,…)` still loses to the 404 route on a 404. **"First match
   wins" would mislead a generator about ordering.** Verified.

4. **inject value-type boundary throw omitted** — the inject row frames the verb as
   fully deterministic sync with no error path. `executeInject`
   (`runtime/execution/execute.ts:207-218`) throws
   `[alis] inject expects string HTML, got <type>` when the evaluated value is not a
   string. Since the value is fixed to `ReadWholePayload(Success)` and a JSON success
   body parses to an object, an inject of an `application/json` body **throws at
   runtime** — a behavioral nuance the matrix glosses. Verified.

5. **Confirm `.And(...)` "compares short-circuit first" is impossible** —
   `04-matrix-triggers-reactions-conditions.md:297` good-default says the compare
   "may avoid the dialog (short-circuit)." `GuardBuilder.And` composes
   `All(ConditionGraph)` with the existing confirm flattened **first** →
   terms `[confirm, compare]` (`GuardBuilder.cs:81-85`), and `evaluateAllInLane`
   (`runtime/conditions/conditions.ts:62-72`) iterates left-to-right from index 0,
   so confirm always opens the dialog before the compare runs. Lowering is
   deterministic, but the documented behavior cannot happen. Verified.

### C. Distinct source overloads with no dedicated matrix row (subsumed under generic axes)

These are the 21 census `uncovered` variants (per-area table above). Highest-impact:

- **`SetText` overload set (4) vs `SetHtml` (3) asymmetry** — `SetText` has a
  `ResponseBody<TResponse>` overload (`ElementBuilder.cs:76`) reading a success/error
  body by path; `SetHtml` has **no** such overload (`ElementBuilder.cs:96-120`,
  by-path SetHtml hardcodes `PayloadSource.Event()` at :109). The matrix treats both
  uniformly (rows 145-147), hiding a real per-overload capability gap.
- **HTTP verb asymmetry** — inline-gather sugar exists for `Post`/`Put` only; there
  is **no bare `Put(url)`** pipeline entry (`PipelineBuilder.Http.cs:11-42`). Matrix
  B.1 presents a clean 4-way `GET·POST·PUT·DELETE` symmetry and only names
  `p.Post(url, g=>…)` for inline gather (line 130). PUT-with-inline-gather is unrowed.
- **`OnError<TError>(int statusCode, …)` 4th overload** (`ResponseBuilder.cs:96`) —
  typed body × exact status; matrix B.5 enumerates only 3 forms (line 180).
- **`GatherExtensions.Include(refId, name)` for DISPLAY components**
  (`GatherExtensions.cs:36-50`) — reads a named (non-`ValueMember`) property; matrix
  B.3 rows assume input-value-member semantics only.
- **`AsFormData`** (`HttpRequestBuilder.cs:65`) — only a prose "Body format axis"
  note (lines 160-172), no authoring→`bodyFormat:"formdata"` row.
- **`Sum(int)/Sum(decimal)/Sum(double)`** (`ReactiveArray.cs:90-99`) — 3 CLR-distinct
  overloads counted as one op (matrix line 240).
- **Plugin registration verbs** `RegisterPlugin(ReactivePlugin)` / `<T>()`
  (`ReactivePlan.cs:66,73`) — Band D walks only a future unified `: Plugin` subclass.
- **~56 plugin arity declaration overloads** (`ReactivePlugin.cs:61-132`,
  `PluginTypeBuilder.cs:59-216`) — Band D explicitly collapses these into a
  *future* args-builder, i.e. treats unbuilt unification as already done.
- **Dead public enums** `DrawerPosition`, `ToastType`, `ToastPosition` — zero
  consumers (`DrawerPosition.cs:6`, `ToastType.cs:6`, `ToastPosition.cs:6`); Band E
  asserts a `p.Toast().Show("Saved", ToastType.Success, ToastPosition.TopRight)`
  signature that **does not exist in source** (FusionToast exposes parameterless
  `.Success()/.Warning()/.Danger()/.Info()`).
- **Band E DSL verbs do not exist** — `p.Drawer()`, `p.Loader()`, `p.Toast()`,
  `p.Confirm().SetContent(...)` (`04-matrix-validation-components-slots.md:298-301`)
  have no `PipelineBuilder` counterpart; actual access is
  `p.Component<NativeDrawer>()` + ComponentRef extensions. The matrix input column is
  aspirational here — **cannot be walked from current source**.

### D. Census-level deterministic concerns (deterministic lowering, under-pinned matrix)

Recorded but not re-counted as gaps; they erode confidence in the "good default"
column rather than the output shape:

- ArrayContains operand is untyped `object` with possibly-`None` `ElementShape`;
  matrix does not pin the `itemShape=None` case (`ConditionSourceBuilder.cs:105-108`).
- Source-vs-source overloads exist only for the 6 equality/ordered operators
  (`ConditionSourceBuilder.cs:112-128`); the matrix's "+1 source-vs-source form"
  reads as universal across all families.
- Nested And/Or flatten is single-level and depends on whether the inner top node
  matches the outer kind; matrix says "flattened into one all/any" (overstated for
  mixed-operator case).
- `set × plugin`, `dom`-vs-`component` element write, payload scope `local` — wire
  enum members with no public producer, parked as undecided
  (`04-matrix-triggers-reactions-conditions.md:356-389`).
- Validation: 18 C# RuleName tokens narrow to ~6 wire families "nowhere in C#" until
  a generator owns it (`ValidationTerms.cs:118-135`; matrix :348-356); `ClientRule`
  under server `When/Unless` enforced by a runtime throw, not the type system
  (`ReactiveValidator.cs:263-272`); no `WhenFieldMaxLength` though `WhenFieldMinLength`
  exists; two distinct `ArrayContains` authoring entry points share one matrix op.
- App-level fixed ids duplicated C#-const-vs-TS-module, not yet one shared constant
  (matrix's own Determinism Hole #2).
- `Literal — arbitrary value` row's `Shape.FromValue` summary is materially
  incomplete: `DateTime/DateOnly→Date`, `TimeSpan/TimeOnly/Guid→String`,
  `Nullable<T>→Nullable(inner)` are reachable and unlisted (`Shape.cs:70-118`).
- Array→JSON body egress shapes items **only when `itemShape.isDeclared`**; undeclared
  item arrays bypass `formatForWire` (`request-payload-writer.ts:221-225`),
  contradicting the universal "shape-once on egress" assertion.

---

## Honest coverage and the explicit gap list

- **Real variant coverage: 263 / 284 = 92.61%** of discrete source variants have a
  deterministic dedicated matrix row.
- **Deterministic? NO.** A live many-to-one input collision (Finding A) remains in
  source and the generated TS contract.
- **realConfidence? NO.** 100% requires coverage = 100% AND all adversaries clean
  AND no non-determinism. None of the three hold.

### Gaps that MUST be fixed before any 100% claim

1. **Close the `responseBody`/`elementValue` collision in C# (Finding A).** Replace
   the magic-member sentinel with the explicit `whole:true` variant the matrix
   already documents — in `ValueExpression.cs:379-403` AND `plan.ts:783-795` AND
   the runtime discriminator `evaluate.ts:287-300` — or add a build-time analyzer
   rejecting `member ∈ {responseBody, elementValue}` from a non-whole read. Until the
   `whole` field actually ships, the proof describes an unbuilt design.
2. **Fix the single-command row (Finding B1).** Either change the matrix to state the
   single command IS sequence-wrapped (`{"kind":"sequence","steps":[node]}`), or change
   `FlushPendingSyncReactions` to not wrap a single reaction. Today the row is false.
3. **Reconcile the two `whole`-payload rows (Finding B2).** The triggers matrix must
   carry the same "Redesign output:" qualifier the HTTP matrix uses, OR ship the
   `whole:true` field. They currently disagree.
4. **Fix the OnError ordering language (Finding B3).** Replace "first match wins" with
   "exact-status preferred, then first any-status" to match `http.ts:263`.
5. **Document the inject string-only boundary throw (Finding B4)** and the impossible
   Confirm short-circuit good-default (Finding B5).
6. **Add the 21 missing per-variant rows (Section C)** — at minimum the SetText
   ResponseBody overload, the SetText/SetHtml asymmetry, `Put(url,gather)`, the 4th
   `OnError` overload, `Include(refId,name)` for display components, `AsFormData`, the
   numeric `Sum` overloads, plugin registration verbs, and the FromDom typed overload.
7. **Remove or row the dead/aspirational Band E surface (Section C, Finding C)** — the
   `p.Drawer()/p.Loader()/p.Toast(...)` verbs and `ToastType/ToastPosition` enums named
   in the matrix do not exist in source; either build them or delete the rows.
8. **Resolve the census-level under-pinned defaults (Section D)** before claiming each
   row's "good default" is validated against runtime order.

Until items 1-5 are closed, the only honest statement is: **the matrix is a strong,
mostly-deterministic spec for ~92.6% of source variants, describing a redesign that
is partly unbuilt; it is not yet a verified 100% generator spec.**
