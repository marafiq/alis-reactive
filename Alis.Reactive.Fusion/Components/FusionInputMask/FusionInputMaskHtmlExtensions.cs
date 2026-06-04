using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionInputMask;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionInputMask inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Phone)</c>, then call
    /// <c>.FusionInputMask(b =&gt; { b.Mask("(999) 000-0000"); })</c>.
    /// </remarks>
    public static class FusionInputMaskHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionInputMask bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TProp">The model value type rendered by the masked input.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the MaskedTextBox (mask format, placeholder, etc.).</param>
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
