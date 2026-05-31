using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive;
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
    /// Only the single-field rules that map directly to EJ2 FormValidator are emitted
    /// (<c>required</c>, <c>minLength</c>, <c>maxLength</c>, <c>email</c>, <c>url</c>,
    /// <c>regex</c>, <c>min</c>, <c>max</c>, numeric <c>range</c>). Conditional
    /// (<c>WhenField</c>), cross-field, and exotic rules carry no EJ2 equivalent and stay
    /// server-authoritative on save.
    /// <code>
    /// @inject Alis.Reactive.Validation.IClientValidationRuleSource ClientRules
    /// @{ var care = FusionGridValidation.From&lt;ResidentCareItemValidator, ResidentCareItem&gt;(ClientRules); }
    /// new GridColumn { Field = "openTasks", EditType = "numericedit",
    ///     ValidationRules = care.Field(r =&gt; r.OpenTasks) }
    /// </code>
    /// </remarks>
    public static class FusionGridValidation
    {
        /// <summary>
        /// Reads the client validation metadata for <typeparamref name="TValidator"/> and
        /// returns a per-field emitter for grid columns over <typeparamref name="TRow"/>.
        /// </summary>
        public static FusionGridFieldValidation<TRow> From<TValidator, TRow>(IClientValidationRuleSource source)
            where TValidator : class
            where TRow : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new FusionGridFieldValidation<TRow>(source.GetClientRules(typeof(TValidator)));
        }
    }

    /// <summary>
    /// Per-field bridge from <c>ReactiveValidator</c> client metadata to an EJ2 column
    /// <c>validationRules</c> object, keyed by typed row field.
    /// </summary>
    public sealed class FusionGridFieldValidation<TRow>
        where TRow : class
    {
        private readonly IReadOnlyList<ClientValidationField> _fields;

        internal FusionGridFieldValidation(IReadOnlyList<ClientValidationField> fields)
        {
            _fields = fields;
        }

        /// <summary>
        /// The EJ2 <c>column.validationRules</c> object for a typed row field, or
        /// <see langword="null"/> when no declared rule maps client-side (every rule on
        /// the field is conditional, cross-field, or otherwise server-only).
        /// </summary>
        public object? Field<TField>(Expression<Func<TRow, TField>> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var path = ExpressionPathHelper.ToPropertyName(field);
            var declared = _fields.FirstOrDefault(candidate => candidate.FieldName == path);
            if (declared == null)
                throw new InvalidOperationException(
                    $"No client validation rules are declared for '{path}'. " +
                    "Add a ClientRule for it on the validator, or remove the ValidationRules emit for this column.");

            var rules = Ej2ColumnRules.From(declared);
            return rules.Count == 0 ? null : rules;
        }
    }

    /// <summary>
    /// Translates a field's client rules into the EJ2 FormValidator object shape
    /// <c>{ rule: [value, message] }</c>. The validator rule names are already EJ2-shaped
    /// (<c>required</c>, <c>minLength</c>, <c>range</c>, ...), so each maps by name.
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
            var key = rule.Name.Value;
            switch (key)
            {
                case "required":
                case "email":
                case "url":
                    rules[key] = new object[] { true, rule.Message };
                    break;

                case "minLength":
                case "maxLength":
                    if (rule.LiteralOperand is int length)
                        rules[key] = new object[] { length, rule.Message };
                    break;

                case "regex":
                    if (rule.LiteralOperand is string pattern)
                        rules[key] = new object[] { pattern, rule.Message };
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

                // gt, lt, exclusiveRange, equalTo, notEqual, notEqualTo, creditCard,
                // atLeastOne, empty: no EJ2 FormValidator equivalent — server-authoritative.
            }
        }

        private static bool IsNumeric(object? value) =>
            value is byte or sbyte or short or ushort or int or uint or long or ulong or decimal or double or float;
    }
}
