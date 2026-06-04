using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeCheckList;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// <c>InputField</c> extension for choosing <see cref="NativeCheckList"/> as the wrapper's rendered control.
    /// </summary>
    public static class NativeCheckListHtmlExtensions
    {
        /// <summary>
        /// Registers the checkbox list with the Reactive Plan and renders it inside the field wrapper.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The model value type registered as the checked option values.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the checkbox list (items, CSS, reactive events).</param>
        public static void NativeCheckList<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeCheckListBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = new NativeCheckListBuilder<TModel, TProp>(
                setup.Helper,
                setup.Expression,
                setup.RenderTarget);
            build(builder);

            setup.Render(builder);
        }
    }
}
