using System;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.Tracing;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class ViewPlans
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    internal static ReactivePlan<TModel> Open<TModel>(AppJson json, Func<string> prefix, Func<string, string> fullName, Func<string, string> id, Func<string> view)
    {
        var startPrefix = prefix();
        var content = new PlanContent(IdOf<TModel>(), CultureInfo.CurrentCulture, ClockTimeZone.Instance, json, text =>
        {
            var now = prefix();
            var name = fullName(text);
            FieldPrefixes.RefuseAMoveAfterThePlanStarted(startPrefix, now, view, name);
            return (now, name, id(name));
        });
        return new(new BlockWriter(content), startPrefix);
    }

    internal static void On<TModel>(ReactivePlan<TModel> plan, Action<Trigger<TModel>> trigger) => trigger(new Trigger<TModel>(plan.Writer));

    internal static string ScriptOf<TModel>(ReactivePlan<TModel> plan, string prefix, Func<string> view)
    {
        FieldPrefixes.RefuseAMoveInsideThePlan(plan.StartPrefix, prefix, view);
        return ScriptOf(plan);
    }

    private static string ScriptOf<TModel>(ReactivePlan<TModel> plan)
        => ScriptOpen + JsonSerializer.Serialize(plan.Writer.Content.Document(RenderTrace.OfTheRender()), PlanJson.Options) + ScriptClose;

    private static string IdOf<TModel>()
        => ClassNames.ModelKey(typeof(TModel));
}
