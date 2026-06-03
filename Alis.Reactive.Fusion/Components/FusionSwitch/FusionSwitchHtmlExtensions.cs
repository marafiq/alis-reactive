using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionSwitch;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds rendering helpers for <see cref="FusionSwitch"/>.
    /// </summary>
    public static class FusionSwitchHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionSwitch bound to the field wrapper's boolean model property.
        /// </summary>
        public static void FusionSwitch<TModel>(
            this InputBoundField<TModel, bool> setup,
            Action<SwitchBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().SwitchFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
