using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionOtpInput;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionOtpInput inside a field wrapper, bound to a string model property.
    /// </summary>
    public static class FusionOtpInputHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionOtpInput bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionOtpInput initial configuration.</param>
        public static void FusionOtpInput<TModel>(
            this InputBoundField<TModel, string?> setup,
            Action<OtpInputBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().OtpInput(setup.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
