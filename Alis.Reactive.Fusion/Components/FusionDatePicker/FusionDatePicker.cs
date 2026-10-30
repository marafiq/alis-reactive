using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DatePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var admission = p.Component&lt;FusionDatePicker&gt;(m =&gt; m.AdmissionDate);
    /// admission.Value.Set(new DateTime(2026, 6, 15));
    /// </code>
    /// Read it in a condition: <c>p.When(admission.Value).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionDatePicker : FusionComponent, IInputComponent
    {
        /// <summary>The selected date.</summary>
        public Property<DateTime> Value => Property<DateTime>();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Method();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Method();

        /// <summary>Raised when the selected date changes.</summary>
        public Event<FusionDatePickerChangeArgs> Change =>
            Event<FusionDatePickerChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionDatePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionDatePickerChangeArgs : JsObject
    {
        /// <summary>Date after the change.</summary>
        public ReadOnly<DateTime?> Value => ReadOnly<DateTime?>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionDatePicker"/> for a model-bound field.</summary>
    public static class FusionDatePickerHtmlExtensions
    {
        /// <summary>Renders a FusionDatePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDatePicker, TModel> FusionDatePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDatePicker(_ => { });

        /// <summary>
        /// Renders a FusionDatePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDatePicker, TModel> FusionDatePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<DatePickerBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().DatePickerFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionDatePicker>(builder.Render());
        }
    }
}
