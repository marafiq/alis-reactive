using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionSlider;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionSlider inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionSliderHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionSlider bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TProp">The model value type rendered by the slider.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionSlider initial configuration.</param>
        public static void FusionSlider<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<SliderBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().Slider(setup.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
