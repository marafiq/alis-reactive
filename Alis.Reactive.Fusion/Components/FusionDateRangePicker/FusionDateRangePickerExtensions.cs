using System;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed value reading for <see cref="FusionDateRangePicker"/> in a reactive pipeline.
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

        /// <summary>Reads the start date for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard:
        /// <c>p.When(p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayDates).StartDate()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the range's start date.</returns>
        public static ComponentValueExpression<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new ComponentValueExpression<DateTime>(self.TargetId, Component.Vendor, "startDate");

        /// <summary>Reads the end date for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard:
        /// <c>p.When(p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayDates).EndDate()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the range's end date.</returns>
        public static ComponentValueExpression<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new ComponentValueExpression<DateTime>(self.TargetId, Component.Vendor, "endDate");

        /// <summary>Reads both dates as an array for use in conditions or gather.</summary>
        /// <remarks>
        /// <para>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations.
        /// </para>
        /// <para>
        /// Use <see cref="StartDate{TModel}"/> or <see cref="EndDate{TModel}"/>
        /// when you need individual date access in conditions.
        /// </para>
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the full date range (start and end).</returns>
        public static ComponentValueExpression<DateTime[]> Value<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new ComponentValueExpression<DateTime[]>(self.TargetId, Component.Vendor, Component.ValueMemberPath);
    }
}
