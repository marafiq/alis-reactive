using System;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// A condition of a pipeline, waiting for the branch it runs.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the pipeline.</typeparam>
public sealed class Guard<TModel>
{
    private readonly BlockWriter _writer;
    private readonly Expression _condition;
    private readonly Func<Arm, IfChain> _taken;

    internal Guard(BlockWriter writer, Expression condition, Func<Arm, IfChain> taken)
    {
        _writer = writer;
        _condition = condition;
        _taken = taken;
    }

    /// <summary>Runs a branch when the condition is true.</summary>
    /// <remarks>A branch holds any number of steps, and what it reads is read in it and in the blocks inside it.</remarks>
    /// <param name="branch">The steps to run.</param>
    /// <returns>The branches written so far, which take an <c>ElseIf</c> or an <c>Else</c>.</returns>
    public Branches<TModel> Then(Action<Pipeline<TModel>> branch)
        => new(_writer, _taken(new Arm(_condition, _writer.Block(() => branch(new Pipeline<TModel>(_writer))))));
}

/// <summary>
/// The branches of a condition written so far: the first whose condition is true runs, and no other.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the pipeline.</typeparam>
public sealed class Branches<TModel>
{
    private readonly BlockWriter _writer;
    private readonly IfChain _chain;

    internal Branches(BlockWriter writer, IfChain chain)
    {
        _writer = writer;
        _chain = chain;
    }

    /// <summary>Starts another condition, read only when no branch before it ran.</summary>
    /// <param name="condition">The condition; its branch runs only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public Guard<TModel> ElseIf(TypedSource<bool> condition)
    {
        _chain.RefuseAfterItsElse();
        return new Guard<TModel>(_writer, condition.Expression, _chain.Continued);
    }

    /// <summary>Runs a branch when no branch before it ran, and ends the condition.</summary>
    /// <param name="branch">The steps to run.</param>
    public void Else(Action<Pipeline<TModel>> branch)
    {
        _chain.RefuseAfterItsElse();
        _chain.Ended(_writer.Block(() => branch(new Pipeline<TModel>(_writer))));
    }
}
