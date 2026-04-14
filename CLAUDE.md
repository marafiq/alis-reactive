# Alis.Reactive Framework

C# fluent builders express reactive browser intent. `Html.RenderPlan(plan)` serializes that
intent to JSON validated against `Alis.Reactive/Schemas/reactive-plan.schema.json` (61
definitions). The TypeScript runtime executes plan instructions without adding behavior the
plan does not describe. C# never executes browser behavior. TypeScript never invents
information the plan does not carry.

Throughout this document, "the runtime" means the TypeScript code in
`Alis.Reactive.SandboxApp/Scripts/` that executes plans in the browser.

## Architecture — 5 Layers, 4 Boundaries

Each boundary is guarded by a test harness. A failing test is the only reason to cross one.

```
Layer 1  C# Domain Model + Builders
         Quality: DDD value objects, internal constructors, C# 8.0, SOLID
         Harness: AssertSchemaValid() — 70 calls across Core, Fusion, Native test bases
         ↓
         BOUNDARY: failing AssertSchemaValid() drives schema update
         ↓
Layer 2  JSON Schema (Alis.Reactive/Schemas/reactive-plan.schema.json)
         Quality: additionalProperties: false on every object, 61 $defs
         Harness: AssertSchemaValid() in PlanTestBase, FusionTestBase, NativeTestBase
         ↓
         BOUNDARY: schema change → failing vitest drives TS type update
         ↓
Layer 3  TypeScript Types + Runtime
         Quality: discriminated unions match schema, fail-fast, no fallbacks
         Harness: npm run typecheck (vitest configured but no tests on this branch yet)
         ↓
         BOUNDARY: browser first, then Playwright — eyes before automation
         ↓
Layer 4  Browser Verification
         Quality: real interactions, visible outcomes, no page.evaluate()
         Harness: Playwright BDD (5 rules), 69 test fixtures
         ↓
         BOUNDARY: working sandbox example before writing docs
         ↓
Layer 5  Documentation + Skills
         Quality: dev-facing language, verified code examples, no internals vocabulary
         Harness: sandbox-verified examples
```

Detailed flows: `.claude/rules/process-pipeline.md`, `process-layers.md`, `process-task-types.md`

## Plan-Driven IDs — No DOM Scanning

`IdGenerator` (`Alis.Reactive/IdGenerator.cs`) generates every HTML element ID at C# render
time from the model type and property expression. Format: `{Namespace_TypeName}__{MemberPath}`.

```
Model:      Alis.Reactive.SandboxApp.Models.OrderModel
Expression: m => m.Address.City
ID:         Alis_Reactive_SandboxApp_Models_OrderModel__Address_City
```

All vendors (Syncfusion, Native) produce the same ID for the same expression. IDs are
deterministic and collision-free by construction. The plan carries every ID the runtime needs.

Non-input component IDs (buttons, elements, containers) are the developer's responsibility —
chosen explicitly via `p.Element("my-id")` or `Html.NativeButton("btn-id", ...)`. The framework
does not generate fallback IDs. If an ID collides, that is a developer error, not a framework
concern. No fallbacks, no auto-generated suffixes, no scanning to resolve ambiguity.

The runtime uses `getElementById` for all plan model class and element resolution. Wide DOM
queries exist in 3 justified locations only:
- `root.ts:25` — discovers `[data-reactive-plan]` script elements at boot
- `inject.ts:16` — discovers plans in dynamically injected HTML
- `retry-indicator.ts:53` — cleans up retry indicator elements by data attribute

Scoped `querySelector` calls exist in `error-display.ts` and `orchestrator.ts` for validation
summary element lookups (generated HTML, not plan components).

If you think you need `querySelectorAll` or DOM traversal for plan component resolution, the
plan is missing information. Fix the C# plan model class to carry it.

## The Plan Contract

C# `Render()` serializes the plan to JSON inside a `<script type="application/json"
data-reactive-plan>` element. The runtime discovers these elements, parses the JSON, merges
partials by `planId`, and boots each composed plan. Sandbox URL: `http://localhost:5220`.

Top-level JSON shape: `version` (3), `planId`, `partId`?, `types`, `components`, `behaviors`.

`WriteOnlyPolymorphicConverter<T>` enables polymorphic serialization by delegating to the
concrete type via `JsonSerializer.Serialize(writer, value, value.GetType(), options)`. Each
concrete plan model class carries its own `kind` property (e.g., `public string Kind => "set"`)
which becomes the discriminator in the JSON, matched by TypeScript discriminated unions.

Schema validation happens in C# tests via `AssertSchemaValid()`. The runtime trusts the
JSON — it does not re-validate. If the JSON is malformed, `JSON.parse` throws at boot time.

## Build & Run

```bash
npm run build:all                # JS bundles + CSS
dotnet build                     # All C# projects
npm run build:api-docs           # API reference from XML docs

npm run watch                    # esbuild watch
npm run watch:css                # Tailwind watch
lsof -ti:5220 | xargs kill -9 2>/dev/null; dotnet run --project Alis.Reactive.SandboxApp

npm run typecheck                # TS type checking
npm run lint                     # ESLint

npm test                                                     # TS vitest (configured, no tests on this branch)
dotnet test tests/Alis.Reactive.UnitTests                    # Core + schema
dotnet test tests/Alis.Reactive.Native.UnitTests             # Native
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests    # Validation
dotnet test tests/Alis.Reactive.Analyzers.Tests              # Analyzers
dotnet test tests/Alis.Reactive.DesignSystem.Tests           # Design system
dotnet test tests/Alis.Reactive.NativeTagHelpers.Tests       # Tag helpers
dotnet test tests/Alis.Reactive.PlaywrightTests \
  --logger "trx;LogFileName=playwright-results.trx" \
  --results-directory TestResults
./scripts/sonar-analyze.sh                                   # SonarQube (Docker)
```

After TS/CSS changes: `npm run build:all && dotnet build`, restart SandboxApp.
All tests pass before every commit. TS tests go in `Alis.Reactive.SandboxApp/Scripts/__tests__/`
with `.test.ts` suffix.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| C# | .NET 10. **C# 8.0 enforced** in 4 library projects (Core, Fusion, Native, FluentValidator). Analyzers uses `latest`. Apps and tests use `latest`. |
| TS | TypeScript 5.8, esbuild ESM, Tailwind CSS v4 |
| Components | Syncfusion EJ2 32.x (Fusion) + Native HTML. Always through DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` |
| Validation | FluentValidation 12.x, extracted to client rules via `FluentValidationAdapter` |
| Tests | NUnit 4.3-4.5, Vitest 3.x + jsdom (configured, no tests yet), Playwright 1.52 |
| Schema | JSON Schema with 61 `$defs`, validated by `JsonSchema.Net` |

## Skills

8 skills in `.claude/skills/`. Load applicable skills BEFORE reading code.

| Skill | Use for |
|-------|---------|
| `reactive-dsl` | Plan, triggers, Element, Dispatch, Component, InputField, .Reactive() |
| `http-pipeline` | Get/Post, Gather, Response, Chained, Parallel, WhileLoading, Into |
| `conditions-dsl` | When/Then/ElseIf/Else, operators, guard composition, source types |
| `validation-rules` | FluentValidation rules, Validate, ValidationErrors, WhenField |
| `onboard-fusion-component` | Adding Syncfusion components, 7-file vertical slice |
| `solid-ts-audit` | SOLID analysis of TypeScript runtime modules |
| `modern-csharp` | C# patterns (needs rewrite — currently promotes C# 12+, repo uses C# 8.0) |
| `bdd-testing` | Playwright BDD tests, 5 rules, 7-behavior contract, blind reviewer |

10 hookify rules in `.claude/hookify.*.local.md` enforce quality gates automatically:
`enforce-csharp8`, `no-public-in-libraries`, `no-raw-inputs`, `bdd-test-enforcement`,
`bdd-public-api-only`, `xml-docs-quality`, `commit-requires-relevant-tests`,
`merge-requires-all-tests` (8 active). `no-js-in-views` and `protect-api-surface` exist
but are currently disabled (`enabled: false`).

## Rules

### 1. Plan Is the Only Contract

No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks — `root.ts` handles discovery and boot automatically.

### 2. New or Changed Primitive — All Layers

1. C# plan model class — sealed class, `internal` constructor
2. Polymorphic registration — `WriteOnlyPolymorphicConverter` delegates to concrete type
3. Builder method — PipelineBuilder, ElementBuilder, or TriggerBuilder
4. JSON schema — failing `AssertSchemaValid()` test drives the update
5. TS types — new interface in `Alis.Reactive.SandboxApp/Scripts/types/`, discriminated union with `kind`
6. Runtime handler — new switch case + `assertNever`
7. C# unit test — `AssertSchemaValid()` validates rendered JSON against schema
8. TS unit test — `Alis.Reactive.SandboxApp/Scripts/__tests__/*.test.ts`, runtime behavior via `boot()`
9. Playwright test — browser behavior with sandbox view
10. Sandbox view — demonstrate the primitive

### 3. Vertical Slices — Duplication Over Abstraction

Each component module is self-contained. No shared base classes for behavior.
Duplication between slices is intentional.

### 4. Vendor Isolation

New component = C# vertical slice with `IInputComponent`. Zero TS runtime changes.
`resolver.ts` is the only module that maps vendor to DOM root (`resolveVendorRoot`) and wires
vendor-specific events (`wireEvent`). Adding a third vendor must only touch `resolver.ts` and
add a `resolution/event-{vendor}.ts` file. Vendor checks in other modules violate this rule.

### 5. Fail Fast — Fallbacks Are Exceptions

Default thinking is throw, not fallback. When something is missing or unknown, surface
the error immediately. Fallbacks hide bugs for hours because wrong values propagate silently.
A fallback is a rare, deliberate, justified exception — never the default response to uncertainty.

**Null escape hatches require justification.** Every NEW per-property `[JsonIgnore(WhenWritingNull)]`,
nullable property declaration, or `?? fallback` you add must be PROVEN necessary by answering
in writing: "Could this be a sentinel/empty/default instead? If yes, why am I taking the shortcut?
If no, what domain meaning would `Empty`/`None` collide with?" Mechanical addition of null markers
during a refactor is the failure pattern from `feedback_null_escape_hatch_blindness.md` — when
removing tech debt, if the count of null markers on ANY surface goes UP, stop and audit each
new marker for sentinel-replaceability before committing.

#### Locked Nullable Properties (1.0 commitment)

P1 (sentinel cleanup) audited every nullable property in `Alis.Reactive/PlanModel/` after the
P1a Shape DU and P1b NoOpReaction refactors. The 19 properties below are LOCKED as
genuinely nullable for 1.0. Each entry has a written rationale explaining why a sentinel would
collide with semantics. Any future PR that adds a nullable plan model property must either fit
this table (with a matching rationale) or convert to a sentinel before merge.

The discipline is: nullable means "genuinely absent as a domain concept." Wherever empty-string,
zero, empty-collection, or a `Xxx.None` singleton would carry a different meaning than absence,
the property stays nullable. Wherever the sentinel is structurally indistinguishable from absence,
the property gets the sentinel.

| File:Line | Property | Type | Locked rationale |
|---|---|---|---|
| `Plan.cs:11` | `Plan.PartId` | `string?` | Only set for partial plans. Empty string would be a distinct partial ID ("the empty-named partial"). |
| `Component.cs:13` | `Component.BindingPath` | `string?` (internal) | Display components have no model binding. Empty string would mean "binding to root" (`m => m`), which is structurally different from "no binding at all." |
| `Component.cs:20` | `Component.ValueMember` | `string?` (internal) | Display components have no value-bearing member. The empty string is not a valid JS property name. |
| `Component.cs:23` | `Component.Container` | `ContainerScope?` | Components outside any form have no container scope. An empty `ContainerScope` (zero components, zero rules) is structurally different from "the component is not inside a form" — the runtime treats container-scoped vs unscoped components differently. |
| `Component.cs:62` | `ComponentValidation.ServerFieldName` | `string?` (internal) | Optional override for the server-side field name. Default behavior (when null) is to use the component ID. Empty string would override to literal `""` which is meaningfully different. |
| `Request.cs:16` | `Request.Container` | `string?` | Requests not scoped to a form have no container. Empty string would be a distinct (broken) container ID. |
| `Request.cs:19` | `Request.Input` | `RequestInput?` | GET/DELETE requests have no body. A `NoBodyInput` sentinel was considered but rejected: GET requests genuinely send no `Content-Type`/`Content-Length`, and a sentinel would force a wire-format decision (`{}` vs `null` vs `{"kind":"none"}`) that has no semantic match in HTTP. |
| `Request.cs:30` | `Request.Next` | `Request?` | Recursive request chaining — null = end of chain. A sentinel `NoNextRequest` would itself need a `.Next`, creating infinite recursion. The chain MUST terminate at null. |
| `Request.cs:65` | `GatherInput.Statics` | `ValueProducer?` (internal) | Optional static fields appended to the gather body. `ValueProducer.None` would technically work, but `Statics` is internal-only and the runtime distinguishes "no static section" from "empty static section" for performance. |
| `Request.cs:151` | `ResponseHandler.Status` | `int?` | HTTP status filter — null = match any status. `0` is technically a valid HTTP status (used for "no response received" / network errors), so 0 ≠ absent. |
| `StartsWhen.cs:29` | `DocumentEventTrigger.PayloadType` | `string?` | Untyped DOM event — null = match any payload. Empty string would mean "match payloadType=='" which is a different filter than "match any payload type." |
| `StartsWhen.cs:56` | `ServerPushTrigger.Event` | `string?` | Server push without event filter — null = match any event from this URL. Empty string would match an event named literally `""`. |
| `StartsWhen.cs:58` | `ServerPushTrigger.PayloadType` | `string?` | Same rationale as `DocumentEventTrigger.PayloadType`. |
| `StartsWhen.cs:74` | `SignalRTrigger.PayloadType` | `string?` | Same rationale as `DocumentEventTrigger.PayloadType`. |
| `Reaction.cs:177` | `DispatchReaction.PayloadType` | `string?` | Untyped dispatch event — null = no payload type tag. Empty string would create a distinct event filter key. |
| `JsType.cs:117` | `JsEvent.PayloadType` | `string?` | Untyped component event registration. Same rationale as the trigger PayloadType properties. |
| `Source.cs:40` | `PayloadSource.Type` | `string` (pre-NRT, JsonIgnore-WhenWritingNull) | Untyped payload source. The pre-NRT declaration style is a P2 cleanup target, but the nullability itself is locked. |
| `Path.cs:35` | `PathSegment.Name` | `string` (pre-NRT, JsonIgnore-WhenWritingNull) | Discriminated pair with `Index` — exactly one is set. `PathSegment.Property("x")` sets `Name`, `PathSegment.AtIndex(3)` sets `Index`. A sentinel would require splitting `PathSegment` into `NamedSegment` / `IndexSegment` subclasses (a separate slice — viable but out of P1 scope). |
| `Path.cs:39` | `PathSegment.Index` | `int?` | Discriminated pair with `Name`. See above. |

**Properties NOT in this table are forbidden to be nullable.** If a future change introduces a
nullable plan model property that is not on this list, the change is wrong and must either:

1. Add the property to this table with a matching rationale (requires user approval and a
   documented sentinel-considered-and-rejected analysis), OR
2. Convert the property to a sentinel (`Xxx.None`, empty collection, empty string, etc.).

The locked table is the single authoritative gate. Reviewers MUST check the table before
approving any PR that adds or modifies a nullable plan model property.

### 6. Plan-Driven IDs — No DOM Scanning

`IdGenerator` generates every element ID from the model expression at C# render time.
The runtime resolves plan components via `getElementById` only. Non-input IDs are the
developer's explicit choice. No fallback IDs. No scanning.

### 7. API Surface Is Frozen

No `public` constructors on plan model classes. All plan model class constructors are
`internal`. All plan model properties use `internal set`. Developers interact through builder
APIs and factory methods (`Html.On`, `Html.InputField`, `p.Get`, `p.When`) — never through
constructors.

#### Locked 1.0 Public Surface (P2 audit)

P2 audited every `public` type in the four library projects. The locked surface is
**351 public types**, organized by category. Each category has a default (KEEP or DEMOTE);
exceptions are listed below. Any future PR that adds a public type in a library project
must either fit a KEEP category or justify a new entry against the rule.

| Category | Project / Pattern | Approx count | Locked default | Rationale |
|---|---|---|---:|---|
| Builder fluent-chain types | `Alis.Reactive/Builders/**/*Builder.cs` (incl. `TriggerBuilder`, `PipelineBuilder`, `ElementBuilder`, `HttpRequestBuilder`, `GatherBuilder`, `ResponseBuilder`, `ParallelBuilder`, `ConditionStart`, `ConditionSourceBuilder`, `BranchBuilder`, `GuardBuilder`, `DispatchPayloadBuilder`, `PluginCallBuilder`, `PluginReadBuilder`, `PluginTypeBuilder`, `TypedSource<>`, `TypedComponentSource<>`, `TypedUrlSource<>`, `TypedPluginSource<>`, `EventArgSource<>`, `PayloadTypedSource<>`) | ~30 | **KEEP public** (sealed) | Returned to consumer code by `Html.On`, `Trigger().DomReady(p => ...)`, `p.Get/Post/Put/Delete`, `p.When`, etc. Constructors are `internal` — consumers obtain instances through factories only. |
| Plan model abstract bases | `Alis.Reactive/PlanModel/Shape.cs` (`Shape`), `ValueProducer.cs` (`ValueProducer`), `Reaction.cs` (`Reaction`), `Condition.cs` (`Condition`), `Source.cs` (`Source`), `Request.cs` (`Request`, `RequestInput`, `ResponseHandler`), `StartsWhen.cs` (`StartsWhen`), `JsType.cs` (`JsType`, `JsEvent`), `Path.cs` (`Path`, `PathSegment`) | 13 | **KEEP public abstract** | Required for STJ polymorphic serialization via `[JsonConverter(typeof(WriteOnlyPolymorphicConverter<T>))]`. Constructors are `private protected`. External consumers cannot derive (no friend-assembly cross-derivation through `private protected`). |
| Plan model concrete subclasses (legacy public-sealed) | `LiteralProducer`, `ReadProducer`, `ObjectProducer`, `ArrayProducer`, `NoneProducer` (in `ValueProducer.cs`); `CompareCondition`, `AllCondition`, `AnyCondition`, `NotCondition`, `ConfirmCondition`, `NoneCondition` (in `Condition.cs`); `SequenceReaction`, `ParallelReaction`, `BranchReaction`, `BranchCase`, `SetReaction`, `CallReaction`, `RequestReaction`, `DispatchReaction`, `InjectReaction`, `ShowValidationErrorsReaction` (in `Reaction.cs`); `ComponentSource`, `PayloadSource`, `PluginSource`, `UrlSource` (in `Source.cs`); `ValueInput` (in `Request.cs`) | ~25 | **KEEP public sealed** (legacy) | Existing convention — `public sealed` since pre-1.0. P1a (`Shape` subclasses → `internal sealed`) and P1b (`NoOpReaction` → `internal sealed`) set the new precedent for newly added subclasses. Converting all existing legacy subclasses is a separate slice (would touch ~25 types and force the audit of every consumer that pattern-matches on them). Locked as-is for 1.0. |
| Plan model concrete subclasses (P1a/P1b new precedent) | `ScalarShape`, `OpaqueShape`, `NoneShape`, `ArrayShape`, `NullableShape`, `ObjectShape`, `NoOpReaction` | 7 | **KEEP internal sealed** | New precedent set by P1a/P1b. Internal subclasses cannot be pattern-matched by external consumers — discipline says the framework owns shape/reaction semantics end-to-end. |
| DSL entry points / view-facing root types | `Alis.Reactive/Html.cs` and `Alis.Reactive/*.cs` root files (`ReactivePlan<TModel>`, `ReactivePlanConfig`, `ResponseBody<T>`, `ComponentRef<TComponent,TModel>`, `IComponent`, `IdGenerator`, `TypedEvent<TArgs>`, `InputBoundField<>`, `InputFieldOptions`) | ~10 | **KEEP public** | Top-level consumer DSL — referenced from views, controllers, `Program.cs`, and component HtmlExtensions. |
| Native components | `Alis.Reactive.Native/Components/**` (HtmlExtensions, Extensions, Args types per slice — 7-file vertical slice × ~10 components) | 71 | **KEEP all public** | Consumer DSL for `.cshtml` views. Each component has `Html.NativeXxx(...)` HtmlExtensions, fluent `Extensions` (`.OnChange`, `.OnBlur`, etc.), and event args types. Removing any breaks consumer view code. |
| Fusion components | `Alis.Reactive.Fusion/Components/**` (HtmlExtensions, Extensions, Args, Methods per Syncfusion component × ~30 components) | 166 | **KEEP all public** | Same as Native — consumer DSL. |
| Native AppLevel + Templates | `Alis.Reactive.Native/AppLevel/**` (Confirm, Toast singletons), `Alis.Reactive.Native/Extensions/**` | ~10 | **KEEP all public** | Consumer DSL singletons. |
| Fusion AppLevel + Templates | `Alis.Reactive.Fusion/AppLevel/**`, `Alis.Reactive.Fusion/Templates/**` | ~9 | **KEEP all public** | Consumer DSL singletons + typed templates for SF components. |
| DesignSystem | `Alis.Reactive/DesignSystem/**` (`TokenMap`, `CssUtils`, `GridCss`, `CardCss`, `ContainerCss`, `DividerCss`, `KvCss`, `TextCss`, `VStackCss`, `HStackCss`, `HeadingCss`, etc.) | ~17 | **KEEP all public** | Used directly in `.cshtml` views for layout and CSS token resolution. |
| Validation cluster | `Alis.Reactive/Validation/*.cs` (`ValidationField`, `ValidationRule`, `IValidationExtractor`, `FieldCondition` + 4 subclasses) | 8 | **KEEP all public** | Exposed via `ReactivePlanConfig.UseValidationExtractor(IValidationExtractor extractor)` — called from consumer `Program.cs` to register the FluentValidation integration. The whole transitive surface (return types, parameter types, property types) must stay public. |
| FluentValidator integration | `Alis.Reactive.FluentValidator/*.cs` (`FluentValidationAdapter`, `ReactiveValidator`, `FieldConditionBuilder`, validators, `IClientConditionSource`) | 11 | **KEEP all public** | Consumer-extensible validator base class and adapter. Consumers extend `ReactiveValidator<TModel>` to define validation rules. |
| Schema serialization helper | `Alis.Reactive/Serialization/WriteOnlyPolymorphicConverter<T>` | 1 | **DEMOTE → internal** (P2) | Used only inside `Alis.Reactive` as a `[JsonConverter]` attribute argument. No consumer reference. Demoted in P2. |
| Internal helpers (P2 demotions) | `ComponentRegistration`, `PlanBuildContext`, `ExpressionPathHelper`, `WriteOnlyPolymorphicConverter<T>` | 4 | **DEMOTE → internal** | Internal builder/registration plumbing with zero consumer references (verified by grep against `Alis.Reactive.SandboxApp` + `examples/`). |
| `IReactionEmitter` interface tightening | `Alis.Reactive/Builders/IReactionEmitter.cs` | 1 | **KEEP public, REMOVE `BuildContext` property** | The interface stays public (used as a parameter type on Fusion event handler extensions like `args.OnFiltering(pipeline)`). Its `BuildContext` property had zero callers — removing it lets `PlanBuildContext` become internal. |

**P2 outcome:**

- 4 types demoted from public to internal (`ComponentRegistration`, `PlanBuildContext`, `ExpressionPathHelper`, `WriteOnlyPolymorphicConverter<T>`)
- 1 interface property removed (`IReactionEmitter.BuildContext`) — zero callers, was the cascade-blocker for `PlanBuildContext`
- Total public surface: **351 types** locked for 1.0 (down from 355 pre-P2)

**Process gate:** any future PR adding a `public` class/interface/struct/enum to `Alis.Reactive` / `Alis.Reactive.Native` / `Alis.Reactive.Fusion` / `Alis.Reactive.FluentValidator` must either:

1. Fit one of the KEEP categories above (with the natural file location), OR
2. Add a new entry to the locked table with a written rationale (requires user approval and a documented "is there a consumer reference?" grep).

Reviewers MUST check the locked table before approving any PR that adds a new public type
in a library project. The `no-public-in-libraries` hookify rule blocks accidental `public`
declarations at edit time as a tripwire, but the locked table is the authoritative gate.

### 8. Root Cause, Not Patch

Trace the full code path. Identify the exact line. Understand WHY before changing WHAT.
If stuck after 2 attempts: research online, save findings, dispatch agents with specific
input and evidence-based output criteria. Fix the root cause. Verify in browser.

A patch is a commit that fixes a symptom without understanding the cause. Ten patches is
ten mistakes. The scout rule applies: leave every file cleaner than you found it. If you
touch a file and see a code smell, fix it — do not walk past broken windows.

### 9. Code Hygiene

These small practices compound. They are not optional style preferences — they prevent
entire categories of bugs and keep the codebase readable under pressure.

**Revealing names over complex conditions.** Extract boolean expressions into named variables
that explain intent. The name is the documentation.

```csharp
// Wrong: condition is opaque
if (field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly)
    ExtractRule(field);

// Right: name reveals intent
var requiresClientValidation = field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly;
if (requiresClientValidation)
    ExtractRule(field);
```

**Variables close to usage.** Declare a variable where it is first needed, not at the top of
the method. Long distances between declaration and usage hide bugs and make code harder to
follow. If a variable is used once, inline it.

**Avoid nesting.** Use early returns and guard clauses to flatten control flow. Deep nesting
hides logic and makes branches hard to trace.

```csharp
// Wrong: nested
if (component != null)
{
    if (component.Vendor == "fusion")
    {
        // 20 lines of logic
    }
}

// Right: guard clause, flat
if (component == null) return;
if (component.Vendor != "fusion") return;
// 20 lines of logic
```

**No dead code.** Delete unused variables, unreachable branches, commented-out code. Do not
keep code "for reference" — git has history. Commented-out code is a lie that rots.

**Small methods, single responsibility.** If a method needs a comment explaining what a
block does, extract that block into a named method. The method name replaces the comment.

### 10. Prefer BDD Vertical Slice Playwright Tests

Every Playwright test is an isolated vertical slice. It tests one user-visible behavior from
page load through interaction to visible outcome.

**Isolated:** Each test navigates to a fresh page. No shared state between tests. No test
ordering. If test B breaks when test A is skipped, test B is broken.

**Vertical:** The test exercises the full stack — C# view renders the plan, runtime boots it,
user interacts, browser reflects the outcome. No mocking. No `page.evaluate()`. No shortcuts.
Framework tests that verify the gather pipeline may assert on `request.PostData` — this is
the one justified exception where asserting on non-visible data is correct.

**Behavior-first:** The test name describes what the user sees, not what the code does.
`selecting_care_level_updates_billing_amount` not `domready_trigger_fires_sequential_reaction`.

Load the `bdd-testing` skill before writing any Playwright test. The 5 BDD rules and the
7-behavior contract per input component are defined there.

### 11. Quality Aspirations

Known weaknesses tracked for improvement:

- **DDD depth**: Domain model uses `null` where Value Objects with constructor invariants
  should enforce valid state. Association and aggregation boundaries are implicit.
  Screaming names (types that express domain intent) are underused.
- **Serialization**: `[JsonIgnore(Condition = WhenWritingNull)]` attributes scattered
  across plan model classes instead of explicit serialization contracts.
- **TS tracing**: `core/trace.ts` is 38 lines using `console.error`/`warn`/`log` dispatched
  by level. Should aspire to OTel-style structured tracing — explicit data flowing through
  modules, correlation IDs, proper span context, actionable error messages.

### 12. Git Worktrees for Feature Work

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

## Process

### Prompt Clarity Gate
Prompt must be clear and specific. If vague — stop, ask, push back, propose a checklist.

### Plan — Identify Layers
1. Which layers does this touch?
2. Load applicable skills FIRST, before reading code.
3. Survey code at each layer. Read, don't guess.
4. Build a master task index (INVEST: Independent, Negotiable, Valuable, Estimable, Small, Testable).

### Thoughtful Editing
Before editing: understand the code path and blast radius. Design your strategy.
If editing the same file multiple times, rethink your approach and design choices.

### Wrong Plan Protocol
If touching an unexpected layer, the plan or task is wrong. Stop, save learnings, return to planning.

### Pre-flight
- [ ] Loaded skills? Read source code at each layer touched?
- [ ] Understood WHY the current code works the way it does?
- [ ] Visibility (`internal`/`public`) chosen deliberately? API surface unchanged?
- [ ] Input evidence: what proves this change is needed?

### Post-flight
- [ ] All tests pass? Verified in actual browser?
- [ ] Each boundary crossing driven by a failing test?
- [ ] Root cause fixed, not a patch? No code smells?
- [ ] Output evidence: what proves this change is correct?
- [ ] Coverage matrix: every item in scope mapped to a test or justified as untestable?

### Review Loop — Every Change

Each change cycles through three gates with team sign-off:

1. **Plan Review**: Post plan → reviewers verify against code → fix findings → sign-off
2. **Implementation Review**: Post diff → reviewers verify against plan + actual code → sign-off
3. **Post-Implementation**: All tests pass → browser verified → no broken cross-references
