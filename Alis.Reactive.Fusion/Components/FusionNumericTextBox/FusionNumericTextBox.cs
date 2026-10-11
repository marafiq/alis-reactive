using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion NumericTextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Amount)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, 14)</c>, <c>.Call(c =&gt; c.Increment)</c>, <c>.Get(c =&gt; c.Value)</c>.
    /// </remarks>
    public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The numeric value.</summary>
        public ComponentProperty<decimal> Value => Property<decimal>(ValueMember);

        /// <summary>The minimum allowed value.</summary>
        public ComponentProperty<decimal> Min => Property<decimal>("min");

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Increments the value by one step.</summary>
        public ComponentMethod Increment => Method("increment");

        /// <summary>Decrements the value by one step.</summary>
        public ComponentMethod Decrement => Method("decrement");

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionNumericTextBoxChangeArgs> Change =>
            Event<FusionNumericTextBoxChangeArgs>("change");

        /// <summary>Raised when the input receives focus.</summary>
        public TypedEvent<FusionNumericTextBoxFocusArgs> Focus =>
            Event<FusionNumericTextBoxFocusArgs>("focus");

        /// <summary>Raised when the input loses focus.</summary>
        public TypedEvent<FusionNumericTextBoxBlurArgs> Blur =>
            Event<FusionNumericTextBoxBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).Gte(100m)</c>.</remarks>
    public class FusionNumericTextBoxChangeArgs
    {
        /// <summary>Numeric value after the change.</summary>
        public decimal Value { get; set; }

        /// <summary>Numeric value before the change.</summary>
        public decimal PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Focus"/>; carries no data.</summary>
    public class FusionNumericTextBoxFocusArgs
    {
    }

    /// <summary>Payload of <see cref="FusionNumericTextBox.Blur"/>; carries no data.</summary>
    public class FusionNumericTextBoxBlurArgs
    {
    }

    /// <summary>Renders a <see cref="FusionNumericTextBox"/> for a model-bound field.</summary>
    public static class FusionNumericTextBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionNumericTextBox bound to the field's model property, with the framework's
        /// element ID and binding name.
        /// </summary>
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
