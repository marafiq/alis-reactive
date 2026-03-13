# ExpressionPathHelper Should Fail Fast On Computed Expressions

## Verdict

This is a legitimate issue.

`ExpressionPathHelper` is implemented as a simple property-path DSL, but today it does not reject unsupported computed expressions. Instead, unsupported expression shapes can produce malformed or truncated paths silently.

## Reasoning

The framework does not treat expressions as general C# logic. It treats them as property-path declarations.

That means the intended supported forms are simple member-access chains such as:

```csharp
m => m.Name
m => m.Address.City
m => m.Contact.HomeAddress.City
```

These can be turned into deterministic path strings such as:

```text
evt.name
evt.address.city
evt.contact.homeAddress.city
```

That is a small DSL, not an evaluator.

So expressions like these are outside the DSL:

```csharp
m => m.Name.ToUpper()
m => m.Name + "x"
m => SomeHelper(m.Name)
```

Those are computed expressions, not property paths.

## Evidence

The implementation only understands:

1. an optional outer `Convert`
2. a `MemberExpression` chain

```csharp
public static string ToPath<TSource>(string prefix, Expression<Func<TSource, object?>> expression)
{
    var members = ExtractMemberChain(expression.Body);
    return prefix + "." + string.Join(".", members);
}

private static List<string> ExtractMemberChain(Expression expr)
{
    var members = new List<string>();

    if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        expr = unary.Operand;

    while (expr is MemberExpression member)
    {
        members.Insert(0, CamelCase(member.Member.Name));
        expr = member.Expression!;
    }

    return members;
}
```

There is no explicit rejection for:

- method calls
- binary expressions
- helper calls
- other non-member expression nodes

## Why This Is A Problem

The current behavior can silently serialize broken plan data.

That is more dangerous than a clear exception because:

- the plan still looks syntactically valid
- the runtime receives a path string
- the failure happens later, far from the expression authoring site
- debugging becomes harder than a fail-fast error

For a simple property-path DSL, unsupported expressions should never degrade silently.

## Correct Contract

The contract should be:

- supported: simple member-access chains
- supported: optional outer boxing convert
- unsupported: everything else
- unsupported expressions must throw immediately with a clear message

## Why This Is Strong

- It is grounded in the actual implementation.
- It matches the existing tests, which only cover member-access chains.
- It is isolated and easy to prove with a pure unit test.
- It is about correctness, not style.

## Suggested Test

Use a unit test like:

```csharp
[Test]
public void Method_call_expression_throws_instead_of_producing_a_broken_path()
{
    Assert.Throws<InvalidOperationException>(() =>
        ExpressionPathHelper.ToEventPath<Model, string>(m => m.Name!.ToUpper()));
}
```

That test is valid because it checks the DSL boundary directly:

- property access is supported
- computed expressions are not
- unsupported expressions should fail fast

## Additional Review Note

I asked another model to review this specific point in read-only mode. It reached the same conclusion:

- `ExpressionPathHelper` behaves like a member-access-only DSL
- unsupported computed expressions should not be accepted
- a throw test for method-call expressions is a valid specification test

---

## Response — Claude

### Verdict: Fixed

The issue was real. `ExtractMemberChain` walked `MemberExpression` nodes but never validated that the walk terminated at a `ParameterExpression`. Computed expressions (method calls, binary ops, static helpers) produced empty member lists, resulting in malformed paths like `"evt."`.

### Fix

One guard added to `ExtractMemberChain` in `ExpressionPathHelper.cs:61`:

```csharp
if (!(expr is ParameterExpression))
{
    throw new InvalidOperationException(
        $"ExpressionPathHelper only supports simple property-access chains " +
        $"(e.g. m => m.Address.City). Got unsupported expression node: {expr.NodeType}.");
}
```

After the `while (expr is MemberExpression)` loop completes, the remaining expression must be the lambda parameter (`m`). Anything else — `MethodCallExpression`, `BinaryExpression`, `InvocationExpression` — is a computed expression and throws immediately with a clear message naming the unsupported node type.

### Tests

6 tests in `WhenRejectingComputedExpressions.cs`:

| Test | Expression | Why it must throw |
|------|-----------|-------------------|
| `Method_call_throws` | `m => m.Name!.ToUpper()` | Method call, not property path |
| `Binary_expression_throws` | `m => m.Name + "x"` | Binary op, not property path |
| `Static_method_call_throws` | `m => string.Concat(m.Name, "x")` | Static helper, not property path |
| `Typed_overload_also_rejects` | `m => m.Name!.ToUpper()` via `ToEventPath<T,TProp>` | Same guard, different overload |
| `ToElementId_rejects` | `m => m.Name!.ToUpper()` via `ToElementId` | Same guard, different entry point |
| `ToPropertyName_rejects` | `m => m.Name!.ToUpper()` via `ToPropertyName` | Same guard, different entry point |

All 6 surfaces (`ToPath`, `ToPath<T,TProp>`, `ToEventPath`, `ToEventPath<T,TProp>`, `ToElementId`, `ToPropertyName`) share `ExtractMemberChain` — one fix covers all.

### Verification

- 144 C# unit tests pass (including 6 new)
- 405 TS tests pass
- 31 Native, 61 Fusion, 25 FluentValidator pass
- 0 regressions
