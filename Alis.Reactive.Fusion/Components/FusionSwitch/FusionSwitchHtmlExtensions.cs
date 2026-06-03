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
    /// Creates a FusionSwitch inside a field wrapper, bound to a boolean model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.IsActive)</c>, then call
    /// <c>.FusionSwitch(b =&gt; { b.CssClass("custom-switch"); })</c>.
    /// </remarks>
    public static class FusionSwitchHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionSwitch bound to the field's boolean model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionSwitch (label, CSS class, etc.).</param>
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
