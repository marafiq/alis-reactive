# plugins-applevel-template

Grounded DSL grammar (AST edges) for the **Plugins**, **App-level objects**, and
**Fusion Template** clusters. Every row is a REAL public method read from the
actual `.cs` builder source, recorded as `Receiver -> Member(params) -> Returns`
with a `file:line`. Derived from signatures, not sandbox usage.

- **Callback** = the callback param type if any (`Action<...>`, `Func<...>`) else `-`.
  A callback handing back a builder is a **NESTING (recursion)** point.
- **ReturnsSelf** = `yes` if the member returns its own receiver type
  (chainable / repeatable — e.g. multiple `.Arg`, multiple template children).
- **Source** = path relative to repo root, with line number.

Paths are relative to `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive`.

---

## PLUGINS

### Cluster entry edges (where the plugin grammar is reached)

These are the methods on `ReactivePlan` / `PipelineBuilder` that hand back a
plugin builder. They are the grammar's connective tissue into the plugin cluster.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ReactivePlan | RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure) | void | Action<PluginTypeBuilder> (NESTING) | no | Alis.Reactive/ReactivePlan.cs:53 |
| ReactivePlan | RegisterPlugin(Plugin plugin) | void | - | no | Alis.Reactive/ReactivePlan.cs:65 |
| ReactivePlan | RegisterPlugin<TPlugin>() | TPlugin | - | no | Alis.Reactive/ReactivePlan.cs:72 |
| PipelineBuilder<TModel> | Plugin<T>(string pluginName, string member) | PluginMemberBuilder<T, TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:164 |
| PipelineBuilder<TModel> | Plugin<T>(string pluginName) | PluginMemberBuilder<T, TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:179 |
| PipelineBuilder<TModel> | PluginProperty<T>(string pluginName, string member) | TypedPluginPropertySource<T> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:194 |
| PipelineBuilder<TModel> | Plugin<T>(PluginFunction<T> function) | PluginMemberBuilder<T, TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:206 |
| PipelineBuilder<TModel> | Plugin<T>(PluginProperty<T> property) | TypedPluginPropertySource<T> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:215 |
| PipelineBuilder<TModel> | Plugin(string pluginName, string member) | PluginCallBuilder<TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:226 |
| PipelineBuilder<TModel> | Plugin(string pluginName) | PluginCallBuilder<TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:240 |
| PipelineBuilder<TModel> | Plugin(PluginCommand command) | PluginCallBuilder<TModel> | - | no | Alis.Reactive/Builders/PipelineBuilder.cs:253 |

### PluginTypeBuilder

Inline plugin declaration face — `RegisterPlugin("name", p => ...)`. `Command` is
an alias for `Void`. Every member returns the builder (chainable / repeatable).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginTypeBuilder | Method<T>(string name) | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:24 |
| PluginTypeBuilder | Property<T>(string name) | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:30 |
| PluginTypeBuilder | Method<TReturn>(string name, Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:40 |
| PluginTypeBuilder | Function<T>() | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:48 |
| PluginTypeBuilder | Function<TReturn>(Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:54 |
| PluginTypeBuilder | Void(string name) | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:62 |
| PluginTypeBuilder | Command(string name) | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:68 |
| PluginTypeBuilder | Void(string name, Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:72 |
| PluginTypeBuilder | Command(string name, Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:80 |
| PluginTypeBuilder | Void() | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:84 |
| PluginTypeBuilder | Command() | PluginTypeBuilder | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:90 |
| PluginTypeBuilder | Void(Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:94 |
| PluginTypeBuilder | Command(Action<PluginArgumentTypes> arguments) | PluginTypeBuilder | Action<PluginArgumentTypes> | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:102 |

### PluginArgumentTypes

Exact-argument-contract builder passed to the `Action<PluginArgumentTypes>`
callbacks above. One method, repeatable.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginArgumentTypes | Arg<T>() | PluginArgumentTypes | - | yes | Alis.Reactive/Builders/PluginTypeBuilder.cs:164 |

### Plugin (abstract subclass declaration face)

`protected` members used when subclassing `Plugin`. Authoring surface for typed
plugin classes registered via `RegisterPlugin<TPlugin>()`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| Plugin | Name (get) | string | - | no | Alis.Reactive/Plugin.cs:27 |
| Plugin | Function<TReturn>(string member) | PluginFunction<TReturn> | - | no | Alis.Reactive/Plugin.cs:30 |
| Plugin | Function<TReturn>(string member, Action<PluginArgumentTypes> arguments) | PluginFunction<TReturn> | Action<PluginArgumentTypes> | no | Alis.Reactive/Plugin.cs:38 |
| Plugin | Function<TReturn>() | PluginFunction<TReturn> | - | no | Alis.Reactive/Plugin.cs:44 |
| Plugin | Function<TReturn>(Action<PluginArgumentTypes> arguments) | PluginFunction<TReturn> | Action<PluginArgumentTypes> | no | Alis.Reactive/Plugin.cs:52 |
| Plugin | Property<TValue>(string member) | PluginProperty<TValue> | - | no | Alis.Reactive/Plugin.cs:56 |
| Plugin | Command(string member) | PluginCommand | - | no | Alis.Reactive/Plugin.cs:64 |
| Plugin | Command(string member, Action<PluginArgumentTypes> arguments) | PluginCommand | Action<PluginArgumentTypes> | no | Alis.Reactive/Plugin.cs:72 |
| Plugin | Command() | PluginCommand | - | no | Alis.Reactive/Plugin.cs:78 |
| Plugin | Command(Action<PluginArgumentTypes> arguments) | PluginCommand | Action<PluginArgumentTypes> | no | Alis.Reactive/Plugin.cs:86 |

### PluginOperation (base descriptor)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginOperation | PluginName (get) | string | - | no | Alis.Reactive/Plugin.cs:178 |
| PluginOperation | Member (get) | string | - | no | Alis.Reactive/Plugin.cs:181 |

### PluginProperty<TValue> (descriptor)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginProperty<TValue> | PluginName (get) | string | - | no | Alis.Reactive/Plugin.cs:228 |
| PluginProperty<TValue> | Member (get) | string | - | no | Alis.Reactive/Plugin.cs:231 |

### PluginFunction<TReturn> (descriptor — argument contract face)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginFunction<TReturn> | Arg<TArg>() | PluginFunction<TReturn> | - | yes | Alis.Reactive/Plugin.cs:259 |
| PluginFunction<TReturn> | Args(Action<PluginArgumentTypes> arguments) | PluginFunction<TReturn> | Action<PluginArgumentTypes> | yes | Alis.Reactive/Plugin.cs:266 |

### PluginCommand (descriptor — argument contract face)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginCommand | Arg<TArg>() | PluginCommand | - | yes | Alis.Reactive/Plugin.cs:290 |
| PluginCommand | Args(Action<PluginArgumentTypes> arguments) | PluginCommand | Action<PluginArgumentTypes> | yes | Alis.Reactive/Plugin.cs:297 |

### PluginMemberBuilder<TReturn, TModel> (the read / "PluginReadBuilder" face)

Read terminal: implicitly converts to `TypedPluginSource<TReturn>` — the source
IS the builder, so there is no explicit `Build()`. Every `Arg` is repeatable.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginMemberBuilder<TReturn, TModel> | Arg<TResponse, TProp>(ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path) | PluginMemberBuilder<TReturn, TModel> | Expression<Func<TResponse,TProp>> | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:55 |
| PluginMemberBuilder<TReturn, TModel> | Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path) | PluginMemberBuilder<TReturn, TModel> | Expression<Func<TArgs,TProp>> | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:64 |
| PluginMemberBuilder<TReturn, TModel> | Arg<TArg>(TypedSource<TArg> source) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:72 |
| PluginMemberBuilder<TReturn, TModel> | Arg(string value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:79 |
| PluginMemberBuilder<TReturn, TModel> | Arg(int value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:87 |
| PluginMemberBuilder<TReturn, TModel> | Arg(bool value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:94 |
| PluginMemberBuilder<TReturn, TModel> | Arg(long value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:101 |
| PluginMemberBuilder<TReturn, TModel> | Arg(decimal value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:108 |
| PluginMemberBuilder<TReturn, TModel> | Arg(double value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:115 |
| PluginMemberBuilder<TReturn, TModel> | Arg(DateTime value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:122 |
| PluginMemberBuilder<TReturn, TModel> | ArgValue<TValue>(TValue value) | PluginMemberBuilder<TReturn, TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:129 |
| PluginMemberBuilder<TReturn, TModel> | (implicit operator) TypedPluginSource<TReturn>(PluginMemberBuilder<TReturn, TModel> b) | TypedPluginSource<TReturn> | - | no | Alis.Reactive/Builders/PluginMemberBuilder.cs:136 |

### PluginCallBuilder<TModel> (the call / command face)

Same `Arg` surface as the read face. `Fire()` is the terminal that emits the
CallReaction into the pipeline.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PluginCallBuilder<TModel> | Arg<TResponse, TProp>(ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path) | PluginCallBuilder<TModel> | Expression<Func<TResponse,TProp>> | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:169 |
| PluginCallBuilder<TModel> | Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path) | PluginCallBuilder<TModel> | Expression<Func<TArgs,TProp>> | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:178 |
| PluginCallBuilder<TModel> | Arg<TArg>(TypedSource<TArg> source) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:186 |
| PluginCallBuilder<TModel> | Arg(string value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:193 |
| PluginCallBuilder<TModel> | Arg(int value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:201 |
| PluginCallBuilder<TModel> | Arg(bool value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:208 |
| PluginCallBuilder<TModel> | Arg(long value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:215 |
| PluginCallBuilder<TModel> | Arg(decimal value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:222 |
| PluginCallBuilder<TModel> | Arg(double value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:229 |
| PluginCallBuilder<TModel> | Arg(DateTime value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:236 |
| PluginCallBuilder<TModel> | ArgValue<TValue>(TValue value) | PluginCallBuilder<TModel> | - | yes | Alis.Reactive/Builders/PluginMemberBuilder.cs:243 |
| PluginCallBuilder<TModel> | Fire() | void | - | no (terminal) | Alis.Reactive/Builders/PluginMemberBuilder.cs:250 |

---

## APP-LEVEL OBJECTS

App-level verbs are **extension methods** on `ComponentRef<TComponent, TModel>`.
Each verb returns the same `ComponentRef<TComponent, TModel>`, so verbs are
chainable / repeatable (ReturnsSelf = yes). The component classes
(`NativeDrawer`, `NativeLoader`, `FusionToast`, `FusionConfirm`) carry only a
fixed `ElementId` const and `DefaultId` property — they hold no DSL verbs.

### NativeDrawer (component) — fixed identity

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| NativeDrawer | ElementId (const) | string | - | no | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawer.cs:20 |
| NativeDrawer | DefaultId (get) | string | - | no | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawer.cs:23 |

### NativeDrawerExtensions — drawer verbs

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ComponentRef<NativeDrawer, TModel> | SetSize<TModel>(DrawerSize size) | ComponentRef<NativeDrawer, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawerExtensions.cs:36 |
| ComponentRef<NativeDrawer, TModel> | Open<TModel>() | ComponentRef<NativeDrawer, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawerExtensions.cs:62 |
| ComponentRef<NativeDrawer, TModel> | Close<TModel>() | ComponentRef<NativeDrawer, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawerExtensions.cs:78 |
| IHtmlHelper | NativeDrawer(this IHtmlHelper html) | IHtmlContent | - | no | Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawerExtensions.cs:100 |

### NativeLoader (component) — fixed identity

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| NativeLoader | ElementId (const) | string | - | no | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoader.cs:18 |
| NativeLoader | DefaultId (get) | string | - | no | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoader.cs:21 |

### NativeLoaderExtensions — loader verbs

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ComponentRef<NativeLoader, TModel> | SetTarget<TModel>(string targetId) | ComponentRef<NativeLoader, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoaderExtensions.cs:41 |
| ComponentRef<NativeLoader, TModel> | SetTimeout<TModel>(int ms) | ComponentRef<NativeLoader, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoaderExtensions.cs:54 |
| ComponentRef<NativeLoader, TModel> | Show<TModel>() | ComponentRef<NativeLoader, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoaderExtensions.cs:65 |
| ComponentRef<NativeLoader, TModel> | Hide<TModel>() | ComponentRef<NativeLoader, TModel> | - | yes | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoaderExtensions.cs:81 |
| IHtmlHelper | NativeLoader(this IHtmlHelper html) | IHtmlContent | - | no | Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoaderExtensions.cs:105 |

### FusionToast (component) — fixed identity

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionToast | ElementId (const) | string | - | no | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:13 |
| FusionToast | DefaultId (get) | string | - | no | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:14 |

### FusionToastExtensions — toast verbs

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ComponentRef<FusionToast, TModel> | SetTitle<TModel>(string title) | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:41 |
| ComponentRef<FusionToast, TModel> | SetContent<TModel>(string content) | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:46 |
| ComponentRef<FusionToast, TModel> | SetTimeout<TModel>(int ms) | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:51 |
| ComponentRef<FusionToast, TModel> | ShowCloseButton<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:56 |
| ComponentRef<FusionToast, TModel> | ShowProgressBar<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:61 |
| ComponentRef<FusionToast, TModel> | Success<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:68 |
| ComponentRef<FusionToast, TModel> | Warning<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:73 |
| ComponentRef<FusionToast, TModel> | Danger<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:78 |
| ComponentRef<FusionToast, TModel> | Info<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:83 |
| ComponentRef<FusionToast, TModel> | Show<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:90 |
| ComponentRef<FusionToast, TModel> | Hide<TModel>() | ComponentRef<FusionToast, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:96 |
| IHtmlHelper | FusionToast(this IHtmlHelper html) | IHtmlContent | - | no | Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:103 |

### FusionConfirm (component) — fixed identity

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionConfirm | ElementId (const) | string | - | no | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirm.cs:12 |
| FusionConfirm | DefaultId (get) | string | - | no | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirm.cs:14 |

### FusionConfirmExtensions — confirm verbs

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ComponentRef<FusionConfirm, TModel> | SetContent<TModel>(string message) | ComponentRef<FusionConfirm, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirmExtensions.cs:21 |
| ComponentRef<FusionConfirm, TModel> | Show<TModel>() | ComponentRef<FusionConfirm, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirmExtensions.cs:29 |
| ComponentRef<FusionConfirm, TModel> | Hide<TModel>() | ComponentRef<FusionConfirm, TModel> | - | yes | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirmExtensions.cs:34 |
| IHtmlHelper | FusionConfirmDialog(this IHtmlHelper html) | IHtmlContent | - | no | Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirmExtensions.cs:39 |

---

## FUSION TEMPLATE

`FusionTemplate.Create<TModel>()` returns a `FusionTemplateBuilder<TModel>`, the
root div builder. Every child method returns the builder (chainable / repeatable),
so a template is an ordered sequence of child edges. `Div`, `When`, and `ShowIf`
take callbacks handing back a builder — those are **NESTING (recursion)** points.

> Source-vs-task note: the actual public API differs from the task's hypothesised
> names. Real members are `Class` (not `CssClass`), `Attr` (not `Attribute`),
> `EventButton` (the dispatch-button — not `DispatchButton`), `When`/`ShowIf`
> (not `WhenTemplate`/`ShowTemplateIf`). There is no `Render(TModel)`, no
> `Badge`/`Icon`/`Button` named `Render`, no plain `Render(...)` content node —
> `Render()` is the string-terminal that emits HTML. The conditional callback
> receiver is `FusionConditionalBuilder<TModel>` (a separate, narrower builder).

### FusionTemplate (static factory)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionTemplate | Create<TModel>() | FusionTemplateBuilder<TModel> | - | no | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:16 |

### FusionTemplateBuilder<TModel> (root div builder)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionTemplateBuilder<TModel> | Id(string id) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:34 |
| FusionTemplateBuilder<TModel> | Class(string cssClass) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:43 |
| FusionTemplateBuilder<TModel> | Attr(string name, string value) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:52 |
| FusionTemplateBuilder<TModel> | Text<TProperty>(Expression<Func<TModel, TProperty>> property) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:61 |
| FusionTemplateBuilder<TModel> | Text(string text) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:71 |
| FusionTemplateBuilder<TModel> | Span<TProperty>(Expression<Func<TModel, TProperty>> property) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:80 |
| FusionTemplateBuilder<TModel> | Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:86 |
| FusionTemplateBuilder<TModel> | Span(string text) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:98 |
| FusionTemplateBuilder<TModel> | Span(string text, string css) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:104 |
| FusionTemplateBuilder<TModel> | Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:116 |
| FusionTemplateBuilder<TModel> | Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:122 |
| FusionTemplateBuilder<TModel> | Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css, string alt) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:128 |
| FusionTemplateBuilder<TModel> | Div(Action<FusionTemplateBuilder<TModel>> configure) | FusionTemplateBuilder<TModel> | Action<FusionTemplateBuilder<TModel>> (NESTING) | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:143 |
| FusionTemplateBuilder<TModel> | Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge") | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:154 |
| FusionTemplateBuilder<TModel> | Badge(string text, string css = "e-badge") | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:163 |
| FusionTemplateBuilder<TModel> | Icon(string iconName) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:172 |
| FusionTemplateBuilder<TModel> | Icon(string iconName, string css) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:178 |
| FusionTemplateBuilder<TModel> | Button(string text, string onClick) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:192 |
| FusionTemplateBuilder<TModel> | Button(string text, string onClick, string css) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:198 |
| FusionTemplateBuilder<TModel> | ButtonFor<TProperty>(string text, Expression<Func<TModel, TProperty>> idProperty, string onClickFn) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:210 |
| FusionTemplateBuilder<TModel> | ButtonFor<TProperty>(string text, Expression<Func<TModel, TProperty>> idProperty, string onClickFn, string css) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:219 |
| FusionTemplateBuilder<TModel> | EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:245 |
| FusionTemplateBuilder<TModel> | EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty, string css) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:254 |
| FusionTemplateBuilder<TModel> | Link<THref, TText>(Expression<Func<TModel, THref>> hrefProperty, Expression<Func<TModel, TText>> textProperty) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,THref>>, Expression<Func<TModel,TText>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:274 |
| FusionTemplateBuilder<TModel> | Link<THref, TText>(Expression<Func<TModel, THref>> hrefProperty, Expression<Func<TModel, TText>> textProperty, string css) | FusionTemplateBuilder<TModel> | Expression<Func<TModel,THref>>, Expression<Func<TModel,TText>> | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:282 |
| FusionTemplateBuilder<TModel> | When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then) | FusionTemplateBuilder<TModel> | Action<FusionConditionalBuilder<TModel>> (NESTING) | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:303 |
| FusionTemplateBuilder<TModel> | When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then, Action<FusionConditionalBuilder<TModel>> @else) | FusionTemplateBuilder<TModel> | Action<FusionConditionalBuilder<TModel>> x2 (NESTING) | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:311 |
| FusionTemplateBuilder<TModel> | ShowIf(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> content) | FusionTemplateBuilder<TModel> | Action<FusionConditionalBuilder<TModel>> (NESTING) | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:343 |
| FusionTemplateBuilder<TModel> | Raw(string html) | FusionTemplateBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:352 |
| FusionTemplateBuilder<TModel> | Render() | string | - | no (terminal) | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:361 |
| FusionTemplateBuilder<TModel> | ToString() | string | - | no (terminal) | Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:383 |

### FusionConditionalBuilder<TModel> (When / ShowIf callback receiver)

Narrower child builder used inside `When` / `ShowIf` branches. Same chainable
child-edge pattern; `Div` nests back to a full `FusionTemplateBuilder<TModel>`
(NESTING). `Render()` is the string terminal.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionConditionalBuilder<TModel> | Span<TProperty>(Expression<Func<TModel, TProperty>> property) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:18 |
| FusionConditionalBuilder<TModel> | Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:24 |
| FusionConditionalBuilder<TModel> | Span(string text) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:36 |
| FusionConditionalBuilder<TModel> | Span(string text, string css) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:42 |
| FusionConditionalBuilder<TModel> | Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge") | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:54 |
| FusionConditionalBuilder<TModel> | Badge(string text, string css = "e-badge") | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:63 |
| FusionConditionalBuilder<TModel> | Icon(string iconName) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:72 |
| FusionConditionalBuilder<TModel> | Icon(string iconName, string css) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:78 |
| FusionConditionalBuilder<TModel> | Div(Action<FusionTemplateBuilder<TModel>> configure) | FusionConditionalBuilder<TModel> | Action<FusionTemplateBuilder<TModel>> (NESTING) | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:90 |
| FusionConditionalBuilder<TModel> | Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:101 |
| FusionConditionalBuilder<TModel> | Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:107 |
| FusionConditionalBuilder<TModel> | Button(string text, string onClick) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:124 |
| FusionConditionalBuilder<TModel> | Button(string text, string onClick, string css) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:130 |
| FusionConditionalBuilder<TModel> | EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:144 |
| FusionConditionalBuilder<TModel> | EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty, string css) | FusionConditionalBuilder<TModel> | Expression<Func<TModel,TProperty>> | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:153 |
| FusionConditionalBuilder<TModel> | Raw(string html) | FusionConditionalBuilder<TModel> | - | yes | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:174 |
| FusionConditionalBuilder<TModel> | Render() | string | - | no (terminal) | Alis.Reactive.Fusion/Templates/FusionConditionalBuilder.cs:183 |

### FusionTemplateExpression (static expression → SF-binding helpers)

Public static helpers the template builders use to lower expressions to
Syncfusion template syntax. Part of the template grammar surface (callable
directly when authoring SF column/template strings).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| FusionTemplateExpression | ToBinding<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression) | string | Expression<Func<TModel,TProperty>> | no | Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:19 |
| FusionTemplateExpression | ToPropertyPath<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression) | string | Expression<Func<TModel,TProperty>> | no | Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:28 |
| FusionTemplateExpression | ToCondition<TModel>(Expression<Func<TModel, bool>> predicate) | string | Expression<Func<TModel,bool>> | no | Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:36 |

---

## Recursion / chaining summary

- **NESTING points** (callback hands back a builder → recursion in the grammar):
  - `FusionTemplateBuilder.Div(Action<FusionTemplateBuilder<TModel>>)`
  - `FusionTemplateBuilder.When(...)` / `When(...,@else)` / `ShowIf(...)`
    — each callback receives a `FusionConditionalBuilder<TModel>`.
  - `FusionConditionalBuilder.Div(Action<FusionTemplateBuilder<TModel>>)`
    — nests back to the full root builder.
  - `ReactivePlan.RegisterPlugin(string, Action<PluginTypeBuilder>)`
    — callback receives the plugin declaration builder.
  - `PluginTypeBuilder` / `Plugin` `Method`/`Function`/`Command`/`Void`/`Args`
    overloads taking `Action<PluginArgumentTypes>` — callback receives the arg
    contract builder.
- **ReturnsSelf = yes** members are chainable / repeatable: all `PluginTypeBuilder`
  declarations, all `Plugin*` `Arg`/`ArgValue`, all app-level `ComponentRef`
  verbs, and every `FusionTemplateBuilder` / `FusionConditionalBuilder` child edge.
- **Terminals**: `PluginCallBuilder.Fire()` (emits CallReaction);
  `PluginMemberBuilder` implicit conversion to `TypedPluginSource<TReturn>`;
  `FusionTemplateBuilder.Render()` / `ToString()` and
  `FusionConditionalBuilder.Render()` (emit HTML string).
