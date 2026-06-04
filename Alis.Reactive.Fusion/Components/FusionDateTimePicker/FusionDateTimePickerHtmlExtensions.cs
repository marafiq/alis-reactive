using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionDateTimePicker;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionDateTimePicker inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.AppointmentTime)</c>, then call
    /// <c>.FusionDateTimePicker(b =&gt; { b.Format("MM/dd/yyyy hh:mm a"); })</c>.
    /// </remarks>
    public static class FusionDateTimePickerHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionDateTimePicker bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TProp">The model value type rendered by the date-time picker.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionDateTimePicker (min/max, format, step, etc.).</param>
        public static void FusionDateTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DateTimePickerBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().DateTimePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
