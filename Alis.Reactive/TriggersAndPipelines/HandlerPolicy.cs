using System;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// What a handler does when its trigger fires again while the run it started last is still in flight.
/// </summary>
/// <remarks>
/// A run is in flight from its trigger until it ends, its <c>Finally</c> blocks included. A handler's door that names
/// no policy runs under <see cref="Overlap"/>.
/// </remarks>
public sealed class HandlerPolicy
{
    private HandlerPolicy()
    {
    }

    /// <summary>Gets the policy that starts another run beside the one in flight.</summary>
    public static HandlerPolicy Overlap => throw new NotImplementedException();

    /// <summary>Gets the policy that ends the run in flight and starts the new one, so a late answer never overwrites a newer one.</summary>
    /// <remarks>
    /// The ended run's request runs no route, and each of its <c>Finally</c> blocks whose <c>WhileLoading</c> completed
    /// runs once. A run ended inside a <c>Finally</c> finishes that <c>Finally</c> and runs nothing after it.
    /// </remarks>
    public static HandlerPolicy Replace => throw new NotImplementedException();

    /// <summary>Gets the policy that starts nothing while a run is in flight, so a save clicked twice sends once.</summary>
    public static HandlerPolicy Skip => throw new NotImplementedException();
}
