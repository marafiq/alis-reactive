using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    // ── Non-model-bound: UI toggles with explicit string ID ──

    /// <summary>
    /// Renders a native HTML &lt;input type="checkbox"&gt; with an explicit id.
    /// For UI toggles that don't carry model data.
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

        internal string ElementId => _elementId;

        public NativeCheckBoxBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

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

    // ── Model-bound: form fields with IdGenerator id + MVC name ──

    /// <summary>
    /// Renders a native HTML &lt;input type="checkbox"&gt; bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
    public class NativeCheckBoxBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string? _cssClass;

        public NativeCheckBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression).ToString();
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeCheckBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<input type=\"checkbox\"");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" name=\"");
            writer.Write(encoder.Encode(_bindingPath));
            writer.Write("\"");
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            // Resolve checked state from model
            try
            {
                var compiled = _expression.Compile();
                var model = _html.ViewData.Model;
                if (model != null)
                {
                    var value = compiled(model);
                    if (value is true)
                    {
                        writer.Write(" checked");
                    }
                }
            }
            catch
            {
                // Model not available
            }
            writer.Write(" />");
        }
    }

    /// <summary>
    /// Factory extensions for creating NativeCheckBoxBuilder.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        // ── Non-model-bound (UI toggles) ──

        public static NativeCheckBoxBuilder<TModel> NativeCheckBox<TModel>(
            this IHtmlHelper<TModel> html, string elementId)
            where TModel : class
        {
            return new NativeCheckBoxBuilder<TModel>(elementId);
        }

        // ── Model-bound ──

        public static NativeCheckBoxBuilder<TModel, TProp> NativeCheckBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            return new NativeCheckBoxBuilder<TModel, TProp>(html, expression);
        }

        public static NativeCheckBoxBuilder<TModel, TProp> NativeCheckBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression).ToString();

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return new NativeCheckBoxBuilder<TModel, TProp>(html, expression);
        }
    }
}
