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
    /// Creates a FusionNumericTextBox inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Quantity)</c>, then call
    /// <c>.FusionNumericTextBox(b =&gt; { b.Min(0).Max(100).Step(1); })</c>.
    /// </remarks>
    public static class FusionNumericTextBoxHtmlExtensions
    {
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        /// <summary>
        /// Renders a FusionNumericTextBox bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionNumericTextBox (min, max, step, format, etc.).</param>
        public static void FusionNumericTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<NumericTextBoxBuilder> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "numerictextbox",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().NumericTextBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
