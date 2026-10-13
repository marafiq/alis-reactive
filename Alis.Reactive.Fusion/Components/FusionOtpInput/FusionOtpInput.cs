using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion OTP Input, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var passcode = p.Component&lt;FusionOtpInput&gt;(m =&gt; m.Passcode);</c> and use its members
    /// directly. Syncfusion applies property writes on its next tick; follow a write with <see cref="DataBind"/>
    /// to apply it before the next step: <c>passcode.Value.Set("1357"); passcode.DataBind();</c>.
    /// </remarks>
    public sealed class FusionOtpInput : FusionComponent, IInputComponent
    {
        /// <summary>The passcode; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the first empty field.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the fields.</summary>
        public void FocusOut() => Call();

        /// <summary>Raised as a field changes while the user types.</summary>
        public TypedEvent<FusionOtpInputInputArgs> Input =>
            Event<FusionOtpInputInputArgs>("input");

        /// <summary>Raised when the complete passcode changes.</summary>
        public TypedEvent<FusionOtpInputValueChangedArgs> ValueChanged =>
            Event<FusionOtpInputValueChangedArgs>("valueChanged");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: ValueChanged);

        /// <summary>Raised when a field receives focus.</summary>
        public TypedEvent<FusionOtpInputFocusArgs> Focus =>
            Event<FusionOtpInputFocusArgs>("focus");

        /// <summary>Raised when a field loses focus.</summary>
        public TypedEvent<FusionOtpInputBlurArgs> Blur =>
            Event<FusionOtpInputBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Input"/>.</summary>
    public class FusionOtpInputInputArgs : JsObject
    {
        /// <summary>Passcode after the edit.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Passcode before the edit.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Zero-based index of the field that changed.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();
    }

    /// <summary>Payload of <see cref="FusionOtpInput.ValueChanged"/>.</summary>
    public class FusionOtpInputValueChangedArgs : JsObject
    {
        /// <summary>Passcode after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Previously committed passcode.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Focus"/>.</summary>
    public class FusionOtpInputFocusArgs : JsObject
    {
        /// <summary>Passcode when the field receives focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Zero-based index of the field that received focus.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Whether user interaction caused the focus.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionOtpInput.Blur"/>.</summary>
    public class FusionOtpInputBlurArgs : JsObject
    {
        /// <summary>Passcode when the field loses focus.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Zero-based index of the field that lost focus.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Whether user interaction caused the blur.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionOtpInput"/> for a model-bound field.</summary>
    public static class FusionOtpInputHtmlExtensions
    {
        /// <summary>Renders a FusionOtpInput with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionOtpInput, TModel> FusionOtpInput<TModel>(
            this InputBoundField<TModel, string?> field)
            where TModel : class
            => field.FusionOtpInput(_ => { });

        /// <summary>
        /// Renders a FusionOtpInput bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
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
