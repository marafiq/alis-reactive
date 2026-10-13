using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion ColorPicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var themeColor = p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor);
    /// themeColor.Value.Set("#e11d48");
    /// themeColor.Toggle();
    /// </code>
    /// Read it in a condition: <c>p.When(themeColor.Value).NotNull()</c>.
    /// </remarks>
    public sealed class FusionColorPicker : FusionComponent, IInputComponent
    {
        /// <summary>The color as a hex string with alpha, for example <c>#e11d48ff</c>; <see langword="null"/> clears it.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Whether the picker is disabled.</summary>
        public Prop<bool> Disabled => Prop<bool>();

        /// <summary>Opens or closes the picker popup.</summary>
        public void Toggle() => Call();

        /// <summary>Raised when the color changes.</summary>
        public TypedEvent<FusionColorPickerChangeArgs> Change =>
            Event<FusionColorPickerChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionColorPicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionColorPickerChangeArgs : JsObject
    {
        /// <summary>Color after the change, as a hex string with alpha such as <c>#1dc7e1ff</c>.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionColorPicker"/> for a model-bound field.</summary>
    public static class FusionColorPickerHtmlExtensions
    {
        /// <summary>Renders a FusionColorPicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionColorPicker, TModel> FusionColorPicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionColorPicker(_ => { });

        /// <summary>
        /// Renders a FusionColorPicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
