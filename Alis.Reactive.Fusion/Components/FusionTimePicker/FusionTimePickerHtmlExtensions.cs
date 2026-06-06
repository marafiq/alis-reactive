using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Calendars;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionTimePicker;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionTimePicker inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.CheckInTime)</c>, then call
    /// <c>.FusionTimePicker(b =&gt; { b.Step(15).Format("hh:mm a"); })</c>.
    /// </remarks>
    public static class FusionTimePickerHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionTimePicker bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the time picker.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial TimePicker options before rendering.</param>
        public static void FusionTimePicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TimePickerBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().TimePickerFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
