using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeTextBox"/> inside a field wrapper.
    /// </summary>
    public static class NativeTextBoxHtmlExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        /// <summary>
        /// Creates a <see cref="NativeTextBoxBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the input.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Configures the text box (type, placeholder, CSS, reactive events).</param>
        public static void NativeTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
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
