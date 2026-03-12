using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input&gt; element bound to a model property.
    /// Supports type="text" (default), "number", "email", "password", etc.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
    public class NativeTextBoxBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string _type = "text";
        private string? _cssClass;
        private string? _placeholder;

        public NativeTextBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression).ToString();
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeTextBoxBuilder<TModel, TProp> Type(string type)
        {
            _type = type;
            return this;
        }

        public NativeTextBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public NativeTextBoxBuilder<TModel, TProp> Placeholder(string placeholder)
        {
            _placeholder = placeholder;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["type"] = _type
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;

            var result = _html.TextBoxFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }

    /// <summary>
    /// Factory extensions for creating NativeTextBoxBuilder.
    /// </summary>
    public static class NativeTextBoxHtmlExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        public static NativeTextBoxBuilder<TModel, TProp> NativeTextBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            return new NativeTextBoxBuilder<TModel, TProp>(html, expression);
        }

        public static NativeTextBoxBuilder<TModel, TProp> NativeTextBoxFor<TModel, TProp>(
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

            return new NativeTextBoxBuilder<TModel, TProp>(html, expression);
        }
    }
}
