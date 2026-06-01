# Grammar-Hardening Review (skeptical language-architect pass)

> Review of `11-dsl-grammar-hardening.md` against the source critiques
> (`grammar-critique-*.md`), the AST grammar tables (`ast-grammar-*.md`), and the
> finalized names in `09-dsl-naming-sheet.md`. For each adjustment, the four tests are:
> **(a)** is the cited current shape really in the ast-grammar tables?
> **(b)** does the AFTER genuinely improve tall-reading / easy-to-write, not churn?
> **(c)** is EVERY capability preserved (zero feature loss)?
> **(d)** consistent with the finalized names in `09`?
>
> Method: every AST citation below was checked against the actual grammar table row; every
> "decided in 09" claim was checked against the actual `09` row (the §4a table). Findings are
> ranked: blocking citation errors first, then framing nits, then the confirmed-sound bulk.

---

## Verdict summary

`11` is a strong, well-grounded distillation. 35+ of the 39 adjustments cite a **real** AST row,
propose an AFTER that is a genuine tall-reading/orthogonality win, and preserve every capability
with an explicit survivor note. The unifying principles (P1–P7) are sound and each adjustment is a
true instance of one.

**Two §4a citations are misattributed** — they claim a rename is "decided in `09`" when `09`
actually marks the same row **KEEP / unchanged**. The *shape* critiques behind them are real, but
`11` overstates their settlement status, which would mislead implementation into treating an
un-ratified rename as locked. These are **fixable by re-labeling**, not by dropping the
adjustment. No capability is dropped anywhere. No churn-for-novelty found in the ranked tiers.

**VERDICT: REVISE** (two citation-grounding fixes in §4a; everything else SOUND).

---

## BLOCKING — ungrounded "decided in 09" citations (§4a)

### B1. Adj #24 — `FocusOut → Blur` is NOT decided in `09` (only `FocusIn → Focus` is)

- **§4a claims:** "Adj 24 … `09:344,451` — `FocusIn`→`Focus`, `FocusOut`→`Blur` decided."
- **What `09` actually locks:** `09:344` renames **only** `FocusIn → Focus`. `09:451`'s rename
  digest lists **only** `FocusIn → Focus` — there is **no** `FocusOut → Blur` line anywhere in
  `09` (verified: `grep Blur 09-dsl-naming-sheet.md` returns nothing; `FocusOut` is absent too).
- **Where `Blur` actually comes from:** the components critique Adj 3
  (`grammar-critique-components-reactive.md:239-243`) *proposes* `FocusOut → Blur` and is explicit
  that it is a NEW name: *"the AST correctly records that no `Blur` mutation exists today."* The
  AST table agrees — `ast-grammar-element-component.md:177-182` says **"`ComponentRef.Blur()` —
  no `Blur` mutation method exists"**; the existing focus-loss mutation is `FocusOut`.
- **(a) grounded?** Half. `FocusIn`/`FocusOut` ARE in source (`ast-grammar-element-component.md:111-125`).
  `Blur` is NOT — and `09` did not coin it.
- **(c) capability preserved?** Yes — `FocusOut → Blur` is a rename of the existing focus-loss
  call, not a new capability, so no feature loss. (The critique's "no Blur exists today" refers to
  the *target name*, not a missing behavior.)
- **(d) consistent with 09?** **No.** `11` reports `Blur` as a settled `09` decision; it is an
  open critique proposal. **Fix:** move the `FocusOut → Blur` half out of the §4a "already settled
  in 09" table and into §4 as a *pending* rename, or ratify it in `09` first. Keep `FocusIn → Focus`
  in §4a (that half is correctly cited).

### B2. Adj #23 — `ComponentRef.SetText → SetDisplayText` is NOT decided in `09` (09 marks it KEEP)

- **§4a claims:** "Adj 23 … `09:343` — `SetText`/`SetHtml` kept for Element lane; ComponentRef
  display verb renamed."
- **What `09:343` actually says:** `ElementBuilder` `SetText`/`SetHtml` → **unchanged, KEEP**.
  And `09:345` marks `.SetValue`/`.Value` → **unchanged, KEEP**. There is **no**
  `SetText → SetDisplayText` row in `09` (verified: `grep SetDisplayText 09-dsl-naming-sheet.md`
  returns nothing). So `09:343` is cited as authority for a rename it does not contain — it is the
  Element-lane *keep* row, not a ComponentRef *rename* row.
- **(a) grounded?** Yes, the collision is real: `ComponentRef<…>.SetText` (sets displayed text,
  `ast-grammar-element-component.md:90-91`) genuinely collides by name with
  `ElementBuilder.SetText` (sets DOM text, `:49`). The problem is real; the **citation of `09` as
  the deciding authority is wrong**.
- **(c) capability preserved?** Yes — rename only; both `SetText`(element) and the renamed
  `SetDisplayText`(component) behaviors survive.
- **(d) consistent with 09?** **No.** `SetDisplayText` is a critique proposal, not a `09` decision;
  `09` currently says KEEP for the colliding verb. **Fix:** label Adj #23 as a *pending grammar
  rename to be ratified in `09`*, not as "settled in `09`." (`09` may even need updating, since it
  currently KEEPs a name `11` wants to change.)

> Both B1 and B2 share one root: §4a's promise is "spelling already decided in the naming sheet —
> do not re-coin." Two rows in that table cite a `09` line that says KEEP, not the rename. The
> adjustments themselves are legitimate; only their **settlement status** is overstated. This is
> exactly the `feedback_verify_before_writing_plans.md` failure mode (a citation asserted without
> reading the cited line), so it is worth a hard flag even though the design intent is sound.

---

## Framing nit (non-blocking)

### N1. Adj #21 — `.Reactive` selector param "named `eventSelector` on 53, `on` on 3"

- **(a) grounded?** Substantially. The AST shows the `on`-named outliers are
  **FusionChipList** (`:136`), **FusionMention** (`:144`), and **FusionSmartTextArea**
  (`:163,164`). That is **3 component slices**, but SmartTextArea has **two** overloads, so the
  raw method count of `on`-named selectors is 4, and `eventSelector`-named is the rest. The "3
  vs 53" is a **slice** count, and 56 total slices is the figure used elsewhere in `11`. The
  count is defensible at slice granularity but the doc never says "slices"; a reader checking
  method-by-method will see 4 `on` signatures. **Minor** — recommend "3 slices (4 overloads)".
- **(b)/(c)/(d):** the rename to one name `on` is pure consistency, pure rename, zero feature
  loss, and consistent with `09:341`/`09:6` (one-concept-one-name). Sound.

---

## CONFIRMED SOUND — spot-verified against the AST tables and critiques

The following were checked row-by-row and pass all four tests. (Not exhaustive of all 39, but
covers every Tier-1 item plus the higher-risk renames and family completions.)

| Adj | (a) AST cite correct? | (b) real win, not churn | (c) zero feature loss | (d) 09-consistent |
|-----|----------------------|-------------------------|------------------------|-------------------|
| **#1** `Html.On→ReactivePlan.On` | YES — `entry-triggers:33` shows `On(...)→void`; threaded `plan` confirmed | YES — restores TALL at the entry edge | YES — free-function `Html.On(plan,…)` kept for partial injection (triggers critique A1:78) | n/a (shape, not name) |
| **#2** widen gather intake to `TypedSource<T>` | YES — `Include`/`Header`/`RouteParam` typed to concrete `TypedComponentSource`/`TypedPluginSource`; the mis-cut seam `cod(AsSource) ⊄ dom(Include)` is grounded at `08:911` and fixed in `08 §6.3:1031` | YES — the single most cross-cutting seam fix; lets a filtered/folded array be gathered | YES — strictly widening; concrete sources still compile | matches `08 §6.3` |
| **#3** Element mutations all return `ElementBuilder` | YES — `element-component:49-55` shows the literal/payload→`PipelineBuilder` vs `TypedSource`→`ElementBuilder` split exactly | YES — kills the overload-sensitive return flip; matches `ComponentRef` (which is uniformly self-returning, `:81-150`) | YES — all arities kept, only return regularized | n/a (shape) |
| **#4** validation `When().Then()` | YES — `value-arrays-validation:120` shows `When(condFactory, define)→void` | YES — mirrors conditions `When().Then()` (`conditions:81-84`), sibling guards stack TALL | YES — same activation; recursion target unchanged (validation critique Adj 1/2) | n/a (shape) |
| **#5** validation nested-grouping And/Or | YES — `value-arrays-validation:212-213` shows `And`/`Or` take only a pre-built `ClientValidationCondition`; nested callback absent | YES — reaches exact parity with conditions' two-shape And/Or (`conditions:81-82`, `09:91-95`) | YES — flat overloads kept; grouping shape ADDED | matches `09 §1.1` |
| **#6** plugin entry → 3 screaming verbs | YES — `pipeline:61-68` shows 8 `Plugin*` overloads across 3 return families (`PluginMemberBuilder`/`TypedPluginPropertySource`/`PluginCallBuilder`), return flips on `<T>` | YES — names the lane; ends the return-family guessing | YES — all 3 families + typed handles reachable; stringly stays at boundary | rides `09 §1.8/§3.7` `Method→Function`/`Void→Command` (verified `09:183-190,406-407`) |
| **#14** numeric folds `Min`/`Max`/`Average` | YES — `value-arrays-validation:41-50` shows `Sum` present, no `Min`/`Max`/`Average` | YES — completes a closed family (P6) | YES — strictly additive | DECIDED in `09:118,297-299` (NEW edges, verbatim) |
| **#15** inline gather on `Get`/`Delete` | YES — `pipeline:71-75` shows inline-gather overload on `Post`/`Put` only | YES — symmetric family completion | YES — pure addition; chained `.Gather` unchanged | n/a |
| **#16** template branch gets full builder | YES — `FusionConditionalBuilder` (`:305-321`) lacks `Text`/`Link`/`ButtonFor`/nested-`When` that `FusionTemplateBuilder` has | YES — deletes a narrower-twin type; branch body becomes a normal fragment | **capability GAIN** (not loss) — branch gains missing verbs; then/else still two callbacks | n/a (shape) |
| **#17** template `WhenTemplate().Else()` | YES — `:311` shows `else` as a 3rd positional `Action` on `When` | YES — matches conditions `Then/Else` voice | YES — 2-arg no-else kept | rides `09:354` `When→WhenTemplate` |
| **#18** template button options callback | YES — `:192-198,282-289` show wide multi-arg `Button`/`Link` + trailing `css` + stringly `onClick` positional | YES — TALL options, labeled `.OnClick`/`.Css` | YES — css/onClick/onClickFn/eventName all become named options; stringly `onClick` kept | n/a (shape) |
| **#19** SmartTextArea `.Reactive` normalized | YES — `component-reactive:157-164` shows the lone outlier: extends `ReactivePlan`, returns `void`, id-string + expr overloads | YES — conforms to the 56-builder gold standard | YES — both events + cross-model id kept via the builder binding | rides `09:341` |
| **#23** `ComponentRef.SetText→SetDisplayText` | YES the collision is real (`:90-91` vs `:49`) | YES — value/display pair self-documents | YES — both behaviors kept | **NO — see B2** (09 marks it KEEP, rename not in 09) |
| **#24** Focus verbs → `Focus()`/`Blur()` | YES `Focus`/`FocusIn`/`FocusOut` real (`:111-125`); `Blur` correctly noted absent as a method | YES — 3 names → 2 concepts | YES — rename, both behaviors callable everywhere | **PARTIAL — see B1** (`FocusIn→Focus` in 09; `FocusOut→Blur` is NOT) |
| **#25** loader `SetAutoHide`/toast `SetDuration` | YES — `applevel:193,211` show both named `SetTimeout` | YES — each names its mechanic; ends JS-`setTimeout` shadowing | YES — both timers kept | DECIDED `09:395,397` |
| **#26** `FusionConfirm` `SetMessage`/renderer `FusionConfirm` | YES — `applevel:233,236` show `SetContent` + renderer `FusionConfirmDialog` | YES — ends `SetContent` double-meaning + type≠renderer | YES — both kept | DECIDED `09:401,402` |
| **#27** toast `WithCloseButton`/`WithProgressBar` | YES — `applevel:212-213,218` show `ShowCloseButton`/`ShowProgressBar` sharing `Show` prefix with action `Show()` (`:218`) | YES — `Show` becomes the sole action verb | YES — same two booleans | n/a (config prefix; consistent with the Show/action split `09` draws) |
| **#28** template `.CssClass`/`.Attribute` | YES — `applevel:266-267` show `.Class`/`.Attr` | YES — keyword-clash + abbreviation fixed (template lane only) | YES — `Attr` stays on `NativeActionLinkBuilder` (`09:403`) | DECIDED `09:348,349,403` |
| **#29** `EventButton→DispatchButton` | YES — `applevel:286` shows `EventButton<TProperty>` | YES — names the dispatch, pairs with `Dispatch` | YES — kept | DECIDED `09:353` |
| **#30** `WhenTemplate`/`ShowTemplateIf` | YES — `applevel:290-292` show `When`/`ShowIf` emitting SSR `${if}` | YES — screams the SSR lane, ends cross-area `When` collision | YES — kept | DECIDED `09:354,355` |
| **#31** `Component<T>` inference + `ComponentId` | YES — `element-component:30-33` show 4 read-identical identity overloads | YES — infer `TOtherModel`; type the by-id string | YES — all 4 strategies reachable | n/a (additive) |
| **#33** `NativeLoader.SetTarget(Element)` | YES — `applevel:192` shows raw `string targetId` | YES — the lone stringly DOM-target hole | YES — string overload kept as boundary escape | n/a (additive) |
| **#34** `FromDom` builder callback | YES — `pipeline:83-84` / `value-arrays-validation:62-63` show two unlabeled positional strings | YES — TALL labeled `.Element(id).Member(name)` | YES — both string overloads kept | `11` itself flags "needs naming-sheet ratification" — honest |

### Orthogonality collapses (Tier 2) — verified by equality, sound

- **#7** pipeline `When` 3 overloads → one `TypedSource<TProp>`: `pipeline:77-79` shows exactly
  `(payload,path)`/`(responseBody,path)`/`(TypedSource)`. Folding payload/response via a shared
  `FromEvent`/`body.Read` factory mirrors `conditions:25-27` (which has the identical 3-overload
  `When`). Both reads kept. Sound.
- **#9** validation 12 doubled `Expression`+`Token` peer overloads → one via implicit conversion:
  `value-arrays-validation:149-161` shows exactly the 12 `Expression`+`Token` pairs (EqualTo,
  NotEqualTo, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo × 2). Literal
  overloads untouched. Sound. (Note Adj #37 correctly *keeps* `NotEqual`/`NotEqualTo` split per
  D1 — no contradiction with #9, which collapses only the operand-wrapper twins.)
- **#10** 8 scalar `Arg(...)` + `ArgValue<T>` → one `Arg<TValue>`: `PluginMemberBuilder` rows
  `:79-129,193-243` (read+call faces) show exactly the 8 scalar + `ArgValue` per face. Call sites
  identical via inference. Sound.
- **#11/#12/#13** plugin declarer synonyms / arg-contract shapes / read-call drift: the
  `Method`/`Function` + `Void`/`Command` synonyms are real (`PluginTypeBuilder:46-58`), the
  inline≠subclass face difference is real (`PluginTypeBuilder` has `Method`/`Void`; `Plugin` base
  has only `Function`/`Command`/`Property`, `:77-86`), and the ~95% read/call `Arg` duplication is
  real (`PluginMemberBuilder:55-133` vs `:169-243`). All ride `08 §6.4` (merge extensionally-equal
  morphisms, `08:794,1050`) and `09:406-408`. Every former call has a survivor. Sound.

### Tier-5 typed-id hardenings (#32, #35–#39) — additive, sound

- **#32** `Into`/`ShowValidationErrors` model-expression overload: `pipeline:69-70` shows raw
  `string` terminals. Adds an `IdGenerator`-derived overload; string form kept. Plan-driven-ids
  win. Sound.
- **#36** plugin read terminal `AsPluginSource()`: `PluginMemberBuilder:136` (implicit conversion)
  vs `:250` (`Fire()`) asymmetry is real. Adds a parallel explicit terminal; implicit kept. Sound.
- **#37** `NotEqual`/`NotEqualTo` KEEP-but-disambiguate: correctly cites D1 forbidding the merge
  (`value-arrays-validation:151-153`); both verbs + all overloads kept verbatim. Sound.
- **#38/#39** `Field` fork lock + condition-opener unification: `value-arrays-validation:118,170`
  show the two `Field` return surfaces and the parallel `ClientValidationConditionBuilder`. Both
  overloads kept; only the opener type unified to the one `ConditionStart` vocabulary. Rides
  `09 §3.6` (`ClientValidation*→Client*`). Sound.

---

## "Already good — do not churn" (§3) — spot-checked, accurate

The protected bones are correctly identified and genuinely load-bearing:
- `.Reactive` gold standard, `(args,p)=>` on all 56 (`component-reactive:101-179`,
  summary `:187`) — verified, every row `ReturnsSelf=yes`, identical shape.
- `ComponentRef` uniformly self-returning (`element-component:81-150`) — verified.
- `Value()` terminates into `TypedComponentSource` (`element-component:160`) — verified `ReturnsSelf=no`.
- `Dispatch(name,literal)` vs `DispatchFrom(name,b=>…)` confirm-and-keep — matches `09:242`.
- `Count()`/`Count(predicate)`, `Any()`/`Any(predicate)` honest-intents KEEP
  (`value-arrays-validation:41-44`) — verified, not redundant.
- `PluginCallBuilder.Fire()` named terminal for the void lane (`plugins:153`) — verified, correctly
  framed as the legitimate `void`-lane exception to P1.

No item in §3 is contradicted by an adjustment in §2 (e.g. §3 keeps Element `SetText`/`SetHtml`
literal overloads; #3 only regularizes the *return type*, #23 only renames the *ComponentRef*
collider — no double-touch).

---

## Reconciliation (§4) — structurally sound except B1/B2

- **§4b** (`08` forcing functions): `§6.3` (widen intake, `08:1031`), `§6.4` (merge equal
  morphisms, `08:1050`), `§6.1`/`§6.2` all exist and say what `11` attributes. The `(C1, internal)`
  lane-stamp obligation is correctly held OUT of the §2 count as an internal lowering duty. Sound.
- **§4c** (the two must-be-identical shapes): the And/Or two-shape parity (`09:91-95`) and the
  `(args,p)=>` callback unification are correctly drawn; both trace to real `09`/AST rows.
- **§4d** (module-map fold): the 5 `10 §5` items each map to a real §2 number; not double-counted.
  Consistent with `10 §4`'s "12-module cut correct."
- **§4a**: the table is the right idea and ~14 of its ~16 rows are correctly cited as `09`
  decisions. **The two exceptions are B1 (#24 Blur) and B2 (#23 SetDisplayText)** — fix those two
  rows and §4a is clean.

---

## Churn / novelty check — none found

No adjustment introduces a name or shape purely for aesthetic novelty. Every AFTER either
(i) removes a real redundancy proven by AST equality (#7,#9,#10,#11,#12,#13), (ii) completes a
closed family (#5,#14,#15), (iii) fixes a real collision or lie proven in the AST
(#23,#24,#25,#26,#28,#29,#30), or (iv) is strictly additive with the old form kept
(#31,#32,#33,#34,#36). The "do not churn" §3 list is honored.

## Feature-loss check — none found

Every row carries an explicit survivor note and each was verified against the AST: renames keep
both behaviors, collapses keep all arities via inference/implicit-conversion, widenings are
strictly additive, and the one capability change (#16) is a *gain*. `Html.On` free-function
(partial injection) is preserved (#1). Stringly boundaries are preserved as escapes
(#6,#31,#33,#34). **Zero capability is dropped.**

---

## Required fixes before SOUND

1. **§4a / Adj #24:** Do not cite `FocusOut → Blur` as decided in `09`. `09` only locks
   `FocusIn → Focus` (`09:344,451`). Either ratify `FocusOut → Blur` in `09` first, or move that
   half to a "pending rename" note. (`FocusIn → Focus` stays correctly cited.)
2. **§4a / Adj #23:** Do not cite `09:343` as deciding `SetText → SetDisplayText`. `09:343`/`:345`
   mark those verbs **KEEP/unchanged**. Re-label #23 as a *pending grammar rename to ratify in
   `09`* (and note `09` currently KEEPs the colliding name, so `09` itself needs the update).
3. *(optional)* **Adj #21:** clarify "3 slices (4 overloads)" so the `on`-vs-`eventSelector` count
   is unambiguous at method granularity.

After fixes 1–2, all four tests pass for all 39 adjustments and the doc is SOUND. As written, the
two misattributed §4a citations make the report **REVISE**.

---

## VERDICT: REVISE
