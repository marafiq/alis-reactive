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
}
#endif
