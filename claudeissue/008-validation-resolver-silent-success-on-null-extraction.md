# FAIL-FAST-008: ValidationResolver Silently Succeeds When Extractor Returns Null

## Status: Closed — Fixed

ValidationResolver.ResolveRequest now throws when ExtractRules returns null.

## File
`Alis.Reactive/Resolvers/ValidationResolver.cs:75-83`

## How to Reproduce

1. Register a validator type that the factory cannot resolve:
   ```csharp
   p.Post("/api/save", g => g.IncludeAll())
    .Validate<NonExistentValidator>("my-form");
   ```
2. The `HttpRequestBuilder.Validate<T>()` (line 94-100 in HttpRequestBuilder.cs) creates:
   ```csharp
   _validatorType = typeof(NonExistentValidator);
   _validation = new ValidationDescriptor(formId, new List<ValidationField>());
   ```
3. At render time, `ValidationResolver.ResolveRequest()` runs:
   ```csharp
   var extracted = extractor.ExtractRules(req.ValidatorType, formId);
   if (extracted != null)
       req.Validation = extracted;
   ```
4. `ExtractRules` returns `null` (factory returned null for unknown type).
5. The `if (extracted != null)` guard means the original empty `ValidationDescriptor` stays.
6. The plan JSON contains: `"validation": { "formId": "my-form", "fields": [] }`.
7. The runtime sees zero fields and skips all validation. The request fires unchecked.

## Deep Reasoning: Why This Is a Real Bug

The developer explicitly wrote `.Validate<T>("form")` — they declared intent to validate. The framework resolved zero rules and proceeded as if validation was not configured. This is a silent failure at the most security-sensitive boundary of the framework: the HTTP request pipeline.

The empty `ValidationDescriptor` is the problem. It is created as a placeholder in `HttpRequestBuilder.Validate<T>()` with the expectation that `ValidationResolver` will fill it. When the resolver fails to fill it, the placeholder persists into the plan as a valid-but-useless descriptor.

Compare with the framework's treatment of other missing resources:
- Missing DOM element → throws `[alis] target not found`
- Missing vendor root → throws `[alis] no vendor root`
- Missing nested validator → throws `Failed to create nested validator`

But missing validation rules → silently passes. This is inconsistent.

## How Fixing This Improves the Codebase

1. **Fail-fast at render time**: If `ValidatorType` was set but `ExtractRules` returns null or empty fields, throw with a message like: `"Validator '{type.Name}' produced no client rules for form '{formId}'. Ensure the validator is registered in the factory."`.
2. **Catch misconfiguration early**: The error surfaces during development (first page load), not in production (first invalid submission).
3. **Consistent with nested validator resolution**: `FluentValidationAdapter.ResolveNestedValidator` already throws for null results (line 167-172). The top-level resolution should follow the same pattern.

## How This Fix Will Not Break Existing Features

- The fix only triggers when `ValidatorType` is set (developer called `.Validate<T>()`) AND the extractor returns null/empty. This is always a misconfiguration.
- If a developer intentionally wants no client validation, they should not call `.Validate<T>()` at all. The method's purpose is to enable validation — calling it and getting zero rules is always wrong.
- Existing validators that successfully extract rules are unaffected.
- The `FluentValidationAdapter` already returns `null` when `fields.Count == 0` (line 63), so this path is well-defined.
