using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion NumericTextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var amount = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Amount);</c> and use its
    /// members directly: <c>amount.Value.Set(14m);</c>, <c>amount.Increment();</c> or
    /// <c>p.When(amount.Value).Gt(100m)</c>.
    /// </remarks>
    public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    {
        /// <summary>The numeric value.</summary>
        public Prop<decimal> Value => Prop<decimal>();

        /// <summary>The minimum allowed value.</summary>
        public Prop<decimal> Min => Prop<decimal>();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Call();

        /// <summary>Increments the value by one step.</summary>
        public void Increment() => Call();

        /// <summary>Decrements the value by one step.</summary>
        public void Decrement() => Call();

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionNumericTextBoxChangeArgs> Change =>
            Event<FusionNumericTextBoxChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the input receives focus.</summary>
        public TypedEvent<FusionNumericTextBoxFocusArgs> Focus =>
            Event<FusionNumericTextBoxFocusArgs>("focus");

        /// <summary>Raised when the input loses focus.</summary>
        public TypedEvent<FusionNumericTextBoxBlurArgs> Blur =>
            Event<FusionNumericTextBoxBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).Gte(100m)</c>.</remarks>
    public class FusionNumericTextBoxChangeArgs : JsObject
    {
        /// <summary>Numeric value after the change.</summary>
        public ReadOnly<decimal> Value => ReadOnly<decimal>();

        /// <summary>Numeric value before the change.</summary>
        public ReadOnly<decimal> PreviousValue => ReadOnly<decimal>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Focus"/>; carries no data.</summary>
    public class FusionNumericTextBoxFocusArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Blur"/>; carries no data.</summary>
    public class FusionNumericTextBoxBlurArgs : JsObject
    {
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionNumericTextBox"/> for a model-bound field.</summary>
    public static class FusionNumericTextBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionNumericTextBox bound to the field's model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionNumericTextBox, TModel> FusionNumericTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NumericTextBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().NumericTextBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionNumericTextBox>(builder.Render());
        }
    }
}
