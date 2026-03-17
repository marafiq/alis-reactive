using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extensions for creating NativeDatePickerBuilder.
    /// </summary>
    public static class NativeDatePickerHtmlExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static void NativeDatePicker<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeDatePickerBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr));

            var builder = new NativeDatePickerBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
