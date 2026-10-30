using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion TextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var textBox = p.Component&lt;FusionTextBox&gt;(m =&gt; m.PreferredName);</c> and use its members
    /// directly. Syncfusion applies property writes on its next tick; follow a write with <see cref="DataBind"/>
    /// to apply it before the next step: <c>textBox.Value.Set(name); textBox.DataBind();</c>.
    /// </remarks>
    public sealed class FusionTextBox : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public Property<string> Value => Property<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Method();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Method();

        /// <summary>Adds an icon to the input.</summary>
        /// <param name="position">Where the icon goes: <c>"append"</c> or <c>"prepend"</c>.</param>
        /// <param name="icons">The icon's CSS classes.</param>
        public void AddIcon(Arg<string> position, Arg<string> icons) => Method(position, icons);

        /// <summary>Raised on every edit while typing.</summary>
        public Event<FusionTextBoxInputArgs> Input =>
            Event<FusionTextBoxInputArgs>();

        /// <summary>Raised when the text changes and focus leaves the input.</summary>
        public Event<FusionTextBoxChangeArgs> Change =>
            Event<FusionTextBoxChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the input receives focus.</summary>
        public Event<FusionTextBoxFocusArgs> Focus =>
            Event<FusionTextBoxFocusArgs>();

        /// <summary>Raised when the input loses focus.</summary>
        public Event<FusionTextBoxBlurArgs> Blur =>
            Event<FusionTextBoxBlurArgs>();
    }

    /// <summary>Payload of <see cref="FusionTextBox.Input"/>.</summary>
    public class FusionTextBoxInputArgs : JsObject
    {
        /// <summary>Text reported by the input event.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Text before this edit.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionTextBox.Change"/>.</summary>
    public class FusionTextBoxChangeArgs : JsObject
    {
        /// <summary>Text after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Previously committed text.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionTextBox.Focus"/>.</summary>
    public class FusionTextBoxFocusArgs : JsObject
    {
        /// <summary>Text when the input receives focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionTextBox.Blur"/>.</summary>
    public class FusionTextBoxBlurArgs : JsObject
    {
        /// <summary>Text when the input loses focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }

    /// <summary>Renders a <see cref="FusionTextBox"/> for a model-bound field.</summary>
    public static class FusionTextBoxHtmlExtensions
    {
        /// <summary>Renders a FusionTextBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextBox, TModel> FusionTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTextBox(_ => { });

        /// <summary>
        /// Renders a FusionTextBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Input, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextBox, TModel> FusionTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<TextBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().TextBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionTextBox>(builder.Render());
        }
    }
}
