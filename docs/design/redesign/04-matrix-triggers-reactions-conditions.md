# Determinism Matrix — Triggers, Reactions, Conditions

> Band 1 of the fresh-design coverage matrix. Source-grounded against the actual
> public DSL, plan model, generated wire shapes, and runtime executors — not
> against the prior atlas. Module names and concept names come from
> [`02-micro-modules.md`](./02-micro-modules.md) and [`03-naming.md`](./03-naming.md).
>
> **Sources read for this band (the requirement):**
> - Trigger: `Alis.Reactive/Builders/TriggerBuilder.cs`, `Alis.Reactive/PlanModel/StartsWhen.cs`,
>   `Alis.Reactive/PlanModel/Behavior.cs`, `Alis.Reactive/ComponentOnboarding/ComponentEventOnboarding.cs`,
>   `Alis.Reactive/PlanModel/PlanBuildContext.cs` (`WireComponentEvent`),
>   `Alis.Reactive.Assets/runtime/execution/trigger.ts`.
> - Reaction: `Alis.Reactive/Builders/PipelineBuilder.cs` (incl. `ValidationErrors`:263,
>   `Into`:273), `ElementBuilder.cs`, `DispatchPayloadBuilder.cs`, `ReactionPipelineDraft.cs`,
>   `Alis.Reactive/PlanModel/ReactionGraph.cs` (incl. `InjectReaction`:424,
>   `ShowValidationErrorsReaction`:443), `Alis.Reactive.Assets/runtime/execution/execute.ts`
>   (incl. `executeInject`:207, `executeShowValidationErrors`:220),
>   `Alis.Reactive.Assets/runtime/execution/inject.ts` (`injectHtml`).
> - Condition: `Alis.Reactive/Builders/PipelineBuilder.Conditions.cs`,
>   `Builders/Conditions/{ConditionStart,ConditionSourceBuilder,GuardBuilder,BranchBuilder,ConditionContinuation,TypedSource,TypedComponentSource,PayloadTypedSource}.cs`,
>   `Alis.Reactive/PlanModel/{ConditionGraph,CompareOp}.cs`,
>   `Alis.Reactive.Assets/runtime/conditions/{conditions,sync-condition}.ts`.
> - Wire shapes: `Alis.Reactive.Assets/runtime/types/plan.ts`.

---

## How to read a matrix row

Each row is a self-contained proof that **one deterministic input** walked through
the **new micro-modules** produces **exactly one output** — both the plan-JSON
shape and the browser behavior it must achieve. The point is that generating the
C# slice, the wire node, and the runtime handler is *mechanical*: nothing in a row
is a judgement call.

| Column | Meaning |
|---|---|
| **Feature / variant** | The single public DSL capability the row pins. Variants are parameterizations of the same lowering. |
| **Input (DSL the developer writes)** | The exact authoring call. |
| **Module interaction path** | The ordered new micro-modules touched and what each does to the data, `→` author/lower side, `⇒` runtime/read side. |
| **Output** | The exact plan-JSON node (camelCase wire shape) **and** the browser behavior the runtime must produce. |
| **Good default** | The one value chosen when the developer says nothing — the choice that removes a decision. |

**Lane legend.** `SYNC` = the reaction returns `void`, runs in the same browser
tick (so Syncfusion `args.cancel` / `args.preventDefaultAction` are visible when SF
re-reads them). `ASYNC` = returns `Promise<void>`; the only async openers in this
band are **Confirm** (user decision) and a **branch/sequence that reaches one**.
HTTP/parallel are async but belong to the Request band. `inject` and
`show-validation-errors` are **SYNC** reaction verbs (`executeInject` /
`executeShowValidationErrors` return `void`): `inject` consumes a success body the
surrounding request already awaited, and `show-validation-errors` renders into a
container in-tick. In the fresh design the lane is a **plan-carried fact**
(`ReactionLane`) stamped by `ReactionPipelineDraft`, not re-discovered at runtime by
`instanceof Promise`.

---

## Parameterization model — why this scales to thousands of cases

The band has a small number of *lowering templates*. Every concrete authored call
is one template instantiated by a finite set of axes. A code generator enumerates
the axes and emits one deterministic case each. The axes:

| Axis | Domain (finite, from source) | Used by |
|---|---|---|
| **TriggerKind** | `page-ready`, `document-event`, `component-event`, `server-push`, `signalr` | Trigger rows |
| **PayloadContract** | `untyped`, `typed(T)` | every trigger/dispatch that carries a payload |
| **ReactionKind** | `set`, `call`, `dispatch`, `branch`, `sequence`, `inject`, `show-validation-errors` *(+ `request`, `parallel` open the async lane and belong to the Request band)* | Reaction rows |
| **TargetSource** | `component` (model-bound id or explicit id or app-level), `payload` (event/success/error scope), `plugin` (call only) | `set` / `call` |
| **ValueSource** (`ValueExpression`) | `literal`, `read(component)`, `read(url)`, `read(plugin)`, `read(payload-path)`, `read(whole-payload)`, `read(whole-element)`, `object`, `array` | every value slot (set value, call arg, condition operand, dispatch field) |
| **CompareOp** | the 21 tokens grouped into 9 operand-shape families (below) | Condition rows |
| **OperandForm** | `unary`, `literal-binary`, `text-literal`, `array`, `range`, `min-length`, `collection-item`, `source-vs-source` | condition right operand |
| **GuardComposition** | `single`, `all` (And), `any` (Or), `not` | guard graph |
| **BranchPosition** | `then`, `else-if`, `else` | first-match routing |
| **Continuation** | `pipeline` (has `Then`), `branch` (after a `Then`), `standalone` (no `Then` — **unrepresentable** in the fresh design) | condition entry context |

A generated case is the tuple `(template, axis values…)`. Determinism holds because
each tuple has exactly one lowering and one runtime reader — there is no point in
the walk where two outputs are possible for one input.

---

## Trigger band — `Trigger` module

`Html.On(plan, t => …)` → `TriggerBuilder` lowers each call to one `Behavior`
(`StartsWhen` + `ReactionGraph`). The runtime `wireTrigger` (today `trigger.ts`)
attaches one browser listener per `StartsWhen.kind` and feeds the originating
payload into one `ExecutionContext`.

**Shared lowering template (T):** `t.<TriggerKind>(args, pipeline)` →
`PipelineBuilder` builds a `ReactionGraph` (`BuildReaction`) → `StartsWhen.<kind>(args)`
node → `Behavior.On(startsWhen, reaction)` → appended to `BehaviorGraph` on the
`PlanDocument`. The `<TPayload>` overloads add `PayloadContract.ForPayload(typeof(T))`;
all others use `PayloadContract.Untyped`.

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **PageReady** | `t.DomReady(p => …)` | `Trigger →` build pipeline → `StartsWhen.PageReady()`; `Trigger ⇒` `wireTrigger` case `page-ready`. | `{ "startsWhen": { "kind": "page-ready" }, "reaction": … }`. Browser: if `readyState==="loading"` run on `DOMContentLoaded`, else run immediately, with empty `ExecutionContext`. | Fire once on load. No payload. |
| **CustomEvent (untyped)** | `t.CustomEvent("ready", p => …)` | `Trigger →` `StartsWhen.DocumentEvent("ready", Untyped)`; `⇒` `addEventListener("ready")`, payload = `CustomEvent.detail` else the `Event`. | `{ "kind":"document-event", "event":"ready", "payloadType":{"kind":"untyped"} }`. Browser: every dispatch of `ready` runs the reaction with `ExecutionContext.event(detail)`. | `payloadType: untyped`. |
| **CustomEvent (typed)** | `t.CustomEvent<OrderReady>("ready", (e, p) => …)` | `Trigger →` `StartsWhen.DocumentEvent("ready", ForPayload(OrderReady))`; the `e` instance is a *phantom* used only to author typed `p.When(e, x=>x.Total)` reads. `⇒` identical listener; payload is read by path. | `{ "kind":"document-event","event":"ready","payloadType":{"kind":"typed","payloadType":"OrderReady"} }`. Browser: same as untyped; the contract enables typed payload-path reads. | `e` carries no value; only its shape. |
| **ComponentEvent** (via `.Reactive()`) | `…NativeTextBox(b=>b.Reactive(plan, evt=>evt.Changed,(args,p)=>…))` | `Component →` resolves deterministic id; `ComponentEventOnboarding.Wire` → `PlanBuildContext.WireComponentEvent(id, vendor, eventName, reaction)` → `StartsWhen.ComponentEvent(id, eventName)`. `Trigger ⇒` `wireTrigger` case `component-event` resolves the `BrowserObject`, looks up the event channel via `BrowserObjectContract`, and uses `ComponentDriver` (`wireFusionEvent`/`wireNativeEvent`) to attach. | `{ "kind":"component-event","component":"<id>","event":"<eventName>" }`. Browser: the vendor event fires the reaction with `ExecutionContext.event(eventData)`; **SYNC** so `args.cancel` is visible to SF. | `component` = id from model expr or builder `ElementId`; vendor seam is the **only** vendor-aware code. |
| **ServerPush (any event)** | `t.ServerPush("/sse", p => …)` | `Trigger →` `StartsWhen.ServerPush(url, AnyEvent())`; `⇒` `wireServerPush` opens EventSource, AbortSignal-scoped. | `{ "kind":"server-push","url":"/sse","eventFilter":{"kind":"any","payloadType":{"kind":"untyped"}} }`. Browser: each SSE message runs the reaction. **ASYNC opener** (remote trigger) — listener wiring is sync, each delivery runs on its own tick. | `eventFilter: any`, `untyped`. |
| **ServerPush (named)** | `t.ServerPush("/sse","tick", p=>…)` | `Trigger →` `StartsWhen.ServerPush(url, NamedEvent("tick"))`. | `{…,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"untyped"}}}`. Browser: only `tick` SSE events fire the reaction. | named filter, untyped. |
| **ServerPush (named, typed)** | `t.ServerPush<Vitals>("/sse","tick",(e,p)=>…)` | `Trigger →` `NamedEvent("tick", ForPayload(Vitals))`. | `{…,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"typed","payloadType":"Vitals"}}}`. | typed payload contract on the filter. |
| **SignalR** | `t.SignalR("/hub","OnTick",p=>…)` | `Trigger →` `StartsWhen.SignalR(hubUrl, method, Untyped)`; `⇒` `wireSignalR`. | `{ "kind":"signalr","hubUrl":"/hub","method":"OnTick","payloadType":{"kind":"untyped"} }`. Browser: hub method invocation runs the reaction. **ASYNC opener** (remote trigger). | untyped. |
| **SignalR (typed)** | `t.SignalR<Vitals>("/hub","OnTick",(e,p)=>…)` | `Trigger →` `SignalR(hubUrl, method, ForPayload(Vitals))`. | `{…,"payloadType":{"kind":"typed","payloadType":"Vitals"}}`. | typed contract. |
| **Multiple triggers** | `t.DomReady(…).CustomEvent("x",…)` | `Trigger →` each chained call appends an **independent** `Behavior`. | `behaviors: [Behavior_1, Behavior_2]`. Browser: each wires independently; order of wiring = author order. | Each trigger is its own behavior; no implicit sharing. |

**Trigger band parameterization.** A row = `(TriggerKind × PayloadContract)`. 5 kinds
× {untyped, typed} (page-ready has no payload axis) = the rows above. A generator
emits: the C# `StartsWhen.<Kind>` factory + `TriggerBuilder` overload, the wire
interface (`<Kind>Trigger`), and the `wireTrigger` switch case. Each is fixed by
the kind; the payload axis only toggles `payloadType` presence.

---

## Reaction band — `Reaction` module

`PipelineBuilder` (the `p` sink) emits `ReactionGraph` nodes; `ReactionPipelineDraft`
sequences sync runs, branches, and async openers, and (fresh design) **stamps the
`ReactionLane`** onto each node. `executeReaction` ⇒ routes on `kind` + carried lane
via `switch` + `assertNever`.

**Shared lowering template (R):** `p.<verb>(…)` → `ReactionGraph.<Kind>(…)` node →
`draft.AddCommand(node)` (sync) or `BeginBranch`/`BeginHttp`/`BeginParallel` (lane
openers) → `BuildReaction()` collapses to a single node when there is one block,
else a `sequence`.

### Sequencing and the lane (the spine that makes ordering deterministic)

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Single command** | `p.Element("s").Show()` | `Reaction →` one node, `BuildReaction` returns it directly (no wrapper). | the bare node (e.g. a `set`). Browser: runs it. **SYNC**. | No `sequence` wrapper for one node. |
| **Ordered sync commands** | `p.Element("a").Show(); p.Dispatch("x")` | `Reaction →` `_pendingSyncReactions` accumulates in author order → `FlushPendingSyncReactions` → one `sequence`. `⇒` `executeSequence` runs steps in array order. | `{ "kind":"sequence","steps":[ set…, dispatch… ] }`. Browser: commands run top-to-bottom, same tick. **SYNC**. | Declaration order = execution order. |
| **Sync then async opener then sync** | `p.Element(..).Show(); p.Get(..)…; p.Element(..).Hide()` | `Reaction →` draft flushes the pre-sync block, appends the async node, flushes the trailing sync block → ordered blocks `[seq, request, seq]`. `⇒` `executeSequence` detects the lane crossing **from the carried lane**, awaits, then continues. | `sequence` of `[sequence(sync), request, sequence(sync)]`. Browser: sync block runs, request awaited, then trailing sync block. **ASYNC** from the request onward. | The lane boundary is the async opener; everything before stays sync. |

### `set` reactions — `SetReaction` (template R, ReactionKind=`set`)

`p.Element(id).<mutation>` and component property writes lower to `set` on a
`component`-kind `Source`. Event-arg mutation lowers to `set` on a `payload`-kind
`Source`.

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Element show** | `p.Element("box").Show()` | `Reaction → ElementBuilder` declares element + property contract (`hidden`, write) → `set(ComponentSource("box"),"hidden", literal false)`. `⇒` `executeSet` resolves `RuntimeObject("box")`, `.set("hidden", false)`. | `{ "kind":"set","on":{"kind":"component","component":"box"},"property":"hidden","value":{"kind":"literal","value":false,"shape":{…bool}} }`. Browser: element becomes visible. **SYNC**. | `Show`=`hidden:false`, `Hide`=`hidden:true` — no separate verb node. |
| **Element set text (literal)** | `p.Element("s").SetText("hi")` | same path, property `text`, value literal string. | `{…,"property":"text","value":{"kind":"literal","value":"hi","shape":{string}}}`. Browser: `textContent="hi"`. **SYNC**. | literal value, `Shape.String`. |
| **Element set text (from value source)** | `p.Element("s").SetText(p.FromUrl("q"))` | `Reaction →` value = `Value` slice `read(url,"q")`. `⇒` `evaluateValue` reads url param, sets text. | `{…,"property":"text","value":{"kind":"read","from":{"kind":"url"},"member":"q",…}}`. Browser: text = the URL param. **SYNC**. | value flows through the one `ValueExpression` path. |
| **Element set HTML** | `p.Element("s").SetHtml(src)` | property `html`. | `{…,"property":"html",…}`. Browser: `innerHTML`=value. **SYNC**. | property `html`. |
| **Component property write** | `p.Component<X>(m=>m.Field).Set(c=>c.Enabled, true)` | `Reaction →` `set(ComponentSource(id),"enabled", literal true)`; contract declared write. `⇒` `RuntimeObject(id).set("enabled", true)` via `ComponentDriver`. | `{ "kind":"set","on":{"kind":"component","component":"<id>"},"property":"enabled","value":{literal true} }`. Browser: vendor property updated. **SYNC**. | id from model expression. |
| **Event-arg mutation** (`set` on payload) | inside `.Reactive((args,p)=>…)`, `p` sets an `args.*` member | `Reaction →` `set(PayloadSource(event),"cancel", literal true)`. `⇒` `executeSet` case `payload`: writes into the live event arg object. | `{ "kind":"set","on":{"kind":"payload","scope":"event","type":…},"property":"cancel","value":{literal true} }`. Browser: SF reads `args.cancel` after callback returns — must be **SYNC**. | scope `event`; sync is mandatory, not optional. |

### `call` reactions — `CallReaction` (template R, ReactionKind=`call`)

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Element CSS class** | `p.Element("b").AddClass("on")` | `Reaction → ElementBuilder.Call` declares method contract → `call(ComponentSource("b"),"addClass",[literal "on"])`. `⇒` `executeCall` → `RuntimeObject.call`. | `{ "kind":"call","on":{"kind":"component","component":"b"},"method":"addClass","args":[{literal "on"}] }`. Browser: class added. **SYNC**. | `AddClass/RemoveClass/ToggleClass` are calls, not sets. |
| **Component method (no return)** | `p.Component<Grid>(…).Call(g=>g.Refresh())` | `Reaction →` `call(ComponentSource(id),"refresh",[])`. `⇒` `RuntimeObject.call("refresh",[])`. | `{…,"method":"refresh","args":[]}`. Browser: vendor method invoked. **SYNC**. | empty args = `[]`. |
| **Component method (args)** | `…Call(g=>g.SelectRow(2))` | `Reaction →` args lowered each through `ValueExpression` (`Value` slice). | `{…,"args":[{literal 2}]}`. Browser: invoked with evaluated args. **SYNC**. | each arg = one `ValueExpression`. |
| **Plugin command** | `p.Plugin("url","push").Arg(...).Fire()` | `Reaction →` `Plugin` slice declares command contract → `call(PluginSource,"push",args)`. `⇒` `executeCall` case `plugin` → `PluginCatalog` instance `.call`. | `{ "kind":"call","on":{"kind":"plugin","name":"url","type":…},"method":"push","args":[…] }`. Browser: plugin operation runs. **SYNC** (a plugin command is sync unless it itself returns a Promise — escape hatch). | call-only target (no `set` on plugin). |
| **Event-arg method call** | inside `.Reactive`, `args.UpdateData(...)` style | `Reaction →` `call(PayloadSource(event),"updateData",args)`. `⇒` `executeCall` case `payload`: calls the function member on the live arg. | `{ "kind":"call","on":{"kind":"payload","scope":"event"},"method":"updateData","args":[…] }`. Browser: arg method invoked in-tick. **SYNC**. | scope `event`. |

### `dispatch` reactions — `DispatchReaction` (template R, ReactionKind=`dispatch`)

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Dispatch (no payload)** | `p.Dispatch("saved")` | `Reaction →` `dispatch("saved", None)`. `⇒` `executeDispatch` → `document.dispatchEvent(new CustomEvent("saved",{detail:{}}))`. | `{ "kind":"dispatch","event":"saved","payload":{"kind":"none"} }`. Browser: fires the event; any `t.CustomEvent("saved")` listener runs. **SYNC**. | empty `{}` detail. |
| **Dispatch (literal payload)** | `p.Dispatch("saved", new Msg{Id=1})` | `Reaction →` payload = `LiteralRaw(obj, shape)`, contract typed. | `{…,"payload":{"kind":"value","data":{"kind":"literal","value":{…},"shape":…},"payloadType":{typed}}}`. Browser: detail = the literal object. **SYNC**. | compile-time literal object. |
| **Dispatch (source-backed payload)** | `p.DispatchWith<Msg>("saved", b=>b.Set(x=>x.Total, src))` | `Reaction → DispatchPayloadBuilder` builds an `object` `ValueExpression` whose fields are each a `ValueExpression` (literal or read). `⇒` `evaluateValue` resolves the object at dispatch time. | `{…,"payload":{"kind":"value","data":{"kind":"object","fields":{"total":{read…}}},"payloadType":{typed}}}`. Browser: detail object assembled from live sources. **SYNC**. | object node; each field = one `ValueExpression`. |

### `inject` reaction — `InjectReaction` (template R, ReactionKind=`inject`)

`p.Into(elementId)` (`Builders/PipelineBuilder.cs:273-279`) is the single inject
authoring verb. It declares the target element on the `PlanBuildContext`
(`Context.DeclareElement(elementId)`) and emits one `inject` node whose value is
**fixed** at lowering time to `ValueExpression.ReadWholePayload(PayloadSource.Success())`
(the HTTP success whole-body — the `WholeElement`/`WholePayload` variant the **Value**
module owns, never a magic member name). The node is `ReactionGraph.Inject(slot, value)`
→ `InjectReaction` (`PlanModel/ReactionGraph.cs:424`, `kind:"inject"`, `slot` (string,
from `ComponentKey`), `value` (`ValueExpression`)). Runtime `executeInject`
(`execution/execute.ts:207`) resolves the element by `slot`, evaluates the value, and
sets the element's HTML via `injectHtml` — **SYNC** (no Promise opener; it consumes a
success body that the surrounding request already awaited).

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Inject success body into element** | `p.Get("/card").Into("card-host")` (the `Into` follows a request, per the C# doc-comment) | `Reaction →` `Into(elementId)` declares the element, builds `value = Value` slice `ReadWholePayload(Success)` and emits `ReactionGraph.Inject(slot, value)`; the draft stamps **SYNC**. `Value →` the whole-payload(success) read variant. `⇒` `executeInject` reads `plan.components.element(slot)`, `evaluateValue(value)` → the success body string, `injectHtml(container, html, slot)` sets `innerHTML`. | `{ "kind":"inject","slot":"card-host","value":{"kind":"read","from":{"kind":"payload","scope":"success"},"whole":true} }`. Browser: the element's `innerHTML` is replaced by the HTTP success body. **SYNC** (within the success scope of the awaited request). | value is **always** `ReadWholePayload(Success)` — the developer supplies only the element id; no value axis to choose. |

### `show-validation-errors` reaction — `ShowValidationErrorsReaction` (template R, ReactionKind=`show-validation-errors`)

`p.ValidationErrors(formId)` (`Builders/PipelineBuilder.cs:263-267`) is a distinct
public reaction verb that emits one `show-validation-errors` node
(`ReactionGraph.ShowValidationErrors(container)` → `ShowValidationErrorsReaction`,
`PlanModel/ReactionGraph.cs:443`, `kind:"show-validation-errors"`, `container` (string,
from `ComponentId`)). It is the **Reaction** edge into the **Validation** display
surface, addressing one **Component** container. Runtime `executeShowValidationErrors`
(`execution/execute.ts:220`) resolves the container and, if a server validation payload
is in scope (e.g. after an HTTP error route), shows server errors there; otherwise it
runs client `validateContainer` for the container — **SYNC**.

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Show accumulated validation errors** | `p.ValidationErrors("resident-form")` | `Reaction →` emits `ReactionGraph.ShowValidationErrors(container)`; draft stamps **SYNC**. `Validation ⇒` `validationOrchestrator`/`errorDisplay` render into the container; `Component ⇒` `RuntimeComponents` resolves the container by id. `⇒` `executeShowValidationErrors`: server payload present → `showServerErrors(container)`, else `validateContainer(container)`. | `{ "kind":"show-validation-errors","container":"resident-form" }`. Browser: accumulated validation errors are rendered in the named container (server errors after a failed request, else current client-rule results). **SYNC**. | container id is **required** — no implicit fallback container. |

### `branch` reactions — `BranchReaction` (the Condition module's reaction edge)

These rows live in the Condition band because the guard is a `ConditionGraph`, but
the *executable* is a `branch` `ReactionGraph`. They are listed there to keep the
first-match proof in one place.

**Reaction band parameterization.** A row = `(ReactionKind × TargetSource × ValueSource)`.
`set`/`call` enumerate over `TargetSource ∈ {component, payload, plugin(call-only)}`
and each value/arg over the 9 `ValueSource` variants; `dispatch` enumerates over
`{none, literal, object}` payload; `inject` and `show-validation-errors` are
**fixed-shape verbs** (`inject` carries a developer element id + the constant
`ReadWholePayload(Success)` value; `show-validation-errors` carries a developer
container id) — neither has a value/target axis to enumerate. The generator emits the
C# builder verb, the `ReactionGraph.<Kind>` node, the wire interface, and the
`executeReaction` case — all fixed by `ReactionKind`; `TargetSource` only selects the
`executeSet`/`executeCall` inner switch arm. **One open question is parked below**
(set-target on `plugin`).

---

## Condition band — `Condition` module

`p.When(...)` / `Confirm(...)` author a `ConditionGraph`; `.Then/.ElseIf/.Else`
route it into `BranchCase`s on a `branch` `ReactionGraph`. The fresh design uses
ONE `CompareEngine` for both lanes — `evaluateCondition` (sync) and a thin
`confirmThenEvaluate` async wrapper — replacing today's two divergent evaluators.

**Shared lowering template (C):**
`When(source)` → `ConditionSourceBuilder<TProp>` (carries `source.ToValueExpression()`
as the left operand + `source.Shape`) → an operator method builds
`ConditionGraph.Compare(op, operands)` → `ConditionContinuation.Wrap` returns a
`GuardBuilder` → `.Then(pipeline)` produces a `BranchCase.Of(condition, reaction)`
on the pipeline's `branch`; `.ElseIf` adds another conditional case; `.Else` adds a
`BranchCase.Default(reaction)`. First match wins at runtime (`executeBranchFrom`
returns on the first matching guard).

### Condition entry / source (left operand)

| Feature / variant | Input (DSL) | Module interaction path | Output (left operand JSON) | Good default |
|---|---|---|---|---|
| **From typed source (component)** | `p.When(p.Component<X>(m=>m.Care).Value(c=>c.Level))` | `Condition →` left = `Value` `read(component,id,member)`, `shape` from `TProp`. | `"left":{"kind":"read","from":{"kind":"component","component":"<id>"},"member":"level","shape":…}`. | shape inferred from `TProp`. |
| **From URL** | `p.When(p.FromUrl<int>("page")).Gt(1)` | left = `read(url,"page")`, shape Number. | `"left":{"kind":"read","from":{"kind":"url"},"member":"page",…}`. | typed `FromUrl<T>` sets shape. |
| **From plugin read** | `p.When(p.PluginProperty<bool>("net","online"))` | `Plugin` slice declares read; left = `read(plugin,…)`. | `"left":{"kind":"read","from":{"kind":"plugin",…}}`. | |
| **From event payload** | `p.When(args, x=>x.Total)` | `Condition →` `PayloadTypedSource.FromEvent(path)` → left = `read(payload,event, path)`. | `"left":{"kind":"read","from":{"kind":"payload","scope":"event"},"member":"…","path":{…}}`. | scope `event`. |
| **From HTTP response body** | `p.When(success, x=>x.Status)` | left = `read(payload, success/error, path)` (Response band supplies the `ResponseBody`). | `"left":{…,"from":{"kind":"payload","scope":"success"}…}`. | scope from the `ResponseBody`. |

### Compare operators — the 9 operand-shape families (21 tokens)

Each family is one wire interface `( <Family>CompareCondition )` and one
right-operand wire shape. `CompareEngine` ⇒ has one arm per family. `shape` is the
left/operand value shape; `itemShape` is `none` except for `array-contains`.

| Family (OperandForm) | Ops | Input example | Right-operand JSON | Runtime semantics (must achieve) |
|---|---|---|---|---|
| **Unary** | `truthy`,`falsy`,`is-null`,`not-null`,`is-empty`,`not-empty` | `.Truthy()`, `.NotNull()`, `.IsEmpty()` | `{"kind":"none"}` | `truthy`=`!!shaped`; `falsy`=`!shaped`; `is-null`/`not-null` test raw `null`/`undefined`; `is-empty`=`""`, missing, or `[]`; `not-empty`=negation. |
| **Equality** | `eq`,`neq` | `.Eq(5)` | `{"kind":"value","value":{literal}}` | `shaped(left) === shaped(right)`; `neq` negates. |
| **Ordered** | `gt`,`gte`,`lt`,`lte` | `.Gt(3)` | `{value:{literal}}` | numeric/string/bool ordered compare; mismatched types ⇒ `false` (no throw). |
| **Membership** | `in`,`not-in` | `.In("a","b")` | `{value:{array}}` | `array.includes(shaped(left))`; `not-in` negates. |
| **Range** | `between` | `.Between(1,10)` | `{value:{array[2]}}` | inclusive `lower ≤ left ≤ upper`; un-orderable ⇒ `false`. |
| **Text** | `contains`,`starts-with`,`ends-with` | `.Contains("xyz")` | `{value:{textLiteral}}` | string predicate on `toString(left)`; non-text left ⇒ `false`. |
| **Regex** | `matches` | `.Matches("^A")` | `{value:{textLiteral}}` | `new RegExp(pattern).test(text(left))`; non-text ⇒ `false`. |
| **TextLength** | `min-length` | `.MinLength(3)` | `{value:{numericLiteral}}` | `text(left).length >= n`; non-text ⇒ `false`. |
| **CollectionItem** | `array-contains` | `.ArrayContains(item)` | `{value:{literal}}`, `itemShape` set | shape each array item by `itemShape`, then `items.includes(item)`; non-array ⇒ `false`. |

Every compare node is `{ "kind":"compare","left":…, "op":"<token>","right":…, "shape":…,"itemShape":… }`.
**Source-vs-source** (`.Eq(otherSource)`, `.Gt(otherSource)`) is the same families
with the right operand's `value` being a `read` `ValueExpression` instead of a
`literal` — one extra OperandForm axis, no new family.

### Guard composition (And / Or / Not)

| Feature / variant | Input (DSL) | Module interaction path | Output (graph JSON) | Good default |
|---|---|---|---|---|
| **Single guard** | `p.When(s).Eq(1)` | `Condition →` one `compare` node. | the `compare` node. | bare compare, no wrapper. |
| **And (chained)** | `p.When(s).Eq(1).And(s2).Gt(0)` | `Condition →` `GuardBuilder.And` composes via `ConditionComposition.All(existing)`; flattens nested `all`. | `{ "kind":"all","terms":[compare,compare] }`. Runtime: short-circuits to `false` on first false. | flattened `all` (no nested `all`). |
| **Or (chained)** | `…Eq(1).Or(s2).Eq(2)` | `ConditionComposition.Any(existing)`, flattens nested `any`. | `{ "kind":"any","terms":[…] }`. Runtime: short-circuits to `true` on first true. | flattened `any`. |
| **And (nested group)** | `.And(inner => inner.When(s).Gt(0))` | `Condition →` builds inner via a `ConditionStart` then `All(flatten(existing)+flatten(inner))`. | `{ "kind":"all","terms":[…inner terms…] }`. | nested group flattened into one `all`. |
| **Or (nested group)** | `.Or(inner => …)` | symmetric → `any`. | `{ "kind":"any","terms":[…] }`. | flattened `any`. |
| **Not** | `p.When(s).Eq(1).Not()` | `Condition →` `ConditionGraph.Not(current)`. | `{ "kind":"not","term":{compare} }`. Runtime: inverts. | single child. |

### Branch routing (first-match) — produces a `branch` `ReactionGraph`

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Then** | `p.When(s).Eq(1).Then(p2=>…)` | `Condition →` `PipelineConditionContinuation.Then` builds the branch reaction, creates `BranchCase.Of(cond, reaction)`, calls `draft.SetConditionalBranches([case])`. `Reaction ⇒` `executeBranch` → `executeBranchFrom` evaluates guard, runs first match. | `{ "kind":"branch","cases":[{"guard":{"kind":"when","condition":…},"reaction":…}] }`. Browser: reaction runs only if guard true. **SYNC** (compare guard). | one case; no implicit else. |
| **ElseIf** | `.ElseIf(s).Gt(0).Then(p3=>…)` | `BranchBuilder.ElseIf` returns a `ConditionSourceBuilder` bound to a `BranchConditionContinuation`; `.Then` appends another conditional `BranchCase` to the **same** `cases` list. | `cases:[ when…, when… ]`. Browser: evaluated top-to-bottom, first match wins, rest skipped. **SYNC**. | appended in author order. |
| **Else** | `.Else(p4=>…)` | `BranchBuilder.Else` appends `BranchCase.Default(reaction)`; guards against post-`Else` additions (`InvalidOperationException` at author time). | `cases:[ …, {"guard":{"kind":"default"},"reaction":…} ]`. Browser: runs only if no prior case matched. **SYNC**. | default is always last; only one allowed. |
| **No match** | (guards all false, no `Else`) | `Reaction ⇒` `executeBranchFrom` loops, logs `branch.no-match`, returns void. | runtime no-op. Browser: nothing runs. **SYNC**. | silent no-op (not an error). |

### Confirm guard (the one async opener in this band)

| Feature / variant | Input (DSL) | Module interaction path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Confirm then run** | `p.Confirm("Delete?").Then(p2=>…)` | `Condition →` `ConditionGraph.Confirm(message)` as the guard. `⇒` `confirmThenEvaluate` awaits `window.alis.confirm(message)`, then routes the branch. | `{ "kind":"branch","cases":[{"guard":{"kind":"when","condition":{"kind":"confirm","message":"Delete?"}},…}] }`. Browser: shows confirm dialog; reaction runs only on accept. **ASYNC** (user decision). | guard composes like any other condition; missing dialog is a real boundary error (throws). |
| **Confirm AND compare** | `p.Confirm("Sure?")` then `.And(s).Gt(0)`... (via guard composition) | `Condition →` `all([confirm, compare])`. `⇒` if a reached term is `confirm`, the whole evaluation lifts to a Promise; compares short-circuit first (sync) per `all` semantics. | `{ "kind":"all","terms":[{confirm},{compare}] }`. Browser: compares may avoid the dialog (short-circuit). **ASYNC if confirm reached**. | confirm is just another term in `all`/`any`. |

**Condition band parameterization.** A row = `(SourceKind × CompareFamily × OperandForm × GuardComposition × BranchPosition × Continuation)`.
The generator enumerates the 9 compare families × their token set, crosses with the
4 source kinds (component/url/plugin/payload), the 4 compositions (single/all/any/not),
and the 3 branch positions. Each tuple maps to: a `ConditionSourceBuilder` operator
method (C#), a `<Family>CompareCondition` wire interface, and a `CompareEngine` arm
(runtime). All are fixed by the family; the source kind only changes the `left`
`ValueExpression`, the operand form only changes the `right` shape, the composition
only changes the wrapping node, and the branch position only changes which
`BranchCase` factory is called. No tuple has two outputs.

**Fresh-design determinism wins recorded in these rows.**
- `Standalone.Then` is **unrepresentable** (it threw at runtime today). In the fresh
  design the `standalone` `Continuation` exposes no `Then`, so the only way to reach
  a branch is from a pipeline or branch continuation — a compile error replaces a
  runtime throw.
- One `CompareEngine` serves both lanes; `confirm` is the only term that wraps async.
  The 21 tokens come from **one** `CompareOp` source (the dual-evaluator divergence
  is gone).
- The branch lane is **carried** in the plan (`ReactionLane`): a branch whose every
  guard is a compare is stamped SYNC; a branch reaching a `confirm` is stamped ASYNC.
  `executeReaction` routes on that fact instead of probing `instanceof Promise`.

---

## Coverage count for this band

**Features + variants made deterministic: 52.**

| Sub-band | Features / variants |
|---|---|
| Triggers | 11 (page-ready; custom-event untyped/typed; component-event; server-push any/named/named-typed; signalr untyped/typed; multiple-triggers) |
| Reaction sequencing & lane | 3 (single command; ordered sync; sync-async-sync) |
| `set` reactions | 6 (element show/hide; set-text literal; set-text source; set-html; component property write; event-arg set) |
| `call` reactions | 5 (element css class; component method no-arg; component method with args; plugin command; event-arg method) |
| `dispatch` reactions | 3 (no payload; literal payload; source-backed object payload) |
| `inject` reaction | 1 (`p.Into(elementId)` — success body into element, SYNC) |
| `show-validation-errors` reaction | 1 (`p.ValidationErrors(formId)` — accumulated errors into container, SYNC) |
| Condition source (left) | 5 (component; url; plugin; event payload; response body) |
| Compare operator families | 9 families covering all 21 tokens, **+1** source-vs-source operand form |
| Guard composition | 6 (single; and-chain; or-chain; and-group; or-group; not) |
| Branch routing | 4 (then; else-if; else; no-match) |
| Confirm | 2 (confirm-then; confirm-in-composition) |

(9 compare families is counted as 9 + 1 source-vs-source = the 10 entries summed
above; the 21 individual operator tokens are sub-variants enumerated by the
parameterization, not separate template rows. The two added flat verbs — `inject`
and `show-validation-errors` — bring the band to 52.)

---

## Cases I could NOT make fully deterministic (and why)

Three edges are deterministic in *behavior* but carry an open design decision that
the fresh design must resolve before code generation is purely mechanical. None
blocks the band; each is a naming/representation choice, recorded here per the
"if the row can't be written from source, stop and read more source" rule.

1. **`set` target on a `plugin` source.** The wire `SetTargetSource` admits only
   `component` and `payload` (plan.ts:371–373) — a plugin can be *called* but a
   property `set` on a plugin object is not representable, while `CallTargetSource`
   *does* include `plugin`. The browser-object model says any member that can be
   written is a settable target, so this asymmetry is a representation gap, not a
   behavior gap. Until the fresh `Plugin`+`Reaction` slices decide whether plugin
   property writes are in scope, the `set`×`plugin` tuple has **no** lowering and is
   excluded from generation. *Why undecided:* the DSL source exposes no
   `Plugin(...).Set(...)` verb today, so there is no authored intent to lower —
   inventing one would violate "no information the plan does not carry."

2. **`dom`-kind source vs `component`-kind source for element mutation.** The wire
   `Source` union has both a `DomSource` (`{kind:"dom",element}`) and a
   `ComponentSource` (`{kind:"component",component}`), and `ReadExpression` has a
   `DomPropertyReadExpression` reading from `dom`. But `ElementBuilder` lowers every
   element mutation to a **`component`** source (`ComponentSource.Of(_componentKey)`),
   never `dom`. So the runtime element-write path is deterministic (always
   `component`), yet the `dom` read/source variant exists in the contract with no
   authoring path in *this* band that produces it for a write. The fresh `Value`/`Component`
   slices must decide whether `dom` is a live variant or dead vocabulary to delete.
   *Why undecided:* it is reachable from the Value band (`read(dom,...)`), so it
   cannot be judged dead from the trigger/reaction/condition band alone — it is
   deferred to the Value-band matrix, not resolved by guessing here.

3. **`payload` scope `local` (and `element`) on the wire enum.** `PayloadScope`
   (plan.ts:400–407) lists `local` and `element`; `02-micro-modules.md` already flags
   `local` as the **dead scope** to remove and `element`/`success`/`error` as the
   live ones. For the reaction/condition band, every authored payload read/write uses
   `event`, `success`, or `error` — `local` has no DSL producer here. The behavior is
   deterministic (no row emits `local`), but whether the fresh `Request`/`Value`
   slices delete `local` from the enum is their call. *Why undecided:* deleting an
   enum member is a cross-band serialization change owned by the `Request` band's
   scope-fold, not this band — recording the edge rather than acting on it follows
   the Wrong Plan Protocol.

Everything else in the band — all 5 trigger kinds × payload axis, all `set`/`call`/`dispatch`
forms, all 9 compare families/21 operators, all 4 guard compositions, all 3 branch
positions, and confirm — has exactly one input→output lowering and one runtime
reader, provably from source, and is ready for mechanical generation.
