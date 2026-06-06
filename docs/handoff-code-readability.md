# Code Readability Comment Handoff

Date: 2026-06-03

## Scope

This began as a report-only audit. Branch implementation passes should append
follow-up notes here rather than rewriting the historical findings.

The audit looked for:

- excessive XML documentation on APIs whose names already carry the intent
- stale or over-specific implementation comments
- inline `//` comments in tests that describe each action instead of naming the behavior through helpers or test names

Target audience is framework developers. Comments should explain DSL intent,
Reactive Plan runtime-boundary behavior, real browser API behavior,
compatibility constraints, or non-obvious tradeoffs. They should not narrate
ordinary code. Use "Active Plan" only when the code is about the runtime-composed
plan state.

## Cleanup Standard

Keep comments when they answer one of these questions:

- What DSL concept, Reactive Plan runtime boundary, or real browser API boundary is this code protecting?
- Why is this implementation intentionally different for `net48`, Syncfusion, or Reactive Plan runtime behavior?
- What invariant would be easy to break during refactoring?
- What public API behavior must remain visible in IntelliSense?

Remove or rewrite comments when they:

- restate the method name or parameter name
- describe each line of straightforward rendering or assertion code
- preserve old vocabulary that is no longer part of the logical structure
- use decorative section banners in tests where method names and helpers can carry the structure
- include long XML examples that are better suited for docs or sandbox pages

## High-Priority Areas

### Test Inline Comments

The highest concentration of inline comments is in Playwright tests. Many are
step narration or section banners. These should become smaller helpers, clearer
test names, or no comment at all.

Initial top files by inline `//` count:

- `tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs` - 95
- `tests/Alis.Reactive.PlaywrightTests/Conditions/HttpMixing/WhenTriggerDrivenConditionsMixWithHttp.cs` - 89
- `tests/Alis.Reactive.PlaywrightTests/Patterns/Cascading/WhenParentSelectionFiltersDependentList.cs` - 86
- `tests/Alis.Reactive.PlaywrightTests/Patterns/ReactiveWiring/WhenGuardsControlReactiveFlow.cs` - 60
- `tests/Alis.Reactive.PlaywrightTests/HttpPipeline/WhenServerDataLoads.cs` - 56
- `tests/Alis.Reactive.PlaywrightTests/CoreBehaviors/WhenPayloadFlowsBetweenEvents.cs` - 52
- `tests/Alis.Reactive.PlaywrightTests/Validation/Contract/WhenMultiFieldFormSubmits.cs` - 48
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenAutoCompleteSuggests.cs` - 48

Examples and current status:

- Resolved on `tiny-safe-but-important-refactorings`: section banners and repeated
  branch-result comments were removed from the touched Fusion date/time/input tests.
- Resolved on `tiny-safe-but-important-refactorings`: redundant method XML
  summaries were removed from `WhenGuardsControlReactiveFlow`, and two test names
  now carry the branch-clearing and Fusion SetValue cascade intent.
- Resolved on `tiny-safe-but-important-refactorings`: Native checkbox/dropdown
  tests no longer repeat plan-shape explanations in both comments and assertion
  messages; only DOM/Reactive Plan join-key and native DOM value-path notes remain.
- Resolved on `tiny-safe-but-important-refactorings`: Native textbox, textarea,
  checklist, radio, hidden-field, and button tests no longer carry class-level
  "Exercises ..." summaries when the test names already enumerate the behavior.
  Comments remain where they explain change-event timing, hidden input scoping,
  or deliberate test ordering.
- Resolved on `tiny-safe-but-important-refactorings`: selected Fusion component
  tests no longer use XML summaries as coverage inventories. ColorPicker,
  Switch, NumericTextBox, DatePicker, DateTimePicker, TimePicker, and InputMask
  now keep only the comments that explain generated ID join keys or Syncfusion
  popup/commit behavior.
- Resolved on `tiny-safe-but-important-refactorings`: RichTextEditor test
  comments no longer duplicate a coverage inventory. The remaining class-level
  note names the Syncfusion contenteditable commit boundary.
- Resolved on `tiny-safe-but-important-refactorings`: InPlaceEditor date and
  masked-MRN commit tests no longer carry XML narrative summaries. The remaining
  notes are concise inline comments for Syncfusion Enter-key submit behavior,
  mask display formatting, validation input shape, and the fixed-wait TODO.
- Resolved on `tiny-safe-but-important-refactorings`: remaining InPlaceEditor
  Playwright test summaries were collapsed or deleted. The kept comments now
  name component-registration/gather, validation slots, lifecycle args,
  Syncfusion wrapper behavior, commit paths, and TODO flakiness notes.
- Resolved on `tiny-safe-but-important-refactorings`: CoreBehaviors event and
  payload tests no longer use XML summaries for test-class navigation. Concise
  comments remain only for dispatch-chain intent, payload casing, trace/order
  invariants, and class-update drift.
- Resolved on `tiny-safe-but-important-refactorings`: cascading dropdown tests
  now use plain comments for DSL gather/load/save intent and Syncfusion
  user-gesture change behavior instead of XML summary scaffolding.
- Resolved on `tiny-safe-but-important-refactorings`: remaining Playwright
  class-level coverage inventories were trimmed from selected condition,
  reactive-wiring, workflow, drawer, grid, date-range, and multiselect tests.
  Syncfusion gesture and duplicate-input notes were kept as plain comments.
- Resolved on `tiny-safe-but-important-refactorings`: a second pass removed
  remaining Playwright class XML inventories from selected HTTP, array, grid,
  tab, accordion, autocomplete, dropdown, file-upload, and component-gather
  tests. Boundary notes for DataTransfer, DOM array normalization, grid
  re-filtering, custom payload arrays, and non-parallel popup behavior remain.
- `WhenAllComponentsGatherIntoOnePost.cs` has long form-fill flows, but helper
  extraction should happen only if it names a reusable domain action. Do not add
  private helper indirection that hides a one-off behavior proof.
- `WhenMultipleItemsSelected.cs` still has several inline comments, but the current
  scan found them mostly explaining Syncfusion popup mode, hidden selected items,
  real keyboard filtering, and generated component IDs. Leave them unless a later
  behavior slice changes those mechanics.
- Resolved on `tiny-safe-but-important-refactorings`: simple validation step
  comments such as "fix it", "submit first", and "trigger error first" no longer
  appear in the active validation/conditions/Fusion test scan.

Comments worth keeping in tests are the ones that explain unstable DOM/event
timing or vendor behavior, such as duplicate Syncfusion inputs, required popup
sequencing, or why a test must be non-parallel.

### TypeScript Runtime Comments

- Resolved on `tiny-safe-but-important-refactorings`: private runtime helper
  invariants in validation, value evaluation, array operations, and boot wiring
  now use plain comments instead of JSDoc blocks. The kept comments still name
  partial-unmount validation behavior, DOM/value boundaries, array normalization,
  and two-phase page-ready wiring.

### Builder XML Documentation

Several public builder APIs have method-by-method XML comments that mostly
repeat fluent method names. Keep public API XML concise for IntelliSense, but
delete boilerplate and move richer examples to docs.

Top XML-heavy source files:

- `Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs` - 106 XML lines
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs` - 85 XML lines
- `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs` - 85 XML lines
- `Alis.Reactive/PlanAuthoring/ExpressionPaths/ExpressionPathHelper.cs` - 84 XML lines
- `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleExtensions.cs` - 66 XML lines
- `Alis.Reactive/Razor/Extensions/PlanExtensions.cs` - 63 XML lines
- `Alis.Reactive.Fusion/Components/FusionInPlaceEditor/FusionInPlaceEditorExtensions.cs` - 63 XML lines
- `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteExtensions.cs` - 60 XML lines

Candidate cleanup:

- Resolved on `tiny-safe-but-important-refactorings`: `FusionTemplateBuilder<T>`
  public XML docs now include concise parameter and type-parameter tags for all
  public overloads. The generated API reference now shows overload signatures
  with real parameter names instead of repeated empty method calls.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionAIAssistView` event
  payload XML docs now describe the public event contract, payload fields, and
  cancellation helper instead of repeating Syncfusion event names.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionTooltip` event
  payload XML docs now describe visible event state instead of repeating
  tooltip event names. The generated API reference also picked up the
  `FusionAIAssistView` event member docs after rebuilding the Fusion XML
  documentation output before `npm run build:api-docs`.
- Resolved on `tiny-safe-but-important-refactorings`: `ConditionSourceBuilder`
  operator docs now include the parameter tags needed for generated API
  signatures such as `Eq(operand)` versus `Eq(right)`. This keeps the public DSL
  IntelliSense standard without adding examples or changing condition behavior.
- `ConditionSourceBuilder<TModel,TProp>` should not be revisited for another
  comment-only pass unless the API generator starts rendering method summaries;
  its remaining XML exists to support public DSL IntelliSense and generated
  overload signatures.
- Resolved on `tiny-safe-but-important-refactorings`: the API reference
  generator now omits extension-method receiver parameters from display
  signatures. Generated docs show calls such as `On<T>(plan, trigger)`,
  `SetValue<T>(value)`, and `PreventDefault(pipeline)` instead of exposing
  implementation receivers like `html`, `self`, `builder`, or event `args`.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionConditionalBuilder`
  XML docs now include standard parameter tags, so generated template-builder
  API signatures show real overloads such as `Span<T>(property)` and
  `Button(text, onClick, css)` instead of repeated empty calls.
- Resolved on `tiny-safe-but-important-refactorings`: plugin argument builder
  XML docs now include standard parameter and return tags, and the API reference
  generator disambiguates duplicate simple overload signatures. Generated docs
  now show `Arg(string value)`, `Arg(int value)`, and
  `RouteParam(string paramName, long value)` instead of repeated empty or
  same-name calls.
- Resolved on `tiny-safe-but-important-refactorings`: plugin declaration XML
  docs now include standard parameter, type-parameter, and return tags for
  protected declaration helpers. Generated docs now show
  `Function<T>(member, arguments)`, `Command(arguments)`, and `Args(arguments)`
  instead of repeated empty calls.
- Resolved on `tiny-safe-but-important-refactorings`: the API reference
  generator now filters XML documentation through the compiled assemblies'
  public/protected surface. Internal implementation types such as
  `PluginArgumentCollector`, `BrowserObject`, `ContractDriftGate`, and
  `PlanContractGenerator` no longer appear on the public API page.
- Resolved on `tiny-safe-but-important-refactorings`: the API reference
  generator now simplifies XML `<see cref="...">` links after removing method
  parameter lists and generic brace notation. Generated summaries show
  `InputField<T>`, `FusionTextBox<T>`, and `NativeRadioGroup<T>` instead of
  malformed artifacts such as `Func{<T>`, `Builder})`, or `Builder{<T>`.
- Resolved on `tiny-safe-but-important-refactorings`: generated API summary
  prose no longer preserves XML indentation from source comments. Two Fusion
  summaries that previously relied on manual indentation now use inline
  `<c>...</c>` examples so generated docs stay readable.
- Resolved on `tiny-safe-but-important-refactorings`: the API reference
  generator now falls back to compiled parameter names when XML docs omit
  parameter tags, and uses simplified reflected parameter types only when
  duplicate overload signatures would otherwise be ambiguous. This removes
  repeated empty method calls across core, Fusion, and Native generated API
  sections without adding boilerplate XML comment noise.
- Resolved on `tiny-safe-but-important-refactorings`: Native component and
  app-level extension XML docs no longer repeat generic `TModel` and extension
  receiver `self` descriptions that only restated the component reference. The
  remaining docs keep user-facing parameters, value-source details, and runtime
  behavior notes.
- Resolved on `tiny-safe-but-important-refactorings`: core `GatherExtensions`
  XML docs no longer repeat `TModel` ownership or the extension receiver. The
  docs still keep request-body, component-contract, source, path, and body-field
  behavior because those are the parts developers need in IntelliSense.
- Resolved on `tiny-safe-but-important-refactorings`: `NativeGatherExtensions`
  keeps its NativeTextBox shorthand behavior docs but no longer repeats the
  generic model or extension receiver boilerplate on `Include<T>()`.
- Native component builders include long XML examples. Keep the user-facing factory summary and move multi-line usage examples to docs or sandbox guidance.
- Event payload constructors often say "Creates a new instance. Framework-internal..." repeatedly. Prefer one concise convention across event payload types.

### Implementation Inline Comments

Some implementation comments are useful because they describe real compatibility
or runtime constraints. Others narrate obvious writes.

Keep:

- `net48` comments that explain MVC `ValueFor` encoding behavior.
- comments that explain Syncfusion rendering quirks, duplicated inputs, or real browser event ordering.
- comments that mark plan/runtime invariants that are not obvious from code.

Rewrite or delete:

- comments like `Container div wraps the radio group`, `Text block`, and similar markup narration in builders.
- comments that say a value is selected, submitted, fixed, or reset immediately before a line that does exactly that.
- decorative separators in tests; use helper names and smaller test fixtures instead.

## Suggested Next Pass

1. Start with Playwright test files, because they have the highest noise and the
   lowest public API risk.
2. Prefer deleting narrating comments or naming locals clearly. Extract helpers
   only when the helper names a genuinely reusable behavior and removes repeated
   mechanics without hiding the assertion proof.
3. Preserve comments that explain Syncfusion, real browser timing, or compatibility quirks.
4. Then trim public XML docs in one logical API slice at a time, keeping concise
   IntelliSense summaries for public DSL methods.
5. Match verification to risk: comment-only and XML-doc slices need build/typecheck
   proof, not Playwright. Use `scripts/playwright.sh --filter "..."` when selectors,
   waits, assertions, test flow, sandbox behavior, or runtime behavior changes.

## Branch Follow-Up Notes

- `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnEventRendered.cs`
  exposes `FusionScheduleEventData` as framework public surface with schedule-domain
  fields such as `ShiftId`, `StaffName`, and `StaffRole`. This may be intentional
  Schedule integration shape because `FusionScheduleExtensions.GetEvents()` returns
  it and the sandbox posts it back, but it should be reviewed in a dedicated API
  surface slice instead of being changed during XML documentation cleanup.
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Patterns/IdGenerator/Index.cshtml`
  renders two forms from the same `ReactivePlan<IdGeneratorModel>`, so they share
  generated component IDs and gather reads the first matching DOM ID. The test now
  marks this with `TODO:`; a later slice should give repeated forms an explicit
  component ID scope instead of relying on first-match DOM lookup.
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/InPlaceEditor/*`
  contains fixed waits around Syncfusion commit completion and negative-request
  assertions. The touched waits are now marked with `TODO:`; a later slice should
  replace them with visible commit signals or a shared behavior-focused no-POST
  proof.
- `scripts/test.sh --no-e2e` has hung on this branch while running Vite asset
  builds: once at `vite.design-system.config.ts`, and again at
  `vite.fusion.config.ts` with process `node .../vite build --config
  vite.fusion.config.ts` stuck for more than 90 seconds after printing
  `build:fusion`. The stuck process tree was cleaned up and the gate passed on
  rerun after the first occurrence. TODO: add wrapper timeout/progress
  diagnostics and capture Vite process state if this repeats.
- Native and Fusion component files are intentional vertical slices. Do not sweep
  every component just because a repeated XML-doc phrase appears; finish one
  component or one non-component concept at a time and keep the commit boundary
  reviewable.
- Active docs under `docs-site/src/content/docs/architecture/` still include
  `descriptors-and-plan.mdx` and related links that explain an older plan model
  shape with `Command`, `Mutation`, and `BindSource` terms. Do not drive-by
  rename this page during comment cleanup; a later docs-model slice should
  verify the current C# plan domain and generated TS contract, then update the
  page title, route/link text, diagrams, and examples together.
- `docs-site/src/content/docs/architecture/onboarding-component.md` now uses
  typed-event wording for `.Reactive(...)`, but still teaches component onboarding
  with older internal examples such as `SetPropMutation`, `CallMutation`,
  `MutateEventCommand`, `ICommandEmitter`, and `pipeline.AddCommand`. Do not
  patch one snippet in isolation; a later component-onboarding docs slice should
  verify current `ComponentRef.EmitSet` / `EmitCall`, event-args extension APIs,
  generated plan JSON, and runtime terms, then update the guide end to end.
