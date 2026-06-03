using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeTextArea;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeTextArea"/> inside a field wrapper.
    /// </summary>
    public static class NativeTextAreaHtmlExtensions
    {
        /// <summary>
        /// Creates a <see cref="NativeTextAreaBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the textarea.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the textarea (rows, placeholder, CSS, reactive events).</param>
        public static void NativeTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NativeTextAreaBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = new NativeTextAreaBuilder<TModel, TProp>(
                setup.Helper,
                setup.Expression,
                setup.RenderTarget);
            build(builder);
            setup.Render(builder);
        }
    }
}
