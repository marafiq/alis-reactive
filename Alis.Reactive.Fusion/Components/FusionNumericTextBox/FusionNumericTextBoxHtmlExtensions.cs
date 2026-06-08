using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionNumericTextBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds rendering helpers for <see cref="FusionNumericTextBox"/>.
    /// </summary>
    public static class FusionNumericTextBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionNumericTextBox bound to the field wrapper's model property.
        /// </summary>
        public static void FusionNumericTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NumericTextBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().NumericTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
