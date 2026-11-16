using System;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// A condition on the parts a request sends, waiting for the branch that writes them.
/// </summary>
/// <typeparam name="TParts">The parts the branch writes.</typeparam>
public sealed class PartsGuard<TParts>
    where TParts : class
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;
    private readonly Expression _condition;
    private readonly Func<int, TParts> _staged;

    internal PartsGuard(RequestPartsWriter parts, int stage, Expression condition, Func<int, TParts> staged)
    {
        _parts = parts;
        _stage = stage;
        _condition = condition;
        _staged = staged;
    }

    /// <summary>Writes the parts the request sends when the condition is true.</summary>
    /// <remarks>A branch writes any number of parts, and each is left out when the condition is not true.</remarks>
    /// <param name="branch">The parts sent under the condition.</param>
    /// <returns>The request's parts, which take more.</returns>
    public TParts Then(Action<TParts> branch) => _staged(_parts.Guarded(_stage, _condition, stage => branch(_staged(stage))));
}
