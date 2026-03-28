using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeTextArea"/> inside a field wrapper.
    /// </summary>
    public static class NativeTextAreaHtmlExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        /// <summary>
        /// Creates a <see cref="NativeTextAreaBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the textarea.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the textarea (rows, placeholder, CSS, reactive events).</param>
        public static void NativeTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeTextAreaBuilder<TModel, TProp>> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "textarea",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeTextAreaBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            build(builder);
            setup.Render(builder);
        }
    }
}
