using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeDropDown"/> inside a field wrapper.
    /// </summary>
    public static class NativeDropDownHtmlExtensions
    {
        private static readonly NativeDropDown _component = new NativeDropDown();

        /// <summary>
        /// Creates a <see cref="NativeDropDownBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the dropdown.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the dropdown (items, placeholder, CSS, reactive events).</param>
        public static void NativeDropDown<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeDropDownBuilder<TModel, TProp>> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "dropdown",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeDropDownBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            build(builder);
            setup.Render(builder);
        }
    }
}
