# entry-triggers

DSL grammar (AST edges) for the **Entry + Triggers** cluster, extracted from real
public builder signatures. Every row is a real `public` method with a `file:line`.

Source roots:
- `Alis.Reactive/Razor/Extensions/` — `PlanExtensions.cs`, `HtmlExtensions.cs`, `InputFieldExtensions.cs`
- `Alis.Reactive/Builders/TriggerBuilder.cs`
- `Alis.Reactive/Razor/InputBoundField.cs` (+ base) — the `Returns` target of `InputField`

Notes:
- Extension methods are declared twice physically under `#if NET48` (`HtmlHelper<TModel>` host)
  vs `#else` (`IHtmlHelper<TModel>` host). They are one logical public method. The `Source`
  column cites the netcore (`IHtmlHelper`) line as canonical; the net48 twin line is noted in
  the row text.
- **NESTING (recursion):** a `Callback` that hands back a `PipelineBuilder<TModel>` is a recursion
  point into the pipeline grammar (a different cluster).
- **ReturnsSelf = yes:** the member returns its own receiver type, so it can be chained / repeated
  (e.g. multiple triggers on one `TriggerBuilder`).

Legend: `Callback` = the callback param type if any, else `-`. `ReturnsSelf` = yes if it returns
its own receiver type. `Source` = relative `file:line`.

## Html (Razor entry extensions on IHtmlHelper&lt;TModel&gt;)

These are the four `Html.*` entry points. Each is an extension method, so its "receiver" is the
Razor `Html` helper (`IHtmlHelper<TModel>` / net48 `HtmlHelper<TModel>`).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| Html | ReactivePlan&lt;TModel&gt;() | ReactivePlan&lt;TModel&gt; | - | no | Alis.Reactive/Razor/Extensions/PlanExtensions.cs:43 (net48: :39) |
| Html | ResolvePlan&lt;TModel&gt;() | ReactivePlan&lt;TModel&gt; | - | no | Alis.Reactive/Razor/Extensions/PlanExtensions.cs:70 (net48: :66) |
| Html | On&lt;TModel&gt;(ReactivePlan&lt;TModel&gt; plan, Action&lt;TriggerBuilder&lt;TModel&gt;&gt; trigger) | void | Action&lt;TriggerBuilder&lt;TModel&gt;&gt; | no | Alis.Reactive/Razor/Extensions/HtmlExtensions.cs:53 |
| Html | InputField&lt;TModel,TProp&gt;(ReactivePlan&lt;TModel&gt; plan, Expression&lt;Func&lt;TModel,TProp&gt;&gt; expression) | InputBoundField&lt;TModel,TProp&gt; | - | no | Alis.Reactive/Razor/Extensions/InputFieldExtensions.cs:33 |
| Html | InputField&lt;TModel,TProp&gt;(ReactivePlan&lt;TModel&gt; plan, Expression&lt;Func&lt;TModel,TProp&gt;&gt; expression, Action&lt;InputFieldOptions&gt; configure) | InputBoundField&lt;TModel,TProp&gt; | Action&lt;InputFieldOptions&gt; | no | Alis.Reactive/Razor/Extensions/InputFieldExtensions.cs:55 |
| Html | RenderPlan&lt;TModel&gt;(ReactivePlan&lt;TModel&gt; plan) | IHtmlContent (net48: IHtmlString) | - | no | Alis.Reactive/Razor/Extensions/PlanExtensions.cs:126 (net48: :105) |

The `On` callback `Action<TriggerBuilder<TModel>>` is the entry edge into the trigger grammar below.

## TriggerBuilder&lt;TModel&gt;

The fluent trigger API handed to the `Html.On` callback. Every method returns the builder itself
(`ReturnsSelf = yes`), so triggers chain and repeat: `t.DomReady(...).CustomEvent(...).SignalR(...).ServerPush(...)`.
Every trigger callback hands back a `PipelineBuilder<TModel>` — each is a NESTING point into the
pipeline grammar.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| TriggerBuilder&lt;TModel&gt; | DomReady(Action&lt;PipelineBuilder&lt;TModel&gt;&gt; pipeline) | TriggerBuilder&lt;TModel&gt; | Action&lt;PipelineBuilder&lt;TModel&gt;&gt; — NESTING | yes | Alis.Reactive/Builders/TriggerBuilder.cs:26 |
| TriggerBuilder&lt;TModel&gt; | CustomEvent(string eventName, Action&lt;PipelineBuilder&lt;TModel&gt;&gt; pipeline) | TriggerBuilder&lt;TModel&gt; | Action&lt;PipelineBuilder&lt;TModel&gt;&gt; — NESTING | yes | Alis.Reactive/Builders/TriggerBuilder.cs:38 |
| TriggerBuilder&lt;TModel&gt; | CustomEvent&lt;TPayload&gt;(string eventName, Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; pipeline) where TPayload : new() | TriggerBuilder&lt;TModel&gt; | Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; — NESTING (typed payload) | yes | Alis.Reactive/Builders/TriggerBuilder.cs:51 |
| TriggerBuilder&lt;TModel&gt; | ServerPush(string url, Action&lt;PipelineBuilder&lt;TModel&gt;&gt; pipeline) | TriggerBuilder&lt;TModel&gt; | Action&lt;PipelineBuilder&lt;TModel&gt;&gt; — NESTING | yes | Alis.Reactive/Builders/TriggerBuilder.cs:67 |
| TriggerBuilder&lt;TModel&gt; | ServerPush(string url, string eventType, Action&lt;PipelineBuilder&lt;TModel&gt;&gt; pipeline) | TriggerBuilder&lt;TModel&gt; | Action&lt;PipelineBuilder&lt;TModel&gt;&gt; — NESTING | yes | Alis.Reactive/Builders/TriggerBuilder.cs:80 |
| TriggerBuilder&lt;TModel&gt; | ServerPush&lt;TPayload&gt;(string url, string eventType, Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; pipeline) where TPayload : new() | TriggerBuilder&lt;TModel&gt; | Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; — NESTING (typed payload) | yes | Alis.Reactive/Builders/TriggerBuilder.cs:94 |
| TriggerBuilder&lt;TModel&gt; | SignalR(string hubUrl, string methodName, Action&lt;PipelineBuilder&lt;TModel&gt;&gt; pipeline) | TriggerBuilder&lt;TModel&gt; | Action&lt;PipelineBuilder&lt;TModel&gt;&gt; — NESTING | yes | Alis.Reactive/Builders/TriggerBuilder.cs:111 |
| TriggerBuilder&lt;TModel&gt; | SignalR&lt;TPayload&gt;(string hubUrl, string methodName, Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; pipeline) where TPayload : new() | TriggerBuilder&lt;TModel&gt; | Action&lt;TPayload, PipelineBuilder&lt;TModel&gt;&gt; — NESTING (typed payload) | yes | Alis.Reactive/Builders/TriggerBuilder.cs:126 |

## ReactivePlan&lt;TModel&gt;

The entry receiver produced by `Html.ReactivePlan` / `Html.ResolvePlan` and consumed by `On`,
`InputField`, and `RenderPlan`. Public surface below. (`RegisterPlugin` is the plugin-boundary
escape hatch — a different cluster — listed here only because it is public on the entry receiver.)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ReactivePlan&lt;TModel&gt; | PlanId { get; } | string | - | no | Alis.Reactive/ReactivePlan.cs:44 |
| ReactivePlan&lt;TModel&gt; | IsPartial { get; } | bool | - | no | Alis.Reactive/ReactivePlan.cs:46 |
| ReactivePlan&lt;TModel&gt; | RegisterPlugin(string pluginName, Action&lt;PluginTypeBuilder&gt; configure) | void | Action&lt;PluginTypeBuilder&gt; | no | Alis.Reactive/ReactivePlan.cs:53 |
| ReactivePlan&lt;TModel&gt; | RegisterPlugin(Plugin plugin) | void | - | no | Alis.Reactive/ReactivePlan.cs:65 |
| ReactivePlan&lt;TModel&gt; | RegisterPlugin&lt;TPlugin&gt;() | TPlugin | - | no | Alis.Reactive/ReactivePlan.cs:72 |
| ReactivePlan&lt;TModel&gt; | Render() | string | - | no | Alis.Reactive/ReactivePlan.cs:90 |
| ReactivePlan&lt;TModel&gt; | Render(IServiceProvider services) | string | - | no | Alis.Reactive/ReactivePlan.cs:96 |
| ReactivePlan&lt;TModel&gt; | RenderFormatted() | string | - | no | Alis.Reactive/ReactivePlan.cs:103 |
| ReactivePlan&lt;TModel&gt; | RenderFormatted(IServiceProvider services) | string | - | no | Alis.Reactive/ReactivePlan.cs:109 |

## InputBoundField&lt;TModel,TProp&gt; (return target of Html.InputField)

The value `Html.InputField` returns. It is a public **type** but exposes **no public members**:
its constructor and `Render` are `internal` (component extensions chain on it from outside this
cluster). Its public-readable state comes from the base class `InputBoundFieldBase`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| InputBoundFieldBase&lt;THelper,TModel,TProp&gt; | Helper { get; } | THelper | - | no | Alis.Reactive/InputField/InputBoundFieldBase.cs:22 |
| InputBoundFieldBase&lt;THelper,TModel,TProp&gt; | Plan { get; } | ReactivePlan&lt;TModel&gt; | - | no | Alis.Reactive/InputField/InputBoundFieldBase.cs:25 |
| InputBoundFieldBase&lt;THelper,TModel,TProp&gt; | Expression { get; } | Expression&lt;Func&lt;TModel,TProp&gt;&gt; | - | no | Alis.Reactive/InputField/InputBoundFieldBase.cs:28 |
| InputBoundFieldBase&lt;THelper,TModel,TProp&gt; | Options { get; } | InputFieldOptions | - | no | Alis.Reactive/InputField/InputBoundFieldBase.cs:31 |

## Cluster edge summary

Entry flow (all grounded above):

```
Html.ReactivePlan<TModel>()  ─┐
Html.ResolvePlan<TModel>()   ─┴─> ReactivePlan<TModel>
                                    │
   Html.On(plan, t => …) ───────────┤  Action<TriggerBuilder<TModel>>
                                    │      └─> TriggerBuilder<TModel> (chainable: DomReady/CustomEvent/ServerPush/SignalR)
                                    │             └─ each pipeline callback ──NESTING──> PipelineBuilder<TModel>  (other cluster)
                                    │
   Html.InputField(plan, m => …) ───┤  ─> InputBoundField<TModel,TProp>  (component extension chains on it — other cluster)
                                    │
   Html.RenderPlan(plan) ───────────┘  ─> IHtmlContent  (closes the view)
```
