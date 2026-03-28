using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires browser events from <see cref="NativeCheckList"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call in the builder chain.
    /// <code>
    /// .NativeCheckList(b => b
    ///     .Items(allergyItems)
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("updated!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeCheckListReactiveExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        /// <summary>
        /// Wires a <see cref="NativeCheckList"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The check list builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive entry to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckListEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeCheckListEvents.Instance);

            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            // Single entry on the hidden input — checklist.ts dispatches change after sync
            var trigger = new ComponentEventTrigger(
                builder.ElementId, descriptor.JsEvent, _component.Vendor,
                builder.BindingPath, _component.ReadExpr);

            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
