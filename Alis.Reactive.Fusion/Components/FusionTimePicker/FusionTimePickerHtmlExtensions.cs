using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating TimePickerBuilder bound to a model property.
    /// </summary>
    public static class FusionTimePickerHtmlExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        public static void TimePicker<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<TimePickerBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "timepicker",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().TimePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
