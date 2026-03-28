using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionDropDownList inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Country)</c>, then call
    /// <c>.FusionDropDownList(b =&gt; { b.Fields&lt;Item&gt;(t =&gt; t.Text, v =&gt; v.Value); })</c>.
    /// </remarks>
    public static class FusionDropDownListHtmlExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        /// <summary>
        /// Configures text and value field mappings using typed expressions.
        /// </summary>
        /// <remarks>
        /// Derives field names from the data source item type and converts them to camelCase
        /// to match the data source property names: <c>.Fields&lt;CountryItem&gt;(t =&gt; t.Text, v =&gt; v.Value)</c>.
        /// </remarks>
        /// <typeparam name="TItem">The data source item type.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="text">Expression selecting the display text property.</param>
        /// <param name="value">Expression selecting the value property.</param>
        /// <returns>The builder for method chaining.</returns>
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
        /// Configures text, value, and group-by field mappings using typed expressions.
        /// </summary>
        /// <remarks>
        /// Groups items in the dropdown popup:
        /// <c>.Fields&lt;CountryItem&gt;(t =&gt; t.Text, v =&gt; v.Value, g =&gt; g.Continent)</c>.
        /// </remarks>
        /// <typeparam name="TItem">The data source item type.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="text">Expression selecting the display text property.</param>
        /// <param name="value">Expression selecting the value property.</param>
        /// <param name="groupBy">Expression selecting the grouping property.</param>
        /// <returns>The builder for method chaining.</returns>
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

        /// <summary>
        /// Renders a FusionDropDownList bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the FusionDropDownList (data source, fields, etc.).</param>
        public static void FusionDropDownList<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DropDownListBuilder> build)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "dropdownlist",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().DropDownListFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
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
