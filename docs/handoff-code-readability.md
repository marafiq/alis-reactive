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
- `ConditionSourceBuilder<TModel,TProp>` has short XML on nearly every operator plus inline category comments. The method names already describe most operators. Keep shape/type-safety guidance at the class level and trim repetitive member summaries.
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
- `scripts/test.sh --no-e2e` hung once on this branch while running
  `vite build --config vite.design-system.config.ts`. The stuck process tree was
  cleaned up, `npm run build:design-system` completed normally, and the full
  non-e2e gate passed on rerun. TODO: capture logs/process state if this repeats
  and decide whether the wrapper needs timeout or progress diagnostics.
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
