using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.MultiColumnComboBox;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionMultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Adds typed field mapping and rendering helpers for <see cref="FusionMultiColumnComboBox"/>.
    /// </summary>
    public static class FusionMultiColumnComboBoxHtmlExtensions
    {
        /// <summary>
        /// Maps display text and selected value fields using typed item expressions.
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

        /// <summary>
        /// Maps display text, selected value, and grouping fields using typed item expressions.
        /// </summary>
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
        /// Renders a FusionMultiColumnComboBox bound to the field wrapper's model property.
        /// </summary>
        public static void FusionMultiColumnComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<MultiColumnComboBoxBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().MultiColumnComboBoxFor(setup.Expression)
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
