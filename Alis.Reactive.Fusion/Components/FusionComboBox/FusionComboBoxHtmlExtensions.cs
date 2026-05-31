using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionComboBox inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Syncfusion's MVC builder owns initial render configuration such as data source,
    /// field names, placeholder, popup size, and allow-custom behavior.
    /// </remarks>
    public static class FusionComboBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionComboBox bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the Syncfusion ComboBox.</param>
        public static void FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<ComboBoxBuilder> build)
            where TModel : class
        {
            var registration = global::Alis.Reactive.Fusion.Components.FusionComboBox.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().ComboBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
