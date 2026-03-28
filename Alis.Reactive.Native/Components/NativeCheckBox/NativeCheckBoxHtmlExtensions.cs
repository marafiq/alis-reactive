using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeCheckBox"/> inside a field wrapper.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        /// <summary>
        /// Creates a <see cref="NativeCheckBoxBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the checkbox.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the checkbox (CSS, reactive events).</param>
        public static void NativeCheckBox<TModel>(
            this InputBoundField<TModel, bool> setup,
            Action<NativeCheckBoxBuilder<TModel, bool>> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "checkbox",
                CoercionTypes.InferFromType(typeof(bool))));

            var builder = new NativeCheckBoxBuilder<TModel, bool>(setup.Helper, setup.Expression);
            build(builder);
            setup.Render(builder);
        }
    }
}
