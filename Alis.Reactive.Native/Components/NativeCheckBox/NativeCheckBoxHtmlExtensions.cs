using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeCheckBoxBuilder bound to a model property.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        public static void NativeCheckBox<TModel>(
            this InputFieldSetup<TModel, bool> setup,
            Action<NativeCheckBoxBuilder<TModel, bool>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "checkbox"));

            var builder = new NativeCheckBoxBuilder<TModel, bool>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
