using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion TextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionTextBox&gt;(m =&gt; m.PreferredName)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow a write with
    /// <c>.Call(c =&gt; c.DataBind)</c> to apply it before the next step:
    /// <c>.Set(c =&gt; c.Value, name).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionTextBox : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Call();

        /// <summary>Adds an icon: the position (<c>"append"</c> or <c>"prepend"</c>) and its CSS classes.</summary>
        public void AddIcon(Arg<string> first, Arg<string> second) => Call(first, second);

        /// <summary>Raised on every edit while typing.</summary>
        public TypedEvent<FusionTextBoxInputArgs> Input =>
            Event<FusionTextBoxInputArgs>("input");

        /// <summary>Raised when the text changes and focus leaves the input.</summary>
        public TypedEvent<FusionTextBoxChangeArgs> Change =>
            Event<FusionTextBoxChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the input receives focus.</summary>
        public TypedEvent<FusionTextBoxFocusArgs> Focus =>
            Event<FusionTextBoxFocusArgs>("focus");

        /// <summary>Raised when the input loses focus.</summary>
        public TypedEvent<FusionTextBoxBlurArgs> Blur =>
            Event<FusionTextBoxBlurArgs>("blur");
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
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionTextBox"/> for a model-bound field.</summary>
    public static class FusionTextBoxHtmlExtensions
    {
        /// <summary>Renders a FusionTextBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextBox, TModel> FusionTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTextBox(_ => { });

        /// <summary>
        /// Renders a FusionTextBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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
