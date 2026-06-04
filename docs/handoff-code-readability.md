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

Top files by inline `//` count:

- `tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs` - 95
- `tests/Alis.Reactive.PlaywrightTests/Conditions/HttpMixing/WhenTriggerDrivenConditionsMixWithHttp.cs` - 89
- `tests/Alis.Reactive.PlaywrightTests/Patterns/Cascading/WhenParentSelectionFiltersDependentList.cs` - 86
- `tests/Alis.Reactive.PlaywrightTests/Patterns/ReactiveWiring/WhenGuardsControlReactiveFlow.cs` - 60
- `tests/Alis.Reactive.PlaywrightTests/HttpPipeline/WhenServerDataLoads.cs` - 56
- `tests/Alis.Reactive.PlaywrightTests/CoreBehaviors/WhenPayloadFlowsBetweenEvents.cs` - 52
- `tests/Alis.Reactive.PlaywrightTests/Validation/Contract/WhenMultiFieldFormSubmits.cs` - 48
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenAutoCompleteSuggests.cs` - 48

Examples to simplify:

- `WhenDateTimeSelected.cs` uses section-banner comments such as `// Section 1: Property Write` and step comments before direct Playwright actions.
- `WhenAllComponentsGatherIntoOnePost.cs` has many field-by-field comments inside long form-fill flows. Prefer helper methods named by domain action.
- validation rule tests use comments like `// Fix it`, `// Submit first`, and `// Trigger error first`; those should either disappear or become focused helper names.

Comments worth keeping in tests are the ones that explain unstable DOM/event
timing or vendor behavior, such as duplicate Syncfusion inputs, required popup
sequencing, or why a test must be non-parallel.

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
2. Replace repeated step comments with helper methods named after the domain action.
3. Preserve comments that explain Syncfusion, real browser timing, or compatibility quirks.
4. Then trim public XML docs in one logical API slice at a time, keeping concise
   IntelliSense summaries for public DSL methods.
5. Run the focused Playwright filter for each touched slice, then the observable
   full Playwright gate before committing the cleanup.

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
