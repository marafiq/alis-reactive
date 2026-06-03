using System;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeCheckList;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="NativeCheckList"/> inside a field wrapper.
    /// </summary>
    public static class NativeCheckListHtmlExtensions
    {
        /// <summary>
        /// Creates a <see cref="NativeCheckListBuilder{TModel,TProp}"/> inside the field wrapper,
        /// registers the component in the plan, and renders the checkbox list.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
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
