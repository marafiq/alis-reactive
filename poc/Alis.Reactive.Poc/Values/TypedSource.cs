using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Values;

public abstract class TypedSource<T>
{
    private protected TypedSource()
    {
    }

    internal abstract Expression Expression { get; }

    internal abstract PlanBuild Build { get; }
}

internal sealed class DataRead<T> : TypedSource<T>
{
    internal DataRead(Expression expression, PlanBuild build)
    {
        Expression = expression;
        Build = build;
    }

    internal override Expression Expression { get; }

    internal override PlanBuild Build { get; }
}

internal static class Literal
{
    internal static Expression Of<T>(T value, PlanBuild build)
        => new LiteralExpression(JsonSerializer.SerializeToElement(value, build.Values));
}
