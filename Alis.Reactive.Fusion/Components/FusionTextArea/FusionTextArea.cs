using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion TextArea, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionTextArea&gt;(m =&gt; m.ShiftNote)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow a write with
    /// <c>.Call(c =&gt; c.DataBind)</c> to apply it before the next step:
    /// <c>.Set(c =&gt; c.Value, note).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionTextArea : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>("value");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the textarea.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the textarea.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Raised on every edit while typing.</summary>
        public TypedEvent<FusionTextAreaInputArgs> Input =>
            Event<FusionTextAreaInputArgs>("input");

        /// <summary>Raised when the text changes and focus leaves the textarea.</summary>
        public TypedEvent<FusionTextAreaChangeArgs> Change =>
            Event<FusionTextAreaChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the textarea receives focus.</summary>
        public TypedEvent<FusionTextAreaFocusArgs> Focus =>
            Event<FusionTextAreaFocusArgs>("focus");

        /// <summary>Raised when the textarea loses focus.</summary>
        public TypedEvent<FusionTextAreaBlurArgs> Blur =>
            Event<FusionTextAreaBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionTextArea.Input"/>.</summary>
    public class FusionTextAreaInputArgs
    {
        /// <summary>Text reported by the input event.</summary>
        public string? Value { get; set; }

        /// <summary>Text before this edit.</summary>
        public string? PreviousValue { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextArea.Change"/>.</summary>
    public class FusionTextAreaChangeArgs
    {
        /// <summary>Text after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Previously committed text.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextArea.Focus"/>.</summary>
    public class FusionTextAreaFocusArgs
    {
        /// <summary>Text when the textarea receives focus.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Payload of <see cref="FusionTextArea.Blur"/>.</summary>
    public class FusionTextAreaBlurArgs
    {
        /// <summary>Text when the textarea loses focus.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Renders a <see cref="FusionTextArea"/> for a model-bound field.</summary>
    public static class FusionTextAreaHtmlExtensions
    {
        /// <summary>Renders a FusionTextArea with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextArea, TModel> FusionTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTextArea(_ => { });

        /// <summary>
        /// Renders a FusionTextArea bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Input, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextArea, TModel> FusionTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<TextAreaBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().TextAreaFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionTextArea>(builder.Render());
        }
    }
}
