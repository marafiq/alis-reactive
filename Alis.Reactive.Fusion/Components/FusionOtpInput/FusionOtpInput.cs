using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion OTP Input, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionOtpInput&gt;(m =&gt; m.Passcode)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow a write with
    /// <c>.Call(c =&gt; c.DataBind)</c> to apply it before the next step:
    /// <c>.Set(c =&gt; c.Value, "1357").Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionOtpInput : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The passcode; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the first empty field.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the fields.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Raised as a field changes while the user types.</summary>
        public TypedEvent<FusionOtpInputInputArgs> Input =>
            Event<FusionOtpInputInputArgs>("input");

        /// <summary>Raised when the complete passcode changes.</summary>
        public TypedEvent<FusionOtpInputValueChangedArgs> ValueChanged =>
            Event<FusionOtpInputValueChangedArgs>("valueChanged");

        /// <summary>Raised when a field receives focus.</summary>
        public TypedEvent<FusionOtpInputFocusArgs> Focus =>
            Event<FusionOtpInputFocusArgs>("focus");

        /// <summary>Raised when a field loses focus.</summary>
        public TypedEvent<FusionOtpInputBlurArgs> Blur =>
            Event<FusionOtpInputBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Input"/>.</summary>
    public class FusionOtpInputInputArgs
    {
        /// <summary>Passcode after the edit.</summary>
        public string? Value { get; set; }

        /// <summary>Passcode before the edit.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Zero-based index of the field that changed.</summary>
        public int Index { get; set; }
    }

    /// <summary>Payload of <see cref="FusionOtpInput.ValueChanged"/>.</summary>
    public class FusionOtpInputValueChangedArgs
    {
        /// <summary>Passcode after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Previously committed passcode.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Focus"/>.</summary>
    public class FusionOtpInputFocusArgs
    {
        /// <summary>Passcode when the field receives focus.</summary>
        public string? Value { get; set; }

        /// <summary>Zero-based index of the field that received focus.</summary>
        public int Index { get; set; }

        /// <summary>Whether user interaction caused the focus.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Blur"/>.</summary>
    public class FusionOtpInputBlurArgs
    {
        /// <summary>Passcode when the field loses focus.</summary>
        public string? Value { get; set; }

        /// <summary>Zero-based index of the field that lost focus.</summary>
        public int Index { get; set; }

        /// <summary>Whether user interaction caused the blur.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionOtpInput"/> for a model-bound field.</summary>
    public static class FusionOtpInputHtmlExtensions
    {
        /// <summary>Renders a FusionOtpInput with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionOtpInput, TModel> FusionOtpInput<TModel>(
            this InputBoundField<TModel, string?> field)
            where TModel : class
            => field.FusionOtpInput(_ => { });

        /// <summary>
        /// Renders a FusionOtpInput bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Input, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionOtpInput, TModel> FusionOtpInput<TModel>(
            this InputBoundField<TModel, string?> field,
            Action<OtpInputBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().OtpInput(field.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionOtpInput>(builder.Render());
        }
    }
}
