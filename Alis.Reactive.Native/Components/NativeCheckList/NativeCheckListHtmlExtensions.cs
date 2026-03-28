using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeCheckList"/> inside a field wrapper.
    /// </summary>
    public static class NativeCheckListHtmlExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        /// <summary>
        /// Creates a <see cref="NativeCheckListBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the checkbox list.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Configures the checkbox list (items, CSS, reactive events).</param>
        public static void NativeCheckList<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeCheckListBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr, "checklist",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = new NativeCheckListBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);

            setup.Render(builder);
        }
    }
}
