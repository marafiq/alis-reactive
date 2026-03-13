# FluentValidation Extraction Is Too Coupled To Internals For The Supported Client Subset

## Verdict

This is a legitimate open issue.

## Actual Contract

The DSL is frozen. Validation is request-chained through `HttpRequestBuilder.Validate(...)`, not a standalone subsystem. Partials are a supported path when they flow through `Html.ResolvePlan()` and runtime merge enrichment correctly.

The supported client-validation subset is narrower than full FluentValidation:

- simple local client rules
- explicit client conditional rules
- no server-only or db-backed rules

Generic FluentValidation `.When()` and `.Unless()` are already intentionally skipped for client extraction. Explicit client conditions currently come from `IConditionalRuleProvider`.

## Why This Is Legit

The problem is not that FluentValidation does not expose a perfect client-export model. Upstream explicitly does not treat client-rule generation as a primary goal.

The problem is that the current extraction path still depends on brittle internals and fallback behavior for a much smaller supported subset than the implementation shape suggests.

Today the repo references `FluentValidation` as `11.*`, while upstream latest is `12.1.1`. The adapter is therefore coupling a frozen framework contract to:

- `Activator.CreateInstance`
- `FluentValidation.Internal`
- traversal of `IValidationRule`
- traversal of `IRuleComponent`

That is the wrong long-term architecture for a supported subset that is intentionally small.

## The Real Gaps

### 1. Supported equality still disappears during extraction

The runtime already supports `equalTo`, but the adapter still only lowers comparison rules for `min` and `max`.

That means a supported simple local rule can exist in the runtime contract and still be lost at the extraction boundary.

### 2. Nested supported rules can still degrade silently

Nested validator instantiation failures are swallowed and skipped.

That means supported local rules inside nested validators can disappear without a precise extraction failure.

### 3. Declared request validation can still fail open at runtime

Once extraction has produced declared validation fields, runtime enrichment still only warns on missing component bindings, and runtime validation still skips unresolved fields.

That means a request can opt into supported client validation and still quietly weaken enforcement if the declared fields cannot be resolved in the active request validation path.

## Why Upstream Matters

This is not just an implementation taste issue.

FluentValidation’s own docs and GitHub guidance make the direction clear:

- generating client rules from server validators is not a primary FluentValidation goal
- condition handling is delegate-based and not designed as export-friendly public metadata
- deeper condition introspection leads to more maintenance burden, not less

That means extending inference further into FluentValidation internals is the wrong direction, especially if this framework wants a stable and truthful client-validation contract.

## Minimal Fix Direction

Do not deepen reflection or internal-rule traversal.

Instead:

- keep the DSL frozen
- keep validation request-chained
- treat explicit client-validation metadata as the authoritative contract for the supported subset

The best direction is to evolve the existing explicit metadata path (`IConditionalRuleProvider` + `ConditionalRuleMetadata`) into a broader explicit client-rule provider, rather than trying to infer more from FluentValidation internals.

Automatic extraction can remain only for the smallest stable subset if desired, but it should stop being the primary source of truth.

Upgrading to the latest FluentValidation version should be treated as a separate compatibility decision, not as the fix by itself.

## Required Proof

This needs explicit proof at the correct boundaries:

- the FluentValidation version strategy is made explicit: either intentionally remain on `11.x` or prove compatibility with current latest upstream
- supported simple rules, including `Equal(x => x.OtherField)`, are emitted truthfully into the existing runtime rule set
- nested supported rules do not silently disappear
- unresolved declared fields in the active request validation path fail fast instead of warning and skipping
- explicit client conditional rules remain truthful for both parent and `ResolvePlan()` partial flows
- unsupported server/db rules and generic `.When()` introspection stay clearly out of scope

---

## Response — Claude

### Verdict: Fixed (gaps 1 and 2). Gap 3 deferred.

### FluentValidation version: upgraded to 12.1.1

Adapter compiles cleanly against FV 12.1.1 with zero breaking changes. All 31 tests pass on 12.x. The `12.*` floating version spec ensures future 12.x patches are picked up automatically.

### Gap 1: `equalTo` extraction — Fixed

`IComparisonValidator` with `Comparison.Equal` and a non-null `MemberToCompare` now emits `"equalTo"` with the other field's name as constraint.

```csharp
case IComparisonValidator cv:
    if (cv.Comparison == Comparison.Equal && cv.MemberToCompare != null)
        result.Add(new ExtractedRule("equalTo", ..., cv.MemberToCompare.Name));
```

**Tests:**
- `Equal_to_other_field_extracts_equalTo_rule` — `Equal(x => x.Email)` → `"equalTo"` with constraint `"Email"`
- `Equal_to_with_custom_message_uses_custom_message` — custom message flows through

### Gap 2: Nested validator failures — Fixed (fail fast)

`ResolveNestedValidator()` replaces both try/catch + `Debug.WriteLine` sites. Throws `InvalidOperationException` if factory returns null or throws.

**Tests:**
- `Throws_when_nested_validator_cannot_be_created` — factory returns null → throw
- `Throws_when_factory_throws_for_nested_validator` — factory throws → wrap in InvalidOperationException

### `Activator.CreateInstance` default — Removed

Parameterless constructor removed. Factory is now required:

```csharp
public FluentValidationAdapter(Func<Type, IValidator?> factory)
```

Null factory throws `ArgumentException`. Callers must provide an explicit factory (e.g. DI container resolution or `Activator.CreateInstance` if they choose).

**Tests:**
- `Null_factory_throws` — null factory → ArgumentException
- `Explicit_factory_works` — explicit factory resolves validators correctly

### Gap 3: Unresolved declared fields fail fast at runtime — Deferred

This is a TS runtime concern (validation enrichment warns instead of throwing when component bindings are missing). Not an adapter-level fix. Should be addressed as a separate TS runtime issue.

### What stays out of scope

- Generic `.When()` / `.Unless()` conditions — opaque delegates, cannot safely introspect
- Server-only or DB-backed rules — not extractable for client-side use
- `IConditionalRuleProvider` — unchanged, remains the explicit path for client conditional rules

### Test coverage: 31 tests (was 25)

| Test class | Tests | What it covers |
|------------|-------|----------------|
| WhenExtractingRequiredRules | 3 | NotEmpty, NotNull, custom message |
| WhenExtractingLengthRules | 3 | MinimumLength, MaximumLength, both |
| WhenExtractingEmailRule | 2 | EmailAddress, custom message |
| WhenExtractingRegexRule | 1 | Matches with pattern |
| WhenExtractingRangeRule | 1 | InclusiveBetween |
| WhenExtractingComparisonRules | 2 | GreaterThanOrEqual, LessThanOrEqual |
| WhenExtractingEqualToRules | 2 | Equal(x => x.Other), custom message |
| WhenExtractingMultipleRulesPerField | 1 | Multiple rules on single field |
| WhenExtractingNestedValidators | 2 | Dotted paths, deeply nested |
| WhenExtractingConditionalRules | 2 | .When() skipped, IConditionalRuleProvider merges |
| WhenExtractingAllRuleTypes | 6 | All rule kinds in one validator |
| WhenFormIsEmpty | 2 | Empty validator → null, FormId threading |
| WhenFailingFastOnBrokenNestedValidators | 2 | Factory null → throw, factory throws → throw |
| WhenRequiringExplicitFactory | 2 | Null factory → throw, explicit factory works |
