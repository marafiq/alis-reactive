using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionDropDownTree;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionDropDownTree inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Syncfusion's MVC builder owns initial render configuration such as fields,
    /// tree settings, popup dimensions, templates, and selection mode.
    /// </remarks>
    public static class FusionDropDownTreeHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionDropDownTree bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the drop-down tree.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial DropDownTree options before rendering.</param>
        public static void FusionDropDownTree<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DropDownTreeBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().DropDownTreeFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
