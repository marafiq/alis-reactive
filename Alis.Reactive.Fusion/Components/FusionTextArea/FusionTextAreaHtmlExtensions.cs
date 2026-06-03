using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionTextArea;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionTextArea inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionTextAreaHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionTextArea bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the TextArea using Syncfusion's MVC builder.</param>
        public static void FusionTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TextAreaBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().TextAreaFor(setup.Expression)
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
