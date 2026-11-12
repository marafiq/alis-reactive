using System;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// A condition of a pipeline, waiting for the branch it runs.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the pipeline.</typeparam>
public sealed class Guard<TModel>
{
    internal Guard()
    {
    }

    /// <summary>Runs a branch when the condition is true.</summary>
    /// <remarks>A branch holds any number of steps, and what it reads is read in it and in the blocks inside it.</remarks>
    /// <param name="branch">The steps to run.</param>
    /// <returns>The branches written so far, which take an <c>ElseIf</c> or an <c>Else</c>.</returns>
    public Branches<TModel> Then(Action<Pipeline<TModel>> branch) => throw new NotImplementedException();
}

/// <summary>
/// The branches of a condition written so far: the first whose condition is true runs, and no other.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the pipeline.</typeparam>
public sealed class Branches<TModel>
{
    internal Branches()
    {
    }

    /// <summary>Starts another condition, read only when no branch before it ran.</summary>
    /// <param name="condition">The condition; its branch runs only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public Guard<TModel> ElseIf(TypedSource<bool> condition) => throw new NotImplementedException();

    /// <summary>Runs a branch when no branch before it ran, and ends the condition.</summary>
    /// <param name="branch">The steps to run.</param>
    public void Else(Action<Pipeline<TModel>> branch) => throw new NotImplementedException();
}
