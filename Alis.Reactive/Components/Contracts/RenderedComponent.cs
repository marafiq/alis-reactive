using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive
{
    /// <summary>
    /// A component the framework has just rendered, joined to its plan and element ID. Behavior
    /// hangs off it with <see cref="Reactive{TArgs}"/>, where the component is rendered.
    /// </summary>
    /// <example>
    /// <code>
    /// Html.InputField(plan, m =&gt; m.Rate)
    ///     .FusionNumericTextBox(b =&gt; b.Min(0))
    ///     .Reactive(evt =&gt; evt.Change, (args, p) =&gt; p.Element("rate-echo").SetText(args, x =&gt; x.Value));
    /// </code>
    /// </example>
    /// <typeparam name="TComponent">The component declaration whose events can be selected.</typeparam>
    /// <typeparam name="TModel">The plan's model type.</typeparam>
    public sealed class RenderedComponent<TComponent, TModel>
        where TComponent : IComponent, new()
        where TModel : class
    {
        private static readonly TComponent Declaration = new TComponent();

        private readonly ReactivePlan<TModel> _plan;
        private readonly string _componentId;

        internal RenderedComponent(ReactivePlan<TModel> plan, string componentId)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _componentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
        }

        /// <summary>Runs <paramref name="pipeline"/> whenever the selected event fires on this component.</summary>
        /// <typeparam name="TArgs">The event's typed payload.</typeparam>
        /// <param name="eventSelector">Selects a declared event, for example <c>evt =&gt; evt.Change</c>.</param>
        /// <param name="pipeline">The reaction, given the typed payload and the pipeline.</param>
        /// <returns>This component, so further events can be wired.</returns>
        public RenderedComponent<TComponent, TModel> Reactive<TArgs>(
            Func<TComponent, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
        {
            if (eventSelector == null) throw new ArgumentNullException(nameof(eventSelector));

            ComponentEventOnboarding.Wire(_plan, _componentId, Declaration.Vendor, eventSelector(Declaration), pipeline);
            return this;
        }
    }
}
