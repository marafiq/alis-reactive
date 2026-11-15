using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.InputsAndIds;

internal static class ModelPlugin
{
    private const string ObjectId = "$model";
    private const string Name = "alis.model";
    private const string NoKey = "";

    internal static void Binds(PlanContent plan, string inputId, InputBinding binding)
    {
        plan.BindsInput(inputId, binding);
        DeclaredIn(plan);
    }
    internal static void Contains(PlanContent plan, string id, ContainerKind kind, string summary)
    {
        plan.RendersContainer(id, new Container(plan.Model, kind, summary, []));
        DeclaredIn(plan);
    }

    internal static Expression Sent(PlanContent plan, string containerId, string model) => Called(plan, "sent", containerId, model);

    internal static Expression Captured(PlanContent plan, string containerId, string model) => Called(plan, "captured", containerId, model);

    private static CallExpression Called(PlanContent plan, string method, string containerId, string model)
    {
        DeclaredIn(plan);
        return new CallExpression([new MemberStep(method)], PathMembers.Required, new ObjExpression(ObjectId),
            [ValuesAndSources.Literals.Text(containerId), ValuesAndSources.Literals.Text(model)]);
    }

    private static void DeclaredIn(PlanContent plan)
        => plan.Declares(ObjectId, new PluginLocator(Name, NoKey, VendorListening.OfAnEventTarget, plan.ModelData));
}

