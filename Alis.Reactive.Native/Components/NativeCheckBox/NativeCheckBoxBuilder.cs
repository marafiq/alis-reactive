using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input type="checkbox"&gt; element with an explicit id.
    /// Not model-bound — for UI toggles and standalone checkboxes.
    ///
    /// Usage:
    ///   @Html.NativeCheckBox("toggle-id")
    ///       .CssClass("h-4 w-4 rounded")
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.When(args, a => a.Checked).Truthy()
    ///            .Then(t => t.Element("panel").Show())
    ///            .Else(e => e.Element("panel").Hide());
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public class NativeCheckBoxBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private string? _cssClass;
        private bool _checked;

        public NativeCheckBoxBuilder(string elementId)
        {
            _elementId = elementId;
        }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets CSS classes on the checkbox element.</summary>
        public NativeCheckBoxBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Sets the initial checked state.</summary>
        public NativeCheckBoxBuilder<TModel> Checked(bool isChecked)
        {
            _checked = isChecked;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<input type=\"checkbox\"");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            if (_checked)
            {
                writer.Write(" checked");
            }
            writer.Write(" />");
        }
    }

    /// <summary>
    /// Factory extension for creating NativeCheckBoxBuilder.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        /// <summary>
        /// Creates a native &lt;input type="checkbox"&gt; builder with an explicit element ID.
        /// </summary>
        public static NativeCheckBoxBuilder<TModel> NativeCheckBox<TModel>(
            this IHtmlHelper<TModel> html, string elementId)
            where TModel : class
        {
            return new NativeCheckBoxBuilder<TModel>(elementId);
        }

        /// <summary>
        /// Creates a native &lt;input type="checkbox"&gt; builder and registers it in the plan's ComponentsMap.
        /// Use this overload on reactive pages so the plan knows about the component.
        /// </summary>
        public static NativeCheckBoxBuilder<TModel> NativeCheckBox<TModel>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            string elementId,
            string bindingPath)
            where TModel : class
        {
            plan.AddToComponentsMap(bindingPath, new ComponentRegistration(
                elementId,
                _component.Vendor,
                bindingPath,
                _component.ReadExpr));

            return new NativeCheckBoxBuilder<TModel>(elementId);
        }
    }
}
