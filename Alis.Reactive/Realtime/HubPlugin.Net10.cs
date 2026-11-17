#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Realtime;

internal static class HubPlugin
{
    private const string Name = "alis.hub";
    private const string ObjectIdPrefix = "$hub:";
    private const string Client = "/_content/AlisReactive/js/alis-reactive-hub.js";

    private static readonly IReadOnlyList<int> ClientsOwnReconnectWaits = [0, 2000, 10000, 30000];

    internal static readonly IReadOnlyList<PathStep> MessageData = [new IndexStep(0), new MemberStep("data")];

    internal static (THub Hub, string ObjectId) Reached<THub>(BlockWriter writer, SignalRSource<THub> source)
        where THub : SignalRHub, new()
    {
        var plan = writer.Content;
        var offer = RealtimeChoices.Offers(plan.Services.GetService<RealtimeChoices>());
        var objectId = ObjectIdPrefix + source.Url;
        plan.Declares(objectId, new PluginLocator(Name, source.Url, VendorListening.OfAnEventTarget, new HubPluginData(offer, ClientsOwnReconnectWaits, ClientOf(plan))));
        return (JsObject.Reaching<THub>(new Reached(new ObjExpression(objectId), writer)), objectId);
    }

    private static string ClientOf(PlanContent plan)
    {
        var pathBase = new PathString(plan.PathBase);
        return plan.Services.GetRequiredService<IFileVersionProvider>().AddFileVersionToPath(pathBase, pathBase.Add(new PathString(Client)).ToString());
    }
}
#endif
