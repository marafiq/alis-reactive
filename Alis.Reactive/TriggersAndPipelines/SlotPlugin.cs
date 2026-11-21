using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.TriggersAndPipelines;

internal static class SlotPlugin
{
    private const string ObjectIdPrefix = "$slot:";
    private const string Name = "alis.slot";
    private const string LoadMethod = "load";
    private const string UnloadMethod = "unload";

    private static readonly ConditionalWeakTable<Statement, string> LoadedElements = new();

    internal static DoStatement Loads(PlanContent plan, string elementId, Expression html)
    {
        var load = Called(plan, elementId, LoadMethod, [html]);
        LoadedElements.Add(load, elementId);
        return load;
    }

    internal static DoStatement Unloads(PlanContent plan, string elementId) => Called(plan, elementId, UnloadMethod, []);

    internal static string? ElementLoadedBy(Statement statement) => LoadedElements.TryGetValue(statement, out var elementId) ? elementId : null;

    private static DoStatement Called(PlanContent plan, string elementId, string method, IReadOnlyList<Expression> args)
        => new(false, new CallExpression([new MemberStep(method)], PathMembers.Required, new ObjExpression(DeclaredIn(plan, elementId)), args));

    private static string DeclaredIn(PlanContent plan, string elementId)
    {
        plan.LoadsOrUnloads(elementId);
        var objectId = ObjectIdPrefix + elementId;
        plan.Declares(objectId, new PluginLocator(Name, elementId, VendorListening.OfAnEventTarget, NonePluginData.Instance));
        return objectId;
    }
}
