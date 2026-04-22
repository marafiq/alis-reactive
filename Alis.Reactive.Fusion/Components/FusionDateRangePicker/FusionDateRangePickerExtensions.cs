using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vendor-declared named reads for <see cref="FusionDateRangePicker"/>.
    /// Typed Value&lt;TProp&gt;() (reads the bound range as an array) is provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class with cross-check
    /// against the registration shape. <see cref="StartDate{TModel}"/> and
    /// <see cref="EndDate{TModel}"/> read the vendor-declared startDate/endDate
    /// properties which are always <see cref="Shape.Date"/> regardless of binding.
    /// </summary>
    public static class FusionDateRangePickerExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        /// <summary>Reads the start date for use in conditions or gather.</summary>
        public static TypedComponentSource<DateTime> StartDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
        {
            self.Pipeline.Context.EnsureComponent(self.TargetId, Component.Vendor);
            self.Pipeline.Context.EnsureProperty(self.TargetId, "startDate", "startDate", Shape.Date, "read");
            return new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");
        }

        /// <summary>Reads the end date for use in conditions or gather.</summary>
        public static TypedComponentSource<DateTime> EndDate<TModel>(
            this ComponentRef<FusionDateRangePicker, TModel> self)
            where TModel : class
        {
            self.Pipeline.Context.EnsureComponent(self.TargetId, Component.Vendor);
            self.Pipeline.Context.EnsureProperty(self.TargetId, "endDate", "endDate", Shape.Date, "read");
            return new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "endDate");
        }
    }
}
