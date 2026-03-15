# Framework-Level Architecture Review

## Scope

This pass reviews the current framework architecture across C#, runtime, components, MVC seams, and proof quality.

Excluded from scope:

- `tools/`

Hard rules used for this review:

- the public DSL is frozen
- no feature reduction
- fixes should improve contract truthfulness, not shift burden back to app authors

## Verdict

The framework is directionally strong, but it still has a few contract-boundary gaps where the architecture claim is stronger than the implementation.

The strongest remaining issues are not "the DSL is bad" issues. They are:

- validation truthfulness
- logical plan identity
- component registration ownership
- render/finalization ownership
- request lifecycle truthfulness
- proof quality for the architecture claims

## Current Framework-Level Issues

### 1. Validation still fails open instead of behaving like an authoritative contract

Severity: `High`

Files and symbols:

- `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` `Validate<TValidator>()`
- `Alis.Reactive/Resolvers/ValidationResolver.cs`
- `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`
- `Alis.Reactive.SandboxApp/Scripts/boot.ts` `enrichValidationFields()`
- `Alis.Reactive.SandboxApp/Scripts/validation.ts` `validate()`, `evalCondition()`

Why this is a real framework issue:

- `.Validate<TValidator>()` is an explicit framework contract, not an optional hint.
- today, missing extraction, missing form containers, unresolved field bindings, and missing conditional sources can degrade into warning/skip/pass behavior
- that means the plan can claim "this request validates" while the runtime path still succeeds without executing the declared contract truthfully

Evidence in current code:

- `Validate<TValidator>()` starts with a placeholder descriptor
- `ValidationResolver` only replaces it if extraction succeeds
- `FluentValidationAdapter` returns `null` when no extractor instance or no fields are produced
- `validation.ts` returns success when the form container is missing
- `validation.ts` skips fields with missing `fieldId` / `vendor` / `readExpr`
- `evalCondition()` returns `true` when the source field cannot be resolved

Why it matters architecturally:

- the framework says validation is part of the request contract
- fail-open behavior turns that into best effort
- once that happens, boot/merge enrichment is no longer a truthful execution phase; it becomes a soft suggestion layer

Recommended direction without changing the DSL:

- unresolved validation should remain an explicit internal state, not silently degrade
- active validation surfaces should fail closed when declared fields cannot be executed
- inactive partial-owned fields should be modeled intentionally, not treated as generic skips
- server and client validation should both ride on an authoritative validation payload/descriptor contract

Related existing issue:

- `006-validation-contract-still-fails-open-and-loses-fidelity.md`

### 2. Logical plan identity is still weaker than the architecture it is trying to represent

Severity: `High`

Files and symbols:

- `Alis.Reactive/IReactivePlan.cs` `ReactivePlan<TModel>.PlanId`
- `Alis.Reactive.Native/Extensions/PlanExtensions.cs` `ResolvePlan()`
- `Alis.Reactive/Schemas/reactive-plan.schema.json`
- `Alis.Reactive.SandboxApp/Scripts/merge-plan.ts` `composeInitialPlans()`

Why this is a real framework issue:

- runtime merge identity is still rooted in `typeof(TModel).FullName`
- that is a useful default, but it is not the same thing as a true logical plan boundary
- two independent reactive surfaces using the same CLR model type can still collapse into the same browser plan boundary by convention

Evidence in current code:

- `ReactivePlan<TModel>.PlanId` is still `typeof(TModel).FullName!`
- the schema documents `planId` as the runtime merge key
- `ResolvePlan<TModel>()` returns a new `ReactivePlan<TModel>()`, relying on same-model identity rather than an explicit logical parent boundary
- `composeInitialPlans()` merges by `planId`

Why it matters architecturally:

- the framework now has real merge behavior, source ownership, validation enrichment, and partial lifecycle
- those features deserve a stronger identity model than "same model type means same logical plan"
- otherwise, correctness depends on convention instead of an explicit framework-owned boundary

Recommended direction without changing the DSL:

- keep `Html.ReactivePlan<TModel>()` and `Html.ResolvePlan<TModel>()` exactly as they are
- introduce an internal logical plan identity model separate from raw CLR type identity
- let root-plan and same-model partial semantics map into that stronger internal identity

### 3. Component registration is still last-write-wins instead of an enforced contract

Severity: `High`

Files and symbols:

- `Alis.Reactive/IReactivePlan.cs` `AddToComponentsMap()`
- model-bound builders across component slices, for example:
- `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxBuilder.cs`
- `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs`
- `Alis.Reactive.SandboxApp/Scripts/merge-plan.ts`

Why this is a real framework issue:

- `ComponentsMap` is no longer an incidental helper
- it now drives validation enrichment, gather resolution, and component-value reads
- but registration still silently overwrites by binding path

Evidence in current code:

- `AddToComponentsMap()` assigns `_componentsMap[bindingPath] = entry`
- slice builders repeat manual registration patterns
- merge logic can rebuild active state, but the base registration contract is still overwrite-by-convention

Why it matters architecturally:

- if two slices register the same binding path differently, the framework changes behavior silently
- that is especially dangerous because the registration map is part of the rendered contract, not just a local convenience

Recommended direction without changing the DSL:

- centralize registration in an internal primitive
- detect conflicting registrations for the same binding path
- if overlapping ownership is unsupported, fail explicitly instead of overwriting silently

### 4. Plan construction is still not truly builder-owned and `Render()` still performs graph finalization

Severity: `High`

Files and symbols:

- `Alis.Reactive/IReactivePlan.cs` `AddEntry()`, `AddToComponentsMap()`, `Render()`, `ResolveAll()`
- `Alis.Reactive/Descriptors/Entry.cs`

Why this is a real framework issue:

- the stated architecture says the DSL builds descriptors and render serializes them
- in practice, the public plan interface still exposes low-level mutation and render-time resolution still mutates request descriptors

Evidence in current code:

- callers can still push raw `Entry` objects directly through `AddEntry()`
- callers can still push raw component registrations directly through `AddToComponentsMap()`
- `Render()` still runs `ResolveAll()`
- `ResolveAll()` drives validation resolution over the built graph before serialization

Why it matters architecturally:

- it weakens the claim that descriptors are finalized by the builders
- it makes rendering part serializer, part compiler, part mutation phase
- it also lets proof pages and custom app code bypass vertical-slice APIs

Recommended direction without changing the DSL:

- keep the external authoring DSL unchanged
- separate internal mutable construction/finalization from the public plan-facing contract
- make the finalized plan graph explicit before serialization

### 5. `WhileLoading` is modeled as a lifecycle feature but executes as one-way user commands

Severity: `Medium-High`

Files and symbols:

- `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` `WhileLoading()`
- `Alis.Reactive.SandboxApp/Scripts/http.ts` `execRequest()`

Why this is a real framework issue:

- the C# builder describes `WhileLoading` as request-in-flight behavior that is reverted after the response arrives
- the runtime simply executes commands up front and leaves cleanup entirely to user-authored success/error branches

Evidence in current code:

- builder comment says loading commands are reverted after response
- runtime comment says revert is caller responsibility
- the HTTP executor has no framework-owned `finally` cleanup lane for loading state

Why it matters architecturally:

- this makes an advertised lifecycle primitive behave like convention-based scripting
- loading cleanup can be missed on unmatched statuses, network failures, or future branching expansions

Recommended direction without changing the DSL:

- keep `.WhileLoading(...)` exactly as authored today
- make it a real request lifecycle phase inside the HTTP executor
- framework should deterministically end loading state when the request lifecycle ends

### 6. The server validation lane still accepts arbitrary object-shaped payloads as field errors

Severity: `Medium-High`

Files and symbols:

- `Alis.Reactive.SandboxApp/Scripts/commands.ts` `validation-errors`
- `Alis.Reactive.SandboxApp/Scripts/validation.ts` `showServerErrors()`, `extractErrors()`

Why this is a real framework issue:

- `ValidationErrors(formId)` looks like a dedicated server-validation contract
- but runtime currently treats almost any object-shaped payload as an error map

Evidence in current code:

- `commands.ts` routes `ctx.responseBody` into `showServerErrors()`
- `extractErrors()` accepts either `{ errors: ... }` or the whole object directly
- tests explicitly support flat object formats too

Why it matters architecturally:

- a validation lane should not reinterpret arbitrary error bodies as validation structures
- otherwise the framework contract becomes "whatever happened to look object-shaped"

Recommended direction without changing the DSL:

- keep `ValidationErrors(formId)` unchanged
- tighten the accepted runtime payload shape
- make non-validation payloads explicit failures for that command path

### 7. The architecture showcase page bypasses the slice boundaries it claims to prove

Severity: `Medium`

Files and symbols:

- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Architecture/Index.cshtml`

Why this is a real framework issue:

- this page is meant to prove the framework architecture
- but some of its most important cases are built using raw `AddToComponentsMap(...)`, manual `Entry(...)`, and manual `ComponentEventTrigger(...)`
- that means the proof can stay green even if real vertical slices are broken

Evidence in current code:

- manual component registrations on the architecture page
- manual `PipelineBuilder` plus raw descriptor assembly for event cases

Why it matters architecturally:

- proof pages should exercise the same slice APIs real authors use
- otherwise the framework can accidentally prove only its low-level escape hatches

Recommended direction without changing the DSL:

- keep the page and scenarios
- rewrite the architecture proof through the real component builders and `.Reactive(...)` surfaces wherever possible

### 8. The architecture tests still prove "effect happened" more often than "contract was correct"

Severity: `Medium`

Files and symbols:

- `tests/Alis.Reactive.PlaywrightTests/Architecture/WhenExercisingComponentArchitecture.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/ArchitectureController.cs`
- `Alis.Reactive.SandboxApp/Scripts/__tests__/architecture-enforcement.test.ts`
- `tests/Alis.Reactive.UnitTests/Architecture/WhenEnforcingArchitectureRules.cs`

Why this is a real framework issue:

- some architecture tests still assert sentinel text such as `"gathered"` rather than exact echoed values, exact request payloads, or exact plan JSON shape
- the current proof is stronger than ordinary smoke tests, but still weaker than the architecture claims being made

Evidence in current code:

- architecture controller echoes request data
- architecture page often collapses success into fixed labels
- Playwright tests assert those labels
- TS "architecture enforcement" checks mostly scan for forbidden strings
- .NET architecture tests mostly enforce sealed descriptors and similar broad rules

Why it matters architecturally:

- the framework claims vendor-neutral reads, normalized contracts, deterministic plan execution, and slice isolation
- those claims should be proved directly, not inferred from a successful side effect

Recommended direction without changing the DSL:

- strengthen tests around exact payloads, exact plan shape, and exact event payload normalization
- add proof for component registration correctness and architecture-page slice fidelity

## Short Summary

The framework is not in bad shape.

The core loop is still coherent:

- C# builds a plan
- plan serializes to JSON
- runtime executes the plan

The remaining framework-level gaps are mostly where that loop is still weaker than the repo claims:

- validation can still degrade instead of enforcing
- logical plan identity is still convention-heavy
- component registration is not yet a hard contract
- request lifecycle semantics are not always owned by the runtime
- some proof surfaces still bypass or under-test the architecture they are meant to guarantee

## Most Important Next Moves

If I had to prioritize the next framework-level work without changing the DSL, I would do it in this order:

1. close validation fail-open behavior fully
2. harden logical plan identity and ownership rules
3. make component registration conflict-safe
4. make `WhileLoading` a true lifecycle primitive
5. strengthen architecture proof to validate exact contracts rather than sentinel outcomes
