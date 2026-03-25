using System;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Source extensions for FusionDateRangePicker (StartDate, EndDate, Value).
    ///
    /// StartDate() reads "startDate" from ej2 instance — returns single DateTime.
    /// EndDate() reads "endDate" from ej2 instance — returns single DateTime.
    /// Value() reads "value" from ej2 instance — returns DateTime[] (both dates).
    ///
    /// StartDate/EndDate use hardcoded readExpr independent of Component.ReadExpr.
    /// They are targeted sub-reads for conditions. Value() reads the component's actual value.
    /// No SetValue() — DateRangePicker is set by user interaction only.
    /// </summary>
    public static class FusionDateRangePickerExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        /// <summary>
        /// Returns a typed source reading the startDate property from the ej2 instance.
        /// Uses hardcoded readExpr "startDate" — independent of Component.ReadExpr.
        /// </summary>
        public static TypedComponentSource<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");

        /// <summary>
        /// Returns a typed source reading the endDate property from the ej2 instance.
        /// Uses hardcoded readExpr "endDate" — independent of Component.ReadExpr.
        /// </summary>
        public static TypedComponentSource<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "endDate");

        /// <summary>
        /// Returns the full value source — reads ej2.value which is [Date, Date] (DateTime[]).
        /// Use StartDate() or EndDate() for individual date access in conditions.
        /// </summary>
        public static TypedComponentSource<DateTime[]> Value<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime[]>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
