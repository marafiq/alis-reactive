using System;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Source extensions for FusionDateRangePicker (StartDate, EndDate, Value).
    ///
    /// UNIQUE: This component exposes TWO readable properties from the ej2 instance.
    /// StartDate() reads "startDate", EndDate() reads "endDate".
    /// Value() is an alias for StartDate() (primary read).
    /// No SetValue() — DateRangePicker is set by user interaction only.
    /// </summary>
    public static class FusionDateRangePickerExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        /// <summary>
        /// Returns a typed source reading the startDate property from the ej2 instance.
        /// </summary>
        public static TypedComponentSource<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");

        /// <summary>
        /// Returns a typed source reading the endDate property from the ej2 instance.
        /// </summary>
        public static TypedComponentSource<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "endDate");

        /// <summary>
        /// Returns the primary read source (startDate). Alias for StartDate().
        /// </summary>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");
    }
}
