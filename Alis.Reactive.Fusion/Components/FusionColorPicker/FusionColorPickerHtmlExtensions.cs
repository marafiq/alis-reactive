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
    /// Factory extension for creating ColorPickerBuilder bound to a model property.
    /// </summary>
    public static class FusionColorPickerHtmlExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

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
