using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires browser events from <see cref="NativeRadioGroup"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates one plan entry per radio option so each radio button triggers the
    /// pipeline independently. <c>.Reactive()</c> is always the last call in the
    /// builder chain.
    /// </para>
    /// <code>
    /// .NativeRadioGroup(b => b
    ///     .Items(careLevelItems)
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("selected!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeRadioGroupReactiveExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        /// <summary>
        /// Wires a <see cref="NativeRadioGroup"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The radio group builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive entries to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeRadioGroupBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeRadioGroupBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeRadioGroupEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeRadioGroupEvents.Instance);

            for (int i = 0; i < builder.Options.Count; i++)
            {
                var pb = new PipelineBuilder<TModel>();
                pipeline(descriptor.Args, pb);

                var radioId = $"{builder.ElementId}_r{i}";
                var trigger = new ComponentEventTrigger(
                    radioId, descriptor.JsEvent, _component.Vendor,
                    builder.BindingPath, _component.ReadExpr);

                foreach (var reaction in pb.BuildReactions())
                    plan.AddEntry(new Entry(trigger, reaction));
            }

            return builder;
        }
    }
}
