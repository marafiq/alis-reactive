using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionInputMask;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Renders the masked input component inside a bound input field.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Phone)</c>, then call
    /// <c>.FusionInputMask(b =&gt; { b.Mask("(999) 000-0000"); })</c>.
    /// </remarks>
    public static class FusionInputMaskHtmlExtensions
    {
        /// <summary>
        /// Renders the masked input component bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the masked input.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial masked-input options before rendering.</param>
        public static void FusionInputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<MaskedTextBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().MaskedTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
