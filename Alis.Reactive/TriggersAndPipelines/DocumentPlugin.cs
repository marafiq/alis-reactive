using System.Collections.Generic;
using System.Text.Json;
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
    private const string DispatchMethod = "dispatch";
    private const string QueryMethod = "query";
    private const string SegmentMethod = "segment";

    internal static readonly IReadOnlyList<PathStep> NoPayload = [];

    internal static readonly IReadOnlyList<PathStep> DetailOfTheEvent = [new IndexStep(0), new MemberStep("detail")];

    internal static string DeclaredIn(PlanContent plan)
    {
        plan.Declares(ObjectId, new PluginLocator(Name, NoKey, VendorListening.OfAnEventTarget, NonePluginData.Instance));
        return ObjectId;
    }

    internal static Expression QueryText(PlanContent plan, string key)
        => new CallExpression([new MemberStep(QueryMethod)], PathMembers.Required, new ObjExpression(DeclaredIn(plan)), [new LitExpression(JsonSerializer.SerializeToElement(key))]);

    internal static Expression SegmentText(PlanContent plan, IReadOnlyList<string> template, int place)
        => new CallExpression([new MemberStep(SegmentMethod)], PathMembers.Required, new ObjExpression(DeclaredIn(plan)),
            [new LitExpression(JsonSerializer.SerializeToElement(template)), new LitExpression(JsonSerializer.SerializeToElement(place))]);

    internal static DoStatement Dispatches(PlanContent plan, string appEvent, Expression payload)
        => new(false, new CallExpression([new MemberStep(DispatchMethod)], PathMembers.Required, new ObjExpression(DeclaredIn(plan)),
            [new LitExpression(JsonSerializer.SerializeToElement(appEvent)), payload]));
}
