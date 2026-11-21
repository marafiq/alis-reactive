using System;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Values;

public interface ITypedSource<out T>;

internal interface IPlanSource
{
    Expression Expression { get; }

    PlanBuild Build { get; }

    HeldForm Form { get; }
}

public abstract class TypedSource<T> : ITypedSource<T>, IPlanSource
{
    private protected TypedSource()
    {
    }

    internal abstract Expression Expression { get; }

    internal abstract PlanBuild Build { get; }

    internal virtual HeldForm Form => Build.OptionsForm;

    private protected TypedSource<TRead> DataPast<TRead>(System.Linq.Expressions.Expression<Func<T, TRead>> path, string argument)
    {
        var read = JsonPath.Of(path, Form, Build.Starts, argument);
        return new DataRead<TRead>(new DataExpression(Expression, read.Steps), Build, read.Form);
    }

    Expression IPlanSource.Expression => Expression;

    PlanBuild IPlanSource.Build => Build;

    HeldForm IPlanSource.Form => Form;
}

internal sealed class DataRead<T> : TypedSource<T>
{
    internal DataRead(Expression expression, PlanBuild build)
        : this(expression, build, build.OptionsForm)
    {
    }

    internal DataRead(Expression expression, PlanBuild build, HeldForm form)
    {
        Expression = expression;
        Build = build;
        Form = form;
    }

    internal override Expression Expression { get; }

    internal override PlanBuild Build { get; }

    internal override HeldForm Form { get; }
}

internal static class Sources
{
    internal static IPlanSource Of<T>(ITypedSource<T> source)
        => source as IPlanSource ?? throw new ArgumentException(
            "A source is one the framework makes: a member's read, a list's operation or a value read from data.", nameof(source));
}

internal static class Literal
{
    internal static LiteralExpression Of<T>(T value, PlanBuild build)
        => new LiteralExpression(JsonSerializer.SerializeToElement(value, build.Values));

    internal static LiteralExpression Of<T>(T value, HeldForm form)
        => new LiteralExpression(form.Write(value, typeof(T)));
}
