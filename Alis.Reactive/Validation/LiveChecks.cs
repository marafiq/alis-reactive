using System.Collections.Generic;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Validation;

internal static class LiveChecks
{
    private const string Plugin = "alis.form";
    private const string ObjectOf = "$form:";

    internal static IReadOnlyList<Live> Of<TModel>(PlanContent plan, string containerId)
    {
        var catalog = plan.Services.GetService<ValidatorCatalog>();
        var registered = catalog?.Find(typeof(TModel));
        if (catalog is null || registered is null) return [];
        var validator = (ReactiveValidator<TModel>)registered.Resolve(plan.Services);
        var validatorKey = ClassNames.ModelKey(registered.ValidatorType);
        var ruleSet = BrowserRegistrations.DefaultRuleSet;
        var key = validatorKey + ":" + ruleSet;
        var registration = BrowserRegistrations.Of(validator, validatorKey, ruleSet, plan.Json, plan.RendersUnder);
        plan.Declares(ObjectOf + key, new PluginLocator(Plugin, key, VendorListening.OfAnEventTarget, new FormPluginData(registration)));
        return [new Live(validatorKey, ruleSet, new RecordExpression([
            new RecordField("format", Literals.Text(BodyFormat.UrlEncoded.Text)),
            new RecordField("value", ModelPlugin.Sent(plan, containerId, plan.Model)),
        ]))];
    }
}
