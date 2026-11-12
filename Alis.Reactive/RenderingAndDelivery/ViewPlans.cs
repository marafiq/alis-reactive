using System;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.Tracing;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class ViewPlans
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    internal static ReactivePlan<TModel> Open<TModel>()
        => new(new PlanContent(IdOf<TModel>(), ViewCulture.Of(CultureInfo.CurrentCulture), ClockTimeZone.Instance));

    internal static string ScriptOf<TModel>(ReactivePlan<TModel> plan)
        => ScriptOpen + JsonSerializer.Serialize(plan.Content.Document(RenderTrace.OfTheRender()), PlanJson.Options) + ScriptClose;

    private static string IdOf<TModel>()
        => typeof(TModel).FullName ?? throw new InvalidOperationException("A plan's model is a type with a full name.");
}
