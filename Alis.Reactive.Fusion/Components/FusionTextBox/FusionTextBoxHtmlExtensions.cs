using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionTextBox inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionTextBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionTextBox bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the TextBox using Syncfusion's MVC builder.</param>
        public static void FusionTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TextBoxBuilder> build)
            where TModel : class
        {
            var registration = global::Alis.Reactive.Fusion.Components.FusionTextBox.Registration;
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
