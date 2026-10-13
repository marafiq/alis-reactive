using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion TextArea, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var shiftNote = p.Component&lt;FusionTextArea&gt;(m =&gt; m.ShiftNote);</c> and use its members
    /// directly. Syncfusion applies property writes on its next tick; follow a write with <see cref="DataBind"/>
    /// to apply it before the next step: <c>shiftNote.Value.Set(note); shiftNote.DataBind();</c>.
    /// </remarks>
    public sealed class FusionTextArea : FusionComponent, IInputComponent
    {
        /// <summary>The text; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the textarea.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the textarea.</summary>
        public void FocusOut() => Call();

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
    public class FusionTextAreaInputArgs : JsObject
    {
        /// <summary>Text reported by the input event.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Text before this edit.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionTextArea.Change"/>.</summary>
    public class FusionTextAreaChangeArgs : JsObject
    {
        /// <summary>Text after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Previously committed text.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionTextArea.Focus"/>.</summary>
    public class FusionTextAreaFocusArgs : JsObject
    {
        /// <summary>Text when the textarea receives focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionTextArea.Blur"/>.</summary>
    public class FusionTextAreaBlurArgs : JsObject
    {
        /// <summary>Text when the textarea loses focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionTextArea"/> for a model-bound field.</summary>
    public static class FusionTextAreaHtmlExtensions
    {
        /// <summary>Renders a FusionTextArea with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTextArea, TModel> FusionTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTextArea(_ => { });

        /// <summary>
        /// Renders a FusionTextArea bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
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
