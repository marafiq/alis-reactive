using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionInPlaceEditor inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.DateOfBirth)</c>, then call
    /// <c>.FusionInPlaceEditor(b =&gt; { b.Type(InputType.Date).Mode(RenderMode.Inline); })</c>.
    /// Use the vendor builder directly for configuration (<c>Type</c>, <c>Mode</c>, <c>EditableOn</c>,
    /// <c>ShowButtons</c>, etc.) and <c>.Reactive(plan, evt =&gt; evt.ActionBegin, …)</c> to own the commit flow.
    /// </remarks>
    public static class FusionInPlaceEditorHtmlExtensions
    {
        private static readonly FusionInPlaceEditor Component = new FusionInPlaceEditor();

        /// <summary>
        /// Renders a FusionInPlaceEditor bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to configure the editor (type, mode, inner model, reactive events).</param>
        public static void FusionInPlaceEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<InPlaceEditorBuilder> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ValueMember, "inplace-editor",
                Shape.FromClrType(typeof(TProp))));

            object? initialValue = null;
            try
            {
                var compiled = setup.Expression.Compile();
                initialValue = compiled(setup.Helper.ViewData.Model);
            }
            catch
            {
                // No initial model value; editor starts empty.
            }

            var builder = setup.Helper.EJS().InPlaceEditor(setup.ElementId)
                .Name(setup.BindingPath)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId });

            if (initialValue != null)
            {
                builder = builder.Value(initialValue);
            }

            build(builder);
            setup.Render(builder.Render());
        }
    }
}
