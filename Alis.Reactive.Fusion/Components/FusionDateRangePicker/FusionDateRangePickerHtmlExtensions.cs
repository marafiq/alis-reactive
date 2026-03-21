using System;
using System.Collections.Generic;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating DateRangePickerBuilder bound to a model property.
    /// The expression targets the start date property. The component ID is based on
    /// the start date binding path.
    /// </summary>
    public static class FusionDateRangePickerHtmlExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        public static void DateRangePicker<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<DateRangePickerBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "daterangepicker"));

            var builder = setup.Helper.EJS().DateRangePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
