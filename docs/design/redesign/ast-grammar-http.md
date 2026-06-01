# http

DSL grammar (AST edges) for the HTTP request cluster, extracted from REAL public
builder signatures. Every row is a public method read directly from the `.cs`
source with a `file:line`. `Callback` names the callback parameter type if any —
a callback handing back a `PipelineBuilder<TModel>` is a **NESTING (recursion)
point**. `ReturnsSelf = yes` means the member returns its own receiver type and
is therefore **chainable / repeatable**.

Source root: `Alis.Reactive/Builders/`.

## Entry points (PipelineBuilder.Http — produce the cluster builders)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PipelineBuilder<TModel> | Get(string url) | HttpRequestBuilder<TModel> | - | no | Builders/PipelineBuilder.Http.cs:11 |
| PipelineBuilder<TModel> | Post(string url) | HttpRequestBuilder<TModel> | - | no | Builders/PipelineBuilder.Http.cs:19 |
| PipelineBuilder<TModel> | Post(string url, Action<GatherBuilder<TModel>> gather) | HttpRequestBuilder<TModel> | Action<GatherBuilder<TModel>> | no | Builders/PipelineBuilder.Http.cs:25 |
| PipelineBuilder<TModel> | Put(string url, Action<GatherBuilder<TModel>> gather) | HttpRequestBuilder<TModel> | Action<GatherBuilder<TModel>> | no | Builders/PipelineBuilder.Http.cs:31 |
| PipelineBuilder<TModel> | Delete(string url) | HttpRequestBuilder<TModel> | - | no | Builders/PipelineBuilder.Http.cs:39 |
| PipelineBuilder<TModel> | Parallel(params Action<HttpRequestBuilder<TModel>>[] branches) | ParallelBuilder<TModel> | Action<HttpRequestBuilder<TModel>>[] | no | Builders/PipelineBuilder.Http.cs:45 |

## HttpRequestBuilder<TModel>

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| HttpRequestBuilder<TModel> | Get(string url) | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:34 |
| HttpRequestBuilder<TModel> | Post(string url) | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:38 |
| HttpRequestBuilder<TModel> | Put(string url) | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:42 |
| HttpRequestBuilder<TModel> | Delete(string url) | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:46 |
| HttpRequestBuilder<TModel> | Gather(Action<GatherBuilder<TModel>> gather) | HttpRequestBuilder<TModel> | Action<GatherBuilder<TModel>> | yes | Builders/Requests/HttpRequestBuilder.cs:51 |
| HttpRequestBuilder<TModel> | AsJson() | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:62 |
| HttpRequestBuilder<TModel> | AsFormData() | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:65 |
| HttpRequestBuilder<TModel> | WhileLoading(Action<PipelineBuilder<TModel>> pipeline) | HttpRequestBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/HttpRequestBuilder.cs:70 |
| HttpRequestBuilder<TModel> | Finally(Action<PipelineBuilder<TModel>> pipeline) | HttpRequestBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/HttpRequestBuilder.cs:89 |
| HttpRequestBuilder<TModel> | Validate<TValidationSource>(string formId) | HttpRequestBuilder<TModel> | - | yes | Builders/Requests/HttpRequestBuilder.cs:103 |
| HttpRequestBuilder<TModel> | Response(Action<ResponseBuilder<TModel>> response) | HttpRequestBuilder<TModel> | Action<ResponseBuilder<TModel>> | yes | Builders/Requests/HttpRequestBuilder.cs:115 |

Note: the task's `OnSettled` / `Chained` / `Header` / `RouteParam` members live
on sibling builders in this cluster, not on `HttpRequestBuilder` — `Finally` is
the after-settle hook here (`OnAllSettled` is `ParallelBuilder`'s settle hook);
`Chained` is on `ResponseBuilder`; `Header` / `RouteParam` are on `GatherBuilder`.

## GatherBuilder<TModel>

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| GatherBuilder<TModel> | IncludeAll() | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:28 |
| GatherBuilder<TModel> | Static(string param, object value) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:38 |
| GatherBuilder<TModel> | FromEvent<TArgs,TProp>(TArgs args, Expression<Func<TArgs,TProp>> path, string param) | GatherBuilder<TModel> | Expression<Func<TArgs,TProp>> | yes | Builders/Requests/GatherBuilder.cs:54 |
| GatherBuilder<TModel> | Header(string name, string value) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:69 |
| GatherBuilder<TModel> | Header<TProp>(string name, TypedSource<TProp> source) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:81 |
| GatherBuilder<TModel> | Header<TArgs,TProp>(string name, TArgs args, Expression<Func<TArgs,TProp>> path) | GatherBuilder<TModel> | Expression<Func<TArgs,TProp>> | yes | Builders/Requests/GatherBuilder.cs:91 |
| GatherBuilder<TModel> | RouteParam(string paramName, int value) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:103 |
| GatherBuilder<TModel> | RouteParam(string paramName, string value) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:111 |
| GatherBuilder<TModel> | RouteParam(string paramName, long value) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:123 |
| GatherBuilder<TModel> | RouteParam<TProp>(string paramName, TypedSource<TProp> source) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:131 |
| GatherBuilder<TModel> | RouteParam<TArgs,TProp>(string paramName, TArgs args, Expression<Func<TArgs,TProp>> path) | GatherBuilder<TModel> | Expression<Func<TArgs,TProp>> | yes | Builders/Requests/GatherBuilder.cs:141 |
| GatherBuilder<TModel> | FromUrl(string paramName) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:157 |
| GatherBuilder<TModel> | FromUrl(string paramName, string asParam) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:169 |
| GatherBuilder<TModel> | FromUrl<T>(string paramName) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:181 |
| GatherBuilder<TModel> | FromUrl<T>(string paramName, string asParam) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:194 |
| GatherBuilder<TModel> | Plugin<T>(TypedPluginSource<T> source, string paramName) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherBuilder.cs:207 |

### GatherExtensions (static — extension methods on GatherBuilder<TModel>)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| GatherBuilder<TModel> | Include<TComponent,TModel>(Expression<Func<TModel,object>> expr) | GatherBuilder<TModel> | Expression<Func<TModel,object>> | yes | Builders/Requests/GatherExtensions.cs:17 |
| GatherBuilder<TModel> | Include<TComponent,TModel>(string refId, string name) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherExtensions.cs:36 |
| GatherBuilder<TModel> | Include<TModel,TProp>(TypedComponentSource<TProp> source) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherExtensions.cs:58 |
| GatherBuilder<TModel> | Include<TModel,TProp>(TypedComponentSource<TProp> source, string paramName) | GatherBuilder<TModel> | - | yes | Builders/Requests/GatherExtensions.cs:71 |

## ResponseBuilder<TModel>

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ResponseBuilder<TModel> | OnSuccess(Action<PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:28 |
| ResponseBuilder<TModel> | OnSuccess<TResponse>(Action<ResponseBody<TResponse>,PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<ResponseBody<TResponse>,PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:40 |
| ResponseBuilder<TModel> | OnError(Action<PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:55 |
| ResponseBuilder<TModel> | OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:67 |
| ResponseBuilder<TModel> | OnError<TError>(Action<ResponseBody<TError>,PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<ResponseBody<TError>,PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:79 |
| ResponseBuilder<TModel> | OnError<TError>(int statusCode, Action<ResponseBody<TError>,PipelineBuilder<TModel>> pipeline) | ResponseBuilder<TModel> | Action<ResponseBody<TError>,PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ResponseBuilder.cs:96 |
| ResponseBuilder<TModel> | Chained(Action<HttpRequestBuilder<TModel>> request) | ResponseBuilder<TModel> | Action<HttpRequestBuilder<TModel>> (NESTING — recurses into HttpRequestBuilder) | yes | Builders/Requests/ResponseBuilder.cs:111 |

## ParallelBuilder<TModel>

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ParallelBuilder<TModel> | OnAllSettled(Action<PipelineBuilder<TModel>> pipeline) | ParallelBuilder<TModel> | Action<PipelineBuilder<TModel>> (NESTING) | yes | Builders/Requests/ParallelBuilder.cs:28 |

> `ParallelBuilder.AddBranch(Action<HttpRequestBuilder<TModel>>)` is `internal`
> (ParallelBuilder.cs:18) — branches are supplied through the public
> `PipelineBuilder.Parallel(params Action<HttpRequestBuilder<TModel>>[])` entry
> point above, each of which recurses into `HttpRequestBuilder`.
