using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeDropDown;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeDropDown"/> inside a field wrapper.
    /// </summary>
    public static class NativeDropDownHtmlExtensions
    {
        /// <summary>
        /// Creates a <see cref="NativeDropDownBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the Reactive Plan, and renders the dropdown.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the dropdown (items, placeholder, CSS, reactive events).</param>
        public static void NativeDropDown<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeDropDownBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = new NativeDropDownBuilder<TModel, TProp>(
                setup.Helper,
                setup.Expression,
                setup.RenderTarget);
            build(builder);
            setup.Render(builder);
        }
    }
}
