using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion TimePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionTimePicker&gt;(m =&gt; m.MedicationTime)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, new DateTime(1, 1, 1, 8, 0, 0))</c> or <c>p.When(picker.Get(c =&gt; c.Value)).NotNull()</c>.
    /// A <see cref="DateTime"/> without a kind is the browser's local wall time.
    /// </remarks>
    public sealed class FusionTimePicker : FusionComponent, IInputComponent
    {
        /// <summary>The selected time; the picker keeps the time of day.</summary>
        public ComponentProperty<DateTime> Value => Property<DateTime>("value");

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Raised when the selected time changes.</summary>
        public TypedEvent<FusionTimePickerChangeArgs> Change =>
            Event<FusionTimePickerChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionTimePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionTimePickerChangeArgs
    {
        /// <summary>Time after the change.</summary>
        public DateTime? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionTimePicker"/> for a model-bound field.</summary>
    public static class FusionTimePickerHtmlExtensions
    {
        /// <summary>Renders a FusionTimePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTimePicker, TModel> FusionTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionTimePicker(_ => { });

        /// <summary>
        /// Renders a FusionTimePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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
