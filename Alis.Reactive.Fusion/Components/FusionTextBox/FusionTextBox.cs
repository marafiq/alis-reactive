using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
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
        public ComponentProperty<string?> Value => Property<string?>("value");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Adds an icon: the position (<c>"append"</c> or <c>"prepend"</c>) and its CSS classes.</summary>
        public ComponentMethod<string, string> AddIcon => Method<string, string>("addIcon");

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
    public class FusionTextBoxInputArgs
    {
        /// <summary>Text reported by the input event.</summary>
        public string? Value { get; set; }

        /// <summary>Text before this edit.</summary>
        public string? PreviousValue { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextBox.Change"/>.</summary>
    public class FusionTextBoxChangeArgs
    {
        /// <summary>Text after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Previously committed text.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextBox.Focus"/>.</summary>
    public class FusionTextBoxFocusArgs
    {
        /// <summary>Text when the input receives focus.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextBox.Blur"/>.</summary>
    public class FusionTextBoxBlurArgs
    {
        /// <summary>Text when the input loses focus.</summary>
        public string? Value { get; set; }
    }

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
