using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ColorPicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "#e11d48")</c>, <c>.Call(c =&gt; c.Toggle)</c> or <c>.Get(c =&gt; c.Value)</c>.
    /// </remarks>
    public sealed class FusionColorPicker : FusionComponent, IInputComponent
    {
        /// <summary>The color as a hex string with alpha, for example <c>#e11d48ff</c>; <see langword="null"/> clears it.</summary>
        public ComponentProperty<string?> Value => Property<string?>("value");

        /// <summary>Whether the picker is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>Opens or closes the picker popup.</summary>
        public ComponentMethod Toggle => Method("toggle");

        /// <summary>Raised when the color changes.</summary>
        public TypedEvent<FusionColorPickerChangeArgs> Change =>
            Event<FusionColorPickerChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionColorPicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionColorPickerChangeArgs
    {
        /// <summary>Color after the change, as a hex string with alpha such as <c>#1dc7e1ff</c>.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Renders a <see cref="FusionColorPicker"/> for a model-bound field.</summary>
    public static class FusionColorPickerHtmlExtensions
    {
        /// <summary>Renders a FusionColorPicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionColorPicker, TModel> FusionColorPicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionColorPicker(_ => { });

        /// <summary>
        /// Renders a FusionColorPicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionColorPicker, TModel> FusionColorPicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<ColorPickerBuilder> build)
            where TModel : class
        {
            // Pass the attributes to ColorPickerFor rather than chaining HtmlAttributes: the fluent call
            // does not override the element ID, and the parameter sets it on both the markup and the
            // script's appendTo target.
            var attributes = new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath };
            var builder = field.Helper.EJS().ColorPickerFor(field.Expression, attributes);
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionColorPicker>(builder.Render());
        }
    }
}
