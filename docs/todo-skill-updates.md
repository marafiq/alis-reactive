# Skill Updates & Tooling — TODO

Status: WIP

## Hooks to Build

### Schema Drift Hook (HIGH PRIORITY)

Build a hook or build-time script that validates C# descriptor JSON output against
`reactive-plan.schema.json` on every build. The schema and descriptors have drifted
in the past. Options:
- Python hook in `.claude/` that runs `dotnet test` schema tests before allowing commits
- MSBuild target that generates sample JSON from each descriptor and validates against schema
- Standalone `tools/SchemaValidator/` console app run as part of `dotnet build`

### Raw Input / SF Builder Detection (DONE — hookify)

Created `.claude/hookify.no-raw-inputs.local.md` — blocks `<input>`, `<select>`, `<textarea>`,
MVC `Html.TextBoxFor` etc., and direct SF builders (`.DropDownListFor()`, `.AutoCompleteFor()`, etc.)
in `.cshtml` files. All inputs must go through `Html.InputField().NativeXxx()` or `.FusionXxx()`.

### DOM Scanning Detection (TODO)

Build a hook that blocks `querySelectorAll`, `getElementsByClassName`, DOM traversal patterns
in TS runtime files. The plan carries all IDs — runtime uses `getElementById` only.

## onboard-fusion-component (6 errors found)

1. `ReactiveWiringHelper.Wire<>()` does not exist — skill references it, code inlines wiring
2. `FusionGatherExtensions` does not exist — actual class is `GatherExtensions` in core project
3. Gather constraint wrong — skill says `FusionComponent`, code says `IComponent` (vendor-agnostic)
4. `PreventDefault`/`UpdateData` param type wrong — skill says `PipelineBuilder<TModel>`, code uses `ICommandEmitter`
5. `UpdateData` generic signature wrong — skill says `<TModel, TResponse>`, code has `<TResponse>`
6. File count "5 + N" framing inconsistent with docs-site "7-file" branding

### Missing from skill
- `ICommandEmitter` interface (actual param type for event args extensions)
- Non-input component pattern (FusionTab has custom builder)
- `IAppLevelComponent` interface
- Two `SetDataSource` overloads (event-source + response-body)
- `Fields` with `GroupBy` 3-arg overload
- `SetText()` method on AutoComplete
- Multiple focus methods (`FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`)

## docs-site onboarding-component.md (3 errors)

1. Plan param type wrong — says `IReactivePlan<TModel>`, code uses `ReactivePlan<TModel>`
2. HtmlAttributes advice contradicts most components (says "always param", 12/13 use fluent)
3. "7-file vertical slice" not universal — ranges from 6-8 depending on event count

## reactive-dsl (WIP — expand scope, verify examples)

Scope expanded per view co-occurrence analysis:
- Merge InputField + component rendering (70% of views use it alongside core DSL)
- Merge SSE/SignalR triggers (too thin for standalone skill, only 1 view)
- Verify all named param examples match current API
- Add: `Html.ReactivePlan<T>()`, `Html.RenderPlan(plan)` lifecycle
- Add: `.Reactive()` wiring pattern with event selectors
- Add: InputField + component selection by data type
- Add: ServerPush/SignalR trigger sections

## design-system (Missing — create new)

- Layout primitives: `native-vstack`, `native-hstack`, `native-card`, `native-grid`, `native-heading`, `native-text`, `native-divider`
- Used in every view — distinct from reactive behavior

## http-pipeline (WIP — verify)

- Verified clean by agent — no changes needed

## conditions-dsl (WIP — verify)

- Verified clean by agent — no changes needed

## validation-rules-alis-reactive (5 gaps found via blind test)

1. Missing `OnError(400, e => e.ValidationErrors("form"))` — server-side error routing
2. Missing `.WithMessage()` — every real validator uses custom messages
3. No numeric threshold conditions documented — WhenField only supports truthy/eq/neq, not gt/lt
4. No Gather + Validate relationship — when to use `g.IncludeAll()` alongside Validate
5. No component selection guidance — which component for which data type

## bdd-testing (WIP — verify)

- Verified clean by agent — no changes needed
