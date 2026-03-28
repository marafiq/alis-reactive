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
    /// Creates a Syncfusion DateTimePicker inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.AppointmentTime)</c>, then call
    /// <c>.DateTimePicker(b =&gt; { b.Format("MM/dd/yyyy hh:mm a"); })</c>.
    /// </remarks>
    public static class FusionDateTimePickerHtmlExtensions
    {
        private static readonly FusionDateTimePicker Component = new FusionDateTimePicker();

        /// <summary>
        /// Renders a Syncfusion DateTimePicker bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the DateTimePicker (min/max, format, step, etc.).</param>
        public static void DateTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DateTimePickerBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "datetimepicker",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().DateTimePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
