using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion TimePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var medicationTime = p.Component&lt;FusionTimePicker&gt;(m =&gt; m.MedicationTime);</c> and use
    /// its members directly: <c>medicationTime.Value.Set(new DateTime(1, 1, 1, 8, 0, 0));</c> or
    /// <c>p.When(medicationTime.Value).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionTimePicker : FusionComponent, IInputComponent
    {
        /// <summary>The selected time; the picker keeps the time of day.</summary>
        public Property<DateTime> Value => Property<DateTime>();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Method();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Method();

        /// <summary>Raised when the selected time changes.</summary>
        public Event<FusionTimePickerChangeArgs> Change =>
            Event<FusionTimePickerChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionTimePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionTimePickerChangeArgs : JsObject
    {
        /// <summary>Time after the change.</summary>
        public ReadOnly<DateTime?> Value => ReadOnly<DateTime?>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionTimePicker"/> for a model-bound field.</summary>
    public static class FusionTimePickerHtmlExtensions
    {
        /// <summary>Renders a FusionTimePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTimePicker, TModel> FusionTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTimePicker(_ => { });

        /// <summary>
        /// Renders a FusionTimePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTimePicker, TModel> FusionTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<TimePickerBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().TimePickerFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionTimePicker>(builder.Render());
        }
    }
}
