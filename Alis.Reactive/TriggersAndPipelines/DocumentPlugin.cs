using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.TriggersAndPipelines;

internal static class DocumentPlugin
{
    internal const string Ready = "ready";

    private const string ObjectId = "$document";
    private const string Name = "alis.document";
    private const string NoKey = "";

    internal static string DeclaredIn(PlanContent plan)
    {
        plan.Declares(ObjectId, new PluginLocator(Name, NoKey, VendorListening.OfAnEventTarget, NonePluginData.Instance));
        return ObjectId;
    }
}
