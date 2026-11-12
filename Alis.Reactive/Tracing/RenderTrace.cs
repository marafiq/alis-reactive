using System;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Tracing;

internal static class RenderTrace
{
    internal static Trace OfTheRender()
    {
        var theRenderIsRecorded = System.Diagnostics.Activity.Current is { Recorded: true };
        if (theRenderIsRecorded) throw new NotImplementedException();
        return OffTrace.Instance;
    }
}
