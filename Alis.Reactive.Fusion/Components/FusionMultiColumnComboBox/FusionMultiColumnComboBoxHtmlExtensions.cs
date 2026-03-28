using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion MultiColumnComboBox inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionMultiColumnComboBoxHtmlExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        /// <summary>
        /// Configures text and value field mappings using typed expressions.
        /// </summary>
        /// <typeparam name="TItem">The data source item type.</typeparam>
        /// <param name="builder">The Syncfusion builder.</param>
        /// <param name="text">Expression selecting the display text property.</param>
        /// <param name="value">Expression selecting the value property.</param>
        /// <returns>The builder for method chaining.</returns>
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

        /// <summary>
        /// Configures text, value, and group-by field mappings using typed expressions.
        /// </summary>
        /// <typeparam name="TItem">The data source item type.</typeparam>
        /// <param name="builder">The Syncfusion builder.</param>
        /// <param name="text">Expression selecting the display text property.</param>
        /// <param name="value">Expression selecting the value property.</param>
        /// <param name="groupBy">Expression selecting the grouping property.</param>
        /// <returns>The builder for method chaining.</returns>
        public static MultiColumnComboBoxBuilder Fields<TItem>(
            this MultiColumnComboBoxBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new MultiColumnComboBoxFieldSettings
            {
                Text = ToCamelCase(GetMemberName(text)),
                Value = ToCamelCase(GetMemberName(value)),
                GroupBy = ToCamelCase(GetMemberName(groupBy))
            });
        }

        /// <summary>
        /// Renders a Syncfusion MultiColumnComboBox bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the MultiColumnComboBox (columns, data source, etc.).</param>
        public static void MultiColumnComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<MultiColumnComboBoxBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "multicolumncombobox",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().MultiColumnComboBoxFor(setup.Expression)
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
