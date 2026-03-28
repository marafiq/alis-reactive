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
    /// Creates a Syncfusion TimePicker inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.CheckInTime)</c>, then call
    /// <c>.TimePicker(b =&gt; { b.Step(15).Format("hh:mm a"); })</c>.
    /// </remarks>
    public static class FusionTimePickerHtmlExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        /// <summary>
        /// Renders a Syncfusion TimePicker bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the TimePicker (step, min/max, format, etc.).</param>
        public static void TimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TimePickerBuilder> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "timepicker",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().TimePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
