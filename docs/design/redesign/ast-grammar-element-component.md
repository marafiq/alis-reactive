# element-component

DSL grammar (AST edges) for the **Element + Component** cluster, extracted from
REAL public C# builder signatures. Every row is a public method with a
`file:line`. Paths are relative to the repo root
(`Alis.Reactive/` is the framework project; `Alis.Reactive.Fusion/` is the
Fusion vendor package).

## How to read the table

`| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |`

- **Callback** — the callback param type if the member nests a sub-builder
  (`Action<PipelineBuilder>`, `Action<TArgs,PipelineBuilder>`, `Func<...>`),
  else `-`. **A callback handing back a `PipelineBuilder` is a NESTING
  (recursion) point.**
- **ReturnsSelf** — `yes` if the member returns its own receiver type, so it can
  be **chained / repeated** (multiple `.Element` mutations, multiple `.Reactive`
  events). `no` means the member returns a *different* builder, ending this
  receiver's chain and starting the next.

## Cluster entry edges (`PipelineBuilder<TModel>`)

These two members are the cluster's grammar **entry points** — the only public
way to obtain an `ElementBuilder` or a `ComponentRef`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `PipelineBuilder<TModel>` | `Element(string elementId)` | `ElementBuilder<TModel>` | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:92 |
| `PipelineBuilder<TModel>` | `Component<TComponent>(Expression<Func<TModel, object>> expr)` | `ComponentRef<TComponent, TModel>` | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:101 |
| `PipelineBuilder<TModel>` | `Component<TComponent, TOtherModel>(Expression<Func<TOtherModel, object>> expr)` | `ComponentRef<TComponent, TModel>` | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:114 |
| `PipelineBuilder<TModel>` | `Component<TComponent>(string refId)` | `ComponentRef<TComponent, TModel>` | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:127 |
| `PipelineBuilder<TModel>` | `Component<TComponent>()` *(app-level: `TComponent : IAppLevelComponent`)* | `ComponentRef<TComponent, TModel>` | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:136 |

## ElementBuilder\<TModel\>

Obtained from `p.Element("id")`. Every public member is a leaf DOM mutation.
Note the **return-type split**: literal / event-payload / response-body overloads
return the **parent `PipelineBuilder`** (ending the element chain), while the
`TypedSource<TProp>` overloads of `SetText`/`SetHtml` return **`this`
ElementBuilder** (so a typed-source set is chainable with further element
mutations). All members declared in `Alis.Reactive/Builders/ElementBuilder.cs`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ElementBuilder<TModel>` | `AddClass(string className)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:31 |
| `ElementBuilder<TModel>` | `RemoveClass(string className)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:39 |
| `ElementBuilder<TModel>` | `ToggleClass(string className)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:47 |
| `ElementBuilder<TModel>` | `SetText(string text)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:55 |
| `ElementBuilder<TModel>` | `SetText<TSource>(TSource source, Expression<Func<TSource, object>> path)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:65 |
| `ElementBuilder<TModel>` | `SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object>> path)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:76 |
| `ElementBuilder<TModel>` | `SetText<TProp>(TypedSource<TProp> source)` | `ElementBuilder<TModel>` | - | yes | Alis.Reactive/Builders/ElementBuilder.cs:87 |
| `ElementBuilder<TModel>` | `SetHtml(string html)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:96 |
| `ElementBuilder<TModel>` | `SetHtml<TSource>(TSource source, Expression<Func<TSource, object>> path)` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:106 |
| `ElementBuilder<TModel>` | `SetHtml<TProp>(TypedSource<TProp> source)` | `ElementBuilder<TModel>` | - | yes | Alis.Reactive/Builders/ElementBuilder.cs:116 |
| `ElementBuilder<TModel>` | `Show()` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:124 |
| `ElementBuilder<TModel>` | `Hide()` | `PipelineBuilder<TModel>` | - | no | Alis.Reactive/Builders/ElementBuilder.cs:131 |

## ComponentRef\<TComponent, TModel\>

Obtained from `p.Component<T>(...)`. **The type itself has ZERO public members** —
`EmitSet`, `EmitCall`, and `Read` are all `internal` plumbing
(`Alis.Reactive/ComponentRef.cs:32,45,68`), used only by vendor extension
methods. The named cluster verbs (`SetValue`, `SetDataSource`, `DataBind`,
`Focus`, `Value`, …) are **public extension methods** defined per component
slice in `Alis.Reactive.Fusion/Components/<Component>/<Component>Extensions.cs`
(and the Native equivalents). They are NOT generic across all components — each
is typed to its own `ComponentRef<TComponent, TModel>`.

Below are the **grounded representative edges** taken from the canonical
exemplar slices. Every mutation extension returns `ComponentRef<TComponent,
TModel>` (`ReturnsSelf = yes`, chainable/repeatable); every read extension
returns a `TypedComponentSource<TValue>` (ends the component chain, feeds a
condition/gather/source). The receiver column shows the concrete component the
signature is bound to.

### `SetValue` (set selected/committed value)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `SetValue<TModel>(string? value)` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:49 |
| `ComponentRef<FusionDropDownList, TModel>` | `SetValue<TModel>(string? value)` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:46 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `SetValue<TModel>(string? value)` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:39 |
| `ComponentRef<FusionInputMask, TModel>` | `SetValue<TModel>(string value)` | `ComponentRef<FusionInputMask, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskExtensions.cs:26 |

### `SetText` (set displayed text without changing value)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `SetText<TModel>(string text)` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:57 |
| `ComponentRef<FusionDropDownList, TModel>` | `SetText<TModel>(string text)` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:54 |

### `SetDataSource` (replace data source — 3 source-overloads per slice)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `SetDataSource<TModel, TSource>(TSource source, Expression<Func<TSource, object?>> path)` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:68 |
| `ComponentRef<FusionAutoComplete, TModel>` | `SetDataSource<TModel, TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:83 |
| `ComponentRef<FusionAutoComplete, TModel>` | `SetDataSource<TModel, TElement>(TypedSource<TElement[]> source)` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:102 |
| `ComponentRef<FusionDropDownList, TModel>` | `SetDataSource<TModel, TSource>(TSource source, Expression<Func<TSource, object?>> path)` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:65 |
| `ComponentRef<FusionDropDownList, TModel>` | `SetDataSource<TModel, TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:80 |
| `ComponentRef<FusionDropDownList, TModel>` | `SetDataSource<TModel, TElement>(TypedSource<TElement[]> source)` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:99 |

### `DataBind` (flush pending property changes to the component)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `DataBind<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:116 |
| `ComponentRef<FusionDropDownList, TModel>` | `DataBind<TModel>()` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:111 |

### `Focus` / `FocusIn` / `FocusOut` (focus methods — the real "focus" verbs)

The cluster spec's `Focus`/`Blur` map to these real source methods: `Focus()`
exists literally on `FusionInPlaceEditor` (SF `setFocus()`); other slices expose
the `FocusIn`/`FocusOut` pair (SF `focusIn()`/`focusOut()`). **No method named
`Blur` exists on `ComponentRef`** — see the "NOT in source" note below.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionInPlaceEditor, TModel>` | `Focus<TModel>()` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:77 |
| `ComponentRef<FusionAutoComplete, TModel>` | `FocusIn<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:123 |
| `ComponentRef<FusionAutoComplete, TModel>` | `FocusOut<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:130 |
| `ComponentRef<FusionDropDownList, TModel>` | `FocusIn<TModel>()` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:118 |
| `ComponentRef<FusionDropDownList, TModel>` | `FocusOut<TModel>()` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:125 |
| `ComponentRef<FusionInputMask, TModel>` | `FocusIn<TModel>()` | `ComponentRef<FusionInputMask, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskExtensions.cs:35 |

### Class mutation on a component wrapper (AddClass / RemoveClass via SF element)

`ElementBuilder` owns the generic DOM `AddClass`/`RemoveClass`/`ToggleClass`.
A component slice may *also* expose `AddClass`/`RemoveClass` that emit a call on
the vendor element (`element.classList.add/remove`). Grounded exemplar:

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionInPlaceEditor, TModel>` | `AddClass<TModel>(string className)` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:93 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `RemoveClass<TModel>(string className)` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:103 |

### Show / Hide popup, Enable / Disable, Save (representative call/set verbs)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `ShowPopup<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:137 |
| `ComponentRef<FusionAutoComplete, TModel>` | `HidePopup<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:144 |
| `ComponentRef<FusionAutoComplete, TModel>` | `Enable<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:155 |
| `ComponentRef<FusionAutoComplete, TModel>` | `Disable<TModel>()` | `ComponentRef<FusionAutoComplete, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:162 |
| `ComponentRef<FusionDropDownList, TModel>` | `ShowPopup<TModel>()` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:132 |
| `ComponentRef<FusionDropDownList, TModel>` | `HidePopup<TModel>()` | `ComponentRef<FusionDropDownList, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:139 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `Enable<TModel>()` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:50 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `Disable<TModel>()` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:58 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `Save<TModel>()` | `ComponentRef<FusionInPlaceEditor, TModel>` | - | yes | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:69 |

### `Value` (read source — ends the component chain, feeds condition/gather)

The component read verb. Returns a `TypedComponentSource<TValue>`, NOT the
receiver, so `ReturnsSelf = no` (it terminates the mutation chain and becomes a
value source for `When` / `SetText(source)` / gather).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| `ComponentRef<FusionAutoComplete, TModel>` | `Value<TModel>()` | `TypedComponentSource<string>` | - | no | Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs:173 |
| `ComponentRef<FusionDropDownList, TModel>` | `Value<TModel>()` | `TypedComponentSource<string>` | - | no | Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs:150 |
| `ComponentRef<FusionInPlaceEditor, TModel>` | `Value<TModel>()` | `TypedComponentSource<string>` | - | no | Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs:122 |
| `ComponentRef<FusionInputMask, TModel>` | `Value<TModel>()` | `TypedComponentSource<string>` | - | no | Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskExtensions.cs:46 |

## NOT in source (named in the cluster spec, no signature found — edge NOT written)

Per "GROUNDED, NOT INFERRED — if the signature is not in source, DO NOT write
the edge," the following cluster-spec names have **no public method** and are
deliberately excluded:

- **`ComponentRef.Call(...)`** — there is no public `Call` extension on
  `ComponentRef`. The only `Call` is `ReactionGraph.Call(...)` (internal plan
  node) and the `internal ComponentRef.EmitCall(...)`
  (`Alis.Reactive/ComponentRef.cs:45`), which vendor extensions invoke
  internally. Component method calls are surfaced only through *named* verbs
  (`DataBind`, `Focus`, `Save`, `ShowPopup`, …), never a generic public `Call`.
- **`ComponentRef.Blur()`** — no `Blur` mutation method exists. `Blur` appears
  only as a typed **event** source, e.g.
  `FusionOtpInput.Blur => TypedEvent<FusionOtpInputBlurArgs>`
  (`Alis.Reactive.Fusion/Components/FusionOtpInput/FusionOtpInputEvents.cs:28`)
  — an event to react *to*, not a method to call. The focus-loss mutation verb
  is `FocusOut` (see Focus table above).

## Grammar notes (nesting & repetition)

- **No NESTING points in this cluster.** No `Element` or `ComponentRef` member
  takes an `Action<PipelineBuilder<...>>` (or any sub-builder callback) — every
  member is a leaf step. Recursion into a nested `PipelineBuilder` happens at
  the *trigger* / *condition* / *response* level (`When/Then`, `OnSuccess`,
  `CustomEvent`), not here. (`Callback = -` on every row.)
- **Chaining / repetition (`ReturnsSelf = yes`).** `ComponentRef` mutation
  extensions are uniformly self-returning, so a single `p.Component<T>(...)` can
  chain many mutations: `.SetDataSource(...).DataBind().SetValue(...)`.
  `ElementBuilder`'s `SetText<TProp>(TypedSource)` / `SetHtml<TProp>(TypedSource)`
  overloads are self-returning; its literal / event-payload / response-body
  overloads return the parent `PipelineBuilder` instead, so they end the element
  chain and continue the pipeline.
- **Chain terminators (`ReturnsSelf = no`).** `Value()` returns a
  `TypedComponentSource<TValue>` — it converts the component reference into a
  value **source** consumed by `When(...)`, `SetText(source)`, gather, dispatch
  payload, etc. The `PipelineBuilder.Element/Component` entry edges also return
  `no` (they start a *new* sub-builder).
- **Vendor-extension shape.** `ComponentRef<TComponent, TModel>` exposes **no**
  public instance members of its own; the entire component grammar is realized
  as `static` extension methods (`this ComponentRef<TConcrete, TModel> self`)
  in each `*Extensions.cs` slice. The rows above are the canonical exemplars
  (AutoComplete, DropDownList, InPlaceEditor, InputMask); ~70 component slices
  follow the identical pattern (`SetValue`/`SetDataSource`/`DataBind`/`Value`/
  focus verbs), each typed to its own concrete component receiver.
