# 11 — DSL Grammar Hardening

> **What this is.** The consolidated grammar-hardening pass over the whole green-field DSL.
> It reads the per-cluster PL-architect critiques (`grammar-critique-*.md`) plus the
> module-map critique (`10-dsl-module-map.md` §5) and distills them into (1) the few
> **unifying grammar principles** that make the entire DSL read TALL and compose, (2) the
> **ranked, deduplicated list of every BEFORE → AFTER adjustment** the critiques propose,
> ranked by writability impact, (3) the **already-good load-bearing bones** that must not be
> churned, and (4) the **reconciliation** against the locked names in `09-dsl-naming-sheet.md`
> and the design discoveries in `08-determinism-formalization.md`.
>
> **Source critiques folded in (6 perspectives):**
> - `grammar-critique-triggers-pipeline.md` — Triggers + Pipeline/Reactions (7 counted)
> - `grammar-critique-components-reactive.md` — Components / `.Reactive` / Element / ComponentRef (10)
> - `grammar-critique-plugins-app-template.md` — Plugins + App-level + Fusion Template (15)
> - `grammar-critique-validation.md` — Validation / ClientRule / WhenField / FieldGuard (8)
> - `grammar-critique-value-arrays.md` — Value spine + ReactiveArray (5)
> - `10-dsl-module-map.md` §5 — cross-module PL critique (5, all overlapping the above)
>
> **Note on file count.** The brief referenced "7 critiques"; the redesign folder contains
> **5** `grammar-critique-*.md` files plus the module-map's own §5 critique (a 6th
> overlapping perspective). All adjustments from all six are folded in and deduplicated
> below; the module-map's 5 items are each a restatement of an item already in one of the 5
> cluster critiques (reconciled in §4, not double-counted).
>
> **Zero feature loss is the invariant.** Every adjustment is a `BEFORE → AFTER` that
> preserves every capability — renames keep every survivor, overload collapses keep call
> sites compiling via inference/implicit-conversion, intake widenings are strictly additive.

---

## 1. THE UNIFYING PRINCIPLES

The bar the whole DSL is judged against: **easy to write, reads TALL** — vertical fluent
chains, one call per line, every callback handing back a clean builder, return types that
match intent, one clear spelling per intent. Seven grammar rules make that true across every
module. Every adjustment in §2 is an instance of one of these.

### P1 — No void dead-ends (the chain never breaks)

A builder method that is **not** a deliberate terminal must return a chainable builder, never
`void`. `void` forces the author to break a vertical chain into disconnected statements and
hold a handle in their head across sites.

- The entry edge `Html.On` returns `void` and dead-ends the very top of every view
  (`grammar-critique-triggers-pipeline.md` A1).
- Validation `When(cond, body)` returns `void` and cannot stack sibling guarded blocks
  (`grammar-critique-validation.md` Adj 2).
- `FusionSmartTextArea.Reactive` returns `void`, breaking the chainable `.Reactive` contract
  (`grammar-critique-components-reactive.md` Adj 6; `10` §5.5).
- Plugin's void-call lane is the **legitimate** exception: it has no value to chain off, so it
  terminates explicitly with `Fire()` — a named terminal, not a silent `void` (kept good).

### P2 — Uniform self-returning builders (one verb, one return type)

Within one builder family, **every mutation verb returns the same builder** (`ReturnsSelf`),
and a chain ends only at a single discoverable terminator. A verb whose return type *flips by
overload* is the canonical "asymmetric re-wrap" wart.

- `ElementBuilder.SetText`/`SetHtml` return `PipelineBuilder` for literal/payload overloads but
  `ElementBuilder` for the `TypedSource` overload — same verb, two returns
  (`grammar-critique-components-reactive.md` Adj 1; `10` §5.1).
- `ComponentRef` already does this right (every mutation self-returns) — Element should match it
  so the two receivers read identically (kept good A2; the fix makes Element conform).

### P3 — One callback shape everywhere: `(args, p) => { }`

Every place that hands the author a pipeline gives back the **same** clean
`PipelineBuilder<TModel>` — reached the same way, with the typed payload read **off the
builder** rather than reshaping the callback's arity. The trigger/response/`.Reactive`/branch
callbacks must all look alike.

- The component `.Reactive` callback `(args, p) => …` is the gold standard, identical on all 56
  builders (kept good A1).
- Trigger typed-payload overloads reshape the callback to a leading positional `(payload, p)` —
  position differs per trigger; fold the payload onto `p.Payload` so the callback shape never
  changes (`grammar-critique-triggers-pipeline.md` A2/A3).
- Condition branch bodies (`Then`/`Else`) and validation `When().Then()` hand back the same
  builder shape — the cross-area rule that ties this principle to P5.

### P4 — One spelling per intent (kill synonyms and redundant overloads)

One concept has exactly one verb. Synonym pairs, name-vs-name drift between two declarer faces,
and redundant overloads that differ only by an operand wrapper all violate ORTHOGONALITY.

- Plugin declarer carries `Method`/`Function` (value) and `Void`/`Command` (void) — two names
  per concept, and a third drift between inline and subclass declarers
  (`grammar-critique-plugins-app-template.md` Adj 1).
- Eight scalar `Arg(...)` overloads + `ArgValue<T>` are nine spellings of "add a literal arg"
  (`grammar-critique-plugins-app-template.md` Adj 3).
- Twelve doubled `Expression`+`Token` peer-comparison overloads in validation differ only by the
  operand wrapper (`grammar-critique-validation.md` Adj 7).
- `From(args, selector)` and `When(payload, path)` re-mint the payload read that a `TypedSource`
  factory already expresses (`grammar-critique-triggers-pipeline.md` B2/B5; `10` §5.2).

### P5 — Same concept = same shape across modules (cross-area consistency)

If two modules express the same idea, they must use the **same grammar shape** — same callback
arity, same composition surface, same if/else voice. Divergence forces the author to relearn
the idea per module.

- **The And/Or composition shape must be identical everywhere.** Conditions has *two* And/Or
  shapes — flat `And(condition)` and nested-grouping `And(g => g…)` — `09 §1.1`. Validation has
  only the flat shape; the nested-grouping shape is *missing from source*
  (`grammar-critique-validation.md` Adj 5). The fix gives validation the **identical** two-shape
  And/Or surface as conditions.
- **The if/else voice must be identical everywhere.** Conditions spells it `.Then(...).Else(...)`.
  Validation `When` takes two positional lambdas (`grammar-critique-validation.md` Adj 1); the
  Fusion template `When` takes a 3rd positional `Action` for `else`
  (`grammar-critique-plugins-app-template.md` Adj 15). Both fold to the `Then/Else` continuation.
- **The value spine `TypedSource<T>` is the one intake everywhere a value is read** — component
  `SetDataSource`, gather `Include`/`Header`/`RouteParam`, validation peer-comparisons, condition
  `When` (`08 §6.3` generalized; appears in every cluster critique).

### P6 — Complete families (no asymmetric holes)

If a family exposes some members of a closed set, it must expose all of them — a dev discovers
the rest by symmetry. A partial family forces the author *out* of the DSL.

- The numeric fold family has `Sum` but not `Min`/`Max`/`Average`
  (`grammar-critique-value-arrays.md` Adj A3).
- Inline-gather exists on `Post`/`Put` but not `Get`/`Delete`
  (`grammar-critique-triggers-pipeline.md` B4).

### P7 — Good defaults and screaming names (minimal ceremony, typed over stringly)

The common case needs minimal ceremony; the right path is the discoverable one; raw strings are
typed wherever the framework already knows the value. Names scream the lane/result so the author
never guesses.

- Wide multi-arg/`css`-overload template `Button`/`Link` calls → TALL options callback; stringly
  `onClick` is *labeled* `.OnClick(...)` not an unnamed 2nd positional
  (`grammar-critique-plugins-app-template.md` Adj 13; `10` §5.4).
- `FromDom(string, string)` two unlabeled strings → labeled builder callback
  (`grammar-critique-value-arrays.md` Adj A5).
- Stringly ids (`Component<T>("id")`, `NativeLoader.SetTarget("id")`, `Into`/`ShowValidationErrors`)
  gain typed overloads while keeping the string boundary escape
  (`grammar-critique-components-reactive.md` Adj 8; `grammar-critique-plugins-app-template.md`
  Adj 9; `grammar-critique-triggers-pipeline.md` B7).
- `AsSource()` → `AsArraySource()`, `Find` → `FindFirst` — names scream the result/contract
  (`grammar-critique-value-arrays.md` A1/A2).

---

## 1.5 — cshtml EXPRESSIBILITY (the Razor lens: what these principles can and cannot promise)

P1–P7 are C#-true, but the DSL is authored in **Razor cshtml**, which constrains *where* they
apply. Grounded in the real signatures + the real view (`Cascading/Index.cshtml`):

**Two surfaces, not one.**
- **PLAN-BUILDING** (inside `b => b…`, the `(args, p) => { }` pipeline, conditions, http,
  validation, plugin declarers) is pure C# inside one `@{ … }` block or one `@( … )` expression —
  **every tall-chain principle applies in full.** Block-bodied lambdas are valid arguments in BOTH
  `@{ }` and `@( )`, so the multi-statement `Then`/`Else`/`Reactive` bodies (proven in
  `ast-proof.md`) compose freely. This is where the complexity *and* the writability win live.
- **RENDER SURFACE** is **location-bound**: a component is emitted where its statement sits in the
  markup. `Html.InputField(...).FusionDropDownList(b => …)` returns **`void`** and renders as a side
  effect (`FusionDropDownListHtmlExtensions.cs:78`) → `@{ … }`; `Html.NativeButton(...)` returns
  **`IHtmlContent`** → `@( … )` (`Cascading/Index.cshtml:99`). You **cannot** chain DSL after a
  component renders, nor collapse a view into one chain.

**Consequences (so this doc only promises what Razor expresses):**
- **P1 "no void dead-ends" does NOT touch render terminals.** A `.FusionXxx(...) → void` render is a
  *deliberate terminal* (P1's own carve-out) — correct, not a wart. P1 applies only to PLAN-BUILDING
  voids (`Html.On`, validation `When`, `FusionSmartTextArea.Reactive`).
- **The render-surface wart to harden is PREDICTABILITY, not chaining.** Input components return
  `void` (force `@{ }`); `NativeButton` returns `IHtmlContent` (force `@( )`) — the author must know
  *per component* which Razor wrapper to use. **New render-surface principle (P8): one consistent
  render-terminal contract** so `@{ }` vs `@( )` is a single rule, never a per-component guess.
  Additive — changes no reaction grammar.
- **Adj #1 (`Html.On → ReactivePlan.On`) is expressible but registration-only.** `@{ plan.On(…); }`
  works and `plan.On(…).On(…)` chains; the instance-receiver form is the real win, chaining a minor
  bonus. Do not oversell "one tall chain for the whole view" — a Razor view is necessarily *setup
  block + per-location render statements + markup*.

**Net:** every Tier-1/Tier-2 adjustment is PLAN-BUILDING and fully cshtml-expressible. The render
surface gets one new principle (P8, a predictable render terminal); its location-bound
`void`/`IHtmlContent` terminals are kept as the correct Razor shape.

---

## 2. THE RANKED ADJUSTMENTS

Every distinct `BEFORE → AFTER` from all six critiques, deduplicated and ranked by **writability
impact** (how much it changes the everyday authoring experience): the chain-breaking and
seam-breaking fixes first, then the orthogonality collapses, then the consistency renames, then
the typed-id/discoverability hardenings.

Legend for the last two columns: **PL property** = the architect property improved; **Capability
preserved** = the zero-feature-loss note. "Cite" gives the AST-grammar `file:line` anchor for the
current shape (from the originating critique).

### Tier 1 — Chain integrity and seam composition (highest writability impact)

| # | Area | Current shape (cite) | AFTER | PL property | Capability preserved |
|---|------|----------------------|-------|-------------|----------------------|
| 1 | Triggers entry | `Html.On(plan, t=>…) -> void`; plan threaded across 3 statements (`ast-grammar-entry-triggers.md:33`) | `ReactivePlan.On(t=>…)` returns the same `ReactivePlan` (chainable); free-function `Html.On(plan,…)` kept for partial injection | TALL-reading, composability (P1) | both spellings; partial-injection path kept |
| 2 | Value→Request seam | gather `Include`/`Header`/`RouteParam` typed to concrete `TypedComponentSource`/`TypedPluginSource` (`GatherBuilder.cs:206,266`; `ast-grammar-value-arrays-validation.md:51`) | widen intake to abstract `TypedSource<T>` so `AsArraySource ⨾ Include` composes (`08 §6.3`) | composability seam `cod=dom` (P5) | strictly widening; concrete sources still compile; array/fold results now gatherable |
| 3 | Element mutations | `SetText`/`SetHtml` return `PipelineBuilder` for literal/payload, `ElementBuilder` for `TypedSource` (`ast-grammar-element-component.md:49-55`, `ElementBuilder.cs:55,65,87`) | every Element mutation returns `ElementBuilder`; one `Done()` (or next pipeline verb on held `p`) terminates — Element now matches `ComponentRef` | least-surprise, consistency (P2) | all literal/payload/response/typed-source arities kept; only return regularized |
| 4 | Validation `When` shape | `When(condFactory, define) -> void`, two positional lambdas (`ast-grammar-value-arrays-validation.md:120`) | `When(cond).Then(define)`; `Then` returns the rules builder so sibling guarded blocks stack TALL | TALL-reading, composability, consistency (P1/P3/P5) | identical activation; recursion target unchanged; mirrors conditions `When().Then()` |
| 5 | Validation And/Or grouping | `And`/`Or` accept only a pre-built `ClientValidationCondition` — nested-grouping callback **missing from source** (`ast-grammar-value-arrays-validation.md:212-213`) | add `And(Func<opener,Condition>)` / `Or(…)` nested shape — identical to conditions `09 §1.1` | composability, consistency (P5 — the And/Or rule) | flat overloads kept; grouping shape added to reach parity |
| 6 | Plugin pipeline entry | `Plugin` has 8 overloads spanning 3 return families; return flips on `<T>` (`ast-grammar-pipeline.md:61-68`) | three screaming verbs `PluginFunction<T>` (value) / `PluginCommand` (void) / `PluginProperty<T>` (read), each with a (name,member) and a typed-handle arity | orthogonality, discoverability, least-surprise (P4) | all 3 families + typed handles reachable; stringly stays at plugin boundary |

### Tier 2 — Orthogonality collapses (kill redundant spellings)

| # | Area | Current shape (cite) | AFTER | PL property | Capability preserved |
|---|------|----------------------|-------|-------------|----------------------|
| 7 | Pipeline `When` entry | 3 overloads: `(payload,path)`, `(responseBody,path)`, `(TypedSource)` (`ast-grammar-pipeline.md:77-79`) | one `When<TProp>(TypedSource<TProp>)`; payload/response fold via `FromEvent(args,path)` / `body.Read(path)` factories (mirrors `09 §1.1` And/Or) | orthogonality, consistency (P4/P5) | both reads kept via the one shared source factory |
| 8 | Pipeline `From` array entry | `From(args, selector)` is a 2nd spelling of "array from a payload"; abstract `From(TypedSource)` also exists (`ast-grammar-pipeline.md:81-84`) | keep abstract `From(TypedSource<T[]>)` + `FromDom`; fold `(args,selector)` through the shared `FromEvent` factory | orthogonality, composability (P4) | payload-array kept via factory; `FromDom` overloads kept |
| 9 | Validation peer overloads | 12 doubled `Expression`+`Token` peer-comparison overloads (`ast-grammar-value-arrays-validation.md:149-161`) | one overload per peer verb via implicit `Expression -> Token` conversion (then widened to `TypedSource<T>` per #2) | orthogonality, discoverability (P4) | inline-expression and reused-token both compile; literal overloads untouched |
| 10 | Plugin literal args | 8 scalar `Arg(...)` + `ArgValue<T>` — nine spellings of "add a literal arg" (`PluginMemberBuilder.cs:79-129,193-243`) | one generic `Arg<TValue>(TValue)` (absorbs all 8 scalars + `ArgValue`); `DateTime` formatting folds into the literal lowering | orthogonality, discoverability (P4) | call sites identical (TValue inferred); source/`(ResponseBody,path)` overloads stay |
| 11 | Plugin declarer synonyms | `Method`/`Function` and `Void`/`Command` both present; inline ≠ subclass declarer (`PluginTypeBuilder.cs:24,48,62,68`) | `Function`/`Command`/`Property` only — one vocabulary, identical to the `Plugin` base | orthogonality, consistency (P4) | every former call has a survivor |
| 12 | Plugin arg-contract shapes | arg-type list spelled two ways: `Arg<T>()` chain **and** `Args(Action<…>)` callback (`Plugin.cs:259,266,290,297`; `PluginTypeBuilder.cs:40-80,164`) | the `Arg<T>()` chain is the one spelling on both faces; `Args(Action<…>)` kept only for loop/programmatic arg lists | orthogonality, easy-to-write (P4) | callback survives as grouping-only escape |
| 13 | Plugin read/call arg drift | `PluginMemberBuilder.Arg*` and `PluginCallBuilder.Arg*` ~95% duplicated, hand-kept in sync (`PluginMemberBuilder.cs:55-133,169-243`) | one shared `PluginArgs<TSelf>` arg spine consumed by both faces (`08 §6.4`) | consistency, anti-drift (P4) | author chains byte-identical; internal spine only |

### Tier 3 — Family completion and symmetric ergonomics

| # | Area | Current shape (cite) | AFTER | PL property | Capability preserved |
|---|------|----------------------|-------|-------------|----------------------|
| 14 | Numeric folds | family has `Sum` but not `Min`/`Max`/`Average` (`ast-grammar-value-arrays-validation.md:41-50`) | add `Min`/`Max` (→ `ReactiveValue<TNum>`, empty→null) and `Average` (→ `ReactiveValue<double>`) | orthogonality (complete family), easy-to-write (P6) | strictly additive; cluster brief required these folds |
| 15 | HTTP inline gather | inline-gather overload on `Post`/`Put` only, not `Get`/`Delete` (`ast-grammar-pipeline.md:71-75`) | add `(url, Action<GatherBuilder>)` to `Get` and `Delete` — all four verbs offer bare+inline | consistency, easy-to-write (P6) | pure addition; chained `.Gather` form unchanged |
| 16 | Template conditional builder | `When`/`ShowIf` callbacks receive a **narrower** `FusionConditionalBuilder` missing `Text`/`Link`/`ButtonFor`/nested-`When` (`FusionTemplateBuilder.cs:303,343`; `FusionConditionalBuilder.cs:18-183`) | branch callbacks receive the **full** `FusionTemplateBuilder`; delete the narrower type — the branch body is just another template fragment | composability seam `cod⊆dom`, discoverability (P5) | capability **gain**: branch body gains the missing verbs; then/else still two callbacks |
| 17 | Template if/else | `else` is a 3rd positional `Action` on `When` (`FusionTemplateBuilder.cs:311`) | `WhenTemplate(cond, then).Else(else)` fluent continuation — matches conditions `Then/Else` voice | TALL-reading, consistency (P5) | 2-arg (no-else) kept; both branches now labeled |
| 18 | Template button shape | wide multi-arg `Button`/`ButtonFor`/`DispatchButton`/`Link` with trailing `css` overloads; stringly `onClick` positional (`FusionTemplateBuilder.cs:192-282`) | one entry per node handing back a `TemplateButtonOptions` callback; `.OnClick(...)`/`.Css(...)` one-per-line | TALL-reading, orthogonality (P7) | css/onClick/onClickFn/eventName all become named options; stringly `onClick` labeled, kept |

### Tier 4 — Cross-receiver / cross-slice consistency (rename to one shape)

| # | Area | Current shape (cite) | AFTER | PL property | Capability preserved |
|---|------|----------------------|-------|-------------|----------------------|
| 19 | SmartTextArea `.Reactive` | the lone outlier: extends `ReactivePlan`, returns `void`, id-string + expr overloads (`ast-grammar-component-reactive.md:157-164`; `FusionSmartTextAreaReactiveExtensions.cs:11,22`) | `.Reactive` on the SmartTextArea **builder**, returns the builder (`ReturnsSelf`); drop the id-string overload | consistency (the outlier), least-surprise (P1/P2/P5) | both events + cross-model id kept via the builder's binding |
| 20 | SmartTextArea builder shape | takes `Action<FusionSmartTextAreaOptions>` config-bag while every sibling takes `Action<XxxBuilder>` (`ast-grammar-component-reactive.md:66`) | give it a `FusionSmartTextAreaBuilder` like every sibling | consistency, TALL-reading (P5) | every option becomes a builder verb |
| 21 | `.Reactive` selector param | named `eventSelector` on 53 slices, `on` on 3 (`ast-grammar-component-reactive.md:28,136,144,163`) | one name (`on`) on all 56 | consistency (P4) | pure rename |
| 22 | `.Reactive` type inference | 2-arity `<TModel,TArgs>` vs 3-arity `<TModel,TProp,TArgs>` tempt explicit type args (`ast-grammar-component-reactive.md:172-179`) | guarantee full inference (`TModel` from plan, `TProp` from builder, `TArgs` from selector) so no slice ever writes type args | least-surprise (P3) | signatures unchanged; arity stays type-honest, reads identical |
| 23 | `ComponentRef.SetText` | collides with `Element.SetText`; means "set display text not value" (`ast-grammar-element-component.md:90-91`) | rename to `SetDisplayText`; pairs with `SetValue` | consistency, least-surprise (P4) | both behaviors kept; value/display pair self-documenting |
| 24 | Focus verbs | `Focus`/`FocusIn`/`FocusOut` — three names, two concepts; `FocusIn` names an event not the method (`ast-grammar-element-component.md:111-125`) | `Focus()` (the focus verb, every slice) + `Blur()` (the focus-loss call); `evt => evt.Blur` stays the event lane | orthogonality, consistency (P4) | focus + focus-loss callable on every slice |
| 25 | App-level `SetTimeout` | two services, two meanings, both shadow JS `setTimeout` (`NativeLoaderExtensions.cs:54`; `FusionToastExtensions.cs:51`) | loader→`SetAutoHide`, toast→`SetDuration` | least-surprise, consistency (P4) | both timers kept, each names its mechanic |
| 26 | `FusionConfirm` naming | `SetContent` collides with toast `SetContent`; renderer `FusionConfirmDialog` ≠ type (`FusionConfirmExtensions.cs:21,39`) | `SetMessage` + renderer `FusionConfirm` (type==renderer) | consistency, least-surprise (P4) | both kept |
| 27 | Toast config/action prefix | `ShowCloseButton`/`ShowProgressBar` (config) share the `Show` prefix with `Show()` (action) (`FusionToastExtensions.cs:56-61,90`) | config toggles → `WithCloseButton`/`WithProgressBar`; `Show` is the sole action verb | least-surprise, TALL-reading (P7) | same two booleans set |
| 28 | Template attr/class verbs | `.Class` (keyword clash) / `.Attr` (abbreviation) (`FusionTemplateBuilder.cs:43,52`) | `.CssClass` / `.Attribute` (template lane only) | discoverability, least-surprise (P4/P7) | kept; `Attr` stays on `NativeActionLinkBuilder` |
| 29 | Template event button | `EventButton` reads as "button with events" — every button has events (`FusionTemplateBuilder.cs:245`) | `DispatchButton<TProperty>` — names the dispatch, pairs with `Dispatch` verb | discoverability, least-surprise (P4) | kept |
| 30 | Template conditional verbs | `When`/`ShowIf` collide by name with runtime `When` but emit SSR `${if}` strings (`FusionTemplateBuilder.cs:303-343`) | `WhenTemplate`/`ShowTemplateIf` (screams the SSR lane) | orthogonality, consistency (P4/P5) | kept |

### Tier 5 — Typed-id / discoverability hardenings (additive, lowest churn)

| # | Area | Current shape (cite) | AFTER | PL property | Capability preserved |
|---|------|----------------------|-------|-------------|----------------------|
| 31 | `Component<T>` identity | 4 overloads (same-model / cross-model / by-id string / app-level) read identically at call site (`ast-grammar-element-component.md:30-33`; `ast-grammar-pipeline.md:55-58`) | infer `TOtherModel` from the lambda param (drop explicit 2nd type arg); type the by-id string as a `ComponentId` value-object | discoverability, least-surprise, easy-to-write (P7) | all 4 identity strategies reachable |
| 32 | `Into`/`ShowValidationErrors` ids | raw `string` element/form id terminals (`ast-grammar-pipeline.md:69-70`) | add a model-expression overload deriving the id via `IdGenerator`; string form kept for explicit element ids | least-surprise (plan-driven ids) (P7) | string form kept |
| 33 | `NativeLoader.SetTarget` | raw `string targetId` — the lone stringly DOM-target hole (`NativeLoaderExtensions.cs:41`) | add typed `SetTarget(Element)` overload; string overload kept as boundary escape | easy-to-write, consistency (P7) | string overload kept |
| 34 | `FromDom` arg shape | two unlabeled positional strings (`PipelineBuilder.Arrays.cs:37,41`) | add a TALL builder-callback overload (`.Element(id).Member(name)`, typed element); string overloads kept | TALL-reading, discoverability (P7) | both string overloads kept; **needs naming-sheet ratification** |
| 35 | `ComponentRef` discoverability | every verb is an invisible extension method scattered across ~70 slice files (`ast-grammar-element-component.md:59-68,203-209`) | co-locate verbs per vendor namespace; promote universal verbs (`SetValue`/`Value`/`Enable`/`Disable`) to one `IInputComponent`-constrained extension | discoverability, consistency (P7) | vendor-specific verbs stay per-slice (Rule 5) |
| 36 | Plugin read terminal | read face terminates only via invisible implicit conversion; call face via explicit `Fire()` — asymmetric (`PluginMemberBuilder.cs:136,250`) | keep implicit conversion; add parallel explicit `AsPluginSource()` so faces terminate symmetrically | consistency, least-surprise (P2) | implicit path kept for common case |
| 37 | Validation `NotEqual`/`NotEqualTo` | literal vs peer split signalled only by spelling; points the wrong way vs overloaded `EqualTo` (`ast-grammar-value-arrays-validation.md:151-153`) | keep both (D1 forbids merge); operand-kind first-sentence XML-doc + distinct param names so the IDE tooltip disambiguates | discoverability (P7) | both verbs + all overloads verbatim |
| 38 | Validation `Field` fork | `Field(expr)` returns rule-builder or condition-start depending on receiver (`ast-grammar-value-arrays-validation.md:118,170`) | keep the verb; lock the two return surfaces as **disjoint** operator sets (rule-lane never exposes `Eq/Gt`; condition-lane never exposes `Required/Email`) | least-surprise, orthogonality (P4) | both overloads kept on both builders |
| 39 | Validation condition opener | `When` exposes a parallel `ClientValidationConditionBuilder`; a second `Field`-bearing builder to learn (`ast-grammar-value-arrays-validation.md:120,170`) | unify the condition-start opener with the **one** conditions-area `ConditionStart` vocabulary | consistency, discoverability (P5) | every operator + And/Or/Not kept; only the opener type unified |

**Total distinct adjustments: 39.**

---

## 3. ALREADY GOOD — DO NOT CHURN

The load-bearing good bones every critique flagged as PL-correct as cut. Touching these is
churn, not improvement. Listed so the hardening stays surgical.

### Components / `.Reactive` / Element / ComponentRef
- **`.Reactive` is the gold-standard shape** — identical on all 56 builders, `(args, p) => …`
  callback, `ReturnsSelf` so `.Reactive(...).Reactive(...)` chains, typed `eventSelector` over
  `<Comp>Events` (`ast-grammar-component-reactive.md:101-179`). The bar the rest is judged against.
- **`ComponentRef` mutation verbs are uniformly self-returning** — `SetValue`/`SetDataSource`/
  `DataBind`/`Enable`/… all return `ComponentRef` (`ast-grammar-element-component.md:81-150`).
- **`Value()` correctly terminates the mutation chain into a `TypedComponentSource`** — a read
  is not a mutation; the return type screams "now a value source" (`ast-grammar-element-component.md:160`).
- **Zero spurious nesting** — no Element/ComponentRef leaf takes an `Action<PipelineBuilder>`;
  recursion lives at trigger/condition/response level (`ast-grammar-element-component.md:185-190`).
- **`Fusion*`/`Native*` vendor prefixes** — vendor-isolation screaming names (HARD RULE `09:340`).
- **`Html.InputField` → `InputBoundField<TModel,TProp>`** — `TProp` flows into the builder; the
  deterministic-id keystone (`ast-grammar-component-reactive.md:38-39`).

### Triggers / Pipeline
- **Every `TriggerBuilder` method `ReturnsSelf`** — triggers chain and repeat
  (`ast-grammar-entry-triggers.md:42-43`).
- **Every trigger callback hands back `PipelineBuilder<TModel>`** — one clean nesting point per
  trigger (`ast-grammar-entry-triggers.md:49-56`).
- **`Dispatch`/`DispatchFrom`/`ShowValidationErrors`/`Into` are self-returning chainable terminals**
  (`ast-grammar-pipeline.md:51,53,69,70`).
- **`Element(id)`→`ElementBuilder` / `Component<…>`→`ComponentRef` open clean narrow sub-grammars**
  — no god-object (`ast-grammar-pipeline.md:54-58`).
- **`Get/Post/Put/Delete` verbs + `{placeholder}` route templates**; **`Parallel(params …branches)`**;
  **`Confirm(message)`→`GuardBuilder`** (the async user-decision lane); **`From*` typed source
  entries** with the `Dom` boundary suffix (`ast-grammar-pipeline.md:59-80`).
- **`Then`/`ElseIf`/`Else` first-match chain**; **`ReactivePlan()`/`ResolvePlan()` factories** (the
  `PlanScope` discriminant screamed by the factory name) (`ast-grammar-entry-triggers.md:31-32`).
- **`Dispatch(name, literal)` vs `DispatchFrom(name, b=>…)`** — literal-vs-source is correct
  orthogonality; the positional literal is the right terse ergonomic (B3, confirm-and-keep).

### Plugins / App-level / Template
- **App-level verbs are self-returning `ComponentRef` extensions** — `SetTitle().SetContent().Success().Show()`
  reads TALL; identity (`ElementId`/`DefaultId`) is separated from behavior.
- **Symmetric open/close pairs** (`Open`/`Close`, `Show`/`Hide`) — true antonyms.
- **Toast severity verbs `Success`/`Warning`/`Danger`/`Info`** as zero-arg chainable methods (no
  stringly `type:"success"`); the dead `ToastType` enum is deleted.
- **`PluginCallBuilder.Fire()`** as the explicit call terminal (the void lane needs a named terminal).
- **`PluginMemberBuilder`'s implicit conversion to `TypedPluginSource<TReturn>`** — a read drops
  straight into any `TypedSource` slot, no `Build()`.
- **`Plugin` base declaration face** uses exactly `Function`/`Property`/`Command` — the canonical pair.
- **Template element-named children** (`Span`/`Img`/`Badge`/`Icon`/`Link`/`Raw`) each name the HTML
  node and return the builder; **`FusionTemplateExpression` static lowering helpers** correctly static.

### Validation
- **The rule-verb chain is the model TALL surface** — `Required`/`Email`/`MinLength`/`Range`/… all
  `ReturnsSelf` (`ast-grammar-value-arrays-validation.md:131-161`).
- **Spelled-out constraint verbs** `GreaterThanOrEqualTo`/`LessThanOrEqualTo`/`EqualTo` (not `Gte`/`Lte`).
- **Peer-field overload reuses the verb name** — `EqualTo(literal)`/`EqualTo(expr)`/`EqualTo(token)`.
- **`When(...).{recurses into the same rules builder}`** — clean recursion point.
- **`And`/`Or`/`Not` on the completed condition are self-returning** (matches `GuardBuilder`).
- **`ClientValidationFieldToken.For(expr)`** — clean DRY factory.
- **The terse condition-operator vocabulary** is the *same* set conditions uses (one-engine law).

### Value spine + ReactiveArray
- **The terminal/continuation split is honest in the type** — chain ops return `ReactiveArray<T>`,
  folds return `ReactiveValue<T>`; "fold ends the chain" is unrepresentable (`ReactiveArray.cs:28-102`).
- **`Select<TResult>` re-types the element through the chain** — `cod(Select)=dom(Where over TResult)`.
- **`ReactiveValue<TValue>` carries no members of its own** — its whole role is being a
  `TypedSource<TValue>`; plugs into `SetText`/`When`/dispatch with zero new overloads.
- **`Sum`'s three CLR-typed terminals** (`int`/`decimal`/`double`) — one wire op, typed narrowing.
- **`From*` shares the HTTP `From` voice + `Dom` boundary suffix.**
- **`TypedSource<T>` exposes no public DSL edges and is the single typed authoring surface** (sacred).
- **`Count()` vs `Count(predicate)` / `Any()` vs `Any(predicate)`** — two honest intents, not
  redundant spellings; the predicate overload is a real ergonomic shortcut (A6, verified KEEP).

---

## 4. RECONCILIATION

Where each adjustment stands relative to the two upstream authorities — `09-dsl-naming-sheet.md`
(the locked spellings) and `08-determinism-formalization.md` (the design discoveries) — so nothing
is double-counted and every dependency is explicit.

### 4a. Already a settled rename in `09` (cited, NOT re-litigated as a grammar change)

These adjustments' **spelling** is already decided in the naming sheet; this doc keeps the *shape*
critique but does not re-coin the name. They are counted here once (as the shape decision they
carry) and must not be re-counted as fresh renames during implementation.

| Adj # | Settled in `09` | Note |
|-------|-----------------|------|
| 6 | `09 §1.8/§3.7` — `Plugin*` member-kind verbs | `Method→Function`, `Void→Command`; this doc adds the *return-family* disambiguation at the pipeline entry |
| 11 | `09 §1.8/§3.7` | `Method`/`Void` deletions are the rename; the inline↔subclass reconciliation is the shape note |
| 14 | `09:297-299,463` | `Min`/`Max`/`Average` are decided NEW edges |
| 19 | `09:341` | `.Reactive` "identical shape everywhere" — SmartTextArea conformance |
| 21 | `09:6` | one-concept-one-name on the selector param |
| 23 | `09:343` (Element `SetText`/`SetHtml` = **KEEP**) | 09 KEEPS Element `SetText`/`SetHtml`. `ComponentRef.SetText → SetDisplayText` is a **PENDING new proposal** (this doc, Adj #23) — NOT in 09. Ratify in 09 or keep pending; do not cite as settled. |
| 24 | `09:344` (`FocusIn`→`Focus` only) | `FocusIn`→`Focus` is decided in 09. `FocusOut`→`Blur` is a **PENDING new proposal** (Adj #24): `Blur` is NOT coined in 09 and no `Blur` mutation exists in source today. Ratify in 09 or keep pending. |
| 25 | `09 §3.7` | loader `SetAutoHide` / toast `SetDuration` decided |
| 26 | `09 §3.7` | `SetContent→SetMessage`, `FusionConfirmDialog→FusionConfirm` decided |
| 28 | `09 §3.5` | `.Class→CssClass`, `.Attr→Attribute` (template lane only) |
| 29 | `09 §3.5` | `EventButton→DispatchButton` decided |
| 30 | `09 §3.5/§4` | `When→WhenTemplate`, `ShowIf→ShowTemplateIf` decided |
| 35 | `09:339` | `IInputComponent`/`ValueMember` named; the merge-by-equality is `08:792-794` |
| 36 | `09 §3.7/§3.3` | `PluginMemberBuilder→PluginReadBuilder`; explicit terminal named `AsPluginSource()` by `§3.3` reasoning |
| 1 (A1/A2 value) | `09:294,293,451` | `AsSource→AsArraySource`, `Find→FindFirst` — pure renames folded into P7 |
| 38/39 | `09 §3.6` | `ClientValidation*→Client*` filler-drop; app-level trio aligned to the FluentValidator trio |

> **Settled renames that are NOT counted as §2 adjustments at all** (the critiques explicitly
> exclude them as "decided spelling, not a shape change"): `DomReady→PageLoad`,
> `CustomEvent→Event`, `DispatchWith→DispatchFrom`, `ValidationErrors→ShowValidationErrors`,
> `ReactionLane→ReactionTiming`, `WhenFieldGt/Gte/Lt/Lte→WhenFieldGreaterThan/…OrEqualTo/LessThan/…OrEqualTo`,
> `ClientValidation*→Client*`. They appear in §2 only where they *carry* a shape change (e.g. #4
> rides the `Then` rename, #32 rides `ShowValidationErrors`).

### 4b. Implements an `08` design discovery (the forcing functions)

| Adj # | `08` discovery | What it forces |
|-------|----------------|----------------|
| 2, 7, 8, 9 | **§6.3** — widen the value intake to abstract `TypedSource<T>` | gather `Include`/`Header`/`RouteParam`, component `SetDataSource`, pipeline `When`/`From`, and validation peer-comparisons all take the one spine so `cod(AsArraySource)=dom(intake)`. The single most cross-cutting discovery — appears in **every** cluster critique. |
| 13 | **§6.4** — one args-builder + one declaration spine for plugins | merge the two ~95%-identical read/call `Arg` surfaces into `PluginArgs<TSelf>` (anti-drift) |
| 10, 12 | **§6.4** (merge extensionally-equal morphisms) | the nine literal-arg spellings and the two arg-contract spellings collapse by equality |
| 5 | **§6.1 / §1.1 parity** | the missing nested-grouping And/Or is the same value-folding the algebra already lowers (`FieldCondition.All/Any/Not` absorbs both) |
| (C1, internal) | **§6.2** — stamp the lane, don't re-detect | `PipelineBuilder`'s draft stamps `ReactionTiming{Sync,Async}` so the runtime never probes `instanceof Promise`. **Internal lowering obligation, not an author-facing adjustment** — flagged so the pipeline builder is not implemented without it; not in the §2 count. |

### 4c. Cross-area reconciliation (the two shapes that MUST be identical everywhere)

The brief's explicit requirement — these two shapes are reconciled to a single form across all
clusters so the author learns each idea once:

- **The And/Or composition shape.** Conditions defines the canonical **two** shapes — flat
  `And(condition)` + nested-grouping `And(g => g…)` (`09 §1.1`, `ast-grammar-conditions.md:81-83`).
  - Validation (#5) **adds** the missing nested-grouping shape to reach exact parity.
  - Validation (#9) routes the flat shape's operand through the same `TypedSource` spine (#2).
  - The plugin `Args` grouping callback (#12) is the *same grouping-only* pattern: keep the
    callback only for what the flat chain cannot express (loop-built lists), exactly as `09 §1.1`
    keeps the nested And/Or only for grouping.
  - **Result:** one And/Or vocabulary — flat + nested — across Conditions, the FluentValidator side,
    and the app-level validation builder (`09:94-96`).

- **The `(args, p) => { }` callback shape.** The component `.Reactive` callback is canonical (P3).
  - Trigger typed-payload (#... folded into A2) reads the payload off `p.Payload` so the callback
    arity never changes — identical to `.Reactive`.
  - Pipeline `When`/`From` (#7/#8) and conditions/gather all read the payload via the **same**
    `FromEvent(args, path)` factory — so `When(args,…)`, `And(g=>…)`, `Include`, `Set`, `Dispatch`
    all spell a payload read one way.
  - Branch bodies (`Then`/`Else`), validation `When().Then()` (#4), and template
    `WhenTemplate().Else()` (#17) all hand back the same builder shape via the same `Then/Else`
    continuation.
  - **Result:** every callback that opens a pipeline or a branch looks alike — one shape to learn.

### 4d. Module-map (`10` §5) items — folded, not double-counted

The module-map's own 5 `BEFORE → AFTER` items are each a restatement of a cluster-critique item
and are merged into the single numbering in §2 (not counted twice):

| `10` §5 item | Folds into §2 |
|--------------|---------------|
| 5.1 `ElementBuilder` return-type split | **#3** |
| 5.2 `When` payload spellings collapse | **#7** |
| 5.3 widen `Include` to `TypedSource` | **#2** |
| 5.4 template `Button(onClick)` stringly de-default | **#18** (and #29 `EventButton`→`DispatchButton`) |
| 5.5 `FusionSmartTextArea.Reactive` normalize | **#19** |

`10` §4's verdict — **the 12-module cut is correct; no module added/removed/split/merged** — is
the structural ground these shape fixes sit on: all 39 are *within-module shape* or the *one
cross-module value-spine widening* (#2), never a re-cut of the module set.

---

## 5. THE TOP 5 (by writability impact)

1. **#1 — `Html.On` returns the plan, not `void`.** Restores TALL reading at the very entry
   point of every view; the whole authoring chain reads top-to-bottom in one expression.
2. **#2 — widen gather `Include`/`Header`/`RouteParam` (and every value intake) to abstract
   `TypedSource<T>`.** Closes the `08 §6.3` seam so `AsArraySource ⨾ Include` composes — a
   filtered/transformed array or a fold result can finally be gathered into a request. The
   single most cross-cutting fix.
3. **#3 — every `ElementBuilder` mutation returns `ElementBuilder` (one `Done()` terminator).**
   Kills the overload-sensitive return-type flip; Element now reads identically to `ComponentRef`.
4. **#4 — validation `When(cond).Then(body)` returning the rules builder.** Replaces the
   two-positional-lambda `void` call with the same `When().Then()` shape conditions uses, so
   sibling guarded blocks stack TALL.
5. **#5 — validation gains the nested-grouping And/Or shape.** Brings the And/Or composition to
   exact parity with conditions (`09 §1.1`), so grouped boolean activation logic reads inline and
   TALL instead of forcing named intermediate conditions.
