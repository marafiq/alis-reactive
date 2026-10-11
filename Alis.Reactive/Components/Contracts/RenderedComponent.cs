using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web;
#else
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// A component the framework has just rendered, joined to its plan and element ID. Behavior
    /// hangs off it with <see cref="Reactive{TArgs}"/>, where the component is rendered.
    /// </summary>
    /// <remarks>
    /// An input renders into its field (<c>field.Render&lt;TComponent&gt;(markup)</c>), which has
    /// already written the markup. Any other component renders against the plan
    /// (<c>plan.Render&lt;TComponent&gt;(elementId, markup)</c>) and writes its markup where the
    /// view writes this handle.
    /// </remarks>
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
#if NET48
        : IHtmlString
#else
        : IHtmlContent
#endif
        where TComponent : IComponent, new()
        where TModel : class
    {
        private static readonly TComponent Declaration = new TComponent();

        private readonly ReactivePlan<TModel> _plan;
        private readonly string _componentId;
#if NET48
        private readonly IHtmlString _markup;

        internal RenderedComponent(ReactivePlan<TModel> plan, string componentId)
            : this(plan, componentId, new HtmlString(string.Empty))
        {
        }

        internal RenderedComponent(ReactivePlan<TModel> plan, string componentId, IHtmlString markup)
#else
        private readonly IHtmlContent _markup;

        internal RenderedComponent(ReactivePlan<TModel> plan, string componentId)
            : this(plan, componentId, HtmlString.Empty)
        {
        }

        internal RenderedComponent(ReactivePlan<TModel> plan, string componentId, IHtmlContent markup)
#endif
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _componentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            _markup = markup ?? throw new ArgumentNullException(nameof(markup));
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

#if NET48
        /// <summary>The component markup, or nothing when its field already wrote it.</summary>
        public string ToHtmlString() => _markup.ToHtmlString();
#else
        /// <summary>Writes the component markup, or nothing when its field already wrote it.</summary>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder) => _markup.WriteTo(writer, encoder);
#endif
    }
}
