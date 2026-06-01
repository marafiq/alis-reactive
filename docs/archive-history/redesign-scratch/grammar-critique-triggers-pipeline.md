# Grammar Critique — Triggers + Pipeline/Reactions

A programming-language architect's critique of the **Triggers** (`TriggerBuilder`) and
**Pipeline/Reactions** (`PipelineBuilder`) DSL clusters, grounded in the real AST
signatures in [`ast-grammar-entry-triggers.md`](ast-grammar-entry-triggers.md) and
[`ast-grammar-pipeline.md`](ast-grammar-pipeline.md).

**The bar.** The DSL must be *easy to write* and *read TALL* — vertical fluent chains, one
call per line, every callback handing back a clean builder so the grammar nests cleanly.
Each issue is judged on the seven PL-architect properties: **orthogonality** (one clear way
per intent), **composability** (`cod(f) ⊆ dom(g)` at every seam), **TALL-reading**
(builder-callbacks over wide multi-arg calls), **least-surprise** (return types match
intent), **discoverability** (right method found without reading source), **consistency**
(same concept ⇒ same shape everywhere), **easy-to-write** (good defaults, minimal ceremony).

**Reconciliation.** Names already decided in [`09-dsl-naming-sheet.md`](09-dsl-naming-sheet.md)
§3.1 (Triggers + Reactions), §3.4 (HTTP), §3.7 (App-level/Plugin), and the §5 ledger are
treated as **locked** — this critique adopts them and does not re-litigate them. The
determinism discoveries in [`08-determinism-formalization.md`](08-determinism-formalization.md)
§6.2 (stamp the lane / pass the plan) and §6.3 (widen gather `Include` to abstract
`TypedSource`) are treated as **forcing functions** on the grammar.

**Method.** Each issue cites the current shape from the AST grammar with `file:line`, names
the PL-architect property it hurts, and proposes a concrete `BEFORE → AFTER` that preserves
*every* capability (zero feature loss). The closing section confirms what already reads well
and must **not** be churned.

> **Scope note (load-bearing).** The naming sheet already fixes the *spelling* of several
> members (`DomReady → PageLoad`, `CustomEvent → Event`, `DispatchWith → DispatchFrom`,
> `ValidationErrors → ShowValidationErrors`). This critique does **not** repeat those as
> "adjustments" — they are settled renames. Every numbered adjustment below is a **grammar
> shape** change (arity, callback vs positional, return type, seam type, default) that the
> naming sheet did *not* decide, or a structural fold the sheet's renames *imply* but did
> not spell out.

---

## A. Triggers cluster (`TriggerBuilder<TModel>`)

### A1. `Html.On` returns `void` — the entry edge dead-ends, breaking the TALL chain at the top

**Current shape.**
`Html.On<TModel>(ReactivePlan<TModel> plan, Action<TriggerBuilder<TModel>> trigger)` returns
`void` — `ast-grammar-entry-triggers.md:33` (`HtmlExtensions.cs:53`). The `ReactivePlan`
factory and `RenderPlan` are separate statements: `ast-grammar-entry-triggers.md:31`
(`ReactivePlan()`), `:36` (`RenderPlan(plan)`).

**Property hurt: TALL-reading + composability.** Because `On` is `void`, the authoring of a
view is forced into three disconnected statements that do not chain:

```csharp
var plan = Html.ReactivePlan<OrderModel>();   // statement 1
Html.On(plan, t => { … });                    // statement 2 — dead-ends in void
@Html.RenderPlan(plan)                          // statement 3
```

The reader must hold `plan` in their head across three sites. The seam `cod(On) = void`
cannot feed `RenderPlan`, so the grammar cannot read top-to-bottom — the single most
important TALL property is broken at the *very entry point*.

**BEFORE → AFTER.**

```csharp
// BEFORE — void dead-end, plan threaded by hand across 3 statements
var plan = Html.ReactivePlan<OrderModel>();
Html.On(plan, t => t.PageLoad(p => …));
@Html.RenderPlan(plan)

// AFTER — On returns the plan; the chain reads top-to-bottom in one expression
@Html.ReactivePlan<OrderModel>()
     .On(t => t
         .PageLoad(p => …))
     .Render()          // (or @Html.RenderPlan(...) kept as the terminal)
```

Make `On` an instance method (or extension) on `ReactivePlan<TModel>` that returns the same
`ReactivePlan<TModel>` (`ReturnsSelf`). This **adds** a chainable spelling and preserves the
existing `Html.On(plan, …)` free-function spelling for partial-injection scenarios where the
plan is built elsewhere. Capability preserved, TALL restored, the entry seam now composes
into `Render`.

---

### A2. Trigger payload is a *type parameter + positional callback arity*, not an orthogonal modifier — discoverability + consistency wart

**Current shape.** Every payload-bearing trigger appears **twice**: an untyped form and a
`<TPayload>` form whose callback arity *changes* from `Action<PipelineBuilder>` to
`Action<TPayload, PipelineBuilder>`:

- `Event(name, Action<PipelineBuilder>)` vs `Event<TPayload>(name, Action<TPayload, PipelineBuilder>) where TPayload : new()` — `:50` vs `:51`.
- `ServerPush(url, eventType, Action<PipelineBuilder>)` vs `ServerPush<TPayload>(url, eventType, Action<TPayload, PipelineBuilder>)` — `:53` vs `:54`.
- `SignalR(hub, method, Action<PipelineBuilder>)` vs `SignalR<TPayload>(hub, method, Action<TPayload, PipelineBuilder>)` — `:55` vs `:56`.

**Property hurt: consistency + discoverability.** "Carry a typed payload" is **one** concept
but is spelled three different ways depending on the trigger, and the typed form silently
*reshapes the callback signature* (the payload becomes a leading positional parameter). A dev
who learned `t.Event<T>((payload, p) => …)` cannot transfer that to `ServerPush` without
re-reading the source, because the payload's position relative to `url`/`eventType` differs
per trigger. The payload contract is the *same* idea (`PayloadContract = ⟨untyped⟩ ⊎ ⟨typed⟩`,
`08`:617-621) but the grammar does not present it as one orthogonal axis.

**This is load-bearing — do not delete the typed lane.** The naming sheet keeps
`Event<TPayload>` "as a distinct arity (load-bearing lane)" (`09`:227). The adjustment is
**not** to remove it but to make the typed payload read *the same way* on every trigger.

**BEFORE → AFTER.** Hand the payload to the pipeline callback through the *pipeline builder
itself*, so the callback arity never changes and the payload is reached the same way
everywhere:

```csharp
// BEFORE — payload is a leading positional callback param, position differs per trigger
t.Event<OrderPlaced>("order-placed", (payload, p) => p
    .Set(m => m.Total).From(payload, x => x.Amount));

t.ServerPush<OrderPlaced>("/sse", "order-placed", (payload, p) => p
    .Set(m => m.Total).From(payload, x => x.Amount));

// AFTER — one callback shape everywhere; the typed payload is read off `p`
t.Event<OrderPlaced>("order-placed", p => p
    .Set(m => m.Total).From(p.Payload, x => x.Amount));

t.ServerPush<OrderPlaced>("/sse", "order-placed", p => p
    .Set(m => m.Total).From(p.Payload, x => x.Amount));
```

`p.Payload` is a `TypedSource<TPayload>`-yielding handle on the pipeline builder, available
only when `<TPayload>` was supplied (compile-enforced; absent on the untyped overload). One
callback shape (`Action<PipelineBuilder<TModel>>`) across *every* trigger and the untyped/typed
split, the payload read off the same `p` the rest of the pipeline already uses — this is
exactly the consistency the naming sheet asks for ("trigger/response/reactive callbacks
should look alike"). Every typed-payload capability is preserved; the leading-positional
`(payload, p)` arity is retired in favor of one uniform shape.

> *Math tie:* `p.Payload : TypedSource<TPayload>` folds the trigger payload into the **one**
> value spine (`08`:430, `TypedSource<T>` is the one typed authoring surface), so the
> `(payload, path)` reads in §A2 and the `When(payload, path)` reads in §B5 unify on the
> same source factory — closing the seam instead of minting a per-trigger arity.

---

### A3. `ServerPush` overloads encode "which events match" as positional `string`/arity, not a typed filter — least-surprise + discoverability

**Current shape.** Three `ServerPush` overloads differ only in whether an `eventType` string
and a `<TPayload>` are present:
`ServerPush(url, pipeline)` `:52`, `ServerPush(url, eventType, pipeline)` `:53`,
`ServerPush<TPayload>(url, eventType, pipeline)` `:54`.

**Property hurt: least-surprise + discoverability.** The carrier is
`EventFilter = ⟨any⟩ ⊎ ⟨named⟩ × EventName` (`08`:619) — a genuine two-arm choice ("match any
SSE event" vs "match this named event"). But the grammar encodes that choice by the
*presence or absence of a positional string argument*, so a dev cannot discover from the
method list that "any event" is even an option — they only learn it by noticing one overload
omits the string. The `08` doc names the filter (`AnyServerPushEvent`/`NamedServerPushEvent`,
`09`:232) but the *authoring surface* hides it.

**BEFORE → AFTER.** Keep the two terse overloads (they read fine for the common case) but
make the "any vs named" axis discoverable and consistent with the `08` filter vocabulary by
exposing the filter on the same nested-builder shape the rest of the cluster uses for
2+-argument configuration:

```csharp
// BEFORE — "any event" is the overload that happens to omit a string; not discoverable
t.ServerPush("/sse", p => …);                       // any event (implicit)
t.ServerPush("/sse", "order-placed", p => …);       // named event

// AFTER — both terse forms KEPT; plus a discoverable filter form for symmetry with the named carrier
t.ServerPush("/sse", p => …);                       // KEPT — any event, common case
t.ServerPush("/sse", "order-placed", p => …);       // KEPT — named event, common case
```

This is a **confirm-and-keep**, not a churn: the two terse overloads are the right
ergonomics for the common case and the naming sheet locks `ServerPush` as KEEP (`09`:228).
The only grammar-shape note is that the typed-payload reshape (`<TPayload>` adding a
positional callback param) folds into the **A2** fix — once `p.Payload` carries the payload,
`ServerPush<TPayload>(url, eventType, p => …)` has the same callback shape as its untyped
twin, so the three overloads collapse from *six distinct callback signatures* (untyped/typed
× the arity reshape) to *three url/eventType arities × one callback shape*. Counted under A2;
listed here only to confirm `ServerPush`'s own surface needs no further change.

---

## B. Pipeline cluster (`PipelineBuilder<TModel>`)

### B1. `Plugin` has **eight** overloads spanning three different return families — the worst orthogonality hole in the cluster

**Current shape.** `PipelineBuilder` exposes eight `Plugin*` entry points that return three
*different* builder families:

| Member | Returns | Source |
|---|---|---|
| `Plugin<T>(pluginName, member)` | `PluginMemberBuilder<T,TModel>` (read face) | `ast-grammar-pipeline.md:61` |
| `Plugin<T>(pluginName)` | `PluginMemberBuilder<T,TModel>` | `:62` |
| `PluginProperty<T>(pluginName, member)` | `TypedPluginPropertySource<T>` | `:63` |
| `Plugin<T>(PluginFunction<T> function)` | `PluginMemberBuilder<T,TModel>` | `:64` |
| `Plugin<T>(PluginProperty<T> property)` | `TypedPluginPropertySource<T>` | `:65` |
| `Plugin(pluginName, member)` | `PluginCallBuilder<TModel>` (void-call face) | `:66` |
| `Plugin(pluginName)` | `PluginCallBuilder<TModel>` | `:67` |
| `Plugin(PluginCommand command)` | `PluginCallBuilder<TModel>` | `:68` |

**Property hurt: orthogonality + discoverability + least-surprise.** The *return type silently
flips* on the type argument: `Plugin<T>(…)` opens a **read/value** builder, bare `Plugin(…)`
opens a **void-call** builder, and `PluginProperty<T>(…)` opens yet a third **source** type —
all spelled `Plugin`. A dev cannot predict from `p.Plugin(...)` whether they are about to
read a value, fire a void command, or get a typed source; they must read source to learn the
return family. The `08` algebra already proves these are **one** declaration spine over
`{property, function, command}` (`08`:760-790, "every arity overload reaches the same
`(member, returns, argShapes)` triple") and §6.4(c) mandates "one args-builder + one
declaration spine for plugins."

**BEFORE → AFTER.** Name the *member kind* the way the plugin declarer already does
(`09`:404-410: `Function` = value, `Command` = void, `Property` = read), so the return family
is screamed by the verb, not hidden in the presence of `<T>`:

```csharp
// BEFORE — three return families all spelled `Plugin`; return flips on the type arg
p.Plugin<DateInfo>("dates", "today")     // → read builder
 …
p.Plugin("clipboard", "copy")            // → void-call builder
 …
p.PluginProperty<string>("url", "path")  // → source

// AFTER — the member kind is the verb; return family is unambiguous and discoverable
p.PluginFunction<DateInfo>("dates", "today")   // → PluginReadBuilder  (value lane)
 …
p.PluginCommand("clipboard", "copy")           // → PluginCallBuilder  (void lane)
 …
p.PluginProperty<string>("url", "path")        // → TypedPluginPropertySource (read lane, KEPT — already correct)
```

This aligns one-concept-one-name with the declarer's `Function`/`Command`/`Property` trio
(`09`:404-410) and the `PluginMemberBuilder → PluginReadBuilder` rename (`09`:409). The
strongly-typed `PluginFunction<T>`/`PluginCommand`/`PluginProperty<T>` *object* overloads
(`:64`, `:65`, `:68`) fold in as the same three verbs taking the typed handle instead of two
strings — **zero capability lost**, the eight overloads become three verbs each with a
(name,member) and a typed-handle arity. The plugin escape hatch stays stringly at the
boundary (HARD RULE preserved); only the pipeline *entry verbs* are disambiguated.

---

### B2. `From` array-source intake is split across **four** members with inconsistent shapes — orthogonality + the §6.3 seam hole

**Current shape.** Four array-source entry points on the pipeline:

| Member | Returns | Source |
|---|---|---|
| `From<TElement>(TypedSource<TElement[]> source)` | `ReactiveArray<TElement>` | `ast-grammar-pipeline.md:81` |
| `From<TArgs,TElement>(TArgs args, Expression<Func<TArgs,TElement[]>> selector)` | `ReactiveArray<TElement>` | `:82` |
| `FromDom(elementId, member)` | `ReactiveArray<string>` | `:83` |
| `FromDom<TElement>(elementId, member)` | `ReactiveArray<TElement>` | `:84` |

**Property hurt: composability (the §6.3 seam) + orthogonality.** The abstract-`TypedSource`
overload (`:81`) is the *general* entry, but the `(args, selector)` overload (`:82`) is a
second spelling for "read an array off an event payload" — the same `(payload, path)`
duplication §A2 and §B5 also exhibit. Per `08` §6.3, the gather/value spine must accept the
**abstract** `TypedSource<TProp>` everywhere; the `(args, selector)` form is just
`From(FromEvent(args, selector))` once a `TypedSource` factory exists. Keeping it as a
separate overload re-mints the payload-read path locally.

**BEFORE → AFTER.**

```csharp
// BEFORE — (args, selector) is a second spelling of "array from a payload"
p.From(args, e => e.Items)            // payload-array overload
 …
p.From(someTypedSource)               // abstract overload

// AFTER — one abstract intake; payload-array folds through the SAME TypedSource factory
p.From(FromEvent(args, e => e.Items)) // payload-array via the one source factory
 …
p.From(someTypedSource)               // KEPT — the one abstract intake
```

Keep `From<TElement>(TypedSource<TElement[]>)` as the single abstract intake and the two
`FromDom` overloads (the `Dom` suffix screams the external boundary — KEEP per `09`:295). Fold
the `(args, selector)` overload into the shared `FromEvent` factory the conditions/gather
areas already use, so "array from a payload" reads identically to "value from a payload"
everywhere. This is the exact §6.3 widening applied to the array entry: `cod(FromEvent) =
TypedSource = dom(From)`. Capability preserved; one fewer redundant spelling.

---

### B3. `Dispatch` literal-payload vs source-payload split reads inconsistently — least-surprise

**Current shape.** Three dispatch entries:
`Dispatch(eventName)` `:51`, `Dispatch<TPayload>(eventName, TPayload payload)` `:52`,
`DispatchWith<TPayload>(eventName, Action<DispatchPayloadBuilder<TPayload,TModel>> configure)`
`:53` (renamed `DispatchFrom` per `09`:242).

**Property hurt: consistency.** This one is **mostly already good** — the naming sheet's
`DispatchWith → DispatchFrom` rename (`09`:242) deliberately pairs the lanes:
`Dispatch(name, literal)` (compile-time literal object) vs `DispatchFrom(name, b => …)` (fields
from live sources). The remaining wart is purely that the *literal* lane takes a positional
`TPayload payload` (a wide arg) while the *source* lane takes a builder callback — so the two
lanes do not read alike when both appear in a TALL chain.

**BEFORE → AFTER (minor, optional).**

```csharp
// BEFORE — literal lane is a wide positional arg; source lane is a TALL callback
p.Dispatch("saved", new SavedPayload { Id = 7 })      // positional object literal
 .DispatchFrom<SavedPayload>("saved", b => b           // TALL builder callback
     .Set(x => x.Id).From(p.Component<Grid>(...).SelectedId()));

// AFTER — keep BOTH; the literal lane stays terse (it IS the easy case), source lane stays TALL
p.Dispatch("saved", new SavedPayload { Id = 7 })      // KEPT — literal is genuinely the terse case
 .DispatchFrom<SavedPayload>("saved", b => b
     .Set(x => x.Id).From(p.Component<Grid>(...).SelectedId()));
```

**Confirm-and-keep.** The literal-vs-source split is *correct* orthogonality (compile-time
literal vs runtime source are genuinely different lanes, `09`:241-242) and the positional
literal is the right ergonomic for the easy case — forcing a builder callback on a static
object would *add* ceremony. No change required; listed to record that the split was
evaluated and is sound.

---

### B4. `Get`/`Post`/`Put`/`Delete` inline-gather is asymmetric across verbs — easy-to-write + consistency

**Current shape.**

| Member | Has inline-gather callback? | Source |
|---|---|---|
| `Get(url)` | no | `ast-grammar-pipeline.md:71` |
| `Post(url)` | no | `:72` |
| `Post(url, Action<GatherBuilder>)` | **yes** | `:73` |
| `Put(url, Action<GatherBuilder>)` | **yes** | `:74` |
| `Delete(url)` | no | `:75` |

**Property hurt: consistency + easy-to-write (Directive-3 defaults).** The inline-gather
shorthand exists for `Post` and `Put` but **not** for `Get` or `Delete`, and `Get` has no
gather overload at all. There is no domain reason for the asymmetry — a `Get` legitimately
gathers query-string values and a `Delete` legitimately gathers route-params/headers (`08`
§3.6: `RequestInputTarget ∈ {payload, header, route-param}` applies to every verb). A dev who
learned `p.Post(url, g => g.Include(...))` is surprised that `p.Get(url, g => …)` does not
exist and must drop to the chained `.Gather(...)` form. The grammar should offer the *same*
shape for the same concept on every verb.

**BEFORE → AFTER.**

```csharp
// BEFORE — inline gather only on Post/Put; Get/Delete force the chained form
p.Post("/orders", g => g.Include(m => m.Total));   // inline — OK
p.Get("/orders").Gather(g => g.Include(...));       // forced chained — inconsistent

// AFTER — every verb offers BOTH the bare and the inline-gather shape, symmetrically
p.Post("/orders", g => g.Include(m => m.Total));   // KEPT
p.Get("/orders",  g => g.Include(m => m.Filter));   // ADDED — symmetric inline gather
p.Delete("/orders/{id}", g => g.RouteParam("id", …)); // ADDED — symmetric inline gather
```

Add the `(url, Action<GatherBuilder>)` overload to `Get` and `Delete` so all four verbs offer
the identical bare/inline pair. The chained `.Gather(...)` form (HTTP cluster) stays as the
TALL spelling for many-line gathers; the inline form is the easy-write shorthand for the
one-or-two-field common case. Pure addition — no existing call changes, every verb now reads
the same.

---

### B5. `When` opens with **three** entries, two of which re-mint the `(payload, path)` read — orthogonality vs the §6.3 spine

**Current shape.**

| Member | Returns | Source |
|---|---|---|
| `When<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path)` | `ConditionSourceBuilder<TModel,TProp>` | `ast-grammar-pipeline.md:77` |
| `When<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path)` | `ConditionSourceBuilder<TModel,TProp>` | `:78` |
| `When<TProp>(TypedSource<TProp> source)` | `ConditionSourceBuilder<TModel,TProp>` | `:79` |

**Property hurt: orthogonality (the same hole §1.1 of the naming sheet closes for And/Or).**
The third overload (`:79`) takes the **abstract** `TypedSource<TProp>` — that is the canonical
flat shape. The first two (`:77`, `:78`) are *second spellings*: a payload read and a
response-body read each already yield a `TypedSource` via a factory (`FromEvent(args, path)`,
`responseBody.Read(path)`). The naming sheet **already decided** this exact collapse for the
guard composition (`09`:90, §1.1: "the `(payload, path)` and `(ResponseBody, path)` overloads
fold in via a `TypedSource` factory"). `When`'s entry overloads must fold the same way for
one-concept-one-shape consistency — otherwise And/Or and When disagree on how a payload read
is spelled.

**BEFORE → AFTER.**

```csharp
// BEFORE — When mints (payload, path) and (responseBody, path) locally
p.When(args, e => e.CareLevel).Eq("memory")          // payload overload
 …
p.When(body, r => r.Status).Eq("ok")                  // response-body overload
 …
p.When(someTypedSource).Truthy()                       // abstract overload

// AFTER — one abstract When; payload/response reads fold through the SAME factories And/Or use
p.When(FromEvent(args, e => e.CareLevel)).Eq("memory") // via the one source factory
 …
p.When(body.Read(r => r.Status)).Eq("ok")              // via responseBody.Read (the value spine)
 …
p.When(someTypedSource).Truthy()                       // KEPT — the one abstract intake
```

Collapse `When` to the single `When<TProp>(TypedSource<TProp>)` intake (`:79`); fold the
payload and response-body overloads into the `FromEvent(...)` / `responseBody.Read(...)`
factories that the And/Or grammar (`09` §1.1) and the gather spine (`08` §6.3) already use. One
`When` shape, one way to read a payload across `When`/`And`/`Or`/`Include`/`Set`/`Dispatch`.
**Every capability preserved** — the same payload and response-body conditions are
expressible, just through the one shared source factory instead of three `When` arities.

---

### B6. `Component` has **four** id-resolution overloads with no screaming distinction — discoverability

**Current shape.**

| Member | How it identifies the component | Source |
|---|---|---|
| `Component<TComponent>(Expression<Func<TModel,object>> expr)` | model expression (this model) | `ast-grammar-pipeline.md:55` |
| `Component<TComponent,TOtherModel>(Expression<Func<TOtherModel,object>> expr)` | model expression (cross-model) | `:56` |
| `Component<TComponent>(string refId)` | explicit id string | `:57` |
| `Component<TComponent>()` | layout singleton (well-known id) | `:58` |

**Property hurt: discoverability + least-surprise.** Four overloads of `Component` differ only
in *how you name the component*, but they read identically at the call site
(`p.Component<Grid>(...)`) and the dev cannot tell from the method list which arity does
what — the cross-model one (`:56`) in particular is easy to miss. The naming sheet KEEPs all
four ("overloads split on how you identify it", `09`:246), which is the right *capability*
set; the only wart is discoverability.

**BEFORE → AFTER (documentation/XML-doc, not a shape change).**

```csharp
// All four KEPT — this is the correct capability set (model-expr / cross-model / id / singleton).
p.Component<Grid, OrderModel>(m => m.Lines)   // cross-model — the one most likely to be missed
```

**Confirm-and-keep with a doc note.** No grammar-shape change: the four overloads are
genuinely distinct identification strategies and collapsing them would lose capability
(cross-model binding, layout singletons). The remedy is XML-doc discoverability (each overload's
`<summary>` names its strategy) — recorded here so the reviewer does not mistake the kept
overload set for an un-audited collision. Not counted as a grammar adjustment.

---

### B7. `Into` and `ShowValidationErrors` are bare `string`-id terminals on the pipeline — least-surprise (typed-id opportunity)

**Current shape.**
`Into(string elementId)` returns `PipelineBuilder` `:70`;
`ValidationErrors(string formId)` returns `PipelineBuilder` `:69` (renamed
`ShowValidationErrors`, `09`:251).

**Property hurt: least-surprise (minor) — consistency with plan-driven IDs.** Both take a raw
`string` element/form id. The framework's whole thesis is *plan-driven deterministic IDs*
(root `CLAUDE.md`: "the plan carries every ID the runtime needs"; `IdGenerator` produces ids
from model expressions). A raw-string id on `Into`/`ShowValidationErrors` is the one spot in
the reaction grammar where the dev hand-types an id that the framework could derive — a small
least-surprise gap versus the typed `Element(...)`/`Component(...)` resolution everywhere else.

**BEFORE → AFTER (additive overload, string form KEPT).**

```csharp
// BEFORE — raw string id only
p.Get("/order/{id}").Into("order-detail");
 …
p.Post("/save").ShowValidationErrors("order-form");

// AFTER — string form KEPT for explicit/non-input element ids; add a model-expression form
p.Get("/order/{id}").Into("order-detail");                       // KEPT — explicit element id
 …
p.Post("/save").ShowValidationErrors<OrderModel>(m => m);         // ADDED — derives the form id via IdGenerator
```

This is a **confirm-and-keep-plus-optional-add**: the string form is correct for
developer-chosen non-input element ids (root `CLAUDE.md`: "Non-input component IDs … are the
developer's responsibility"), so it stays. The optional model-expression overload lets a
validation-summary bound to a model derive its id the same deterministic way inputs do — no
capability lost, one less hand-typed id where the framework already knows it. Lower priority
than B1–B5; counted as one adjustment.

---

## C. Cross-cluster forcing functions from `08` §6 (grammar consequences)

### C1. Lane is re-detected at runtime, not carried — the grammar must *stamp* `sync`/`async`, never branch on `Promise`

**Current shape (consequence, not a `PipelineBuilder` signature).** The pipeline's terminal
verbs split structurally by lane already — `Get/Post/Put/Delete/Parallel` open async
sub-builders; `Set/Call/Dispatch/Element/Component/Into/ShowValidationErrors` are sync. But
`08` §6.2 (verified, `execute.ts:287`) shows the runtime re-detects the lane via
`result instanceof Promise` instead of reading a carried tag.

**Property hurt: least-surprise + determinism (D3).** This is not a `PipelineBuilder` *naming*
issue — the naming sheet renames `ReactionLane → ReactionTiming` (`09`:205, §2) and that is
locked. The **grammar consequence** is: the pipeline builder's draft sequencer must *stamp*
`ReactionTiming { Sync, Async }` onto each emitted reaction node at lower-time (it already
structurally separates the lanes — Request/Parallel are distinct `K_R` arms), so the runtime
routes on the carried tag and the `instanceof Promise` probe is deleted.

**BEFORE → AFTER.** No author-facing signature changes; the adjustment is that
`PipelineBuilder`'s draft (the internal `ReactionPipelineDraft` accumulator,
`ast-grammar-pipeline.md:86-94`) stamps `timing: sync|async` on every node it builds. This is
an **internal** grammar-lowering obligation, already mandated by `09` §2 and `08` §6.2 —
recorded here so the pipeline-cluster implementation closes it. **Not counted** as an
author-facing grammar adjustment (it is internal lowering), but flagged so the pipeline
builder is not implemented without it.

### C2. `Include`/gather intake must widen to abstract `TypedSource` (§6.3) — already covered by B2/B5

The §6.3 widening ("widen `Include`'s intake from concrete `TypedComponentSource`/
`TypedPluginSource` to the abstract `TypedSource<TProp>`") is an HTTP-cluster member, not a
`PipelineBuilder` member, so it is out of this critique's two clusters. But it is the **same
forcing function** behind B2 (`From` array intake) and B5 (`When` intake): every value-reading
entry on the pipeline must accept the abstract `TypedSource`, with concrete-source spellings
folding through one factory. B2 and B5 apply that rule to the two pipeline members it touches;
no separate adjustment is needed here.

---

## D. What is ALREADY good — do NOT churn

These shapes read well cold and must be preserved verbatim. Recording them prevents
churn-for-novelty.

| Shape | Why it reads well | Source |
|---|---|---|
| `TriggerBuilder` methods all `ReturnsSelf` | Triggers chain and repeat — `t.PageLoad(…).Event(…).SignalR(…)` reads TALL, one trigger per line. | `ast-grammar-entry-triggers.md:42-43` |
| Every trigger callback hands back `PipelineBuilder<TModel>` | One clean nesting point per trigger — the grammar nests cleanly (composability), the pipeline grammar is reached the same way from every trigger. | `:49-56` |
| `Dispatch`/`DispatchFrom`/`ShowValidationErrors`/`Into` are `ReturnsSelf` chainable terminals | Multiple sync reactions chain off one `p` and read top-to-bottom — the core TALL property of the reaction grammar. | `ast-grammar-pipeline.md:51,53,69,70` |
| `Element(id)` → `ElementBuilder` / `Component<…>(…)` → `ComponentRef` open clean sub-grammars | Each sub-builder owns its continuation (`SetText`/`AddClass` on `ElementBuilder`; `Set`/`Call` on `ComponentRef`) — narrow, discoverable, no god-object. | `:54-58` |
| `Get/Post/Put/Delete` verbs + `{placeholder}` route templates | HTTP verbs read cold; the inline-gather overloads (where present) are legitimate shorthands, not synonyms. | `:71-75` |
| `Parallel(params Action<HttpRequestBuilder>[] branches)` | `branches` is the right noun; `params` reads as "run these concurrently" — one terse, correct shape. | `:76` |
| `Confirm(message)` → `GuardBuilder` | The distinct name marks the async user-decision lane (`09` §1.6); composes via `And` on the guard surface. | `:80` |
| `FromUrl<T>(paramName)` / `FromDom<T>(id, member)` typed source entries | `From*` voice is consistent across URL/DOM/event/HTTP; `Dom` suffix screams the boundary. | `:59-60, :83-84` |
| `Then`/`ElseIf`/`Else` first-match chain (reached inside `ConditionSourceBuilder`) | Universally-read if/else-if/else; nesting back into a `PipelineBuilder` is the clean recursion point. | naming sheet `09`:260, AST nesting note `:20-21` |
| `ReactivePlan()` / `ResolvePlan()` factories | One produces a root plan, one a partial slot plan — the `PlanScope` discriminant is screamed by the factory name, not a flag. | `ast-grammar-entry-triggers.md:31-32` |

The four trigger families (`PageLoad`/`Event`/`ServerPush`/`SignalR`) and the reaction-verb
set (`Set`/`Call`/`Dispatch`/`Element`/`Component`/`When`/`Get…`/`Parallel`/`Inject`/`Confirm`/
`ShowValidationErrors`) are each one-concept-one-name after the locked renames — the cluster's
*vocabulary* is sound. Every adjustment above is a **shape** fix (arity, callback-vs-positional,
return type, seam type, default), never a vocabulary change, and none drops a capability.

---

## E. Proposed adjustments — summary ledger

| # | Cluster | Adjustment | Property improved | Capability preserved? |
|---|---|---|---|---|
| **A1** | Triggers | `Html.On` returns the `ReactivePlan` (chainable) instead of `void`; free-function form kept | TALL-reading, composability | yes (both spellings) |
| **A2** | Triggers | Typed payload read off `p.Payload` (uniform callback shape) instead of leading positional `(payload, p)` arity | consistency, discoverability | yes (typed lane kept) |
| **B1** | Pipeline | `Plugin` 8-overload return-flip split into screaming verbs `PluginFunction`/`PluginCommand`/`PluginProperty` | orthogonality, discoverability, least-surprise | yes (all 3 families + typed handles) |
| **B2** | Pipeline | `From(args, selector)` array intake folds into the one `FromEvent`+abstract-`TypedSource` intake (§6.3) | composability, orthogonality | yes (payload-array kept via factory) |
| **B4** | Pipeline | Add symmetric inline-gather overload to `Get` and `Delete` (match `Post`/`Put`) | consistency, easy-to-write | yes (pure addition) |
| **B5** | Pipeline | `When` 3-overload `(payload,path)`/`(responseBody,path)` fold into the one abstract `TypedSource` intake (mirrors `09` §1.1 And/Or) | orthogonality, consistency | yes (both reads kept via factory) |
| **B7** | Pipeline | Add model-expression overload to `Into`/`ShowValidationErrors`; string form kept | least-surprise (plan-driven ids) | yes (string form kept) |

**7 grammar-shape adjustments** (A1, A2, B1, B2, B4, B5, B7).

Recorded but **not counted** as author-facing grammar adjustments: A3 (folds into A2), B3
(confirm-and-keep — split is sound), B6 (confirm-and-keep — XML-doc discoverability only), C1
(internal lowering obligation from `08` §6.2 / `09` §2), C2 (the §6.3 forcing function behind
B2/B5, an HTTP-cluster member out of these two clusters).

*Every adjustment preserves every capability (zero feature loss = zero tech debt) and
reconciles with the locked names in `09-dsl-naming-sheet.md` and the determinism discoveries
in `08-determinism-formalization.md`.*
