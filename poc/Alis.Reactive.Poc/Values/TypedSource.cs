using System;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Values;

public interface ITypedSource<out T>;

internal interface IPlanSource
{
    Expression Expression { get; }

    PlanBuild Build { get; }
}

public abstract class TypedSource<T> : ITypedSource<T>, IPlanSource
{
    private protected TypedSource()
    {
    }

    internal abstract Expression Expression { get; }

    internal abstract PlanBuild Build { get; }

    Expression IPlanSource.Expression => Expression;

    PlanBuild IPlanSource.Build => Build;
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

internal static class Sources
{
    internal static IPlanSource Of<T>(ITypedSource<T> source)
        => source as IPlanSource ?? throw new ArgumentException(
            "A source is one the framework makes: a member's read, a list's operation or a value read from data.", nameof(source));
}

internal static class Literal
{
    internal static Expression Of<T>(T value, PlanBuild build)
        => new LiteralExpression(JsonSerializer.SerializeToElement(value, build.Values));
}
