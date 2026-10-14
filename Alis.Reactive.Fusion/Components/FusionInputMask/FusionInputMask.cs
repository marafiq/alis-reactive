using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion MaskedTextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Target it with <c>var phone = p.Component&lt;FusionInputMask&gt;(m =&gt; m.PhoneNumber)</c>, then write or
    /// read its members, for example <c>phone.Value.Set("(555) 123-4567")</c> or
    /// <c>p.When(phone.Value).NotEmpty()</c>.
    /// </remarks>
    public sealed class FusionInputMask : FusionComponent, IInputComponent
    {
        /// <summary>The value without the mask's literal characters; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionInputMaskChangeArgs> Change =>
            Event<FusionInputMaskChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionInputMask.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionInputMaskChangeArgs : JsObject
    {
        /// <summary>Value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionInputMask"/> for a model-bound field.</summary>
    public static class FusionInputMaskHtmlExtensions
    {
        /// <summary>Renders a FusionInputMask with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInputMask, TModel> FusionInputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionInputMask(_ => { });

        /// <summary>
        /// Renders a FusionInputMask bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        /// <param name="field">The field from <c>Html.InputField(plan, m =&gt; m.PhoneNumber)</c>.</param>
        /// <param name="build">Configures the mask and the rest on the Syncfusion builder.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInputMask, TModel> FusionInputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<MaskedTextBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().MaskedTextBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionInputMask>(builder.Render());
        }
    }
}
