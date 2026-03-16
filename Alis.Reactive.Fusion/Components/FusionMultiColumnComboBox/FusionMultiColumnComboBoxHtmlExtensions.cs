using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating MultiColumnComboBoxBuilder bound to a model property.
    /// </summary>
    public static class FusionMultiColumnComboBoxHtmlExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        public static MultiColumnComboBoxBuilder MultiColumnComboBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For(expression);
            var name = html.NameFor(expression);

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                Component.Vendor,
                name,
                Component.ReadExpr));

            return html.EJS().MultiColumnComboBoxFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }

        /// <summary>
        /// Typed Fields binding — derives text/value field names from DataSource item expressions.
        /// Converts PascalCase C# to camelCase (matching global Newtonsoft serialization).
        /// Usage: .Fields&lt;FacilityItem&gt;(t =&gt; t.Text, v =&gt; v.Value)
        /// </summary>
        public static MultiColumnComboBoxBuilder Fields<TItem>(
            this MultiColumnComboBoxBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MultiColumnComboBoxFieldSettings
            {
                Text = ToCamelCase(GetMemberName(text)),
                Value = ToCamelCase(GetMemberName(value))
            });
        }

        private static string GetMemberName<T>(Expression<Func<T, object?>> expr)
        {
            var body = expr.Body;
            if (body is UnaryExpression unary) body = unary.Operand;
            if (body is MemberExpression member) return member.Member.Name;
            throw new ArgumentException("Expression must be a member access (e.g., x => x.Text)");
        }

        private static string ToCamelCase(string name)
            => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
