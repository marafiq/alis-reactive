#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Realtime;

internal static class StreamPlugin
{
    private const string Name = "alis.stream";
    private const string ObjectIdPrefix = "$stream:";

    internal static readonly IReadOnlyList<PathStep> MessageData = [new IndexStep(0), new MemberStep("data")];

    internal static (TStream Stream, string ObjectId) Reached<TStream>(BlockWriter writer, ServerPushSource<TStream> source)
        where TStream : ServerPushStream, new()
    {
        var plan = writer.Content;
        var offer = RealtimeChoices.Offers(plan.Services.GetService<RealtimeChoices>());
        var objectId = ObjectIdPrefix + source.Url;
        plan.Declares(objectId, new PluginLocator(Name, source.Url, VendorListening.OfAnEventTarget, new StreamPluginData(offer)));
        return (JsObject.Reaching<TStream>(new Reached(new ObjExpression(objectId), writer)), objectId);
    }
}
#endif
