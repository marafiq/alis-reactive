using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating SwitchBuilder bound to a model property.
    /// </summary>
    public static class FusionSwitchHtmlExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        public static void Switch<TModel>(
            this InputFieldSetup<TModel, bool> setup,
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
