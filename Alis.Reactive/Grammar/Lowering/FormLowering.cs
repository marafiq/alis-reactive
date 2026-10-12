using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// Lowers a validation container's rules to <c>alis.form</c> data: per field its value read, the
    /// name the server reports errors under, and one test per rule. A test's check is a lambda of the
    /// field's value that is true when the value passes.
    /// </summary>
    internal sealed class FormLowering
    {
        private const string Value = "v";
        private const string NetWhitespace = "\\t\\n\\v\\f\\r \\u0085\\u00A0\\u1680\\u2000-\\u200A\\u2028\\u2029\\u202F\\u205F\\u3000";

        private readonly LoweringContext _context;
        private readonly ValueLowering _values;
        private readonly LoweringScope _scope = LoweringScope.ForHandler("$form");

        internal FormLowering(LoweringContext context, ValueLowering values)
        {
            _context = context;
            _values = values;
        }

        internal FormPluginData Form(IReadOnlyList<ComponentValidation> validations) =>
            new FormPluginData(_context.SummaryId, validations.Select(Field).ToList());

        private FormField Field(ComponentValidation validation)
        {
            var fieldShape = validation.Value.OutputShape;
            var tests = validation.Rules
                .Select(rule => new FormTest(rule.Name, Check(rule, fieldShape), When(rule.Execution.Activation), rule.Message))
                .ToList();
            return new FormField(validation.Component, validation.ServerFieldName, _values.Value(validation.Value, _scope), tests);
        }

        private Expr When(ValidationRuleActivation activation) =>
            activation.TryGetCondition(out var condition) ? _values.Condition(condition, _scope) : G.True;

        // ── checks: the rule's verdict on the field's value, as the server's rule decides it ─────

        private FnExpr Check(ValidationRuleNode rule, Shape fieldShape)
        {
            var execution = rule.Execution;
            var v = G.Ref(Value);
            var comparison = execution.ComparisonShape;
            Expr passes;
            switch (rule.Name)
            {
                case "required": passes = G.Not(BlankOrDefault(v, fieldShape)); break;
                case "empty": passes = BlankOrDefault(v, fieldShape); break;
                case "minLength": passes = G.Or(NothingEntered(v), G.Op(Operator.Gte, TextLength(v), G.Lit(Constraint(execution)))); break;
                case "maxLength": passes = G.Or(NothingEntered(v), G.Op(Operator.Lte, TextLength(v), G.Lit(Constraint(execution)))); break;
                case "email": passes = G.Or(NothingEntered(v), Matches(v, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")); break;
                case "regex": passes = G.Or(NothingEntered(v), Pattern(v, execution)); break;
                case "url": passes = G.Or(NothingEntered(v), Matches(v, "^https?:\\/\\/.+")); break;
                case "creditCard": passes = G.Or(NothingEntered(v), Luhn(v)); break;
                case "min": passes = Ordered(v, Target(execution), comparison, Operator.Gte, skipsEmpty: true); break;
                case "max": passes = Ordered(v, Target(execution), comparison, Operator.Lte, skipsEmpty: true); break;
                case "lt": passes = Ordered(v, Target(execution), comparison, Operator.Lt, skipsEmpty: true); break;
                case "gt": passes = Ordered(v, Target(execution), comparison, Operator.Gt, skipsEmpty: false); break;
                case "range": passes = Range(v, execution, comparison, Operator.Gte, Operator.Lte); break;
                case "exclusiveRange": passes = Range(v, execution, comparison, Operator.Gt, Operator.Lt); break;
                case "equalTo": passes = G.Or(NothingEntered(v), Same(v, Target(execution), comparison)); break;
                case "notEqual":
                case "notEqualTo": passes = G.Or(NothingEntered(v), G.Not(Same(v, Target(execution), comparison))); break;
                case "atLeastOne": passes = AtLeastOne(v, fieldShape); break;
                default: throw new InvalidOperationException("No browser check for validation rule '" + rule.Name + "'.");
            }

            return G.Fn(passes, Value);
        }

        private static object Constraint(ValidationRuleExecution execution) =>
            execution.TryGetConstraint(out var constraint) && constraint.Value != null
                ? constraint.Value
                : throw new InvalidOperationException("The rule needs a constraint value.");

        private Expr Target(ValidationRuleExecution execution) =>
            execution.TryGetPeer(out var peer)
                ? _values.Value(peer, _scope)
                : G.Lit(Constraint(execution));

        // Nothing was entered: a missing value, one whose text is "" (an empty list included), or one
        // with no text form. A pattern with one character of anything fails on exactly those.
        private static Expr NothingEntered(Expr v) => G.Not(Matches(v, "[\\s\\S]"));

        // What NotEmpty rejects, as far as the field's shape tells: nothing entered, text of only
        // whitespace, false for a boolean, 0 for a number.
        private static Expr BlankOrDefault(Expr v, Shape fieldShape)
        {
            var terms = new List<Expr> { NothingEntered(v), Matches(v, "^[" + NetWhitespace + "]*$") };
            if (fieldShape.Kind == "boolean") terms.Add(G.Eq(v, G.False));
            if (fieldShape.Kind == "number") terms.Add(G.Eq(v, G.Lit(0)));
            return G.Or(terms);
        }

        private static Expr AtLeastOne(Expr v, Shape fieldShape)
        {
            var isArray = G.Eq(G.Get(v, new[] { "constructor", "name" }, Shape.None), G.Lit("Array"));
            var hasItems = G.Op(Operator.Gt, G.Get(v, new[] { "length" }, Shape.None), G.Lit(0));
            return G.Op(Operator.Cond, isArray, hasItems, G.Not(BlankOrDefault(v, fieldShape)));
        }

        private static Expr Matches(Expr v, string pattern) => G.Op(Operator.Matches, v, G.Lit(pattern), G.Lit(""));

        private static Expr Pattern(Expr v, ValidationRuleExecution execution)
        {
            var patternAndFlags = (object[])Constraint(execution);
            return G.Op(Operator.Matches, v, G.Lit(patternAndFlags[0]), G.Lit(patternAndFlags[1]));
        }

        private static Expr TextLength(Expr v) => G.Get(G.Op(Operator.ToText, v), new[] { "length" }, Shape.None);

        // Card numbers: the digits only, at least 13, passing the Luhn check.
        private Expr Luhn(Expr v)
        {
            var digits = _context.Fresh("d");
            var character = _context.Fresh("c");
            var sum = _context.Fresh("s");
            var digit = _context.Fresh("c");
            var index = _context.Fresh("i");

            var onlyDigits = G.Call(
                G.Call(G.Call(G.Op(Operator.ToText, v), "split", G.Lit("")), "filter", G.Fn(Matches(G.Ref(character), "^\\d$"), character)),
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
                G.Op(Operator.Gte, G.Get(G.Ref(digits), new[] { "length" }, Shape.None), G.Lit(13)),
                G.Eq(G.Op(Operator.Mod, total, G.Lit(10)), G.Lit(0)));
            return G.Apply(G.Fn(valid, digits), onlyDigits);
        }

        // An ordered rule compares the value and its target as numbers under the comparison shape;
        // a pair that cannot be compared fails. Every ordered rule but GreaterThan passes on nothing.
        private Expr Ordered(Expr v, Expr target, Shape comparison, Operator op, bool skipsEmpty)
        {
            var a = _context.Fresh("a");
            var b = _context.Fresh("b");
            var holds = G.And(IsNumber(G.Ref(a)), IsNumber(G.Ref(b)), G.Op(op, G.Ref(a), G.Ref(b)));
            var compared = G.Apply(G.Fn(holds, a, b), Comparable(v, comparison), Comparable(target, comparison));
            return skipsEmpty ? G.Or(NothingEntered(v), compared) : G.And(G.Not(NothingEntered(v)), compared);
        }

        private Expr Range(Expr v, ValidationRuleExecution execution, Shape comparison, Operator lower, Operator upper)
        {
            var bounds = Constraint(execution) as object[]
                ?? throw new InvalidOperationException("A range rule needs its two bounds.");
            var a = _context.Fresh("a");
            var low = _context.Fresh("l");
            var high = _context.Fresh("h");
            var inRange = G.And(
                IsNumber(G.Ref(a)), IsNumber(G.Ref(low)), IsNumber(G.Ref(high)),
                G.Op(lower, G.Ref(a), G.Ref(low)),
                G.Op(upper, G.Ref(a), G.Ref(high)));
            var compared = G.Apply(
                G.Fn(inRange, a, low, high),
                Comparable(v, comparison),
                Comparable(G.Lit(bounds[0]), comparison),
                Comparable(G.Lit(bounds[1]), comparison));
            return G.Or(NothingEntered(v), compared);
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

        // Equality under the comparison shape: both sides converted the way the shape converts a
        // missing value too; with no shape, both as text.
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
