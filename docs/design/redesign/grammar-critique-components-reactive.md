# Grammar Critique — Components + .Reactive + Element/ComponentRef

A programming-language-architect's hardening pass over the **Components / `.Reactive` /
`Element` / `ComponentRef`** cluster. Every current shape is cited from the AST grammar
tables (`ast-grammar-element-component.md`, `ast-grammar-component-reactive.md`) with its
`file:line`. Every adjustment is **BEFORE → AFTER**, names the PL property it improves, and
preserves every capability (zero feature loss). Reconciled with the finalized names in
`09-dsl-naming-sheet.md` and the determinism design discoveries in
`08-determinism-formalization.md` (esp. §6.3 — widen gather `Include` to the abstract
`TypedSource`).

The bar: **easy to write, reads TALL** — vertical fluent chains, one call per line, every
callback hands back a clean builder, return types match intent, one clear spelling per
intent.

---

## How to read this

- **The PL-architect properties** in play: ORTHOGONALITY (one way per intent),
  COMPOSABILITY (`cod(f) ⊆ dom(g)` at every seam — a callback hands back a usable builder),
  TALL-READING (callback-nesting over wide multi-arg calls), LEAST-SURPRISE (return type
  matches intent — no forced re-wrap), DISCOVERABILITY (find the method without reading
  source), CONSISTENCY (same concept = same shape everywhere), EASY-TO-WRITE (good defaults,
  minimal ceremony).
- A "seam bug" is a place where `cod(f) ⊄ dom(g)`: a callback hands back type A but the next
  verb only accepts B ⊊ A. The grammar stops composing and forces a re-wrap or a cast.
- The naming sheet (`09`) is **already decided** — where it locks a name (`Focus`,
  `AsArraySource`, `DispatchButton`, `WhenTemplate`), this critique conforms to it and does
  not re-litigate the *spelling*; it critiques the *shape* (return type, overload split,
  arity, callback vs wide args).

---

## PART A — What is ALREADY GOOD (do not churn)

These read TALL and compose cleanly today. Touching them is churn, not improvement. Stated
first so the hardening stays surgical.

### A1. `.Reactive` is the model fluent edge — keep it verbatim

`ast-grammar-component-reactive.md:101` (and 55 identical sibling rows):

```
AutoCompleteBuilder.Reactive<TModel,TArgs>(
    ReactivePlan<TModel> plan,
    Func<FusionAutoCompleteEvents, TypedEvent<TArgs>> eventSelector,
    Action<TArgs, PipelineBuilder<TModel>> pipeline)
  -> AutoCompleteBuilder   (ReturnsSelf = yes)
```

This is the gold standard the rest of the cluster should be judged against:

- **CONSISTENCY** — the *exact same shape* on all 56 builders (`ast-grammar-component-reactive.md:101-179`).
  A dev learns it once and it never varies per component.
- **TALL-READING / COMPOSABILITY** — the `pipeline` callback hands back a clean
  `PipelineBuilder<TModel>` (`Callback = Action<TArgs,PipelineBuilder<TModel>>`, a true
  NESTING point, line 24). The body reads top-to-bottom, one reaction per line.
- **LEAST-SURPRISE** — `ReturnsSelf = yes` means `.Reactive(...).Reactive(...)` chains
  cleanly for multiple events on one component (`ast-grammar-component-reactive.md:13,187`).
  The return type matches the intent ("more events on the same builder").
- **DISCOVERABILITY** — the `eventSelector` `evt => evt.Changed` is a typed member pick over
  `<Comp>Events`, so IntelliSense lists the legal events; no string lookup.

The naming sheet locks this (`09:341`: `.Reactive` KEEP, "identical shape everywhere"). **Keep.**

### A2. The `ComponentRef` mutation verbs are uniformly self-returning

`ast-grammar-element-component.md:81-150` — every mutation extension
(`SetValue`/`SetText`/`SetDataSource`/`DataBind`/`Enable`/`Disable`/`ShowPopup`/`Save`/…)
returns `ComponentRef<TComponent,TModel>` (`ReturnsSelf = yes`). A single
`p.Component<T>(...)` chains many mutations top-to-bottom
(`ast-grammar-element-component.md:192-193`):

```
p.Component<FusionDropDownList>(m => m.City)
 .SetDataSource(resp, r => r.Items)
 .DataBind()
 .SetValue("London")
```

This is **TALL-READING + COMPOSABILITY** done right. **Keep.**

### A3. `Value()` correctly terminates the mutation chain into a source

`ast-grammar-element-component.md:160` — `Value() -> TypedComponentSource<string>`
(`ReturnsSelf = no`). This is the **right** asymmetry: a read is not a mutation, so it
*should* end the mutation chain and hand back a value source that feeds `When` / gather /
`SetText(source)` (`ast-grammar-element-component.md:198-201`). The naming sheet keeps it
(`09:345`). LEAST-SURPRISE is satisfied because the return type screams "this is now a
value source, not a fluent component." **Keep.**

### A4. The cluster has zero spurious nesting — leaves stay leaves

`ast-grammar-element-component.md:185-190` — no `Element`/`ComponentRef` member takes an
`Action<PipelineBuilder>`. Recursion lives at the *trigger/condition/response* level, not
buried in a DOM-mutation leaf. This keeps the grammar's nesting points few and meaningful
(ORTHOGONALITY of structure). **Keep.**

### A5. The `Fusion*` / `Native*` vendor prefix on the component-builder entries

`ast-grammar-component-reactive.md:47-76` — `FusionAutoComplete` / `NativeTextBox` etc.
Vendor-isolation screaming names; HARD RULE in the naming sheet (`09:340`). One concept, one
name per slice; a dev reads the vendor off the verb. **Keep.**

### A6. `Html.InputField` → `InputBoundField` entry is clean and typed

`ast-grammar-component-reactive.md:38-39` — `InputField(plan, expr)` and the
`configure`-overload both return `InputBoundField<TModel,TProp>`, with `TProp` flowing into
the component builder so value-typed components pin `bool`/`double`/`string?`
(`ast-grammar-component-reactive.md:43,48,62-63`). The expression-driven binding is the
deterministic-id keystone (`08:692-695`). **Keep.**

---

## PART B — PROPOSED ADJUSTMENTS (BEFORE → AFTER)

Ten hardening adjustments. Each cites the current shape, names the PL property it hurts, and
gives a concrete BEFORE → AFTER that preserves every capability.

---

### Adjustment 1 — `ElementBuilder.SetText`/`SetHtml`: unify the return type (kill the asymmetric re-wrap)

**Current shape.** `ast-grammar-element-component.md:49-55`:

| Member | Returns | ReturnsSelf |
|---|---|---|
| `SetText(string text)` | `PipelineBuilder<TModel>` | no |
| `SetText<TSource>(source, path)` | `PipelineBuilder<TModel>` | no |
| `SetText<TResponse>(ResponseBody<T>, path)` | `PipelineBuilder<TModel>` | no |
| `SetText<TProp>(TypedSource<TProp> source)` | **`ElementBuilder<TModel>`** | **yes** |

The grammar itself flags this as a "return-type split" (`ast-grammar-element-component.md:38-42`):
the **literal / payload / response-body** overloads bounce back to the parent
`PipelineBuilder` (ending the element chain), but the **`TypedSource`** overload returns
`this` ElementBuilder (chainable). Same for `SetHtml` (`:53-55,116`).

**PL property hurt — LEAST-SURPRISE + CONSISTENCY.** *One verb, four overloads, two different
return types.* A dev who writes `el.SetText("Hi")` ends the chain; a dev who writes
`el.SetText(src)` keeps it. The same method name behaves differently based on which overload
resolves — the textbook "asymmetric return that forces a re-wrap" wart. To set text **and**
add a class on a literal, the dev must re-enter the element:

```csharp
// BEFORE — literal overload dumps you back to the pipeline, so you re-Element to keep going
p.Element("greeting").SetText("Hello")           // returns PipelineBuilder
 .Element("greeting").AddClass("greeting--shown") // re-wrap: name the same id twice
```

whereas the `TypedSource` overload would have let you chain `.AddClass` directly.

**AFTER — every `ElementBuilder` mutation returns `ElementBuilder<TModel>` (`ReturnsSelf = yes`);
return to the pipeline with one explicit terminator.**

```csharp
// AFTER — uniform self-return; the element chain reads TALL, one terminator closes it
p.Element("greeting")
 .SetText("Hello")
 .AddClass("greeting--shown")
 .Done()          // single, discoverable "back to pipeline" verb
```

- Make `AddClass`/`RemoveClass`/`ToggleClass`/`SetText`(all 4)/`SetHtml`(all 3)/`Show`/`Hide`
  (`ast-grammar-element-component.md:46-57`) **all** return `ElementBuilder<TModel>`.
- Add **one** `Done()` (or reuse the implicit pipeline continuation) that returns the parent
  `PipelineBuilder<TModel>`. This mirrors how `ComponentRef` already works (every mutation
  self-returns, `ast-grammar-element-component.md:192-193`) — so **Element and Component become
  the same shape**, closing a cross-receiver inconsistency.
- **Zero capability lost** — the literal/payload/response-body/typed-source value families all
  survive as overloads of one verb (the naming sheet keeps the overload set, `09:238,343`); only
  the return type is regularized. Improves CONSISTENCY (Element now matches Component) and
  LEAST-SURPRISE (one verb, one return type).

---

### Adjustment 2 — `ComponentRef` has zero discoverable surface; the verbs are invisible extension methods

**Current shape.** `ast-grammar-element-component.md:59-68,203-209`: `ComponentRef<TComponent,
TModel>` *itself* has **zero public members** — `EmitSet`/`EmitCall`/`Read` are `internal`.
Every named verb (`SetValue`, `SetDataSource`, `DataBind`, `Focus`, `Value`, …) is a **public
extension method** living in a per-slice `*Extensions.cs`, typed to the concrete
`ComponentRef<FusionDropDownList,TModel>` (etc.).

**PL property hurt — DISCOVERABILITY.** When a dev writes `p.Component<FusionDropDownList>(m =>
m.City).` and hits `.`, IntelliSense surfaces the verbs **only if** the slice's
`*Extensions.cs` namespace is in scope. There is no method *on the type* to anchor discovery,
and the extension methods are scattered across ~70 files
(`ast-grammar-element-component.md:206-209`). A dev cannot find "what can I do to a
DropDownList?" without reading source — exactly the discoverability failure the bar forbids.

**AFTER — keep the extension-method *implementation* (vendor isolation, vertical slices), but
guarantee discoverability with two cheap, non-breaking moves:**

1. **Co-locate every component's verbs in one `using`-discoverable namespace per vendor**
   (e.g. `Alis.Reactive.Fusion`), and make the `*Extensions.cs` files `partial`-grouped per
   component so `IComponent`'s XML doc can `<see cref>` the verb set. No signature changes.
2. **Promote the universal verbs that every input slice repeats** — `SetValue`,
   `Value`, focus, `Enable`/`Disable` (`ast-grammar-element-component.md:81-163`) — to an
   *interface-constrained* extension on `ComponentRef<TComponent,TModel> where TComponent :
   IInputComponent`. The naming sheet already names `IInputComponent` / `ValueMember`
   (`09:339`). One generic extension per universal verb means the dev sees `SetValue`/`Value`
   on **every** input component without 70 duplicated declarations — DISCOVERABILITY and
   CONSISTENCY both rise, vendor-specific verbs (`SetDataSource`, `ShowPopup`) stay per-slice.

- **Zero capability lost** — every verb still exists; vendor isolation (Rule 5) is preserved
  because the *vendor-specific* verbs stay in their slice. Only the *universal* verbs collapse
  to one constrained declaration. This is the "merge-by-equality" the algebra mandates for
  extensionally-equal members (`08:792-794`).

---

### Adjustment 3 — `Focus`/`FocusIn`/`FocusOut` is three names for two concepts; reconcile to the locked `Focus`

**Current shape.** `ast-grammar-element-component.md:111-125`: the cluster exposes
`Focus()` (only on `FusionInPlaceEditor`, SF `setFocus()`), and the `FocusIn()`/`FocusOut()`
pair on AutoComplete/DropDownList/InputMask (SF `focusIn()`/`focusOut()`). So "give this
component focus" is spelled **two different ways** depending on the slice.

**PL property hurt — ORTHOGONALITY + CONSISTENCY + LEAST-SURPRISE.** The naming sheet already
ruled (`09:344`): **`FocusIn` lies** — the DOM focus method is `focus`; `focusin` is a
*different bubbling event*. A dev calling `.FocusIn()` expecting "focus the field" is reading
a method named after an event. Three names (`Focus`/`FocusIn`/`FocusOut`) for the
focus-vs-blur concept across slices is a per-slice inconsistency.

**AFTER — conform to the locked naming-sheet decision (`09:344,451`):**

```
BEFORE:  .Focus()            (InPlaceEditor only)
         .FocusIn()          (AutoComplete, DropDownList, InputMask)
         .FocusOut()         (AutoComplete, DropDownList)

AFTER:   .Focus()            (the focus verb — every slice, one name, SF focus/setFocus)
         .Blur()             (the focus-loss verb — every slice that has focusOut)
```

- Collapse `FocusIn` → `Focus` on **every** slice (the naming sheet: "Other slices use
  `ComponentMethod.Named("focus")` — collapse to `Focus()`", `09:344`).
- Rename `FocusOut` → `Blur` to name the **mutation** (call `blur()`/`focusOut()`). Note: the
  AST correctly records that **no `Blur` mutation exists today** and `Blur` currently appears
  only as an *event* source (`ast-grammar-element-component.md:178-182`). Resolve the
  collision by lane: the focus-loss **verb** is `Blur()` (a call), the focus-loss **event** is
  `evt => evt.Blur` (a `TypedEvent`). Same word, two lanes (call vs event) — exactly the
  `WhenTemplate`-vs-`When` lane-split pattern the sheet already blesses (`09:354,437`).
- **Zero capability lost** — focus and focus-loss both still callable on every slice;
  ORTHOGONALITY rises (one focus name, one blur name across all slices).

---

### Adjustment 4 — `SetText` is overloaded across TWO receivers with different meanings (Element vs ComponentRef)

**Current shape.** `SetText` exists in two places with **different semantics**:

- `ElementBuilder.SetText(...)` (`ast-grammar-element-component.md:49-52`) — set a DOM
  element's text content.
- `ComponentRef<FusionAutoComplete>.SetText(string text)`
  (`ast-grammar-element-component.md:90-91`) — "set *displayed* text without changing value"
  (a vendor concept distinct from `SetValue`).

**PL property hurt — CONSISTENCY / LEAST-SURPRISE.** The *same verb* means "write DOM
textContent" on an element and "set the display text but not the committed value" on a
component. A dev reading `x.SetText("London")` cannot tell which contract applies without
knowing whether `x` is an `ElementBuilder` or a `ComponentRef`. The element one is a terminal
DOM write; the component one is one of a {`SetValue`, `SetText`} pair where the distinction
(value vs display) is load-bearing.

**AFTER — name the component-side concept for what it is: `SetDisplayText`.**

```
BEFORE:  componentRef.SetText("London")   // ambiguous vs Element.SetText / vs SetValue
AFTER:   componentRef.SetDisplayText("London")
         componentRef.SetValue("london")  // the committed-value pair, now unmistakable
```

- The component pair becomes `SetValue` (commit) / `SetDisplayText` (presentation), which
  reads cold as two distinct intents and never collides with the DOM `Element.SetText`.
- The naming sheet keeps `SetText`/`SetHtml` for the **Element/value-source** lane (`09:343`,
  "one verb over the unified value sources") — that decision is for *ElementBuilder*; the
  *ComponentRef* display-text verb is a different concept and should not borrow the name.
- **Zero capability lost** — both behaviors survive; CONSISTENCY rises (one verb ↦ one
  concept; the value/display pair is self-documenting).

---

### Adjustment 5 — `SetDataSource` takes a 2-arg `(source, path)` wide call; make the value-source intake the abstract `TypedSource` (§6.3 seam fix)

**Current shape.** `ast-grammar-element-component.md:97-102` — `SetDataSource` has 3
overloads per slice:

```
SetDataSource<TModel,TSource>(TSource source, Expression<Func<TSource,object?>> path)        // :97
SetDataSource<TModel,TResponse>(ResponseBody<TResponse> source, Expression<...> path)        // :98
SetDataSource<TModel,TElement>(TypedSource<TElement[]> source)                                // :99
```

**PL property hurt — COMPOSABILITY (seam bug) + EASY-TO-WRITE.** Two issues:

1. The `(source, path)` overloads are **wide two-arg calls** that read sideways, not TALL —
   the dev supplies a source *and* a path lambda inline. The third overload
   (`TypedSource<TElement[]>`) is the clean one (a pre-built array source). But a
   `ReactiveArray<T>.AsArraySource()` result (the abstract `TypedSource<T[]>`,
   `09:294`) is exactly that clean shape — yet the determinism doc proves the *gather*
   `Include` intake does **not** accept the abstract `TypedSource` (`08:911,1031-1039`, §6.3:
   `cod(AsArraySource) = TypedSource ⊄ dom(Include) = TypedComponentSource ⊎ TypedPluginSource`).
2. The component `SetDataSource(TypedSource<TElement[]>)` overload *does* take the abstract
   source — so the **same array source composes into `SetDataSource` but not into `Include`**:
   a cross-area inconsistency at the exact seam §6.3 flags.

**AFTER — widen *every* value-source intake (component `SetDataSource`, `SetText`, AND gather
`Include`/`Header`/`RouteParam`) to the abstract `TypedSource<T>` (§6.3 generalized).**

```
BEFORE (gather seam):  Include accepts TypedComponentSource ⊎ TypedPluginSource   (⊊ TypedSource)
AFTER  (gather seam):  Include accepts TypedSource<T>                              (the whole spine)

// then this composes uniformly — array source into BOTH a component AND a request:
var top = ReactiveArray.From(...).OrderByDescending(x => x.Score).AsArraySource();
p.Component<FusionDropDownList>(m => m.City).SetDataSource(top)   // already works
p.Post("/save").Gather(g => g.Include(top))                       // §6.3 — now also works
```

- This is `08` §6.3 applied not just to `Include` but as the *general grammar rule*: **every
  place that consumes a readable value consumes `TypedSource<T>`** — "one `ValueExpression`
  reads all values" (`08:1036`, naming sheet `09:48,430`). The naming sheet's cross-area row
  already declares `TypedSource<T>` the single intake for "component data-source" *and* "HTTP
  gather" (`09:430`) — this adjustment makes the *signatures* honor that decision.
- The `(source, path)` overloads stay as **convenience sugar** (they lower to a `TypedSource`
  via a factory, exactly as the And/Or fold does, `09:90`), so no capability is lost; the
  TALL path becomes "build the source once, hand it in by one argument."
- **Zero capability lost** — improves COMPOSABILITY (seam `cod ⊆ dom` restored) and
  EASY-TO-WRITE (one-arg source intake; build complex sources upstream once).

---

### Adjustment 6 — `FusionSmartTextArea.Reactive` breaks the one universal `.Reactive` shape

**Current shape.** `ast-grammar-component-reactive.md:157-164` — **the lone exception**:
SmartTextArea's `.Reactive` extends `ReactivePlan<TModel>` (not the builder) and returns
`void`, with **two** overloads keyed by id-string or model-expression:

```
ReactivePlan<TModel>.Reactive<TModel,TArgs>(string componentId, on, pipeline) -> void          // :163
ReactivePlan<TModel>.Reactive<TModel,TProp,TArgs>(Expr<...> component, on, pipeline) -> void     // :164
```

Every other component (`ast-grammar-component-reactive.md:101-179`, 55 rows) has
`.Reactive` **on its builder**, returning the builder (`ReturnsSelf = yes`).

**PL property hurt — CONSISTENCY (worst kind: the one outlier) + LEAST-SURPRISE.** The naming
sheet's own cross-area law says `.Reactive` is "the one shared verb … **identical shape
everywhere**" (`09:341`). SmartTextArea violates exactly that: a dev who learned `b.Reactive(...)`
returns `b` and chains, then meets a component where `.Reactive` lives on the *plan*, returns
*void*, and can't chain. It also re-introduces an id-string overload (`ast-grammar-component-reactive.md:163`)
that the rest of the cluster avoids by using typed builders.

**AFTER — bring SmartTextArea onto the universal builder shape.**

```
BEFORE:  plan.Reactive(componentId, on, pipeline) -> void
         plan.Reactive(m => m.Bio, on, pipeline)  -> void

AFTER:   field.FusionSmartTextArea(b => b
            .Reactive(plan, evt => evt.X, (args, p) => ...))   // -> SmartTextAreaBuilder, ReturnsSelf = yes
```

- Move `.Reactive` onto the SmartTextArea **builder** (whatever `FusionSmartTextArea(Action<...
  Options>)` nests into, `ast-grammar-component-reactive.md:66`), returning the builder so it
  chains like all 55 siblings.
- Drop the `string componentId` overload — the builder already knows its bound expression
  (the typed path is the discoverable, collision-free id, `08:692-695`); the id-string lane is
  a stringly escape hatch the cluster otherwise doesn't need.
- **Zero capability lost** — both events on SmartTextArea still wire; the cross-model /
  by-expression identification survives via the builder's own binding. CONSISTENCY restored to
  *all 56* components.

---

### Adjustment 7 — `FusionSmartTextArea` uses `Action<...Options>` (configure-bag) while every sibling uses `Action<XxxBuilder>` (fluent builder)

**Current shape.** `ast-grammar-component-reactive.md:66`:

```
FusionSmartTextArea<TModel,TProp>(Action<FusionSmartTextAreaOptions> configure) -> void
```

vs every sibling (`ast-grammar-component-reactive.md:47-76`):

```
FusionAutoComplete<TModel,TProp>(Action<AutoCompleteBuilder> build) -> void
FusionTextBox<TModel,TProp>(Action<TextBoxBuilder> build) -> void
...
```

**PL property hurt — CONSISTENCY + TALL-READING.** One component takes a **property bag**
(`Options`, set fields), the rest take a **fluent builder** (`build`, chainable verbs). The
bag style reads as a block of assignments; the builder style reads TALL with one verb per
line and is where `.Fields(...)` / `.Reactive(...)` live (`ast-grammar-component-reactive.md:84-91,186`).
A configure-bag cannot host the fluent `.Reactive` chain — which is *why* Adjustment 6 had to
exist. Fixing the receiver here makes Adjustment 6 natural rather than bolted-on.

**AFTER — give SmartTextArea a `FusionSmartTextAreaBuilder` like every sibling.**

```
BEFORE:  field.FusionSmartTextArea(o => { o.Foo = ...; o.Bar = ...; })
AFTER:   field.FusionSmartTextArea(b => b
            .Foo(...)
            .Bar(...)
            .Reactive(plan, evt => evt.X, (args, p) => ...))
```

- **Zero capability lost** — every option the bag exposed becomes a builder verb (the standard
  vertical-slice shape); the SmartTextArea is no longer a special case in two ways at once.
  CONSISTENCY (one component-builder shape) + TALL-READING both rise.

---

### Adjustment 8 — `Component<TComponent>(...)` has 4 overloads that read identically at the call site

**Current shape.** `ast-grammar-element-component.md:30-33`:

```
Component<TComponent>(Expression<Func<TModel,object>> expr)              -> ComponentRef   // :30  same-model
Component<TComponent,TOtherModel>(Expression<Func<TOtherModel,object>>)  -> ComponentRef   // :31  cross-model
Component<TComponent>(string refId)                                      -> ComponentRef   // :32  by-id
Component<TComponent>()  where TComponent : IAppLevelComponent           -> ComponentRef   // :33  app-level singleton
```

**PL property hurt — DISCOVERABILITY + LEAST-SURPRISE.** Four overloads of one verb that split
on *how you identify the component* (`09:246`). The first two are distinguishable only by
arity/type inference; the third (`string refId`) is a stringly path that reads the same as the
others at a glance; the fourth (no-arg, constrained) is invisible unless you know the type is
`IAppLevelComponent`. A dev cannot tell from `p.Component<X>(...)` which identity strategy is
in play, and `Component<X>("some-id")` reaches for a string when the typed-expression form
exists.

**AFTER — keep the verb `Component` (naming sheet locks it, `09:246`), but make the
identity-strategy explicit and reduce the stringly surface.**

```
BEFORE:  p.Component<FusionDropDownList>(m => m.City)         // same-model
         p.Component<FusionDropDownList, OrderModel>(o => o.City)  // cross-model
         p.Component<FusionDropDownList>("Order__City")       // by-id string
         p.Component<FusionConfirm>()                          // app-level

AFTER:   p.Component<FusionDropDownList>(m => m.City)         // same-model (KEEP — the common path)
         p.Component<FusionDropDownList>(o => o.City)         // cross-model: infer TOtherModel from the lambda's param (drop the explicit 2nd type arg)
         p.Component<FusionConfirm>()                          // app-level (KEEP — constrained, discoverable via IAppLevelComponent)
         // by-id string: DEMOTE — make it Component<T>(ComponentId id) over a typed id value-object, not a raw string
```

- Fold the cross-model overload's redundant `TOtherModel` type argument into inference from the
  lambda parameter type — the dev writes one expression, not `<X, OtherModel>`. Fewer type
  arguments = EASY-TO-WRITE.
- Replace the raw `string refId` with a typed `ComponentId` value object (the `IdGenerator`
  already produces these deterministically, `08:662,692-695`). A stringly id is a developer-error
  trap (no compile check); a `ComponentId` keeps the escape hatch but types it.
- **Zero capability lost** — all four identification strategies remain reachable; LEAST-SURPRISE
  and DISCOVERABILITY rise because the call site no longer hides which strategy is active.

---

### Adjustment 9 — `eventSelector` is named inconsistently (`eventSelector` vs `on`) across slices

**Current shape.** `ast-grammar-component-reactive.md:28` notes the `.Reactive` selector
parameter is named **`eventSelector`** on most slices but **`on`** on FusionChipList,
FusionMention, FusionSmartTextArea (`ast-grammar-component-reactive.md:136,144,163` —
`Func<...Events, TypedEvent<TArgs>> on`).

**PL property hurt — CONSISTENCY.** Same parameter, same shape, two names. This leaks to the
dev as named-argument inconsistency (`Reactive(on: ...)` works on some, `Reactive(eventSelector:
...)` on others) and to docs/tooling that surface parameter names. A 56-row grammar where 53
say `eventSelector` and 3 say `on` is a pure churn-inviting wart.

**AFTER — one parameter name everywhere.**

```
BEFORE:  Reactive(plan, Func<...Events,TypedEvent<TArgs>> eventSelector, pipeline)   // 53 slices
         Reactive(plan, Func<...Events,TypedEvent<TArgs>> on, pipeline)              // 3 slices

AFTER:   Reactive(plan, Func<...Events,TypedEvent<TArgs>> on, pipeline)              // all 56
```

- Pick **`on`** (shorter, reads as "on this event", pairs with the trigger verb `On`,
  `09:223`) — or keep `eventSelector` — but **one** of them, on all 56. The naming sheet's
  one-concept-one-name law (`09:6`) demands it.
- **Zero capability lost** — pure parameter-name regularization; CONSISTENCY rises, named-arg
  call sites become uniform.

---

### Adjustment 10 — `NativeButton.Reactive` and the display-builder `.Reactive` are in a different generic-arity family than native input builders

**Current shape.** `ast-grammar-component-reactive.md:172-179`:

```
NativeButtonBuilder<TModel>.Reactive<TModel,TArgs>(...)            -> NativeButtonBuilder<TModel>        // :172  (2 type args)
NativeTextBoxBuilder<TModel,TProp>.Reactive<TModel,TProp,TArgs>(..)-> NativeTextBoxBuilder<TModel,TProp> // :179  (3 type args)
```

Native **input** builders carry `TProp` and so `.Reactive` is `<TModel,TProp,TArgs>`; the
non-model-bound `NativeButton` and the Fusion display builders
(`ast-grammar-component-reactive.md:131-155`) are `<TModel,TArgs>`. The display builders also
carry no `TProp` (`FusionGridBuilder<TModel>`, etc.).

**PL property hurt — CONSISTENCY (mild, defensible) + LEAST-SURPRISE.** This split is *mostly
honest* — a button has no bound value so it has no `TProp` — and the grammar documents it
clearly (`ast-grammar-component-reactive.md:168`). It is the **least-severe** item and is
listed to confirm it should **not** be churned into a forced-uniform 3-arity. The one real
hardening: a dev reading two adjacent `.Reactive` calls sees `<TModel,TArgs>` vs
`<TModel,TProp,TArgs>` and may think they are different verbs.

**AFTER — keep the arity difference (it is type-honest), but make `TArgs` the only
*explicit* type argument the dev ever writes, by ordering it last and inferring the rest.**

```
BEFORE:  b.Reactive<OrderModel,string,ChangeArgs>(plan, on, pipeline)    // dev tempted to spell all three
AFTER:   b.Reactive(plan, evt => evt.Changed, (args, p) => ...)          // TModel/TProp/TArgs all inferred from plan + selector + lambda
```

- Ensure `TModel` infers from `plan`, `TProp` from the builder's own type, and `TArgs` from the
  `TypedEvent<TArgs>` the selector returns — so **no slice ever needs explicit type arguments**
  at the call site. Then the 2-arity vs 3-arity difference is invisible to the author and the
  two `.Reactive` calls *read* identically (LEAST-SURPRISE) while staying type-honest.
- **Zero capability lost** — the generic signatures are unchanged; only inference is guaranteed
  so the author writes the same call shape everywhere. This is the lightest-touch item: it
  hardens *readability* without merging two genuinely different arities.

---

## PART C — Summary ledger

| # | Adjustment | PL property improved | Capability preserved |
|---|---|---|---|
| 1 | `ElementBuilder` mutations all return `ElementBuilder` + one `Done()` terminator | LEAST-SURPRISE, CONSISTENCY (matches ComponentRef) | all value-source overloads kept |
| 2 | Make `ComponentRef` verbs discoverable; promote universal verbs to one `IInputComponent`-constrained extension | DISCOVERABILITY, CONSISTENCY | vendor-specific verbs stay per-slice (Rule 5) |
| 3 | `FocusIn`→`Focus`, `FocusOut`→`Blur` across all slices (conforms `09:344`) | ORTHOGONALITY, CONSISTENCY | focus + focus-loss callable on every slice |
| 4 | `ComponentRef.SetText`→`SetDisplayText` (de-collide with `Element.SetText`) | CONSISTENCY, LEAST-SURPRISE | value/display pair both kept |
| 5 | Widen every value-source intake (`SetDataSource`, gather `Include`/`Header`/`RouteParam`) to abstract `TypedSource<T>` (§6.3 generalized) | COMPOSABILITY (seam), EASY-TO-WRITE | `(source,path)` sugar kept as fold |
| 6 | `FusionSmartTextArea.Reactive` onto the builder, returning the builder (`ReturnsSelf=yes`) | CONSISTENCY (the lone outlier), LEAST-SURPRISE | both events + cross-model id kept |
| 7 | `FusionSmartTextArea` takes `Action<Builder>` not `Action<Options>` bag | CONSISTENCY, TALL-READING | every option becomes a builder verb |
| 8 | `Component<T>` 4 overloads: infer cross-model `TOtherModel`, type the by-id string as `ComponentId` | DISCOVERABILITY, LEAST-SURPRISE, EASY-TO-WRITE | all 4 identity strategies reachable |
| 9 | One `.Reactive` selector param name (`on`) across all 56 slices | CONSISTENCY | pure rename |
| 10 | Guarantee full inference on `.Reactive` so no slice needs explicit type args | LEAST-SURPRISE | 2-arity/3-arity split stays type-honest |

**10 proposed adjustments.** Six are already good and explicitly kept (Part A). Every
adjustment is BEFORE → AFTER, grounded in an AST `file:line`, reconciled with the locked names
in `09-dsl-naming-sheet.md` and the §6.3 widening in `08-determinism-formalization.md`, and
loses zero capability.
