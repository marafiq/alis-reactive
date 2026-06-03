using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionCheckBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionCheckBox inside a field wrapper, bound to a boolean model property.
    /// </summary>
    public static class FusionCheckBoxHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionCheckBox bound to the field's boolean model property.
        /// </summary>
        public static void FusionCheckBox<TModel>(
            this InputBoundField<TModel, bool> setup,
            Action<CheckBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().CheckBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
