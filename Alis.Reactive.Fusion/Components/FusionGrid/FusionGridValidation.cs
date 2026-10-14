using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Emits Syncfusion-native column <c>validationRules</c> from the <em>same</em>
    /// <c>ReactiveValidator</c> client metadata that powers form validation, so a Grid's
    /// in-cell editor validates client-side without a second hand-written ruleset. The
    /// validation source stays single (the validator), and EJ2's own <c>FormValidator</c>
    /// runs the rule on cell save.
    /// </summary>
    /// <remarks>
    /// Only the single-field rules EJ2 FormValidator decides as the server does are emitted:
    /// <c>required</c>, <c>minLength</c>, <c>maxLength</c>, <c>min</c>, <c>max</c>, numeric
    /// <c>range</c>, and one pattern per column. An email or a URL rule goes to the cell as EJ2's
    /// <c>regex</c> with the pattern the form tests it with, because EJ2's own <c>email</c> and
    /// <c>url</c> rules decide differently (EJ2 refuses <c>o'brien@example.com</c>, which the server
    /// accepts). Conditional (under <c>When</c>), cross-field, flagged-pattern, and other rules have no
    /// EJ2 equivalent and are checked by the server on save.
    /// <code>
    /// var columns = FusionGridColumns.Of&lt;ResidentCareItem&gt;();
    /// var care = FusionGridValidation.From&lt;ResidentCareItem&gt;(plan.ClientRules&lt;ResidentCareItemValidator&gt;());
    /// new GridColumn { Field = columns.Field(r =&gt; r.OpenTasks), EditType = "numericedit",
    ///     ValidationRules = care.Field(r =&gt; r.OpenTasks) }
    /// </code>
    /// </remarks>
    public static class FusionGridValidation
    {
        /// <summary>The column rules for grid columns over <typeparamref name="TRow"/>, from a validator's browser rules.</summary>
        /// <typeparam name="TRow">Your row class.</typeparam>
        /// <param name="rules">The row validator's browser rules: <c>plan.ClientRules&lt;ResidentCareItemValidator&gt;()</c>.</param>
        public static FusionGridFieldValidation<TRow> From<TRow>(IReadOnlyList<ClientValidationField> rules)
            where TRow : class
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            return new FusionGridFieldValidation<TRow>(rules);
        }
    }

    /// <summary>
    /// Per-field bridge from <c>ReactiveValidator</c> client metadata to an EJ2 column
    /// <c>validationRules</c> object, keyed by typed row field.
    /// </summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public sealed class FusionGridFieldValidation<TRow>
        where TRow : class
    {
        private readonly IReadOnlyList<ClientValidationField> _fields;

        internal FusionGridFieldValidation(IReadOnlyList<ClientValidationField> fields)
        {
            _fields = fields;
        }

        /// <summary>
        /// EJ2 <c>column.validationRules</c> object for a typed row field, or
        /// <see langword="null"/> when no declared rule maps client-side (every rule on
        /// the field is conditional, cross-field, or otherwise server-only).
        /// </summary>
        /// <remarks>
        /// Assign it to Syncfusion's <c>GridColumn.ValidationRules</c>, which takes any object.
        /// </remarks>
        /// <typeparam name="TField">The type of the column's member.</typeparam>
        /// <param name="field">The column's member, for example <c>r =&gt; r.OpenTasks</c>.</param>
        public IReadOnlyDictionary<string, object>? Field<TField>(Expression<Func<TRow, TField>> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var path = PropertyPathOf(field);
            var declared = _fields.FirstOrDefault(candidate => candidate.FieldName == path);
            if (declared == null)
                throw new InvalidOperationException(
                    $"No client validation rules are declared for '{path}'. " +
                    "Add a rule for it on the validator, or remove the ValidationRules emit for this column.");

            var rules = Ej2ColumnRules.From(declared);
            return rules.Count == 0 ? null : rules;
        }

        // The validator keys a field by its property path, the C# member names joined by dots.
        private static string PropertyPathOf<TField>(Expression<Func<TRow, TField>> field)
        {
            var names = new List<string>();
            var body = field.Body is UnaryExpression conversion ? conversion.Operand : field.Body;
            while (body is MemberExpression member)
            {
                names.Insert(0, member.Member.Name);
                body = member.Expression;
            }

            if (!(body is ParameterExpression) || names.Count == 0)
                throw new ArgumentException("Select a member of the row, for example r => r.OpenTasks.", nameof(field));

            return string.Join(".", names);
        }
    }

    /// <summary>
    /// Translates a field's client rules into the EJ2 FormValidator object shape
    /// <c>{ rule: [value, message] }</c>: each rule EJ2 decides as the server does, under EJ2's name.
    /// </summary>
    internal static class Ej2ColumnRules
    {
        internal static Dictionary<string, object> From(ClientValidationField field)
        {
            var rules = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var rule in field.Rules)
            {
                if (!rule.IsUnconditional) continue; // EJ2 column rules are always-on
                Add(rules, rule);
            }
            return rules;
        }

        private static void Add(Dictionary<string, object> rules, ClientRule rule)
        {
            var key = rule.Name;
            switch (key)
            {
                case "required":
                    rules[key] = new object[] { true, rule.Message };
                    break;

                case "minLength":
                case "maxLength":
                    if (rule.LiteralOperand is int length)
                        rules[key] = new object[] { length, rule.Message };
                    break;

                // EJ2 tests one pattern per column, with no flags; a second pattern is the server's.
                case "regex":
                case "email":
                case "url":
                    if (rule.TextPattern is string pattern && !rules.ContainsKey("regex"))
                        rules["regex"] = new object[] { pattern, rule.Message };
                    break;

                case "min":
                case "max":
                    if (IsNumeric(rule.LiteralOperand))
                        rules[key] = new object[] { rule.LiteralOperand!, rule.Message };
                    break;

                case "range":
                    var bounds = rule.RangeOperand;
                    if (bounds is { Length: 2 } && IsNumeric(bounds[0]) && IsNumeric(bounds[1]))
                        rules["range"] = new object[] { new[] { bounds[0], bounds[1] }, rule.Message };
                    break;

                // gt, lt, exclusiveRange, equalTo, notEqual, notEqualTo, creditCard, atLeastOne,
                // empty: no EJ2 FormValidator equivalent, so the server checks them on save.
            }
        }

        private static bool IsNumeric(object? value) =>
            value is byte or sbyte or short or ushort or int or uint or long or ulong or decimal or double or float;
    }
}
