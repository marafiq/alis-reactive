using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive
{
    /// <summary>
    /// Wires a component's declared event into a Reactive Plan. A component's rendering helper
    /// calls this from its <c>.Reactive(...)</c> overload with the ID the framework rendered.
    /// </summary>
    public static class ComponentEvents
    {
        /// <summary>Runs <paramref name="pipeline"/> whenever the selected event fires on the component.</summary>
        /// <typeparam name="TComponent">The component declaration that owns the event.</typeparam>
        /// <typeparam name="TModel">The plan's model type.</typeparam>
        /// <typeparam name="TArgs">The event's typed payload.</typeparam>
        /// <param name="plan">The plan that receives the behavior.</param>
        /// <param name="componentId">The component's element ID, as the framework rendered it.</param>
        /// <param name="eventSelector">Selects the declared event, for example <c>c =&gt; c.Change</c>.</param>
        /// <param name="pipeline">The reaction, given the typed payload.</param>
        public static void Wire<TComponent, TModel, TArgs>(
            ReactivePlan<TModel> plan,
            string componentId,
            Func<TComponent, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TComponent : IComponent, new()
            where TModel : class
        {
            if (eventSelector == null) throw new ArgumentNullException(nameof(eventSelector));

            var component = new TComponent();
            ComponentEventOnboarding.Wire(plan, componentId, component.Vendor, eventSelector(component), pipeline);
        }
    }
}
