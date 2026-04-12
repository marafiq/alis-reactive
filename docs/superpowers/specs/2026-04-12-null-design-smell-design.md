# Design Spec: Eliminate Null-as-Domain-Vocabulary

**Date:** 2026-04-12
**Branch:** `fix/null-design-smell` (from `gettingclose`)
**Baseline:** 128 unit + 78 Fusion + 109 FluentValidator + 19 Native + 825 Playwright

## Problem

The plan model uses `null` at every layer to mean "not specified":

- **C#**: Constructors destroy domain defaults into null (`Shape.None -> null`)
- **JSON**: Fields omitted via global `WhenWritingNull`
- **TS types**: Properties marked `?:` (optional = undefined when absent)
- **TS runtime**: `if (producer.shape)` guards at 60+ call sites

This causes 120 NRT warnings, makes debugging impossible (null tells you nothing about intent),
and spreads conditional checks across every layer.

## Vision

**Domain defaults flow through every layer. Always present. Always meaningful. No null. No undefined. No if-guards for absence.**

| Layer | Before | After |
|-------|--------|-------|
| C# property | `Shape = null` | `Shape = Shape.None` (non-nullable) |
| C# constructor | `shape.IsNone ? null : shape` | `Shape = shape ?? Shape.None` |
| JSON output | field omitted | `"shape": {"kind":"none"}` always present |
| JSON schema | `shape` optional | `shape` required, "none" valid kind |
| TS type | `shape?: Shape` | `shape: Shape` (required) |
| TS runtime | `if (producer.shape)` | just use `producer.shape` |

Same for collections:

| Layer | Before | After |
|-------|--------|-------|
| C# | `Args = null` | `Args = Array.Empty<VP>()` |
| JSON | field omitted | `"args": []` always present |
| TS type | `args?: VP[]` | `args: VP[]` |
| TS runtime | `if (args?.length)` | `args.forEach(...)` (empty = no-op) |

## New Domain Sentinels

| Sentinel | Implementation | Serializes to |
|----------|---------------|---------------|
| `Shape.None` | Already exists (kind="none") | `{"kind":"none"}` |
| `Path.None` | Already exists (empty segments) | `[]` |
| `ValueProducer.None` | New sealed `NoneProducer`, kind="none" | `{"kind":"none"}` |
| `Condition.None` | New sealed `NoneCondition`, kind="none" | `{"kind":"none"}` |
| Empty collections | `Array.Empty<T>()`, `new Dictionary<>()` | `[]` or `{}` |

## The ONLY Surviving Null

`LiteralProducer.Value` (`object?`) -- because JSON `null` is a valid literal value,
not absence. `ValueProducer.Null()` creates `new LiteralProducer(null, Shape.None)`.

## Properties That Stay Nullable (Genuine Domain Absence)

These are NOT "not specified" -- they represent genuine absence in the domain:

| Property | Why null is correct |
|----------|-------------------|
| `Component.BindingPath` (string?) | Unbound components have no binding |
| `Component.ValueMember` (string?) | Display components have no value to read |
| `Component.Container` (ContainerScope?) | Not all components are in a form |
| `Request.Container` (string?) | Not all requests target a form |
| `Request.Input` (RequestInput?) | GET requests have no body |
| `Request.Next` (Request?) | Not all requests chain |
| `Request.Status` (int?) | Null = match any status |
| `Reaction.OnSettled` (Reaction?) | Not all parallels need cleanup |
| `Reaction.Data` (ValueProducer?) | Not all dispatches carry data |
| `StartsWhen.PayloadType/Event` (string?) | Untyped triggers |
| `Plan.PartId` (string?) | Only set for partial plans |
| `PathSegment.Name/Index` | Segment is either named or indexed |

## Serialization Strategy

With domain defaults always present, serialization simplifies:

1. Remove global `DefaultIgnoreCondition = WhenWritingNull` from `ReactivePlanSerializer`
2. Types with domain defaults serialize ALL properties (defaults included in JSON)
3. Genuinely nullable properties use per-property `[JsonIgnore(Condition = WhenWritingNull)]`
4. `WriteOnlyPolymorphicConverter` unchanged -- delegates to concrete type serialization

The serializer stops controlling the domain model. The domain model is always fully populated.
Per-property `[JsonIgnore(WhenWritingNull)]` is used surgically for the small set of genuinely
nullable properties listed above.

## Schema Changes

Fields moving from optional to required:
- ReadProducer: `path`, `shape`, `args`
- LiteralProducer: `shape`
- ObjectProducer: `shape`
- ArrayProducer: `shape`
- CompareCondition: `right`, `shape`, `itemShape`
- JsMethod: `args`, `returns`
- ComponentValidation: `constraint`, `otherValue`, `when`, `shape`
- ConditionalReaction: `when`
- CallReaction: `args`
- Request: `headers`, `routeParams`, `before`, `success`, `error`, `complete`

New union members:
- ValueProducer oneOf gains `NoneProducer: {"kind":"none"}`
- Condition oneOf gains `NoneCondition: {"kind":"none"}`
- Shape kind enum gains `"none"`

## TS Type Changes

Optional `?:` properties that become required (matches schema changes above).
New discriminated union members for `NoneProducer` and `NoneCondition`.

## TS Runtime Changes

- Remove `=== undefined` / `== null` checks on plan model properties that now carry defaults
- `producer.shape` always exists -- no optional chaining needed
- `producer.args` always an array -- no null guards
- `condition.right` always a ValueProducer -- check `kind !== "none"` for unary ops
- `evaluate.ts`: shape fallback chain `producer.shape ?? prop.shape` becomes
  `producer.shape.kind !== "none" ? producer.shape : prop.shape`

## Verification Strategy

1. VerifyJson snapshots will change (more fields in output) -- update ALL snapshots
2. AssertSchemaValid tests will change (schema updates) -- update schema + tests
3. Playwright tests should be behavior-unchanged -- same user-visible output
4. Typecheck (`npm run typecheck`) must pass with new required properties
5. Zero NRT warnings on plan model files (the 120 we're fixing)

## Non-Goals

- Refactoring builder internals (they can still use null for transient state)
- Changing FluentValidation adapter patterns (null checks on user model values are correct)
- Changing test infrastructure patterns (`= null!` for late-init in Playwright fixtures is fine)
