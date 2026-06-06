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
- Resolved on `tiny-safe-but-important-refactorings`: the InPlaceEditor
  lifecycle trace test no longer comments every asserted event field; the
  test name and trace-cell assertions carry the flow, while the class-level
  boundary note remains.
- Resolved on `tiny-safe-but-important-refactorings`: the InPlaceEditor masked
  MRN invalid-input test no longer narrates the typed invalid value. Syncfusion
  raw-versus-formatted mask comments remain because they explain the vendor
  boundary under test.
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
- Resolved on `tiny-safe-but-important-refactorings`: condition-runtime sync-lane
  invariants now use plain comments instead of JSDoc blocks. The comments remain
  because validation and branch execution depend on sync behavior until confirm
  crosses the async boundary.
- Resolved on `tiny-safe-but-important-refactorings`: shape-conversion runtime
  invariants now use plain comments instead of JSDoc blocks. The comments remain
  because best-effort runtime reads and strict validation comparisons intentionally
  have different failure behavior.

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
- Resolved on `tiny-safe-but-important-refactorings`: Native component
  event-wiring XML docs no longer repeat the extension receiver as "builder
  being wired." `NativeButton` also dropped generic model boilerplate where the
  summary already explains the explicit-ID event target.
- Resolved on `tiny-safe-but-important-refactorings`: Native changed-event
  payload remarks now describe typed Reactive Plan event-payload reads instead
  of calling the public properties "typed markers." Payload shape remains
  unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion component
  event-wiring XML docs no longer repeat generic `TModel` ownership or the
  extension receiver. Event selector, pipeline, and Reactive Plan parameters
  remain documented because they describe the public DSL wiring contract.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionDropDownList`
  event selector docs now stop at two event examples and use "etc." rather
  than listing every available event in the method remarks.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion render-factory XML
  docs no longer repeat generic `TModel` ownership or the Razor HTML helper
  receiver. Bound value types, field wrappers, Reactive Plan, controlled IDs,
  and Syncfusion builder callbacks remain documented.
- Resolved on `tiny-safe-but-important-refactorings`: Native render-factory XML
  docs no longer repeat Razor HTML helper or generic model boilerplate where the
  summaries already name the rendered element, action-link browser boundary, or
  returned builder behavior.
- Resolved on `tiny-safe-but-important-refactorings`: the active Native
  `*Builder.cs` scan no longer shows long XML examples. Remaining component
  examples are short public DSL examples or explain runtime/browser boundaries.
- Resolved on `tiny-safe-but-important-refactorings`: the active source scan no
  longer shows repeated "Creates a new instance. Framework-internal..."
  constructor docs.
- Resolved on `tiny-safe-but-important-refactorings`: simple component
  `SetValue(value)` docs no longer repeat the parameter name as "The value to
  set." Summaries still name the component contract being written.
- Resolved on `tiny-safe-but-important-refactorings`: active Razor helper XML
  docs no longer repeat extension receiver parameters such as "The Razor HTML
  helper." Plan, expression, controlled ID, and builder callback docs remain
  because they describe the DSL contract.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion event-payload
  helper XML docs no longer repeat the extension `args` receiver or describe
  pipeline parameters as only "current builder." The remaining parameter docs
  name the event-arg mutation added to the Reactive Plan.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionConditionalBuilder`
  fluent XML docs no longer repeat "The current conditional builder" on every
  append method. Parameter docs and raw `onclick`/HTML trust boundary remarks
  remain because they carry the template authoring contract.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionTemplateBuilder`
  and `FusionConditionalBuilder` literal-text and nested-template parameter
  docs, plus root class and attribute parameter docs, now keep
  generator-required XML tags while dropping prose that repeated the method
  name. Trust-boundary remarks for raw HTML, attributes, events, and `onclick`
  remain.
- Resolved on `tiny-safe-but-important-refactorings`: plugin argument builder
  XML docs no longer repeat "current plugin member/call builder" on every
  fluent argument method. Source, response-body, event-payload, and literal
  parameter docs remain because they describe the plugin argument contract.
- Resolved on `tiny-safe-but-important-refactorings`: `PipelineBuilder`
  fluent return docs no longer use repeated "current builder" phrasing. The
  ordering facts remain because chained reaction order is part of the public
  authoring contract.
- Resolved on `tiny-safe-but-important-refactorings`: plugin declaration
  `Arg<T>()` and `Args(...)` XML docs no longer repeat the fluent return type,
  and the typed plugin-source conversion parameter is named `builder` instead
  of `b`. Argument-contract docs remain because they describe the public DSL
  shape.
- Resolved on `tiny-safe-but-important-refactorings`: plugin literal overload
  XML docs no longer repeat `value` as "the literal value to pass" for every
  primitive type. Summaries still name the literal DSL action, and response,
  event-payload, source, and DateTime formatting docs remain where they carry
  contract or runtime meaning.
- Resolved on `tiny-safe-but-important-refactorings`: `InputBoundFieldBase`
  public property summaries now explain their component-rendering and
  registration roles instead of repeating "Gets the..." boilerplate. The render
  invariant comment remains because it protects validation/gather behavior.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionMultiSelect`
  filtering `UpdateData` XML now describes the `ResponseBody<T>` argument as
  the response-body scope that supplies popup items instead of a vague
  "response body instance." Syncfusion popup lifecycle remarks remain.
- Resolved on `tiny-safe-but-important-refactorings`: Playwright extension
  summaries for breadcrumb and slider helpers now state whether they locate or
  read test-visible state instead of saying "The current...". Selectors and
  wait behavior were left unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion TextBox,
  TextArea, and OtpInput event payload `Value` summaries now say when the
  value is captured by the event instead of repeating "Current ... value."
  Payload shape and event wiring remain unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: remaining Fusion event
  payload summaries with vague "Current..." wording now name the event moment
  or Syncfusion action context for Grid, Carousel, ProgressButton, and
  AIAssistView payloads. Payload shape and wiring remain unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion change-event
  payload `Value` summaries now identify the after-change value instead of
  terse "Selected value" or "New value" wording. `PreviousValue` and
  interaction metadata docs remain unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion Grid row
  selection, toolbar click, and text-filter payload summaries now identify the
  event source or criterion role instead of terse labels such as "Selected row
  data" or "The clicked toolbar item." Payload shape remains unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion Stepper event
  payload index summaries now state zero-based indexing and distinguish
  clicked, pending-transition, and completed-change timing. Payload shape
  remains unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion Accordion expanded
  payload docs now describe the public Reactive Plan event-payload path instead
  of exposing `ExpressionPathHelper` implementation vocabulary. Payload shape
  remains unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: `PlanBuildContext` is no
  longer public API surface. It has only internal construction and internal
  members, and is now kept out of generated public API docs as an implementation
  boundary between DSL builders and Reactive Plan domain state.
- Resolved on `tiny-safe-but-important-refactorings`: `IReactionEmitter`
  remains public because component event helper methods accept it, but its XML
  docs now describe the event-helper append contract instead of exposing
  implementation vocabulary such as `ComponentRef`.
- Resolved on `tiny-safe-but-important-refactorings`: `IReactionEmitter.AddStep`
  is no longer public API surface. Component packages still use it through
  friend-assembly access, while generated public docs no longer teach
  application developers to append raw `ReactionGraph` nodes.
- Resolved on `tiny-safe-but-important-refactorings`: `ExpressionPathHelper`
  is no longer public API surface. It remains shared framework plumbing for
  friend assemblies, while public developers use the DSL, `IdGenerator`, and
  component/gather helpers instead of raw expression-path conversion.
- Resolved on `tiny-safe-but-important-refactorings`: `ComponentRegistration`
  is no longer public API surface. It remains internal component registration
  plumbing between HtmlExtensions, gather, validation, and generated Reactive
  Plan component metadata.
- Resolved on `tiny-safe-but-important-refactorings`: app-level component XML
  docs now explain the public behavior, "can be referenced without an explicit
  ID," instead of leading with `IAppLevelComponent` implementation mechanics.
- Resolved on `tiny-safe-but-important-refactorings`: Native loader/drawer TS
  runtime comments now keep the layout-singleton boundary note without naming
  the C# `IAppLevelComponent` interface in runtime-side prose.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion grid and tab XML
  docs now explain missing form-value/input-field behavior directly instead of
  leading with non-input component labels or `IInputComponent` implementation
  vocabulary.
- Resolved on `tiny-safe-but-important-refactorings`: `FusionAIAssistView` XML
  docs now name the typed prompt reads, component methods, and events instead
  of leading with a "non-input component" category label.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion button-family
  extension XML docs no longer expose `dataBind`/flush mechanics as public
  IntelliSense wording. Button, DropDownButton, SplitButton, and ProgressButton
  operations now describe rendered component behavior; Syncfusion wording remains
  only where it names a public component concept such as item IDs.
- Resolved on `tiny-safe-but-important-refactorings`: remaining Fusion
  component extension XML docs no longer use "flush" wording for visible value
  updates. Text input, OTP, slider, rating, breadcrumb, radio, and checkbox docs
  now name the rendered value/state contract instead of the internal refresh
  mechanism.
- Resolved on `tiny-safe-but-important-refactorings`: lowercase "reactive
  pipeline" wording was normalized to "Reactive Plan pipeline" in active Fusion
  and app-level Native public docs, the InPlaceEditor docs page, and the one
  Playwright invariant comment that described generated plan wiring. This keeps
  authored plan vocabulary consistent without changing pipeline behavior.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion Grid command and
  read XML docs no longer expose Syncfusion method names such as
  `sortColumn`, `addRecord`, or `getSelectedRecords` in public
  IntelliSense text. The docs now describe the rendered Grid behavior while
  preserving public builder prerequisites such as export and column chooser flags.
- Resolved on `tiny-safe-but-important-refactorings`: InPlaceEditor event
  payload XML docs now describe the event moment for change and edit-mode
  values instead of vague "current/previous inner integrated component"
  wording. The public payload shape remains unchanged.
- Resolved on `tiny-safe-but-important-refactorings`: Schedule popup
  cancellation XML docs no longer say "current Syncfusion popup" in the public
  helper summary. The remarks still keep the popup lifecycle boundary because it
  explains why the cancellation mutation must happen before the callback returns.
- Resolved on `tiny-safe-but-important-refactorings`: remaining non-API TS
  helper comments in `assertNever` and `ExecutionContext.withElement` now use
  plain comments instead of JSDoc blocks. Exported runtime type field docs remain
  JSDoc because they describe payload scopes for TypeScript consumers.
- Resolved on `tiny-safe-but-important-refactorings`: Native component value-read
  XML docs no longer repeat `<returns>A typed source representing ...</returns>`
  where the summary already states the read contract for conditions and gather.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion component value-read
  XML docs no longer repeat `<returns>A typed source representing ...</returns>`
  where the summary already names the value being read. Usage remarks and
  date-range shape guidance remain where they add IntelliSense value.
- Resolved on `tiny-safe-but-important-refactorings`: remaining consumer-facing
  Fusion `DataBind()` docs no longer describe "flush" or Syncfusion instance
  mechanics. Framework onboarding docs still name `dataBind` where the reader is
  implementing a component contract.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion component overview
  and value docs now describe the Reactive Plan contract and rendered component
  behavior instead of `ej2_instances`, Syncfusion instance writes, or public
  method-table call mechanics.
- Resolved on `tiny-safe-but-important-refactorings`: InPlaceEditor public XML
  docs now keep event-order, CSS-class, and registered-shape invariants while
  removing Syncfusion method/path narration from consumer IntelliSense.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion template builder
  XML docs no longer repeat generic type-parameter lines such as "The selected
  property type." Binding summaries, raw HTML/onclick trust warnings, and
  Syncfusion template-context remarks remain.
- Resolved on `tiny-safe-but-important-refactorings`: Fusion template builder
  XML docs now use short `css` parameter text instead of repeating "The CSS
  class to emit..." on every styled overload. Parameter tags remain because the
  API doc generator uses them to render overload signatures.
- Resolved on `tiny-safe-but-important-refactorings`: current reactivity docs
  now describe lazy realtime connections and plan execution as Reactive Plan
  runtime boot behavior instead of using "browser boots/loads" as a broad
  placeholder. Wording that truly refers to browser APIs or stale browser
  assets remains.
- Resolved on `tiny-safe-but-important-refactorings`: plugin argument builders
  no longer repeat `TValue` XML type-parameter docs on `ArgValue<TValue>()`;
  the summaries already explain that the plan shape is derived from the type.
- Resolved on `tiny-safe-but-important-refactorings`: gather builder XML docs
  no longer repeat generic type-parameter lines for typed component reads and
  typed URL reads; the method summaries and parameter docs carry the contract.

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
  `build:fusion`. It repeated on 2026-06-06 during the Fusion factory XML-doc
  slice and required killing the stuck process tree. TODO: add wrapper
  timeout/progress diagnostics and capture Vite process state if this repeats.
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
