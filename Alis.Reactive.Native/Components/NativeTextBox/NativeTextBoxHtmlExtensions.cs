using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extensions for creating NativeTextBoxBuilder.
    /// </summary>
    public static class NativeTextBoxHtmlExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        public static void NativeTextBox<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeTextBoxBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "textbox",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeTextBoxBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
