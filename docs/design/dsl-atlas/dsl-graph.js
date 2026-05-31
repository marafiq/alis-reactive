window.DSL_GRAPH = {
  "areas": [
    {
      "key": "plan-triggers",
      "title": "Plan & Triggers",
      "blurb": "Plans are the unit of work; triggers are the entry points that each append one Behavior (trigger + reaction).",
      "features": [
        {
          "name": "Create root-view plan",
          "variants": ["Html.ReactivePlan<TModel>() -> new ReactivePlan<TModel>(ReactivePlanScope.RootView, RequestServices)"],
          "example": "var plan = Html.ReactivePlan<OrderModel>();",
          "domainTypes": ["ReactivePlan<TModel>", "ReactivePlanScope", "RootViewPlanScope", "PlanId", "PlanIdentity", "PlanScope.Root (RootPlanScope)", "PlanBuildContext"]
        },
        {
          "name": "Create partial-view plan (merges into parent by PlanId)",
          "variants": ["Html.ResolvePlan<TModel>() -> new ReactivePlan<TModel>(ReactivePlanScope.PartialView, RequestServices)", "IsPartial=true", "RendersValidationSummary=false"],
          "example": "var plan = Html.ResolvePlan<OrderModel>();",
          "domainTypes": ["ReactivePlan<TModel>", "ReactivePlanScope", "PartialViewPlanScope", "PlanIdentity.Partial", "PlanScope.Partial (PartialPlanScope)"]
        },
        {
          "name": "Render plan to plan-JSON script element",
          "variants": ["emits <script type=application/json data-reactive-plan id=alis-plan-{planId}>{json}</script>", "root scope additionally emits a hidden data-reactive-validation-summary div", "partial scope emits only the script"],
          "example": "@Html.RenderPlan(plan)",
          "domainTypes": ["ReactivePlan<TModel>", "PlanDocument", "ReactivePlanSerializer", "PlanScope"]
        },
        {
          "name": "Serialize plan document directly",
          "variants": ["plan.Render() (ambient services)", "plan.Render(IServiceProvider services)", "plan.RenderFormatted() (indented, ambient)", "plan.RenderFormatted(IServiceProvider services)"],
          "example": "string json = plan.Render();",
          "domainTypes": ["ReactivePlan<TModel>", "PlanBuildContext.BuildPlan()", "PlanDocument", "ReactivePlanSerializer (Compact/Formatted, camelCase)"]
        },
        {
          "name": "Register plugin type metadata on the plan",
          "variants": ["plan.RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure)", "plan.RegisterPlugin(ReactivePlugin plugin)", "plan.RegisterPlugin<TPlugin>() where TPlugin : ReactivePlugin, new()"],
          "example": "plan.RegisterPlugin<UrlPlugin>();",
          "domainTypes": ["ReactivePlan<TModel>", "Builders.PluginTypeBuilder", "ReactivePlugin", "PluginContract", "PlanBuildContext.RegisterPlugin"]
        },
        {
          "name": "Attach triggers to a plan",
          "variants": ["Html.On<TModel>(this IHtmlHelper<TModel>, ReactivePlan<TModel> plan, Action<TriggerBuilder<TModel>> trigger) -> opens a TriggerBuilder over plan.Context", "triggers chain, each adds one independent Behavior"],
          "example": "Html.On(plan, t => t.DomReady(p => p.Element(\"status\").SetText(\"Ready\")));",
          "domainTypes": ["TriggerBuilder<TModel>", "PlanBuildContext", "Behavior", "BehaviorGraph"]
        },
        {
          "name": "DomReady trigger (page load)",
          "variants": ["t.DomReady(Action<PipelineBuilder<TModel>> pipeline) -> StartsWhen.PageReady()"],
          "example": "t.DomReady(p => p.Element(\"banner\").AddClass(\"shown\"));",
          "domainTypes": ["StartsWhen", "PageReadyTrigger (kind=page-ready)", "Behavior.On"]
        },
        {
          "name": "CustomEvent trigger (named document event)",
          "variants": ["t.CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline) -> DocumentEventTrigger with PayloadContract.Untyped", "t.CustomEvent<TPayload>(string eventName, Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload:new() -> DocumentEventTrigger with PayloadContract.ForPayload(typeof(TPayload))"],
          "example": "t.CustomEvent<OrderPlaced>(\"order-placed\", (e, p) => p.Element(\"toast\").SetText(\"Saved\"));",
          "domainTypes": ["StartsWhen.DocumentEvent", "DocumentEventTrigger (kind=document-event)", "EventName", "PayloadContract", "UntypedPayloadContract", "NamedPayloadContract"]
        },
        {
          "name": "ServerPush trigger (Server-Sent Events)",
          "variants": ["t.ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline) -> ServerPushTrigger + ServerPushEventFilter.AnyEvent()", "t.ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline) -> NamedEvent filter, untyped payload", "t.ServerPush<TPayload>(string url, string eventType, Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload:new() -> NamedEvent filter with PayloadContract.ForPayload"],
          "example": "t.ServerPush<Ticker>(\"/sse/prices\", \"tick\", (e, p) => p.Element(\"price\").SetText(\"updated\"));",
          "domainTypes": ["StartsWhen.ServerPush", "ServerPushTrigger (kind=server-push)", "ServerPushEventFilter", "AnyServerPushEvent (kind=any)", "NamedServerPushEvent (kind=named)", "RequestUrl", "PayloadContract"]
        },
        {
          "name": "SignalR trigger (hub method)",
          "variants": ["t.SignalR(string hubUrl, string methodName, Action<PipelineBuilder<TModel>> pipeline) -> SignalRTrigger with PayloadContract.Untyped", "t.SignalR<TPayload>(string hubUrl, string methodName, Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload:new() -> SignalRTrigger with PayloadContract.ForPayload"],
          "example": "t.SignalR<Alert>(\"/hubs/alerts\", \"OnAlert\", (e, p) => p.Element(\"alert\").SetText(\"new\"));",
          "domainTypes": ["StartsWhen.SignalR", "SignalRTrigger (kind=signalr)", "RequestUrl", "MemberName", "PayloadContract"]
        },
        {
          "name": "Component event trigger (.Reactive on a vendor component)",
          "variants": ["builder.Reactive<TModel,TArgs>(plan, Func<TEvents, TypedEvent<TArgs>> eventSelector, Action<TArgs, PipelineBuilder<TModel>> pipeline) -> ComponentEventOnboarding.Wire -> PlanBuildContext.WireComponentEvent -> StartsWhen.ComponentEvent(componentId, eventName)", "one overload per component slice (Fusion ComboBox, AutoComplete, Slider, Sidebar, Tooltip, etc.)", "BehaviorGraph also declares an ObjectEventContract on the component object for the wired event"],
          "example": "Html.FusionComboBox(plan, m => m.City).FusionComboBox(b => b.Reactive(plan, e => e.Change, (e, p) => p.Element(\"out\").SetText(\"changed\")));",
          "domainTypes": ["TypedEvent<TArgs>", "ComponentEventOnboarding", "PlanBuildContext.WireComponentEvent", "StartsWhen.ComponentEvent", "ComponentEventTrigger (kind=component-event)", "ComponentKey", "EventName", "ObjectEventContract", "ComponentObjectTarget", "Behavior"]
        },
        {
          "name": "Behavior = trigger + reaction (plan graph node)",
          "variants": ["Behavior.On(StartsWhen trigger, ReactionGraph reaction) added to BehaviorGraph", "each Html.On trigger and each .Reactive wiring produces exactly one Behavior", "declaration order preserved in PlanDocument.Behaviors"],
          "example": "// implicit: every t.DomReady/CustomEvent/.Reactive(...) call appends one Behavior",
          "domainTypes": ["Behavior", "BehaviorGraph", "StartsWhen", "ReactionGraph", "PlanDocument (PlanId, Scope, Components, Behaviors)"]
        }
      ]
    },
    {
      "key": "reactions",
      "title": "Reactions",
      "blurb": "The command sequence a trigger fires; sync commands buffer, async boundaries flush, branches split the segment.",
      "features": [
        {
          "name": "Set property (element)",
          "variants": ["SetText(string)", "SetText<TSource>(TSource source, Expression<Func<TSource,object>> path) (event payload)", "SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse,object>> path) (HTTP body)", "SetText<TProp>(TypedSource<TProp> source) (component/plugin/URL)", "SetHtml(string)", "SetHtml<TSource>(TSource source, Expression path)", "SetHtml<TProp>(TypedSource<TProp> source)", "Show() (hidden=false)", "Hide() (hidden=true)"],
          "example": "p.Element(\"banner\").SetText(\"Saved\");",
          "domainTypes": ["SetReaction", "ComponentSource", "ValueExpression", "BrowserElementMembers", "ComponentProperty<T>", "MemberName", "ComponentKey"]
        },
        {
          "name": "Set property (component)",
          "variants": ["ComponentRef.EmitSet<TValue>(ComponentProperty<TValue>, ValueExpression) emitted by vendor extensions (e.g. SetValue)", "target via p.Component<T>(m => m.Prop), p.Component<T,TOther>(expr), p.Component<T>(\"refId\"), or p.Component<T>() (app-level)", "value sources: literal, TypedSource, event payload, response body via ValueExpression"],
          "example": "p.Component<FusionTextBox>(m => m.Name).SetValue(\"Ada\");",
          "domainTypes": ["SetReaction", "ComponentSource", "ComponentObjectTarget", "ComponentProperty<T>", "ValueExpression", "ComponentKey", "MemberAccess"]
        },
        {
          "name": "Call method (element)",
          "variants": ["AddClass(string) -> addClass", "RemoveClass(string) -> removeClass", "ToggleClass(string) -> toggleClass"],
          "example": "p.Element(\"row\").AddClass(\"active\");",
          "domainTypes": ["CallReaction", "ComponentSource", "ValueExpression", "BrowserElementMembers", "ComponentMethod", "MemberName"]
        },
        {
          "name": "Call method (component)",
          "variants": ["ComponentRef.EmitCall(ComponentMethod) (no-arg, e.g. Focus, Show)", "ComponentRef.EmitCall(ComponentMethod, List<ValueExpression> args) (typed args)", "same Component<T> target overloads as Set (expression / cross-model / explicit-id / app-level)"],
          "example": "p.Component<FusionDialog>().Open();",
          "domainTypes": ["CallReaction", "ComponentSource", "ComponentObjectTarget", "ComponentMethod", "ValueExpression", "ComponentKey", "Shape"]
        },
        {
          "name": "Call method (plugin)",
          "variants": ["p.Plugin(name, member) (void member command)", "p.Plugin(name) (root function)", "p.Plugin(PluginCommand) (declared descriptor)", "PluginCallBuilder.Arg overloads: ResponseBody+path, event args+path, TypedSource, string, int, bool, long, decimal, double, DateTime, ArgValue<TValue>", "PluginCallBuilder.Fire() (terminal: emits CallReaction on PluginSource)"],
          "example": "p.Plugin(\"clipboard\", \"copy\").Arg(\"hello\").Fire();",
          "domainTypes": ["CallReaction", "PluginSource", "PluginOperationId", "PluginArguments", "PluginInvocationArgument", "MethodArgumentContract", "ValueExpression"]
        },
        {
          "name": "Dispatch custom event",
          "variants": ["p.Dispatch(string eventName) (no payload, DispatchPayload.None)", "p.Dispatch<TPayload>(string eventName, TPayload payload) (literal payload, LiteralRaw + PayloadContract)", "p.DispatchWith<TPayload>(string eventName, Action<DispatchPayloadBuilder<TPayload,TModel>> configure) (runtime source-backed payload)"],
          "example": "p.Dispatch(\"order-saved\");",
          "domainTypes": ["DispatchReaction", "DispatchPayload", "NoDispatchPayload", "PresentDispatchPayload", "PayloadContract", "ValueExpression", "EventName"]
        },
        {
          "name": "Dispatch payload composition",
          "variants": ["DispatchPayloadBuilder.Set<TProp>(field, TypedSource<TProp>) (live source)", "Set(field, string) (literal string)", "Set(field, int) (literal int)", "Set(field, bool) (literal bool)", "supports nested object paths x => x.A.B via DispatchPayloadPath", "conflicting leaf/parent throws"],
          "example": "p.DispatchWith<Saved>(\"saved\", b => b.Set(x => x.Id, idSource).Set(x => x.Status, \"ok\"));",
          "domainTypes": ["DispatchPayloadDraft", "DispatchPayloadPath", "ValueExpression", "PayloadContract"]
        },
        {
          "name": "Conditional branch",
          "variants": ["p.When<TPayload,TProp>(payload, path)", "p.When<TPayload,TProp>(ResponseBody<TPayload>, path)", "p.When<TProp>(TypedSource<TProp> source)", "p.Confirm(message) (user-confirmation guard, no source)", "operator -> GuardBuilder.Then(pipeline) emits first BranchCase", "BranchBuilder.ElseIf(...) (event/response/typed-source)", "BranchBuilder.Else(pipeline) (default, BranchGuard.Else)", "And/Or/Not compose", "ConditionStart.When for nested standalone"],
          "example": "p.When(level).Eq(\"Memory\").Then(t => t.Element(\"fee\").SetText(\"$2400\")).Else(t => t.Element(\"fee\").SetText(\"$0\"));",
          "domainTypes": ["BranchReaction", "BranchCase", "BranchGuard", "ConditionGraph", "ReactionGraph"]
        },
        {
          "name": "Reaction sequencing (sync/async lanes)",
          "variants": ["every command appends in authored order via PipelineBuilder.AddStep -> ReactionPipelineDraft.AddCommand", "sync segment commands buffer into a SequenceReaction", "async boundaries (HTTP/parallel) flush the pending sync segment then emit Request/Parallel block", "branch flush splits sync reactions before/after the BranchReaction", "single block collapses, multiple wrap in SequenceReaction"],
          "example": "p.Element(\"a\").AddClass(\"x\"); p.Dispatch(\"e\"); // two ordered sync reactions",
          "domainTypes": ["SequenceReaction", "ReactionGraph", "ReactionPipelineDraft<TModel>", "PendingBranch", "PendingAsyncReaction<T>"]
        },
        {
          "name": "Parallel reaction (concurrent branches)",
          "variants": ["ParallelReaction holds concurrent steps plus ParallelCompletion", "ParallelCompletion.None (NoParallelCompletion)", "ParallelCompletion.OnSettled(reaction) (SettledParallelCompletion)"],
          "example": "p.Parallel(b => { ... }).OnAllSettled(t => t.Element(\"done\").Show());",
          "domainTypes": ["ParallelReaction", "ParallelCompletion", "NoParallelCompletion", "SettledParallelCompletion", "ReactionGraph"]
        },
        {
          "name": "Inject HTML into element",
          "variants": ["p.Into(elementId) injects HTTP success response body as HTML into the target element (follows a request)"],
          "example": "p.Get(\"/fragment\").Into(\"panel\");",
          "domainTypes": ["InjectReaction", "ValueExpression", "PayloadSource", "ComponentKey"]
        },
        {
          "name": "Show validation errors reaction",
          "variants": ["p.ValidationErrors(formId) emits a reaction that renders accumulated validation errors in the container element"],
          "example": "p.ValidationErrors(\"summary\");",
          "domainTypes": ["ShowValidationErrorsReaction", "ComponentId"]
        }
      ]
    },
    {
      "key": "conditions",
      "title": "Conditions",
      "blurb": "Deterministic guard graphs over shared value sources; first matching BranchCase wins, And/Or/Not compose.",
      "features": [
        {
          "name": "When (start conditional branch)",
          "variants": ["p.When<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path) (event payload)", "p.When<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path) where TPayload:class (HTTP body)", "p.When<TProp>(TypedSource<TProp> source) (any typed source)"],
          "example": "p.When(args, e => e.Value).Gte(5).Then(t => t.Set(m => m.Flag, true))",
          "domainTypes": ["ConditionSourceBuilder<TModel,TProp>", "PayloadTypedSource<TPayload,TProp>", "ResponseBody<TPayload>", "TypedSource<TProp>", "BranchReaction"]
        },
        {
          "name": "Comparison operators (literal operand)",
          "variants": ["Eq -> eq", "NotEq -> neq", "Gt -> gt", "Gte -> gte", "Lt -> lt", "Lte -> lte"],
          "example": "p.When(args, e => e.Age).Gte(65).Then(t => t.Set(m => m.IsSenior, true))",
          "domainTypes": ["CompareCondition", "CompareOperator", "ComparisonOperands (Binary)", "ValueExpression.LiteralRaw", "Shape"]
        },
        {
          "name": "Source-vs-source comparison operators",
          "variants": ["Eq(TypedSource<TProp> right) -> eq", "NotEq -> neq", "Gt -> gt", "Gte -> gte", "Lt -> lt", "Lte -> lte"],
          "example": "p.When(p.FromUrl<int>(\"min\")).Lte(startSource).Then(t => t.Dispatch(\"valid\"))",
          "domainTypes": ["CompareCondition", "ComparisonOperands (Binary)", "TypedSource<TProp>.ToValueExpression", "PresentComparisonRightOperand"]
        },
        {
          "name": "Presence / unary operators",
          "variants": ["Truthy() -> truthy", "Falsy() -> falsy", "IsNull() -> is-null", "NotNull() -> not-null", "IsEmpty() -> is-empty", "NotEmpty() -> not-empty"],
          "example": "p.When(args, e => e.SelectedId).NotNull().Then(t => t.Component(...))",
          "domainTypes": ["CompareCondition", "ComparisonOperands.Unary", "AbsentComparisonRightOperand"]
        },
        {
          "name": "Membership / range operators",
          "variants": ["In(params TProp[] values) -> in (ValueExpression.Array)", "NotIn(params TProp[] values) -> not-in", "Between(TProp low, TProp high) -> between (two-element array endpoints)"],
          "example": "p.When(args, e => e.Level).In(\"Memory\", \"Skilled\").Then(t => t.Set(m => m.HighCare, true))",
          "domainTypes": ["CompareCondition", "ComparisonOperands (Binary)", "ValueExpression.Array", "Shape.ArrayOf"]
        },
        {
          "name": "Text operators",
          "variants": ["Contains(string substring) -> contains", "StartsWith(string prefix) -> starts-with", "EndsWith(string suffix) -> ends-with", "Matches(string pattern) -> matches (regex)", "MinLength(int length) -> min-length (MinimumTextLength, Shape.Number)"],
          "example": "p.When(args, e => e.Name).StartsWith(\"Dr.\").Then(t => t.Set(m => m.IsDoctor, true))",
          "domainTypes": ["CompareCondition", "ComparisonOperands (Binary)", "ValueExpression.LiteralRaw", "MinimumTextLength", "Shape.String / Shape.Number"]
        },
        {
          "name": "Array membership operator",
          "variants": ["ArrayContains(object item) -> array-contains (carries element ItemShape via ComparisonOperands.CollectionItem)"],
          "example": "p.When(args, e => e.Tags).ArrayContains(\"urgent\").Then(t => t.Dispatch(\"alert\"))",
          "domainTypes": ["CompareCondition", "ComparisonOperands.CollectionItem", "TypedSource.ElementShape", "CompareCondition.ItemShape"]
        },
        {
          "name": "Then / first-match branch execution",
          "variants": ["GuardBuilder.Then(Action<PipelineBuilder<TModel>> pipeline) -> first BranchCase.Of, returns BranchBuilder", "standalone Then throws InvalidOperationException (requires pipeline context)"],
          "example": "p.When(args, e => e.Score).Gt(80).Then(t => t.Set(m => m.Pass, true))",
          "domainTypes": ["BranchBuilder<TModel>", "BranchCase.Of", "BranchGuard.When (ConditionalBranchGuard, \"when\")", "BranchReaction", "PipelineConditionContinuation / BranchConditionContinuation"]
        },
        {
          "name": "ElseIf chained branches",
          "variants": ["ElseIf<TPayload,TProp>(payload, path) (event)", "ElseIf<TPayload,TProp>(responseBody, path) (response)", "ElseIf<TProp>(TypedSource<TProp>) (typed source)", "throws InvalidOperationException if added after Else (EnsureElseIfCanBeAdded)"],
          "example": "p.When(args,e=>e.Lvl).Eq(1).Then(t=>...).ElseIf(args,e=>e.Lvl).Eq(2).Then(t=>...)",
          "domainTypes": ["BranchBuilder<TModel>", "ConditionSourceBuilder<TModel,TProp>", "BranchCase.Of", "ordered BranchReaction.Cases"]
        },
        {
          "name": "Else default case",
          "variants": ["BranchBuilder.Else(Action<PipelineBuilder<TModel>> pipeline) -> BranchCase.Default (BranchGuard.Else, \"default\")", "throws if Else already called or branches added after"],
          "example": "p.When(args,e=>e.Ok).Truthy().Then(t=>...).Else(t => t.Dispatch(\"failed\"))",
          "domainTypes": ["BranchBuilder<TModel>", "BranchCase.Default", "DefaultBranchGuard (\"default\")"]
        },
        {
          "name": "Guard composition (And / Or / Not)",
          "variants": ["And<TPayload,TProp>(payload, path)", "And<TPayload,TProp>(responseBody, path)", "And<TProp>(TypedSource<TProp>)", "And(Func<ConditionStart<TModel>,GuardBuilder<TModel>> inner) (nested, flattened)", "Or<TPayload,TProp>(payload, path)", "Or<TPayload,TProp>(responseBody, path)", "Or<TProp>(TypedSource<TProp>)", "Or(Func<...> inner)", "Not()"],
          "example": "p.When(args,e=>e.A).Gt(0).And(args,e=>e.B).NotNull().Or(c => c.When(other).Eq(1)).Then(t=>...)",
          "domainTypes": ["GuardBuilder<TModel>", "AllCondition (\"all\")", "AnyCondition (\"any\")", "NotCondition (\"not\")", "ConditionComposition (FlattenAll/FlattenAny)", "ConditionStart<TModel>"]
        },
        {
          "name": "Confirm guard (async user decision)",
          "variants": ["p.Confirm(string message) (from PipelineBuilder, begins branch then returns GuardBuilder)", "ConditionStart.Confirm(string message) (standalone for And/Or)"],
          "example": "p.Confirm(\"Discharge this resident?\").Then(t => t.Post<DischargeModel>(\"/discharge\"))",
          "domainTypes": ["ConfirmCondition (\"confirm\")", "GuardBuilder<TModel>", "ConditionGraph.Confirm"]
        },
        {
          "name": "Typed condition value sources",
          "variants": ["PayloadTypedSource<TPayload,TProp> (event/success/error/dispatch payload via PayloadSource + ExpressionPathHelper.ToEventPath)", "ResponseBody<TPayload>.Read(path) (HTTP body)", "TypedComponentSource<TProp> (ValueExpression.Read over ComponentSource, or FromMethod -> Invoke)", "TypedUrlSource<TProp> (ValueExpression.ReadUrl, registers RequestScalarTarget.UrlQueryParameter)", "TypedPluginSource<TProp> (Invoke over PluginSource)", "TypedPluginPropertySource<TProp> (Read over PluginSource)"],
          "example": "p.When(careLevelDdl.ReactiveValue()).Eq(\"Memory\").Then(t => t.Element(\"warn\").Show())",
          "domainTypes": ["TypedSource<TProp>", "ValueExpression (Read/Invoke/ReadPayload/ReadUrl/LiteralRaw/Array)", "PayloadSource", "ComponentSource", "PluginSource", "Shape.FromClrType"]
        }
      ]
    },
    {
      "key": "http",
      "title": "HTTP Pipeline",
      "blurb": "The primary async lane: verbs open a RequestPlan, Gather reads targets, Response routes success/error, Chained and Parallel compose.",
      "features": [
        {
          "name": "HTTP request verb (Get/Post/Put/Delete)",
          "variants": ["p.Get(string url)", "p.Post(string url)", "p.Post(string url, Action<GatherBuilder<TModel>> gather)", "p.Put(string url, Action<GatherBuilder<TModel>> gather)", "p.Delete(string url)", "builder.Get/Post/Put/Delete(string url) (fluent re-selection on HttpRequestBuilder)", "url may carry {placeholder} route-template params validated at build time"],
          "example": "p.Get(\"/api/residents/{id}\")",
          "domainTypes": ["RequestPlan", "RequestEndpoint", "HttpMethodName", "RequestUrl", "RequestReaction", "RequestRouteTemplate"]
        },
        {
          "name": "Request body format",
          "variants": [".AsJson() (default)", ".AsFormData()", "GET emits query string, POST/PUT/DELETE emit declared body format"],
          "example": "p.Post(\"/api/intake\").Gather(g => g.IncludeAll()).AsFormData()",
          "domainTypes": ["RequestBodyFormat", "GatherRequestInput"]
        },
        {
          "name": "Gather - payload from component value",
          "variants": [".Include<TComponent,TModel>(Expression<Func<TModel,object>> expr) (model expression -> param name)", ".Include<TComponent,TModel>(string refId, string name) (explicit id+name)", ".Include<TModel,TProp>(TypedComponentSource<TProp> source) (typed member, default name)", ".Include<TModel,TProp>(TypedComponentSource<TProp> source, string paramName) (typed member + explicit param)", ".IncludeAll() (all registered input components)"],
          "example": "p.Post(\"/api/save\").Gather(g => g.Include<FusionTextBox, OrderModel>(m => m.Name))",
          "domainTypes": ["GatherBuilder", "RequestInputAssignment", "RequestPayloadTarget", "ValueExpression", "ComponentSource", "RegisteredInputSelection", "InputComponentPlanBinding", "InputValueContract", "BindingPath"]
        },
        {
          "name": "Gather - payload from static / event / plugin / URL",
          "variants": [".Static(string param, object value)", ".FromEvent<TArgs,TProp>(TArgs args, Expression path, string param)", ".FromUrl(string paramName)", ".FromUrl(string paramName, string asParam)", ".FromUrl<T>(string paramName) (typed)", ".FromUrl<T>(string paramName, string asParam) (typed + name)", ".Plugin<T>(TypedPluginSource<T> source, string paramName)"],
          "example": "p.Get(\"/api/search\").Gather(g => g.FromUrl<int>(\"page\").Static(\"pageSize\", 20))",
          "domainTypes": ["RequestInputAssignment", "RequestPayloadTarget", "ValueExpression", "PayloadSource", "UrlSource", "TypedPluginSource", "RequestScalarTarget", "UrlParameterName", "Shape"]
        },
        {
          "name": "Gather - header target (scalar)",
          "variants": [".Header(string name, string value) (literal, null rejected)", ".Header<TProp>(string name, TypedSource<TProp> source) (scalar-only)", ".Header<TArgs,TProp>(string name, TArgs args, Expression path) (event-arg, scalar-only)"],
          "example": "p.Get(\"/api/me\").Gather(g => g.Header(\"X-Tenant\", token))",
          "domainTypes": ["RequestHeaderTarget", "HeaderName", "ValueExpression", "RequestScalarTarget", "Shape"]
        },
        {
          "name": "Gather - route param target (scalar)",
          "variants": [".RouteParam(string name, int value)", ".RouteParam(string name, string value) (null rejected)", ".RouteParam(string name, long value)", ".RouteParam<TProp>(string name, TypedSource<TProp> source) (scalar-only)", ".RouteParam<TArgs,TProp>(string name, TArgs args, Expression path) (scalar-only)", "every {placeholder} must have a matching RouteParam and vice versa"],
          "example": "p.Get(\"/api/residents/{id}\").Gather(g => g.RouteParam(\"id\", 42))",
          "domainTypes": ["RequestRouteParameterTarget", "RouteParameterName", "ValueExpression", "RequestScalarTarget", "RequestRouteTemplate", "Shape"]
        },
        {
          "name": "Response success route",
          "variants": [".OnSuccess(Action<PipelineBuilder<TModel>>) (untyped)", ".OnSuccess<TResponse>(Action<ResponseBody<TResponse>, PipelineBuilder<TModel>>) (typed body)", "ResponseBody<T>.Read<TProp>(expr) (typed source from success body)"],
          "example": ".Response(r => r.OnSuccess<ApiResponse>((json, s) => s.Element(\"name\").SetText(json, r => r.Data.Name)))",
          "domainTypes": ["ResponseBuilder", "ResponseRoute", "ResponseStatusMatch", "AnyResponseStatusMatch", "ReactionGraph", "PayloadSource", "PayloadContract", "ResponseBody"]
        },
        {
          "name": "Response error route",
          "variants": [".OnError(Action<PipelineBuilder<TModel>>) (any, untyped)", ".OnError(int statusCode, Action<PipelineBuilder<TModel>>) (exact, untyped)", ".OnError<TError>(Action<ResponseBody<TError>, PipelineBuilder<TModel>>) (any, typed)", ".OnError<TError>(int statusCode, Action<ResponseBody<TError>, PipelineBuilder<TModel>>) (exact, typed)", "status validated 100-599, network/client failures use the no-status overload"],
          "example": ".Response(r => r.OnError(404, e => e.Element(\"msg\").SetText(\"Not found\")))",
          "domainTypes": ["ResponseRoute", "ResponseStatusMatch", "ExactResponseStatusMatch", "AnyResponseStatusMatch", "HttpResponseStatusCode", "PayloadSource", "PayloadContract", "ResponseBody"]
        },
        {
          "name": "Chained request",
          "variants": [".Chained(Action<HttpRequestBuilder<TModel>>) (one follow-up after success)", "only one chained request per response (a second throws)", "chained request is a full HttpRequestBuilder - may gather from the previous success scope and nest its own Chained"],
          "example": ".Response(r => r.OnSuccess(...).Chained(c => c.Get(\"/api/next/{id}\").Gather(g => g.Include<...>(...))))",
          "domainTypes": ["RequestChain", "TerminalRequestChain", "FollowUpRequestChain", "RequestPlan", "ResponseRouting"]
        },
        {
          "name": "Parallel requests",
          "variants": ["p.Parallel(params Action<HttpRequestBuilder<TModel>>[] branches) (N concurrent branches)", ".OnAllSettled(Action<PipelineBuilder<TModel>>) (after every branch settles)", "requires at least one branch"],
          "example": "p.Parallel(b => b.Get(\"/api/a\"), b => b.Get(\"/api/b\")).OnAllSettled(s => s.Dispatch(\"loaded\"))",
          "domainTypes": ["ParallelBuilder", "ParallelReaction", "ParallelCompletion", "NoParallelCompletion", "SettledParallelCompletion", "RequestReaction", "RequestPlan", "ReactionGraph"]
        },
        {
          "name": "WhileLoading reaction",
          "variants": [".WhileLoading(Action<PipelineBuilder<TModel>>) (runs before the request is sent, e.g. show spinner)"],
          "example": ".WhileLoading(s => s.Component(loader).Show())",
          "domainTypes": ["RequestReactions", "ReactionGraph", "RequestPlan"]
        },
        {
          "name": "Finally reaction",
          "variants": [".Finally(Action<PipelineBuilder<TModel>>) (runs after the request settles regardless of outcome; no response-body access)"],
          "example": ".Finally(s => s.Component(loader).Hide())",
          "domainTypes": ["RequestReactions", "ReactionGraph", "RequestPlan"]
        },
        {
          "name": "Validate before request",
          "variants": [".Validate<TValidationSource>(string formId) (runs client validation against the registered rule source, displaying errors in the form container, before sending)", "default when not called is RequestValidationTarget.None"],
          "example": "p.Post(\"/api/save\").Validate<OrderValidator>(\"order-form\").Gather(g => g.IncludeAll())",
          "domainTypes": ["RequestValidationTarget", "NoRequestValidationTarget", "ContainerRequestValidationTarget", "ClientValidationBeforeRequest", "ComponentId"]
        },
        {
          "name": "Bodiless request",
          "variants": ["no Gather call leaves RequestInput.None (NoRequestInput) - bodiless GET/DELETE"],
          "example": "p.Delete(\"/api/residents/{id}\").Gather(g => g.RouteParam(\"id\", id))",
          "domainTypes": ["RequestInput", "NoRequestInput", "GatherRequestInput"]
        }
      ]
    },
    {
      "key": "arrays",
      "title": "Arrays",
      "blurb": "Array transforms compile to ArrayOperationExpression; Where/Select/OrderBy shape, Count/Any/All/Sum/Find reduce, AsSource binds.",
      "features": [
        {
          "name": "p.From - begin an array transform from a typed array source",
          "variants": ["From<TElement>(TypedSource<TElement[]> source) (component member, plugin read, response-body Read, URL, literal, or prior AsSource())", "From<TArgs,TElement>(TArgs args, Expression<Func<TArgs,TElement[]>> selector) (captures a .Reactive() event-payload array; lambda captured into a plan read, never invoked)", "element type TElement flows through the chain"],
          "example": "var residents = p.From(json.Read(x => x.Residents));",
          "domainTypes": ["ReactiveArray<TElement>", "TypedSource<TElement[]>", "PayloadTypedSource<TArgs,TElement[]>", "ValueExpression", "Shape"]
        },
        {
          "name": "p.FromDom - begin an array transform from a DOM element's array-like member",
          "variants": ["FromDom(string elementId, string member) => ReactiveArray<string> (string-element default for DOMTokenList/classList)", "FromDom<TElement>(string elementId, string member) => ReactiveArray<TElement> (declared element type)", "element resolved by getElementById, member via ValueExpression.ReadDom", "array-like collections normalized at the input boundary"],
          "example": "var classes = p.FromDom(\"resident-card\", \"classList\");",
          "domainTypes": ["ReactiveArray<string>", "ReactiveArray<TElement>", "ReadExpression", "DomSource", "Shape"]
        },
        {
          "name": "Where - filter elements by a per-element sync predicate",
          "variants": ["Where(Expression<Func<TElement,bool>> predicate) => ReactiveArray<TElement> (chainable)", "predicate compiled to a sync ConditionGraph (compare/all/any/not) reading element scope", "supports == != > >= < <=, && || !, string Contains/StartsWith/EndsWith (literal arg), and boolean members"],
          "example": "residents.Where(x => x.Status == \"active\")",
          "domainTypes": ["ReactiveArray<TElement>", "ArrayOperationExpression (op=\"filter\")", "ConditionGraph", "ValueExpression.ArrayFilter", "ElementExpressionCompiler"]
        },
        {
          "name": "Select - project each element through a per-element selector",
          "variants": ["Select<TResult>(Expression<Func<TElement,TResult>> selector) => ReactiveArray<TResult> (result element shape = Shape.FromClrType(typeof(TResult)))", "compiled to a ValueExpression read against element scope", "v1 supports member access, the element itself (x => x), and whitelisted pure element method calls", "object-init/arithmetic throw at render"],
          "example": "residents.Select(x => x.Name)",
          "domainTypes": ["ReactiveArray<TResult>", "ArrayOperationExpression (op=\"map\")", "ValueExpression.ArrayMap", "ElementExpressionCompiler"]
        },
        {
          "name": "OrderBy / OrderByDescending - order elements by a per-element key",
          "variants": ["OrderBy<TKey>(Expression<Func<TElement,TKey>> key) => ReactiveArray<TElement> (op=\"orderBy\")", "OrderByDescending<TKey>(...) (op=\"orderByDescending\")", "key must project to a sortable scalar (string/number/boolean/date/nullable)", "a non-scalar key throws at plan render time"],
          "example": "roster.OrderBy(x => x.Name)",
          "domainTypes": ["ReactiveArray<TElement>", "ArrayOperationExpression (op=\"orderBy\"/\"orderByDescending\")", "ValueExpression.ArrayOrderBy", "Shape", "ElementExpressionCompiler"]
        },
        {
          "name": "Count - count all elements or count matching elements",
          "variants": ["Count() => ReactiveValue<int> (op=\"count\", no predicate)", "Count(Expression<Func<TElement,bool>> predicate) => ReactiveValue<int> (sugar for Where(predicate).Count())"],
          "example": "residents.Count(x => x.Status == \"active\")",
          "domainTypes": ["ReactiveValue<int>", "ArrayOperationExpression (op=\"count\"/\"filter\")", "ValueExpression.ArrayCount", "ConditionGraph"]
        },
        {
          "name": "Any - true when array non-empty or when any element matches",
          "variants": ["Any() => ReactiveValue<bool> (op=\"any\", predicate null = non-empty)", "Any(Expression<Func<TElement,bool>> predicate) => ReactiveValue<bool> (op=\"any\" with compiled predicate)"],
          "example": "residents.Any(x => x.Status == \"critical\")",
          "domainTypes": ["ReactiveValue<bool>", "ArrayOperationExpression (op=\"any\")", "ValueExpression.ArrayAny", "ConditionGraph"]
        },
        {
          "name": "All - true when every element matches the predicate",
          "variants": ["All(Expression<Func<TElement,bool>> predicate) => ReactiveValue<bool> (op=\"all\", predicate required)"],
          "example": "residents.All(x => x.Age >= 18)",
          "domainTypes": ["ReactiveValue<bool>", "ArrayOperationExpression (op=\"all\")", "ValueExpression.ArrayAll", "ConditionGraph"]
        },
        {
          "name": "Sum - sum a numeric per-element selector (typed by selector return)",
          "variants": ["Sum(Expression<Func<TElement,int>> selector) => ReactiveValue<int>", "Sum(... decimal ...) => ReactiveValue<decimal>", "Sum(... double ...) => ReactiveValue<double>", "all are op=\"sum\" carrying a projection ValueExpression", "output Shape is always Shape.Number"],
          "example": "residents.Where(x => x.Status == \"active\").Sum(x => x.Age)",
          "domainTypes": ["ReactiveValue<int>", "ReactiveValue<decimal>", "ReactiveValue<double>", "ArrayOperationExpression (op=\"sum\")", "ValueExpression.ArraySum"]
        },
        {
          "name": "Find - first matching element, optionally projected to a field",
          "variants": ["Find(Expression<Func<TElement,bool>> predicate) => ReactiveValue<TElement> (projection null, result shape = element shape; null when none match)", "Find<TField>(Expression<Func<TElement,bool>> predicate, Expression<Func<TElement,TField>> selector) => ReactiveValue<TField> (result shape = Shape.FromClrType(typeof(TField)))"],
          "example": "residents.OrderByDescending(x => x.Age).Find(x => true, x => x.Name)",
          "domainTypes": ["ReactiveValue<TElement>", "ReactiveValue<TField>", "ArrayOperationExpression (op=\"find\")", "ValueExpression.ArrayFind", "ConditionGraph"]
        },
        {
          "name": "AsSource - expose the transformed array as a typed array source",
          "variants": ["AsSource() => TypedSource<TElement[]> (backed by ReactiveArraySource<TElement> wrapping the array-op ValueExpression)", "lets a shaped array bind directly to a component via SetDataSource(TypedSource<T[]>) with no HTTP round-trip"],
          "example": "roster.OrderBy(x => x.Name).AsSource()",
          "domainTypes": ["TypedSource<TElement[]>", "ReactiveArraySource<TElement>", "ArrayOperationExpression", "ValueExpression"]
        },
        {
          "name": "ReactiveValue<T> - scalar result of a reduction, usable anywhere TypedSource<T> is",
          "variants": ["ReactiveValue<TValue> : TypedSource<TValue> produced by Count/Sum/Any/All/Find", "plugs into SetText, When, and dispatch payloads with no new overloads (base-source consumers)", "gather intake is typed to component/plugin sources, not the base source"],
          "example": "s.Element(\"res-total\").SetText(residents.Count());",
          "domainTypes": ["ReactiveValue<TValue>", "TypedSource<TValue>", "ValueExpression"]
        },
        {
          "name": "Element-scope expression compilation (per-element predicates & selectors)",
          "variants": ["predicate -> sync ConditionGraph (compare/all/any/not), comparison shape derived from the typed member operand", "element member read -> ReadPayload(PayloadSource.Element(), path, shape)", "identity (x => x) -> ReadWholeElement(shape)", "whitelisted pure element method call (getDay/getMonth/getFullYear/getDate/getHours/getMinutes/getSeconds/getTime/toUpperCase/toLowerCase/trim/getAttribute/hasAttribute) -> InvokeElement", "non-whitelisted/side-effecting methods throw at render", "parameter-free subexpression -> LiteralFromValue"],
          "example": "residents.Where(x => x.StartDate.GetMonth() == 3 && x.Name.StartsWith(\"A\"))",
          "domainTypes": ["ElementExpressionCompiler", "ConditionGraph", "ComparisonOperands", "CompareOperator", "PayloadSource (Element)", "ValueExpression.ReadWholeElement", "ValueExpression.InvokeElement", "ValueExpression.ReadPayload"]
        }
      ]
    },
    {
      "key": "values",
      "title": "Values",
      "blurb": "ValueExpression is the single value path every area reads through; Shape is the type contract; TypedSource<TProp> preserves type safety.",
      "features": [
        {
          "name": "ValueExpression (the single value path)",
          "variants": ["abstract base with internal OutputShape", "polymorphic over kind literal | read | object | array | array-op", "consumed by SetReaction, CallReaction, ConditionGraph operands, gather request input, dispatch payload, validation condition, plugin args, route params, headers", "internal static factories only - never constructed in app code"],
          "example": "// every source below resolves to a ValueExpression node serialized into the plan",
          "domainTypes": ["Alis.Reactive.PlanModel.ValueExpression"]
        },
        {
          "name": "LiteralExpression (constant value)",
          "variants": ["Literal(bool) -> Boolean", "Literal(string) -> String", "Literal(int/long/decimal/double) -> Number", "Literal(DateTime) -> Date (ISO 'O')", "Null() -> LiteralExpression(null, Shape.None)", "LiteralRaw(object?, Shape) (caller-declared shape, condition operands)", "LiteralFromValue(object?) (shape inferred via Shape.FromValue, null becomes Null())", "kind = literal, carries Value + Shape"],
          "example": "p.Element(\"greeting\").SetText(\"Hello\");",
          "domainTypes": ["Alis.Reactive.PlanModel.LiteralExpression", "Alis.Reactive.PlanModel.Shape"]
        },
        {
          "name": "ReadExpression (live value read from a Source)",
          "variants": ["Read(Source, member)", "Read(Source, member, Path)", "Read(Source, member, Shape)", "Read(Source, member, Path, Shape)", "ReadUrl(paramName)", "ReadUrl(paramName, Shape)", "ReadDom(elementId, member, Shape)", "ReadPayload(PayloadSource, path) / (.., Shape)", "ReadWholePayload(PayloadSource) / (.., Shape)", "ReadWholeElement() / (Shape)", "Invoke(RuntimeObjectSource, method, returns, args)", "InvokeElement(receiverPath, method, returns, args)", "kind = read, carries From/Member/Path/Shape/Access"],
          "example": "p.When(p.FromUrl<int>(\"page\")).Gt(1).Then(s => s.Element(\"prev\").Show());",
          "domainTypes": ["Alis.Reactive.PlanModel.ReadExpression", "ValueRead", "ValueReadTarget", "ValueReadAccess", "PropertyValueReadAccess", "MethodValueReadAccess"]
        },
        {
          "name": "Source hierarchy (what a read points at)",
          "variants": ["abstract Source (write-only polymorphic, kind discriminator)", "RuntimeObjectSource (base for browser objects with declared members)", "ComponentSource (\"component\") via ComponentKey", "PluginSource (\"plugin\") carries PluginName + TypeKey.Plugin", "UrlSource (\"url\") singleton", "DomSource (\"dom\") by id", "PayloadSource (\"payload\") scope + contract"],
          "example": "p.Component<FusionTextBox>(m => m.Name).Read<string>(...) // ComponentSource",
          "domainTypes": ["Source", "RuntimeObjectSource", "ComponentSource", "PluginSource", "UrlSource", "DomSource", "PayloadSource"]
        },
        {
          "name": "PayloadSource scopes (event / response / request / dispatch / local / element)",
          "variants": ["Event() / Event(PayloadContract) / Event(string)", "Success() / Success(contract) / Success(string)", "Error() / Error(contract) / Error(string)", "Request() / Request(contract) / Request(string)", "Dispatch() / Dispatch(contract) / Dispatch(string)", "Local() (view-model, untyped)", "Element() (current array element)", "each carries PayloadScope + PayloadContract"],
          "example": ".OnSuccess<ApiResponse>((json, s) => s.When(json, r => r.Status).Eq(\"approved\").Then(...));",
          "domainTypes": ["Alis.Reactive.PlanModel.PayloadSource", "Alis.Reactive.PlanModel.PayloadContract"]
        },
        {
          "name": "TypedSource<TProp> (compile-time type carrier over a value source)",
          "variants": ["abstract; ToValueExpression() + Shape (Shape.FromClrType(TProp)) + ElementShape (collection item shape)", "the universal accepted type for When, And/Or, source-vs-source compare, gather, headers, route params, dispatch payload, plugin args, SetText, array From", "TProp preserves operator type safety in ConditionSourceBuilder"],
          "example": "TypedSource<int> page = p.FromUrl<int>(\"page\");",
          "domainTypes": ["Alis.Reactive.Builders.Conditions.TypedSource<TProp>"]
        },
        {
          "name": "TypedComponentSource<TProp> (component member as value source)",
          "variants": ["ctor(componentId, valueMember) (property read, Shape.FromClrType(TProp))", "FromMethod(ComponentSource, method, args) (method-return via Invoke)", "exposes DefaultPayloadName for gather naming", "minted by ComponentRef.Read<TValue>(ComponentProperty) / Read<TValue>(ComponentMethod) / Read<TValue>(ComponentMethod, args)"],
          "example": "g.Include<FusionDropDownList, OrderModel>(m => m.CareLevel)",
          "domainTypes": ["Alis.Reactive.Builders.Conditions.TypedComponentSource<TProp>", "ComponentSource", "ComponentRef"]
        },
        {
          "name": "TypedUrlSource<TProp> (URL query parameter as value source)",
          "variants": ["p.FromUrl(name) -> TypedUrlSource<string>", "p.FromUrl<T>(name) -> TypedUrlSource<T>", "ctor validates via UrlParameterName.Of and registers RequestScalarTarget.UrlQueryParameter<TProp>", "ToValueExpression -> ReadUrl(name, Shape.FromClrType(TProp))"],
          "example": "p.When(p.FromUrl<string>(\"tab\")).Eq(\"billing\").Then(...);",
          "domainTypes": ["Alis.Reactive.Builders.Conditions.TypedUrlSource<TProp>", "UrlSource"]
        },
        {
          "name": "PayloadTypedSource<TPayload,TProp> (event/response/error/dispatch payload property as source)",
          "variants": ["FromEvent(expression) (Event(PayloadContract.ForPayload(TPayload)) + expression)", "ctor(PayloadSource, expression) (any payload scope)", "minted by When(payload, path) (event) and ResponseBody<T>.Read(path) (success/error)", "ToValueExpression compiles lambda via ExpressionPathHelper.ToEventPath -> ReadPayload"],
          "example": "t.CustomEvent<CareEvent>(\"care\", (e, p) => p.When(e, x => x.Level).Eq(3).Then(...));",
          "domainTypes": ["PayloadTypedSource<TPayload,TProp>", "PayloadSource", "ResponseBody<T>"]
        },
        {
          "name": "ResponseBody<T> (typed HTTP response body source factory)",
          "variants": ["Read<TProp>(Expression<Func<T,TProp>>) -> TypedSource<TProp> via PayloadTypedSource over success/error scope", "passed as first lambda param of OnSuccess<TResponse>/OnError<TError> (compile-time inference, no runtime instance)", "also used directly with SetText/SetHtml binding"],
          "example": ".OnSuccess<ApiResponse>((json, s) => s.Element(\"name\").SetText(json, r => r.Data.Name));",
          "domainTypes": ["Alis.Reactive.ResponseBody<T>", "PayloadTypedSource<TPayload,TProp>"]
        },
        {
          "name": "TypedPluginSource<TProp> / TypedPluginPropertySource<TProp> (plugin member as value source)",
          "variants": ["TypedPluginSource<TProp> (method return; Invoke(PluginSource, method, Shape, args))", "TypedPluginPropertySource<TProp> (property read; Read(PluginSource, member, Shape))", "p.Plugin<T>(name, member) -> PluginReadBuilder<T> implicit -> TypedPluginSource<T>", "p.Plugin<T>(name) (root function)", "p.Plugin<T>(PluginFunction<T>)", "p.PluginProperty<T>(name, member) -> TypedPluginPropertySource<T>", "p.Plugin<T>(PluginProperty<T>)"],
          "example": "p.When(p.Plugin<int>(\"cart\",\"itemCount\").Arg(json, x => x.Items)).Gt(0).Then(...);",
          "domainTypes": ["TypedPluginSource<TProp>", "TypedPluginPropertySource<TProp>", "PluginReadBuilder", "PluginSource"]
        },
        {
          "name": "ReactiveValue<TValue> (scalar produced by an array operation, as a typed source)",
          "variants": ["wraps an array-op ValueExpression (count/sum/any/all/find) and is itself a TypedSource<TValue>", "plugs into SetText, When, dispatch payloads (base-source consumers)", "produced by ReactiveArray<T>.Count/Count(pred)/Any()/Any(pred)/All(pred)/Sum(selector)/Find(pred)/Find(pred,selector)"],
          "example": "p.When(p.From(json, r => r.Items).Count(x => x.Active)).Gt(5).Then(...);",
          "domainTypes": ["Alis.Reactive.Builders.Arrays.ReactiveValue<TValue>", "ArrayOperationExpression"]
        },
        {
          "name": "ReactiveArraySource<TElement> (transformed array exposed as a typed source)",
          "variants": ["internal TypedSource<TElement[]> over a composed array-op expression", "produced by ReactiveArray<T>.AsSource() so a filtered/mapped/sorted array binds to a component data source without an HTTP round-trip"],
          "example": "p.Component<FusionGrid>(m => m.Rows).SetDataSource(p.From(json, r => r.Rows).Where(x => x.Open).AsSource());",
          "domainTypes": ["ReactiveArraySource<TElement>", "TypedSource<TProp>"]
        },
        {
          "name": "ObjectExpression (composite value from named fields)",
          "variants": ["Object(IReadOnlyDictionary<string,ValueExpression>) (shape inferred, ObjectOf per field)", "Object(fields, Shape) (explicit)", "kind = object, field names non-empty, values non-null", "minted by DispatchPayloadBuilder.Build()"],
          "example": "p.DispatchWith<CarePayload>(\"care\", b => { b.Set(x => x.Level, p.FromUrl<int>(\"lvl\")); b.Set(x => x.Note, \"hi\"); });",
          "domainTypes": ["Alis.Reactive.PlanModel.ObjectExpression", "Shape"]
        },
        {
          "name": "ArrayExpression (composite value from ordered items)",
          "variants": ["Array(IReadOnlyList<ValueExpression>) (shape inferred: homogeneous -> ArrayOf(item), else ArrayOf(Any))", "Array(items, Shape) (explicit)", "empty -> ArrayOf(Shape.Any), items non-null", "builds In/NotIn/Between/Range operand arrays"],
          "example": "p.When(p.Component<FusionDropDownList>(m => m.State).Read<string>(...)).In(\"WA\",\"OR\",\"CA\").Then(...);",
          "domainTypes": ["Alis.Reactive.PlanModel.ArrayExpression", "Shape"]
        },
        {
          "name": "ArrayOperationExpression (deterministic op over an array-shaped value)",
          "variants": ["ArrayCount -> Number", "ArrayFilter(source, ConditionGraph, itemShape) -> ArrayOf(itemShape)", "ArrayMap(source, projection, itemShape, resultItemShape) -> ArrayOf(resultItemShape)", "ArraySum(source, projection?, itemShape) -> Number", "ArrayAny(source, predicate?, itemShape) -> Boolean", "ArrayAll(source, predicate, itemShape) -> Boolean", "ArrayFind(...)", "ArrayOrderBy(source, key, itemShape, descending) (orderBy/orderByDescending)", "kind = array-op, Op sub-discriminator", "predicate/projection nullable + WhenWritingNull", "read element scope"],
          "example": "p.When(p.From(json, r => r.Lines).Sum(x => x.Amount)).Gt(1000).Then(...);",
          "domainTypes": ["Alis.Reactive.PlanModel.ArrayOperationExpression", "ReactiveArray<TElement>", "ConditionGraph"]
        },
        {
          "name": "Shape (declared type contract carried by every value)",
          "variants": ["scalars: String, Number, Boolean, Date, Raw, Any, None", "ArrayOf(item) (rejects None)", "ObjectOf(fields) closed / OpenObject() open", "Nullable(inner)", "FromClrType(Type) (string/bool/date/numeric/Guid/TimeSpan/enum/collection inference, Nullable<T> unwrap)", "CollectionItemShapeOrNone(Type), FromValue(object?)", "IsScalar gate (header/route/query suitability), IsNone, structural Equals/==/!=", "ShapeJsonConverter write-only"],
          "example": "// Shape.FromClrType(typeof(int)) => Number",
          "domainTypes": ["Alis.Reactive.PlanModel.Shape", "ShapeStructure", "ShapeObjectContract"]
        },
        {
          "name": "ConditionSourceBuilder operand minting (source -> compare operands)",
          "variants": ["literal operand (Eq/NotEq/Gt/Gte/Lt/Lte typed) -> LiteralRaw(operand, sourceShape)", "text literal (Contains/StartsWith/EndsWith/Matches) -> LiteralRaw(string, String)", "MinLength -> LiteralRaw(MinimumTextLength, Number)", "unary -> ComparisonOperands.Unary(leftValue, shape)", "array (In/NotIn) -> Array of LiteralRaw", "Between -> two-endpoint Array", "ArrayContains -> CollectionItem(left, item, shape, elementShape)", "source-vs-source -> Binary(left, right.ToValueExpression(), shape)"],
          "example": "p.When(checkOut).Gt(checkIn) // two TypedSource<DateTime> compared",
          "domainTypes": ["ConditionSourceBuilder<TModel,TProp>", "ComparisonOperands", "ConditionGraph"]
        }
      ]
    },
    {
      "key": "validation",
      "title": "Validation",
      "blurb": "FluentValidation stays server authority; ReactiveValidator<T> declares explicit client metadata that binds to input components.",
      "features": [
        {
          "name": "ReactiveValidator<T> base class",
          "variants": ["abstract class ReactiveValidator<T> : AbstractValidator<T>, IClientValidationMetadataSource where T : class", "server rules and client metadata in the same ctor", "GetClientRules() returns IReadOnlyList<ClientValidationField> (single source for server + browser)"],
          "example": "public sealed class ResidentValidator : ReactiveValidator<ResidentModel> { public ResidentValidator() { ClientRule(m => m.Name).Required(\"Name is required\"); } }",
          "domainTypes": ["ReactiveValidator<T>", "IClientValidationMetadataSource", "ClientValidationRuleSet", "ClientValidationField"]
        },
        {
          "name": "ClientRule(field) - declare a server+client rule for one field",
          "variants": ["ClientRule<TValue>(Expression<Func<T,TValue>> field) -> ReactiveClientRuleBuilder<T,TValue> (pairs RuleFor + client field rule)", "ClientRule<TChild>(field, ReactiveValidator<TChild> validator) (nested child: SetValidator server, AddRulesFrom with prefix client)", "ClientRuleEach<TItem>(Expression<Func<T,IEnumerable<TItem>>> field) -> ReactiveClientCollectionRuleBuilder<T,TItem> (RuleForEach)"],
          "example": "ClientRule(m => m.Email).Required(\"Required\").Email(\"Bad email\");",
          "domainTypes": ["ReactiveClientRuleBuilder<TModel,TValue>", "ReactiveClientCollectionRuleBuilder<TModel,TItem>", "ClientValidationFieldRuleBuilder<TModel,TValue>", "ClientValidationFieldToken<TModel,TValue>", "ClientRuleActivation"]
        },
        {
          "name": "ClientRulesFrom - compose rules from another validator",
          "variants": ["ClientRulesFrom(ReactiveValidator<T> validator) (Include server + AddRulesFrom empty prefix)", "ClientRulesFrom<TSource>(ReactiveValidator<TSource> validator) (client-only from a different source, no server Include)"],
          "example": "ClientRulesFrom(new SharedContactRules());",
          "domainTypes": ["ReactiveValidator<T>", "ClientValidationRuleSet", "ValidationFieldPath.Empty", "ClientRuleActivation"]
        },
        {
          "name": "Single-field client rule kinds",
          "variants": ["Required (NotEmpty + required)", "Empty (empty)", "Email (string?, email)", "Url (url)", "CreditCard (creditCard)", "AtLeastOne (atLeastOne)", "MinLength(int) (minLength)", "MaxLength(int) (maxLength)", "Regex(string) (regex)", "Range(low,high) (range, struct+class)", "ExclusiveRange(low,high) (exclusiveRange, struct+class)", "Min (min, struct+class)", "Max (max, struct+class)", "GreaterThanOrEqualTo (min)", "LessThanOrEqualTo (max)", "GreaterThan (gt)", "LessThan (lt)", "EqualTo(literal) (equalTo)", "NotEqual(literal) (notEqual)"],
          "example": "ClientRule(m => m.Age).Range(0, 120, \"0-120\").GreaterThan(17, \"Must be 18+\");",
          "domainTypes": ["ReactiveClientRules", "ValidationRuleName", "ValidationRuleOperand (None/Literal/Range)", "ClientValidationLiteral", "ValidationRangeBounds", "Shape"]
        },
        {
          "name": "Cross-field (peer) client rule kinds",
          "variants": ["EqualTo(Expression peerField) (equalTo peer, server Equal)", "NotEqualTo(peerField) (notEqualTo peer)", "GreaterThan(peerField) (gt peer, struct+class)", "GreaterThanOrEqualTo(peerField) (min peer)", "LessThan(peerField) (lt peer)", "LessThanOrEqualTo(peerField) (max peer)", "field-rule-builder also accepts a pre-built ClientValidationFieldToken<TModel,TValue> peer overload per comparison"],
          "example": "ClientRule(m => m.ConfirmPassword).EqualTo(m => m.Password, \"Passwords must match\");",
          "domainTypes": ["PeerFieldValidationRuleOperand", "ClientValidationFieldReference", "ValidationRuleExecution.WithPeer", "ValidationPlanBinding.ResolvePeerValue", "ValueExpression"]
        },
        {
          "name": "WhenField - declarative client-side conditional activation (single field)",
          "variants": ["WhenField(Expression<Func<T,bool>>, defineRules) (Truthy)", "WhenFieldNot (Falsy)", "WhenField<TProp>(field, value, ...) (Eq)", "WhenFieldNot<TProp>(field, value, ...) (Neq)", "WhenFieldGt/Gte/Lt/Lte<TProp>(field, value, ...)", "WhenFieldNull/NotNull<TProp>", "WhenFieldEmpty/NotEmpty(Expression<Func<T,string?>>)", "WhenFieldIn/NotIn<TProp>(field, TProp[], ...)", "WhenFieldBetween<TProp>(field, low, high, ...)", "WhenFieldContains/StartsWith/EndsWith/Matches(field, string, ...)", "WhenFieldMinLength(field, int, ...)", "WhenFieldArrayContains<TProp>(Expression<Func<T,IEnumerable<TProp>?>>, value, ...)", "each wraps base.When(guard.ServerPredicate, defineRules)", "nested AND-composes via FieldCondition.All"],
          "example": "WhenField(m => m.IsVeteran, () => ClientRule(m => m.ServiceBranch).Required(\"Branch required\"));",
          "domainTypes": ["FieldStart<T,TProp>", "FieldGuard<T>", "FieldCondition (FieldCompare/FieldAll/FieldAny/FieldNot)", "CompareOperator", "FieldComparisonValue", "ConditionalClientRuleActivation", "ClientRuleActivation"]
        },
        {
          "name": "WhenFields - multi-field / composed guard condition",
          "variants": ["WhenFields(Func<FieldConditionBuilder<T>,FieldGuard<T>> buildCondition, defineRules)", "FieldConditionBuilder<T>.Field<TProp>(expr) -> FieldStart<T,TProp> with full operator set (Truthy/Falsy/Eq/Neq/Gt/Gte/Lt/Lte/IsNull/NotNull/IsEmpty/NotEmpty/Contains/StartsWith/EndsWith/Matches/MinLength/In/NotIn/Between/ArrayContains)", "FieldGuard<T>.And/Or/Not mirrored to client FieldCondition and server predicate"],
          "example": "WhenFields(f => f.Field(m => m.Country).Eq(\"US\").And(f.Field(m => m.Age).Gte(18)), () => ClientRule(m => m.Ssn).Required(\"SSN required\"));",
          "domainTypes": ["FieldConditionBuilder<T>", "FieldStart<T,TProp>", "FieldGuard<T>", "FieldCondition", "SelectedClientValidationField<TModel,TValue>", "CompareOperator"]
        },
        {
          "name": "Server-only conditions - FluentValidation When/Unless rejecting ClientRule",
          "variants": ["When(Func<T,bool>, action) and When(Func<T,ValidationContext<T>,bool>, action) (server-only)", "Unless(...) (both overloads)", "WhenAsync/UnlessAsync (both overloads, async)", "calling ClientRule inside throws InvalidOperationException (use WhenField)", ".Otherwise() re-enters server-only scope"],
          "example": "WhenAsync(async (m, ct) => await repo.ExistsAsync(m.Email, ct), () => RuleFor(m => m.Email).Must(_ => false).WithMessage(\"Taken\"));",
          "domainTypes": ["IConditionBuilder", "ServerOnlyConditionBuilder", "ClientConditionScope (_serverOnlyDepth)", "ClientRuleActivation"]
        },
        {
          "name": "App-level / non-FluentValidation client rules",
          "variants": ["ClientValidationRulesBuilder<TModel>.Field<TValue>(Expression) and .Field(ClientValidationFieldToken) -> ClientValidationFieldRuleBuilder (same rule surface, no server pairing)", "full rule set directly (Required/Empty/Email/Url/CreditCard/AtLeastOne/MinLength/MaxLength/Regex/Range/ExclusiveRange/Min/Max/GreaterThanOrEqualTo/LessThanOrEqualTo/GreaterThan/LessThan/EqualTo(literal|peer)/NotEqual/NotEqualTo(peer))", ".When(condition builder, define) -> ClientValidationConditionBuilder<TModel>.Field(...) with full operators + And/Or/Not", "combines via ClientRuleActivation.Combine"],
          "example": "services.AddReactiveClientValidation(b => b.Add<MyMeta, OrderModel>(r => r.Field(m => m.Qty).GreaterThan(0, \"Must be positive\")));",
          "domainTypes": ["ClientValidationRulesBuilder<TModel>", "ClientValidationFieldRuleBuilder<TModel,TValue>", "ClientValidationConditionBuilder<TModel>", "ClientValidationFieldConditionStart<TModel,TValue>", "ClientValidationCondition<TModel>"]
        },
        {
          "name": "DI registration of validation metadata",
          "variants": ["AddReactiveFluentValidation(b => b.Add<TValidator>() | .AddFromAssembly(asm) | .AddFromAssemblyContaining<T>()) (registers IValidator + ReactiveValidatorClientMetadataProvider, builds rules once per validator at startup)", "AddReactiveClientValidation(b => b.Add<TSource,TModel>(define)) (ConfiguredClientValidationMetadataProvider keyed by source type)", "both TryAddSingleton<IClientValidationRuleSource, ClientValidationRuleSource>", "ClientValidationRuleSource aggregates all providers + exposes Ambient for net48"],
          "example": "services.AddReactiveFluentValidation(b => b.AddFromAssemblyContaining<ResidentValidator>());",
          "domainTypes": ["ReactiveFluentValidationBuilder", "ReactiveClientValidationBuilder", "ClientValidationRuleSource", "IClientValidationMetadataProvider", "ReactiveValidatorClientMetadataProvider", "ConfiguredClientValidationMetadataProvider", "IClientValidationRuleSource"]
        },
        {
          "name": "Validate before request - wire form validation into the HTTP pipeline",
          "variants": ["HttpRequestBuilder<TModel>.Validate<TValidationSource>(string formId) records a ClientValidationBeforeRequest (source type + form container id)", "resolved at end of Render() into ComponentValidation via ValidationJob"],
          "example": "p.Post(\"/residents\").Validate<ResidentValidator>(\"resident-form\").Gather(...);",
          "domainTypes": ["HttpRequestBuilder<TModel>", "ClientValidationBeforeRequest", "ValidationJob", "ComponentId"]
        },
        {
          "name": "Render-time binding to plan domain",
          "variants": ["ClientValidationFieldBinder resolves each ClientValidationField to a registered input ComponentRegistration, else to a model-field IdGenerator id (ModelFieldInput)", "ValidationFieldBinding.ToComponentValidation builds ComponentValidation.ForServerField(componentId, ReadValue, planRules, serverFieldName)", "ValidationRule.ToPlanRule -> PlanModel.ValidationRule with execution WithoutTarget/WithConstraint(literal)/WithPeer(ValueExpression), activation Always or When(ConditionGraph)", "collection item-field expansion via RenderedItemFieldMatch", "shape mismatch / unknown peer or condition field throws"],
          "example": "(internal) binder.ResolveAll(field, ruleBinding) => ComponentValidation per registered/model field",
          "domainTypes": ["ClientValidationFieldBinder", "ValidationFieldBinding", "ModelFieldInput", "ValidationPlanBinding", "FieldConditionPlanBinding", "FieldComparisonTarget", "PlanModel.ComponentValidation", "PlanModel.ValidationRule", "ValidationRuleExecution", "ValidationRuleActivation", "ConditionGraph"]
        },
        {
          "name": "Fusion EJ2 grid column validationRules emit",
          "variants": ["FusionGridValidation.From<TValidator,TRow>(IClientValidationRuleSource) -> FusionGridFieldValidation<TRow>", ".Field<TField>(Expression<Func<TRow,TField>>) -> EJ2 column.validationRules ({ rule: [value, message] }) or null", "Ej2ColumnRules.From emits only unconditional single-field rules with EJ2 equivalents (required/email/url -> [true,msg], minLength/maxLength -> [int,msg], regex -> [pattern,msg], min/max -> [value,msg], numeric range -> [[lo,hi],msg])", "conditional/cross-field/exotic rules skipped (server-authoritative)"],
          "example": "new GridColumn { Field = \"openTasks\", EditType = \"numericedit\", ValidationRules = care.Field(r => r.OpenTasks) }",
          "domainTypes": ["FusionGridValidation", "FusionGridFieldValidation<TRow>", "Ej2ColumnRules", "ClientValidationField", "ValidationRule (IsUnconditional/LiteralOperand/RangeOperand)"]
        }
      ]
    },
    {
      "key": "components",
      "title": "Components",
      "blurb": "Browser objects with typed members; InputField wraps model-bound fields, vendor identity is the only resolver join key, 7-file vertical slices.",
      "features": [
        {
          "name": "Html.InputField - model-bound field wrapper",
          "variants": ["InputField(plan, m => m.Prop) (no label/required, InputFieldConfiguration.Default)", "InputField(plan, m => m.Prop, o => o.Label(\"X\").Required()) (configured)", "returns InputBoundField<TModel,TProp>", "chain a component factory to fill the wrapper", "wrapper emits div + optional label (required *) + content slot + data-valmsg-for error span", "Render() throws if the chained component never called RegisterInputComponent"],
          "example": "Html.InputField(plan, m => m.Name, o => o.Required().Label(\"Name\")).NativeTextBox(b => b.Placeholder(\"Enter name\"));",
          "domainTypes": ["InputBoundField<TModel,TProp>", "InputBoundFieldBase<THelper,TModel,TProp>", "BoundInputField<TModel,TProp>", "InputFieldOptions", "InputFieldConfiguration (Default/Configured)", "InputFieldBuilder", "InputFieldRenderScope", "ModelBoundInputComponentSlot", "InputComponentRenderTarget"]
        },
        {
          "name": "Vendor identity - IComponent / IInputComponent / IAppLevelComponent",
          "variants": ["IComponent.Vendor (NativeComponent => native, FusionComponent => fusion, only join key into resolver.ts)", "IInputComponent.ValueMember (JS gather + validation read, e.g. value, checked)", "IAppLevelComponent.DefaultId (fixed layout id)", "NativeComponent / FusionComponent abstract bases", "a 3rd vendor only touches resolver.ts + resolution/event-{vendor}.ts"],
          "example": "public sealed class NativeTextBox : NativeComponent, IInputComponent { public string ValueMember => \"value\"; }",
          "domainTypes": ["IComponent", "IInputComponent", "IAppLevelComponent", "NativeComponent", "FusionComponent", "ComponentVendor"]
        },
        {
          "name": "Native input component slices (7-file vertical slice)",
          "variants": ["NativeTextBox (value, Type/CssClass/Placeholder, Changed)", "NativeCheckBox (checked)", "NativeCheckList (multi-select array)", "NativeDropDown (value, select)", "NativeRadioGroup", "NativeTextArea", "NativeHiddenField", "each slice: Component.cs (IInputComponent + static Registration), Builder (renders <input> via html.TextBoxFor), HtmlExtensions (.NativeXxx() factory), ReactiveExtensions (.Reactive()), Extensions (SetValue/Read), Events + Events/*Args"],
          "example": "Html.InputField(plan, m => m.Email).NativeTextBox(b => b.Type(\"email\").Reactive(plan, e => e.Changed, (a,p) => p.Element(\"status\").SetText(\"changed\")));",
          "domainTypes": ["NativeTextBox", "NativeCheckBox", "NativeCheckList", "NativeDropDown", "NativeRadioGroup", "NativeTextArea", "NativeHiddenField", "NativeTextBoxBuilder<TModel,TProp>", "InputComponentRegistrationProfile", "ComponentRegistration", "RegisteredComponentIdentity", "RegisteredInputBinding", "ComponentKind"]
        },
        {
          "name": "Native non-input components",
          "variants": ["NativeButton (phantom type, NativeButtonBuilder, explicit elementId, no IInputComponent, reactive via .Reactive(e => e.Click))", "NativeActionLink (special 4-file slice, no Component.cs/Events; NativeActionLinkBuilder renders <a data-reactive-link=payloadJson>, own IdGenerator + Serializer that bakes a pipeline into the link payload)", "NativeButton(html, elementId, text) takes explicit developer-chosen id"],
          "example": "Html.NativeButton(\"save-btn\", \"Save\").Reactive(plan, e => e.Click, (a,p) => p.Component<FusionToast>().Success().Show());",
          "domainTypes": ["NativeButton", "NativeButtonBuilder<TModel>", "NativeActionLinkBuilder<TModel>", "NativeActionLinkIdGenerator", "NativeActionLinkSerializer"]
        },
        {
          "name": "Fusion (Syncfusion EJ2) input component slices",
          "variants": ["FusionDropDownList", "FusionAutoComplete", "FusionComboBox", "FusionMultiColumnComboBox", "FusionMultiSelect", "FusionTextBox", "FusionTextArea", "FusionSmartTextArea", "FusionInputMask", "FusionOtpInput", "FusionRichTextEditor", "FusionNumericTextBox", "FusionSlider", "FusionRating", "FusionDatePicker", "FusionTimePicker", "FusionDateTimePicker", "FusionDateRangePicker", "FusionCheckBox", "FusionSwitch", "FusionColorPicker", "FusionDropDownTree", "FusionFileUpload", "FusionInPlaceEditor", "all FusionComponent + IInputComponent; HtmlExtension renders via setup.Helper.EJS().XxxFor(expr).HtmlAttributes(id+name) and registers", "typed .Fields<TItem>(t=>t.Text, v=>v.Value[, g=>g.Group])", "Extensions expose SetValue/SetText/SetDataSource (event/response/TypedSource<T[]> overloads) + DataBind/FocusIn/FocusOut/ShowPopup/HidePopup + Value()"],
          "example": "Html.InputField(plan, m => m.Country).FusionDropDownList(b => { b.Fields<Item>(t => t.Text, v => v.Value); b.Reactive(plan, e => e.Changed, (a,p) => p.Element(\"out\").SetText(\"picked\")); });",
          "domainTypes": ["FusionDropDownList", "FusionDatePicker", "FusionNumericTextBox", "FusionTextBox", "FusionMultiSelect", "FusionComponent", "ComponentProperty<TValue>", "ComponentMethod", "Shape", "PayloadSource", "ResponseBody<T>", "TypedSource<T[]>"]
        },
        {
          "name": "Fusion display/container component slices (events + methods, no form value)",
          "variants": ["FusionGrid (DataStateChange/server binding)", "FusionAccordion", "FusionTab", "FusionDialog", "FusionSidebar", "FusionStepper", "FusionToolbar", "FusionMenu", "FusionContextMenu", "FusionBreadcrumb", "FusionCarousel", "FusionChipList", "FusionListView", "FusionListBox", "FusionKanban", "FusionPivotView", "FusionTooltip", "FusionMention", "FusionAIAssistView", "FusionBulletChart", "FusionButton", "FusionDropDownButton", "FusionSplitButton", "FusionProgressButton", "FusionSmartPasteButton", "FusionRadioButton", "all extend FusionComponent only (no IInputComponent)", "HtmlExtension takes explicit elementId, renders directly with no InputField wrapper / no input registration", "events via .Reactive(), methods/props via p.Component<T>(\"id\")"],
          "example": "Html.FusionGrid(plan, \"residents-grid\", b => b.DataSource(rows)).Reactive(plan, e => e.RowSelected, (a,p) => p.Element(\"detail\").SetText(a, x => x.Name));",
          "domainTypes": ["FusionGrid", "FusionAccordion", "FusionTab", "FusionDialog", "FusionSidebar", "FusionButton", "FusionGridBuilder<TModel>", "FusionAccordionBuilder<TModel>", "TypedEvent<TArgs>"]
        },
        {
          "name": "FusionSchedule (scheduler / appointment calendar)",
          "variants": ["Html.FusionSchedule(plan, string elementId, Action<ScheduleBuilder> build) (non-input, no InputField wrapper, no input registration; renders SF Schedule directly)", "events via .Reactive(evt => evt.Member, (args, p) => ...): CellClicked (cellClick), EventClicked (eventClick), ActionBegin (actionBegin: eventCreate/eventChange/eventRemove/dateNavigate/viewNavigate), ActionComplete (actionComplete: added/changed/deletedRecords), Navigating (navigating: date/view, cancelable), PopupOpen (popupOpen: QuickInfo/Editor/DeleteAlert, cancelable), PopupClose (popupClose), DataBound (dataBound), EventRendered (eventRendered, cancelable)", "mutations via p.Component<FusionSchedule>(\"id\"): SetDataSource (event/response/TypedSource overloads), SetView (currentView), SetSelectedDate; methods dataBind/getEvents/addEvent/saveEvent/deleteEvent/openEditor/closeEditor/refreshEvents/print", "non-input: no Value()/SetValue(), data is server-driven via SetDataSource"],
          "example": "Html.FusionSchedule(plan, \"shift-schedule\", b => { /* views, resources */ }).Reactive(evt => evt.CellClicked, (args, p) => p.Component<FusionDialog>(\"edit-dialog\").Show());",
          "domainTypes": ["FusionSchedule", "FusionScheduleBuilder<TModel>", "FusionScheduleEvents", "FusionScheduleReactiveExtensions", "FusionScheduleExtensions", "TypedEvent<TArgs>", "ComponentProperty<TValue>", "ComponentMethod", "ComponentRef<TComponent,TModel>"]
        },
        {
          "name": "p.Component<T>() - typed component reference (ComponentRef)",
          "variants": ["Component<T>(m => m.Prop) (id from model expression via IdGenerator)", "Component<T,TOtherModel>(m => m.Prop) (cross-partial)", "Component<T>(\"explicit-id\") (non-input/display)", "Component<T>() where T:IAppLevelComponent (layout singleton via DefaultId -> LayoutObjectTarget)", "EmitSet(property,value) => Set", "EmitCall(method[,args]) => Call", "Read(property)/Read<T>(method,args) => TypedComponentSource"],
          "example": "p.Component<FusionDropDownList>(m => m.Country).SetValue(\"US\").DataBind();",
          "domainTypes": ["ComponentRef<TComponent,TModel>", "ComponentObjectTarget (ObjectTarget/LayoutObjectTarget)", "ComponentKey", "ComponentSource", "ComponentProperty<TValue>", "ComponentMethod", "ReactionGraph.Set", "ReactionGraph.Call", "TypedComponentSource<T>", "MemberAccess", "ObjectPropertyContract", "ObjectMethodContract"]
        },
        {
          "name": "p.Element(id) - raw DOM element mutations",
          "variants": ["AddClass/RemoveClass/ToggleClass(name) (classList Call)", "SetText(literal) / SetText(source, m=>m.Path) / SetText(ResponseBody<T>, path) / SetText(TypedSource<T>) (textContent Set)", "SetHtml(literal) / SetHtml(eventPayload, path) / SetHtml(TypedSource<T>) (innerHTML Set)", "Show()/Hide() (hidden false/true)", "members via BrowserElementMembers (classAdd->classList.add, text->textContent, html->innerHTML, hidden)"],
          "example": "p.Element(\"status\").AddClass(\"active\").SetText(\"Saved\");",
          "domainTypes": ["ElementBuilder<TModel>", "BrowserElementMembers", "ComponentKey", "ComponentSource", "ReactionGraph.Set", "ReactionGraph.Call", "ComponentProperty<TValue>", "ComponentMethod"]
        },
        {
          "name": "App-level components - layout singletons with fixed DOM id",
          "variants": ["NativeDrawer (id alis-drawer): Open/Close/SetSize(DrawerSize Sm/Md/Lg) + Html.NativeDrawer()", "NativeLoader (id alis-loader): Show/Hide/SetTarget(id)/SetTimeout(ms) + Html.NativeLoader()", "FusionConfirm: SetContent/Show/Hide + Html.FusionConfirmDialog()", "FusionToast: SetTitle/SetContent/SetTimeout/ShowCloseButton/ShowProgressBar + Success/Warning/Danger/Info + Show/Hide + Html.FusionToast() (ToastPosition/ToastType)", "all IAppLevelComponent => p.Component<T>() with no expression"],
          "example": "p.Component<NativeDrawer>().SetSize(DrawerSize.Lg).Open();",
          "domainTypes": ["NativeDrawer", "NativeLoader", "FusionConfirm", "FusionToast", "IAppLevelComponent", "DrawerSize", "DrawerPosition", "ToastPosition", "ToastType", "ComponentRef<TComponent,TModel>", "LayoutObjectTarget"]
        },
        {
          "name": ".Reactive() - component event wiring",
          "variants": ["Native: builder.Reactive(plan, evt => evt.Changed, (args, p) => {...}) on NativeXxxBuilder", "Fusion: builder.Reactive(plan, evt => evt.Changed, (args, p) => {...}) on the EJ2 builder (componentId from HtmlAttributes[\"id\"])", "event selector returns TypedEvent<TArgs> from a sealed Events.Instance", "always the last call in the chain", "goes through ComponentEventOnboarding.Wire => plan.Context.WireComponentEvent(id, vendor, jsEvent, reaction)"],
          "example": "b.Reactive(plan, e => e.Changed, (args, p) => p.Component<FusionToast>().SetContent(\"saved\").Show());",
          "domainTypes": ["TypedEvent<TArgs>", "NativeTextBoxEvents", "FusionDropDownListEvents", "ComponentEventOnboarding", "PipelineBuilder<TModel>", "ReactionGraph"]
        },
        {
          "name": "Typed Fusion templates (column/item template builder)",
          "variants": ["FusionTemplate.Create<TModel>() => FusionTemplateBuilder<TModel>", "Id/Class/Attr", "Text(literal) / Text(m=>m.Prop)", "Span/Img/Badge/Icon/Link (literal + bound + css overloads)", "Div (nested)", "Button(text,onClick[,css])", "ButtonFor(text, m=>m.Id, fn[,css])", "EventButton(text, eventName, m=>m.Id[,css]) (dispatches custom event with row id)", "When(cond, then[, else]) / ShowIf(cond, content) (SF ${if}/${else})", "Raw(html)", "Render() => HTML string"],
          "example": "FusionTemplate.Create<Item>().Span(m => m.Name, \"font-bold\").EventButton(\"Edit\", \"edit-row\", m => m.Id);",
          "domainTypes": ["FusionTemplateBuilder<TModel>", "FusionTemplate", "FusionConditionalBuilder<TModel>", "FusionTemplateExpression", "TemplateElements", "TemplateElementId", "TemplateCss", "TemplateAltText", "TemplateElseBranch<TModel>"]
        },
        {
          "name": "Input component registration profile (the join into the plan)",
          "variants": ["InputComponentRegistrationProfile.For(component, componentTypeName) (static per component)", ".RegisterInputComponent(profile) in the HtmlExtension => slot.Register(profile) => ComponentRegistration.RegisteredInput(identity, binding, kind, valueShape)", "carries RegisteredComponentIdentity (id+vendor), RegisteredInputBinding (bindingPath + value MemberName), ComponentKind, value Shape", "ModelBoundInputComponentSlot.For<TModel,TProp>(expr, html.NameFor(expr)) is the deterministic join key"],
          "example": "setup.RegisterInputComponent(NativeTextBox.Registration); // inside .NativeTextBox()",
          "domainTypes": ["InputComponentRegistrationProfile", "ModelBoundInputComponentSlot", "ComponentRegistration", "RegisteredComponentIdentity", "RegisteredInputBinding", "ComponentKind", "ComponentId", "BindingPath", "MemberName", "Shape"]
        }
      ]
    },
    {
      "key": "slots-plugins",
      "title": "Slots & Plugins",
      "blurb": "SSR/browser composition by PlanId and the typed escape hatch; plugins, Into, and app-level singletons flow through ValueExpression.",
      "features": [
        {
          "name": "Root plan creation (Html.ReactivePlan)",
          "variants": ["html.ReactivePlan<TModel>() (root-view, RootViewPlanScope, RendersValidationSummary=true)", "internal new ReactivePlan<TModel>() / new ReactivePlan<TModel>(ReactivePlanScope) used by factory only (constructors internal)"],
          "example": "var plan = Html.ReactivePlan<OrderModel>();",
          "domainTypes": ["ReactivePlan<TModel>", "ReactivePlanScope", "RootViewPlanScope", "PlanIdentity (Root)", "PlanId", "PlanBuildContext"]
        },
        {
          "name": "Same-model partial plan creation (Html.ResolvePlan)",
          "variants": ["html.ResolvePlan<TModel>() (partial-view, merges into the owning view's plan by shared PlanId, PartialViewPlanScope, RendersValidationSummary=false)"],
          "example": "var plan = Html.ResolvePlan<OrderModel>();",
          "domainTypes": ["ReactivePlan<TModel>", "ReactivePlanScope", "PartialViewPlanScope", "PlanIdentity (Partial)", "PlanScope.Partial / PartialPlanScope"]
        },
        {
          "name": "Plan rendering / serialization (Html.RenderPlan + Render)",
          "variants": ["html.RenderPlan(plan) (emits <script type=application/json data-reactive-plan id=alis-plan-{planId}>; appends hidden validation-summary div only for root scope)", "plan.Render() (compact camelCase)", "plan.Render(IServiceProvider) (resolves validation source from DI)", "plan.RenderFormatted() / RenderFormatted(IServiceProvider) (indented)", "plan.PlanId", "plan.IsPartial"],
          "example": "@Html.RenderPlan(plan)",
          "domainTypes": ["PlanExtensions", "ReactivePlanSerializer", "PlanDocument (Version=3, PlanId, Scope, Types, Components, Behaviors)", "PlanIdentity", "PlanScope / RootPlanScope / PartialPlanScope"]
        },
        {
          "name": "Plan composition keys (PlanId / PlanScope / SlotId)",
          "variants": ["PlanId.ForModel(type) (full-type-name identity to compose root + same-model partials at boot)", "PlanScope.Root (\"root\") vs PlanScope.Partial (\"partial\") (selects boot-compose vs slot-load)", "Object Contract Merge / Component Merge (compatible declarations compose from boot + active slot sources, runtime side)"],
          "example": "// PlanId derived automatically: plan.PlanId == typeof(OrderModel).FullName",
          "domainTypes": ["PlanId", "PlanIdentity", "PlanScope", "RootPlanScope", "PartialPlanScope", "PlanDocument"]
        },
        {
          "name": "Partial slot injection (p.Into)",
          "variants": ["p.Into(elementId) (injects HTTP success response body as HTML into a host element/slot; declares element, reads whole success payload, emits InjectReaction kind=\"inject\", target.slot)", "slot load/unload + active-plan recomposition are runtime-keyed by SlotId (no separate load/unload DSL verb in C#)"],
          "example": "p.Get(\"/orders/panel\").Into(\"order-panel\");",
          "domainTypes": ["InjectReaction (Slot, Value)", "ReactionGraph.Inject", "ComponentKey", "ValueExpression.ReadWholePayload", "PayloadSource.Success"]
        },
        {
          "name": "Plugin registration on the plan (RegisterPlugin)",
          "variants": ["plan.RegisterPlugin(string name, Action<PluginTypeBuilder>) (stringly compatibility)", "plan.RegisterPlugin(ReactivePlugin) (typed instance)", "plan.RegisterPlugin<TPlugin>() where TPlugin: ReactivePlugin, new() (construct + register + return typed descriptor)"],
          "example": "var url = plan.RegisterPlugin<UrlPlugin>();",
          "domainTypes": ["ReactivePlan<TModel>.RegisterPlugin", "PluginTypeBuilder", "ReactivePlugin", "PluginContract", "PlanBuildContext.RegisterPlugin", "BrowserObjectContracts"]
        },
        {
          "name": "Typed plugin descriptor (ReactivePlugin base)",
          "variants": ["Function<TReturn>(member) / Function<TReturn>() (root) / Function<TReturn>(member, Action<PluginArgumentTypes>) / Function<TReturn>(Action<...>) (root)", "Function<TReturn,TArg1>(member|root) / Function<TReturn,TArg1,TArg2>(member|root) / Function<TReturn,TArg1,TArg2,TArg3>(member|root)", "Command(member) / Command() (root) / Command(member, Action<...>) / Command(Action<...>) (root)", "Command<TArg1>(member|root) / Command<TArg1,TArg2>(member|root) / Command<TArg1,TArg2,TArg3>(member|root)", "Property<TValue>(member)", "PluginFunction<TReturn>.Arg<TArg>() / .Args(...) and PluginCommand.Arg<TArg>() / .Args(...)", "EnsureNoPropertyMethodCollision"],
          "example": "class UrlPlugin : ReactivePlugin { public UrlPlugin():base(\"url\"){ } public PluginFunction<string> Param = ...; }",
          "domainTypes": ["ReactivePlugin", "PluginOperation", "PluginFunction<TReturn>", "PluginCommand", "PluginProperty<TValue>", "PluginMemberDeclarations", "PluginArgumentTypes", "Shape", "MethodSignature", "MethodArgumentContract"]
        },
        {
          "name": "Stringly plugin contract builder (PluginTypeBuilder)",
          "variants": ["Method<T>(name) / Method<TReturn>(name, Action<PluginArgumentTypes>) / Method<TReturn,TArg1>(name) / Method<TReturn,TArg1,TArg2>(name) / Method<TReturn,TArg1,TArg2,TArg3>(name)", "Function<T>() (root) / Function<TReturn>(Action<...>) / Function<TReturn,TArg1>() / Function<TReturn,TArg1,TArg2>() / Function<TReturn,TArg1,TArg2,TArg3>()", "Property<T>(name)", "Void(name) / Void(name, Action<...>) / Void() (root) / Void(Action<...>) / Void<TArg1>(name|root) / Void<TArg1,TArg2>(name|root) / Void<TArg1,TArg2,TArg3>(name|root)", "Command(name) / Command(name, Action<...>) / Command() / Command(Action<...>) (alias for Void)", "PluginArgumentTypes.Arg<T>()"],
          "example": "plan.RegisterPlugin(\"url\", p => p.Method<string>(\"getToken\"));",
          "domainTypes": ["PluginTypeBuilder", "PluginArgumentTypes", "PluginOperationContract", "PluginPropertyContract", "PluginOperationId", "PluginPropertyId", "MethodArgumentContract", "MethodSignature", "Shape"]
        },
        {
          "name": "Plugin function read (p.Plugin<T> value source)",
          "variants": ["p.Plugin<T>(pluginName, member) (member function return)", "p.Plugin<T>(pluginName) (root function return)", "p.Plugin<T>(PluginFunction<T>) (typed descriptor)", "returns PluginReadBuilder<T,TModel>", ".Arg(...) chains", "implicit conversion to TypedPluginSource<T> (no Build())"],
          "example": "p.SetText(label, p.Plugin<string>(\"url\", \"getToken\"));",
          "domainTypes": ["PluginReadBuilder<TReturn,TModel>", "TypedPluginSource<TProp>", "PluginOperationId", "PluginMethodRequirement.Function", "ValueExpression.Invoke", "PluginSource", "PluginArguments"]
        },
        {
          "name": "Plugin property read (p.PluginProperty / p.Plugin property)",
          "variants": ["p.PluginProperty<T>(pluginName, member) (stringly property read)", "p.Plugin<T>(PluginProperty<T>) (typed property descriptor)", "returns TypedPluginPropertySource<T>"],
          "example": "p.When(p.PluginProperty<bool>(\"feature\", \"enabled\")).Then(...);",
          "domainTypes": ["TypedPluginPropertySource<TProp>", "PluginPropertyId", "PluginPropertyRequirement.Read", "ValueExpression.Read", "PluginSource"]
        },
        {
          "name": "Plugin command call (p.Plugin(...).Fire)",
          "variants": ["p.Plugin(pluginName, member) (void member command)", "p.Plugin(pluginName) (plugin root function as command)", "p.Plugin(PluginCommand) (typed command descriptor)", "returns PluginCallBuilder<TModel>", "terminal .Fire() emits the call reaction"],
          "example": "p.Plugin(\"clipboard\", \"copy\").Arg(\"hello\").Fire();",
          "domainTypes": ["PluginCallBuilder<TModel>", "PluginOperationId", "PluginMethodRequirement.Command", "ReactionGraph.Call", "CallReaction", "PluginSource", "PluginArguments"]
        },
        {
          "name": "Plugin invocation arguments (.Arg overloads)",
          "variants": ["Arg<TResponse,TProp>(ResponseBody<TResponse>, Expression) (response-body, success/error scope)", "Arg<TArgs,TProp>(TArgs, Expression) (event-args, FromEvent)", "Arg<TArg>(TypedSource<TArg>) (typed source)", "Arg(string) / Arg(int) / Arg(bool) / Arg(long) / Arg(decimal) / Arg(double) / Arg(DateTime) (literals)", "ArgValue<TValue>(TValue) (generic literal)", "on both PluginCallBuilder and PluginReadBuilder", "arg shapes validated against MethodArgumentContract"],
          "example": "p.Plugin<string>(\"fmt\",\"join\").Arg(\",\").Arg(p.Read(grid)).Fire();",
          "domainTypes": ["PluginInvocationArgument", "PluginArguments", "MethodArgumentContract", "ValueExpression", "Shape", "ResponseBody<T>", "PayloadSource.Event"]
        },
        {
          "name": "Plugin read in HTTP gather (Gather.Plugin)",
          "variants": ["GatherBuilder.Plugin<T>(TypedPluginSource<T> source, string paramName) (invoke/read plugin and assign result into the request payload before fetch)"],
          "example": "p.Post(\"/save\").Gather(g => g.Plugin(p.Plugin<string>(\"auth\",\"token\"), \"csrf\"));",
          "domainTypes": ["GatherBuilder<TModel>", "TypedPluginSource<T>", "RequestInputAssignment.Payload", "BindingPath", "ValueExpression"]
        },
        {
          "name": "App-level / layout object reference (p.Component<TAppLevel>)",
          "variants": ["p.Component<TAppLevel>() where TAppLevel: IAppLevelComponent, new() (fixed-id layout object target, role.kind=\"layout-object\", using DefaultId + Vendor; no per-instance wiring or model binding)", "contrast p.Component<TComponent>(expr) / <TComponent,TOtherModel>(expr) / <TComponent>(refId) (model-bound or explicit-id)"],
          "example": "p.Component<NativeDrawer>().Open();",
          "domainTypes": ["IAppLevelComponent", "ComponentObjectTarget.ForLayout", "LayoutObjectTarget", "ComponentRef<TComponent,TModel>", "PlanBuildContext.DeclareLayoutObject"]
        },
        {
          "name": "Native Drawer app-level object",
          "variants": ["Html.NativeDrawer() (render fixed-id <aside> in _Layout once)", "Open() (add visible class + remove aria-hidden)", "Close() (remove visible class)", "SetSize(DrawerSize.Sm|Md|Lg) (swap size class; DrawerPosition enum also defined)"],
          "example": "p.Component<NativeDrawer>().SetSize(DrawerSize.Lg).Open();",
          "domainTypes": ["NativeDrawer (IAppLevelComponent)", "NativeDrawerExtensions", "DrawerSize", "DrawerPosition", "ComponentMethod", "ValueExpression.Literal", "ComponentRef"]
        },
        {
          "name": "Native Loader app-level object",
          "variants": ["Html.NativeLoader() (render fixed-id overlay in _Layout once)", "Show() / Hide() (toggle visible class + aria-hidden)", "SetTarget(targetId) (data-target covers a specific element)", "SetTimeout(ms) (data-timeout auto-hide)"],
          "example": "p.Component<NativeLoader>().SetTarget(\"grid\").Show();",
          "domainTypes": ["NativeLoader (IAppLevelComponent)", "NativeLoaderExtensions", "ComponentMethod", "ValueExpression.Literal", "ComponentRef"]
        },
        {
          "name": "Fusion Toast app-level object",
          "variants": ["Html.FusionToast() (render EJ2 Toast singleton in _Layout once)", "setters SetTitle / SetContent / SetTimeout(ms) / ShowCloseButton / ShowProgressBar", "type convenience Success / Warning / Danger / Info (set e-toast-* cssClass)", "actions Show() (dataBind + show) / Hide()", "ToastPosition and ToastType value types"],
          "example": "p.Component<FusionToast>().Success().SetContent(\"Saved\").Show();",
          "domainTypes": ["FusionToast (IAppLevelComponent)", "FusionToastExtensions", "ComponentProperty<T>", "ComponentMethod", "ToastPosition", "ToastType", "ValueExpression.Literal", "ComponentRef"]
        },
        {
          "name": "Fusion Confirm app-level object",
          "variants": ["Html.FusionConfirmDialog() (render fixed-id dialog host in _Layout once)", "SetContent(message) (set content property + dataBind)", "Show() / Hide()", "note confirm-as-guard (p.Confirm(...).Then(...)) lives in the conditions area - this slice is the imperative dialog object"],
          "example": "p.Component<FusionConfirm>().SetContent(\"Delete?\").Show();",
          "domainTypes": ["FusionConfirm (IAppLevelComponent)", "FusionConfirmExtensions", "ComponentProperty<string>", "ComponentMethod", "ValueExpression.Literal", "ComponentRef"]
        },
        {
          "name": "Native ActionLink (inline-plan link)",
          "variants": ["Html.NativeActionLink(linkText, url, Action<PipelineBuilder<TModel>>) (renders <a data-reactive-link=...> whose payload is a self-contained inline plan executed on click)", "fluent .CssClass(css) / .Attr(name, value) (reserved id/href/data-reactive-link cannot be overridden; class attr routes to CssClass)", "id assigned by NativeActionLinkIdGenerator.Next", "single-request constraint enforced by NativeActionLinkSingleRequestAnalyzer"],
          "example": "@Html.NativeActionLink(\"Delete\", \"/items/1\", p => p.Delete(\"/items/1\").Into(\"list\"))",
          "domainTypes": ["NativeActionLinkBuilder<TModel>", "NativeActionLinkHtmlExtensions", "NativeActionLinkSerializer", "NativeActionLinkIdGenerator", "PipelineBuilder<TModel>"]
        }
      ]
    },
    {
      "key": "domain-contract",
      "title": "Domain Contract & Serialization",
      "blurb": "Where every area converges: PlanDocument (Version 3) serializes write-only via kind discriminators and projects into generated TS unions.",
      "features": [
        {
          "name": "PlanDocument (serialized plan root)",
          "variants": ["Version => 3 (mirrored as TS version: 3)", "PlanId => identity.PlanIdForJson (camelCased to planId)", "Scope => identity.ScopeForJson (root | partial)", "Types => IReadOnlyDictionary<string, BrowserObjectContract>", "Components => IReadOnlyDictionary<string, ComponentObject>", "Behaviors => IReadOnlyList<Behavior>", "built only by PlanBuildContext.BuildPlan()"],
          "example": "@Html.RenderPlan(plan)  // serializes the PlanDocument produced by plan.Render()",
          "domainTypes": ["PlanDocument", "PlanBuildContext", "PlanIdentity", "PlanScope"]
        },
        {
          "name": "PlanIdentity / PlanId / PlanScope (plan identity & merge scope)",
          "variants": ["PlanId.ForModel(Type) (uses modelType.FullName)", "PlanId.Of(string) (explicit)", "PlanIdentity.Root(PlanId) -> RootPlanScope (\"root\")", "PlanIdentity.Partial(PlanId) -> PartialPlanScope (\"partial\", same planId merges in browser)", "PlanScope polymorphic abstract with Kind discriminator", "Root/Partial are the two singletons"],
          "example": "var plan = Html.ReactivePlan<Order>();  // root scope, planId = Order.FullName",
          "domainTypes": ["PlanIdentity", "PlanId", "PlanScope", "RootPlanScope", "PartialPlanScope"]
        },
        {
          "name": "WriteOnlyPolymorphicConverter<T> (kind-discriminated polymorphic serialization)",
          "variants": ["Write: delegates to JsonSerializer.Serialize(writer, value, value.GetType(), options) so each concrete subtype writes its own kind", "Read: throws NotSupportedException(\"Plan types are write-only.\")", "registered on bases Source, ReactionGraph, ConditionGraph, ValueExpression, StartsWhen, PlanScope, PayloadContract, ParallelCompletion, ServerPushEventFilter, ValueReadAccess, DispatchPayload(internal)", "hand-written siblings: ShapeJsonConverter, PathJsonConverter/PathSegmentJsonConverter, BranchCaseJsonConverter, BranchGuardJsonConverter, CompareConditionJsonConverter, DispatchReactionJsonConverter, DispatchPayloadJsonConverter"],
          "example": "[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ReactionGraph>))] public abstract class ReactionGraph",
          "domainTypes": ["WriteOnlyPolymorphicConverter<T>", "Source", "ReactionGraph", "ConditionGraph", "ValueExpression", "StartsWhen", "PlanScope", "PayloadContract"]
        },
        {
          "name": "ReactivePlanSerializer (JSON emit + embed)",
          "variants": ["Serialize(PlanDocument) (compact JSON, JsonNamingPolicy.CamelCase)", "SerializeFormatted(PlanDocument) (indented, CamelCase)", "RenderPlan<TModel> embeds JSON in <script type=\"application/json\" id=\"alis-plan-{planId}\" data-reactive-plan data-trace=\"trace\">", "root view also emits <div data-reactive-validation-summary hidden> fallback, partials emit script only"],
          "example": "@Html.RenderPlan(plan)  // emits the <script data-reactive-plan> the runtime discovers",
          "domainTypes": ["ReactivePlanSerializer", "PlanDocument", "PlanExtensions"]
        },
        {
          "name": "PlanTypeScriptContract -> PlanTypeGenerator -> runtime/types/plan.ts",
          "variants": ["PlanTypeScriptContract.Render() builds a TypeScriptContract and renders the full discriminated-union file", "declaration builders Interface(name).Requires/.Optional, Union(name, members...), LiteralUnion(name, values), Alias(name, type), ComponentVariant(...)", "Literal(value) wraps a string as TS literal; LiteralUnion projects C# value-object .Values into a string-literal union", "emit primitives TypeScriptContract.GeneratedBy, TypeScriptInterface, TypeScriptProperty (Required/Optional), TypeScriptTypeAlias, TypeScriptType (Single/Union), TypeScriptWriter (Indent/Outdent/Line/BlankLine, CRLF->LF)", "PlanTypeGenerator Program.Main(args) writes to args[0] (default Alis.Reactive.Assets/runtime/types/plan.ts); invoked by npm generate:plan-types", "output is // <auto-generated />, version literal 3 in lockstep with PlanDocument.Version"],
          "example": "npm run generate:plan-types -w Alis.Reactive.Assets  // C# plan domain -> runtime/types/plan.ts",
          "domainTypes": ["PlanTypeScriptContract", "TypeScriptContract", "TypeScriptInterface", "TypeScriptTypeAlias", "TypeScriptType", "TypeScriptWriter", "PlanTypeGenerator.Program"]
        },
        {
          "name": "Shape (cross-cutting type contract value object)",
          "variants": ["scalar singletons String/Number/Boolean/Date/Raw/Any/None (kind discriminators)", "ArrayOf(item) -> ArrayShape { item } (rejects None)", "ObjectOf(fields) -> ObjectShape { fields, additional:false }", "OpenObject() -> ObjectShape { fields:{}, additional:true }", "Nullable(inner) -> NullableShape { inner } (rejects None)", "FromClrType(Type) (Nullable<T>, string, bool, Date types, numerics, Guid/TimeSpan/TimeOnly->string, enum->string, IEnumerable<T>->array, else Any)", "FromValue(object?) (None for null else FromClrType)", "CollectionItemShapeOrNone(Type), IsScalar, structural Equals", "ShapeJsonConverter (writes kind + nested item/inner/fields/additional)"],
          "example": "Shape.Nullable(Shape.ArrayOf(Shape.String))  // nullable<array<string>> type contract",
          "domainTypes": ["Shape", "ShapeStructure", "ShapeObjectContract", "ShapeJsonConverter"]
        },
        {
          "name": "Path / PathSegment (member navigation path)",
          "variants": ["Path.None, Path.Property(name), path.Then(name), path.AtIndex(index)", "Path.Parse(dotPath) (splits on '.', numeric parts -> IndexSegment else PropertySegment, rejects empty)", "PathSegment.Property(name) -> PropertyPathSegmentBody (\"property\")", "PathSegment.AtIndex(index) -> IndexPathSegmentBody (\"index\", non-negative PathIndex)", "PathJsonConverter writes a bare JSON array", "TS Path=PathSegment[], EmptyPath=[], StructuredPath=[PathSegment,...PathSegment[]]", "Path.Overlaps/IsPrefixOf for binding-path conflict detection"],
          "example": "Path.Parse(\"Address.Lines.0.City\")  // [property, property, index, property]",
          "domainTypes": ["Path", "PathSegment", "PathSegmentBody", "PathIndex", "PathJsonConverter", "PathSegmentJsonConverter"]
        },
        {
          "name": "PlanString family (validated string value objects)",
          "variants": ["base PlanString (non-null + non-empty default, Allow for RequestUrl, value equality by Type+Value)", "PlanId, ComponentId, ComponentKey, TypeKey (NativeElement/ComponentObject/Plugin factories), BindingPath (carries parsed Path), MemberName, ComponentKind, EventName, PluginName (no-whitespace)", "RequestUrl (empty allowed), HeaderName, RouteParameterName ([a-zA-Z0-9_]), ComponentVendor (token regex; Native/Fusion singletons + From), PayloadTypeName", "constrained enum-like with .Values: MemberAccess (read/write/readwrite + Widen), HttpMethodName (GET/POST/PUT/DELETE), RequestBodyFormat (json/form-data), PayloadScope (event/success/error/request/dispatch/local/element)", "non-PlanString value objects MinimumTextLength (>=0), HttpResponseStatusCode (100-599), PathIndex (>=0)"],
          "example": "ComponentVendor.From(\"fusion\")  // validated vendor token value object",
          "domainTypes": ["PlanString", "ComponentKey", "TypeKey", "ComponentVendor", "MemberAccess", "HttpMethodName", "RequestBodyFormat", "PayloadScope"]
        },
        {
          "name": "PayloadContract (payload typing contract)",
          "variants": ["PayloadContract.Untyped -> UntypedPayloadContract (\"untyped\")", "PayloadContract.Named(string) -> NamedPayloadContract (\"typed\", Type=name)", "PayloadContract.ForPayload(Type) -> Named(type.FullName)", "SameAs(other) structural compare", "DisplayName", "polymorphic via WriteOnlyPolymorphicConverter<PayloadContract>", "TS PayloadContract = UntypedPayloadContract | TypedPayloadContract", "carried by events/triggers/dispatch/server-push/SignalR"],
          "example": "PayloadContract.ForPayload(typeof(OrderDto))  // typed payload contract on a dispatch/trigger",
          "domainTypes": ["PayloadContract", "UntypedPayloadContract", "NamedPayloadContract", "PayloadTypeName"]
        },
        {
          "name": "CompareOp / CompareOperator (operator vocabulary value object)",
          "variants": ["CompareOp string constants surfaced as CompareOperator singletons: Eq/Neq, Gt/Gte/Lt/Lte, Truthy/Falsy/IsNull/NotNull/IsEmpty/NotEmpty, In/NotIn, Between, Contains/StartsWith/EndsWith, Matches, MinLength, ArrayContains", "categorized arrays the TS generator projects into literal unions: EqualityValues, OrderedValues, UnaryValues, MembershipValues, RangeValues, TextValues, RegexValues, TextLengthValues, CollectionItemValues", "CompareOperator.Values (all) + RequiresRightOperand (false for unary)", "generated TS CompareOp literal union plus per-category unions driving the CompareCondition subfamily"],
          "example": "LiteralUnion(\"CompareOp\", CompareOperator.Values)  // emits the TS operator union from the C# value object",
          "domainTypes": ["CompareOp", "CompareOperator"]
        }
      ]
    },
    {
      "key": "tag-helpers",
      "title": "Tag Helpers",
      "blurb": "Server-rendered <native-*> layout/presentation tag helpers from Alis.Reactive.NativeTagHelpers: design-system styling, no plan behavior.",
      "features": [
        {
          "name": "native-card (styled card surface)",
          "variants": ["elevation: Flat | Low (default) | Medium | High", "accent: optional AccentColor (Primary/Secondary/Success/Warning/Error/Info) left border", "class: extra CSS appended", "renders a <div> with design-system card classes"],
          "example": "<native-card elevation=\"Medium\" accent=\"Primary\"><native-card-header>Resident</native-card-header></native-card>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Card.NativeCardTagHelper", "CardElevation", "AccentColor"]
        },
        {
          "name": "native-card-header (card header section)",
          "variants": ["divider: None (default) | Header | Footer | Both (separating border edges)", "class: extra CSS appended", "ParentTag = native-card", "renders a <div> with header classes"],
          "example": "<native-card-header divider=\"Footer\">Care Plan</native-card-header>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Card.NativeCardHeaderTagHelper", "CardDivider"]
        },
        {
          "name": "native-card-body (card content section)",
          "variants": ["padding: None | Compact | Standard (default)", "class: extra CSS appended", "ParentTag = native-card", "renders a <div> with body classes"],
          "example": "<native-card-body padding=\"Compact\">Room 204</native-card-body>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Card.NativeCardBodyTagHelper", "CardPadding"]
        },
        {
          "name": "native-card-footer (card footer section)",
          "variants": ["divider: None (default) | Header | Footer | Both (separating border edges)", "class: extra CSS appended", "ParentTag = native-card", "renders a <div> with footer classes"],
          "example": "<native-card-footer divider=\"Header\">Updated today</native-card-footer>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Card.NativeCardFooterTagHelper", "CardDivider"]
        },
        {
          "name": "native-kv (key/value pair)",
          "variants": ["label: required (throws when empty)", "value: required (throws when empty)", "layout: Stacked (default, label above value) | Inline (label: value on one line)", "class: extra CSS appended", "renders a <dl> with <dt>/<dd>, HTML-encoded"],
          "example": "<native-kv label=\"Care Level\" value=\"Memory Care\" layout=\"Inline\" />",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Kv.NativeKvTagHelper", "KvLayout"]
        },
        {
          "name": "native-heading (styled heading + optional overline)",
          "variants": ["level: H1 | H2 (default) | H3 | H4 | H5 | H6 (selects tag + type scale)", "spacing: ElementSpacing None/Xs/Sm (default)/Base/Md/Lg bottom margin", "overline: optional small label rendered above", "class: extra CSS appended", "renders <h1>..<h6> with optional preceding <p> overline"],
          "example": "<native-heading level=\"H1\" overline=\"Resident\">Ada Lovelace</native-heading>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Heading.NativeHeadingTagHelper", "HeadingLevel", "ElementSpacing"]
        },
        {
          "name": "native-hstack (horizontal flex row)",
          "variants": ["gap: SpacingScale None/Xs/Sm/Base (default)/Md/Lg/Xl/Xxl/Max", "align: AlignItems Start/Center (default)/End/Stretch/Baseline (cross axis)", "justify: JustifyContent Start (default)/Center/End/Between/Around/Evenly", "wrap: bool (children wrap onto multiple lines)", "class: extra CSS appended", "renders a flex-row <div>"],
          "example": "<native-hstack gap=\"Md\" justify=\"Between\" align=\"Center\"> ... </native-hstack>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.HStack.NativeHStackTagHelper", "SpacingScale", "AlignItems", "JustifyContent"]
        },
        {
          "name": "native-vstack (vertical flex column)",
          "variants": ["gap: SpacingScale None/Xs/Sm/Base (default)/Md/Lg/Xl/Xxl/Max", "divide-y: bool (separating border between children)", "class: extra CSS appended", "renders a flex-column <div>"],
          "example": "<native-vstack gap=\"Sm\" divide-y=\"true\"> ... </native-vstack>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.VStack.NativeVStackTagHelper", "SpacingScale"]
        },
        {
          "name": "native-container (page-width centered container)",
          "variants": ["class: extra CSS appended", "renders a centered, max-width-capped <div> (no other attributes)"],
          "example": "<native-container> ... page content ... </native-container>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Container.NativeContainerTagHelper"]
        },
        {
          "name": "native-divider (horizontal rule, optional label)",
          "variants": ["style: Plain (default, solid rule) | Dashed", "label: optional centered label (renders a labeled section break instead of a plain <hr>)", "class: extra CSS appended", "renders <hr> (plain) or a labeled <div> structure"],
          "example": "<native-divider style=\"Dashed\" label=\"Medications\" />",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Divider.NativeDividerTagHelper", "DividerStyle"]
        },
        {
          "name": "native-text (styled paragraph or inline span)",
          "variants": ["size: TextSize Xs/Sm/Base (default)/Lg/Xl", "color: TextColor Primary (default)/Secondary/Muted/Inverse/Accent/Success/Warning/Error/Inherit", "bold: bool", "spacing: ElementSpacing None/Xs/Sm/Base (default)/Md/Lg bottom margin", "as-span: bool (inline <span> instead of block <p>)", "class: extra CSS appended"],
          "example": "<native-text color=\"Muted\" size=\"Sm\" as-span=\"true\">Last seen 2h ago</native-text>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Text.NativeTextTagHelper", "TextSize", "TextColor", "ElementSpacing"]
        },
        {
          "name": "native-grid (CSS grid of columns)",
          "variants": ["cols: GridCols C1/C2 (default)/C3/C4/C5/C6", "gap: SpacingScale None/Xs/Sm/Base/Md (default)/Lg/Xl/Xxl/Max", "responsive: bool (default true; column count scales down on smaller screens)", "class: extra CSS appended", "renders a CSS-grid <div>"],
          "example": "<native-grid cols=\"C3\" gap=\"Lg\" responsive=\"true\"> ... </native-grid>",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.Grid.NativeGridTagHelper", "GridCols", "SpacingScale"]
        },
        {
          "name": "native-validation-summary (hidden plan validation error container)",
          "variants": ["plan-id: required (throws when empty) - id of the plan whose validation errors this summary displays", "class: extra CSS appended", "renders a hidden <div data-reactive-validation-summary=\"{planId}\" id=\"{planId}_validation_summary\" hidden> (dots/plus in planId become underscores)"],
          "example": "<native-validation-summary plan-id=\"Alis.Reactive.Models.ResidentModel\" />",
          "domainTypes": ["Alis.Reactive.NativeTagHelpers.ValidationSummary.NativeValidationSummaryTagHelper"]
        }
      ]
    }
  ],
  "edges": [
    { "from": "plan-triggers", "to": "reactions", "label": "Action<PipelineBuilder> -> BuildReaction()" },
    { "from": "plan-triggers", "to": "values", "label": "typed payloads / event args as PayloadContract sources" },
    { "from": "plan-triggers", "to": "components", "label": ".Reactive -> ComponentEventOnboarding.Wire" },
    { "from": "plan-triggers", "to": "slots-plugins", "label": "ResolvePlan partial scope + RegisterPlugin" },
    { "from": "plan-triggers", "to": "http", "label": "ServerPush/SignalR RequestUrl -> pipelines begin HTTP" },
    { "from": "plan-triggers", "to": "validation", "label": "render binds ValidationJobs; root emits summary div" },
    { "from": "plan-triggers", "to": "conditions", "label": "trigger reactions branch via When/Then" },
    { "from": "plan-triggers", "to": "domain-contract", "label": "serialize PlanDocument; generate trigger union" },
    { "from": "reactions", "to": "values", "label": "every command carries a ValueExpression" },
    { "from": "reactions", "to": "conditions", "label": "When/Confirm/ElseIf/Else -> ConditionGraph" },
    { "from": "reactions", "to": "http", "label": "p.Get/Post flush sync segment -> RequestReaction" },
    { "from": "reactions", "to": "components", "label": "ComponentRef / ElementBuilder emit Set/Call" },
    { "from": "reactions", "to": "slots-plugins", "label": "p.Plugin(...).Fire / Into" },
    { "from": "reactions", "to": "validation", "label": "p.ValidationErrors(formId)" },
    { "from": "reactions", "to": "arrays", "label": "p.From/FromDom array transforms" },
    { "from": "reactions", "to": "domain-contract", "label": "per-reaction kind discriminator" },
    { "from": "conditions", "to": "values", "label": "operands resolve through ValueExpression + Shape" },
    { "from": "conditions", "to": "reactions", "label": "Then/ElseIf/Else -> nested ReactionGraph" },
    { "from": "conditions", "to": "http", "label": "ResponseBody.Read sources; Confirm gates requests" },
    { "from": "conditions", "to": "arrays", "label": "In/NotIn/Between/ArrayContains array operands" },
    { "from": "conditions", "to": "components", "label": "TypedComponentSource feeds When" },
    { "from": "conditions", "to": "slots-plugins", "label": "TypedPluginSource / property reads" },
    { "from": "conditions", "to": "validation", "label": "CompareOperator shared with WhenField" },
    { "from": "conditions", "to": "domain-contract", "label": "ConditionGraph subclass kinds" },
    { "from": "http", "to": "values", "label": "gather/header/route/response read ValueExpression" },
    { "from": "http", "to": "reactions", "label": "async ReactionGraph nodes; flush sync lanes" },
    { "from": "http", "to": "conditions", "label": "ResponseBody.Read -> TypedSource; OnError status routing" },
    { "from": "http", "to": "components", "label": "Gather.Include reads ComponentSource" },
    { "from": "http", "to": "validation", "label": "Validate gates request before send" },
    { "from": "http", "to": "arrays", "label": "scalar targets reject array/object shapes" },
    { "from": "http", "to": "slots-plugins", "label": "Gather.Plugin / response Inject into slots" },
    { "from": "http", "to": "domain-contract", "label": "request/response/parallel kind discriminators" },
    { "from": "arrays", "to": "values", "label": "every op compiles to ArrayOperationExpression" },
    { "from": "arrays", "to": "components", "label": "AsSource -> SetDataSource; From component members" },
    { "from": "arrays", "to": "reactions", "label": "ReactiveValue scalars feed reactions" },
    { "from": "arrays", "to": "conditions", "label": "per-element predicates -> ConditionGraph" },
    { "from": "arrays", "to": "http", "label": "From json.Read over response body" },
    { "from": "arrays", "to": "plan-triggers", "label": "From args event-payload arrays" },
    { "from": "arrays", "to": "slots-plugins", "label": "plugin reads + FromDom escape hatches" },
    { "from": "arrays", "to": "domain-contract", "label": "array-op kind + Shape" },
    { "from": "values", "to": "conditions", "label": "operands of compare/unary/in/between" },
    { "from": "values", "to": "http", "label": "gather targets + response reads" },
    { "from": "values", "to": "arrays", "label": "ReactiveValue / ReactiveArraySource" },
    { "from": "values", "to": "components", "label": "ComponentRef.Read -> TypedComponentSource" },
    { "from": "values", "to": "slots-plugins", "label": "PluginSource reads + method args" },
    { "from": "values", "to": "reactions", "label": "SetReaction / CallReaction / dispatch payload operands" },
    { "from": "values", "to": "plan-triggers", "label": "PayloadSource.Event via PayloadContract" },
    { "from": "values", "to": "validation", "label": "validation operands share the value path" },
    { "from": "values", "to": "domain-contract", "label": "ValueExpression/Source/Shape kind discriminators" },
    { "from": "components", "to": "plan-triggers", "label": ".Reactive registers component-event trigger" },
    { "from": "components", "to": "reactions", "label": "Set/Call reactions; SetText/AddClass" },
    { "from": "components", "to": "conditions", "label": "ComponentRef.Read -> condition guards" },
    { "from": "components", "to": "http", "label": "Include gather; SetDataSource from response" },
    { "from": "components", "to": "arrays", "label": "SetDataSource from AsSource" },
    { "from": "components", "to": "values", "label": "setters consume ValueExpression" },
    { "from": "components", "to": "validation", "label": "InputField registers binding; error span" },
    { "from": "components", "to": "slots-plugins", "label": "model-bound id is slot/SSR join key" },
    { "from": "components", "to": "domain-contract", "label": "ObjectPropertyContract / ObjectMethodContract" },
    { "from": "validation", "to": "components", "label": "binds to registered input components" },
    { "from": "validation", "to": "conditions", "label": "FieldCondition -> ConditionGraph" },
    { "from": "validation", "to": "values", "label": "peer/literal/range operands share ValueExpression" },
    { "from": "validation", "to": "http", "label": "Validate(formId) gates a request" },
    { "from": "validation", "to": "arrays", "label": "collection item-fields expand against array ids" },
    { "from": "validation", "to": "slots-plugins", "label": "DI behind IClientValidationRuleSource; slot unload" },
    { "from": "validation", "to": "domain-contract", "label": "ComponentValidation + ValidationRule unions" },
    { "from": "slots-plugins", "to": "values", "label": "plugin reads/calls + gather-plugin -> ValueExpression" },
    { "from": "slots-plugins", "to": "http", "label": "Into injects success body; Gather.Plugin" },
    { "from": "slots-plugins", "to": "reactions", "label": "plugin commands emit CallReaction; Into" },
    { "from": "slots-plugins", "to": "components", "label": "app-level objects via ComponentRef" },
    { "from": "slots-plugins", "to": "conditions", "label": "plugin reads are condition sources" },
    { "from": "slots-plugins", "to": "validation", "label": "partial scope drives validation summary" },
    { "from": "slots-plugins", "to": "plan-triggers", "label": "reactions inside trigger pipelines; ActionLink" },
    { "from": "slots-plugins", "to": "arrays", "label": "escape hatch for array/object manipulation" },
    { "from": "slots-plugins", "to": "domain-contract", "label": "PlanScope/PlanId composition keys; PluginContract" },
    { "from": "tag-helpers", "to": "components", "label": "native-* layout wraps Html.InputField content slots" },
    { "from": "tag-helpers", "to": "validation", "label": "native-validation-summary mirrors plan summary div by plan-id" },
    { "from": "tag-helpers", "to": "slots-plugins", "label": "native-validation-summary plan-id == PlanId composition key" }
  ],
  "flow": ["plan-triggers", "conditions", "validation", "http", "values", "arrays", "components", "tag-helpers", "domain-contract"]
};
