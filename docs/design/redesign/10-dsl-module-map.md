# 10 — DSL Module Map

> **What this is.** Every authoring builder type from the 8 AST grammar tables
> (`ast-grammar-*.md`) assigned to exactly one of the 12 micro-modules
> (`02-micro-modules.md`), the module-boundary **seam edges** read straight from the
> grammar (wherever a member's return type belongs to a different module than its
> receiver), and a PL-architect critique of where the grammar's *actual cuts* match
> the blueprint vs. suggest a re-cut.
>
> **Grounding rule.** Every type and every seam is cited to a grammar row with a
> `file:line` (the grammar tables already carry the source anchors). Every proposed
> adjustment is `BEFORE → AFTER` + the PL property it improves + the zero-feature-loss
> note. Names reconcile to the decided `09-dsl-naming-sheet.md`; the one capability
> widening reconciles to `08-determinism-formalization.md §6.3`.
>
> **Module count: 12** (9 vocabulary concept-slices + 3 shared spines: Shape, Kind,
> Plan). The grammar's authoring cuts match the blueprint's 12 modules — **no re-cut
> of the module set is needed.** Three *within-module* grammar warts and one
> *cross-module* capability hole are flagged with `BEFORE → AFTER` fixes that preserve
> every feature. Detail below.

---

## 1. Authoring-surface vs domain/runtime-only modules

The 12 modules split into two bands by **whether a developer ever names the module's
types while authoring a `.cshtml`**:

| Band | Modules | A developer types these? |
|------|---------|--------------------------|
| **Authoring-surface** (9) | Plan, Trigger, Reaction, Condition, Value, Request, Component, Validation, Plugin | Yes — every one owns at least one fluent builder a dev calls (`On`, `t.PageLoad`, `p.Element`, `p.When`, `When(source)`, `p.Get`, `Html.InputField`, `Field(...).Required(...)`, `p.Plugin(...)`). |
| **Domain/runtime-only** (3) | **Shape**, **Kind**, **Plan**\* | Mostly no. **Shape** and **Kind** are pure kernels — a dev never writes `Shape` or `kind`; they ride invisibly on every value/node. **Plan** is a hybrid: its *root verbs* (`Html.ReactivePlan`, `RenderPlan`) are authoring-surface, but its document/serialize/boot internals (`PlanDocument`, `ReactivePlanSerializer`, `root.ts`, `boot.ts`) are runtime-only. |

\* **Plan is a spine, not a pure kernel.** The blueprint files it with Shape/Kind as a
"shared spine," but unlike the two kernels it *does* expose a thin authoring surface
(`Html.ReactivePlan<TModel>()`, `Html.ResolvePlan`, `Html.RenderPlan`,
`ReactivePlan.RegisterPlugin`). So: **Shape + Kind are the only zero-authoring kernels;
Plan is the authoring root that also owns the runtime document spine.**

---

## 2. Module → authoring types → source folder → depends-on

`→` = C# authoring/plan side. `⇒` = TS runtime side. **Authoring types** lists only the
*public DSL builder/entry types* (the AST-grammar receivers/returns); internal plan-model
and runtime types are named in the source-folder column, not repeated here. Folders are
relative to the framework project root `Alis.Reactive/` (C#) and
`Alis.Reactive.Assets/runtime/` (TS); vendor builders live in `Alis.Reactive.Fusion/`
and `Alis.Reactive.Native/`.

| Module | Authoring builder types (from AST grammar) | Source folder (→ C# / ⇒ TS) | Depends on (via which seam edge) |
|--------|--------------------------------------------|------------------------------|----------------------------------|
| **Shape** *(kernel)* | *(none — no authoring type; rides on every value/operand/member)* | → `Builders/Conditions/TypedSource.cs` (`Shape`/`ElementShape` internal members) ⇒ `core/shape-convert.ts`, `domain/runtime-shape.ts` | — *(DAG bottom)* |
| **Kind** *(kernel)* | *(none — the `kind` discriminator + generated contract)* | → `Serialization/` (`ReactivePlanSerializer`, polymorphic converter, `PlanTypeGenerator`) ⇒ `core/assert-never.ts`, `types/plan.ts` *(generated)* | **Shape** |
| **Value** | `TypedSource<T>`, `PayloadTypedSource`, `TypedComponentSource<T>`, `TypedPluginSource<T>`, `TypedPluginPropertySource<T>`, `TypedUrlSource<T>`, `ReactiveArray<T>`, `ReactiveValue<T>`, `ResponseBody<T>` | → `Builders/Conditions/Typed*Source.cs`, `Builders/Arrays/` ⇒ `core/evaluate.ts`, `value/array-op-engine.ts`, `domain/runtime-value.ts`/`runtime-path.ts` | **Shape**, **Kind** |
| **Condition** | `ConditionStart<TModel>`, `ConditionSourceBuilder<TModel,TProp>`, `GuardBuilder<TModel>`, `BranchBuilder<TModel>` | → `Builders/Conditions/` ⇒ `conditions/compare-engine.ts`, `conditions/conditions.ts` | **Value** (`When(TypedSource)`), **Shape**, **Kind** |
| **Reaction** | `PipelineBuilder<TModel>`, `ElementBuilder<TModel>`, `DispatchPayloadBuilder<TPayload,TModel>`, `ComponentRef<TComponent,TModel>` *(handle; verbs owned by Component)* | → `Builders/PipelineBuilder*.cs`, `Builders/ElementBuilder.cs`, `Builders/DispatchPayloadBuilder.cs` ⇒ `execution/execute.ts` | **Value**, **Condition**, **Request**, **Slot**, **Component**, **Kind** |
| **Request** | `HttpRequestBuilder<TModel>`, `GatherBuilder<TModel>` (+`GatherExtensions`), `ResponseBuilder<TModel>`, `ParallelBuilder<TModel>` | → `Builders/Requests/` ⇒ `execution/http.ts`, `gather.ts`, `http-fetch.ts`, `request-payload-writer.ts` | **Value**, **Condition**, **Component**, **Shape**, **Kind** |
| **Trigger** | `TriggerBuilder<TModel>` (+ `Html.On` entry) | → `Builders/TriggerBuilder.cs`, `Razor/Extensions/HtmlExtensions.cs` ⇒ `execution/trigger.ts`, `server-push.ts`, `signalr.ts` | **Reaction** (every trigger callback → `PipelineBuilder`), **Component** (`ComponentEvent`), **Kind** |
| **Component** | `InputBoundField<TModel,TProp>`, all `*Builder` component builders + `.FusionXxx`/`.NativeXxx` + `.Reactive` + `.Fields` + per-slice `ComponentRef` verb extensions; app-level `ComponentRef` verbs (Drawer/Loader/Toast/Confirm) | → `InputField/`, `IdGenerator.cs`, `ComponentRef.cs`; vendor slices in `Alis.Reactive.Fusion/Components/`, `Alis.Reactive.Native/Components/`, `*/AppLevel/` ⇒ `resolution/`, `lifecycle/object-contracts.ts`, `components/{fusion,native}` | **Value** (`.Value()` → `TypedComponentSource`), **Shape**, **Kind** |
| **Slot** | *(no distinct authoring builder — `Inject` verb is authored on `PipelineBuilder`/Reaction; partial vs root is a `PlanScope` fact)* | → `PlanModel/` (`PlanScope`) ⇒ `lifecycle/inject.ts`, `applied-plans.ts`, `merge-policy.ts`, `component-merge.ts` | **Plan**, **Component** *(downward only)* |
| **Validation** | `ReactiveClientValidationBuilder`, `ClientValidationRulesBuilder<TModel>`, `ClientValidationFieldRuleBuilder<TModel,TValue>`, `ClientValidationConditionBuilder<TModel>`, `ClientValidationFieldConditionStart<TModel,TValue>`, `ClientValidationCondition<TModel>`, `ClientValidationFieldToken<TModel,TValue>` | → `Validation/` ⇒ `validation/orchestrator.ts`, `rule-engine.ts`, `error-display.ts`, `live-clear.ts`, `rule-operands.ts` | **Condition** (reuses `CompareEngine` for `WhenField`), **Component**, **Value**, **Plan**, **Kind** |
| **Plugin** | `PluginTypeBuilder`, `PluginArgumentTypes`, `Plugin` (subclass face), `PluginFunction<T>`, `PluginCommand`, `PluginProperty<T>`, `PluginMemberBuilder<T,TModel>`, `PluginCallBuilder<TModel>` | → `Plugin.cs`, `Builders/PluginTypeBuilder.cs`, `Builders/PluginMemberBuilder.cs`, `Builders/PluginArguments.cs` ⇒ `core/plugin-catalog.ts` | **Value**, **Component**, **Shape**, **Kind** |
| **Plan** *(spine + root)* | `ReactivePlan<TModel>` (+ `Html.ReactivePlan`/`ResolvePlan`/`RenderPlan` entries); `FusionTemplateBuilder<TModel>` + `FusionConditionalBuilder<TModel>` *(the render-time template grammar — see §5.4)* | → `ReactivePlan.cs`, `Razor/Extensions/PlanExtensions.cs`, `PlanModel/`, `Serialization/`; templates in `Alis.Reactive.Fusion/Templates/` ⇒ `root.ts`, `lifecycle/boot.ts` | **Trigger**, **Reaction**, **Component**, **Slot**, **Kind** |

---

## 3. The seam edges — read straight from the grammar

A **module-boundary edge** exists wherever a grammar row's **Return type** lives in a
different module than its **Receiver**. These are the *real* dependency seams (a callback
that hands back a builder from another module is the same kind of edge). Below, every seam
is one grammar row with its `file:line`. `cod(member) ∈ Module(B)` while `member` is
declared on `Module(A)` ⇒ edge `A → B`.

### 3.1 Plan → Trigger / Reaction / Component (root wiring)

| Seam edge | Grammar row (Receiver → Member → Return) | Source |
|-----------|------------------------------------------|--------|
| **Plan → Trigger** | `Html.On(plan, Action<TriggerBuilder<TModel>>)` → opens `TriggerBuilder` | `Razor/Extensions/HtmlExtensions.cs:53` |
| **Plan → Component** | `Html.InputField(plan, expr)` → `InputBoundField<TModel,TProp>` | `Razor/Extensions/InputFieldExtensions.cs:33` |
| **Plan → Plugin** | `ReactivePlan.RegisterPlugin(name, Action<PluginTypeBuilder>)` → `PluginTypeBuilder` | `ReactivePlan.cs:53` |

### 3.2 Trigger → Reaction (every trigger nests a pipeline)

Every `TriggerBuilder` method takes `Action<PipelineBuilder<TModel>>` (or
`Action<TPayload, PipelineBuilder<TModel>>`) — the callback hands back a `PipelineBuilder`,
the **Reaction** module's root. This is the single most-repeated seam in the grammar.

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Trigger → Reaction** | `TriggerBuilder.PageLoad(Action<PipelineBuilder>)` *(was `DomReady`, §3.1 naming)* | `Builders/TriggerBuilder.cs:26` |
| **Trigger → Reaction** | `TriggerBuilder.Event(name, Action<PipelineBuilder>)` *(was `CustomEvent`)* | `Builders/TriggerBuilder.cs:38,51` |
| **Trigger → Reaction** | `TriggerBuilder.ServerPush(...) / SignalR(...)` → `Action<…,PipelineBuilder>` | `Builders/TriggerBuilder.cs:67–126` |

### 3.3 Reaction → Component / Request / Condition / Value / Slot / Plugin

`PipelineBuilder` (Reaction) is the hub. Each entry verb returns a sub-builder owned by a
*different* module — the return type names the seam:

| Seam edge | Grammar row (Return type → owning module) | Source |
|-----------|-------------------------------------------|--------|
| **Reaction → Component** | `p.Element(id)` → `ElementBuilder` *(Reaction-owned leaf)*; `p.Component<T>(...)` → `ComponentRef<T,TModel>` *(Component)* | `Builders/PipelineBuilder.cs:92,101–136` |
| **Reaction → Request** | `p.Get/Post/Put/Delete(url)` → `HttpRequestBuilder`; `p.Parallel(...)` → `ParallelBuilder` | `Builders/PipelineBuilder.Http.cs:11–45` |
| **Reaction → Condition** | `p.When<TProp>(TypedSource<TProp>)` → `ConditionSourceBuilder`; `p.Confirm(msg)` → `GuardBuilder` | `Builders/PipelineBuilder.Conditions.cs:11–42` |
| **Reaction → Value** | `p.From<TElement>(TypedSource<TElement[]>)` → `ReactiveArray`; `p.FromUrl<T>(name)` → `TypedUrlSource` | `Builders/PipelineBuilder.Arrays.cs:15`; `Builders/PipelineBuilder.cs:147` |
| **Reaction → Plugin** | `p.Plugin<T>(name, member)` → `PluginMemberBuilder`; `p.Plugin(name)` → `PluginCallBuilder` | `Builders/PipelineBuilder.cs:164–253` |
| **Reaction → Slot** | `p.Inject(...)` *(Inject verb on the pipeline; slot identity = `SlotId`)* | `02-micro-modules.md:71` (Reaction owns `inject` reaction; Slot owns load/unload) |
| **Component/Element → Reaction** *(return edge)* | `ElementBuilder.AddClass/SetText(...)` → `PipelineBuilder` *(literal/event/response overloads return parent)* | `Builders/ElementBuilder.cs:31,55` |

### 3.4 Condition seams (the value spine + first-match routing)

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Condition → Value** | `ConditionStart.When<TProp>(TypedSource<TProp>)` → `ConditionSourceBuilder`; right-operand `Eq(TypedSource<TProp>)` reads a Value | `ConditionStart.cs:35`; `ConditionSourceBuilder.cs:112` |
| **Condition → Reaction** *(branch body)* | `GuardBuilder.Then(Action<PipelineBuilder>)` → `BranchBuilder`; `BranchBuilder.Else(Action<PipelineBuilder>)` → `void` | `GuardBuilder.cs:126`; `BranchBuilder.cs:61` |
| **ConditionSourceBuilder → Condition** | every operator (`Eq/Gt/Truthy/…`) → `GuardBuilder` *(within Condition, terminal-to-guard)* | `ConditionSourceBuilder.cs:49–122` |

### 3.5 Request seams (gather reads values; response nests pipelines)

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Request → Value** | `GatherBuilder.Include<…>(TypedComponentSource<TProp>)`, `Header<TProp>(TypedSource<TProp>)`, `RouteParam<TProp>(TypedSource<TProp>)`, `Plugin<T>(TypedPluginSource<T>)` | `GatherExtensions.cs:58,71`; `GatherBuilder.cs:81,131,207` |
| **Request → Component** | `GatherBuilder.Include<TComponent,TModel>(Expression<Func<TModel,object>>)` reads a component value | `GatherExtensions.cs:17,36` |
| **Request → Reaction** *(scope bodies)* | `ResponseBuilder.OnSuccess/OnError(Action<PipelineBuilder>)`, `HttpRequestBuilder.WhileLoading/Finally(Action<PipelineBuilder>)`, `ParallelBuilder.OnAllSettled(Action<PipelineBuilder>)` | `ResponseBuilder.cs:28–96`; `HttpRequestBuilder.cs:70,89`; `ParallelBuilder.cs:28` |
| **Request → Request** *(recursion)* | `ResponseBuilder.Chained(Action<HttpRequestBuilder>)` re-enters the HTTP grammar | `ResponseBuilder.cs:111` |
| **Request → Validation** | `HttpRequestBuilder.Validate<TValidationSource>(formId)` *(binds a client-rule set before submit)* | `HttpRequestBuilder.cs:103` |

### 3.6 Component seams (read becomes a value source)

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Component → Value** | `ComponentRef<T,TModel>.Value()` → `TypedComponentSource<string>` *(read terminal — feeds When/Gather/SetText)* | `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:150` *(canonical exemplar)* |
| **Component → Reaction** | `<Builder>.Reactive(plan, eventSelector, Action<TArgs, PipelineBuilder<TModel>>)` → the component-fires-event trigger nests a pipeline | `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListReactiveExtensions.cs:40` *(56 builders, uniform)* |
| **Component (InputField) → Component (builder)** | `InputBoundField.FusionXxx/NativeXxx(Action<XxxBuilder>)` → opens the vendor component builder | `ast-grammar-component-reactive.md:47–76` |

### 3.7 Validation seams (reuses the compare engine + reads peer fields)

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Validation → Condition** | `ClientValidationRulesBuilder.When(Func<ConditionBuilder, Condition>, define)` reuses the same compare semantics; `ClientValidationCondition.And/Or/Not` mirror `GuardBuilder` | `Validation/ClientValidationRulesBuilder.cs:34`; `ClientValidationConditionBuilder.cs:191–207` |
| **Validation → Component / Value** | `ClientValidationFieldRuleBuilder.EqualTo(Expression<Func<TModel,TValue>> peerField)` reads a peer field (a model-bound value) | `Validation/ClientValidationFieldRuleBuilder.cs:98` |
| **Validation → Validation** *(recursion)* | `ClientValidationRulesBuilder.When(…, Action<ClientValidationRulesBuilder>)` scopes nested rules | `Validation/ClientValidationRulesBuilder.cs:34` |

### 3.8 Plugin seams (args read values; read terminal is a value source)

| Seam edge | Grammar row | Source |
|-----------|-------------|--------|
| **Plugin → Value** | `PluginMemberBuilder.Arg<TArg>(TypedSource<TArg>)`, `Arg<TArgs,TProp>(args, path)` read values into the call | `Builders/PluginMemberBuilder.cs:72,64` |
| **Plugin → Value** *(read terminal)* | `PluginMemberBuilder` `implicit operator TypedPluginSource<TReturn>` — the read IS a Value source | `Builders/PluginMemberBuilder.cs:136` |
| **Plugin → Reaction** *(call terminal)* | `PluginCallBuilder.Fire()` → `void` emits the call reaction into the pipeline | `Builders/PluginMemberBuilder.cs:250` |

### 3.9 Slot → Plan (downward-only — breaks the old boot↔plans cycle)

| Seam edge | Status | Source |
|-----------|--------|--------|
| **Slot → Plan** | Composition recomposes a *new* `PlanDocument`; Plan does **not** depend back on Slot (boot calls slot injection through the Reaction `inject` handler, not by importing Slot) | `02-micro-modules.md:172–177`; `08-determinism-formalization.md:913` |

**Seam-cleanliness verdict (from `08 §4`).** Of the seams above, **3 fail clean composition
today** and are the design discoveries (all *within-module structure* or *one capability
hole*, not module-set re-cuts): `Value → Shape/Kind` (whole-payload sentinel, §6.1),
`Reaction/Plan → … D3` (lane re-detection + active-plan singleton, §6.2), and
`Request → Value` (`Include` intake too narrow, §6.3 — see §5.3 below). Every *other* seam
already composes cleanly.

---

## 4. Does the grammar's actual cut match the blueprint's 12 modules?

**Yes — the 12-module set is the right cut. No module is added, removed, split, or merged.**
The grammar's natural seams (every cross-module return type) land exactly on the blueprint's
module boundaries. Evidence, module by module:

- **The Reaction hub is real.** `PipelineBuilder` is the one receiver whose return types fan
  out to *six* other modules (Component, Request, Condition, Value, Plugin, Slot). The
  blueprint predicts exactly this dependency fan-out (`02:71`). ✅
- **The Value spine is real.** `TypedSource<T>` is the single intake type that Condition
  (`When`), Request (`Include`/`Header`/`RouteParam`), Plugin (`Arg`), Component (`Value()`
  return), and Reaction (`SetText`) all touch. One write path, one read path. ✅
- **The Component vendor seam is real and uniform.** 56 `.Reactive` builders + ~70
  `ComponentRef` verb-slices follow one identical shape (`ast-grammar-component-reactive.md:187`,
  `ast-grammar-element-component.md:208`). Adding a 71st component touches only its own slice
  — the blueprint's "sole vendor seam" holds. ✅
- **Trigger, Request, Condition, Validation, Plugin each own a self-contained builder family**
  with one entry edge and clean internal recursion — each is a textbook concept-slice. ✅
- **Shape + Kind never appear as authoring types** — confirming they are pure kernels, not
  concept-slices. ✅

**Three within-module grammar warts and one cross-module hole** remain (the grammar *cut* is
right; these are *shape* fixes inside or across the established modules). All preserve every
feature. They are detailed in §5.

> **One naming note that is NOT a re-cut:** the AST tables surface `FusionTemplateBuilder` /
> `FusionConditionalBuilder` (the SF render-time template grammar). The blueprint's 12
> modules do not name a "Template" module. This is correct — templates are a **Component/Plan
> render-time concern** (they emit an HTML string consumed by SF columns/templates, not a
> plan node), so they file under **Plan** (render surface) without minting a 13th module.
> See §5.4.

---

## 5. PL-architect critique — grounded `BEFORE → AFTER` adjustments

Each adjustment is cited to a grammar row, names the PL-architect property it improves, and
carries a zero-feature-loss note. Names already decided in `09` are used as the AFTER.

### 5.1 `ElementBuilder` return-type split is a least-surprise wart

**Grounded observation.** `ElementBuilder` has an asymmetric return contract
(`ast-grammar-element-component.md:44–57`):

```
SetText(string literal)               → PipelineBuilder   (ends element chain)   ElementBuilder.cs:55
SetText<TSource>(source, path)        → PipelineBuilder   (ends element chain)   ElementBuilder.cs:65
SetText<TProp>(TypedSource<TProp>)    → ElementBuilder    (chains!)              ElementBuilder.cs:87
```

The *same verb* `SetText` returns the **parent** for two overloads and **this** for the
third. A dev who writes `p.Element("x").SetText(lit).AddClass("y")` finds `AddClass`
resolving on `PipelineBuilder` (wrong receiver) for the literal overload but on
`ElementBuilder` for the typed-source overload — an overload-sensitive return type.

- **PL property hurt:** LEAST-SURPRISE + CONSISTENCY (an asymmetric return that forces a
  re-wrap is the canonical "wart"). TALL-READING also suffers — the chain breaks
  unpredictably mid-element.

```
BEFORE  Element(id).SetText(literal)            -> PipelineBuilder   (chain ends)
        Element(id).SetText(TypedSource)        -> ElementBuilder    (chain continues)
AFTER   Element(id).SetText(literal)            -> ElementBuilder    (chain continues)
        Element(id).SetText(TypedSource)        -> ElementBuilder    (chain continues)
        // every Element mutation returns ElementBuilder; the chain ends only when the
        // dev calls the next pipeline verb on `p` (the builder already holds `p`).
```

- **Property improved:** CONSISTENCY (one verb, one return), TALL-READING (uninterrupted
  vertical chain of element mutations), LEAST-SURPRISE.
- **Zero feature loss:** all three `SetText`/`SetHtml` value-source arities stay (literal /
  `(source, path)` / `TypedSource`) — only the *return type* is unified to `ElementBuilder`.
  `09 §3.1` already keeps the `Set` family as "one verb over distinct value-source arities —
  do not collapse." This fix is fully compatible: it unifies the **return**, not the
  overload set.

### 5.2 `When` entry has two redundant payload spellings (orthogonality)

**Grounded observation.** `PipelineBuilder.When` and `ConditionStart.When` each carry three
overloads (`PipelineBuilder.Conditions.cs:11–34`, `ConditionStart.cs:16–35`):

```
When<TPayload,TProp>(TPayload payload, Expression path)            // payload + path
When<TPayload,TProp>(ResponseBody<TPayload> body, Expression path) // responseBody + path
When<TProp>(TypedSource<TProp> source)                             // the flat source
```

The first two are *second spellings* of the third: a payload-read and a responseBody-read
each already yield a `TypedSource`. `09 §1.1` already made exactly this collapse for the
`And`/`Or` guard composition ("THREE shapes → TWO"). The `When` entry was not yet folded.

- **PL property hurt:** ORTHOGONALITY (one intent — "compare this value" — has three
  spellings) and CONSISTENCY (And/Or already collapsed; When did not).

```
BEFORE  When(payload, x => x.Prop) | When(responseBody, x => x.Prop) | When(TypedSource)
AFTER   When(TypedSource<TProp>)                       // the one flat shape
        // payload/responseBody reads fold in via TypedSource factories, exactly as
        // 09 §1.1 folds And/Or:  FromEvent(args, x => x.Prop)  /  responseBody.Read(path)
```

- **Property improved:** ORTHOGONALITY (one way to start a condition), CONSISTENCY
  (`When` now matches the already-decided `And`/`Or` two-shape grammar).
- **Zero feature loss:** payload-path and responseBody-path reads remain fully expressible —
  they become `TypedSource` factory calls (the same factories `09 §1.1` introduces for
  And/Or). Every condition a dev can write today is still writable. This generalises the
  decided And/Or collapse to its sibling entry point.

### 5.3 `Gather.Include` intake is too narrow — composability hole (reconciles `08 §6.3`)

**Grounded observation.** `Include` is typed to the *concrete* source families
(`GatherExtensions.cs:58,71` take `TypedComponentSource<TProp>`), but `ReactiveArray.AsArraySource()`
and `ReactiveValue<T>` yield the **abstract** `TypedSource<T>` (`ReactiveArray.cs:121`). So:

```
cod(AsArraySource) = TypedSource<T[]>   ⊄   dom(Include) = TypedComponentSource ⊎ TypedPluginSource
```

The morphism `AsArraySource ⨾ Include` does **not** compose — an array-op result cannot be
gathered into a request. This is `08 §6.3`'s flagged mis-cut seam (`08:911,1031`).

- **PL property hurt:** COMPOSABILITY (a callback's return type does not fit the next
  builder's intake — "seams where cod(f) does not fit dom(g) are bugs").

```
BEFORE  Include<TModel,TProp>(TypedComponentSource<TProp> source)        // concrete only
AFTER   Include<TModel,TProp>(TypedSource<TProp> source)                 // abstract intake
        // every concrete source already lowers to ValueExpression.Read/Invoke, so the
        // runtime reader is UNCHANGED. Closes the value-spine hole at the gather boundary.
```

- **Property improved:** COMPOSABILITY (the value spine becomes genuinely one-write-path for
  *all* readable values, including array-op folds), ORTHOGONALITY (gather reads through the
  same `TypedSource` as every other consumer).
- **Zero feature loss:** *widening* — every call that compiled before still compiles
  (`TypedComponentSource` IS a `TypedSource`), and `ReactiveValue`/array results become
  gatherable. Pure capability gain, exactly as `08 §6.3` specifies.

### 5.4 Template `Button`/`onClick` is a stringly escape hatch inside a typed builder (discoverability)

**Grounded observation.** `FusionTemplateBuilder` is otherwise a clean typed builder, but
two child verbs take raw JS strings (`FusionTemplateBuilder.cs:192,210`):

```
Button(string text, string onClick)                                  // raw JS handler string
ButtonFor<TProperty>(string text, Expression idProperty, string onClickFn)
```

while the *same builder* already offers the typed, plan-wired alternative `EventButton`
(`FusionTemplateBuilder.cs:245`) that dispatches a named event into the reactive pipeline:

```
EventButton<TProperty>(string text, string eventName, Expression idProperty)
```

Two ways to wire a button click — one stringly (`onClick`), one typed (`EventButton`). The
stringly pair is discoverable noise that invites Rule-2 ("no manual JS in views") violations.

- **PL property hurt:** DISCOVERABILITY + LEAST-SURPRISE (a dev finds `Button(text, onClick)`
  first and reaches for raw JS; the right answer `EventButton` is further down the list).

```
BEFORE  Button(text, onClick) / ButtonFor(text, id, onClickFn)   // raw JS handler strings
        EventButton(text, eventName, id)                          // typed, plan-wired
AFTER   EventButton(text, eventName, id)                          // the ONE button-click shape
        // raw-JS Button/ButtonFor retained ONLY where a non-reactive static template needs
        // a literal handler (documented as the deliberate escape hatch), never as the
        // first/default overload a dev meets.
```

- **Property improved:** DISCOVERABILITY (the typed `EventButton` is the obvious path),
  CONSISTENCY (template button clicks wire through the same `Dispatch`/event mechanism as the
  rest of the DSL — `09 §3.1` pairs listen `t.Event("x")` / emit `p.Dispatch("x")`).
- **Zero feature loss:** `Button`/`ButtonFor` are **kept** for the genuine static-template
  case (this template grammar emits an HTML string for SF columns where no plan exists);
  they are de-emphasised, not deleted. Every template a dev can render today still renders.

### 5.5 `FusionSmartTextArea.Reactive` breaks the `.Reactive` consistency contract (least-surprise)

**Grounded observation.** Every one of the 56 component `.Reactive` methods returns its own
builder so multiple events chain (`ast-grammar-component-reactive.md:187`,
`ReturnsSelf = yes`). The lone exception is `FusionSmartTextArea`, whose two `.Reactive`
overloads extend `ReactivePlan<TModel>` and return `void`
(`FusionSmartTextAreaReactiveExtensions.cs:11,22`):

```
// every other component:
<Builder>.Reactive(plan, evt => evt.X, (args, p) => ...) -> <Builder>   // chains
// SmartTextArea (exception):
ReactivePlan.Reactive(componentId, on, pipeline) -> void                 // does NOT chain
```

- **PL property hurt:** CONSISTENCY + LEAST-SURPRISE (same concept — wire a component event —
  has a different receiver and return shape for one component; a dev who learned the pattern
  on 56 components is surprised by the 57th).

```
BEFORE  ReactivePlan.Reactive(componentId, on, pipeline) -> void          // off-pattern
        ReactivePlan.Reactive(expr, on, pipeline)        -> void
AFTER   SmartTextAreaBuilder.Reactive(plan, on, pipeline) -> SmartTextAreaBuilder
        // route SmartTextArea through the same InputField -> builder -> .Reactive chain as
        // every other input component, so the receiver and return match the contract.
```

- **Property improved:** CONSISTENCY (one `.Reactive` shape across all components),
  DISCOVERABILITY (the dev finds it where every other component's `.Reactive` lives).
- **Zero feature loss:** both SmartTextArea event-wiring entry points (by id-string and by
  model expression) remain expressible through the unified builder; only the receiver/return
  is normalised. *(Flagged as a vendor-slice consistency item; the fix lives entirely in the
  SmartTextArea slice — no runtime change.)*

---

## 6. Summary

- **Module count: 12** — 9 authoring-surface concept-slices (Plan, Trigger, Reaction,
  Condition, Value, Request, Component, Validation, Plugin) + 3 spines, of which **Shape and
  Kind are pure zero-authoring kernels** and **Plan is the authoring root that also owns the
  runtime document/boot spine**.
- **No module-set re-cut needed.** Every cross-module return type in the 8 AST grammar tables
  lands on a blueprint module boundary. The Reaction hub (fan-out to 6 modules), the
  `TypedSource` Value spine, and the uniform Component vendor seam are all *confirmed by the
  grammar*, not just asserted by the blueprint.
- **Five grounded `BEFORE → AFTER` adjustments**, all feature-preserving, all citing a grammar
  row: (5.1) unify `ElementBuilder` return type [consistency], (5.2) collapse `When` payload
  spellings [orthogonality], (5.3) widen `Include` to `TypedSource` [composability — reconciles
  `08 §6.3`], (5.4) de-default the template's stringly `Button(onClick)` in favour of typed
  `EventButton` [discoverability], (5.5) normalise `FusionSmartTextArea.Reactive` to the
  builder-chained contract [least-surprise].
- The three seams that *fail clean composition today* (`08 §4`) are within-module structure or
  the one capability hole — **not** evidence for a different module cut. The cut is right; the
  shape needs the five fixes above.
