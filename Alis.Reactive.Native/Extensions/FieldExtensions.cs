using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive.Native.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Alis.Reactive;

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
        /// The expression resolves the field name; the id is generated via IdGenerator.For().
        /// The callback receives both the expression and the generated id string.
        /// </summary>
        public static void Field<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            string label, bool isRequired,
            Expression<Func<TModel, TProp>> expression,
            Func<Expression<Func<TModel, TProp>>, string, IHtmlContent> inputBuilder)
        {
            var writer = html.ViewContext.Writer;
            var id = IdGenerator.For<TModel, TProp>(expression);
            var b = new FieldBuilder(writer, html.NameFor(expression).ToString())
                .Label(label)
                .ForId(id);
            if (isRequired) b.Required();
            using (b.Begin()) { inputBuilder(expression, id).WriteTo(writer, HtmlEncoder.Default); }
        }
    }
}
