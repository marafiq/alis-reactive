using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeButtonBuilder.
    ///
    /// Usage (in .cshtml):
    ///   @Html.NativeButton("save-btn", "Save")
    ///       .CssClass("...")
    ///       .Reactive(plan, evt => evt.Click, (args, p) =>
    ///       {
    ///           p.Post("/api/save", g => g.Static("name", "John"))
    ///            .Response(r => r.OnSuccess(s => s.Element("result").SetText("Saved!")));
    ///       })
    ///
    /// .Reactive() is always the last call -- the builder implements IHtmlContent.
    /// </summary>
    public static class NativeButtonReactiveExtensions
    {
        public static NativeButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeButtonBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<NativeButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeButtonEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.WireComponentEvent(builder.ElementId, "native", descriptor.JsEvent, pb.BuildReactions());

            return builder;
        }
    }
}
