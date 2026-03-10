using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive.Native.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Html.Field() — renders a model-bound form field with label, required marker,
    /// the control (provided by the caller), and a validation error slot.
    /// Label and required are always explicit parameters — no model metadata inspection.
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>
        /// Renders a model-bound form field.
        /// The expression resolves the field name and id for label + validation wiring.
        /// </summary>
        public static void Field<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            string label, bool isRequired,
            Expression<Func<TModel, TProp>> expression,
            Func<Expression<Func<TModel, TProp>>, IHtmlContent> inputBuilder)
        {
            var writer = html.ViewContext.Writer;
            var b = new FieldBuilder(writer, html.NameFor(expression).ToString())
                .Label(label)
                .ForId(html.IdFor(expression).ToString());
            if (isRequired) b.Required();
            using (b.Begin()) { inputBuilder(expression).WriteTo(writer, HtmlEncoder.Default); }
        }

        /// <summary>
        /// Renders a model-bound form field with a prefix applied to the element ID.
        /// The label's <c>for</c> attribute and the callback's <c>id</c> parameter
        /// use the prefixed ID, while <c>data-valmsg-for</c> uses the unprefixed model name.
        /// </summary>
        public static void Field<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            string label, bool isRequired,
            Expression<Func<TModel, TProp>> expression,
            string idPrefix,
            Func<Expression<Func<TModel, TProp>>, string, IHtmlContent> inputBuilder)
        {
            var writer = html.ViewContext.Writer;
            var name = html.NameFor(expression).ToString();
            var id = idPrefix + html.IdFor(expression);
            var b = new FieldBuilder(writer, name)
                .Label(label)
                .ForId(id);
            if (isRequired) b.Required();
            using (b.Begin()) { inputBuilder(expression, id).WriteTo(writer, HtmlEncoder.Default); }
        }
    }
}
