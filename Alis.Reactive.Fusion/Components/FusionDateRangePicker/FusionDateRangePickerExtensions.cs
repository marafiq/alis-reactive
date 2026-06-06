using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed value reading for <see cref="FusionDateRangePicker"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayDates).Value()</c>.
    /// </para>
    /// <para>
    /// <see cref="StartDate{TModel}"/> and <see cref="EndDate{TModel}"/> read individual
    /// dates for use in conditions. <see cref="Value{TModel}"/> reads both dates as an array.
    /// </para>
    /// <para>
    /// No <c>SetValue()</c> is provided. The date range is set by user interaction only.
    /// </para>
    /// </remarks>
    public static class FusionDateRangePickerExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        private static readonly ComponentProperty<DateTime> StartDateProperty =
            ComponentProperty<DateTime>.Named("startDate");

        private static readonly ComponentProperty<DateTime> EndDateProperty =
            ComponentProperty<DateTime>.Named("endDate");

        private static readonly ComponentProperty<DateTime[]> ValueProperty =
            ComponentProperty<DateTime[]>.Named(Component.ValueMember);

        /// <summary>Reads the start date for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard:
        /// <c>p.When(p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayDates).StartDate()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the range's start date.</returns>
        public static TypedComponentSource<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => self.Read(StartDateProperty);

        /// <summary>Reads the end date for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard:
        /// <c>p.When(p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayDates).EndDate()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the range's end date.</returns>
        public static TypedComponentSource<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => self.Read(EndDateProperty);

        /// <summary>Reads both dates as an array for use in conditions or gather.</summary>
        /// <remarks>
        /// <para>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations.
        /// </para>
        /// <para>
        /// Use <see cref="StartDate{TModel}"/> or <see cref="EndDate{TModel}"/>
        /// when you need individual date access in conditions.
        /// </para>
        /// </remarks>
        /// <returns>A typed source representing the full date range (start and end).</returns>
        public static TypedComponentSource<DateTime[]> Value<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
