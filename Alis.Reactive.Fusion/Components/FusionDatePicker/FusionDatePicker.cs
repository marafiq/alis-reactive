using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DatePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.AdmissionDate)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, new DateTime(2026, 6, 15))</c> or <c>p.When(picker.Get(c =&gt; c.Value)).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionDatePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected date.</summary>
        public ComponentProperty<DateTime> Value => Property<DateTime>(ValueMember);

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Raised when the selected date changes.</summary>
        public TypedEvent<FusionDatePickerChangeArgs> Change =>
            Event<FusionDatePickerChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionDatePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionDatePickerChangeArgs
    {
        /// <summary>Date after the change.</summary>
        public DateTime? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionDatePicker"/> for a model-bound field.</summary>
    public static class FusionDatePickerHtmlExtensions
    {
        /// <summary>Renders a FusionDatePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDatePicker, TModel> FusionDatePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDatePicker(_ => { });

        /// <summary>
        /// Renders a FusionDatePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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
