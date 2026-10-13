using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Writes a validation container's rules as <c>alis.form</c> data: per field its value read, the
    /// name the server reports errors under, and one test per rule. A test's check is a lambda of the
    /// field's value that is true when the value passes. Every input a rule reads is read as MVC binds
    /// it, and each check is FluentValidation's own verdict on that value (grammar.md 7.3).
    /// </summary>
    internal sealed class FormWriter
    {
        private const string Value = "v";
        private const string NetWhitespace = "\\t\\n\\v\\f\\r \\u0085\\u00A0\\u1680\\u2000-\\u200A\\u2028\\u2029\\u202F\\u205F\\u3000";

        // A rule's reads wait for no promise: it is checked on every keystroke, where nothing waits.
        private readonly Scope _scope;

        internal FormWriter(Scope scope)
        {
            _scope = scope;
        }

        internal FormPluginData Form(IReadOnlyList<CheckedField> fields) =>
            new FormPluginData(SummaryId, fields.Select(Field).ToList());

        /// <summary>The element id of the plan's validation summary.</summary>
        private string SummaryId => _scope.PlanId.Replace('.', '_').Replace('+', '_') + "_validation_summary";

        private FormField Field(CheckedField field)
        {
            var tests = field.Rules
                .Select(rule => new FormTest(rule.Name, Check(rule, field), When(rule, field.Values), rule.Message))
                .ToList();
            return new FormField(field.ComponentId, field.ServerName, field.Value.Write(_scope), tests);
        }

        private Expr When(ClientRule rule, FormFieldValues values) =>
            rule.Condition.IsAlways ? G.True : rule.Condition.ToGraph(values).Write(_scope);

        // ── checks: the rule's verdict on the field's value, as the server's rule decides it ─────

        private FnExpr Check(ClientRule rule, CheckedField field)
        {
            var fieldShape = field.Shape;
            var v = G.Ref(Value);
            var comparison = rule.ComparisonShape;
            Expr passes;
            switch (rule.Name)
            {
                case "required": passes = G.Not(BlankOrDefault(v, fieldShape)); break;
                case "empty": passes = BlankOrDefault(v, fieldShape); break;
                case "minLength": passes = G.Or(Absent(v), G.Op(Operator.Gte, TextLength(v), G.Lit(Constraint(rule)))); break;
                case "maxLength": passes = G.Or(Absent(v), G.Op(Operator.Lte, TextLength(v), G.Lit(Constraint(rule)))); break;
                case "email": passes = G.Or(Absent(v), Matches(v, "^[^@]+@[^@]+$")); break;
                case "regex": passes = G.Or(Absent(v), Pattern(v, rule)); break;
                case "url": passes = G.Or(Absent(v), Matches(v, "^https?:\\/\\/.+")); break;
                case "creditCard": passes = G.Or(Absent(v), CardNumber(v)); break;
                case "min": passes = Ordered(v, Target(rule, field.Values), comparison, Operator.Gte); break;
                case "max": passes = Ordered(v, Target(rule, field.Values), comparison, Operator.Lte); break;
                case "lt": passes = Ordered(v, Target(rule, field.Values), comparison, Operator.Lt); break;
                case "gt": passes = Ordered(v, Target(rule, field.Values), comparison, Operator.Gt); break;
                case "range": passes = Range(v, rule, comparison, Operator.Gte, Operator.Lte); break;
                case "exclusiveRange": passes = Range(v, rule, comparison, Operator.Gt, Operator.Lt); break;
                case "equalTo": passes = Same(v, Target(rule, field.Values), comparison); break;
                case "notEqual":
                case "notEqualTo": passes = G.Not(Same(v, Target(rule, field.Values), comparison)); break;
                case "atLeastOne": passes = AtLeastOne(v, fieldShape); break;
                default: throw new InvalidOperationException("No browser check for validation rule '" + rule.Name + "'.");
            }

            return G.Fn(passes, Value);
        }

        private static object Constraint(ClientRule rule) =>
            rule.Constraint ?? throw new InvalidOperationException("The rule needs a constraint value.");

        // The sibling the rule compares with, or the value the rule fixes.
        private Expr Target(ClientRule rule, FormFieldValues values) =>
            rule.Peer != null
                ? values.Value(rule.Peer).Write(_scope)
                : G.Lit(Constraint(rule));

        // A bound value FluentValidation's null checks skip: nothing bound (blank text binds as null).
        private static Expr Absent(Expr v) => G.Eq(v, G.Null);

        // Nothing was entered: a missing value, one whose text is "" (an empty list included), or one
        // with no text form. A pattern with one character of anything fails on exactly those.
        private static Expr NothingEntered(Expr v) => G.Not(Matches(v, "[\\s\\S]"));

        // What NotEmpty rejects, as far as the field's shape tells: nothing entered, text of only
        // whitespace, and the type's default: false, 0, or 0001-01-01 for a date.
        private static Expr BlankOrDefault(Expr v, Shape fieldShape)
        {
            var terms = new List<Expr> { NothingEntered(v), Matches(v, "^[" + NetWhitespace + "]*$") };
            if (fieldShape.Kind == "boolean") terms.Add(G.Eq(v, G.False));
            if (fieldShape.Kind == "number") terms.Add(G.Eq(v, G.Lit(0)));
            if (fieldShape.Kind == "date" || fieldShape.Kind == "instant") terms.Add(G.Op(Operator.Lt, G.Op(Operator.ToNumber, v), G.Lit(DayAfterEarliestDate)));
            return G.Or(terms);
        }

        // 0001-01-02T00:00:00Z: a date before it is DateTime.MinValue, in whatever zone it was read.
        private const long DayAfterEarliestDate = -62135510400000;

        private static Expr AtLeastOne(Expr v, Shape fieldShape)
        {
            var isArray = G.Eq(G.Get(v, new[] { "constructor", "name" }, Shape.None), G.Lit("Array"));
            var hasItems = G.Op(Operator.Gt, G.Get(v, new[] { "length" }, Shape.None), G.Lit(0));
            return G.Op(Operator.Cond, isArray, hasItems, G.Not(BlankOrDefault(v, fieldShape)));
        }

        private static Expr Matches(Expr v, string pattern) => G.Op(Operator.Matches, v, G.Lit(pattern), G.Lit(""));

        private static Expr Pattern(Expr v, ClientRule rule)
        {
            var patternAndFlags = (object[])Constraint(rule);
            return G.Op(Operator.Matches, v, G.Lit(patternAndFlags[0]), G.Lit(patternAndFlags[1]));
        }

        private static Expr TextLength(Expr v) => G.Get(G.Op(Operator.ToText, v), new[] { "length" }, Shape.None);

        // FluentValidation's card check: dashes and spaces dropped, every other character a digit, and
        // the Luhn sum a multiple of ten.
        private Expr CardNumber(Expr v)
        {
            var digits = _scope.Fresh("d");
            var character = _scope.Fresh("c");
            var sum = _scope.Fresh("s");
            var digit = _scope.Fresh("c");
            var index = _scope.Fresh("i");

            var separator = G.Or(G.Eq(G.Ref(character), G.Lit("-")), G.Eq(G.Ref(character), G.Lit(" ")));
            var withoutSeparators = G.Call(
                G.Call(G.Call(G.Op(Operator.ToText, v), "split", G.Lit("")), "filter", G.Fn(G.Not(separator), character)),
                "join",
                G.Lit(""));
            var value = G.Op(Operator.ToNumber, G.Ref(digit));
            var doubled = G.Op(Operator.Mul, value, G.Lit(2));
            var weighted = G.Op(Operator.Cond,
                G.Eq(G.Op(Operator.Mod, G.Ref(index), G.Lit(2)), G.Lit(1)),
                G.Op(Operator.Cond, G.Op(Operator.Gt, doubled, G.Lit(9)), G.Op(Operator.Sub, doubled, G.Lit(9)), doubled),
                value);
            var total = G.Call(
                G.Call(G.Call(G.Ref(digits), "split", G.Lit("")), "reverse"),
                "reduce",
                G.Fn(G.Op(Operator.Add, G.Ref(sum), weighted), sum, digit, index),
                G.Lit(0));
            var valid = G.And(
                Matches(G.Ref(digits), "^\\d*$"),
                G.Eq(G.Op(Operator.Mod, total, G.Lit(10)), G.Lit(0)));
            return G.Apply(G.Fn(valid, digits), withoutSeparators);
        }

        // An ordered rule passes on nothing bound, as FluentValidation's comparisons do; otherwise it
        // compares the value and its target as numbers under the comparison shape, and a pair that
        // cannot be compared (a peer with nothing bound) fails.
        private Expr Ordered(Expr v, Expr target, Shape comparison, Operator op)
        {
            var a = _scope.Fresh("a");
            var b = _scope.Fresh("b");
            var holds = G.And(IsNumber(G.Ref(a)), IsNumber(G.Ref(b)), G.Op(op, G.Ref(a), G.Ref(b)));
            var compared = G.Apply(G.Fn(holds, a, b), Comparable(v, comparison), Comparable(target, comparison));
            return G.Or(Absent(v), compared);
        }

        private Expr Range(Expr v, ClientRule rule, Shape comparison, Operator lower, Operator upper)
        {
            var bounds = Constraint(rule) as object[]
                ?? throw new InvalidOperationException("A range rule needs its two bounds.");
            var a = _scope.Fresh("a");
            var low = _scope.Fresh("l");
            var high = _scope.Fresh("h");
            var inRange = G.And(
                IsNumber(G.Ref(a)), IsNumber(G.Ref(low)), IsNumber(G.Ref(high)),
                G.Op(lower, G.Ref(a), G.Ref(low)),
                G.Op(upper, G.Ref(a), G.Ref(high)));
            var compared = G.Apply(
                G.Fn(inRange, a, low, high),
                Comparable(v, comparison),
                Comparable(G.Lit(bounds[0]), comparison),
                Comparable(G.Lit(bounds[1]), comparison));
            return G.Or(Absent(v), compared);
        }

        private static Expr IsNumber(Expr value) => G.Eq(G.Op(Operator.Finite, value), value);

        // A value as the number a comparison under the shape orders it by; anything else cannot be compared.
        private static Expr Comparable(Expr value, Shape shape)
        {
            if (shape.TryGetNullableInnerShape(out var inner))
            {
                var absent = G.Eq(value, G.Null);
                if (inner.Kind == "number" || inner.Kind == "boolean" || inner.Kind == "date" || inner.Kind == "instant")
                    absent = G.Or(absent, Matches(value, "^[" + NetWhitespace + "]*$"));
                return G.Op(Operator.Cond, absent, G.Null, Comparable(value, inner));
            }

            switch (shape.Kind)
            {
                case "number":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.Lit(0), G.Op(Operator.ToNumber, value));
                case "date":
                case "instant":
                    return G.Call(G.Op(Operator.ToDate, value), "getTime");
                case "boolean":
                    return G.Op(Operator.Cond, G.Op(Operator.ToBoolean, value), G.Lit(1), G.Lit(0));
                default:
                    return G.Null;
            }
        }

        // Equality under the comparison shape, with no skip for nothing bound, as FluentValidation's
        // Equal and NotEqual: both sides converted the way the shape converts a missing value too;
        // with no shape, both as text.
        private static Expr Same(Expr v, Expr target, Shape comparison) =>
            G.Eq(Canonical(v, comparison), Canonical(target, comparison));

        private static Expr Canonical(Expr value, Shape shape)
        {
            if (shape.TryGetNullableInnerShape(out var inner))
            {
                var absent = G.Eq(value, G.Null);
                if (inner.Kind == "number" || inner.Kind == "boolean" || inner.Kind == "date" || inner.Kind == "instant")
                    absent = G.Or(absent, Matches(value, "^[" + NetWhitespace + "]*$"));
                return G.Op(Operator.Cond, absent, G.Null, Canonical(value, inner));
            }

            switch (shape.Kind)
            {
                case "string":
                    return G.Op(Operator.Coalesce, G.Op(Operator.ToText, value), G.Lit(""));
                case "number":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.Lit(0), G.Op(Operator.ToNumber, value));
                case "boolean":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.False, G.Op(Operator.ToBoolean, value));
                case "date":
                case "instant":
                    return G.Call(G.Op(Operator.ToDate, value), "getTime");
                case "none":
                    return G.Op(Operator.Coalesce, G.Op(Operator.ToText, value), G.Lit(""));
                default:
                    return value;
            }
        }
    }
}
