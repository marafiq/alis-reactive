# Surgical Plan: Expression Walker Conditions — 1.0 Final

## Blast Radius

```
57 C# files + 17 cshtml views + 3 TS/JSON files = 77 files total

DELETE:  7 condition builder classes
ADD:    12 walker + ComponentRef + If/ElseIf/Else files
MODIFY: 58 files (tests, views, extensions, descriptors, runtime)
```

---

## Dependency Map

```
TypedSource<TProp> (abstract)
├── TypedComponentSource<TProp>
│   ├── Used by: 11 component *Extensions.cs (.Value() returns this)
│   ├── Used by: PipelineBuilder.When(TypedSource)     → DELETED (replaced by If)
│   ├── Used by: ElementBuilder.SetText(TypedSource)   → replaced by SetText(ComponentRef)
│   └── Used by: ConditionSourceBuilder                → DELETED
├── EventArgSource<TPayload, TProp>
│   ├── Used by: PipelineBuilder.When(payload, expr)   → DELETED (replaced by If)
│   └── NOT used by ElementBuilder (has separate overload with expression)
└── ConditionSourceBuilder, GuardBuilder, BranchBuilder, ConditionStart → ALL DELETED

ComponentRef<TProp> (NEW — replaces TypedComponentSource)
├── Has: ToBindSource() → ComponentSource  (for ElementBuilder.SetText)
├── Has: .Value → TProp                    (for expression tree conditions)
├── Created by: per-component .Value() extensions (same pattern, different return type)
└── Internal ctor — devs use Component<T>(m => m.Prop).Value()
```

---

## Steps (dependency-ordered, each leaves codebase green)

### Step 1: Add walker infrastructure (0 existing files modified)

**Add 12 new files:**
```
Alis.Reactive/Walker/
├── GuardExpressionVisitor.cs
├── SourceResolver.cs
├── ConstantEvaluator.cs
├── OperatorMapper.cs
├── CompositionFlattener.cs
└── Handlers/
    ├── MethodCallHandler.cs
    ├── StringMethodHandler.cs
    ├── NullOrEmptyHandler.cs
    ├── RegexHandler.cs
    └── MembershipHandler.cs
Alis.Reactive/ComponentRef.cs           (internal ctor, ToBindSource())
Alis.Reactive/ExpressionExtensions.cs   (In, NotIn, Between markers)
```

**Quality gate:** `dotnet build` — compiles. No tests affected (nothing references new code yet).

### Step 2: Add ValueGuard.RightSource (1 file modified)

**Modify:** `Alis.Reactive/Descriptors/Guards/ValueGuard.cs`
- Add `public BindSource? RightSource { get; private init; }` with `[JsonIgnore(WhenWritingNull)]`
- Add `static ValueGuard SourceVsSource(...)` factory

**Quality gate:** All 907 existing tests pass (additive, nullable, ignored when null).

### Step 3: Add If/ElseIf/Else to PipelineBuilder (2 new files)

**Add:**
- `Alis.Reactive/Builders/PipelineBuilder.If.cs` — partial class, `If()` overloads
- `Alis.Reactive/Builders/Conditions/ExprBranchBuilder.cs` — `ElseIf()`, `Else()`, `End()`

**Quality gate:** `dotnet build`. Old syntax still works. Both coexist.

### Step 4: Change .Value() return type on all components (11 files modified)

**Modify each component's *Extensions.cs:**
```
NativeCheckBoxExtensions.cs          → .Value() returns ComponentRef<bool>
NativeTextBoxExtensions.cs           → .Value() returns ComponentRef<string>
NativeDatePickerExtensions.cs        → .Value() returns ComponentRef<DateTime>
NativeDropDownExtensions.cs          → .Value() returns ComponentRef<string>
FusionNumericTextBoxExtensions.cs    → .Value() returns ComponentRef<decimal>
FusionComboBoxExtensions.cs          → .Value() returns ComponentRef<string>
FusionDropDownListExtensions.cs      → .Value() returns ComponentRef<string>
FusionDatePickerExtensions.cs        → .Value() returns ComponentRef<DateTime>
FusionTimePickerExtensions.cs        → .Value() returns ComponentRef<DateTime>
FusionAutoCompleteExtensions.cs      → .Value() returns ComponentRef<string>
FusionMultiSelectExtensions.cs       → .Value() returns ComponentRef<string>
FusionMultiColumnComboBoxExtensions.cs → .Value() returns ComponentRef<string>
TestWidgetSyncFusionExtensions.cs    → .Value() returns ComponentRef<string>
```

**Also modify:** `ElementBuilder.cs`
- Change `SetText<TProp>(TypedSource<TProp>)` → `SetText<TProp>(ComponentRef<TProp>)` (calls `.ToBindSource()`)
- Change `SetHtml<TProp>(TypedSource<TProp>)` → `SetHtml<TProp>(ComponentRef<TProp>)` (calls `.ToBindSource()`)

**Quality gate:** `dotnet build` — existing `.Value()` call sites don't change syntax, only return type. Tests that assert `Is.TypeOf<TypedComponentSource<decimal>>()` will fail — fixed in Step 6.

### Step 5: Update runtime for rightSource (3 files modified)

**Modify:**
- `Scripts/types.ts` — add `rightSource?: BindSource` to `ValueGuard`
- `Scripts/conditions.ts` — 3 lines: if `rightSource`, resolve it instead of literal `operand`
- `Schemas/reactive-plan.schema.json` — add `rightSource` to ValueGuard `$defs`

**Quality gate:** `npm test` — all TS tests pass. `npm run build` — bundle builds.

### Step 6: Convert ALL condition tests to new syntax (19 test files)

**Core condition tests (4 files — rewrite):**
```
tests/Alis.Reactive.UnitTests/Conditions/
├── WhenBranchingOnConditions.cs              → rewrite all When/Then to If/ElseIf/Else
├── WhenConditionReadsComponent.cs            → rewrite to use ComponentRef<TProp>
├── WhenExecutingAConditionalWorkflow.cs      → rewrite branching syntax
└── WhenUsingConditionsInEveryDslSurface.cs   → rewrite OnSuccess/OnError conditions
```

**Component tests that assert TypedComponentSource (6 files — update assertions):**
```
tests/Alis.Reactive.Fusion.UnitTests/Components/
├── FusionNumericTextBox/WhenMutatingAFusionNumericTextBox.cs     → TypeOf<ComponentRef<decimal>>
├── FusionNumericTextBox/WhenUsingNumericTextBoxFullApi.cs        → TypeOf<ComponentRef<decimal>>
├── FusionDropDownList/WhenMutatingAFusionDropDownList.cs         → TypeOf<ComponentRef<string>>
├── FusionDropDownList/WhenUsingDropDownListFullApi.cs            → TypeOf<ComponentRef<string>>
tests/Alis.Reactive.Native.UnitTests/Components/
├── NativeDropDown/WhenMutatingANativeDropDown.cs                → TypeOf<ComponentRef<string>>
```

**Component reactive event tests that use When() (6 files — rewrite conditions):**
```
FusionNumericTextBox/WhenReactingToFusionNumericTextBoxEvents.cs
FusionNumericTextBox/WhenWiringFusionReactiveExtension.cs
FusionDropDownList/WhenReactingToFusionDropDownListEvents.cs
FusionDropDownList/WhenWiringFusionDropDownListReactiveExtension.cs
NativeDropDown/WhenReactingToNativeDropDownEvents.cs
NativeDropDown/WhenWiringNativeReactiveExtension.cs
NativeDatePicker/WhenReactingToNativeDatePickerEvents.cs
```

**Architecture + analyzer tests (3 files — update references):**
```
tests/Alis.Reactive.UnitTests/Architecture/WhenEnforcingPipelineRules.cs
tests/Alis.Reactive.UnitTests/Architecture/WhenRejectingNonSequentialInCommandListSurfaces.cs
tests/Alis.Reactive.Analyzers.Tests/WhenDetectingIncompleteConditionalChains.cs
```

**FluentValidator tests (2 files — update condition syntax):**
```
tests/Alis.Reactive.FluentValidator.UnitTests/WhenExtractingConditionalRules.cs
tests/Alis.Reactive.FluentValidator.UnitTests/WhenExtractingClientConditionalRules.cs
```

**Quality gate:** All C# tests pass — `dotnet test` on all 5 test projects.

### Step 7: Convert ALL sandbox views (17 cshtml files)

Convert every `When().Gte().Then()` to `If(() => expr, t => ...)`:
```
Areas/Sandbox/Views/Conditions/Index.cshtml
Areas/Sandbox/Views/PlaygroundSyntax/ReactiveConditions.cshtml
Areas/Sandbox/Views/NumericTextBox/Index.cshtml
Areas/Sandbox/Views/DropDownList/Index.cshtml
Areas/Sandbox/Views/CheckBox/Index.cshtml
Areas/Sandbox/Views/NativeDropDown/Index.cshtml
Areas/Sandbox/Views/NativeDatePicker/Index.cshtml
Areas/Sandbox/Views/NativeTextBox/Index.cshtml
Areas/Sandbox/Views/TimePicker/Index.cshtml
Areas/Sandbox/Views/FusionDatePicker/Index.cshtml
Areas/Sandbox/Views/AutoComplete/Index.cshtml
Areas/Sandbox/Views/MultiSelect/Index.cshtml
Areas/Sandbox/Views/MultiColumnComboBox/Index.cshtml
Areas/Sandbox/Views/TestWidget/Index.cshtml
Areas/Sandbox/Views/Validation/Index.cshtml
Areas/Sandbox/Views/ValidationContract/ConditionalHide.cshtml
Areas/Sandbox/Views/ValidationContract/AjaxPartial.cshtml
```

**Quality gate:** `dotnet build` on SandboxApp. All Playwright tests pass (186 tests).

### Step 8: Update FluentValidator + Analyzer source (4 files)

**Modify:**
- `Alis.Reactive.FluentValidator/ReactiveValidator.cs` — update condition references
- `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs` — update TypedComponentSource refs
- `Alis.Reactive.Analyzers/IncompleteConditionalChainAnalyzer.cs` — update for If/ElseIf/Else pattern

**Quality gate:** FluentValidator tests (43) + Analyzer tests pass.

### Step 9: Delete old condition builder classes (7 files deleted)

**Delete:**
```
Alis.Reactive/Builders/Conditions/ConditionSourceBuilder.cs
Alis.Reactive/Builders/Conditions/GuardBuilder.cs
Alis.Reactive/Builders/Conditions/BranchBuilder.cs
Alis.Reactive/Builders/Conditions/ConditionStart.cs
Alis.Reactive/Builders/Conditions/TypedSource.cs
Alis.Reactive/Builders/Conditions/TypedComponentSource.cs
Alis.Reactive/Builders/PipelineBuilder.Conditions.cs
```

**Quality gate:** `dotnet build` — zero references to deleted types. ALL tests pass.

### Step 10: Add walker unit tests + source-vs-source BDD tests

**Add:**
- Walker unit tests (ported from experiment — 145+ tests)
- Source-vs-source sandbox view + Playwright tests
- Multiple typed reads BDD tests

**Quality gate:** Full test suite — 907 existing + ~150 new walker tests.

### Step 11: Update CLAUDE.md

- Document `If/ElseIf/Else` syntax
- Document `ComponentRef<TProp>`
- Document expression walker architecture
- Remove all references to When/Then/ConditionSourceBuilder/GuardBuilder
- Update test counts
- Update feedback loop

**Quality gate:** CLAUDE.md matches reality. No stale references.

---

## Final Test Counts (estimated)

| Suite | Before | After |
|-------|--------|-------|
| TS unit tests | 432 | 432 (unchanged + rightSource tests) |
| C# unit tests (core) | 150 | ~150 (conditions rewritten, same count) |
| C# unit tests (native) | 35 | ~35 (conditions rewritten) |
| C# unit tests (fusion) | 61 | ~61 (conditions rewritten) |
| C# unit tests (FluentValidator) | 43 | ~43 (conditions rewritten) |
| Walker unit tests | 0 | **+150 NEW** |
| Playwright | 186 | 186 (unchanged + source-vs-source page) |
| **Total** | **907** | **~1060** |

---

## Files Touched Summary

| Category | Count | Action |
|----------|-------|--------|
| New walker + infrastructure | 12 | ADD |
| New builder surface | 2 | ADD |
| New walker tests | ~10 | ADD |
| Component extensions | 13 | MODIFY (return ComponentRef) |
| ElementBuilder | 1 | MODIFY (accept ComponentRef) |
| ValueGuard | 1 | MODIFY (add RightSource) |
| C# test files | 19 | MODIFY (rewrite conditions) |
| Sandbox views | 17 | MODIFY (rewrite conditions) |
| FluentValidator source | 2 | MODIFY |
| Analyzer source | 1 | MODIFY |
| TS runtime | 2 | MODIFY (3 lines + type) |
| JSON schema | 1 | MODIFY |
| CLAUDE.md | 1 | MODIFY |
| Old condition builders | 7 | DELETE |
| **Total** | **~89** | |

---

## Rollback

If any step fails and can't be fixed forward:
- Steps 1-3 are purely additive — delete new files
- Steps 4+ modify existing code — git revert to pre-migration commit

Every step is on a single branch. One commit per step. Each commit leaves codebase green.

---

## What Stays Unchanged

- `p.Confirm("msg").Then(...).Else(...)` — stays on builder (not a boolean expression)
- `el.When(args, x => x.Score, csb => csb.Gte(90))` — per-action When stays on ElementBuilder
- All mutation DSL: Element, Dispatch, Component registration, HTTP, Gather, Validation
- All descriptors: Guard, ValueGuard, AllGuard, Command, Reaction, Entry
- JS runtime behavior (except rightSource addition)
- All TS tests, all Playwright tests (verify same behavior)
