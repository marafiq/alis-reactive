# Validation Architecture

This document describes the current validation design. Keep it aligned with
`docs/reactive-plan-domain-language.md`.

## Boundary

FluentValidation remains the server authority. Alis Reactive extracts only the
deterministic client-side projection that can be represented in the Reactive
Plan and executed in the browser runtime.

Unsupported browser projections are not guessed. They are recorded in
`ValidationExtractionReport.SkippedClientRules` with a reason, while the server
rule still runs normally on postback or HTTP submit.

## Data Flow

```text
Validate<TValidator>(containerId)
  -> RequestValidation registers a ValidationJob
  -> ReactivePlan.Render() calls ResolveAll()
  -> input component registrations are materialized into the plan
  -> ClientValidationProjectionBinder binds queued validation jobs
  -> Component.container.validationRules carries browser validation intent
  -> HTTP runtime validates the container before dispatch
```

## C# Plan Side

`ConfiguredRequestValidation.Register(...)` records a `ValidationJob` containing:

- request URL, used for diagnostics;
- validation container id;
- validator type to project.

`ClientValidationProjectionBinder` then:

1. Requires a registered `IValidationExtractor`.
2. Calls `Extract(ValidationExtractionRequest.For(validatorType, container))`.
3. Binds each `ClientValidationField` through `ValidationProjectionBindingScope`.
4. Merges the resulting `ComponentValidation` rules onto the validation-container component.

Field binding has two deterministic paths:

- Registered input fields use the rendered component id, value member, and shape from `ComponentRegistration`.
- Deferred fields resolve model shape from the root model and use the deterministic component id a partial will render later.

## FluentValidation Adapter

`FluentValidationAdapter` translates supported FluentValidation validators into
client validation projections. The adapter builds a `ClientValidationProjectionDraft`
while walking a validator: projected rules are attached to a field path, and
unproven browser rules are recorded as skipped projections with a reason.

Custom validators can opt in through
`ProjectToClient(...)`, which attaches an explicit browser rule projection to the
FluentValidation rule component.

Conditions are projected only when the validator supplies a matching symbolic
client guard through the ReactiveValidator `WhenField*` language. Server-only
conditions are skipped for the browser projection instead of being inferred from
FluentValidation internals.

Nested `WhenField*` scopes project as one active client condition. A single
scope keeps its guard directly; multiple active scopes are composed with `all`
in the same outer-to-inner order the server predicates use.

If a rule is declared under both a `WhenField*` guard and a server-only
FluentValidation `When`/`Unless` scope, the browser projection is skipped. The
client guard would be only a partial activation, so the adapter records a
skipped client projection instead of guessing the missing predicate.

## Runtime Side

`RequestValidationGate` runs before HTTP dispatch. If a request has
`validation.kind === "container"`, it calls:

```ts
validateContainer(plan, validation.container, context)
```

`validateContainer` resolves the validation container from `RuntimePlan`, clears
current errors, evaluates each `ComponentValidation`, and routes failures to
inline spans or the plan-level summary.

Server validation errors are displayed separately through
`showServerErrors(plan, container, data)`. Runtime maps server errors by
`serverFieldName`; component keys are not fallback field names.

## Partial Lifecycle

Partial load can contribute validation rules to a root-owned validation
container. Partial unload removes only the exact rule objects contributed by
that partial slot. It must not delete root validation rules or layout-owned app
components.

## Key Types

| Area | Types |
| --- | --- |
| Request gate | `RequestValidation`, `ValidationJob`, `RequestValidationTarget` |
| Extraction contract | `IValidationExtractor`, `ValidationExtractionRequest`, `ValidationExtractionReport` |
| Projection binding | `ClientValidationProjectionBinder`, `ValidationProjectionBindingScope`, `ValidationFieldBinding` |
| Plan payload | `ComponentValidation`, `ValidationRuleExecution`, `ValidationRuleOperand`, `ValidationRuleActivation` |
| Runtime execution | `validateContainer`, `showServerErrors`, `RuntimeValidationActivation`, `RuntimeValidationPeerOperand`, `rule-engine.ts` |

## Design Rules

- Do not call client projection “server rule extraction.”
- Do not use null as behavior; `none`, missing component, and literal `null` are distinct cases.
- Do not infer custom FluentValidation behavior from implementation details; require `ProjectToClient(...)`.
- Do not create a separate validation read path in runtime; validation reads component values through the same declared object/member contract as gather and reactions.
