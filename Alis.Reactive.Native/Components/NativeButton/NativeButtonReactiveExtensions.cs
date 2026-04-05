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
        private static readonly NativeButton _component = new NativeButton();

        public static NativeButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeButtonBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<NativeButtonEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeButtonEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.EnsureComponent(builder.ElementId, "native");
            var trigger = StartsWhen.ComponentEvent(builder.ElementId, descriptor.JsEvent);
            foreach (var reaction in pb.BuildReactions())
                plan.Context.AddBehavior(Behavior.On(trigger, reaction));

            return builder;
        }
    }
}
