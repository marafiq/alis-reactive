using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating DatePickerBuilder bound to a model property.
    /// </summary>
    public static class FusionDatePickerHtmlExtensions
    {
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        public static void DatePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DatePickerBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "datepicker",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().DatePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
