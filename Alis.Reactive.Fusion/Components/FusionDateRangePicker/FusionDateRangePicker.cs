using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion DateRangePicker, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayPeriod)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>p.When(stay.Get(c =&gt; c.StartDate)).NotNull()</c>.
    /// </remarks>
    public sealed class FusionDateRangePicker : FusionComponent, IInputComponent
    {
        /// <summary>The first day of the selected range.</summary>
        public Prop<DateTime> StartDate => Prop<DateTime>();

        /// <summary>The last day of the selected range.</summary>
        public Prop<DateTime> EndDate => Prop<DateTime>();

        /// <summary>The selected range as its start and end dates.</summary>
        public Prop<DateTime[]> Value => Prop<DateTime[]>();

        /// <summary>Raised when the selected range changes.</summary>
        public TypedEvent<FusionDateRangePickerChangeArgs> Change =>
            Event<FusionDateRangePickerChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionDateRangePicker.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.StartDate).NotNull()</c>.</remarks>
    public class FusionDateRangePickerChangeArgs : JsObject
    {
        /// <summary>First day of the range after the change.</summary>
        public ReadOnly<DateTime?> StartDate => ReadOnly<DateTime?>();

        /// <summary>Last day of the range after the change.</summary>
        public ReadOnly<DateTime?> EndDate => ReadOnly<DateTime?>();

        /// <summary>Number of days the range covers.</summary>
        public ReadOnly<int> DaySpan => ReadOnly<int>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionDateRangePicker"/> for a model-bound field.</summary>
    public static class FusionDateRangePickerHtmlExtensions
    {
        /// <summary>Renders a FusionDateRangePicker with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDateRangePicker, TModel> FusionDateRangePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDateRangePicker(_ => { });

        /// <summary>
        /// Renders a FusionDateRangePicker bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDateRangePicker, TModel> FusionDateRangePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<DateRangePickerBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().DateRangePickerFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionDateRangePicker>(builder.Render());
        }
    }
}
