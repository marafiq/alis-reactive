using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeRadioGroup"/> inside a field wrapper.
    /// </summary>
    public static class NativeRadioGroupHtmlExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        /// <summary>
        /// Creates a <see cref="NativeRadioGroupBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the radio group.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the radio group (items, CSS, reactive events).</param>
        public static void NativeRadioGroup<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeRadioGroupBuilder<TModel, TProp>> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "radiogroup",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeRadioGroupBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            build(builder);
            setup.Render(builder);
        }
    }
}
