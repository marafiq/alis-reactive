using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion Switch inside a field wrapper, bound to a boolean model property.
    /// </summary>
    public static class FusionSwitchHtmlExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        /// <summary>
        /// Renders a Syncfusion Switch bound to the field's boolean model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the Switch (label, CSS class, etc.).</param>
        public static void Switch<TModel>(
            this InputBoundField<TModel, bool> setup,
            Action<SwitchBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "switch",
                CoercionTypes.InferFromType(typeof(bool))));

            var builder = setup.Helper.EJS().SwitchFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
