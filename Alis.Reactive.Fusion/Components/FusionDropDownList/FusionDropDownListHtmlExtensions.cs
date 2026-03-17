using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating DropDownListBuilder bound to a model property.
    /// </summary>
    public static class FusionDropDownListHtmlExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        /// <summary>
        /// Typed Fields binding — derives text/value field names from DataSource item expressions.
        /// Converts PascalCase C# member names to camelCase (matching global Newtonsoft serialization).
        /// Usage: .Fields&lt;CountryItem&gt;(t =&gt; t.Text, v =&gt; v.Value)
        /// </summary>
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new DropDownListFieldSettings
            {
                Text = ToCamelCase(GetMemberName(text)),
                Value = ToCamelCase(GetMemberName(value))
            });
        }

        /// <summary>
        /// Typed Fields with GroupBy — groups items in the dropdown popup.
        /// Usage: .Fields&lt;CountryItem&gt;(t =&gt; t.Text, v =&gt; v.Value, g =&gt; g.Continent)
        /// </summary>
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new DropDownListFieldSettings
            {
                Text = ToCamelCase(GetMemberName(text)),
                Value = ToCamelCase(GetMemberName(value)),
                GroupBy = ToCamelCase(GetMemberName(groupBy))
            });
        }

        public static void DropDownList<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<DropDownListBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr));

            var builder = setup.Helper.EJS().DropDownListFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
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
