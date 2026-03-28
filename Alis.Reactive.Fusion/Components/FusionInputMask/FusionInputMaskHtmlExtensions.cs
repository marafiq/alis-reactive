using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion MaskedTextBox inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Phone)</c>, then call
    /// <c>.InputMask(b =&gt; { b.Mask("(999) 000-0000"); })</c>.
    /// </remarks>
    public static class FusionInputMaskHtmlExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        /// <summary>
        /// Renders a Syncfusion MaskedTextBox bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the MaskedTextBox (mask format, placeholder, etc.).</param>
        public static void InputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<MaskedTextBoxBuilder> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "inputmask",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().MaskedTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
