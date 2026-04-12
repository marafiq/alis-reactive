# Implementation Order — All 4 Value Source Features

## Dependency Graph

```
Task 0: Extract formatForWire to Scripts/core/wire-format.ts
    │
    ├──► Plan 1: HTTP Headers (Request.headers + GatherBuilder.Header())
    │       └──► builds on formatForWire extraction
    │       └──► changes buildFetch signature to (req, gatherResult, plan, ctx)
    │
    ├──► Plan 2: URL Templates (Request.routeParams + GatherBuilder.RouteParam())
    │       └──► builds on formatForWire extraction
    │       └──► builds on buildFetch signature change (from Plan 1)
    │
    └──► Plan 3: URL Query Source (UrlSource + PipelineBuilder.FromUrl())
            └──► WIDENS Source union (2→3 kinds)
            └──► NO formatForWire needed (URL params are strings)
            │
            └──► Plan 4: Plugin Source (PluginSource + PipelineBuilder.Plugin())
                    └──► WIDENS Source union again (3→4 kinds)
                    └──► Uses formatForWire for date plugins
                    └──► Introduces JsType for non-component sources
```

## Recommended Order

| # | Feature | Source Change | Complexity | Depends On |
|---|---------|-------------|-----------|-----------|
| 0 | formatForWire extraction | none | trivial | — |
| 1 | HTTP Headers | none | low | Task 0 |
| 2 | URL Templates | none | low | Task 0, Plan 1 (buildFetch) |
| 3 | URL Query Source | Source union 2→3 | medium | — |
| 4 | Plugin Source | Source union 3→4 | medium-high | Plan 3 (validates pattern) |

Plans 1+2 can be done in parallel (both add to Request, neither changes Source).
Plan 3 should land before Plan 4 (simpler, validates the union widening).

## Layer Crossings Per Plan

| Plan | C# Model | C# Builder | Schema | TS Types | TS Runtime | Tests |
|------|----------|-----------|--------|----------|-----------|-------|
| Headers | Request.cs | GatherBuilder, HttpRequestBuilder | Request def | plan.ts Request | http.ts | unit + Playwright |
| URL Templates | Request.cs | GatherBuilder, HttpRequestBuilder | Request def | plan.ts Request | http.ts | unit + Playwright |
| URL Query | Source.cs | PipelineBuilder, GatherBuilder | Source union | plan.ts Source | resolver.ts, evaluate.ts | unit + Playwright |
| Plugin | Source.cs, PlanBuildContext | PipelineBuilder, GatherBuilder, PluginTypeBuilder | Source union | plan.ts Source | resolver.ts, evaluate.ts, plugin-registry.ts | unit + Playwright |

## Files Touched (Cumulative)

### C# Files
- `Alis.Reactive/PlanModel/Source.cs` — +UrlSource, +PluginSource
- `Alis.Reactive/PlanModel/ValueProducer.cs` — +ReadUrl, +ReadPlugin
- `Alis.Reactive/PlanModel/Request.cs` — +Headers, +RouteParams
- `Alis.Reactive/PlanModel/PlanBuildContext.cs` — +EnsurePluginType, +EnsurePluginProperty, +EnsurePluginMethod
- `Alis.Reactive/Builders/Requests/GatherBuilder.cs` �� +HeaderFields, +RouteParamFields, +Header(), +RouteParam(), +FromUrl(), +Plugin()
- `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` — wire headers + route params
- `Alis.Reactive/Builders/PipelineBuilder.cs` ��� +FromUrl(), +Plugin()
- `Alis.Reactive/Builders/Conditions/TypedUrlSource.cs` — NEW
- `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs` — NEW
- `Alis.Reactive/Builders/PluginTypeBuilder.cs` — NEW
- `Alis.Reactive/ReactivePlan.cs` — +RegisterPlugin()

### Schema
- `Alis.Reactive/Schemas/reactive-plan.schema.json` — +headers/routeParams on Request, +UrlSource, +PluginSource in Source union

### TS Files
- `Scripts/types/plan.ts` — +headers/routeParams on Request, +UrlSource, +PluginSource in Source, +PluginSource interface
- `Scripts/core/wire-format.ts` — NEW (extracted from gather.ts)
- `Scripts/core/plugin-registry.ts` — NEW
- `Scripts/core/evaluate.ts` — +url branch, +plugin branch
- `Scripts/execution/gather.ts` — import formatForWire from core/wire-format
- `Scripts/execution/http.ts` — +header evaluation, +route param resolution, +plan/ctx params
- `Scripts/resolution/resolver.ts` — +url case, +plugin case, update getJsTypeForSource
- `Scripts/root.ts` — expose registerPlugin
