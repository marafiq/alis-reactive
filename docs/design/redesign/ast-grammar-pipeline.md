# pipeline

AST grammar edges for the **PipelineBuilder** cluster, extracted GROUNDED from real
public builder signatures (not sandbox usage, not docs). Each row is one real public
method `Receiver -> Member(paramShape) -> Returns` with a `file:line`.

Cluster = `PipelineBuilder<TModel>`, a single `public partial class` split across 4 source
files. `ReactionPipelineDraft<TModel>` (the 5th file) is `internal sealed` — it has **zero
public members**, so it contributes no grammar edges (it is the private accumulator behind
`_draft`). It is listed at the end for completeness with a note.

Receiver for every row is `PipelineBuilder<TModel>` (the `p` parameter inside trigger callbacks).

## Reading the columns

- **Callback** — the callback parameter type if the member accepts one, else `-`.
  A callback that hands back a `PipelineBuilder` would be a **NESTING (recursion) point**.
  None of PipelineBuilder's own members do that — its only callbacks hand back *sub-builders*
  (`DispatchPayloadBuilder`, `GatherBuilder`, `HttpRequestBuilder`), so nesting back into a
  pipeline happens one level down (inside `ConditionSourceBuilder.Then`, `Response.OnSuccess`,
  etc.), not on PipelineBuilder itself.
- **ReturnsSelf** — `yes` when the member returns `PipelineBuilder<TModel>` (chainable /
  repeatable: multiple terminal commands chain off one `p`). `no` when it returns a
  sub-builder that opens a new fluent sub-grammar (the continuation lives on that type).

## NESTING / REPEAT flags

- **ReturnsSelf=yes (chainable terminals):** `Dispatch`, `Dispatch<TPayload>`, `DispatchWith`,
  `ValidationErrors`, `Into`. These return `p` itself, so they can be chained/repeated freely
  within one pipeline.
- **NESTING (sub-grammar openers):** every other member returns a *different* builder type
  (`ElementBuilder`, `ComponentRef`, `HttpRequestBuilder`, `ParallelBuilder`,
  `ConditionSourceBuilder`, `GuardBuilder`, `ReactiveArray`, `PluginMemberBuilder`,
  `PluginCallBuilder`, `TypedUrlSource`, `TypedPluginPropertySource`). Each opens its own
  AST sub-tree; the pipeline recursion (callback handing back a `PipelineBuilder`) is reached
  inside those sub-builders, not here.
- **Callback (sub-builder configurators):** `DispatchWith` (`Action<DispatchPayloadBuilder>`),
  `Post(url, gather)` / `Put(url, gather)` (`Action<GatherBuilder>`), `From<TArgs,TElement>`
  (`Expression<Func<TArgs,TElement[]>>` — captured into a plan read, NOT invoked),
  `Parallel` (`params Action<HttpRequestBuilder>[]` — repeated branch configurators),
  `When<TPayload,TProp>` (`Expression<Func<TPayload,TProp>>` path — captured, NOT invoked).

## PipelineBuilder&lt;TModel&gt;

`public partial class PipelineBuilder<TModel> : IReactionEmitter where TModel : class`
(ctor `internal`; `Context` `internal`). Source files: `PipelineBuilder.cs`,
`PipelineBuilder.Http.cs`, `PipelineBuilder.Conditions.cs`, `PipelineBuilder.Arrays.cs`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PipelineBuilder&lt;TModel&gt; | Dispatch(string eventName) | PipelineBuilder&lt;TModel&gt; | - | yes | Builders/PipelineBuilder.cs:43 |
| PipelineBuilder&lt;TModel&gt; | Dispatch&lt;TPayload&gt;(string eventName, TPayload payload) | PipelineBuilder&lt;TModel&gt; | - | yes | Builders/PipelineBuilder.cs:54 |
| PipelineBuilder&lt;TModel&gt; | DispatchWith&lt;TPayload&gt;(string eventName, Action&lt;DispatchPayloadBuilder&lt;TPayload,TModel&gt;&gt; configure) | PipelineBuilder&lt;TModel&gt; | Action&lt;DispatchPayloadBuilder&lt;TPayload,TModel&gt;&gt; | yes | Builders/PipelineBuilder.cs:75 |
| PipelineBuilder&lt;TModel&gt; | Element(string elementId) | ElementBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.cs:92 |
| PipelineBuilder&lt;TModel&gt; | Component&lt;TComponent&gt;(Expression&lt;Func&lt;TModel,object&gt;&gt; expr) | ComponentRef&lt;TComponent,TModel&gt; | - | no | Builders/PipelineBuilder.cs:101 |
| PipelineBuilder&lt;TModel&gt; | Component&lt;TComponent,TOtherModel&gt;(Expression&lt;Func&lt;TOtherModel,object&gt;&gt; expr) | ComponentRef&lt;TComponent,TModel&gt; | - | no | Builders/PipelineBuilder.cs:114 |
| PipelineBuilder&lt;TModel&gt; | Component&lt;TComponent&gt;(string refId) | ComponentRef&lt;TComponent,TModel&gt; | - | no | Builders/PipelineBuilder.cs:127 |
| PipelineBuilder&lt;TModel&gt; | Component&lt;TComponent&gt;() | ComponentRef&lt;TComponent,TModel&gt; | - | no | Builders/PipelineBuilder.cs:136 |
| PipelineBuilder&lt;TModel&gt; | FromUrl(string paramName) | Conditions.TypedUrlSource&lt;string&gt; | - | no | Builders/PipelineBuilder.cs:147 |
| PipelineBuilder&lt;TModel&gt; | FromUrl&lt;T&gt;(string paramName) | Conditions.TypedUrlSource&lt;T&gt; | - | no | Builders/PipelineBuilder.cs:155 |
| PipelineBuilder&lt;TModel&gt; | Plugin&lt;T&gt;(string pluginName, string member) | PluginMemberBuilder&lt;T,TModel&gt; | - | no | Builders/PipelineBuilder.cs:164 |
| PipelineBuilder&lt;TModel&gt; | Plugin&lt;T&gt;(string pluginName) | PluginMemberBuilder&lt;T,TModel&gt; | - | no | Builders/PipelineBuilder.cs:179 |
| PipelineBuilder&lt;TModel&gt; | PluginProperty&lt;T&gt;(string pluginName, string member) | Conditions.TypedPluginPropertySource&lt;T&gt; | - | no | Builders/PipelineBuilder.cs:194 |
| PipelineBuilder&lt;TModel&gt; | Plugin&lt;T&gt;(PluginFunction&lt;T&gt; function) | PluginMemberBuilder&lt;T,TModel&gt; | - | no | Builders/PipelineBuilder.cs:206 |
| PipelineBuilder&lt;TModel&gt; | Plugin&lt;T&gt;(PluginProperty&lt;T&gt; property) | Conditions.TypedPluginPropertySource&lt;T&gt; | - | no | Builders/PipelineBuilder.cs:215 |
| PipelineBuilder&lt;TModel&gt; | Plugin(string pluginName, string member) | PluginCallBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.cs:226 |
| PipelineBuilder&lt;TModel&gt; | Plugin(string pluginName) | PluginCallBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.cs:240 |
| PipelineBuilder&lt;TModel&gt; | Plugin(PluginCommand command) | PluginCallBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.cs:253 |
| PipelineBuilder&lt;TModel&gt; | ValidationErrors(string formId) | PipelineBuilder&lt;TModel&gt; | - | yes | Builders/PipelineBuilder.cs:263 |
| PipelineBuilder&lt;TModel&gt; | Into(string elementId) | PipelineBuilder&lt;TModel&gt; | - | yes | Builders/PipelineBuilder.cs:273 |
| PipelineBuilder&lt;TModel&gt; | Get(string url) | HttpRequestBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.Http.cs:11 |
| PipelineBuilder&lt;TModel&gt; | Post(string url) | HttpRequestBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.Http.cs:19 |
| PipelineBuilder&lt;TModel&gt; | Post(string url, Action&lt;GatherBuilder&lt;TModel&gt;&gt; gather) | HttpRequestBuilder&lt;TModel&gt; | Action&lt;GatherBuilder&lt;TModel&gt;&gt; | no | Builders/PipelineBuilder.Http.cs:25 |
| PipelineBuilder&lt;TModel&gt; | Put(string url, Action&lt;GatherBuilder&lt;TModel&gt;&gt; gather) | HttpRequestBuilder&lt;TModel&gt; | Action&lt;GatherBuilder&lt;TModel&gt;&gt; | no | Builders/PipelineBuilder.Http.cs:31 |
| PipelineBuilder&lt;TModel&gt; | Delete(string url) | HttpRequestBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.Http.cs:39 |
| PipelineBuilder&lt;TModel&gt; | Parallel(params Action&lt;HttpRequestBuilder&lt;TModel&gt;&gt;[] branches) | ParallelBuilder&lt;TModel&gt; | params Action&lt;HttpRequestBuilder&lt;TModel&gt;&gt;[] | no | Builders/PipelineBuilder.Http.cs:45 |
| PipelineBuilder&lt;TModel&gt; | When&lt;TPayload,TProp&gt;(TPayload payload, Expression&lt;Func&lt;TPayload,TProp&gt;&gt; path) | ConditionSourceBuilder&lt;TModel,TProp&gt; | Expression&lt;Func&lt;TPayload,TProp&gt;&gt; | no | Builders/PipelineBuilder.Conditions.cs:11 |
| PipelineBuilder&lt;TModel&gt; | When&lt;TPayload,TProp&gt;(ResponseBody&lt;TPayload&gt; responseBody, Expression&lt;Func&lt;TPayload,TProp&gt;&gt; path) | ConditionSourceBuilder&lt;TModel,TProp&gt; | Expression&lt;Func&lt;TPayload,TProp&gt;&gt; | no | Builders/PipelineBuilder.Conditions.cs:22 |
| PipelineBuilder&lt;TModel&gt; | When&lt;TProp&gt;(TypedSource&lt;TProp&gt; source) | ConditionSourceBuilder&lt;TModel,TProp&gt; | - | no | Builders/PipelineBuilder.Conditions.cs:34 |
| PipelineBuilder&lt;TModel&gt; | Confirm(string message) | GuardBuilder&lt;TModel&gt; | - | no | Builders/PipelineBuilder.Conditions.cs:42 |
| PipelineBuilder&lt;TModel&gt; | From&lt;TElement&gt;(TypedSource&lt;TElement[]&gt; source) | ReactiveArray&lt;TElement&gt; | - | no | Builders/PipelineBuilder.Arrays.cs:15 |
| PipelineBuilder&lt;TModel&gt; | From&lt;TArgs,TElement&gt;(TArgs args, Expression&lt;Func&lt;TArgs,TElement[]&gt;&gt; selector) | ReactiveArray&lt;TElement&gt; | Expression&lt;Func&lt;TArgs,TElement[]&gt;&gt; | no | Builders/PipelineBuilder.Arrays.cs:23 |
| PipelineBuilder&lt;TModel&gt; | FromDom(string elementId, string member) | ReactiveArray&lt;string&gt; | - | no | Builders/PipelineBuilder.Arrays.cs:37 |
| PipelineBuilder&lt;TModel&gt; | FromDom&lt;TElement&gt;(string elementId, string member) | ReactiveArray&lt;TElement&gt; | - | no | Builders/PipelineBuilder.Arrays.cs:41 |

## ReactionPipelineDraft&lt;TModel&gt; (no grammar edges)

`internal sealed class ReactionPipelineDraft<TModel> where TModel : class`
(`Builders/ReactionPipelineDraft.cs:7`). This is the private accumulator behind
PipelineBuilder's `_draft` field. **Zero public members** — all members are `internal`
(`BeginHttp`, `BeginParallel`, `BeginBranch`, `SetConditionalBranches`, `FlushSegment`,
`BuildReaction`, `AddCommand`) or `private`. It is NOT part of the authored DSL grammar and
contributes no AST edges. Listed only so the reader knows the 5th cluster file was inspected
and intentionally excluded.

## Notes on the prompt's illustrative member list

The prompt named `Set/SetText/SetHtml/Call` as candidate members. **None of these exist on
`PipelineBuilder<TModel>`** — they are members of the *returned* sub-builders:
`Element(...)` returns `ElementBuilder<TModel>` (which carries `SetText`/`SetHtml`/`AddClass`/…)
and `Component<...>(...)` returns `ComponentRef<TComponent,TModel>` (which carries `Set`/`Call`/…).
They are out of cluster scope and are correctly absent from this table (GROUNDED, not inferred).

## Edge count

34 grammar edges (34 public methods on `PipelineBuilder<TModel>`).
