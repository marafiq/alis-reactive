using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionTextArea;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds rendering helpers for <see cref="FusionTextArea"/>.
    /// </summary>
    public static class FusionTextAreaHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionTextArea bound to the field wrapper's model property.
        /// </summary>
        public static void FusionTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<TextAreaBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().TextAreaFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object>
                {
                    ["id"] = setup.ElementId,
                    ["name"] = setup.BindingPath
                });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
