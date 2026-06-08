using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionOtpInput;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds rendering helpers for <see cref="FusionOtpInput"/>.
    /// </summary>
    public static class FusionOtpInputHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionOtpInput bound to the field wrapper's string model property.
        /// </summary>
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
