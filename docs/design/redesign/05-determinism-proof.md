# Determinism Proof — Public DSL Coverage of the Redesign Matrix

> **Claim under test:** the redesign determinism matrix
> (`04-matrix-triggers-reactions-conditions.md`,
> `04-matrix-http-arrays-values.md`,
> `04-matrix-validation-components-slots.md`) gives **100% deterministic coverage
> of the public DSL with good defaults** — every public authoring call lowers to
> exactly one plan-JSON shape and one browser behavior, and the choice made when
> the developer says nothing is the right one.
>
> **Method.** This document does not trust the matrix's own counts. It enumerates
> the actual public DSL surface from source — `Alis.Reactive/Builders/**`,
> `Alis.Reactive/Razor/Extensions/**`, the validator surface
> (`Alis.Reactive.FluentValidator/ReactiveValidator.cs`,
> `Alis.Reactive/Validation/**`), and the component `.Reactive()` / app-level
> surfaces (`Alis.Reactive.Native/**`, `Alis.Reactive.Fusion/**`) — and maps each
> public feature to a matrix case **by name**. Where a feature has no
> deterministic case, it is listed honestly as a gap.
>
> **Verdict (headline).** The matrix is sound and deterministic for **every
> public DSL feature family**: every value source, every condition operator, every
> HTTP feature, every array op, every validation rule, every component / plugin /
> app-level member, every trigger kind, **and both previously-missing reaction
> verbs** — `p.Into(elementId)` (the `inject` reaction) and
> `p.ValidationErrors(formId)` (the `show-validation-errors` reaction). Both now
> have dedicated, source-grounded matrix rows
> (`04-matrix-triggers-reactions-conditions.md`, the `inject` and
> `show-validation-errors` sub-bands), the slot band's authoring verb is named
> correctly as `p.Into` (the invented `InjectInto` is gone), and the component
> count is stated as the real ~60 slices. **Coverage of named public feature
> families is therefore 100%.**

---

## What changed since the prior (97%) proof

The prior revision of this document scored coverage at **97%** because the matrix
had two real public reaction verbs with **no authoring row**, named the inject
verb as a call that does not exist (`InjectInto`), and undercounted the component
surface (31 vs ~60). All three defects are now fixed **in the matrix itself** —
verified against source for this revision:

| Prior defect | Fix in the matrix (verified against source) |
|---|---|
| `p.Into(elementId)` had no row | `04-matrix-triggers-reactions-conditions.md` now has an **`inject` reaction sub-band** with a dedicated row pinning `p.Into` → `ReactionGraph.Inject(slot, ReadWholePayload(Success))` → `executeInject`. Source: `Builders/PipelineBuilder.cs:273-279`, `PlanModel/ReactionGraph.cs:424` (`kind:"inject"`), `execution/execute.ts:207`. |
| `p.ValidationErrors(formId)` had no row | The same matrix now has a **`show-validation-errors` reaction sub-band** pinning `p.ValidationErrors` → `ReactionGraph.ShowValidationErrors(container)` → `executeShowValidationErrors`. Source: `Builders/PipelineBuilder.cs:263-267`, `PlanModel/ReactionGraph.cs:443` (`kind:"show-validation-errors"`), `execution/execute.ts:220`. |
| Band C cited `InjectInto` (not in source) | `04-matrix-validation-components-slots.md` Band C now names the verb `p.Into(elementId)` and states explicitly *"There is no `InjectInto` and no `p.Slot(...)` verb in source"*; slot load/unload is a runtime mechanism (`loadPartialSlot`/`unloadPartialSlot`), not a separate C# verb. |
| Band B1 said "31 input components" | Band B1 now states **~60 component slices = 51 Fusion + 9 Native + 4 app-level**, and explicitly folds display/container slices (no `ValueMember`) under B2/B3/B4 instead of implying 31 inputs. Verified: `ls Alis.Reactive.Fusion/Components` = 51, `Alis.Reactive.Native/Components` = 9, AppLevel dirs = 4 (NativeDrawer, NativeLoader, FusionConfirm, FusionToast). |

Because the gaps were never *non-determinism* (each missing verb had exactly one
obvious lowering — proven below), closing them was mechanical, and the matrix now
satisfies the repo's **Coverage Completeness Gate**: every public DSL item in
scope is mapped to a matrix case **by name**.

---

## How coverage is scored

A public feature is **covered** when the matrix names an authoring call that
produces it AND fixes one output (plan node + browser behavior) AND states the
default. A feature is **partial** when the output is deterministic but the matrix
mis-states the authoring call or undercounts the surface (the lowering is still
mechanical). A feature is a **gap** when no row produces it from the real DSL.

"Good default?" is scored against the principle: the no-ceremony common case must
be the correct one.

---

## Band-by-band coverage table

### 1. Triggers — `TriggerBuilder<TModel>` (source: `Builders/TriggerBuilder.cs`)

| Public DSL call | Matrix case | Deterministic? | Good default? |
|---|---|---|---|
| `t.DomReady(p => …)` | Trigger band: **PageReady** | Yes | Yes — fire once on load, no payload |
| `t.CustomEvent(name, p => …)` | **CustomEvent (untyped)** | Yes | Yes — `payloadType: untyped` |
| `t.CustomEvent<T>(name, (e,p) => …)` | **CustomEvent (typed)** | Yes | Yes — `e` is shape-only phantom |
| `t.ServerPush(url, p => …)` | **ServerPush (any)** | Yes | Yes — any-event filter, untyped |
| `t.ServerPush(url, evt, p => …)` | **ServerPush (named)** | Yes | Yes — named filter |
| `t.ServerPush<T>(url, evt, (e,p) => …)` | **ServerPush (named, typed)** | Yes | Yes |
| `t.SignalR(hub, method, p => …)` | **SignalR** | Yes | Yes — untyped |
| `t.SignalR<T>(hub, method, (e,p) => …)` | **SignalR (typed)** | Yes | Yes |
| chained `t.DomReady(…).CustomEvent(…)` | **Multiple triggers** | Yes | Yes — each trigger independent behavior |
| component event via `.Reactive(...)` | **ComponentEvent** (also Band B3) | Yes | Yes — SYNC so SF `args.cancel` visible |

**Trigger band: 10/10 authoring entry points covered.** Verified the source has
exactly the trigger methods above (`TriggerBuilder.cs`: `DomReady`, `CustomEvent`
×2, `ServerPush` ×3, `SignalR` ×2, chained) and five `StartsWhen` kinds
(page-ready, document-event, server-push, signalr, component-event), payload axis
`{untyped, typed}` — matches the matrix's TriggerKind × PayloadContract
parameterization precisely.

### 2. Reactions — `PipelineBuilder<TModel>` (source: `Builders/PipelineBuilder*.cs`, `ElementBuilder.cs`, `DispatchPayloadBuilder.cs`)

| Public DSL call | Matrix case | Deterministic? | Good default? |
|---|---|---|---|
| single command / ordered sync / sync→async→sync | Reaction sequencing & lane (3 rows) | Yes | Yes — declaration order = execution order |
| `p.Element(id).Show()/.Hide()` | `set` element show/hide | Yes | Yes — `hidden:false/true`, no separate verb |
| `p.Element(id).SetText(literal)` | `set` set-text literal | Yes | Yes — `Shape.String` |
| `p.Element(id).SetText(source)` / `SetText(args,path)` / `SetText(body,path)` | `set` set-text from value source | Yes | Yes — one `ValueExpression` path |
| `p.Element(id).SetHtml(...)` (literal/event/source) | `set` set-html | Yes | Yes — property `html` |
| `p.Element(id).AddClass/RemoveClass/ToggleClass` | `call` element css class | Yes | Yes — calls, not sets |
| `p.Component<T>(expr).Set(...)` | `set` component property write | Yes | Yes — id from model expression |
| `p.Component<T>(...).Call(...)` (no-arg / args) | `call` component method | Yes | Yes — args via `ValueExpression`, `[]` default |
| event-arg `set` / `call` inside `.Reactive` | `set`/`call` payload scope `event` | Yes | Yes — SYNC mandatory |
| `p.Dispatch(name)` | `dispatch` no payload | Yes | Yes — empty `{}` detail |
| `p.Dispatch<T>(name, literal)` | `dispatch` literal payload | Yes | Yes |
| `p.DispatchWith<T>(name, b => …)` | `dispatch` source-backed object payload | Yes | Yes — object node, field = `ValueExpression` |
| `p.Plugin(name, member).Arg(...).Fire()` | `call` plugin command (also Band D) | Yes | Yes — call-only target |
| **`p.Into(elementId)`** | **`inject` reaction** sub-band row | **Yes** | Yes — value is always `ReadWholePayload(Success)` |
| **`p.ValidationErrors(formId)`** | **`show-validation-errors` reaction** sub-band row | **Yes** | Yes — container id required, no implicit fallback |

**Reaction band: 15/15 public verbs covered.** `p.Into` and `p.ValidationErrors`
now have dedicated matrix rows (`Builders/PipelineBuilder.cs:273` and `:263`
respectively) — the two former gaps are closed.

### 3. Conditions — `PipelineBuilder.Conditions.cs` + `Builders/Conditions/**`

| Public DSL call | Matrix case | Deterministic? | Good default? |
|---|---|---|---|
| `p.When(typedSource)` | Condition source: component/url/plugin | Yes | Yes — shape from `TProp` |
| `p.When(args, path)` | Condition source: event payload | Yes | Yes — scope `event` |
| `p.When(responseBody, path)` | Condition source: response body | Yes | Yes — scope from body |
| `.Eq/.NotEq/.Gt/.Gte/.Lt/.Lte(operand)` | Equality + Ordered families | Yes | Yes |
| `.Truthy/.Falsy/.IsNull/.NotNull/.IsEmpty/.NotEmpty()` | Unary family | Yes | Yes |
| `.In/.NotIn(values)` | Membership family | Yes | Yes |
| `.Between(lo,hi)` | Range family | Yes | Yes — inclusive |
| `.Contains/.StartsWith/.EndsWith(s)` | Text family | Yes | Yes |
| `.Matches(pattern)` | Regex family | Yes | Yes |
| `.MinLength(n)` | TextLength family | Yes | Yes |
| `.ArrayContains(item)` | CollectionItem family | Yes | Yes — `itemShape` set |
| `.Eq/.NotEq/.Gt/.Gte/.Lt/.Lte(typedSource)` | source-vs-source operand form | Yes | Yes |
| `.And/.Or` (chained + nested group), `.Not()` | Guard composition (6 rows) | Yes | Yes — flattened all/any |
| `.Then/.ElseIf/.Else` | Branch routing first-match (4 rows) | Yes | Yes — Else last, only one |
| `p.Confirm(message).Then(…)` | Confirm guard (async opener) | Yes | Yes — confirm is a guard term |

**Condition band: all 21 operator tokens + 3 source kinds + composition + branch +
confirm covered.** Verified `ConditionSourceBuilder.cs` exposes exactly the 21
tokens the matrix's 9 families enumerate (`Eq`/`NotEq`/`Gt`/`Gte`/`Lt`/`Lte`,
`Truthy`/`Falsy`/`IsNull`/`NotNull`/`IsEmpty`/`NotEmpty`, `In`/`NotIn`, `Between`,
`Contains`/`StartsWith`/`EndsWith`, `Matches`, `MinLength`, `ArrayContains`), plus
the 6 source-vs-source overloads (`Eq`/`NotEq`/`Gt`/`Gte`/`Lt`/`Lte(TypedSource)`).

### 4. Values — `TypedSource<T>` family (source: `Builders/Conditions/Typed*Source.cs`, `Builders/Arrays/ReactiveValue.cs`)

| Public value source | Matrix case (Part A) | Deterministic? | Good default? |
|---|---|---|---|
| literal scalar / null / arbitrary | A.1 literals (3) | Yes | Yes — ISO date, `Shape.None` for null, `any` fallback |
| component property / method read | A.2 read component (2) | Yes | Yes — shape from `TProp`, `args:[]` |
| plugin method / property read | A.2 read plugin (2) | Yes | Yes — unknown plugin = boundary throw |
| `p.FromUrl(name)` / `p.FromUrl<T>(name)` | A.2 read URL (2) | Yes | Yes — `Shape.String` default, typed coerces |
| payload read (event/success/error/request/dispatch) | A.2 read payload | Yes | Yes — path parsed, scope from creation site |
| whole payload / whole element | A.2 whole-read variants | Yes | Yes — explicit variant, no magic member |
| `p.FromDom(id, member)` | A.2 read DOM member | Yes | Yes — id plan-carried |
| `DispatchWith` object / array value | A.3 composites (2) | Yes | Yes — closed object, homogeneous-item array |

**Value band: 15/15 covered.** One write path (`ValueExpression`), one read path
(`evaluateValue`).

### 5. HTTP — `PipelineBuilder.Http.cs`, `Builders/Requests/**`

| Public DSL call | Matrix case (Part B) | Deterministic? | Good default? |
|---|---|---|---|
| `p.Get/Post/Put/Delete(url)` + inline `Post/Put(url, gather)` | B.1 verbs (3) | Yes | Yes — body only when fields present |
| URL `{placeholder}` + route param | B.2 endpoint/template | Yes | Yes — every placeholder required |
| `g.Include` (×4) / `Static` / `FromEvent` / `FromUrl` (×4) / `Plugin` / `Header` (×3) / `RouteParam` (×5) / `IncludeAll` / bodiless | B.3 gather ×P-TARGET (11) | Yes | Yes — param name = property; scalar-only header/route |
| body egress (scalar/array → json/query/form) | B.4 writer (4) | Yes | Yes — `""`→`null`, files force form-data |
| `OnSuccess` (untyped/typed) / `OnError` (any/status/typed/typed+status) | B.5 response routes (4) | Yes | Yes — match `any` default, first match wins |
| `WhileLoading` / `Finally` / `Validate<T>(formId)` | B.6 loading/finally (3) | Yes | Yes — finally always runs; no validate = always sends |
| `Chained` / `Parallel(...).OnAllSettled(...)` | B.7 chained+parallel (2) | Yes | Yes — chain on success only; all branches start |

**HTTP band: 28/28 covered.** Verified `PipelineBuilder.Http.cs` exposes
`Get`/`Post`/`Post(url,gather)`/`Put(url,gather)`/`Delete`/`Parallel`, and
`GatherBuilder`/`GatherExtensions` expose `Include` ×4, `Static`, `FromEvent`,
`Header` ×3, `RouteParam` ×5, `FromUrl` ×4, `Plugin`, `IncludeAll`. The only async
lane in this band.

### 6. Arrays — `PipelineBuilder.Arrays.cs`, `Builders/Arrays/**`

| Public DSL call | Matrix case (Part C) | Deterministic? | Good default? |
|---|---|---|---|
| `p.From(source)` / `From(args, sel)` / `FromDom(id, member)` (×2 overloads) | entries (3, source has 4 overloads) | Yes | Yes — element type carried |
| `.Count()` / `.Count(pred)` | count ×2 (predicated = filter+count) | Yes | Yes |
| `.Where` / `.Select` / `.Sum` | filter / map / sum | Yes | Yes — sum projection optional |
| `.Any()` / `.Any(pred)` / `.All(pred)` | any ×2 / all | Yes | Yes — `Any()` = non-empty |
| `.Find(pred)` / `.Find(pred, proj)` | find ×2 | Yes | Yes — `null` when none |
| `.OrderBy` / `.OrderByDescending` | orderBy | Yes | Yes — ascending default, non-scalar key = compile error |
| chained ops / `.AsSource()` | chaining + terminal (2) | Yes | Yes |

**Array band: 16/16 covered.** Verified `PipelineBuilder.Arrays.cs` exposes
`From<TElement>(TypedSource<TElement[]>)`, `From<TArgs,TElement>(args, selector)`,
`FromDom(id, member)` and the typed `FromDom<TElement>` overload. Pure sync;
predicates are the sync condition subset only (no confirm).

### 7. Validation — `ReactiveValidator<T>`, `Validation/ClientValidationFieldRuleBuilder.cs`, `ValidationTerms.cs`

| Public DSL surface | Matrix case (Band A) | Deterministic? | Good default? |
|---|---|---|---|
| 18 `ClientRule(...)` rule methods → 18 `ValidationRuleName` tokens | A1 (18 rule types) | Yes | Yes — `activation:always`, empty-passes for non-Required |
| peer overloads (`EqualTo(m=>...)`, ordered peers) | A2 peer comparison (6) | Yes | Yes — peer read via same Value spine |
| 19 `WhenFieldX` + `WhenFields` | A3 (~22 forms + composition) | Yes | Yes — `WhenField` no value = Truthy |
| `ClientRuleEach`, nested `ClientRule(child)`, server errors, inline span, summary | A4 (5 surfaces) | Yes | Yes — collection binding is a value object |

**Validation band: covered.** Verified exactly **18** `ValidationRuleName` tokens
in `ValidationTerms.cs` (required, empty, minLength, maxLength, email, regex, url,
creditCard, range, exclusiveRange, min, max, gt, lt, equalTo, notEqual,
notEqualTo, atLeastOne) — matrix A1's "18" is exact — and **19** distinct
`WhenFieldX` family methods plus `WhenFields` composition (matrix A3's "~22 forms"
generously covers, incl. value overloads). A4's "Server validation errors" row now
correctly states that `p.ValidationErrors(formId)` **is** a distinct plan node (the
`show-validation-errors` Reaction-band node), not "no new plan node" — the prior
mischaracterization is fixed.

### 8. Components — Native + Fusion slices + `.Reactive()` (source: `Alis.Reactive.Native/**`, `Alis.Reactive.Fusion/**`)

| Public DSL surface | Matrix case (Band B) | Deterministic? | Good default? |
|---|---|---|---|
| `Html.InputField(...).<Native/Fusion input>(...)` rendering + registration | B1 (3 variants) | Yes | Yes — same id per expression regardless of vendor |
| `p.Component<T>(...).SetX/CallX/ReadX` | B2 (5 mutation/read variants) | Yes | Yes — sync lane, one Value spine |
| `.Reactive(e => e.Event, (args,p) => …)` | B3 (2 event-wiring variants) | Yes | Yes — vendor seam in one place |
| Fusion Grid render / DataStateChange / mutation / inline validation | B4 (4 surfaces) | Yes | Yes — no grid-specific plan node |

**Component band: covered, count now accurate.** Verified the source has **51
Fusion component slices** (`Alis.Reactive.Fusion/Components/*`) + **9 Native**
(`Alis.Reactive.Native/Components/*`) + **4 app-level** (NativeDrawer,
NativeLoader, FusionConfirm, FusionToast) = **~60 slices**. Band B1 now states this
explicitly and folds display/container slices (no `ValueMember` — Breadcrumb,
BulletChart, Carousel, ContextMenu, Kanban, ListView, Menu, PivotView, Schedule,
Sidebar, Stepper, Toolbar, Grid, Accordion, Tab, Tooltip, Dialog, etc.) under
B2/B3/B4 rather than implying 31 inputs. The determinism argument holds regardless
of count: every slice reduces to `set`/`call`/`read` nodes (B2) or `componentEvent`
behaviors (B3), and B4 proves the largest display component (Grid) needs no new
node kind. The stale "31" count is removed.

### 9. Slots / Composition — `PlanExtensions.cs`, `ReactionGraph.InjectReaction`

| Public DSL surface | Matrix case (Band C) | Deterministic? | Good default? |
|---|---|---|---|
| `Html.ReactivePlan<M>()` + `RenderPlan` | C: Root view plan | Yes | Yes — one root plan per model |
| `Html.ResolvePlan<M>()` (same model) | C: Same-model partial (SSR join) | Yes | Yes — same `PlanId` ⇒ auto merge |
| independent-model partial | C: Independent-model partial | Yes | Yes |
| browser slot load / unload via `p.Get(url).Into(slot)` (recompose) | C: browser load / unload | Yes | Yes — abort scopes slot wiring |

**Slot band: covered, authoring verb now named correctly.** Band C's load/unload
rows now name the real authoring verb `p.Into(elementId)`
(`Builders/PipelineBuilder.cs:273`) and state explicitly that **there is no
`InjectInto` and no `p.Slot(...)` verb in source** — slot load/unload is a runtime
mechanism (`loadPartialSlot`/`unloadPartialSlot` in `lifecycle/boot.ts`) that
`injectHtml` invokes automatically based on whether the injected success HTML
carries `<script data-reactive-plan>` elements. The invented `InjectInto` call is
gone.

### 10. Plugins — `PipelineBuilder.cs` plugin methods, `PluginReadBuilder.cs`, `PluginCallBuilder.cs`

| Public DSL call | Matrix case (Band D) | Deterministic? | Good default? |
|---|---|---|---|
| `class X : Plugin { ... }` / inline declaration | D: declare a plugin | Yes | Yes — `Function`=value, `Command`=void |
| `p.Plugin<T>(name, member).Arg(...)` / `PluginProperty<T>` | D: read property/function | Yes | Yes — read = source like any other |
| `p.Plugin(name, member).Arg(...).Fire()` | D: call a command | Yes | Yes — one terminal `.Fire()` |
| `.Arg(typedSource)` / `.Arg(args,path)` / `ArgValue<T>` | D: arg from any value source | Yes | Yes — one Value spine |

**Plugin band: 4/4 covered.** Verified `Plugin<T>(name, member)`, `Plugin<T>(name)`,
`PluginProperty<T>`, `Plugin<T>(PluginFunction<T>)`, `Plugin<T>(PluginProperty<T>)`,
`Plugin(name, member)`, `Plugin(name)`, `Plugin(PluginCommand)` all funnel into
read/call nodes with typed args.

### 11. App-Level Objects — `Native/AppLevel/**`, `Fusion/AppLevel/**`

| Public DSL surface | Matrix case (Band E) | Deterministic? | Good default? |
|---|---|---|---|
| `@Html.NativeDrawer()`; `p.Component<NativeDrawer>().Open/Close/SetSize` | E: Drawer | Yes | Yes — fixed id `alis-drawer`, sync |
| `@Html.NativeLoader()`; `Show/Hide/SetTarget/SetTimeout` | E: Loader | Yes | Yes — fixed id, sync |
| `@Html.FusionConfirmDialog()`; `SetContent/Show/Hide` | E: Confirm | Yes | Yes — fixed id; also a Condition guard |
| `@Html.FusionToast()`; `Success/Warning/Danger/Info/Show/Hide/SetTitle/SetContent/...` | E: Toast | Yes | Yes — enum args → literals |
| `@Html.NativeActionLink(...)` single request | E: ActionLink | Yes | Yes — one request per link (analyzer-enforced) |

**App-level band: 5/5 covered.** Verified the 4 app-level singleton dirs
(NativeDrawer, NativeLoader, FusionConfirm, FusionToast) plus the NativeActionLink
component slice that drives a single request. Each exposes its members as
`set`/`call` on a fixed-id layout-object.

---

## Overall coverage

### By named public feature family

| Band | Public feature families | Covered | Partial (deterministic, stale/misnamed) | Gap |
|---|---|---|---|---|
| Triggers | 10 | 10 | 0 | 0 |
| Reactions | 15 | 15 | 0 | 0 |
| Conditions | 15 | 15 | 0 | 0 |
| Values | 15 | 15 | 0 | 0 |
| HTTP | 28 | 28 | 0 | 0 |
| Arrays | 16 | 16 | 0 | 0 |
| Validation | 4 surfaces (18+6+~22+5) | 4 | 0 | 0 |
| Components | 4 | 4 | 0 | 0 |
| Slots | 4 | 4 | 0 | 0 |
| Plugins | 4 | 4 | 0 | 0 |
| App-level | 5 | 5 | 0 | 0 |

**Totals (counting the discrete cells above): 120 named public feature families /
variants. Covered cleanly: 120. Partial-but-deterministic: 0. True gaps: 0.**

- **Clean deterministic coverage: 120/120 = 100%.**
- **Deterministic coverage (clean + partial): 120/120 = 100%.**
- **True gaps (no deterministic authoring row at all): 0/120 = 0%.**

### Verdict on the headline claim

The claim of **"100% deterministic coverage of the public DSL with good defaults"
is MET.** Every public authoring call enumerated from source
(`Builders/**`, `Razor/Extensions/**`, the validator surface, and the
component/app-level surfaces) maps to exactly one matrix case **by name**, each
case fixes exactly one plan-JSON shape and one browser behavior, and each states a
good default for the no-ceremony common case. The parameterization model
(template × finite axes ⇒ one lowering, one reader) is sound for every band, which
is the design's strongest property: **one input → one lowering → one reader, so
generation is mechanical.**

This 100% is **earned, not asserted**: it became true only after the matrix added
the two missing reaction rows (`p.Into`, `p.ValidationErrors`), corrected the slot
band's authoring verb to the real `p.Into` (deleting the invented `InjectInto`),
and stated the real ~60-slice component count with the explicit display/container
fold. Each of those was a *completeness* defect in the matrix — not a
*non-determinism* — so closing them was mechanical and is now verified against
source.

---

## What remains "could not make fully deterministic" — and why none of it is a public-DSL gap

Both matrices honestly flag a small number of edges under "Cases I could NOT make
fully deterministic." **None of these is a public-DSL coverage gap** — each is
either a representation choice on a wire enum with **no public authoring producer**,
or a generator/constant **mechanism** the redesign must build. They are recorded
so the code generator does not paper over them, but they do not lower public
feature-family coverage below 100%.

### Representation choices with no public authoring verb (triggers/reactions/conditions band)

1. **`set` target on a `plugin` source.** `CallTargetSource` includes `plugin` but
   `SetTargetSource` does not. There is **no `Plugin(...).Set(...)` verb in source**,
   so there is no authored intent to lower — the `set×plugin` tuple is correctly
   *excluded* from generation rather than invented. Not a coverage gap (no public
   feature is uncovered); a wire-enum scope decision.
2. **`dom`-kind vs `component`-kind source for element *mutation*.** `ElementBuilder`
   lowers every element mutation to a `component` source; the `dom` *read* variant
   is reachable from the Value band (`p.FromDom`) and is covered there (A.2 read DOM).
   No element-*write* authoring verb produces `dom`, so there is no missing row.
3. **`payload` scope `local` on the wire enum.** No public authoring call emits
   `local`; it is dead vocabulary the Request band's scope-fold deletes. Removing a
   dead enum member is a serialization cleanup, not a missing feature.

### Generator/constant mechanisms the redesign must build (validation/components/slots band)

4. **Validation rule-name narrowing (18 C# tokens → 6 runtime wire families).** The
   mapping is deterministic *by design* once `PlanContractGenerator` derives the TS
   union + narrowing from the C# `RuleName` × operand cross-product and
   `ContractDriftGate` guards it. Every public rule **is** covered (A1/A2); this is a
   contract-generation mechanism, not a missing authoring row.
5. **App-level object fixed ids.** Each app-level id (`alis-drawer`, etc.) must be
   **one shared constant** (C# const projected to TS) so the plan `target` and DOM
   `getElementById` agree by construction. Every app-level object **is** covered (E);
   this is a shared-constant mechanism, not a missing authoring row.
6. **`ClientRule` under FluentValidation server `When`.** Enforced today by a runtime
   throw in the authoring API; the redesign should lift it into the type system
   (unrepresentable, like `Standalone.Then`). The public client path (`WhenField`) **is**
   covered (A3); this is a make-invalid-unrepresentable hardening, not a missing row.

These six are exactly the kind of edge the design's "make invalid states
unrepresentable in C#" and "the contract is generated, never hand-mirrored"
principles already own. They are tracked as **mechanism work**, and they do not
affect the public-DSL feature-family coverage, which is **100%**.

---

## What this means for code generation

The redesign's central thesis — *one deterministic input → one lowering → one
runtime reader, so generation is mechanical* — **holds for all 120 public feature
families.** A developer (or an LLM) opens a module spec, sees the exact authoring
surface, the single node family, the camelCase wire shape, the runtime reader, and
the good default, and types the obvious body. There are no unwritten rows for
existing verbs and no invented calls. The matrix is now a true generator spec:
every public DSL feature has a deterministic row with a good default.
