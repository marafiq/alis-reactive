using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeCheckBox;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// <c>InputField</c> extension for choosing <see cref="NativeCheckBox"/> as the wrapper's rendered control.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        /// <summary>
        /// Registers the checkbox with the Reactive Plan and renders it inside the field wrapper.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
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
