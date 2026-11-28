using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class Guard<TModel>
{
    private readonly PlanBuild _build;
    private readonly Expression _condition;
    private readonly Func<Expression, Action, Branching> _arm;

    internal Guard(PlanBuild build, Expression condition, Func<Expression, Action, Branching> arm)
    {
        _build = build;
        _condition = condition;
        _arm = arm;
    }

    public Branches<TModel> Then(Action<Pipeline<TModel>> branch)
        => new(_build, _arm(_condition, () => branch(new Pipeline<TModel>(_build))));
}

public sealed class Branches<TModel>
{
    private readonly PlanBuild _build;
    private readonly Branching _branching;

    internal Branches(PlanBuild build, Branching branching)
    {
        _build = build;
        _branching = branching;
    }

    public Guard<TModel> ElseIf(TypedSource<bool> condition)
        => new(_build, Sources.Of(condition).Expression, (expression, body) =>
        {
            _build.ElseIf(_branching, expression, body);
            return _branching;
        });

    public void Else(Action<Pipeline<TModel>> branch)
        => _build.Else(_branching, () => branch(new Pipeline<TModel>(_build)));
}
