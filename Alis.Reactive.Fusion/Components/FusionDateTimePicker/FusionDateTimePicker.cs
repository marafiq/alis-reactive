using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DateTimePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.MedicationTime)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, new DateTime(2026, 6, 15, 14, 30, 0))</c> or <c>p.When(picker.Get(c =&gt; c.Value)).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionDateTimePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected date and time.</summary>
        public ComponentProperty<DateTime> Value => Property<DateTime>(ValueMember);

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Raised when the selected date and time change.</summary>
        public TypedEvent<FusionDateTimePickerChangeArgs> Change =>
            Event<FusionDateTimePickerChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionDateTimePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionDateTimePickerChangeArgs
    {
        /// <summary>Date and time after the change.</summary>
        public DateTime? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionDateTimePicker"/> for a model-bound field.</summary>
    public static class FusionDateTimePickerHtmlExtensions
    {
        /// <summary>Renders a FusionDateTimePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDateTimePicker, TModel> FusionDateTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDateTimePicker(_ => { });

        /// <summary>
        /// Renders a FusionDateTimePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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
