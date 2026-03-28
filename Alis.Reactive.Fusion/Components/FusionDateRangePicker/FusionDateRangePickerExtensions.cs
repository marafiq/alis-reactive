using System;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed value reading for <see cref="FusionDateRangePicker"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
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
        /// <returns>A typed source representing the range's start date.</returns>
        public static TypedComponentSource<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");

        /// <summary>Reads the end date for use in conditions or gather.</summary>
        /// <returns>A typed source representing the range's end date.</returns>
        public static TypedComponentSource<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "endDate");

        /// <summary>Reads both dates as an array for use in conditions or gather.</summary>
        /// <remarks>
        /// Use <see cref="StartDate{TModel}"/> or <see cref="EndDate{TModel}"/>
        /// when you need individual date access in conditions.
        /// </remarks>
        /// <returns>A typed source representing the full date range (start and end).</returns>
        public static TypedComponentSource<DateTime[]> Value<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime[]>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
