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
    /// Factory extension for creating RichTextEditorBuilder bound to a model property.
    ///
    /// SF RichTextEditorBuilder.Render() differs from simpler input builders:
    /// 1. It writes directly to Output (TextWriter) — throws NullRef if null.
    /// 2. It emits id={model.Id} from the expression, then appends HtmlAttributes,
    ///    causing duplicate id attributes. We fix this by overriding model.Id
    ///    to match our IdGenerator-based element ID.
    /// </summary>
    public static class FusionRichTextEditorHtmlExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

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
