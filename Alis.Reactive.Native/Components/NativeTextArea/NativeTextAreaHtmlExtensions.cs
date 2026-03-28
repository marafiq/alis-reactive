using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extensions for creating NativeTextAreaBuilder.
    /// </summary>
    public static class NativeTextAreaHtmlExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static void NativeTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeTextAreaBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "textarea",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeTextAreaBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
