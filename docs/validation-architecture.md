# Validation Architecture

This document describes the current validation design. Keep it aligned with
`docs/reactive-plan-domain-language.md`.

## Boundary

FluentValidation still executes normally on submit or HTTP endpoints. Alis
Reactive emits only deterministic client-side rules that are explicitly declared
as browser metadata.

Unsupported browser behavior is not guessed or emitted into the plan. The
original FluentValidation rule still runs normally on postback or HTTP submit.

## Data Flow

```text
Validate<TValidationSource>(containerId)
  -> RequestValidation registers a ValidationJob
  -> ReactivePlan.Render() calls ResolveAll()
  -> input component registrations are materialized into the plan
  -> ClientValidationRuleBinder binds queued validation jobs
  -> Component.container.validationRules carries browser validation rules
  -> HTTP runtime validates the container before dispatch
```

## C# Plan Side

`ConfiguredRequestValidation.Register(...)` records a `ValidationJob` containing:

- request URL, used for diagnostics;
- validation container id;
- validation source type that provides browser rule metadata.

`ClientValidationRuleBinder` then:

1. Requires a registered `IClientValidationRuleSource`.
2. Calls `GetClientRules(validationSourceType)`.
3. Binds each `ClientValidationField` through `ClientValidationFieldBinder`.
4. Merges the resulting `ComponentValidation` rules onto the validation-container component.

Field binding has two deterministic paths:

- Registered input fields use the rendered component id, value member, and shape from `ComponentRegistration`.
- Deferred fields use the metadata's declared field shape and the deterministic component id a partial will render later.

## Metadata Sources

`IClientValidationRuleSource` is registered through DI. The built-in source
combines metadata providers registered by:

- `services.AddReactiveClientValidation(...)` for app-level browser rules;
- `services.AddReactiveFluentValidation(...)` for `ReactiveValidator<T>` metadata.

Both paths key rules by the validation source type named by
`Validate<TValidationSource>()`, but public authoring selects fields through
typed expressions and `ClientValidationFieldToken<TModel, TValue>`, not field
name strings.

FluentValidation metadata is snapshotted once by the singleton metadata
provider. Validators still run normally through FluentValidation for server
validation; the snapshot is only the deterministic browser-rule metadata used
when rendering a plan.

Rule-source fields carry their declared shape into render-time binding.
That lets deferred partial fields bind through the same deterministic component
id policy without reflecting over the model just to rediscover the field type.
Peer fields and condition fields are also entered into the metadata so their
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

`ClientRule(...)` cannot be declared inside regular FluentValidation
`When`/`Unless` or async guards. Use `WhenField*` when a rule needs a browser
condition; keep regular FluentValidation guards server-only.

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
| Rule source contract | `IClientValidationRuleSource.GetClientRules(Type)` returning `IReadOnlyList<ClientValidationField>` |
| Metadata registration | `AddReactiveClientValidation`, `AddReactiveFluentValidation`, `ClientValidationRulesBuilder<TModel>`, `ClientValidationFieldToken<TModel, TValue>` |
| Rule binding | `ClientValidationRuleBinder`, `ClientValidationFieldBinder`, `ValidationFieldBinding` |
| Plan payload | `ComponentValidation`, `ValidationRuleExecution`, `ValidationRuleOperand`, `ValidationRuleActivation`, `ValidationCondition` |
| Runtime execution | `validateContainer`, `showServerErrors`, `RuntimeValidationActivation`, `RuntimeValidationPeerOperand`, `rule-engine.ts` |

## Design Rules

- Do not name browser metadata rules as if they were the normal validator execution path.
- Do not use null as behavior; `none`, missing component, and literal `null` are distinct cases.
- Do not infer FluentValidation behavior from implementation details; require explicit `ClientRule(...)` metadata for browser rules.
- Do not create a separate validation read path in runtime; validation reads component values through the same declared object/member contract as gather and reactions.
