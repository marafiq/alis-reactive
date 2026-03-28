---
name: project_pipeline_consistency_refactor
description: Pipeline parameter ordering inconsistencies to refactor — pipeline should always be first arg (or second after plan)
type: project
---

## Pipeline Parameter Ordering — Consistency Refactor

**Status:** Noted for future refactor

**Problem:** `pipeline` parameter position is inconsistent across the API:

### Current inconsistencies:

1. **OnSuccess<T>** — `(json, pipeline)` — pipeline is second, json (ResponseBody<TResponse>) is first
2. **CustomEvent<T>** — `(payload, pipeline)` — pipeline is second, payload is first
3. **args.{Method}** — `args.UpdateData(pipeline, json, j => j.Items)` — pipeline is first here
4. **args.{SetProp}** — `args.PreventDefault(pipeline)` — pipeline is first here
5. **.Reactive()** — `(args, pipeline)` — pipeline is second, args is first

### Proposed rule:

`pipeline` should always be:
- **First arg** when it's the only context parameter
- **Second arg** when `plan` is passed (e.g., `.Reactive(plan, evt, (args, pipeline))`)

### Also noted:

- `HiddenFieldFor` → should be `NativeHiddenField` (Fusion prefix consistency already applied in grammar tree, code refactor pending)
- Fusion component factory methods need `Fusion` prefix (grammar tree uses it, code refactor pending)
- `Component<T>("refId")` string overload should not be available for input components — input components should only use `Component<T>(m => m.Prop)` expression overload
- Missing `Component<TComponent, TCrossModel>(crossModel => crossModel.Prop)` overload — currently `TModel` is always inferred from `PipelineBuilder<TModel>`, no way to target a component from a different model (e.g., partial with a different TModel)

**Why:** API surface should be predictable — a dev shouldn't have to remember which stages put pipeline first vs second.

**How to apply:** When refactoring, update all `Action<TPayload, PipelineBuilder>` to `Action<PipelineBuilder, TPayload>` or choose one consistent pattern and apply everywhere.
