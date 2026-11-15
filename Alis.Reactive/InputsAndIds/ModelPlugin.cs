using System;
using System.Collections.Generic;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.InputsAndIds;

internal static class ModelPlugin
{
    private const string ObjectId = "$model";
    private const string Name = "alis.model";
    private const string NoKey = "";

    private static readonly IReadOnlyDictionary<string, Container> NoContainers = new SortedDictionary<string, Container>(StringComparer.Ordinal);

    internal static void Binds(PlanContent plan, string inputId, InputBinding binding)
    {
        var inputs = plan.BindsInput(inputId, binding);
        plan.Declares(ObjectId, new PluginLocator(Name, NoKey, VendorListening.OfAnEventTarget, new ModelPluginData(inputs, NoContainers)));
    }
}
