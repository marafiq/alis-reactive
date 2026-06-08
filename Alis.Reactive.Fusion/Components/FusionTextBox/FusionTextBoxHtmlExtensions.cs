using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionTextBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds rendering helpers for <see cref="FusionTextBox"/>.
    /// </summary>
    public static class FusionTextBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionTextBox bound to the field wrapper's model property.
        /// </summary>
        public static void FusionTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TextBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().TextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object>
                {
                    ["id"] = setup.ElementId,
                    ["name"] = setup.BindingPath
                });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
