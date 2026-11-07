using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Values;

public abstract class Arg
{
    private protected Arg()
    {
    }

    internal abstract Expression ExpressionIn(PlanBuild build);
}

internal sealed class WrittenArg : Arg
{
    private readonly Func<PlanBuild, Expression> _expression;

    internal WrittenArg(Func<PlanBuild, Expression> expression)
    {
        _expression = expression;
    }

    internal override Expression ExpressionIn(PlanBuild build) => _expression(build);
}

public sealed class Arg<T> : Arg
{
    private readonly Func<PlanBuild, Expression> _expression;

    private Arg(Func<PlanBuild, Expression> expression)
    {
        _expression = expression;
    }

    public static implicit operator Arg<T>(T value) => new(build => Literal.Of(value, build));

    public static implicit operator Arg<T>(TypedSource<T> source) => new(_ => source.Expression);

    internal override Expression ExpressionIn(PlanBuild build) => build.IntoLive<T>(_expression(build));
}
