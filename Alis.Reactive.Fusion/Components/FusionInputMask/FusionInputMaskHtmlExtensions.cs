using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating MaskedTextBoxBuilder bound to a model property.
    /// </summary>
    public static class FusionInputMaskHtmlExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        public static void InputMask<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<MaskedTextBoxBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "inputmask",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().MaskedTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
