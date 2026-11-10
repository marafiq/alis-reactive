using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
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
    private readonly Func<PlanBuild, HeldForm> _form;

    private Arg(Func<PlanBuild, Expression> expression, Func<PlanBuild, HeldForm> form)
    {
        _expression = expression;
        _form = form;
    }

    public static implicit operator Arg<T>(T value) => new(build => Literal.Of(value, build), build => build.OptionsForm);

    public static implicit operator Arg<T>(TypedSource<T> source) => new(_ => source.Expression, _ => source.Form);

    internal static Arg<T> Of(ITypedSource<T> source)
    {
        var read = Sources.Of(source);
        return new(_ => read.Expression, _ => read.Form);
    }

    internal override Expression ExpressionIn(PlanBuild build) => build.IntoLive<T>(_expression(build), _form(build));
}
