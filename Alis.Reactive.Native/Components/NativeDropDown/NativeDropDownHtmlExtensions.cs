using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeDropDownBuilder bound to a model property.
    /// </summary>
    public static class NativeDropDownHtmlExtensions
    {
        private static readonly NativeDropDown _component = new NativeDropDown();

        public static void NativeDropDown<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeDropDownBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr));

            var builder = new NativeDropDownBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
