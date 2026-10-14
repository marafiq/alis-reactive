using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DateTimePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var medicationTime = p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.MedicationTime);
    /// medicationTime.Value.Set(new DateTime(2026, 6, 15, 14, 30, 0));
    /// </code>
    /// Read it in a condition: <c>p.When(medicationTime.Value).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionDateTimePicker : FusionComponent, IInputComponent
    {
        /// <summary>The selected date and time.</summary>
        public Prop<DateTime> Value => Prop<DateTime>();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Call();

        /// <summary>Raised when the selected date and time change.</summary>
        public TypedEvent<FusionDateTimePickerChangeArgs> Change =>
            Event<FusionDateTimePickerChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionDateTimePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionDateTimePickerChangeArgs : JsObject
    {
        /// <summary>Date and time after the change.</summary>
        public ReadOnly<DateTime?> Value => ReadOnly<DateTime?>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionDateTimePicker"/> for a model-bound field.</summary>
    public static class FusionDateTimePickerHtmlExtensions
    {
        /// <summary>Renders a FusionDateTimePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDateTimePicker, TModel> FusionDateTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDateTimePicker(_ => { });

        /// <summary>
        /// Renders a FusionDateTimePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDateTimePicker, TModel> FusionDateTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<DateTimePickerBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().DateTimePickerFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionDateTimePicker>(builder.Render());
        }
    }
}
