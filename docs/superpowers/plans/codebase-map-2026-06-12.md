# Codebase Map — 2026-06-12

Read from the tree on `tiny-safe-but-important-refactorings` at `1e7a0be3`,
not from memory. One line per piece; the architecture model itself lives in
root `CLAUDE.md`.

## Projects

| Project | What ships |
|---|---|
| `Alis.Reactive` | The core: C# DSL, plan domain, serialization, contract generation |
| `Alis.Reactive.Fusion` | Syncfusion component vertical slices (one folder per component) |
| `Alis.Reactive.Native` | Native HTML component slices |
| `Alis.Reactive.NativeTagHelpers` | net10-only tag helpers (`<native-hstack>` etc.) |
| `Alis.Reactive.FluentValidator` | `ReactiveValidator<T>` + `ClientRule(...)` — one call records server rule + client metadata |
| `Alis.Reactive.DesignSystem` | Design-system CSS |
| `Alis.Reactive.Assets` | TS runtime source + esbuild/vite bundles (`dist/`), shipped inside the NuGets |
| `Alis.Reactive.Analyzers` | Roslyn analyzer project (small: `AnalyzerHelpers.cs`) |
| `Alis.Reactive.SandboxApp` | The proving ground — every primitive demonstrated on a page |
| `tests/` | `ArchitectureTests` (statics gate), `PlaywrightTests` + `Playwright.Extensions` |
| `tools/` | `PlanContractGenerator` (C# → `plan.ts`), `ApiDocGenerator`, `FusionOnboarding` (artifact tree) |

`tools/md-viewer` was deleted from git on this branch; untracked leftovers
remain on disk (`node_modules`, `reader.db` + WAL files). The db may be live
Reader data — flagged for the owner, not deleted.

## Alis.Reactive (C# core) — by directory

| Directory | Role |
|---|---|
| `PlanAuthoring/Pipelines` | What developers chain: `PipelineBuilder` (+ `.Http`/`.Conditions`/`.Arrays` partials), `TriggerBuilder`, `ElementBuilder`, `DispatchPayloadBuilder` |
| `PlanAuthoring/Requests` | `Get/Post/Put/Delete`, Gather, Response routes, chained + parallel |
| `PlanAuthoring/Conditions` | `When/ElseIf/Else`, operators, guard composition (`ConditionStart.cs` is the entry) |
| `PlanAuthoring/{Plugins,Arrays,Events,ExpressionPaths}` | Plugin registration, array ops, typed events, lambda→path extraction |
| `PlanModel/Reactions` | `StartsWhen` (triggers), `ReactionGraph` (set/call/dispatch/branch/sequence/request), `BehaviorGraph` |
| `PlanModel/Values` | `Source` (component/payload/url/plugin/dom) + `ValueExpression` — the one value path |
| `PlanModel/BrowserObjects` | The object model contracts: `BrowserObject(s)`, `BrowserObjectContract(s)` (properties/methods/events maps), `PluginContract` |
| `PlanModel/{Requests,Conditions,Validation,Document}` | Wire models for HTTP, condition graphs, validation containers, the plan document |
| `PlanModel/Serialization` | `PlanNodeDiscriminator<T>` — each model class carries its own `kind` |
| `PlanModel/ContractGeneration` | `PlanContractGenerator` — emits `runtime/types/plan.ts` |
| `Components/{Contracts,InputRegistration,Onboarding}` | `ComponentRef` (`EmitSet`/`EmitCall`/`Read<T>`), `IInputComponent`, input registration, typed event onboarding |
| `Razor/Extensions` | `HtmlExtensions` (`ReactivePlan`, `RenderPlan`, `On`), `InputFieldExtensions`, `PlanExtensions` |
| `Validation/` | Client-rule plan binding (`ClientValidationFieldBinding` → `ComponentValidation`) |

## TS runtime (`Alis.Reactive.Assets/runtime/`) — 50 files

| Directory | Role |
|---|---|
| `root.ts` | Discovers `[data-reactive-plan]` scripts, parses, boots — the only entry |
| `lifecycle/` | `boot.ts` (stamps `data-alis-booted`, fires `alis:booted`), `applied-plans.ts` (Active Plan composition, partial slot load/unload), merge policies |
| `types/plan.ts` | GENERATED contract — never hand-edited (hook-enforced) |
| `browser-objects/` | Runtime object model: vendor `component-driver`, event contracts (reads `channel`), `runtime-object` (vendor-neutral property/method execution), paths, shapes, values |
| `events/` | Per-vendor adapters `event-fusion` / `event-native` + `resolver` |
| `execution/triggers` | Trigger wiring (document/component/server-push/signalr/page-ready) |
| `execution/reactions` | `execute.ts` — the reaction switch (set/call/dispatch/branch/sequence/request) + `assertNever` |
| `execution/requests` | `gather.ts` (target <- source), `http.ts`/`http-fetch.ts` (one real `fetch(url, init)` at `http.ts:83`), url templates, payload writer |
| `execution/realtime` | `server-push.ts` (SSE), `signalr.ts`, `retry-indicator.ts` (global container ID + self-stamped `data-reactive-retry` markers) |
| `execution/partials` | `inject.ts` — partial injection + slot boot |
| `values/` | `evaluate.ts` — the ONE ValueExpression resolver; `array-op-engine.ts` |
| `conditions/` | Condition graph walk + `compare-engine` |
| `validation/` | `orchestrator` (evaluates `ComponentValidation.value` then rules), `rule-engine`, `live-clear`, `error-display` |
| `components/` | App-level objects: native drawer/loader/action-link, fusion confirm |
| `plugins/catalog.ts` | Plugin object registry (the string boundary) |
| `shared/`, `diagnostics/` | `assert-never`, wire-format helpers, `trace` (scoped logging) |

## Naming families (current truth)

- `data-reactive-*` — plan discovery (`data-reactive-plan`), retry markers
  (`data-reactive-retry`).
- `alis`-prefixed survivors, ONE deferred family, one future slice: the
  `data-alis-booted` marker + `alis:booted`/`alis:retry` events +
  `alis-realtime-connection-retry-container` element ID +
  `dataset.alisRetryWired` latch. All are load-bearing public/test hooks or
  sit beside them; renaming requires its own full-gate run (recorded in the
  rc3 log, 2026-06-11/12).

## Recently removed (do not look for these)

`PayloadContract` (wire term then internal class — deleted entirely),
`ObjectEvent.Merge`, payload `type:` on wire payload sources, JSON schema as
contract, `plan-and-entries` docs slug, md-viewer tool.
