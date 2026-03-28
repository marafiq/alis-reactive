using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion RichTextEditor inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Notes)</c>, then call
    /// <c>.RichTextEditor(b =&gt; { b.Height(200); })</c>.
    /// </remarks>
    public static class FusionRichTextEditorHtmlExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        /// <summary>
        /// Renders a Syncfusion RichTextEditor bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the RichTextEditor (toolbar, iframe mode, etc.).</param>
        public static void RichTextEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<RichTextEditorBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "richtexteditor",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().RichTextEditorFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });

            // Override the SF-derived Id BEFORE configure so .Reactive() can read it.
            // RichTextEditorFor sets model.Id from the expression member name, but
            // Render() uses model.Id for the textarea's id attribute AND for the
            // Script Manager's appendTo selector. Setting model.Id ensures a single,
            // correct id attribute and proper SF component initialization.
            builder.model.Id = setup.ElementId;

            configure(builder);

            // RTE Render() writes directly to Output — set it to the field wrapper's writer.
            builder.Output = setup.Writer;
            setup.Render(() => builder.Render());
        }
    }
}
