using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeCheckBox;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeCheckBox"/> inside a field wrapper.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
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
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = new NativeCheckBoxBuilder<TModel, bool>(
                setup.Helper,
                setup.Expression,
                setup.RenderTarget);
            build(builder);
            setup.Render(builder);
        }
    }
}
