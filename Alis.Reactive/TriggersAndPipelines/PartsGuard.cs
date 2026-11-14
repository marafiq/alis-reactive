using System;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// A condition on the parts a request sends, waiting for the branch that writes them.
/// </summary>
/// <typeparam name="TParts">The parts the branch writes.</typeparam>
public sealed class PartsGuard<TParts>
    where TParts : class
{
    internal PartsGuard()
    {
    }

    /// <summary>Writes the parts the request sends when the condition is true.</summary>
    /// <remarks>A branch writes any number of parts, and each is left out when the condition is not true.</remarks>
    /// <param name="branch">The parts sent under the condition.</param>
    /// <returns>The request's parts, which take more.</returns>
    public TParts Then(Action<TParts> branch) => throw new NotImplementedException();
}
