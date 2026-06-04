using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeRadioGroup;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// <c>InputField</c> extension for choosing <see cref="NativeRadioGroup"/> as the wrapper's rendered control.
    /// </summary>
    public static class NativeRadioGroupHtmlExtensions
    {
        /// <summary>
        /// Registers the radio group with the Reactive Plan and renders it inside the field wrapper.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The model value type registered as the selected radio value.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the radio group (items, CSS, reactive events).</param>
        public static void NativeRadioGroup<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeRadioGroupBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = new NativeRadioGroupBuilder<TModel, TProp>(
                setup.Helper,
                setup.Expression,
                setup.RenderTarget);
            build(builder);
            setup.Render(builder);
        }
    }
}
