#if NET10_0_OR_GREATER
using Alis.Reactive.Realtime;

namespace Alis.Reactive.TriggersAndPipelines;

public sealed partial class Pipeline<TModel>
{
    /// <summary>Reaches a stream the page holds, to read its state or close it.</summary>
    /// <remarks>The plan that names the stream holds it while the plan lives, so a plan that only reads its state reads the page's open connection.</remarks>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <param name="stream">The stream, as <c>ServerPushStream.At&lt;TStream&gt;(url)</c> names it.</param>
    /// <returns>The stream.</returns>
    public TStream Stream<TStream>(ServerPushSource<TStream> stream)
        where TStream : ServerPushStream, new()
        => StreamPlugin.Reached(_writer, stream).Stream;

    /// <summary>Reaches a hub the page holds, to call its methods or read its state and connection id.</summary>
    /// <remarks>The plan that names the hub holds it while the plan lives; a call before the hub's first start settles waits for it.</remarks>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <returns>The hub.</returns>
    public THub Hub<THub>(SignalRSource<THub> hub)
        where THub : SignalRHub, new()
        => HubPlugin.Reached(_writer, hub).Hub;
}
#endif
