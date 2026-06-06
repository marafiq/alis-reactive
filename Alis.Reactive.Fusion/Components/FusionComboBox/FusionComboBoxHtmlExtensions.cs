using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionComboBox;

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
        /// <typeparam name="TProp">Model value type rendered by the combo box.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial ComboBox options before rendering.</param>
        public static void FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<ComboBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().ComboBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
