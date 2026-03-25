using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating NumericTextBoxBuilder bound to a model property.
    /// </summary>
    public static class FusionNumericTextBoxHtmlExtensions
    {
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        public static void NumericTextBox<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NumericTextBoxBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "numerictextbox",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().NumericTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
