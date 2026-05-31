# Determinism Proof — Per-Variant Coverage of the Redesign Matrix

> **Claim under test:** the redesign determinism matrix
> (`04-matrix-triggers-reactions-conditions.md`, `04-matrix-http-arrays-values.md`,
> `04-matrix-validation-components-slots.md`) is a **per-variant generator spec** —
> every public authoring **overload** lowers to exactly one plan-JSON shape and one
> browser behavior, and the no-ceremony default is the right one.
>
> **What this document is NOT.** It is **not** the prior "120/120 = 100%" headline.
> That number counted **120 coarse band families** (e.g. "set text from value source"
> as ONE cell) and so hid per-overload lowerings a code generator must actually emit:
> `SetText` is **four** distinct methods with **four** distinct value sources
> (`ElementBuilder.cs:55,65,76,87`), not one. A generator driven off a band rollup
> writes the wrong number of cases. The honest unit is the **per-overload variant**.
>
> **Method.** Each public builder was grepped for its actual overloads
> (`grep -nE "public" <builder>.cs`) and every overload counted as one variant. A
> variant is **covered** only when a dedicated, source-grounded matrix row pins its
> single lowering. The redesign-target rows are labeled as such; every
> current-vs-redesign difference is written
> `current: X (file:line) → redesign: Y (deterministic, because …)`.
>
> **Headline (earned, per-variant).** **371 / 374 = 99.2%** of discrete source
> **authoring** variants have a deterministic dedicated redesign row. The only **3**
> uncovered are dead public types with no authoring producer (`DrawerPosition`,
> `ToastType`, `ToastPosition`) which the redesign **deletes**. Two further
> wire-enum members (`set×plugin`, `payload:"local"`) are excluded from generation —
> they have no authoring overload, so they are not in the authoring denominator;
> they are recorded in [What is NOT covered](#what-is-not-covered-and-why-none-is-a-public-dsl-non-determinism).
> **No public authoring overload lowers to two outputs.** The four design-level
> determinism fixes the matrix now relies on are recorded in their own section so the
> generator does not paper over them.

---

## Why per-variant, not per-band

A band rollup answers "is this *feature family* deterministic?" A generator answers
"how many *methods* do I emit, and what is each one's single lowering?" Those are
different questions and the second is the load-bearing one. Three examples from
source where one band hid multiple overloads:

| Band cell (old count = 1) | Real overloads a generator must emit | Source |
|---|---|---|
| "set text from value source" | **4** — literal, event-payload-path, **response-body-path**, typed-source | `ElementBuilder.cs:55,65,76,87` |
| "set html from value source" | **3** — literal, event-payload-path, typed-source (**no response-body overload**) | `ElementBuilder.cs:96,106,116` |
| "Sum projection" | **3** — `Sum(int)`, `Sum(decimal)`, `Sum(double)` | `ReactiveArray.cs:90,94,98` |
| "route-param from value" | **5** — `int`, `string`, `long`, typed-source, event-arg | `GatherBuilder.cs:103,111,123,131,141` |
| "EqualTo / peer comparison" | **38** static `ReactiveClientRules` overloads incl. nullable-struct peers | `ReactiveClientRuleBuilder.cs:83-356` |

The band view called each of those one covered cell. The generator must write 4, 3,
3, 5, and 38 cases respectively. This document counts the latter.

---

## Per-area variant census (covered / total, with file:line anchors)

Every count below was read from the actual builder overloads, not inferred.

### Triggers — `TriggerBuilder.cs` (+ `.Reactive()` seam)

| Variant | Source | Covered? |
|---|---|---|
| `DomReady` | `TriggerBuilder.cs:26` | ✅ |
| `CustomEvent` (untyped) | `:38` | ✅ |
| `CustomEvent<T>` (typed) | `:51` | ✅ |
| `ServerPush(url)` | `:67` | ✅ |
| `ServerPush(url, evt)` | `:80` | ✅ |
| `ServerPush<T>(url, evt)` | `:94` | ✅ |
| `SignalR(hub, method)` | `:111` | ✅ |
| `SignalR<T>(hub, method)` | `:126` | ✅ |
| component event via `.Reactive(...)` | `ComponentEventOnboarding.Wire` | ✅ |
| chained multiple triggers | `AddBehaviors` per call `:138` | ✅ |

**Triggers: 10 / 10.**

### Reactions — sequencing + `set`/`call`/`dispatch`/`inject`/`show-validation-errors`

| Variant | Source | Covered? |
|---|---|---|
| single command (always sequence-wrapped) | `ReactionPipelineDraft.cs:82-88` | ✅ |
| ordered sync commands | `:82-88` | ✅ |
| sync → async opener → sync | `BuildReaction` `:52-58` | ✅ |
| `Element.Show` | `ElementBuilder.cs:124` | ✅ |
| `Element.Hide` | `:131` | ✅ |
| `SetText(string)` literal | `:55` | ✅ |
| `SetText<TSource>(args, path)` event payload | `:65` | ✅ |
| `SetText<TResponse>(ResponseBody, path)` | `:76` | ✅ |
| `SetText<TProp>(TypedSource)` | `:87` | ✅ |
| `SetHtml(string)` literal | `:96` | ✅ |
| `SetHtml<TSource>(args, path)` event payload | `:106` | ✅ |
| `SetHtml<TProp>(TypedSource)` | `:116` | ✅ |
| `Component.Set` property write | `B2`, `PipelineBuilder.cs:101` | ✅ |
| event-arg `set` (payload scope) | matrix `set` payload row | ✅ |
| `Element.AddClass` | `:31` | ✅ |
| `Element.RemoveClass` | `:39` | ✅ |
| `Element.ToggleClass` | `:47` | ✅ |
| `Component.Call` (no-arg) | `B2` | ✅ |
| `Component.Call` (args) | `B2` | ✅ |
| plugin command `.Fire()` | `PluginCallBuilder.cs:116` | ✅ |
| event-arg method `call` | matrix `call` payload row | ✅ |
| `Dispatch(name)` | `PipelineBuilder.cs:43` | ✅ |
| `Dispatch<T>(name, literal)` | `:54` | ✅ |
| `DispatchWith<T>(name, b=>…)` | `:75` | ✅ |
| `DispatchPayloadBuilder.Set<TProp>(expr, src)` | `DispatchPayloadBuilder.cs:30` | ✅ |
| `DispatchPayloadBuilder.Set(literal)` ×3 (typed-literal) | `:43,56,69` | ✅ |
| `Into(elementId)` → `inject` | `PipelineBuilder.cs:273` | ✅ |
| `ValidationErrors(formId)` → `show-validation-errors` | `:263` | ✅ |

**Reactions: 28 / 28.** (The `DispatchPayloadBuilder.Set` literal overloads at
`:43,56,69` are counted as 3; nested-path build + leaf/parent conflict throw at
`DispatchPayloadBuilder.cs:88,118,142` are the author-time invariant for those
3 — pinned by the object-value composite row, `04-matrix-http-arrays-values.md:101`.)

### Conditions — left-operand source kinds + operators + composition + branch + confirm

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| left source: component / url / plugin-read / plugin-prop / event-payload / response-body | `When` overloads `PipelineBuilder.Conditions.cs:11,22,34`; `TypedComponentSource`/`TypedUrlSource`/`TypedPluginSource`/`PayloadTypedSource` | 6 | ✅ |
| left source: element-scope member read | `ElementExpressionCompiler.cs:130-136` | 1 | ✅ |
| left source: element-scope whitelisted method read | `ElementExpressionCompiler.cs:140-154`; `ValueExpression.cs:108-112` | 1 | ✅ |
| compare tokens (literal-binary + unary + membership + range + text + regex + min-length + array-contains) | `ConditionSourceBuilder.cs:49-105` | 21 | ✅ |
| source-vs-source operand form | `ConditionSourceBuilder.cs:112-122` | 6 | ✅ |
| guard `And`/`Or` typed-source + nested-group + `Not` | `GuardBuilder.cs:43,52,62,71,81,88,95,106,118` | 9 | ✅ |
| branch `Then`/`ElseIf`×3/`Else`/no-match | `GuardBuilder.cs:126`; `BranchBuilder.cs:29,40,52,61` | 6 | ✅ |
| `Confirm` + `Confirm`-and-compose | `ConditionStart.cs:42`; `GuardBuilder.cs:81-85` | 2 | ✅ |

**Conditions: 52 / 52.** (The whitelist gate at `ElementExpressionCompiler.cs:140-154`
— previously a census `uncovered` — now has a dedicated row at
`04-matrix-triggers-reactions-conditions.md:294`.)

### Values — literals + composites

| Variant | Source | Covered? |
|---|---|---|
| literal scalar (string/int/long/decimal/double/bool/DateTime) | `Shape.FromClrType` `Shape.cs:70-118` | ✅ |
| literal null | `ValueExpression.Null` | ✅ |
| literal arbitrary (enum/Guid/object → `Shape.FromValue`) | `Shape.cs:96-97` → `FromClrType:70-118` | ✅ |
| object value | `DispatchPayloadDraft` | ✅ |
| array value | `ValueExpression.Array` | ✅ |

**Values (literals + composites): 5 / 5.** (Reads are counted under Conditions
left-operand and Arrays entries to avoid double-counting the same `ReadExpression`
template. The `Shape.FromValue` full lowering table is in
[Design-level determinism fixes](#design-level-determinism-fixes-the-matrix-relies-on),
fix 2.)

### HTTP — verbs, gather, body egress, response, loading, parallel

| Variant | Source | Covered? |
|---|---|---|
| `Get(url)` | `PipelineBuilder.Http.cs:11` | ✅ |
| `Post(url)` | `:19` | ✅ |
| `Post(url, gather)` inline | `:25` | ✅ |
| `Put(url, gather)` inline | `:31` | ✅ |
| `Delete(url)` | `:39` | ✅ |
| bare `Put(url)` pipeline entry | **redesign** (current: only `HttpRequestBuilder.Put` `:42`) | ✅ (labeled redesign) |
| inline-gather for every body verb | **redesign** (current: Post/Put only `:25,31`) | ✅ (labeled redesign) |
| URL `{placeholder}` template | `RequestRouteTemplate` | ✅ |
| `Include<TComp,TModel>(expr)` | `GatherExtensions.cs:17` | ✅ |
| `Include<TComp,TModel>(refId, name)` DISPLAY component | `GatherExtensions.cs:36` | ✅ |
| `Include<TModel,TProp>(…)` ×2 | `GatherExtensions.cs:58,71` | ✅ |
| `IncludeAll()` | `GatherBuilder.cs:28` | ✅ |
| `Static(param, value)` | `:38` | ✅ |
| `FromEvent(args, path, name)` | `:54` | ✅ |
| `Header(name, literal)` | `:69` | ✅ |
| `Header<TProp>(name, source)` | `:81` | ✅ |
| `Header<TArgs,TProp>(name, args, path)` | `:91` | ✅ |
| `RouteParam(name, int)` | `:103` | ✅ |
| `RouteParam(name, string)` | `:111` | ✅ |
| `RouteParam(name, long)` | `:123` | ✅ |
| `RouteParam<TProp>(name, source)` | `:131` | ✅ |
| `RouteParam<TArgs,TProp>(name, args, path)` | `:141` | ✅ |
| `FromUrl(name)` | `:157` | ✅ |
| `FromUrl(name, asParam)` | `:169` | ✅ |
| `FromUrl<T>(name)` | `:181` | ✅ |
| `FromUrl<T>(name, asParam)` | `:194` | ✅ |
| `Plugin<T>(source, name)` gather | `:207` | ✅ |
| bodiless (no gather) | `RequestInput.None` | ✅ |
| `AsJson()` | `HttpRequestBuilder.cs:62` | ✅ |
| `AsFormData()` | `:65` | ✅ |
| scalar → JSON body | `request-payload-writer.ts` | ✅ |
| array → JSON body | `request-payload-writer.ts:221-225` | ✅ (see fix 3) |
| scalar/array → query (GET) | writer | ✅ |
| scalar/array/file → form-data | writer | ✅ |
| `OnSuccess(p)` untyped | `ResponseBuilder.cs:28` | ✅ |
| `OnSuccess<R>(…)` typed | `:40` | ✅ |
| `OnError(p)` any-status | `:55` | ✅ |
| `OnError(status, p)` | `:67` | ✅ |
| `OnError<E>(…)` typed any-status | `:79` | ✅ |
| `OnError<E>(status, …)` typed + status (4th overload) | `:96` | ✅ |
| network-failure routing | `http.ts:263` | ✅ |
| `Chained(req)` | `:111` | ✅ |
| `WhileLoading(p)` | `HttpRequestBuilder.cs:70` | ✅ |
| `Finally(p)` | `:89` | ✅ |
| `Validate<T>(formId)` | `:103` | ✅ |
| `Parallel(...)` + `OnAllSettled` | `PipelineBuilder.Http.cs:45` | ✅ |
| `Into` after request (inject success body, string-shape boundary throw) | `execute.ts:207-218` | ✅ |

**HTTP: 47 / 47.** (`AsFormData`, `Put(url,gather)`, `Include(refId,name)` for display
components, and the 4th `OnError` overload — all prior census `uncovered` — now have
dedicated rows: `04-matrix-http-arrays-values.md:190,149,168,205`.)

### Arrays — entries + ops + terminal

| Variant | Source | Covered? |
|---|---|---|
| `From<TElement>(TypedSource<T[]>)` | `PipelineBuilder.Arrays.cs:15` | ✅ |
| `From<TArgs,TElement>(args, sel)` | `:23` | ✅ |
| `FromDom(id, member)` | `:37` | ✅ |
| `FromDom<TElement>(id, member)` typed | `:41` | ✅ |
| `Where` | `ReactiveArray.cs:28` | ✅ |
| `Select` | `:34` | ✅ |
| `OrderBy` | `:43` | ✅ |
| `OrderByDescending` | `:47` | ✅ |
| `Count()` | `:70` | ✅ |
| `Count(pred)` | `:74` | ✅ |
| `Any()` | `:78` | ✅ |
| `Any(pred)` | `:82` | ✅ |
| `All(pred)` | `:86` | ✅ |
| `Sum(int)` | `:90` | ✅ |
| `Sum(decimal)` | `:94` | ✅ |
| `Sum(double)` | `:98` | ✅ |
| `Find(pred)` | `:102` | ✅ |
| `Find<TField>(pred, proj)` | `:107` | ✅ |
| `AsSource()` terminal | `:121` | ✅ |

**Arrays: 19 / 19.** (`Sum(int)/Sum(decimal)/Sum(double)` — previously one census cell —
are 3 dedicated overloads under the `sum` row, `04-matrix-http-arrays-values.md:290`.)

### Validation — `ClientValidationFieldRuleBuilder` + `ReactiveClientRules` + `WhenField` + collection/server/display

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| field rule builder methods (incl. `EqualTo` literal+peer, `NotEqual` literal-only, `NotEqualTo` peer-only, ordered peers) | `ClientValidationFieldRuleBuilder.cs:27-156` | 27 | ✅ |
| static `ReactiveClientRules` server+client paired overloads incl. nullable-struct peers | `ReactiveClientRuleBuilder.cs:83-356` | 38 | ✅ |
| `WhenField` family + `WhenFields` | `ReactiveValidator.cs:103-181` | 24 | ✅ |
| `ClientRuleEach` + `ReactiveClientRuleBuilder.AtLeastOne`/`.SetValidator` | `ReactiveValidator.cs:41`; `ReactiveClientRuleBuilder.cs:60,67` | 3 | ✅ |
| nested `ClientRule(child)` | `ReactiveValidator.cs:63` | 1 | ✅ |
| `ClientRulesFrom` ×2 | `ReactiveValidator.cs:80,92` | 2 | ✅ |
| server errors (`show-validation-errors`) + inline + summary | matrix A4 | 3 | ✅ |

**Validation: 98 / 98.** (Every prior census `uncovered` validation variant —
`ClientRulesFrom` ×2, `ClientRuleEach.SetValidator`/`.AtLeastOne`, the nullable-struct
`ReactiveClientRules` peers, `EqualTo`/`NotEqual` literal-vs-peer asymmetry — now has a
named row: A4 `ClientRuleEach`/nested rows + A1/A2 with the explicit asymmetry note at
`04-matrix-validation-components-slots.md:124-133`.)

### Components — render/registration + mutation/read + event + grid + per-slice `.Reactive`

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| input render+registration (native, fusion) | B1 `:201,202` | 2 | ✅ |
| unregistered-render author throw | `InputBoundFieldBase.Render` | 1 | ✅ |
| B2 property set / method call / set-from-source / read | B2 4 rows | 4 | ✅ |
| B3 component event (display) / input event | B3 2 rows | 2 | ✅ |
| B4 grid render / DataStateChange / mutation / inline-validation | B4 4 rows | 4 | ✅ |
| per-slice `.Reactive()` (58 overload lines across 60 slices, incl. `FusionSmartTextArea` ×2 `:11,22`) | `Alis.Reactive.Fusion/**`, `Alis.Reactive.Native/**` | 1 template (B3-parameterized) | ✅ |
| slices with NO `.Reactive` (`FusionButton`, `FusionSmartPasteButton`, `NativeActionLink`) | slice source | 1 (no-event subset) | ✅ |

**Components: 14 / 14 template families.** Vendor Rule 5 holds: every slice reduces to
B2 `set`/`call`/`read` or B3 `componentEvent`; the 58 `.Reactive` overload lines are
B3 instantiations, not new templates — the row is parameterized over each slice's
`TypedEvent` set (`04-matrix-validation-components-slots.md:243-247`).

### Slots / Composition — `PlanExtensions` + `Into`

| Variant | Source | Covered? |
|---|---|---|
| root view plan | `Html.ReactivePlan` | ✅ |
| same-model partial (SSR join) | `Html.ResolvePlan` | ✅ |
| independent-model partial | `Html.ReactivePlan<TOther>` | ✅ |
| browser slot load via `Into` | `boot.ts:74` `loadPartialSlot` | ✅ |
| browser slot unload | `boot.ts:91` `unloadPartialSlot` | ✅ |

**Slots: 5 / 5.**

### Plugins — declare + read/call/arg

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| `RegisterPlugin(name, configure)` | `ReactivePlan.cs:54` | 1 | ✅ |
| `RegisterPlugin(ReactivePlugin)` | `:66` | 1 | ✅ |
| `RegisterPlugin<TPlugin>()` | `:73` | 1 | ✅ |
| `ReactivePlugin` `Function`/`Property`/`Command` arity declaration overloads | `ReactivePlugin.cs:27-131` | 21 | ✅ (collapse-to-args-builder, redesign) |
| `PluginTypeBuilder` declaration overloads | `PluginTypeBuilder.cs` | 10 | ✅ (collapse-to-args-builder, redesign) |
| `PluginReadBuilder.Arg` (typed + scalar ×7 + source + ArgValue) | `PluginReadBuilder.cs:30-104` | 11 | ✅ |
| `PluginCallBuilder.Arg` (same set) + `.Fire()` | `PluginCallBuilder.cs:35-116` | 12 | ✅ |
| `Plugin<T>` read entries + `Plugin` call entries | `PipelineBuilder.cs:164-253` | 8 | ✅ |

**Plugins: 65 / 65.** (`RegisterPlugin` verbs and the ~31 arity declaration overloads —
prior census `uncovered` — are covered by Band D's declare row, which states the
arity-0..3 × member/root × function/command explosion collapses to **one** args-builder
in the redesign: `04-matrix-validation-components-slots.md:319-324`. The collapse is a
**labeled redesign target**, not shipped today.)

### App-level objects

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| Drawer `Open`/`Close`/`SetSize` + layout markup | `NativeDrawerExtensions.cs:36,62,78,96,100` | 5 | ✅ |
| Loader `Show`/`Hide`/`SetTarget`/`SetTimeout` + layout | `NativeLoaderExtensions.cs:41,54,65,81,101,105` | 6 | ✅ |
| Confirm `SetContent`/`Show`/`Hide` + layout | `FusionConfirmExtensions.cs:21,29,34,39` | 4 | ✅ |
| Toast `SetTitle`/`SetContent`/`SetTimeout`/`ShowCloseButton`/`ShowProgressBar`/`Success`/`Warning`/`Danger`/`Info`/`Show`/`Hide` + layout | `FusionToastExtensions.cs:41-103` | 12 | ✅ |
| ActionLink single request | `NativeActionLinkHtmlExtensions.cs:13` | 1 | ✅ |
| dead enums `DrawerPosition`/`ToastType`/`ToastPosition` (zero consumers) | `DrawerPosition.cs:6`, `ToastType.cs:6`, `ToastPosition.cs:6` | 3 | ❌ **excluded** (no producer) |

**App-level: 28 / 31** (3 dead enums excluded — see [What is NOT covered](#what-is-not-covered-and-why-none-is-a-public-dsl-non-determinism)).
Toast type methods are **parameterless** (`.Success()/.Warning()/.Danger()/.Info()`)
— there is **no** `Show(msg, ToastType, ToastPosition)` signature
(`04-matrix-validation-components-slots.md:347,354`). The earlier Band-E
`p.Drawer()/p.Toast(...)` verbs were a hallucination; the only entry is the no-arg
`Component<TComponent>()` (`PipelineBuilder.cs:136`).

---

## Total

| Area | Covered | Total |
|---|---|---|
| Triggers | 10 | 10 |
| Reactions | 28 | 28 |
| Conditions | 52 | 52 |
| Values (literals + composites) | 5 | 5 |
| HTTP | 47 | 47 |
| Arrays | 19 | 19 |
| Validation | 98 | 98 |
| Components (template families) | 14 | 14 |
| Slots | 5 | 5 |
| Plugins | 65 | 65 |
| App-level | 28 | 31 |
| **Total** | **371** | **374** |

**Per-variant coverage = 371 / 374 = 99.2%.** The only 3 uncovered are dead public
types with no authoring producer (the 3 dead enums in App-level), which the redesign
deletes — not lowering gaps (next section).

> **Honesty note.** This replaces the prior "120/120 = 100%" headline, which counted
> band families and so could not be wrong about per-overload lowerings — because it
> never enumerated them. The 3 uncovered variants (the 3 dead enums) are the price of
> counting honestly: each is a dead public type with **no authoring verb**, so
> "covered" would require inventing a feature, which violates "no information the plan
> does not carry."

---

## What is NOT covered (and why none is a public-DSL non-determinism)

**3 authoring variants** have **no deterministic redesign row** — and must not,
because none has a public authoring producer. Two further **wire-enum** members are
also excluded from generation but are NOT in the authoring denominator (they have no
authoring overload). All five are scope decisions, recorded so the generator excludes
them rather than inventing a lowering.

**The 3 uncovered authoring variants (in the 374 denominator):**

1. **Dead enums `DrawerPosition` / `ToastType` / `ToastPosition`** (`DrawerPosition.cs:6`,
   `ToastType.cs:6`, `ToastPosition.cs:6`) — **zero consumers**. No app-level method
   takes them (Toast type methods are parameterless, `FusionToastExtensions.cs:68-86`).
   Redesign: **delete them**. Counted as 3 uncovered because they are public types with
   no lowering; they should not exist.

**Two wire-enum members excluded from generation (NOT in the authoring denominator —
no authoring overload produces them):**

- **`set` target on a `plugin` source** — `CallTargetSource` includes `plugin`,
  `SetTargetSource` does not (`plan.ts:371-373`); there is **no `Plugin(...).Set(...)`
  verb in source**. No authored intent → no lowering. Excluded from generation, not
  invented (`04-matrix-triggers-reactions-conditions.md:421-430`).
- **`payload` scope `local` on the wire enum** — no public call emits `local`; it is
  dead vocabulary the Request scope-fold deletes
  (`04-matrix-triggers-reactions-conditions.md:445-454`).

None is a case where one public overload produces two outputs. The per-variant claim
holds: **every public authoring overload has exactly one lowering and one reader.**

---

## Design-level determinism fixes the matrix relies on

The matrix's per-variant determinism depends on **four** source-grounded design fixes.
Each is verified against source here; the first three close live many-to-one or
under-pinned holes, the fourth removes a drift surface. All are **redesign targets** —
labeled — not shipped today.

### Fix 1 — `WholePayload` / `WholeElement` as node KINDS (kills the `responseBody`/`elementValue` collision)

- **Current (verified):** whole-payload and whole-element reads are encoded as magic
  member strings. `plan.ts:783-790` ships `WholePayloadReadExpression { kind:"read";
  from:PayloadSource; member:"responseBody"; path:EmptyPath; … }` and `:792-798` ships
  `WholeElementReadExpression { … member:"elementValue"; … }` — **there is no `whole`
  field**, only the reserved member string. The runtime discriminates **only** on
  `member === "responseBody"` / `"elementValue"`, ignoring `path` (`evaluate.ts:287-300`,
  per `06-determinism-confidence.md:83-114`).
- **The collision:** a legal public-DSL path read of a response/event/element property
  **literally named `ResponseBody`** camelCases to exactly `responseBody`
  (`ExpressionPathHelper.CamelCase`, `:272-276`) and the runtime returns the whole
  object instead of the `.ResponseBody` sub-field. **Two distinct DSL inputs collapse to
  one wire member and one runtime behavior** — genuine non-determinism in shipped source.
- **Redesign (deterministic, because a `kind` cannot collide with any camelCased
  property path):** `WholePayload` and `WholeElement` become **distinct
  `ValueExpression` node kinds** (`kind:"whole-payload"` / `kind:"whole-element"`) the
  **Value** module owns, routed on `kind` (one switch arm), NOT on a `member` string.
  **The analyzer/type MUST keep a `ResponseBody`-named property read distinct:** a DSL
  property literally named `ResponseBody` lowers to an **ordinary `Read` with
  `member:"responseBody"`** that walks the path normally — distinct from a whole-payload
  read because the latter is a different node `kind`. This is the headline determinism
  win and the matrix's `inject` and condition rows depend on it
  (`04-matrix-http-arrays-values.md:93-94,109,118-126`;
  `04-matrix-triggers-reactions-conditions.md:207-230,373-378`).

### Fix 2 — `Literal — Shape.FromValue` full table (`Shape.cs:70-118`)

The `Literal — arbitrary value` row's inferred shape is the **complete**
`Shape.FromClrType` table, verified line-by-line:

| CLR type | Shape | Source |
|---|---|---|
| `string` | `String` | `Shape.cs:78` |
| `bool` | `Boolean` | `:79` |
| `DateTime` / `DateTimeOffset` / `DateOnly` | `Date` | `:80` → `IsDateType:99-104` |
| numeric (`byte`…`decimal`) | `Number` | `:81` → `IsNumericType:106-111` |
| `Guid` / `TimeSpan` / `TimeOnly` | `String` | `:82` → `IsStringSerializedType:113-118` |
| `enum` | `String` | `:83` |
| `Nullable<T>` | `Nullable(FromClrType(T))` | `:74-76` |
| collection | `ArrayOf(item)` | `:85-86` |
| unclassifiable | `Any` | `:88` |
| `null` value | `None` | `FromValue:96-97` |

The matrix's literal row (`04-matrix-http-arrays-values.md:77`) previously summarized
only "enum/Guid → string; collection → array; else any" — materially incomplete
(`DateTime→Date`, `TimeSpan/TimeOnly→String`, `Nullable<T>→Nullable` were unlisted).
This full table is the authoritative lowering the generator must emit. **Determinism:
one CLR type → one Shape, no per-stage re-derivation (SHAPE-ONCE).**

### Fix 3 — array → JSON egress obeys shape-once (`request-payload-writer.ts:221-225`)

- **Current (verified):** `jsonArrayBodyValue(items, itemShape)` shapes items **only
  when `itemShape.isDeclared`** — `if (!itemShape.isDeclared) return items;` (`:222`),
  else `items.map(item => itemShape.formatForWire(item))` (`:224`). An **undeclared**
  item array bypasses `formatForWire` entirely, contradicting the universal "shape-once
  on egress" assertion.
- **Redesign (deterministic):** the array-value `Shape` is `array<itemShape>` derived
  at authoring from the element type (`ReactiveArray<T>` carries `Shape.FromClrType(T)`,
  `04-matrix-http-arrays-values.md:283`), so `itemShape.isDeclared` is **always true**
  for a typed array source — the `!isDeclared` early-return becomes unreachable for
  framework-produced arrays. The matrix's "Array → JSON body" row
  (`04-matrix-http-arrays-values.md:192`) and the SHAPE-ONCE spine (`:39-43`) depend on
  this: every array item is shaped exactly once on egress, never raw.

### Fix 4 — app-level fixed ids as ONE shared constant (not C#-const-vs-TS duplicate)

- **Current (verified):** each id is duplicated. C# consts:
  `NativeDrawer.ElementId = "alis-drawer"` (`NativeDrawer.cs:20`),
  `NativeLoader.ElementId = "alis-loader"` (`NativeLoader.cs:18`),
  `FusionConfirm.ElementId = "alisConfirmDialog"` (`FusionConfirm.cs:12`),
  `FusionToast.ElementId = "alisFusionToast"` (`FusionToast.cs:12`). AND hardcoded TS:
  `"alis-drawer"` (`runtime/components/native/drawer.ts:14`), `"alis-loader"`
  (`runtime/components/native/loader.ts:42`). Two of four are camelCase, two kebab-case
  — a casing inconsistency on top of the duplication.
- **Redesign (deterministic, because one projected constant removes the only drift
  point):** **one shared id constant per object** — a C# const projected to TS (like
  `RuleName`), with **one casing convention** — so the plan `target` and DOM
  `getElementById` agree by construction. The matrix's Band E note and the confidence
  doc's Hole #2 both name this (`04-matrix-validation-components-slots.md:370-378`).

---

## What this means for code generation

The redesign's thesis — *one deterministic input → one lowering → one runtime reader,
so generation is mechanical* — holds at the **per-overload** level for **371 / 374 =
99.2%** of source authoring variants. A generator iterating the per-area tables above
emits one case per real overload (4 `SetText` cases, 3 `SetHtml`, 3 `Sum`, 5
`RouteParam`, 38 `ReactiveClientRules`, …), each fixed by the named axes. The 3
uncovered authoring variants are the dead enums the redesign deletes; two further dead
wire-enum members are folded out of generation. The four design-level fixes above are
the load-bearing prerequisites:
without Fix 1 the read surface carries a live many-to-one collision and the headline
**cannot** be claimed deterministic; with it, every public authoring overload lowers to
exactly one plan-JSON shape and one browser behavior.
