# Validation Architecture

This document describes the current validation design. Keep it aligned with
`docs/reactive-plan-domain-language.md`.

## Boundary

FluentValidation still executes normally on submit or HTTP endpoints. Alis
Reactive projects only the deterministic client-side rules that can be
represented in the Reactive Plan and executed in the browser runtime.

Unsupported browser projections are not guessed or emitted into the plan. The
original FluentValidation rule still runs normally on postback or HTTP submit.

## Data Flow

```text
Validate<TValidationSource>(containerId)
  -> RequestValidation registers a ValidationJob
  -> ReactivePlan.Render() calls ResolveAll()
  -> input component registrations are materialized into the plan
  -> ClientValidationProjectionBinder binds queued validation jobs
  -> Component.container.validationRules carries browser validation rules
  -> HTTP runtime validates the container before dispatch
```

## C# Plan Side

`ConfiguredRequestValidation.Register(...)` records a `ValidationJob` containing:

- request URL, used for diagnostics;
- validation container id;
- validation source type to project.

`ClientValidationProjectionBinder` then:

1. Requires a registered `IClientValidationProjectionSource`.
2. Calls `ProjectClientRules(validationSourceType)`.
3. Binds each `ClientValidationField` through `ValidationFieldBindingCatalog`.
4. Merges the resulting `ComponentValidation` rules onto the validation-container component.

Field binding has two deterministic paths:

- Registered input fields use the rendered component id, value member, and shape from `ComponentRegistration`.
- Deferred fields use the projection's declared field shape and the deterministic component id a partial will render later.

## Core Projection Registry

`ClientValidationProjections` is the core-owned projection source for
deterministic browser validation rules that are authored directly, without
FluentValidation inspection. It keys projections by the validation source type
named by `Validate<TValidationSource>()`, but public authoring selects fields through
typed expressions and `ClientValidationFieldToken<TModel, TValue>`, not field
name strings.

Registry-authored fields carry their projected shape into render-time binding.
That lets deferred partial fields bind through the same deterministic component
id policy without reflecting over the model just to rediscover the field type.
Peer fields and condition fields are also entered into the projection so their
component value contracts can be resolved by the same binding path as ordinary
rules.

## ReactiveValidator Metadata

`ReactiveValidator<T>` records browser validation metadata explicitly through
`ClientRule(...)`. FluentValidation remains the server authority: `RuleFor`,
`Must`, regular `When`/`Unless`, and async rules still run on the server as normal.

Browser rules are emitted only when the validator declares `ClientRule(...)`.
Peer comparisons are declared through typed expressions such as `EqualTo(...)`,
`NotEqualTo(...)`, and `GreaterThan(...)`; the framework does not infer peer
paths from FluentValidation internals.

Conditions are browser-visible only through the ReactiveValidator `WhenField*`
language. Regular FluentValidation guards are server-only. Nested `WhenField*`
scopes emit one active client condition; multiple active scopes compose with
`all` in the same outer-to-inner order the server predicates use.

After render-time field binding, client guards become `ValidationCondition`,
the deterministic plan condition subset used by validation activation. It
supports compare/all/any/not over declared value producers and excludes
`Confirm`, which belongs to reactive branches because prompts cross into the
async execution lane.

If a rule is declared under both a `WhenField*` guard and a FluentValidation
`When`/`Unless` guard outside the client projection language, the browser
projection is skipped. The client guard would be only a partial activation, so
the adapter omits the browser rule instead of guessing the missing predicate.

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

Partial load can add validation rules to a root-owned validation container.
Partial unload removes only the exact rule objects loaded by that slot. It must
not delete root validation rules or layout-owned app components.

## Key Types

| Area | Types |
| --- | --- |
| Request gate | `RequestValidation`, `ValidationJob`, `RequestValidationTarget` |
| Projection contract | `IClientValidationProjectionSource.ProjectClientRules(Type)` returning `IReadOnlyList<ClientValidationField>` |
| Core projection source | `ClientValidationProjections`, `ClientValidationProjectionBuilder<TModel>`, `ClientValidationFieldToken<TModel, TValue>` |
| Projection binding | `ClientValidationProjectionBinder`, `ValidationFieldBindingCatalog`, `ValidationFieldBinding` |
| Plan payload | `ComponentValidation`, `ValidationRuleExecution`, `ValidationRuleOperand`, `ValidationRuleActivation`, `ValidationCondition` |
| Runtime execution | `validateContainer`, `showServerErrors`, `RuntimeValidationActivation`, `RuntimeValidationPeerOperand`, `rule-engine.ts` |

## Design Rules

- Do not name projected browser rules as if they were the normal validator execution path.
- Do not use null as behavior; `none`, missing component, and literal `null` are distinct cases.
- Do not infer custom FluentValidation behavior from implementation details; require `ProjectToClient(...)`.
- Do not create a separate validation read path in runtime; validation reads component values through the same declared object/member contract as gather and reactions.
