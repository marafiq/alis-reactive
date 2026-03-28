using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion ColorPicker inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.ThemeColor)</c>, then call
    /// <c>.ColorPicker(b =&gt; { b.Mode(ColorPickerMode.Palette); })</c>.
    /// </remarks>
    public static class FusionColorPickerHtmlExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

        /// <summary>
        /// Renders a Syncfusion ColorPicker bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the ColorPicker (mode, columns, palette, etc.).</param>
        public static void ColorPicker<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<ColorPickerBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "colorpicker",
                CoercionTypes.InferFromType(typeof(TProp))));

            // CRITICAL: Pass htmlAttributes as a parameter to ColorPickerFor(), NOT as a fluent
            // .HtmlAttributes() call. The fluent method does not override the element ID on
            // ColorPicker — passing as a parameter bakes the custom ID into both the HTML
            // output and the JS appendTo() target.
            var attrs = new Dictionary<string, object>
            {
                ["id"] = setup.ElementId,
                ["name"] = setup.BindingPath
            };
            var builder = setup.Helper.EJS().ColorPickerFor(setup.Expression, attrs);
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
