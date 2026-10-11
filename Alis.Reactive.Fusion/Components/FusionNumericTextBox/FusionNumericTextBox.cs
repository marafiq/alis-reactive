using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
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
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>.
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
        public static void FusionNumericTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NumericTextBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().NumericTextBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            field.Render<global::Alis.Reactive.Fusion.Components.FusionNumericTextBox>(builder.Render());
        }
    }

    /// <summary>Wires events and reads or updates a <see cref="FusionNumericTextBox"/> from a plan.</summary>
    public static class FusionNumericTextBoxExtensions
    {
        /// <summary>Runs a reaction when the selected event fires, for example <c>c =&gt; c.Change</c>.</summary>
        public static NumericTextBoxBuilder Reactive<TModel, TArgs>(
            this NumericTextBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionNumericTextBox, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var componentId = (string)((IDictionary<string, object>)builder.model.HtmlAttributes)["id"];
            ComponentEvents.Wire(plan, componentId, eventSelector, pipeline);
            return builder;
        }

        /// <summary>Sets the numeric value.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class => self.Set(c => c.Value, value);

        /// <summary>Sets the minimum allowed value.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class => self.Set(c => c.Min, min);

        /// <summary>Moves focus into the input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class => self.Call(c => c.FocusIn);

        /// <summary>Removes focus from the input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class => self.Call(c => c.FocusOut);

        /// <summary>Increments the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class => self.Call(c => c.Increment);

        /// <summary>Decrements the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class => self.Call(c => c.Decrement);

        /// <summary>Reads the numeric value for conditions or gather.</summary>
        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class => self.Get(c => c.Value);
    }
}
